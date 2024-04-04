using Avalonia.Controls;
using Avalonia.Interactivity;
using NervaWalletMiner.Helpers;
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
            nupThreads.Value = GlobalData.ApplicationSettings.MiningThreads;
        }

        public void StartStopMiningClicked(object sender, RoutedEventArgs args)
        {
            try
            {
                var btnStartStopMining = this.Get<Button>("btnStartStopMining");
                var nupThreads = this.Get<NumericUpDown>("nupThreads");

                if (btnStartStopMining.Content!.ToString()!.ToLower().Equals("stop mining"))
                {
                    // Stop mining
                    StopMiningAsync();

                    // TODO: Do below based on MiningStatus!
                    btnStartStopMining.Content = "Start Mining";
                    nupThreads.IsEnabled = true;
                }
                else
                {
                    // Start mining
                    if(GlobalData.ApplicationSettings.MiningThreads != nupThreads.Value)
                    {
                        GlobalData.ApplicationSettings.MiningThreads = Convert.ToInt32(nupThreads.Value);
                        GlobalMethods.SaveConfig();
                    }

                    StartMiningAsync(GlobalData.ApplicationSettings.MiningThreads);

                    // TODO: Do below based on MiningStatus!
                    btnStartStopMining.Content = "Stop Mining";
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
            MiningResponse response = await StartMining.CallServiceAsync(GlobalData.ApplicationSettings.MiningAddress, threads);
        }

        public static async void StopMiningAsync()
        {
            // TODO: Check response and alert user in case of issues
            MiningResponse response = await StopMining.CallServiceAsync();
        }
    }
}