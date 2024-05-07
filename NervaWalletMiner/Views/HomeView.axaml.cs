using Avalonia.Controls;
using Avalonia.Interactivity;
using NervaWalletMiner.Helpers;
using NervaWalletMiner.Objects.Constants;
using NervaWalletMiner.Rpc.Daemon;
using System;
using static NervaWalletMiner.Rpc.Daemon.StartMining;

namespace NervaWalletMiner.Views
{
    public partial class HomeView : UserControl
    {
        public HomeView()
        {
            InitializeComponent();

            var nupThreads = this.Get<NumericUpDown>("nupThreads");
            nupThreads.Maximum = GlobalData.CpuThreadCount;
            nupThreads.Value = GlobalData.ApplicationSettings.Daemon.MiningThreads;
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
                    StopMiningAsync();
                    btnStartStopMining.Content = StatusMiner.StartMining;
                    nupThreads.IsEnabled = true;
                }
                else
                {
                    // Start mining
                    if(GlobalData.ApplicationSettings.Daemon.MiningThreads != nupThreads.Value)
                    {
                        GlobalData.ApplicationSettings.Daemon.MiningThreads = Convert.ToInt32(nupThreads.Value);
                        GlobalMethods.SaveConfig();
                    }

                    StartMiningAsync(GlobalData.ApplicationSettings.Daemon.MiningThreads);
                    btnStartStopMining.Content = StatusMiner.StopMining;
                    nupThreads.IsEnabled = false;
                }                
            }
            catch (Exception ex)
            {
                Logger.LogException("Hom.SSMC", ex);
            }
        }

        public static async void StartMiningAsync(int threads)
        {
            // TODO: Check response and alert user in case of issues
            MiningResponse response = await StartMining.CallServiceAsync(GlobalData.ApplicationSettings.Daemon.MiningAddress, threads);
        }

        public static async void StopMiningAsync()
        {
            // TODO: Check response and alert user in case of issues
            MiningResponse response = await StopMining.CallServiceAsync();
        }
    }
}