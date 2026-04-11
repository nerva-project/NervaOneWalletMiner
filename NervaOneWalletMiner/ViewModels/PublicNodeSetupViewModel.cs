using NervaOneWalletMiner.Helpers;
using ReactiveUI;
using System;

namespace NervaOneWalletMiner.ViewModels
{
    internal class PublicNodeSetupViewModel : ViewModelBase
    {
        #region Reactive Properties

        private bool _isPublicNode;
        public bool IsPublicNode
        {
            get => _isPublicNode;
            set => this.RaiseAndSetIfChanged(ref _isPublicNode, value);
        }

        private string _publicNodeArguments = string.Empty;
        public string PublicNodeArguments
        {
            get => _publicNodeArguments;
            set => this.RaiseAndSetIfChanged(ref _publicNodeArguments, value);
        }

        private string _publicIp = string.Empty;
        public string PublicIp
        {
            get => _publicIp;
            set
            {
                this.RaiseAndSetIfChanged(ref _publicIp, value);
                this.RaisePropertyChanged(nameof(NodeAddress));
            }
        }

        public string NodeAddress
        {
            get
            {
                if (string.IsNullOrEmpty(PublicIp)) { return string.Empty; }
                int port = GlobalData.AppSettings.Daemon[GlobalData.AppSettings.ActiveCoin].Rpc.Port;
                return PublicIp + ":" + port;
            }
        }

        #endregion // Reactive Properties

        public PublicNodeSetupViewModel()
        {
            try
            {
                var daemonSettings = GlobalData.AppSettings.Daemon[GlobalData.AppSettings.ActiveCoin];

                IsPublicNode = daemonSettings.IsPublicNode;
                PublicNodeArguments = string.IsNullOrEmpty(daemonSettings.PublicNodeArguments)
                    ? GlobalData.CoinSettings[GlobalData.AppSettings.ActiveCoin].LocalPublicNodeArgumentsDefault
                    : daemonSettings.PublicNodeArguments;

                PublicIp = daemonSettings.PublicNodeIp;
            }
            catch (Exception ex)
            {
                Logger.LogException("PNM.CONS", ex);
            }
        }

        public bool ApplySettings()
        {
            try
            {
                bool isSaveNeeded = false;
                var daemonSettings = GlobalData.AppSettings.Daemon[GlobalData.AppSettings.ActiveCoin];

                if (daemonSettings.IsPublicNode != IsPublicNode)
                {
                    daemonSettings.IsPublicNode = IsPublicNode;
                    Logger.LogDebug("PNM.APST", "IsPublicNode changed to: " + IsPublicNode);
                    isSaveNeeded = true;
                }

                if (daemonSettings.PublicNodeArguments != PublicNodeArguments)
                {
                    daemonSettings.PublicNodeArguments = PublicNodeArguments;
                    Logger.LogDebug("PNM.APST", "PublicNodeArguments changed to: " + PublicNodeArguments);
                    isSaveNeeded = true;
                }

                if (daemonSettings.PublicNodeIp != PublicIp)
                {
                    daemonSettings.PublicNodeIp = PublicIp;
                    Logger.LogDebug("PNM.APST", "PublicNodeIp changed to: " + PublicIp);
                    isSaveNeeded = true;
                }

                if (isSaveNeeded)
                {
                    GlobalMethods.SaveConfig();
                    Logger.LogDebug("PNM.APST", "Settings saved");
                }

                return isSaveNeeded;
            }
            catch (Exception ex)
            {
                Logger.LogException("PNM.APST", ex);
                return false;
            }
        }
    }
}
