using NervaOneWalletMiner.Helpers;
using NervaOneWalletMiner.Objects.Constants;
using ReactiveUI;
using System;
using System.Threading.Tasks;

namespace NervaOneWalletMiner.ViewModels
{
    internal class DaemonSetupViewModel : ViewModelBase
    {
        public record SaveSettingsResult(bool IsSaved, bool RestartRequired, bool LocalRemoteChanged);

        #region Reactive Properties

        private string _miningAddress = string.Empty;
        public string MiningAddress
        {
            get => _miningAddress;
            set => this.RaiseAndSetIfChanged(ref _miningAddress, value);
        }

        private string _dataDir = string.Empty;
        public string DataDir
        {
            get => _dataDir;
            set => this.RaiseAndSetIfChanged(ref _dataDir, value);
        }

        private string _additionalArguments = string.Empty;
        public string AdditionalArguments
        {
            get => _additionalArguments;
            set => this.RaiseAndSetIfChanged(ref _additionalArguments, value);
        }

        private string _portNumber = string.Empty;
        public string PortNumber
        {
            get => _portNumber;
            set => this.RaiseAndSetIfChanged(ref _portNumber, value);
        }

        private string _logLevel = string.Empty;
        public string LogLevel
        {
            get => _logLevel;
            set => this.RaiseAndSetIfChanged(ref _logLevel, value);
        }

        private bool _autoStartMining;
        public bool AutoStartMining
        {
            get => _autoStartMining;
            set => this.RaiseAndSetIfChanged(ref _autoStartMining, value);
        }

        private bool _stopOnExit;
        public bool StopOnExit
        {
            get => _stopOnExit;
            set => this.RaiseAndSetIfChanged(ref _stopOnExit, value);
        }

        private bool _enableConnectionsGuard;
        public bool EnableConnectionsGuard
        {
            get => _enableConnectionsGuard;
            set => this.RaiseAndSetIfChanged(ref _enableConnectionsGuard, value);
        }

        private bool _useNoAnalyticsFlag;
        public bool UseNoAnalyticsFlag
        {
            get => _useNoAnalyticsFlag;
            set => this.RaiseAndSetIfChanged(ref _useNoAnalyticsFlag, value);
        }

        private bool _useNoDnsFlag;
        public bool UseNoDnsFlag
        {
            get => _useNoDnsFlag;
            set => this.RaiseAndSetIfChanged(ref _useNoDnsFlag, value);
        }

        private bool _thresholdEnabled;
        public bool ThresholdEnabled
        {
            get => _thresholdEnabled;
            set => this.RaiseAndSetIfChanged(ref _thresholdEnabled, value);
        }

        private decimal _hashThreshold;
        public decimal HashThreshold
        {
            get => _hashThreshold;
            set => this.RaiseAndSetIfChanged(ref _hashThreshold, value);
        }

        private bool _isWalletOnly;
        public bool IsWalletOnly
        {
            get => _isWalletOnly;
            set => this.RaiseAndSetIfChanged(ref _isWalletOnly, value);
        }

        private string _remoteNodeAddress = string.Empty;
        public string RemoteNodeAddress
        {
            get => _remoteNodeAddress;
            set => this.RaiseAndSetIfChanged(ref _remoteNodeAddress, value);
        }

        #endregion // Reactive Properties

        public DaemonSetupViewModel()
        {
            try
            {
                var daemonSettings = GlobalData.AppSettings.Daemon[GlobalData.AppSettings.ActiveCoin];
                var walletSettings = GlobalData.AppSettings.Wallet[GlobalData.AppSettings.ActiveCoin];

                MiningAddress = daemonSettings.MiningAddress;
                DataDir = daemonSettings.DataDir;
                AdditionalArguments = daemonSettings.AdditionalArguments;
                PortNumber = daemonSettings.Rpc.Port.ToString();
                LogLevel = daemonSettings.LogLevel.ToString();
                AutoStartMining = daemonSettings.AutoStartMining;
                StopOnExit = daemonSettings.StopOnExit;
                EnableConnectionsGuard = daemonSettings.EnableConnectionsGuard;
                UseNoAnalyticsFlag = daemonSettings.UseNoAnalyticsFlag;
                UseNoDnsFlag = daemonSettings.UseNoDnsFlag;
                ThresholdEnabled = daemonSettings.EnableMiningThreshold;
                HashThreshold = daemonSettings.StopMiningThreshold;
                IsWalletOnly = daemonSettings.IsWalletOnly;
                RemoteNodeAddress = string.IsNullOrEmpty(walletSettings.PublicNodeAddress)
                    ? GlobalData.CoinSettings[GlobalData.AppSettings.ActiveCoin].RemotePublicNodeUrlDefault
                    : walletSettings.PublicNodeAddress;
            }
            catch (Exception ex)
            {
                Logger.LogException("DSM.CONS", ex);
            }
        }

