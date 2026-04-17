using NervaOneWalletMiner.Objects.Constants;
using NervaOneWalletMiner.ViewModels;
using System;

namespace NervaOneWalletMiner.Helpers
{
    public static class MasterProcess
    {
        public static System.Timers.Timer? _masterUpdateTimer;
        public static readonly int _masterTimerInterval = GlobalMethods.IsAndroid() ? 2000 : 1000;
        public static DateTime _cliToolsRunningLastCheck = DateTime.MinValue;
        public static bool _killMasterProcess = false;

        // Per-operation timestamps. It used to be _masterTimerCount but it's unreliable on Android as it gets throttled in the background
        public static DateTime _lastDaemonDataFetch = DateTime.MinValue;
        public static DateTime _lastWalletDataFetch = DateTime.MinValue;
        public static DateTime _lastWalletSave = DateTime.Now;

        // 5 sec on Desktop, 10 sec on Android
        public static readonly int _daemonFetchSeconds = (_masterTimerInterval / 1000) * GlobalData.AppSettings.TimerIntervalMultiplier;
        public static readonly int _walletFetchSeconds = _daemonFetchSeconds * 2;
        public static readonly int _backgroundFetchSeconds = 60;
        public static readonly int _cliToolsHealthCheckSeconds = 60;
        public static readonly int _walletSaveSeconds = 300;
        public static readonly int _daemonResponseRestartSeconds = 300;

        // Skip all UI updates when the Android activity is paused (screen off / app in background)        
        public static volatile bool _isInBackgroundMode = false;

        // Only fire UpdateDaemonView when fresh data arrives
        public static volatile bool _isDaemonDataFresh = false;

        public static void EnterBackgroundMode()
        {
            _isInBackgroundMode = true;
        }

        public static void ExitBackgroundMode()
        {
            _isInBackgroundMode = false;

            // Reset all fetch timestamps so data is fetched immediately on the next tick
            _lastDaemonDataFetch = DateTime.MinValue;
            _lastWalletDataFetch = DateTime.MinValue;
        }


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

                    Logger.LogDebug("MSP.SMUP", "Master timer running every " + _masterTimerInterval / 1000 + " seconds. Daemon updates every " + _daemonFetchSeconds + " seconds. Wallet updates every " + _walletFetchSeconds + " seconds");
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


                // If kill master process is issued at any point, skip everything else and do not restart master timer
                if (_cliToolsRunningLastCheck.AddSeconds(_cliToolsHealthCheckSeconds) < DateTime.Now)
                {
                    //Logger.LogDebug("MSP.MUPS", "_cliToolsRunningLastCheck: " + _cliToolsRunningLastCheck.ToLongTimeString() + ", IsGetAndSetDaemonDataComplete: " + GlobalData.IsGetAndSetDaemonDataComplete + ", LastDaemonResponseTime: " + GlobalData.LastDaemonResponseTime.ToLongTimeString());
                    _cliToolsRunningLastCheck = DateTime.Now;

                    if (!GlobalData.AppSettings.Daemon[GlobalData.AppSettings.ActiveCoin].IsWalletOnly)
                    {
                        if (!_killMasterProcess)
                        {                            
                            KeepDaemonRunning();

                            // Auto start mining if setting enabled
                            if (GlobalData.NetworkStats.YourHeight > 0
                                && GlobalData.NetworkStats.YourHeight == GlobalData.NetworkStats.NetHeight
                                && GlobalData.NetworkStats.MinerStatus == StatusMiner.Inactive
                                && GlobalData.AppSettings.Daemon[GlobalData.AppSettings.ActiveCoin].AutoStartMining
                                && !GlobalData.IsManualStoppedMining
                                && !GlobalData.IsNoConnectionsStoppedMining
                                && !GlobalData.IsHashRateMonitoringStoppedMining)
                            {
                                Logger.LogDebug("MSP.MUPS", "Auto starting mining");
                                ((DaemonViewModel)GlobalData.ViewModelPages[SplitViewPages.Daemon]).StartMiningNonUi(GlobalData.AppSettings.Daemon[GlobalData.AppSettings.ActiveCoin].MiningThreads);
                            }

                            // If 0 connections, stop mining so we're not using CPU resources when no connections to the network
                            if (GlobalData.NetworkStats.MinerStatus == StatusMiner.Mining
                                && GlobalData.NetworkStats.ConnectionsOut + GlobalData.NetworkStats.ConnectionsIn < 1
                                && !GlobalData.IsNoConnectionsStoppedMining)
                            {
                                GlobalData.IsNoConnectionsStoppedMining = true;

                                Logger.LogDebug("MSP.MUPS", "Auto stopping mining. No connections");
                                ((DaemonViewModel)GlobalData.ViewModelPages[SplitViewPages.Daemon]).StopMiningNonUi();
                            }

                            // When connections come back, start mining again
                            if (GlobalData.NetworkStats.ConnectionsOut + GlobalData.NetworkStats.ConnectionsIn > 0
                                && GlobalData.IsNoConnectionsStoppedMining)
                            {
                                GlobalData.IsNoConnectionsStoppedMining = false;

                                Logger.LogDebug("MSP.MUPS", "Auto restarting mining. Total connections: " + (GlobalData.NetworkStats.ConnectionsOut + GlobalData.NetworkStats.ConnectionsIn));
                                ((DaemonViewModel)GlobalData.ViewModelPages[SplitViewPages.Daemon]).StartMiningNonUi(GlobalData.AppSettings.Daemon[GlobalData.AppSettings.ActiveCoin].MiningThreads);
                            }

                            // Hashrate monitoring
                            if (GlobalData.NetworkStats.YourHeight > 0
                                && GlobalData.NetworkStats.YourHeight == GlobalData.NetworkStats.NetHeight
                                && !GlobalData.IsManualStoppedMining
                                && !GlobalData.IsNoConnectionsStoppedMining)
                            {
                                if (GlobalData.AppSettings.Daemon[GlobalData.AppSettings.ActiveCoin].EnableMiningThreshold)
                                {
                                    // Mining threshold is enabled. Run through logic to pause/unpause mining
                                    HashRateMonitoringStartStopMining();
                                }
                                else if (GlobalData.IsHashRateMonitoringStoppedMining)
                                {
                                    Logger.LogDebug("MSP.MUPS", "Hash rate monitoring stopped mining but mining threshold is now disabled. Restarting mining.");
                                    GlobalData.IsHashRateMonitoringStoppedMining = false;
                                    ((DaemonViewModel)GlobalData.ViewModelPages[SplitViewPages.Daemon]).StartMiningNonUi(GlobalData.AppSettings.Daemon[GlobalData.AppSettings.ActiveCoin].MiningThreads);
                                }
                            }
                        }
                    }


                    if (!_killMasterProcess)
                    {
                        KeepWalletProcessRunning();
                    }
                }


