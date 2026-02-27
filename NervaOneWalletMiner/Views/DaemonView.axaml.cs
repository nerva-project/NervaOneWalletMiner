using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using NervaOneWalletMiner.Helpers;
using NervaOneWalletMiner.Objects;
using NervaOneWalletMiner.Objects.Constants;
using NervaOneWalletMiner.Rpc.Daemon.Requests;
using NervaOneWalletMiner.Rpc.Daemon.Responses;
using NervaOneWalletMiner.ViewModels;
using NervaOneWalletMiner.ViewsDialogs;
using System;

namespace NervaOneWalletMiner.Views
{
    public partial class DaemonView : UserControl
    {
        Window GetWindow() => TopLevel.GetTopLevel(this) as Window ?? throw new NullReferenceException("Invalid Owner");

        public DaemonView()
        {
            try
            {
                InitializeComponent();
                imgCoinIcon.Source = GlobalMethods.GetLogo();

                if (!GlobalData.CoinSettings[GlobalData.AppSettings.ActiveCoin].IsCpuMiningSupported)
                {
                    btnStartStopMining.IsEnabled = false;
                }

                Initialized += DaemonView_Initialized;

                var nupThreads = this.Get<NumericUpDown>("nupThreads");
                nupThreads.Maximum = GlobalData.CpuThreadCount;
                nupThreads.Value = GlobalData.AppSettings.Daemon[GlobalData.AppSettings.ActiveCoin].MiningThreads;
            }
            catch (Exception ex)
            {
                Logger.LogException("DMN.CONS", ex);
            }
        }

        private void DaemonView_Initialized(object? sender, EventArgs e)
        {
            try
            {
                if (!GlobalData.AreDaemonEventsRegistered)
                {
                    DaemonViewModel vm = (DaemonViewModel)DataContext!;
                    vm.StartMiningUiEvent += (owner, threads) => StartMiningAsync(owner, threads);
                    vm.StartMiningNonUiEvent += StartMiningNonUi;
                    vm.StopMiningNonUiEvent += StopMiningNonUi;
                    GlobalData.AreDaemonEventsRegistered = true;
                }
            }
            catch (Exception ex)
            {
                Logger.LogException("DMN.DAVI", ex); ;
            }
        }

        private void nupThreads_ValueChanged(object sender, NumericUpDownValueChangedEventArgs args)
        {
            if (GlobalData.AppSettings.Daemon[GlobalData.AppSettings.ActiveCoin].MiningThreads != nupThreads.Value)
            {
                GlobalData.AppSettings.Daemon[GlobalData.AppSettings.ActiveCoin].MiningThreads = Convert.ToInt32(nupThreads.Value);
                Logger.LogDebug("DMN.NTVC", "Setting mining threads: " + GlobalData.AppSettings.Daemon[GlobalData.AppSettings.ActiveCoin].MiningThreads);
                GlobalMethods.SaveConfig();
            }
        }

        public async void StartStopMining_Clicked(object sender, RoutedEventArgs args)
        {
            try
            {
                var btnStartStopMining = this.Get<Button>("btnStartStopMining");
                var nupThreads = this.Get<NumericUpDown>("nupThreads");

                if (btnStartStopMining.Content!.ToString()!.Equals(StatusMiner.StopMining))
                {
                    // Stop mining
                    Logger.LogDebug("DMN.SSMC", "Stopping mining");
                    GlobalData.IsManualStoppedMining = true;
                    StopMiningAsync(GetWindow());
                    btnStartStopMining.Content = StatusMiner.StartMining;
                    nupThreads.IsEnabled = true;
                }
                else
                {
                    if(string.IsNullOrEmpty(GlobalData.AppSettings.Daemon[GlobalData.AppSettings.ActiveCoin].MiningAddress))
                    {
                        // Get and save Mining Address
                        Logger.LogDebug("DMN.SSMC", "Mining address missing. Asking user to provide it");
                        var window = new TextBoxView("Start Mining", "Please provide mining address", string.Empty, "Required - Mining Address");
                        DialogResult dialogRes = await window.ShowDialog<DialogResult>(GetWindow());

                        if (dialogRes != null && dialogRes.IsOk)
                        {
                            if(!string.IsNullOrEmpty(dialogRes.TextBoxValue))
                            {                                
                                GlobalData.AppSettings.Daemon[GlobalData.AppSettings.ActiveCoin].MiningAddress = dialogRes.TextBoxValue;
                                Logger.LogDebug("DMN.SSMC", "Setting and saving mining address: " + GlobalMethods.GetShorterString(GlobalData.AppSettings.Daemon[GlobalData.AppSettings.ActiveCoin].MiningAddress, 12));
                                GlobalMethods.SaveConfig();                                
                            }
                        }
                    }

                    if (string.IsNullOrEmpty(GlobalData.AppSettings.Daemon[GlobalData.AppSettings.ActiveCoin].MiningAddress))
                    {
                        Logger.LogDebug("DMN.SSMC", "Mining address still missing. Aborting mining");
                        MessageBoxView window = new("Start Mining", "Cannot start mining without Mining Address", true);
                        await window.ShowDialog(GetWindow());
                    }
                    else
                    {
                        if (GlobalData.NetworkStats.NetHeight > GlobalData.NetworkStats.YourHeight)
                        {
                            Logger.LogDebug("DMN.SSMC", "Incorrect height. Aborting mining. NetHeight: " + GlobalData.NetworkStats.NetHeight + " | YourHeight: " + GlobalData.NetworkStats.YourHeight);
                            MessageBoxView window = new("Start Mining", "Cannot start mining because you're not fully synchronized.\r\nPlease try again later.", true);
                            await window.ShowDialog(GetWindow());
                        }
                        else
                        {
                            // Start mining
                            if (GlobalData.AppSettings.Daemon[GlobalData.AppSettings.ActiveCoin].MiningThreads != nupThreads.Value)
                            {
                                GlobalData.AppSettings.Daemon[GlobalData.AppSettings.ActiveCoin].MiningThreads = Convert.ToInt32(nupThreads.Value);
                                GlobalMethods.SaveConfig();
                            }

                            Logger.LogDebug("DMN.SSMC", "Starting mining");                            
                            StartMiningAsync(GetWindow(), GlobalData.AppSettings.Daemon[GlobalData.AppSettings.ActiveCoin].MiningThreads);
                            btnStartStopMining.Content = StatusMiner.StopMining;
                            nupThreads.IsEnabled = false;
                        }
                    }
                }                
            }
            catch (Exception ex)
            {
                Logger.LogException("DMN.SSMC", ex);
            }
        }
        
