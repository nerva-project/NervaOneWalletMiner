using Avalonia.Controls;
using Avalonia.Controls.Selection;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using NervaOneWalletMiner.Helpers;
using NervaOneWalletMiner.Objects.Constants;
using NervaOneWalletMiner.Objects.DataGrid;
using NervaOneWalletMiner.Rpc;
using NervaOneWalletMiner.Rpc.Daemon.Requests;
using NervaOneWalletMiner.Rpc.Daemon.Responses;
using NervaOneWalletMiner.Rpc.Wallet.Requests;
using NervaOneWalletMiner.Rpc.Wallet.Responses;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Input;

namespace NervaOneWalletMiner.ViewModels;

public class MainViewModel : ViewModelBase
{
    public static System.Timers.Timer? _masterUpdateTimer;
    public const int _masterTimerInterval = 1000;
    public static int _masterTimerCount = 0;
    public static bool _killMasterProcess = false;
    public static bool _cliToolsFound = true;

    public static DateTime _cliToolsRunningLastCheck = DateTime.MinValue;
    public static DateTime _lastDaemonResponseTime = DateTime.Now;
    public static bool _isInitialDaemonConnectionSuccess = false;

    public static Dictionary<string, ViewModelBase> ViewModelPagesDictionary = new();

    public static readonly Bitmap _inImage = new Bitmap(AssetLoader.Open(new Uri("avares://NervaOneWalletMiner/Assets/transfer_in.png")));
    public static readonly Bitmap _outImage = new Bitmap(AssetLoader.Open(new Uri("avares://NervaOneWalletMiner/Assets/transfer_out.png")));
    public static readonly Bitmap _blockImage = new Bitmap(AssetLoader.Open(new Uri("avares://NervaOneWalletMiner/Assets/transfer_block.png")));
    public static readonly Bitmap _walletImage = new Bitmap(AssetLoader.Open(new Uri("avares://NervaOneWalletMiner/Assets/wallet.png")));

    public static bool _isTransfersUpdateComplete = true;

    public static string _currentCoin = GlobalData.AppSettings.ActiveCoin;

    private bool? _isPaneOpen = false;
    private ViewModelBase _CurrentPage;
    public SelectionModel<ListBoxItem> Selection { get; }
    public ICommand TriggerPaneCommand { get; }
    

    public MainViewModel()
    {
        // Set up split view pages
        ViewModelPagesDictionary.Add(SplitViewPages.Daemon, new DaemonViewModel());
        ViewModelPagesDictionary.Add(SplitViewPages.Wallet, new WalletViewModel());        
        ViewModelPagesDictionary.Add(SplitViewPages.Transfers, new TransfersViewModel());
        ViewModelPagesDictionary.Add(SplitViewPages.DaemonSetup, new DaemonSetupViewModel());
        ViewModelPagesDictionary.Add(SplitViewPages.WalletSetup, new WalletSetupViewModel());
        ViewModelPagesDictionary.Add(SplitViewPages.Settings, new SettingsViewModel());

        TriggerPaneCommand = ReactiveCommand.Create(TriggerPane);

        _CurrentPage = ViewModelPagesDictionary[SplitViewPages.Daemon];

        Selection = new SelectionModel<ListBoxItem>();
        Selection.SelectionChanged += SelectionChanged;

        StartMasterUpdateProcess();

        UpdateMainView();
    }

    public bool? IsPaneOpen
    {
        get => _isPaneOpen;
        set => this.RaiseAndSetIfChanged(ref _isPaneOpen, value);
    }

    public ViewModelBase CurrentPage
    {
        get { return _CurrentPage; }
        private set { this.RaiseAndSetIfChanged(ref _CurrentPage, value); }
    }

    private string _DaemonStatus = "";
    public string DaemonStatus
    {
        get => _DaemonStatus;
        set => this.RaiseAndSetIfChanged(ref _DaemonStatus, value);
    }

    // Status Bar
    private string _DaemonVersion = "";
    public string DaemonVersion
    {
        get => _DaemonVersion;
        set => this.RaiseAndSetIfChanged(ref _DaemonVersion, value);
    }

    private string _WalletStatus = GlobalData.WalletClosedMessage;
    public string WalletStatus
    {
        get => _WalletStatus;
        set => this.RaiseAndSetIfChanged(ref _WalletStatus, value);
    }

    // TODO: Figure out how to do this in one place instead of on each view
    private Bitmap _CoinIcon = GlobalData.Logo;
    public Bitmap CoinIcon
    {
        get => _CoinIcon;
        set => this.RaiseAndSetIfChanged(ref _CoinIcon, value);
    }

