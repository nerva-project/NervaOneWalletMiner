using NervaOneWalletMiner.Objects.Constants;
using NervaOneWalletMiner.Rpc.Wallet.Requests;
using NervaOneWalletMiner.Rpc.Wallet.Responses;
using NervaOneWalletMiner.ViewModels;
using System;

namespace NervaOneWalletMiner.Helpers
{
    public static class MasterProcess
    {
        public static System.Timers.Timer? _masterUpdateTimer;
        public const int _masterTimerInterval = 1000;        
        public static DateTime _cliToolsRunningLastCheck = DateTime.MinValue;
        public static bool _killMasterProcess = false;
        public static bool _cliToolsFound = true;
        public static int _masterTimerCount = 0;
        

        public static void StartMasterUpdateProcess()
        {
            try
            {
                Logger.LogDebug("MSP.SMUP", "Start Master Update Process");

                if (_masterUpdateTimer == null)
                {
                    _masterUpdateTimer = new System.Timers.Timer();
                    _masterUpdateTimer.Interval = _masterTimerInterval;
                    _masterUpdateTimer.Elapsed += (s, e) => MasterUpdateProcess();
                    _masterUpdateTimer.Start();

                    if (GlobalData.CoinSettings[GlobalData.AppSettings.ActiveCoin].IsDaemonWalletSeparateApp)
                    {
                        // If wallet process was not closed properly before, it could be running listening on different port so opening wallet will fail
                        Logger.LogDebug("MSP.SMUP", "Calling wallet ForceClose");
                        ProcessManager.Kill(GlobalData.WalletProcessName);
                    }

                    Logger.LogDebug("MSP.SMUP", "Master timer running every " + _masterTimerInterval / 1000 + " seconds. Update every " + (_masterTimerInterval / 1000) * GlobalData.AppSettings.TimerIntervalMultiplier + " seconds");
                }
            }
            catch (Exception ex)
            {
                Logger.LogException("MSP.SMUP", ex);
            }
        }

        public static void MasterUpdateProcess()
        {
            try
            {
                if (_masterUpdateTimer != null)
                {
                    _masterUpdateTimer.Stop();
                }

                if (UIManager.GetCoinIcon() != GlobalMethods.GetLogo())
                {
                    UIManager.UpdateCoinIcon(GlobalMethods.GetLogo());
                }

                // If kill master process is issued at any point, skip everything else and do not restrt master timer            
                if (_cliToolsRunningLastCheck.AddSeconds(10) < DateTime.Now)
                {
                    _cliToolsRunningLastCheck = DateTime.Now;

                    if (!_killMasterProcess)
                    {
                        KeepDaemonRunning();

                        // TODO: Maybe do it another way. Might be good enough though
                        if (GlobalData.NetworkStats.YourHeight > 0
                            && GlobalData.NetworkStats.YourHeight == GlobalData.NetworkStats.NetHeight
                            && GlobalData.NetworkStats.MinerStatus == StatusMiner.Inactive
                            && GlobalData.AppSettings.Daemon[GlobalData.AppSettings.ActiveCoin].AutoStartMining
                            && !GlobalData.IsManualStopMining)
                        {
                            Logger.LogDebug("MSP.MUPS", "Auto starting mining");
                            ((DaemonViewModel)GlobalData.ViewModelPages[SplitViewPages.Daemon]).StartMiningNonUi(GlobalData.AppSettings.Daemon[GlobalData.AppSettings.ActiveCoin].MiningThreads);
                        }
                    }


                    if (!_killMasterProcess)
                    {
                        KeepWalletProcessRunning();
                    }
                }


                if (!_cliToolsFound)
                {
                    // TODO: For now
                    UIManager.UpdateDaemonStatus("Client tools not found. Attempting to download...");
                }

                // Update UI
                if (!_killMasterProcess && _cliToolsFound)
                {
                    if (_masterTimerCount % GlobalData.AppSettings.TimerIntervalMultiplier == 0)
                    {
                        UIManager.DaemonUiUpdate();
                    }
                }

                if (!_killMasterProcess && GlobalData.IsInitialDaemonConnectionSuccess && GlobalData.IsWalletOpen)
                {
                    if (GlobalData.IsWalletJustOpened)
                    {
                        GlobalMethods.WalletUiUpdate();
                        UIManager.TransfersUiUpdate();
                        SetWalletHeight();
                    }
                    else if (_masterTimerCount % (GlobalData.AppSettings.TimerIntervalMultiplier * 2) == 0)
                    {
                        // Update wallet every 2nd call because you do not need to do it more often
                        GlobalMethods.WalletUiUpdate();
                        UIManager.TransfersUiUpdate();
                        SetWalletHeight();
                    }
                }

                // Actual UI update. If walet was closed, it will clear things
                UIManager.UpdateWalletView();
                UIManager.UpdateTransfersView();

                if (GlobalData.IsWalletJustOpened)
                {
                    // Will use this to auto-save wallet so need to reset it at the end
                    GlobalData.IsWalletJustOpened = false;
                }

                if (GlobalData.IsWalletOpen & _masterTimerCount % 300 == 0)
                {
                    // Auto save wallet every 5 min
                    Logger.LogDebug("MSP.MUPS", "Auto saving wallet: " + GlobalData.OpenedWalletName);
                    GlobalMethods.SaveWallet();
                }
            }
            catch (Exception ex)
            {
                Logger.LogException("MSP.MUPS", ex);
            }
            finally
            {
                // Restart timer
                if (_masterUpdateTimer == null)
                {
                    Logger.LogError("MSP.MUPS", "Timer is NULL. Recreating. Why?");
                    _masterUpdateTimer = new System.Timers.Timer();
                    _masterUpdateTimer.Interval = _masterTimerInterval;
                    _masterUpdateTimer.Elapsed += (s, e) => MasterUpdateProcess();
                }

                if (!_killMasterProcess)
                {
                    _masterTimerCount++;
                    _masterUpdateTimer.Start();
                }
            }
        }

