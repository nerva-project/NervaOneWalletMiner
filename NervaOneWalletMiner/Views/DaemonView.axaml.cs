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

                var nupThreads = this.Get<NumericUpDown>("nupThreads");
                nupThreads.Maximum = GlobalData.CpuThreadCount;
                nupThreads.Value = GlobalData.AppSettings.Daemon[GlobalData.AppSettings.ActiveCoin].MiningThreads;

                Initialized += DaemonView_Initialized;
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
                    vm.StartMiningUIEvent += (owner, threads) => StartMiningAsync(owner, threads);
                    vm.StartMiningProcessEvent += StartMiningNonUiAsync;
                    GlobalData.AreDaemonEventsRegistered = true;
                }
            }
            catch (Exception ex)
            {
                Logger.LogException("DMN.DAVI", ex); ;
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
                    StopMiningAsync();
                    btnStartStopMining.Content = StatusMiner.StartMining;
                    nupThreads.IsEnabled = true;
                }
                else
                {
                    if(string.IsNullOrEmpty(GlobalData.AppSettings.Daemon[GlobalData.AppSettings.ActiveCoin].MiningAddress))
                    {
                        // Get and save Mining Address
                        Logger.LogDebug("DMN.SSMC", "Mining address missing. Asking user to provide it");
                        var window = new TextBoxView("Start Mining", string.Empty, "Required - Mining Address", "Please provide mining address", true);
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
                        // Start mining
                        if (GlobalData.AppSettings.Daemon[GlobalData.AppSettings.ActiveCoin].MiningThreads != nupThreads.Value)
                        {
                            GlobalData.AppSettings.Daemon[GlobalData.AppSettings.ActiveCoin].MiningThreads = Convert.ToInt32(nupThreads.Value);
                            GlobalMethods.SaveConfig();
                        }

                        Logger.LogDebug("DMN.SSMC", "Starting mining");
                        GlobalData.IsManualStopMining = false;
                        StartMiningAsync(GetWindow(), GlobalData.AppSettings.Daemon[GlobalData.AppSettings.ActiveCoin].MiningThreads);
                        btnStartStopMining.Content = StatusMiner.StopMining;
                        nupThreads.IsEnabled = false;
                    }
                }                
            }
            catch (Exception ex)
            {
                Logger.LogException("DMN.SSMC", ex);
            }
        }

        // Master Process does not have owner so doing it this way
        public void StartMiningNonUiAsync(int threads)
        {
            StartMiningAsync(GetWindow(), threads, true);
        }
        public async void StartMiningAsync(Window owner, int threads, bool isProcess = false)
        {
            try
            {
                if(string.IsNullOrEmpty(GlobalData.AppSettings.Daemon[GlobalData.AppSettings.ActiveCoin].MiningAddress))
                {
                    Logger.LogError("GLM.STMA", "Mining address missing. Cannot start mining");
                }
                else
                {
                    StartMiningRequest request = new()
                    {
                        MiningAddress = GlobalData.AppSettings.Daemon[GlobalData.AppSettings.ActiveCoin].MiningAddress,
                        ThreadCount = threads
                    };

                    Logger.LogDebug("GLM.STMA", "Calling StartMining. Address: " + GlobalMethods.GetShorterString(request.MiningAddress, 12) + " | Threads: " + request.ThreadCount);
                    StartMiningResponse response = await GlobalData.DaemonService.StartMining(GlobalData.AppSettings.Daemon[GlobalData.AppSettings.ActiveCoin].Rpc, request);
                    if (response.Error.IsError)
                    {
                        Logger.LogDebug("GLM.STMA", "Error starting mining | Message: " + response.Error.Message + " | Code: " + response.Error.Code);

                        if(!isProcess)
                        {
                            await Dispatcher.UIThread.Invoke(async () =>
                            {
                                MessageBoxView window = new("Start Mining", "Error when starting mining\r\n\r\n" + response.Error.Message, true);
                                await window.ShowDialog(owner);
                            });
                        }
                    }
                    else
                    {
                        // Some errors are not reported as errors, such as not being able to mine to subaddress
                        Logger.LogDebug("GLM.STMA", "Start mining response status: " + response.Status);

                        if(!isProcess)
                        {
                            await Dispatcher.UIThread.Invoke(async () =>
                            {
                                MessageBoxView window = new("Start Mining", "Response: " + response.Status, true);
                                await window.ShowDialog(owner);
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogException("GLM.STMA", ex);
            }
        }


        public async void StopMiningAsync()
        {
            try
            {
                StopMiningRequest request = new();

                Logger.LogDebug("GLM.SPMA", "Calling StopMining");
                StopMiningResponse response = await GlobalData.DaemonService.StopMining(GlobalData.AppSettings.Daemon[GlobalData.AppSettings.ActiveCoin].Rpc, request);
                if (response.Error.IsError)
                {
                    await Dispatcher.UIThread.Invoke(async () =>
                    {
                        MessageBoxView window = new("Stop Mining", "Error when stopping mining\r\n\r\n" + response.Error.Message, true);
                        await window.ShowDialog(GetWindow());
                    });
                }
            }
            catch (Exception ex)
            {
                Logger.LogException("GLM.SPMA", ex);
            }
        }
    }
}