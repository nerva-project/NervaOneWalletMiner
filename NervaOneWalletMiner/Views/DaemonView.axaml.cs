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
using System.Drawing.Printing;
using System.Threading;
using System.Threading.Tasks;

namespace NervaOneWalletMiner.Views
{
    public partial class DaemonView : UserControl
    {
        Window GetWindow() => TopLevel.GetTopLevel(this) as Window ?? throw new NullReferenceException("Invalid Owner");

        private CancellationTokenSource? _monitoringCts;

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
                    GlobalData.IsManualStopMining = true;
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
                        ThreadCount = threads,
                        EnableMiningThreshold = GlobalData.AppSettings.Daemon[GlobalData.AppSettings.ActiveCoin].EnableMiningThreshold,
                        StopMiningThreshold = GlobalData.AppSettings.Daemon[GlobalData.AppSettings.ActiveCoin].StopMiningThreshold
                    };

                    Logger.LogDebug("DMN.STMA", "Calling StartMining. Address: " + GlobalMethods.GetShorterString(request.MiningAddress, 12) + " | Threads: " + request.ThreadCount + " | Enable Mining Threshold: " + request.EnableMiningThreshold + " | Threshold: " + request.StopMiningThreshold);
                    
                    if (request.EnableMiningThreshold)
                    {
                        StartHashRateMonitoring(threads);  // Always start monitoring
                    }
                    
                    StartMiningResponse response = request.EnableMiningThreshold
                        ? await GlobalData.DaemonService.StartMiningAuto(
                            GlobalData.AppSettings.Daemon[GlobalData.AppSettings.ActiveCoin].Rpc, request)
                        : await GlobalData.DaemonService.StartMining(
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
                        
                        // Change the button to "Stop mining"
                        Dispatcher.UIThread.Post(() =>
                        {
                            if (DataContext is DaemonViewModel vm)
                            {
                                vm.StartStopMining = StatusMiner.StopMining;
                                vm.IsNumThreadsEnabled = false;
                            }
                        });

                        if (isUiThread)
                        {
                            await Dispatcher.UIThread.Invoke(async () =>
                            {
                                MessageBoxView window = new("Start Mining", "Response: " + response.Status, true);
                                await window.ShowDialog(owner!);
                            });
                        }
                    }

                    GlobalData.IsManualStopMining = false;
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
                // Stop the monitoring loop first so it doesn't restart mining
                StopHashRateMonitoring();

                StopMiningRequest request = new()
                {
                    EnableMiningThreshold = GlobalData.AppSettings.Daemon[GlobalData.AppSettings.ActiveCoin].EnableMiningThreshold,
                    StopMiningThreshold = GlobalData.AppSettings.Daemon[GlobalData.AppSettings.ActiveCoin].StopMiningThreshold
                };

                Logger.LogDebug("DMN.SPMA", "Calling StopMining");
                // Always call StopMining directly - user explicitly wants to stop
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
                // Change the button to "Start mining"
                if (!request.EnableMiningThreshold)
                {
                    Dispatcher.UIThread.Post(() =>
                    {
                        if (DataContext is DaemonViewModel vm)
                        {
                            vm.StartStopMining = StatusMiner.StartMining;
                            vm.IsNumThreadsEnabled = true;
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                Logger.LogException("DMN.SPMA", ex);
            }
        }

        #region Hash Rate Monitoring
        private void StartHashRateMonitoring(int threads)
        {
            StopHashRateMonitoring();
            _monitoringCts = new CancellationTokenSource();
            var token = _monitoringCts.Token;

            _ = Task.Run(async () =>
            {
                Logger.LogDebug("DMN.HMON", "Hash rate monitoring started");

                while (!token.IsCancellationRequested)
                {
                    try
                    {
                        await Task.Delay(60_000, token);
                    }
                    catch (OperationCanceledException)
                    {
                        break;
                    }

                    try
                    {
                        if (GlobalData.IsManualStopMining)
                        {
                            Logger.LogDebug("DMN.HMON", "Manual stop detected, ending monitoring");
                            break;
                        }

                        var rpc = GlobalData.AppSettings.Daemon[GlobalData.AppSettings.ActiveCoin].Rpc;
                        var enableThreshold = GlobalData.AppSettings.Daemon[GlobalData.AppSettings.ActiveCoin].EnableMiningThreshold;
                        var threshold = GlobalData.AppSettings.Daemon[GlobalData.AppSettings.ActiveCoin].StopMiningThreshold;

                        if (!enableThreshold)
                        {
                            Logger.LogDebug("DMN.HMON", "Threshold disabled, ending monitoring");
                            break;
                        }

                        var infoRes = await GlobalData.DaemonService.GetInfo(rpc, new GetInfoRequest());
                        if (infoRes.Error.IsError)
                        {
                            Logger.LogDebug("DMN.HMON", "Failed to get network info, skipping check");
                            continue;
                        }

                        var hashRateKH = infoRes.NetworkHashRate / 1000.0d;

                        var statusRes = await GlobalData.DaemonService.GetMiningStatus(rpc, new MiningStatusRequest());
                        if (statusRes.Error.IsError)
                        {
                            Logger.LogDebug("DMN.HMON", "Failed to get mining status, skipping check");
                            continue;
                        }

                        if (statusRes.IsActive && hashRateKH > threshold)
                        {
                            // Hash rate rose above threshold - pause mining
                            Logger.LogDebug("DMN.HMON", "Hash rate " + hashRateKH.ToString("F1") + " KH/s above threshold " + threshold + ", pausing mining");
                            await GlobalData.DaemonService.StopMining(rpc, new StopMiningRequest());
                        }
                        else if (!statusRes.IsActive && hashRateKH <= threshold)
                        {
                            // Hash rate dropped to or below threshold - resume mining
                            Logger.LogDebug("DMN.HMON", "Hash rate " + hashRateKH.ToString("F1") + " KH/s at or below threshold " + threshold + ", resuming mining");
                            var startRequest = new StartMiningRequest
                            {
                                MiningAddress = GlobalData.AppSettings.Daemon[GlobalData.AppSettings.ActiveCoin].MiningAddress,
                                ThreadCount = threads,
                            };
                            await GlobalData.DaemonService.StartMining(rpc, startRequest);
                            GlobalData.NetworkStats.MinerStatus = StatusMiner.Mining;
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.LogException("DMN.HMON", ex);
                    }
                }

                Logger.LogDebug("DMN.HMON", "Hash rate monitoring stopped");
            }, token);
        }

        private void StopHashRateMonitoring()
        {
            try
            {
                _monitoringCts?.Cancel();
                _monitoringCts?.Dispose();
                _monitoringCts = null;
            }
            catch (Exception ex)
            {
                Logger.LogException("DMN.SHRM", ex);
            }
        }
        #endregion // Hash Rate Monitoring
    }
}