    void SelectionChanged(object? sender, SelectionModelSelectionChangedEventArgs e)
    {
        // TODO: Figoure out better way of doing this
        switch(((ListBoxItem)e.SelectedItems[0]!).Name)
        {
            case SplitViewPages.Wallet:
                CurrentPage = ViewModelPagesDictionary[SplitViewPages.Wallet];
                break;
            case SplitViewPages.Transfers:
                CurrentPage = ViewModelPagesDictionary[SplitViewPages.Transfers];
                break;
            case SplitViewPages.DaemonSetup:
                CurrentPage = ViewModelPagesDictionary[SplitViewPages.DaemonSetup];
                break;
            case SplitViewPages.WalletSetup:
                CurrentPage = ViewModelPagesDictionary[SplitViewPages.WalletSetup];
                break;
            case SplitViewPages.Settings:
                CurrentPage = ViewModelPagesDictionary[SplitViewPages.Settings];
                break;
            default:
                CurrentPage = ViewModelPagesDictionary[SplitViewPages.Daemon];
                break;
        }
    }

    private void TriggerPane()
    {
        IsPaneOpen = !IsPaneOpen;
    }

    public void UpdateMainView()
    {
        // Daemon View
        ((DaemonViewModel)ViewModelPagesDictionary[SplitViewPages.Daemon]).NetHeight = GlobalData.NetworkStats.NetHeight.ToString();
        ((DaemonViewModel)ViewModelPagesDictionary[SplitViewPages.Daemon]).YourHeight = GlobalData.NetworkStats.YourHeight.ToString();
        ((DaemonViewModel)ViewModelPagesDictionary[SplitViewPages.Daemon]).NetHash = GlobalData.NetworkStats.NetHash;
        ((DaemonViewModel)ViewModelPagesDictionary[SplitViewPages.Daemon]).RunTime = GlobalData.NetworkStats.RunTime;

        ((DaemonViewModel)ViewModelPagesDictionary[SplitViewPages.Daemon]).MinerMessage = GlobalData.NetworkStats.MinerStatus;
        ((DaemonViewModel)ViewModelPagesDictionary[SplitViewPages.Daemon]).YourHash = GlobalData.NetworkStats.YourHash;
        ((DaemonViewModel)ViewModelPagesDictionary[SplitViewPages.Daemon]).BlockTime = GlobalData.NetworkStats.BlockTime;
        ((DaemonViewModel)ViewModelPagesDictionary[SplitViewPages.Daemon]).MiningAddress = GlobalData.NetworkStats.MiningAddress;

        ((DaemonViewModel)ViewModelPagesDictionary[SplitViewPages.Daemon]).Connections = GlobalData.Connections;

        if(GlobalData.NetworkStats.MinerStatus.Equals(StatusMiner.Mining))
        {
            // Mining so disable number of threads and show Stop Mining
            if (((DaemonViewModel)ViewModelPagesDictionary[SplitViewPages.Daemon]).StartStopMining.Equals(StatusMiner.StartMining))
            {
                ((DaemonViewModel)ViewModelPagesDictionary[SplitViewPages.Daemon]).StartStopMining = StatusMiner.StopMining;                
            }
            if(((DaemonViewModel)ViewModelPagesDictionary[SplitViewPages.Daemon]).IsNumThreadsEnabled)
            {
                ((DaemonViewModel)ViewModelPagesDictionary[SplitViewPages.Daemon]).IsNumThreadsEnabled = false;
            }
        }
        else
        {
            // Not mining so enable number of threads and set Start Mining
            if (((DaemonViewModel)ViewModelPagesDictionary[SplitViewPages.Daemon]).StartStopMining.Equals(StatusMiner.StopMining))
            {
                ((DaemonViewModel)ViewModelPagesDictionary[SplitViewPages.Daemon]).StartStopMining = StatusMiner.StartMining;
            }
            if (!((DaemonViewModel)ViewModelPagesDictionary[SplitViewPages.Daemon]).IsNumThreadsEnabled)
            {
                ((DaemonViewModel)ViewModelPagesDictionary[SplitViewPages.Daemon]).IsNumThreadsEnabled = true;
            }
        }

        // Status Bar
        DaemonStatus = "Connections: " + GlobalData.NetworkStats.ConnectionsOut + "(out) + " + GlobalData.NetworkStats.ConnectionsIn + "(in)" + GlobalData.NetworkStats.StatusSync;        
        DaemonVersion = "Version: " + GlobalData.NetworkStats.Version;
    }

