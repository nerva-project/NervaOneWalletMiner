using Avalonia.Controls;
using Avalonia.Interactivity;
using NervaOneWalletMiner.Helpers;
using NervaOneWalletMiner.Objects.Constants;
using System;

namespace NervaOneWalletMiner.Views
{
    public partial class DaemonView : UserControl
    {
        public DaemonView()
        {
            InitializeComponent();

            var nupThreads = this.Get<NumericUpDown>("nupThreads");
            nupThreads.Maximum = GlobalData.CpuThreadCount;
            nupThreads.Value = GlobalData.AppSettings.Daemon[GlobalData.AppSettings.ActiveCoin].MiningThreads;
        }

        public void StartStopMiningClicked(object sender, RoutedEventArgs args)
        {
            try
            {
                var btnStartStopMining = this.Get<Button>("btnStartStopMining");
                var nupThreads = this.Get<NumericUpDown>("nupThreads");

                if (btnStartStopMining.Content!.ToString()!.Equals(StatusMiner.StopMining))
                {
                    // Stop mining
                    Logger.LogDebug("HomV.SSMC", "Stopping mining");
                    GlobalData.IsManualStopMining = true;
                    GlobalMethods.StopMiningAsync();
                    btnStartStopMining.Content = StatusMiner.StartMining;
                    nupThreads.IsEnabled = true;
                }
                else
                {
                    // Start mining
                    if(GlobalData.AppSettings.Daemon[GlobalData.AppSettings.ActiveCoin].MiningThreads != nupThreads.Value)
                    {
                        GlobalData.AppSettings.Daemon[GlobalData.AppSettings.ActiveCoin].MiningThreads = Convert.ToInt32(nupThreads.Value);
                        GlobalMethods.SaveConfig();
                    }

                    Logger.LogDebug("HomV.SSMC", "Starting mining");
                    GlobalData.IsManualStopMining = false;
                    GlobalMethods.StartMiningAsync(GlobalData.AppSettings.Daemon[GlobalData.AppSettings.ActiveCoin].MiningThreads);
                    btnStartStopMining.Content = StatusMiner.StopMining;
                    nupThreads.IsEnabled = false;
                }                
            }
            catch (Exception ex)
            {
                Logger.LogException("HomV.SSMC", ex);
            }
        }
    }
}