        public SaveSettingsResult ApplySettings()
        {
            try
            {
                bool isSaveSettingsNeeded = false;
                bool isRestartRequired = false;
                bool isLocalRemoteNodeChange = false;

                var daemonSettings = GlobalData.AppSettings.Daemon[GlobalData.AppSettings.ActiveCoin];
                var walletSettings = GlobalData.AppSettings.Wallet[GlobalData.AppSettings.ActiveCoin];

                if (IsWalletOnly)
                {
                    if (!daemonSettings.IsWalletOnly)
                    {
                        daemonSettings.IsWalletOnly = true;
                        Logger.LogDebug("DSM.APST", "Switching to Wallet Only");
                        isSaveSettingsNeeded = isLocalRemoteNodeChange = true;
                    }

                    if (walletSettings.PublicNodeAddress != RemoteNodeAddress.Trim())
                    {
                        walletSettings.PublicNodeAddress = RemoteNodeAddress.Trim();
                        Logger.LogDebug("DSM.APST", "New Public Node: " + walletSettings.PublicNodeAddress);
                        isSaveSettingsNeeded = isLocalRemoteNodeChange = true;
                    }
                }
                else
                {
                    if (daemonSettings.IsWalletOnly)
                    {
                        daemonSettings.IsWalletOnly = false;
                        Logger.LogDebug("DSM.APST", "Switching to Full Node");
                        isSaveSettingsNeeded = isLocalRemoteNodeChange = true;
                    }
                }

                if (daemonSettings.MiningAddress != MiningAddress)
                {
                    daemonSettings.MiningAddress = MiningAddress;
                    Logger.LogDebug("DSM.APST", "New Mining Address: " + GlobalMethods.GetShorterString(daemonSettings.MiningAddress, 12));
                    isSaveSettingsNeeded = isRestartRequired = true;
                }

                if (!string.IsNullOrEmpty(DataDir) && daemonSettings.DataDir != DataDir)
                {
                    daemonSettings.DataDir = DataDir;
                    Logger.LogDebug("DSM.APST", "New Data Directory: " + daemonSettings.DataDir);
                    isSaveSettingsNeeded = isRestartRequired = true;
                }

                if (daemonSettings.AdditionalArguments != AdditionalArguments)
                {
                    daemonSettings.AdditionalArguments = AdditionalArguments;
                    Logger.LogDebug("DSM.APST", "New Additional Arguments: " + daemonSettings.AdditionalArguments);
                    isSaveSettingsNeeded = isRestartRequired = true;
                }

                if (!string.IsNullOrEmpty(PortNumber) && int.TryParse(PortNumber, out int portNumber) && daemonSettings.Rpc.Port != portNumber)
                {
                    daemonSettings.Rpc.Port = portNumber;
                    Logger.LogDebug("DSM.APST", "New RCP Port: " + daemonSettings.Rpc.Port);
                    isSaveSettingsNeeded = isRestartRequired = true;
                }

                if (!string.IsNullOrEmpty(LogLevel) && int.TryParse(LogLevel, out int logLevel) && daemonSettings.LogLevel != logLevel)
                {
                    daemonSettings.LogLevel = logLevel;
                    Logger.LogDebug("DSM.APST", "New Log Level: " + daemonSettings.LogLevel);
                    isSaveSettingsNeeded = isRestartRequired = true;
                }

                if (daemonSettings.AutoStartMining != AutoStartMining)
                {
                    daemonSettings.AutoStartMining = AutoStartMining;
                    Logger.LogDebug("DSM.APST", "New Auto Start Mining: " + daemonSettings.AutoStartMining);
                    isSaveSettingsNeeded = true;
                }

                if (daemonSettings.StopOnExit != StopOnExit)
                {
                    daemonSettings.StopOnExit = StopOnExit;
                    Logger.LogDebug("DSM.APST", "New Stop on Exit: " + daemonSettings.StopOnExit);
                    isSaveSettingsNeeded = true;
                }

                if (daemonSettings.EnableConnectionsGuard != EnableConnectionsGuard)
                {
                    daemonSettings.EnableConnectionsGuard = EnableConnectionsGuard;
                    Logger.LogDebug("DSM.APST", "New Enable Connections Guard: " + daemonSettings.EnableConnectionsGuard);
                    isSaveSettingsNeeded = true;
                }

                if (daemonSettings.UseNoAnalyticsFlag != UseNoAnalyticsFlag)
                {
                    daemonSettings.UseNoAnalyticsFlag = UseNoAnalyticsFlag;
                    Logger.LogDebug("DSM.APST", "New No Analytics Flag: " + daemonSettings.UseNoAnalyticsFlag);
                    isSaveSettingsNeeded = isRestartRequired = true;
                }

                if (daemonSettings.UseNoDnsFlag != UseNoDnsFlag)
                {
                    daemonSettings.UseNoDnsFlag = UseNoDnsFlag;
                    Logger.LogDebug("DSM.APST", "New No DNS Flag: " + daemonSettings.UseNoDnsFlag);
                    isSaveSettingsNeeded = isRestartRequired = true;
                }

                if (daemonSettings.EnableMiningThreshold != ThresholdEnabled)
                {
                    daemonSettings.EnableMiningThreshold = ThresholdEnabled;
                    Logger.LogDebug("DSM.APST", "New Enable Mining Threshold: " + daemonSettings.EnableMiningThreshold);
                    isSaveSettingsNeeded = true;
                }

                int hashThreshold = (int)HashThreshold;
                if (daemonSettings.StopMiningThreshold != hashThreshold)
                {
                    daemonSettings.StopMiningThreshold = hashThreshold;
                    Logger.LogDebug("DSM.APST", "New Stop Mining Threshold: " + daemonSettings.StopMiningThreshold);
                    isSaveSettingsNeeded = true;
                }

                if (isSaveSettingsNeeded)
                {
                    GlobalMethods.SaveConfig();
                    Logger.LogDebug("DSM.APST", "Settings saved");
                }

                return new SaveSettingsResult(isSaveSettingsNeeded, isRestartRequired, isLocalRemoteNodeChange);
            }
            catch (Exception ex)
            {
                Logger.LogException("DSM.APST", ex);
                return new SaveSettingsResult(false, false, false);
            }
        }