        public void StartMiningNonUi(int threads)
        {
            // Master Process does not have owner so do not attempt to show messages
            StartMiningAsync(null, threads, false);
        }
        public async void StartMiningAsync(Window? owner, int threads, bool isUiThread = true)
        {
            try
            {
                if(string.IsNullOrEmpty(GlobalData.AppSettings.Daemon[GlobalData.AppSettings.ActiveCoin].MiningAddress))
                {
                    Logger.LogError("DMN.STMA", "Mining address missing. Cannot start mining");
                }
                else
                {
                    StartMiningRequest request = new()
                    {
                        MiningAddress = GlobalData.AppSettings.Daemon[GlobalData.AppSettings.ActiveCoin].MiningAddress,
                        ThreadCount = threads
                    };

                    Logger.LogDebug("DMN.STMA", "Calling StartMining. Address: " + GlobalMethods.GetShorterString(request.MiningAddress, 12) + " | Threads: " + request.ThreadCount);
                    StartMiningResponse response = await GlobalData.DaemonService.StartMining(
                            GlobalData.AppSettings.Daemon[GlobalData.AppSettings.ActiveCoin].Rpc, request);
                    
                    if (response.Error.IsError)
                    {
                        Logger.LogDebug("DMN.STMA", "Error starting mining | Code: " + response.Error.Code + " | Message: " + response.Error.Message + " | Content: " + response.Error.Content);

                        Console.WriteLine(response.Error.Content);
                        Console.WriteLine(response.Error.Message);
                        if (isUiThread && !string.Equals(response.Error.Message, "Network hash too high"))
                        {
                            await Dispatcher.UIThread.Invoke(async () =>
                            {
                                MessageBoxView window = new("Start Mining", "Error when starting mining\r\n\r\n" + response.Error.Message, true);
                                await window.ShowDialog(owner!);
                            });
                        }
                    }
                    else
                    {                       
                        // Some errors are not reported as errors, such as not being able to mine to subaddress
                        Logger.LogDebug("DMN.STMA", "Start mining response status: " + response.Status);

                        // Without this, "Auto start mining" might try to start mining again before update happens
                        GlobalData.NetworkStats.MinerStatus = StatusMiner.Mining;

                        if (isUiThread)
                        {
                            await Dispatcher.UIThread.Invoke(async () =>
                            {
                                MessageBoxView window = new("Start Mining", "Response: " + response.Status, true);
                                await window.ShowDialog(owner!);
                            });
                        }
                    }

                    GlobalData.IsManualStoppedMining = false;
                }
            }
            catch (Exception ex)
            {
                Logger.LogException("DMN.STMA", ex);
            }
        }

        public void StopMiningNonUi()
        {
            // Master Process does not have owner so do not attempt to show messages
            StopMiningAsync(null, false);
        }
        public async void StopMiningAsync(Window? owner, bool isUiThread = true)
        {
            try
            {
                StopMiningRequest request = new();

                Logger.LogDebug("DMN.SPMA", "Calling StopMining");
                StopMiningResponse response =
                    await GlobalData.DaemonService.StopMining(
                        GlobalData.AppSettings.Daemon[GlobalData.AppSettings.ActiveCoin].Rpc, request);
                    
                if (response.Error.IsError)
                {
                    if (isUiThread)
                    {
                        await Dispatcher.UIThread.Invoke(async () =>
                        {
                            MessageBoxView window = new("Stop Mining", "Error when stopping mining\r\n\r\n" + response.Error.Message, true);
                            await window.ShowDialog(owner!);
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogException("DMN.SPMA", ex);
            }
        }
    }
}