    public void UpdateWalletView()
    {       
        if(GlobalData.IsWalletOpen)
        {
            if (!((WalletViewModel)ViewModelPagesDictionary[SplitViewPages.Wallet]).TotalCoins.Equals(GlobalData.WalletStats.TotalBalanceLocked.ToString()))
            {
                ((WalletViewModel)ViewModelPagesDictionary[SplitViewPages.Wallet]).TotalCoins = GlobalData.WalletStats.TotalBalanceLocked.ToString();
            }

            if (!((WalletViewModel)ViewModelPagesDictionary[SplitViewPages.Wallet]).UnlockedCoins.Equals(GlobalData.WalletStats.TotalBalanceUnlocked.ToString()))
            {
                ((WalletViewModel)ViewModelPagesDictionary[SplitViewPages.Wallet]).UnlockedCoins = GlobalData.WalletStats.TotalBalanceUnlocked.ToString();
            }

            string totalLockedLabel = "Total " + GlobalData.AppSettings.Wallet[GlobalData.AppSettings.ActiveCoin].DisplayUnits + ":";
            if (!((WalletViewModel)ViewModelPagesDictionary[SplitViewPages.Wallet]).TotalLockedLabel.Equals(totalLockedLabel))
            {
                ((WalletViewModel)ViewModelPagesDictionary[SplitViewPages.Wallet]).TotalLockedLabel = totalLockedLabel;
            }

            string totalUnlockedLabel = "Unlocked " + GlobalData.AppSettings.Wallet[GlobalData.AppSettings.ActiveCoin].DisplayUnits + ":";
            if (!((WalletViewModel)ViewModelPagesDictionary[SplitViewPages.Wallet]).TotalUnlockedLabel.Equals(totalUnlockedLabel))
            {
                ((WalletViewModel)ViewModelPagesDictionary[SplitViewPages.Wallet]).TotalUnlockedLabel = totalUnlockedLabel;
            }

            if (((WalletViewModel)ViewModelPagesDictionary[SplitViewPages.Wallet]).WalletAddresses.Count == 0)
            {
                ((WalletViewModel)ViewModelPagesDictionary[SplitViewPages.Wallet]).WalletAddresses = GlobalData.WalletStats.Subaddresses.Values.ToList<Account>();
            }
            else
            {
                // TODO: ((WalletViewModel)ViewModelPagesDictionary[SplitViewPages.Wallet]).WalletAddresses RaiseAndSetIfChanged not firing on updates. Find another way


                // Trying to avoid loop within a loop
                List<Account> deleteWallets = [];
                HashSet<uint> checkedIndexes = [];

                foreach (Account wallet in ((WalletViewModel)ViewModelPagesDictionary[SplitViewPages.Wallet]).WalletAddresses)
                {
                    checkedIndexes.Add(wallet.Index);

                    if (GlobalData.WalletStats.Subaddresses.ContainsKey(wallet.Index))
                    {
                        // Update, only if value changed
                        if (!wallet.Label.Equals(GlobalData.WalletStats.Subaddresses[wallet.Index].Label))
                        {
                            wallet.Label = GlobalData.WalletStats.Subaddresses[wallet.Index].Label;
                        }
                        if (!wallet.AddressShort.Equals(GlobalData.WalletStats.Subaddresses[wallet.Index].AddressShort))
                        {
                            wallet.AddressShort = GlobalData.WalletStats.Subaddresses[wallet.Index].AddressShort;
                        }
                        if (wallet.BalanceLocked != (GlobalData.WalletStats.Subaddresses[wallet.Index].BalanceLocked))
                        {
                            wallet.BalanceLocked = GlobalData.WalletStats.Subaddresses[wallet.Index].BalanceLocked;
                        }
                        if (wallet.BalanceUnlocked != (GlobalData.WalletStats.Subaddresses[wallet.Index].BalanceUnlocked))
                        {
                            wallet.BalanceUnlocked = GlobalData.WalletStats.Subaddresses[wallet.Index].BalanceUnlocked;
                        }
                    }
                    else
                    {
                        // Wallets to remove
                        deleteWallets.Add(wallet);
                    }
                }

                foreach (Account wallet in deleteWallets)
                {
                    ((WalletViewModel)ViewModelPagesDictionary[SplitViewPages.Wallet]).WalletAddresses.Remove(wallet);
                }

                foreach (uint index in GlobalData.WalletStats.Subaddresses.Keys)
                {
                    if (!checkedIndexes.Contains(index))
                    {
                        // Need to add new wallet
                        ((WalletViewModel)ViewModelPagesDictionary[SplitViewPages.Wallet]).WalletAddresses.Add(GlobalData.WalletStats.Subaddresses[index]);
                    }
                }
            }

            ((WalletViewModel)ViewModelPagesDictionary[SplitViewPages.Wallet]).OpenCloseWallet = StatusWallet.CloseWallet;

            // Status Bar
            string statusBarMessage = GlobalData.OpenedWalletName + " | Account(s): " + GlobalData.WalletStats.Subaddresses.Count + " | Balance: " + GlobalData.WalletStats.TotalBalanceLocked + " " + GlobalData.AppSettings.Wallet[GlobalData.AppSettings.ActiveCoin].DisplayUnits + " | Height: " + GlobalData.WalletHeight;
            if (WalletStatus != statusBarMessage)
            {
                WalletStatus = statusBarMessage;
            }
        }
        else
        {
            // If wallet is closed/was closed by the user, clear fields
            if (!string.IsNullOrEmpty(((WalletViewModel)ViewModelPagesDictionary[SplitViewPages.Wallet]).TotalCoins))
            {
                ((WalletViewModel)ViewModelPagesDictionary[SplitViewPages.Wallet]).TotalCoins = string.Empty;
            }

            if (!string.IsNullOrEmpty(((WalletViewModel)ViewModelPagesDictionary[SplitViewPages.Wallet]).UnlockedCoins))
            {
                ((WalletViewModel)ViewModelPagesDictionary[SplitViewPages.Wallet]).UnlockedCoins = string.Empty;
            }

            string totalLockedLabel = "Total " + GlobalData.AppSettings.Wallet[GlobalData.AppSettings.ActiveCoin].DisplayUnits + ":";
            if (!((WalletViewModel)ViewModelPagesDictionary[SplitViewPages.Wallet]).TotalLockedLabel.Equals(totalLockedLabel))
            {
                ((WalletViewModel)ViewModelPagesDictionary[SplitViewPages.Wallet]).TotalLockedLabel = totalLockedLabel;
            }

            string totalUnlockedLabel = "Unlocked " + GlobalData.AppSettings.Wallet[GlobalData.AppSettings.ActiveCoin].DisplayUnits + ":";
            if (!((WalletViewModel)ViewModelPagesDictionary[SplitViewPages.Wallet]).TotalUnlockedLabel.Equals(totalUnlockedLabel))
            {
                ((WalletViewModel)ViewModelPagesDictionary[SplitViewPages.Wallet]).TotalUnlockedLabel = totalUnlockedLabel;
            }

            if (((WalletViewModel)ViewModelPagesDictionary[SplitViewPages.Wallet]).WalletAddresses.Count != 0)
            {
                ((WalletViewModel)ViewModelPagesDictionary[SplitViewPages.Wallet]).WalletAddresses = new List<Account>();
            }

            ((WalletViewModel)ViewModelPagesDictionary[SplitViewPages.Wallet]).OpenCloseWallet = StatusWallet.OpenWallet;

            // Status Bar
            if (WalletStatus != GlobalData.WalletClosedMessage)
            {
                WalletStatus = GlobalData.WalletClosedMessage;
            }
        }       
    }