                if (_isInBackgroundMode)
                {
                    if (!_killMasterProcess
                        && !GlobalData.AppSettings.Daemon[GlobalData.AppSettings.ActiveCoin].IsWalletOnly
                        && _lastDaemonDataFetch.AddSeconds(_backgroundFetchSeconds) < DateTime.Now)
                    {
                        //Logger.LogDebug("MSP.MUPS", "IsGASDDC: " + GlobalData.IsGetAndSetDaemonDataComplete + ", LDRT: " + GlobalData.LastDaemonResponseTime.ToLongTimeString() + ", _lDDF: " + _lastDaemonDataFetch.ToLongTimeString()  + ", _cTRLC: " + _cliToolsRunningLastCheck.ToLongTimeString());
                        _lastDaemonDataFetch = DateTime.Now;
                        UIManager.GetAndSetDaemonData();
                    }
                }
                else
                {
                    // Get Daemon data
                    if (!GlobalData.AppSettings.Daemon[GlobalData.AppSettings.ActiveCoin].IsWalletOnly)
                    {
                        if (!_killMasterProcess && _lastDaemonDataFetch.AddSeconds(_daemonFetchSeconds) < DateTime.Now)
                        {
                            _lastDaemonDataFetch = DateTime.Now;
                            UIManager.HandleNetworkStats();
                            UIManager.GetAndSetDaemonData();
                        }

                        // Actual Daemon UI update — only run when GetAndSetDaemonData just produced fresh data
                        if (_isDaemonDataFresh && GlobalData.IsGetAndSetDaemonDataComplete)
                        {
                            _isDaemonDataFresh = false;
                            UIManager.UpdateDaemonView();
                            UIManager.UpdateStatusBar();
                        }
                    }
                    else
                    {
                        UIManager.HandleNetworkStats();
                        UIManager.UpdateStatusBar();
                    }

                    // Get Wallets/Transfers data
                    if (!_killMasterProcess
                        && (GlobalData.IsInitialDaemonConnectionSuccess || GlobalData.AppSettings.Daemon[GlobalData.AppSettings.ActiveCoin].IsWalletOnly)
                        && GlobalData.IsWalletOpen)
                    {
                        if (GlobalData.IsWalletJustOpened || _lastWalletDataFetch.AddSeconds(_walletFetchSeconds) < DateTime.Now)
                        {
                            _lastWalletDataFetch = DateTime.Now;
                            UIManager.CallWalletDataMethodsInSync();
                        }
                    }

                    // Actual Wallets/Transfers UI update
                    if (GlobalData.IsGetAndSetWalletDataComplete)
                    {
                        UIManager.UpdateWalletView();
                    }
                    if (GlobalData.IsGetAndSetTransfersDataComplete)
                    {
                        UIManager.UpdateTransfersView();
                    }

                    if (GlobalData.IsWalletJustOpened)
                    {
                        // Will use this to auto-save wallet so need to reset it when wallet opens
                        GlobalData.IsWalletJustOpened = false;
                        _lastWalletSave = DateTime.Now;
                    }

                    if (GlobalData.IsWalletOpen && _lastWalletSave.AddSeconds(_walletSaveSeconds) < DateTime.Now)
                    {
                        if (GlobalData.CoinSettings[GlobalData.AppSettings.ActiveCoin].IsSavingWalletSupported)
                        {
                            // Auto save wallet every 5 min
                            _lastWalletSave = DateTime.Now;
                            Logger.LogDebug("MSP.MUPS", "Auto saving wallet: " + GlobalData.OpenedWalletName);

                            GlobalMethods.SaveWallet();
                        }
                    }

                    if (!GlobalData.AppSettings.Daemon[GlobalData.AppSettings.ActiveCoin].IsWalletOnly)
                    {
                        if (GlobalData.AppSettings.Daemon[GlobalData.AppSettings.ActiveCoin].EnableConnectionsGuard)
                        {
                            if (GlobalData.ConnectGuardLastGoodTime.AddMinutes(GlobalData.ConnectGuardMinutes * GlobalData.ConnectGuardRestartCount) < DateTime.Now)
                            {
                                Logger.LogDebug("MSP.MUPS", "Connections guard forcing restart. Last good time: " + GlobalData.ConnectGuardLastGoodTime.ToLongTimeString() + " | Restart Ct: " + GlobalData.ConnectGuardRestartCount);

                                // Pop blocks every 3rd restart
                                ConnectionsGuardRestart(GlobalData.ConnectGuardRestartCount % 3 == 0);
                                GlobalData.ConnectGuardRestartCount++;
                            }
                        }
                    }
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
                    _masterUpdateTimer.Start();
                }
            }
        }

        private static void KeepDaemonRunning()
        {
            try
            {
                bool forceRestart = false;
                DateTime daemonRestartTime = GlobalData.LastDaemonResponseTime.AddSeconds(_daemonResponseRestartSeconds);
                if (_isInBackgroundMode)
                {
                    // Double the restart time when in background mode
                    daemonRestartTime = daemonRestartTime.AddSeconds(_daemonResponseRestartSeconds);
                }

                if (daemonRestartTime < DateTime.Now)
                {
                    // Daemon not responding. Kill and restart
                    forceRestart = true;
                    Logger.LogDebug("MSP.KDNR", "No response from daemon since: " + GlobalData.LastDaemonResponseTime.ToLongTimeString() + ". Forcing restart...");
                    GlobalData.LastDaemonResponseTime = DateTime.Now;
                }

                bool isDaemonRunning = ProcessManager.IsRunning(GlobalData.DaemonProcessName);
                if (!isDaemonRunning || forceRestart)
                {
                    if (GlobalData.LastDaemonRestartAttempt.AddSeconds(_daemonResponseRestartSeconds) < DateTime.Now)
                    {
                        if (!GlobalData.IsCliToolsDownloading && GlobalMethods.DirectoryContainsCliTools(GlobalData.CliToolsDir))
                        {
                            Logger.LogDebug("MSP.KDNR", "Issues with Daemon process. isDaemonRunning: " + isDaemonRunning + ", forceRestart: " + forceRestart + ". Restarting...");

                            GlobalData.LastDaemonRestartAttempt = DateTime.Now;
                            ProcessManager.Kill(GlobalData.DaemonProcessName);
                            ProcessManager.StartExternalProcess(GlobalMethods.GetDaemonProcess(), GlobalData.CoinSettings[GlobalData.AppSettings.ActiveCoin].GenerateDaemonOptions(GlobalData.AppSettings.Daemon[GlobalData.AppSettings.ActiveCoin]));
                            GlobalData.IsInitialDaemonConnectionSuccess = false;
                            GlobalData.IsCliToolsFound = true;
                        }
                        else
                        {
                            Logger.LogInfo("MSP.KDNR", "CLI tools not found");
                        }
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
                if (!GlobalData.IsCliToolsDownloading && GlobalMethods.DirectoryContainsCliTools(GlobalData.CliToolsDir) && !GlobalData.IsCliToolsFound)
                {
                    // Client tools downloaded and found
                    Logger.LogDebug("MSP.KWPR", "Client tools found.");
                    GlobalData.IsCliToolsFound = true;
                }

                if (GlobalData.CoinSettings[GlobalData.AppSettings.ActiveCoin].IsDaemonWalletSeparateApp)
                {
                    if (!GlobalData.IsCliToolsDownloading && !ProcessManager.IsRunning(GlobalData.WalletProcessName))
                    {
                        if (GlobalMethods.DirectoryContainsCliTools(GlobalData.CliToolsDir))
                        {
                            Logger.LogDebug("MSP.KWPR", "Killing wallet process if it's running.");
                            ProcessManager.Kill(GlobalData.WalletProcessName);
                            Logger.LogDebug("MSP.KWPR", "Starting wallet process");
                            ProcessManager.StartExternalProcess(GlobalMethods.GetRpcWalletProcess(), GlobalData.CoinSettings[GlobalData.AppSettings.ActiveCoin].GenerateWalletOptions(GlobalData.AppSettings.Wallet[GlobalData.AppSettings.ActiveCoin], GlobalData.AppSettings.Daemon[GlobalData.AppSettings.ActiveCoin]));
                        }
                        else
                        {
                            Logger.LogDebug("MSP.KWPR", "CLI tools not found");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogException("MSP.KWPR", ex);
            }
        }

        private static void ConnectionsGuardRestart(bool popBlocks)
        {
            try
            {
                if (GlobalData.AppSettings.Daemon[GlobalData.AppSettings.ActiveCoin].EnableConnectionsGuard)
                {
                    string restartOptions = string.Empty;

                    if (popBlocks)
                    {
                        restartOptions = GlobalData.CoinSettings[GlobalData.AppSettings.ActiveCoin].IsPoppingBlocksSupported ? GlobalData.CoinSettings[GlobalData.AppSettings.ActiveCoin].GeneratePopBlocksOption(GlobalData.ConnectGuardBlocksToPop) : string.Empty;
                    }

                    ProcessManager.Kill(GlobalData.WalletProcessName);
                    GlobalMethods.StopAndCloseDaemon();
                    GlobalData.IsDaemonRestarting = true;

                    Logger.LogDebug("MSP.CSGR", "Connections guard restaring using options: " + restartOptions);
                    ProcessManager.StartExternalProcess(GlobalMethods.GetDaemonProcess(), GlobalData.CoinSettings[GlobalData.AppSettings.ActiveCoin].GenerateDaemonOptions(GlobalData.AppSettings.Daemon[GlobalData.AppSettings.ActiveCoin]) + " " + restartOptions);
                }
            }
            catch (Exception ex)
            {
                Logger.LogException("MSP.CSGR", ex);
            }
        }

        private static void HashRateMonitoringStartStopMining()
        {
            try
            {
                if(GlobalData.AppSettings.Daemon[GlobalData.AppSettings.ActiveCoin].EnableMiningThreshold)
                {
                    int hashRateKH = (int)(GlobalData.NetworkStats.NetHashRate / 1000.0d);
                    int stopThreshold = GlobalData.AppSettings.Daemon[GlobalData.AppSettings.ActiveCoin].StopMiningThreshold;

                    if(GlobalData.NetworkStats.MinerStatus == StatusMiner.Mining && hashRateKH > stopThreshold)
                    {
                        Logger.LogDebug("MSP.HMSS", "Hash rate " + hashRateKH.ToString("F1") + " KH/s above threshold " + stopThreshold + " KH/s, pausing mining");

                        ((DaemonViewModel)GlobalData.ViewModelPages[SplitViewPages.Daemon]).StopMiningNonUi();
                        GlobalData.IsHashRateMonitoringStoppedMining = true;
                    }

                    if(GlobalData.IsHashRateMonitoringStoppedMining && hashRateKH < stopThreshold)
                    {
                        Logger.LogDebug("MSP.HMSS", "Hash rate " + hashRateKH.ToString("F1") + " KH/s below threshold " + stopThreshold + " KH/s, restarting mining");

                        ((DaemonViewModel)GlobalData.ViewModelPages[SplitViewPages.Daemon]).StartMiningNonUi(GlobalData.AppSettings.Daemon[GlobalData.AppSettings.ActiveCoin].MiningThreads);
                        GlobalData.IsHashRateMonitoringStoppedMining = false;
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogException("MSP.HMSS", ex);
            }
        }
    }
}
