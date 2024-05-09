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
            nupThreads.Value = GlobalData.ApplicationSettings.Daemon[GlobalData.ApplicationSettings.ActiveCoin].MiningThreads;
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
                    if(GlobalData.ApplicationSettings.Daemon[GlobalData.ApplicationSettings.ActiveCoin].MiningThreads != nupThreads.Value)
                    {
                        GlobalData.ApplicationSettings.Daemon[GlobalData.ApplicationSettings.ActiveCoin].MiningThreads = Convert.ToInt32(nupThreads.Value);
                        GlobalMethods.SaveConfig();
                    }

                    StartMiningAsync(GlobalData.ApplicationSettings.Daemon[GlobalData.ApplicationSettings.ActiveCoin].MiningThreads);
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
                    MiningAddress = GlobalData.ApplicationSettings.Daemon[GlobalData.ApplicationSettings.ActiveCoin].MiningAddress,
                    ThreadCount = threads
                };

                StartMiningResponse response = await GlobalData.DaemonService.StartMining(GlobalData.ApplicationSettings.Daemon[GlobalData.ApplicationSettings.ActiveCoin].Rpc, request);
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

                StopMiningResponse response = await GlobalData.DaemonService.StopMining(GlobalData.ApplicationSettings.Daemon[GlobalData.ApplicationSettings.ActiveCoin].Rpc, request);
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