    public void UpdateTransfersView()
    {
        int newTransfersCount = 0;

        if(GlobalData.IsWalletOpen)
        {
            if (((TransfersViewModel)ViewModelPagesDictionary[SplitViewPages.Transfers]).Transactions.Count == 0)
            {
                ((TransfersViewModel)ViewModelPagesDictionary[SplitViewPages.Transfers]).Transactions = GlobalData.TransfersStats.Transactions.Values.ToList<Transfer>().OrderByDescending(tr => tr.Height).ToList();
                // Need to reset so we do not process transactions every second until next update
                GlobalData.TransfersStats.Transactions = [];

                if(((TransfersViewModel)ViewModelPagesDictionary[SplitViewPages.Transfers]).Transactions.Count > 0)
                {
                    // TODO: This will also save after initial open BUT it will cover restoring wallet
                    newTransfersCount = ((TransfersViewModel)ViewModelPagesDictionary[SplitViewPages.Transfers]).Transactions.Count;
                }
            }
            else
            {
                List<Transfer> newTransfers = [];

                foreach (string newTransferKey in GlobalData.TransfersStats.Transactions.Keys)
                {
                    // Check if transaction already exists in datagrid and add it to the top if it does not
                    // TODO: If for some reason, you keep getting a lot of transactions (that might already be in memory), this is very expensive and can block updating process
                    if (!((TransfersViewModel)ViewModelPagesDictionary[SplitViewPages.Transfers]).Transactions.Any(transfer => transfer.TransactionId + "_" + transfer.Type == newTransferKey))
                    {
                        newTransfers.Add(GlobalData.TransfersStats.Transactions[newTransferKey]);
                    }
                }
                // Need to reset so we do not process transactions every second until next update
                GlobalData.TransfersStats.Transactions = [];

                if (newTransfers.Count > 0)
                {
                    // TODO: If you scroll in the datagrid, new rows show up, otherwise, they do not. Figure out how to force refresh
                    ((TransfersViewModel)ViewModelPagesDictionary[SplitViewPages.Transfers]).Transactions.InsertRange(0, newTransfers.OrderByDescending(tr => tr.Height).ToList());
                    newTransfersCount = newTransfers.Count;
                }
            }

            if(!GlobalData.IsWalletJustOpened && newTransfersCount > 0)
            {
                Logger.LogDebug("Main.UTV", "Auto-saving wallet. New transfers count: " + newTransfersCount);
                SaveWallet();
            }
        }
        else
        {
            // If wallet is closed/was closed by the user, clear fields
            if (((TransfersViewModel)ViewModelPagesDictionary[SplitViewPages.Transfers]).Transactions.Count != 0)
            {
                ((TransfersViewModel)ViewModelPagesDictionary[SplitViewPages.Transfers]).Transactions = [];
            }
        }
    }