        public void RestartWithCommand(string restartOptions)
        {
            try
            {
                Logger.LogDebug("DSM.RWCM", "Restarting with command");
                ProcessManager.Kill(GlobalData.WalletProcessName);
                GlobalMethods.StopAndCloseDaemon();

                if (GlobalData.AppSettings.Daemon[GlobalData.AppSettings.ActiveCoin].IsWalletOnly)
                {
                    Logger.LogDebug("DSM.RWCM", "Running as Wallet Only");
                }
                else
                {
                    Logger.LogDebug("DSM.RWCM", "Running as Full Node");
                    GlobalData.IsDaemonRestarting = true;
                    ProcessManager.StartExternalProcess(
                        GlobalMethods.GetDaemonProcess(),
                        GlobalData.CoinSettings[GlobalData.AppSettings.ActiveCoin].GenerateDaemonOptions(GlobalData.AppSettings.Daemon[GlobalData.AppSettings.ActiveCoin]) + " " + restartOptions);
                }
            }
            catch (Exception ex)
            {
                Logger.LogException("DSM.RWCM", ex);
            }
        }

        public bool IsQuickSyncSupported()
        {
            try
            {
                return !string.IsNullOrEmpty(GlobalData.CoinSettings[GlobalData.AppSettings.ActiveCoin].QuickSyncUrl);
            }
            catch (Exception ex)
            {
                Logger.LogException("DSM.IQSS", ex);
                return false;
            }
        }

        public string GetCliToolsDownloadLink()
        {
            try
            {
                return GlobalMethods.GetCliToolsDownloadLink(GlobalData.AppSettings.ActiveCoin);
            }
            catch (Exception ex)
            {
                Logger.LogException("DSM.GCTL", ex);
                return string.Empty;
            }
        }

        public async Task PerformCliToolsUpdate(string downloadLink)
        {
            try
            {
                // We'll be downloading new client tools so clean up
                GlobalData.IsCliToolsDownloading = true;

                if (GlobalData.IsWalletOpen)
                {
                    Logger.LogDebug("DSM.PCTU", "Closing wallet: " + GlobalData.OpenedWalletName);
                    await ((WalletViewModel)GlobalData.ViewModelPages[SplitViewPages.Wallet]).CloseWalletNonUi();
                }

                if (GlobalData.CoinSettings[GlobalData.AppSettings.ActiveCoin].IsDaemonWalletSeparateApp)
                {
                    Logger.LogDebug("DSM.PCTU", "Calling wallet ForceClose...");
                    ProcessManager.Kill(GlobalData.WalletProcessName);
                }

                Logger.LogDebug("DSM.PCTU", "Stopping daemon...");
                GlobalMethods.StopAndCloseDaemon();

                GlobalMethods.SetUpCliTools(downloadLink, GlobalData.CliToolsDir);
            }
            catch (Exception ex)
            {
                Logger.LogException("DSM.PCTU", ex);
            }
        }
    }
}