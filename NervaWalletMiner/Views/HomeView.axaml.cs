using Avalonia.Controls;
using Avalonia.Interactivity;
using NervaWalletMiner.Helpers;
using NervaWalletMiner.Objects.Constants;
using NervaWalletMiner.Rpc.Daemon.Requests;
using NervaWalletMiner.Rpc.Daemon.Responses;
using System;

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
                Logger.LogException("HomV.SSMC", ex);
            }
        }

        public static async void StartMiningAsync(int threads)
        {
            try
            {                
                StartMiningRequest request = new()
                {
                    MiningAddress = GlobalData.ApplicationSettings.Daemon.MiningAddress,
                    ThreadCount = threads
                };

                StartMiningResponse response = await GlobalData.DaemonService.StartMining(GlobalData.ApplicationSettings.Daemon.Rpc, request);
                if(response.Error.IsError)
                {
                    // TODO: Error so alert user
                }
            }
            catch (Exception ex)
            {
                Logger.LogException("HomV.SMA", ex);
            }
        }

        public static async void StopMiningAsync()
        {
            try
            {
                StopMiningRequest request = new();

                StopMiningResponse response = await GlobalData.DaemonService.StopMining(GlobalData.ApplicationSettings.Daemon.Rpc, request);
                if (response.Error.IsError)
                {
                    // TODO: Error so alert user
                }
            }
            catch (Exception ex)
            {
                Logger.LogException("HomV.SMA", ex);
            }
        }
    }
}