    // TODO: Move this somewhere else.
    #region Master Process Methods    
    public void StartMasterUpdateProcess()
    {
        try
        {
            Logger.LogDebug("Main.SMUP", "Start Master Update Process");

            if (_masterUpdateTimer == null)
            {
                _masterUpdateTimer = new System.Timers.Timer();
                _masterUpdateTimer.Interval = _masterTimerInterval;
                _masterUpdateTimer.Elapsed += (s, e) => MasterUpdateProcess();
                _masterUpdateTimer.Start();

                Logger.LogDebug("Main.SMUP", "Master timer running every " + _masterTimerInterval / 1000 + " seconds. Update every " + (_masterTimerInterval / 1000) * GlobalData.AppSettings.TimerIntervalMultiplier + " seconds.");
            }
        }
        catch (Exception ex)
        {
            Logger.LogException("Main.SMUP", ex);
        }
    }

    public void MasterUpdateProcess()
    {
        try
        {
            if (_masterUpdateTimer != null)
            {
                _masterUpdateTimer.Stop();
            }

            if(_currentCoin != GlobalData.AppSettings.ActiveCoin)
            {
                UpdateLogos();
            }

            // If kill master process is issued at any point, skip everything else and do not restrt master timer            
            if (_cliToolsRunningLastCheck.AddSeconds(10) < DateTime.Now)
            {
                _cliToolsRunningLastCheck = DateTime.Now;

                if (!_killMasterProcess)
                {
                    KeepDaemonRunning();

                    // TODO: Maybe do it another way. Might be good enough though
                    if(GlobalData.NetworkStats.YourHeight > 0
                        && GlobalData.NetworkStats.YourHeight == GlobalData.NetworkStats.NetHeight
                        && GlobalData.NetworkStats.MinerStatus == StatusMiner.Inactive
                        && GlobalData.AppSettings.Daemon[GlobalData.AppSettings.ActiveCoin].AutoStartMining
                        && !GlobalData.IsManualStopMining)
                    {
                        Logger.LogDebug("Main.MUP", "Auto starting mining.");
                        GlobalMethods.StartMiningAsync(GlobalData.AppSettings.Daemon[GlobalData.AppSettings.ActiveCoin].MiningThreads);
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
                DaemonStatus = "Client tools not found. Attempting to download...";
            }

            // Update UI
            if (!_killMasterProcess && _cliToolsFound)
            {
                if (_masterTimerCount % GlobalData.AppSettings.TimerIntervalMultiplier == 0)
                {
                    DaemonUiUpdate();
                }
            }

            if (!_killMasterProcess && _isInitialDaemonConnectionSuccess && GlobalData.IsWalletOpen)
            {
                if(GlobalData.IsWalletJustOpened)
                {                    
                    WalletUiUpdate();
                    TransfersUiUpdate();
                    SetWalletHeight();
                }
                else if(_masterTimerCount % (GlobalData.AppSettings.TimerIntervalMultiplier * 3) == 0)
                {
                    // Update wallet every 3rd call because you do not need to do it more often
                    WalletUiUpdate();
                    TransfersUiUpdate();
                    SetWalletHeight();
                }                
            }

            // Actual UI update. If walet was closed, it will clear things
            UpdateWalletView();
            UpdateTransfersView();

            if (GlobalData.IsWalletJustOpened)
            {
                // Will use this to auto-save wallet so need to reset it at the end
                GlobalData.IsWalletJustOpened = false;
            }

            if(GlobalData.IsWalletOpen & _masterTimerCount % 300 == 0)
            {
                // Auto save wallet every 5 min
                Logger.LogDebug("Main.MUP", "Auto saving wallet: " + GlobalData.OpenedWalletName);
                SaveWallet();
            }
        }
        catch (Exception ex)
        {
            Logger.LogException("Main.MUP", ex);
        }
        finally
        {
            // Restart timer
            if (_masterUpdateTimer == null)
            {
                Logger.LogError("Main.MUP", "Timer is NULL. Recreating. Why?");
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
    public void UpdateLogos()
    {
        // Update coins icons when coin changes

        // TODO: I don't like how those icons work. Chage it!

        CoinIcon = GlobalMethods.GetLogo();
        ((DaemonViewModel)ViewModelPagesDictionary[SplitViewPages.Daemon]).CoinIcon = GlobalMethods.GetLogo();
        ((WalletViewModel)ViewModelPagesDictionary[SplitViewPages.Wallet]).CoinIcon = GlobalMethods.GetLogo();
        ((TransfersViewModel)ViewModelPagesDictionary[SplitViewPages.Transfers]).CoinIcon = GlobalMethods.GetLogo();
        ((DaemonSetupViewModel)ViewModelPagesDictionary[SplitViewPages.DaemonSetup]).CoinIcon = GlobalMethods.GetLogo();
        ((WalletSetupViewModel)ViewModelPagesDictionary[SplitViewPages.WalletSetup]).CoinIcon = GlobalMethods.GetLogo();
        ((SettingsViewModel)ViewModelPagesDictionary[SplitViewPages.Settings]).CoinIcon = GlobalMethods.GetLogo();

        _currentCoin = GlobalData.AppSettings.ActiveCoin;
    }

    private static void KeepDaemonRunning()
    {
        try
        {
            bool forceRestart = false;
            if (_lastDaemonResponseTime.AddMinutes(5) < DateTime.Now)
            {
                // Daemon not responding. Kill and restart
                forceRestart = true;
                _lastDaemonResponseTime = DateTime.Now;
                Logger.LogDebug("Main.KDR", "No response from daemon since: " + _lastDaemonResponseTime.ToLongTimeString() + " . Forcing restart...");
            }

            if (!ProcessManager.IsRunning(GlobalData.DaemonProcessName, out Process? process) || forceRestart)
            {
                if (GlobalMethods.DirectoryContainsCliTools(GlobalData.CliToolsDir))
                {
                    DaemonProcess.ForceClose();
                    Logger.LogDebug("Main.KDR", "Starting daemon process");
                    ProcessManager.StartExternalProcess(GlobalMethods.GetDaemonProcess(), DaemonProcess.GenerateOptions(GlobalData.AppSettings.Daemon[GlobalData.AppSettings.ActiveCoin]));
                    _isInitialDaemonConnectionSuccess = false;
                    _cliToolsFound = true;
                }
                else
                {
                    Logger.LogInfo("Main.KDR", "CLI tools not found");
                    _cliToolsFound = false;
                }
            }
        }
        catch (Exception ex)
        {
            Logger.LogException("Main.KDR", ex);
        }
    }

    private static void KeepWalletProcessRunning()
    {
        try
        {
            if (!ProcessManager.IsRunning(GlobalData.WalletProcessName, out Process? process))
            {
                if (GlobalMethods.DirectoryContainsCliTools(GlobalData.CliToolsDir))
                {
                    WalletProcess.ForceClose();
                    Logger.LogDebug("Main.KWPR", "Starting wallet process");
                    ProcessManager.StartExternalProcess(GlobalMethods.GetRpcWalletProcess(), WalletProcess.GenerateOptions(GlobalData.AppSettings.Wallet[GlobalData.AppSettings.ActiveCoin], GlobalData.AppSettings.Daemon[GlobalData.AppSettings.ActiveCoin].Rpc));
                }
                else
                {
                    Logger.LogDebug("Main.KWPR", "CLI tools not found");
                }
            }
        }
        catch (Exception ex)
        {
            Logger.LogException("Main.KWPR", ex);
        }
    }

    public async void DaemonUiUpdate()
    {
        try
        {
            if (!_isInitialDaemonConnectionSuccess)
            {
                GlobalData.NetworkStats = new()
                {
                    StatusSync = " | Trying to establish connection with daemon..."
                };
                GlobalData.Connections = [];
                UpdateMainView();
            }

            if(GlobalData.IsDaemonRestarting)
            {
                GlobalData.NetworkStats = new()
                {
                    StatusSync = " | Restarting daemon..."
                };
                GlobalData.Connections = [];
                UpdateMainView();
            }

            GetInfoResponse infoRes = await GlobalData.DaemonService.GetInfo(GlobalData.AppSettings.Daemon[GlobalData.AppSettings.ActiveCoin].Rpc, new GetInfoRequest());

            if (!infoRes.Error.IsError)
            {
                _lastDaemonResponseTime = DateTime.Now;
                if (_isInitialDaemonConnectionSuccess == false)
                {
                    // This will be used to get rid of establishing connection message and to StartWalletUiUpdate 
                    _isInitialDaemonConnectionSuccess = true;
                }

                if(GlobalData.IsDaemonRestarting)
                {
                    GlobalData.IsDaemonRestarting = false;
                }

                GlobalData.NetworkStats.NetHeight = (infoRes.TargetHeight > infoRes.Height ? infoRes.TargetHeight : infoRes.Height);
                GlobalData.NetworkStats.YourHeight = infoRes.Height;

                if(((infoRes.Difficulty / GlobalData.AppSettings.Daemon[GlobalData.AppSettings.ActiveCoin].BlockSeconds) / 1000000000.0d) > 1)
                {
                    GlobalData.NetworkStats.NetHash = Math.Round(((infoRes.Difficulty / GlobalData.AppSettings.Daemon[GlobalData.AppSettings.ActiveCoin].BlockSeconds) / 1000000000.0d), 2) + " GH/s";
                }
                else if(((infoRes.Difficulty / GlobalData.AppSettings.Daemon[GlobalData.AppSettings.ActiveCoin].BlockSeconds) / 1000000.0d) > 1)
                {
                    GlobalData.NetworkStats.NetHash = Math.Round(((infoRes.Difficulty / GlobalData.AppSettings.Daemon[GlobalData.AppSettings.ActiveCoin].BlockSeconds) / 1000000.0d), 2) + " MH/s";
                }
                else
                {
                    GlobalData.NetworkStats.NetHash = Math.Round(((infoRes.Difficulty / GlobalData.AppSettings.Daemon[GlobalData.AppSettings.ActiveCoin].BlockSeconds) / 1000.0d), 2) + " KH/s";
                }                

                DateTime miningStartTime = infoRes.StartTime;
                GlobalData.NetworkStats.RunTime = (DateTime.Now.ToUniversalTime() - miningStartTime).ToString(@"%d\.hh\:mm\:ss");

                GlobalData.NetworkStats.ConnectionsIn = infoRes.ConnectionCountIn;
                GlobalData.NetworkStats.ConnectionsOut = infoRes.ConnectionCountOut;
                GlobalData.NetworkStats.Difficulty = infoRes.Difficulty;

                GlobalData.NetworkStats.Version = infoRes.Version;
                GlobalData.NetworkStats.StatusSync = "";
                if (infoRes.TargetHeight != 0 && infoRes.Height < infoRes.TargetHeight)
                {
                    GlobalData.NetworkStats.StatusSync += " | Sync (Height " + infoRes.Height + " of " + infoRes.TargetHeight + ")";
                }
                else
                {
                    GlobalData.NetworkStats.StatusSync += " | Sync OK | Status " + infoRes.Status;
                }


                //Logger.LogDebug("Main.DUU", "GetInfo Response Height: " + infoRes.height);


                MiningStatusResponse miningRes = await GlobalData.DaemonService.MiningStatus(GlobalData.AppSettings.Daemon[GlobalData.AppSettings.ActiveCoin].Rpc, new MiningStatusRequest());
                if (miningRes.IsActive)
                {
                    GlobalData.NetworkStats.MinerStatus = StatusMiner.Mining;
                    GlobalData.NetworkStats.MiningAddress = GlobalMethods.GetShorterString(miningRes.Address, 12);

                    if (miningRes.Speed > 1000)
                    {
                        GlobalData.NetworkStats.YourHash = miningRes.Speed / 1000.0d + " KH/s";
                    }
                    else
                    {
                        GlobalData.NetworkStats.YourHash = miningRes.Speed + " h/s";
                    }

                    if (GlobalData.NetworkStats.Difficulty > 0)
                    {
                        double blockMinutes = ((GlobalData.NetworkStats.Difficulty / GlobalData.AppSettings.Daemon[GlobalData.AppSettings.ActiveCoin].BlockSeconds) / miningRes.Speed);

                        if ((blockMinutes / 1440d) > 1)
                        {
                            GlobalData.NetworkStats.BlockTime = String.Format("{0:F1}", Math.Round(blockMinutes, 1) / 1440d) + " days (est)";
                        }
                        else if ((blockMinutes / 60.0d) > 1)
                        {
                            GlobalData.NetworkStats.BlockTime = String.Format("{0:F1}", Math.Round(blockMinutes, 1) / 60.0d) + " hours (est)";
                        }
                        else
                        {
                            GlobalData.NetworkStats.BlockTime = String.Format("{0:F0}", Math.Round(blockMinutes, 0)) + " minutes (est)";
                        }
                    }
                }
                else
                {
                    GlobalData.NetworkStats.MinerStatus = StatusMiner.Inactive;
                    GlobalData.NetworkStats.MiningAddress = "None";
                    GlobalData.NetworkStats.YourHash = "0 h/s";
                    GlobalData.NetworkStats.BlockTime = "∞";
                }


                GetConnectionsResponse connectResp = await GlobalData.DaemonService.GetConnections(GlobalData.AppSettings.Daemon[GlobalData.AppSettings.ActiveCoin].Rpc, new GetConnectionsRequest());

                if(!connectResp.Error.IsError)
                {
                    // TODO: For now just recreate items in grid. Consider, removing disconnected and adding new ones to List instead.
                    GlobalData.Connections = connectResp.Connections;
                    foreach (Connection connection in GlobalData.Connections)
                    {
                        connection.InOutIcon = connection.IsIncoming ? _inImage : _outImage;
                    }

                    UpdateMainView();
                }
            }
        }
        catch (Exception ex)
        {
            Logger.LogException("Main.DUU", ex);
        }
    }

    public async void WalletUiUpdate()
    {
        try
        {
            // Get accounts for Wallets view
            GetAccountsResponse resGetAccounts = await GlobalData.WalletService.GetAccounts(GlobalData.AppSettings.Wallet[GlobalData.AppSettings.ActiveCoin].Rpc, new GetAccountsRequest());

            if(resGetAccounts.Error.IsError)
            {
                Logger.LogError("Main.WUU", "GetAccounts Error Code: " + resGetAccounts.Error.Code + ", Message: " + resGetAccounts.Error.Message);
            }
            else
            {
                GlobalData.WalletStats.TotalBalanceLocked = resGetAccounts.BalanceLocked;
                GlobalData.WalletStats.TotalBalanceUnlocked = resGetAccounts.BalanceUnlocked;

                GlobalData.WalletStats.Subaddresses = [];

                // TODO: Set icon inside CallAsync method above?
                foreach (Account account in resGetAccounts.SubAccounts)
                {
                    account.WalletIcon = _walletImage;

                    GlobalData.WalletStats.Subaddresses.Add(account.Index, account);
                }
            }            
        }
        catch (Exception ex)
        {
            Logger.LogException("Main.WUU", ex);
        }
    }

    public async void TransfersUiUpdate()
    {
        try
        {
            if (_isTransfersUpdateComplete)
            {
                // Wait for one GetTransactions to finish before calling next one
                _isTransfersUpdateComplete = false;

                // TODO: Kepp track of latest transaction height and set min_hight
                // Scanning from height 0 can take 15+ seconds and is CPU intensive

                // Get transactions for Transfers view
                GetTransfersRequest reqTransfers = new GetTransfersRequest();
                reqTransfers.IncludeIn = true;
                reqTransfers.IncludeOut = true;
                reqTransfers.IncludePending = true;
                reqTransfers.IncludeFailed = false;
                reqTransfers.IncludePool = false;
                reqTransfers.IsFilterByHeight = true;
                reqTransfers.MinHeight = GlobalData.NewestTransactionHeight;
                reqTransfers.AccountIndex = 0;
                reqTransfers.SubaddressIndices = [];
                reqTransfers.IsAllAccounts = true;

                // TODO: Remove stopwatch when no longer needed
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();
                GetTransfersResponse resTransfers = await GlobalData.WalletService.GetTransfers(GlobalData.AppSettings.Wallet[GlobalData.AppSettings.ActiveCoin].Rpc, reqTransfers);
                stopwatch.Stop();
                
                if (resTransfers.Error.IsError)
                {
                    Logger.LogError("Main.WUU", "GetTransfers Error Code: " + resTransfers.Error.Code + ", Message: " + resTransfers.Error.Message);
                }
                else
                {
                    GlobalData.TransfersStats.Transactions = [];

                    foreach (Transfer transfer in resTransfers.Transfers)
                    {
                        if (transfer.Type.Equals("in"))
                        {
                            transfer.Icon = _inImage;
                        }
                        else if (transfer.Type.Equals("out"))
                        {
                            transfer.Icon = _outImage;
                        }
                        else if (transfer.Type.Equals("block"))
                        {
                            transfer.Icon = _blockImage;
                        }

                        GlobalData.TransfersStats.Transactions.Add(transfer.TransactionId + "_" + transfer.Type, transfer);

                        // TODO: Maybe do this in a different way
                        if (transfer.Height > GlobalData.NewestTransactionHeight)
                        {
                            GlobalData.NewestTransactionHeight = transfer.Height;
                        }
                    }
                }

                _isTransfersUpdateComplete = true;
            }
        }
        catch (Exception ex)
        {
            Logger.LogException("Main.TUU", ex);
        }
    }

    public async void SetWalletHeight()
    {
        try
        {
            GetHeightResponse resGetHeight = await GlobalData.WalletService.GetHeight(GlobalData.AppSettings.Wallet[GlobalData.AppSettings.ActiveCoin].Rpc, new GetHeightRequest());

            if (resGetHeight.Error.IsError)
            {
                Logger.LogError("Main.WUU", "GetTransfers Error Code: " + resGetHeight.Error.Code + ", Message: " + resGetHeight.Error.Message);
            }
            else
            {
                GlobalData.WalletHeight = resGetHeight.Height;
            }            
        }
        catch (Exception ex)
        {
            Logger.LogException("Main.TUU", ex);
        }
    }
    #endregion // Master Process Methods

    public async void SaveWallet()
    {
        try
        {
            SaveWalletResponse resStore = await GlobalData.WalletService.SaveWallet(GlobalData.AppSettings.Wallet[GlobalData.AppSettings.ActiveCoin].Rpc, new SaveWalletRequest());

            if (resStore.Error.IsError)
            {
                Logger.LogError("Main.SW", "Error saving wallet: " + GlobalData.OpenedWalletName + ". Code: " + resStore.Error.Code + ", Message: " + resStore.Error.Message);
            }
            else
            {
                Logger.LogDebug("Main.SW", "Wallet " + GlobalData.OpenedWalletName + " saved!");
            }
        }
        catch (Exception ex)
        {
            Logger.LogException("Main.SW", ex);
        }
    }
}