        private static void KeepDaemonRunning()
        {
            try
            {
                bool forceRestart = false;
                if (GlobalData.LastDaemonResponseTime.AddMinutes(5) < DateTime.Now)
                {
                    // Daemon not responding. Kill and restart
                    forceRestart = true;
                    GlobalData.LastDaemonResponseTime = DateTime.Now;
                    Logger.LogDebug("MSP.KDNR", "No response from daemon since: " + GlobalData.LastDaemonResponseTime.ToLongTimeString() + " . Forcing restart...");
                }

                if (!ProcessManager.IsRunning(GlobalData.DaemonProcessName) || forceRestart)
                {
                    if (GlobalMethods.DirectoryContainsCliTools(GlobalData.CliToolsDir))
                    {
                        ProcessManager.Kill(GlobalData.DaemonProcessName);
                        Logger.LogDebug("MSP.KDNR", "Starting daemon process");
                        ProcessManager.StartExternalProcess(GlobalMethods.GetDaemonProcess(), GlobalData.CoinSettings[GlobalData.AppSettings.ActiveCoin].GenerateDaemonOptions(GlobalData.AppSettings.Daemon[GlobalData.AppSettings.ActiveCoin]));
                        GlobalData.IsInitialDaemonConnectionSuccess = false;
                        _cliToolsFound = true;
                    }
                    else
                    {
                        Logger.LogInfo("MSP.KDNR", "CLI tools not found");
                        _cliToolsFound = false;
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogException("MSP.KDNR", ex);
            }
        }

        private static void KeepWalletProcessRunning()
        {
            try
            {
                if (!ProcessManager.IsRunning(GlobalData.WalletProcessName))
                {
                    if (GlobalMethods.DirectoryContainsCliTools(GlobalData.CliToolsDir))
                    {
                        Logger.LogDebug("MSP.KWPR", "Calling Wallet ForceClose");
                        ProcessManager.Kill(GlobalData.WalletProcessName);
                        Logger.LogDebug("MSP.KWPR", "Starting wallet process");
                        ProcessManager.StartExternalProcess(GlobalMethods.GetRpcWalletProcess(), GlobalData.CoinSettings[GlobalData.AppSettings.ActiveCoin].GenerateWalletOptions(GlobalData.AppSettings.Wallet[GlobalData.AppSettings.ActiveCoin], GlobalData.AppSettings.Daemon[GlobalData.AppSettings.ActiveCoin].Rpc));
                    }
                    else
                    {
                        Logger.LogDebug("MSP.KWPR", "CLI tools not found");
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogException("MSP.KWPR", ex);
            }
        }

        public static async void SetWalletHeight()
        {
            try
            {
                GetHeightResponse resGetHeight = await GlobalData.WalletService.GetHeight(GlobalData.AppSettings.Wallet[GlobalData.AppSettings.ActiveCoin].Rpc, new GetHeightRequest());

                if (resGetHeight.Error.IsError)
                {
                    Logger.LogError("MSP.SWHT", "GetTransfers Error Code: " + resGetHeight.Error.Code + ", Message: " + resGetHeight.Error.Message);
                }
                else
                {
                    GlobalData.WalletHeight = resGetHeight.Height;
                }
            }
            catch (Exception ex)
            {
                Logger.LogException("MSP.SWHT", ex);
            }
        }
    }
}