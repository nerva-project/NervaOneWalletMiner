using Avalonia;
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
        private DataGridTextColumn? _colHeight;
        private DataGridTextColumn? _colState;
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

                // Index 2 = Height, 4 = State (icon=0, Address=1, Height=2, LiveTime=3, State=4)
                var dgConnections = this.Get<DataGrid>("dgConnections");
                _colHeight = (DataGridTextColumn)dgConnections.Columns[2];
                _colState = (DataGridTextColumn)dgConnections.Columns[4];

                // Prevent row selection from triggering RequestBringIntoView
                dgConnections.AddHandler(
                    RequestBringIntoViewEvent,
                    (object? sender, RequestBringIntoViewEventArgs e) => { e.Handled = true; },
                    RoutingStrategies.Bubble);

                for (int i = 1; i <= GlobalData.CpuThreadCount; i++)
                {
                    cbxThreads.Items.Add(i);
                }
                cbxThreads.SelectedItem = GlobalData.AppSettings.Daemon[GlobalData.AppSettings.ActiveCoin].MiningThreads;
            }
            catch (Exception ex)
            {
                Logger.LogException("DMN.CONS", ex);
            }
        }

        private void DaemonView_SizeChanged(object? sender, SizeChangedEventArgs e)
        {
            try
            {
                if (e.NewSize.Width < 450)
                {
                    // Narrow: threads+button below icon/label
                    grdHeader.ColumnDefinitions = ColumnDefinitions.Parse("Auto,*");
                    Grid.SetRow(spThreadsAndButton, 1);
                    Grid.SetColumn(spThreadsAndButton, 0);
                    spThreadsAndButton.Margin = new Thickness(0, 10, 5, 0);

                    // Narrow: miner stats below daemon stats
                    grdStats.ColumnDefinitions = ColumnDefinitions.Parse("200,Auto");
                    Grid.SetColumn(grdMinerStats, 0);
                    Grid.SetRow(grdMinerStats, 1);

                    // Narrow: icon + Address + LiveTime
                    if (_colHeight != null) { _colHeight.IsVisible = false; }
                    if (_colState != null) { _colState.IsVisible = false; }
                }
                else if (e.NewSize.Width < 700)
                {
                    // Medium: threads+button inline
                    grdHeader.ColumnDefinitions = ColumnDefinitions.Parse("Auto,*,Auto");
                    Grid.SetRow(spThreadsAndButton, 0);
                    Grid.SetColumn(spThreadsAndButton, 2);
                    spThreadsAndButton.Margin = new Thickness(0, 0, 5, 0);

                    // Medium: miner stats on the right
                    grdStats.ColumnDefinitions = ColumnDefinitions.Parse("200,*,200");
                    Grid.SetColumn(grdMinerStats, 2);
                    Grid.SetRow(grdMinerStats, 0);

                    // Medium: icon + Address + Height + LiveTime
                    if (_colHeight != null) { _colHeight.IsVisible = true; }
                    if (_colState != null) { _colState.IsVisible = false; }
                }
                else
                {
                    // Wide: threads+button on the right of icon/label
                    grdHeader.ColumnDefinitions = ColumnDefinitions.Parse("Auto,*,Auto");
                    Grid.SetRow(spThreadsAndButton, 0);
                    Grid.SetColumn(spThreadsAndButton, 2);
                    spThreadsAndButton.Margin = new Thickness(0, 0, 5, 0);

                    // Wide: miner stats on the right
                    grdStats.ColumnDefinitions = ColumnDefinitions.Parse("200,*,200");
                    Grid.SetColumn(grdMinerStats, 2);
                    Grid.SetRow(grdMinerStats, 0);

                    // Wide: all columns
                    if (_colHeight != null) { _colHeight.IsVisible = true; }
                    if (_colState != null) { _colState.IsVisible = true; }
                }
            }
            catch (Exception ex)
            {
                Logger.LogException("DMN.DVSC", ex);
            }
        }

        private void DaemonView_Initialized(object? sender, EventArgs e)
        {
            try
            {
                if (!GlobalData.AreDaemonEventsRegistered)
                {
                    DaemonViewModel vm = (DaemonViewModel)DataContext!;
                    vm.StartMiningUiEvent += threads => StartMiningAsync(threads);
                    vm.StartMiningNonUiEvent += StartMiningNonUi;
                    vm.StopMiningNonUiEvent += StopMiningNonUi;
                    GlobalData.AreDaemonEventsRegistered = true;
                }
            }
            catch (Exception ex)
            {
                Logger.LogException("DMN.DAVI", ex);
            }
        }

        private void cbxThreads_SelectionChanged(object sender, SelectionChangedEventArgs args)
        {
            try
            {
                if (cbxThreads.SelectedItem is int selected &&  GlobalData.AppSettings.Daemon[GlobalData.AppSettings.ActiveCoin].MiningThreads != selected)
                {
                    GlobalData.AppSettings.Daemon[GlobalData.AppSettings.ActiveCoin].MiningThreads = selected;
                    Logger.LogDebug("DMN.CTSC", "Setting mining threads: " + selected);
                    GlobalMethods.SaveConfig();
                }
            }
            catch (Exception ex)
            {
                Logger.LogException("DMN.CTSC", ex); ;
            }
        }

        public async void StartStopMining_Clicked(object sender, RoutedEventArgs args)
        {
            try
            {
                var btnStartStopMining = this.Get<Button>("btnStartStopMining");

                if (btnStartStopMining.Content!.ToString()!.Equals(StatusMiner.StopMining))
                {
                    // Stop mining
                    Logger.LogDebug("DMN.SSMC", "Stopping mining");
                    GlobalData.IsManualStoppedMining = true;
                    StopMiningAsync();
                    btnStartStopMining.Content = StatusMiner.StartMining;
                    cbxThreads.IsEnabled = true;
                }
                else
                {
                    if (string.IsNullOrEmpty(GlobalData.AppSettings.Daemon[GlobalData.AppSettings.ActiveCoin].MiningAddress))
                    {
                        // Get and save Mining Address
                        Logger.LogDebug("DMN.SSMC", "Mining address missing. Asking user to provide it");
                        var window = new TextBoxView(title: "Start Mining", labelValue: "Please provide mining address", textValue: string.Empty, textWatermark: "Required - Mining Address", okButtonText: "Start");
                        DialogResult? dialogRes = await DialogService.ShowAsync<DialogResult>(window);

                        if (dialogRes != null && dialogRes.IsOk)
                        {
                            if (!string.IsNullOrEmpty(dialogRes.TextBoxValue))
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
                        await DialogService.ShowAsync(new MessageBoxView("Start Mining", "Cannot start mining without Mining Address", true));
                    }
                    else
                    {
                        if (GlobalData.NetworkStats.NetHeight > GlobalData.NetworkStats.YourHeight)
                        {
                            Logger.LogDebug("DMN.SSMC", "Incorrect height. Aborting mining. NetHeight: " + GlobalData.NetworkStats.NetHeight + " | YourHeight: " + GlobalData.NetworkStats.YourHeight);
                            await DialogService.ShowAsync(new MessageBoxView("Start Mining", "Cannot start mining because you're not fully synchronized.\r\nPlease try again later.", true));
                        }
                        else
                        {
                            // Start mining
                            if (cbxThreads.SelectedItem is int selected && GlobalData.AppSettings.Daemon[GlobalData.AppSettings.ActiveCoin].MiningThreads != selected)
                            {
                                GlobalData.AppSettings.Daemon[GlobalData.AppSettings.ActiveCoin].MiningThreads = selected;
                                GlobalMethods.SaveConfig();
                            }

                            Logger.LogDebug("DMN.SSMC", "Starting mining");
                            StartMiningAsync(GlobalData.AppSettings.Daemon[GlobalData.AppSettings.ActiveCoin].MiningThreads);
                            btnStartStopMining.Content = StatusMiner.StopMining;
                            cbxThreads.IsEnabled = false;
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
            StartMiningAsync(threads, false);
        }

        public async void StartMiningAsync(int threads, bool isUiThread = true)
        {
            try
            {
                if (string.IsNullOrEmpty(GlobalData.AppSettings.Daemon[GlobalData.AppSettings.ActiveCoin].MiningAddress))
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
                            await Dispatcher.UIThread.InvokeAsync(async () =>
                            {
                                await DialogService.ShowAsync(new MessageBoxView("Start Mining", "Error when starting mining\r\n\r\n" + response.Error.Message, true));
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
                            await Dispatcher.UIThread.InvokeAsync(async () =>
                            {
                                await DialogService.ShowAsync(new MessageBoxView("Start Mining", "Response: " + response.Status, true));
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
            StopMiningAsync(false);
        }

        public async void StopMiningAsync(bool isUiThread = true)
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
                        await Dispatcher.UIThread.InvokeAsync(async () =>
                        {
                            await DialogService.ShowAsync(new MessageBoxView("Stop Mining", "Error when stopping mining\r\n\r\n" + response.Error.Message, true));
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
