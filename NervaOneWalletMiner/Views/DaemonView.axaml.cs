using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using NervaOneWalletMiner.Helpers;
using NervaOneWalletMiner.Objects.Constants;
using NervaOneWalletMiner.Rpc.Daemon.Requests;
using NervaOneWalletMiner.Rpc.Daemon.Responses;
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
                    StopMiningAsync();
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
                    StartMiningAsync(GlobalData.AppSettings.Daemon[GlobalData.AppSettings.ActiveCoin].MiningThreads);
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
                    MiningAddress = GlobalData.AppSettings.Daemon[GlobalData.AppSettings.ActiveCoin].MiningAddress,
                    ThreadCount = threads
                };

                Logger.LogDebug("HomV.StMA", "Calling StartMining. Address: " + GlobalMethods.GetShorterString(request.MiningAddress, 12) + " | Threads: " + request.ThreadCount);
                StartMiningResponse response = await GlobalData.DaemonService.StartMining(GlobalData.AppSettings.Daemon[GlobalData.AppSettings.ActiveCoin].Rpc, request);
                if(response.Error.IsError)
                {
                    await Dispatcher.UIThread.Invoke(async () =>
                    {
                        var box = MessageBoxManager.GetMessageBoxStandard("Start Mining", "Error when starting mining\r\n\r\n" + response.Error.Message, ButtonEnum.Ok);
                        _ = await box.ShowAsync();
                    });
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

                StopMiningResponse response = await GlobalData.DaemonService.StopMining(GlobalData.AppSettings.Daemon[GlobalData.AppSettings.ActiveCoin].Rpc, request);
                if (response.Error.IsError)
                {
                    await Dispatcher.UIThread.Invoke(async () =>
                    {
                        var box = MessageBoxManager.GetMessageBoxStandard("Stop Mining", "Error when stopping mining\r\n\r\n" + response.Error.Message, ButtonEnum.Ok);
                        _ = await box.ShowAsync();
                    });
                }
            }
            catch (Exception ex)
            {
                Logger.LogException("HomV.SpMA", ex);
            }
        }
    }
}