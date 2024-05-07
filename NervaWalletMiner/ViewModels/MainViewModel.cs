using Avalonia.Controls;
using Avalonia.Controls.Selection;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using NervaWalletMiner.Helpers;
using NervaWalletMiner.Objects;
using NervaWalletMiner.Objects.Constants;
using NervaWalletMiner.Objects.DataGrid;
using NervaWalletMiner.Rpc;
using NervaWalletMiner.Rpc.Daemon;
using NervaWalletMiner.Rpc.Wallet;
using NervaWalletMiner.Rpc.Wallet.Requests;
using NervaWalletMiner.Rpc.Wallet.Responses;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Input;
using static NervaWalletMiner.Rpc.Daemon.MiningStatus;

namespace NervaWalletMiner.ViewModels;

public class MainViewModel : ViewModelBase
{
    public static System.Timers.Timer? _masterUpdateTimer;
    public const int _masterTimerInterval = 1000;
    public static int _masterTimerCount = 0;
    public static bool _killMasterProcess = false;

    public static DateTime _cliToolsRunningLastCheck = DateTime.MinValue;
    public static DateTime _lastDaemonResponseTime = DateTime.Now;
    public static bool _isInitialDaemonConnectionSuccess = false;

    public static Dictionary<string, ViewModelBase> ViewModelPagesDictionary = new();

    public static readonly Bitmap _inImage = new Bitmap(AssetLoader.Open(new Uri("avares://NervaWalletMiner/Assets/transfer_in.png")));
    public static readonly Bitmap _outImage = new Bitmap(AssetLoader.Open(new Uri("avares://NervaWalletMiner/Assets/transfer_out.png")));
    public static readonly Bitmap _blockImage = new Bitmap(AssetLoader.Open(new Uri("avares://NervaWalletMiner/Assets/transfer_block.png")));
    public static readonly Bitmap _walletImage = new Bitmap(AssetLoader.Open(new Uri("avares://NervaWalletMiner/Assets/wallet.png")));

    private bool? _isPaneOpen = false;
    private ViewModelBase _CurrentPage;
    public SelectionModel<ListBoxItem> Selection { get; }
    public ICommand TriggerPaneCommand { get; }
    

    public MainViewModel()
    {
        // Set up split view pages
        ViewModelPagesDictionary.Add(SplitViewPages.Home, new HomeViewModel());
        ViewModelPagesDictionary.Add(SplitViewPages.Wallet, new WalletViewModel());
        ViewModelPagesDictionary.Add(SplitViewPages.Transfers, new TransfersViewModel());
        ViewModelPagesDictionary.Add(SplitViewPages.Settings, new SettingsViewModel());

        TriggerPaneCommand = ReactiveCommand.Create(TriggerPane);

        _CurrentPage = ViewModelPagesDictionary[SplitViewPages.Home];

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

    private string _DaemonVersion = "";
    public string DaemonVersion
    {
        get => _DaemonVersion;
        set => this.RaiseAndSetIfChanged(ref _DaemonVersion, value);
    }

    private string _WalletStatus = "Wallet offline - see Wallet tab to open";
    public string WalletStatus
    {
        get => _WalletStatus;
        set => this.RaiseAndSetIfChanged(ref _WalletStatus, value);
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
            case SplitViewPages.Settings:
                CurrentPage = ViewModelPagesDictionary[SplitViewPages.Settings];
                break;
            default:
                CurrentPage = ViewModelPagesDictionary[SplitViewPages.Home];
                break;
        }
    }

    private void TriggerPane()
    {
        IsPaneOpen = !IsPaneOpen;
    }

    public void UpdateMainView()
    {
        // Daemon (HomeView)
        ((HomeViewModel)ViewModelPagesDictionary[SplitViewPages.Home]).NetHeight = GlobalData.NetworkStats.NetHeight.ToString();
        ((HomeViewModel)ViewModelPagesDictionary[SplitViewPages.Home]).YourHeight = GlobalData.NetworkStats.YourHeight.ToString();
        ((HomeViewModel)ViewModelPagesDictionary[SplitViewPages.Home]).NetHash = GlobalData.NetworkStats.NetHash;
        ((HomeViewModel)ViewModelPagesDictionary[SplitViewPages.Home]).RunTime = GlobalData.NetworkStats.RunTime;

        ((HomeViewModel)ViewModelPagesDictionary[SplitViewPages.Home]).MinerMessage = GlobalData.NetworkStats.MinerStatus;
        ((HomeViewModel)ViewModelPagesDictionary[SplitViewPages.Home]).YourHash = GlobalData.NetworkStats.YourHash;
        ((HomeViewModel)ViewModelPagesDictionary[SplitViewPages.Home]).BlockTime = GlobalData.NetworkStats.BlockTime;
        ((HomeViewModel)ViewModelPagesDictionary[SplitViewPages.Home]).MiningAddress = GlobalData.NetworkStats.MiningAddress;

        ((HomeViewModel)ViewModelPagesDictionary[SplitViewPages.Home]).Connections = GlobalData.Connections;

        if(GlobalData.NetworkStats.MinerStatus.Equals(StatusMiner.Mining))
        {
            // Mining so disable number of threads and show Stop Mining
            if (((HomeViewModel)ViewModelPagesDictionary[SplitViewPages.Home]).StartStopMining.Equals(StatusMiner.StartMining))
            {
                ((HomeViewModel)ViewModelPagesDictionary[SplitViewPages.Home]).StartStopMining = StatusMiner.StopMining;                
            }
            if(((HomeViewModel)ViewModelPagesDictionary[SplitViewPages.Home]).IsNumThreadsEnabled)
            {
                ((HomeViewModel)ViewModelPagesDictionary[SplitViewPages.Home]).IsNumThreadsEnabled = false;
            }
        }
        else
        {
            // Not mining so enable number of threads and set Start Mining
            if (((HomeViewModel)ViewModelPagesDictionary[SplitViewPages.Home]).StartStopMining.Equals(StatusMiner.StopMining))
            {
                ((HomeViewModel)ViewModelPagesDictionary[SplitViewPages.Home]).StartStopMining = StatusMiner.StartMining;
            }
            if (!((HomeViewModel)ViewModelPagesDictionary[SplitViewPages.Home]).IsNumThreadsEnabled)
            {
                ((HomeViewModel)ViewModelPagesDictionary[SplitViewPages.Home]).IsNumThreadsEnabled = true;
            }
        }

        // Status Bar
        DaemonStatus = "Connections: " + GlobalData.NetworkStats.ConnectionsOut + "(out) + " + GlobalData.NetworkStats.ConnectionsIn + "(in) " + GlobalData.NetworkStats.StatusSync;
        DaemonVersion = "Version: " + GlobalData.NetworkStats.Version;
    }

    public void UpdateWalletView()
    {       
        // Wallet
        if(!((WalletViewModel)ViewModelPagesDictionary[SplitViewPages.Wallet]).TotalXnv.Equals(GlobalData.WalletStats.TotalBalanceLocked.ToString()))
        {
            ((WalletViewModel)ViewModelPagesDictionary[SplitViewPages.Wallet]).TotalXnv = GlobalData.WalletStats.TotalBalanceLocked.ToString();
        }
        
        if(!((WalletViewModel)ViewModelPagesDictionary[SplitViewPages.Wallet]).UnlockedXnv.Equals(GlobalData.WalletStats.TotalBalanceUnlocked.ToString()))
        {
            ((WalletViewModel)ViewModelPagesDictionary[SplitViewPages.Wallet]).UnlockedXnv = GlobalData.WalletStats.TotalBalanceUnlocked.ToString();
        }        

        if(((WalletViewModel)ViewModelPagesDictionary[SplitViewPages.Wallet]).WalletAddresses.Count == 0)
        {
            ((WalletViewModel)ViewModelPagesDictionary[SplitViewPages.Wallet]).WalletAddresses = GlobalData.WalletStats.Subaddresses.Values.ToList<Wallet>();
        }
        else
        {
            // TODO: ((WalletViewModel)ViewModelPagesDictionary[SplitViewPages.Wallet]).WalletAddresses RaiseAndSetIfChanged not firing on updates. Find another way


            // Trying to avoid loop within a loop
            List<Wallet> deleteWallets = [];
            HashSet<int> checkedIndexes = [];

            foreach (Wallet wallet in ((WalletViewModel)ViewModelPagesDictionary[SplitViewPages.Wallet]).WalletAddresses)
            {
                checkedIndexes.Add(wallet.Index);

                if (GlobalData.WalletStats.Subaddresses.ContainsKey(wallet.Index))
                {
                    // Update, only if value changed
                    if (!wallet.Label.Equals(GlobalData.WalletStats.Subaddresses[wallet.Index].Label))
                    {
                        wallet.Label = GlobalData.WalletStats.Subaddresses[wallet.Index].Label;
                    }
                    if (!wallet.Address.Equals(GlobalData.WalletStats.Subaddresses[wallet.Index].Address))
                    {
                        wallet.Address = GlobalData.WalletStats.Subaddresses[wallet.Index].Address;
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

            foreach (Wallet wallet in deleteWallets)
            {
                ((WalletViewModel)ViewModelPagesDictionary[SplitViewPages.Wallet]).WalletAddresses.Remove(wallet);
            }

            foreach (int index in GlobalData.WalletStats.Subaddresses.Keys)
            {
                if (!checkedIndexes.Contains(index))
                {
                    // Need to add new wallet
                    ((WalletViewModel)ViewModelPagesDictionary[SplitViewPages.Wallet]).WalletAddresses.Add(GlobalData.WalletStats.Subaddresses[index]);
                }
            }
        }       

        if(GlobalData.IsWalletOpen)
        {
            ((WalletViewModel)ViewModelPagesDictionary[SplitViewPages.Wallet]).OpenCloseWallet = StatusWallet.CloseWallet;
        }
        else
        {
            ((WalletViewModel)ViewModelPagesDictionary[SplitViewPages.Wallet]).OpenCloseWallet = StatusWallet.OpenWallet;
        }

        // Status Bar
        WalletStatus = "Account(s): " + GlobalData.WalletStats.Subaddresses.Count + " | Balance: " + GlobalData.WalletStats.TotalBalanceLocked + " XNV";        
    }

    public void UpdateTransfersView()
    {
        if (((TransfersViewModel)ViewModelPagesDictionary[SplitViewPages.Transfers]).Transactions.Count == 0)
        {
            ((TransfersViewModel)ViewModelPagesDictionary[SplitViewPages.Transfers]).Transactions = GlobalData.TransfersStats.Transactions.Values.ToList<Transfer>().OrderByDescending(tr=>tr.Height).ToList();
        }
        else
        {
            List<Transfer> newTransfers = [];

            foreach(string transactionId in GlobalData.TransfersStats.Transactions.Keys)
            {
                // Check if transaction already exists in datagrid and add it to the top if it does not
                if(!((TransfersViewModel)ViewModelPagesDictionary[SplitViewPages.Transfers]).Transactions.Any(transfer => transfer.TransactionId == transactionId))
                {
                    newTransfers.Add(GlobalData.TransfersStats.Transactions[transactionId]);                    
                }
            }

            if (newTransfers.Count > 0)
            {
                // TODO: If you scroll in the datagrid, new rows show up, otherwise, they do not. Figure out how to force refresh
                ((TransfersViewModel)ViewModelPagesDictionary[SplitViewPages.Transfers]).Transactions.InsertRange(0, newTransfers.OrderByDescending(tr => tr.Height).ToList());
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

                Logger.LogDebug("Main.SMUP", "Master timer running every " + _masterTimerInterval / 1000 + " seconds. Update every " + (_masterTimerInterval / 1000) * GlobalData.ApplicationSettings.Misc.TimerIntervalMultiplier + " seconds.");
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

            // If kill master process is issued at any point, skip everything else and do not restrt master timer            
            if (_cliToolsRunningLastCheck.AddSeconds(10) < DateTime.Now)
            {
                _cliToolsRunningLastCheck = DateTime.Now;

                if (!_killMasterProcess)
                {
                    KeepDaemonRunning();
                }


                if (!_killMasterProcess)
                {
                    KeepWalletProcessRunning();
                }
            }


            // Update UI
            if (!_killMasterProcess)
            {
                if (_masterTimerCount % GlobalData.ApplicationSettings.Misc.TimerIntervalMultiplier == 0)
                {
                    DaemonUiUpdate();
                }
            }

            if (!_killMasterProcess && _isInitialDaemonConnectionSuccess && GlobalData.IsWalletOpen)
            {
                if(GlobalData.IsWalletJustOpened)
                {
                    GlobalData.IsWalletJustOpened = false;
                    WalletUiUpdate();
                }
                else if(_masterTimerCount % (GlobalData.ApplicationSettings.Misc.TimerIntervalMultiplier * 3) == 0)
                {
                    // Update wallet every 3rd call because you do not need to do it more often
                    WalletUiUpdate();
                }                
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

    private static void KeepDaemonRunning()
    {
        try
        {
            bool forceRestart = false;
            if (_lastDaemonResponseTime.AddMinutes(5) < DateTime.Now)
            {
                // Daemon not responding.  Kill and restart
                forceRestart = true;
                _lastDaemonResponseTime = DateTime.Now;
                Logger.LogDebug("Main.KDR", "No response from daemon since: " + _lastDaemonResponseTime.ToLongTimeString() + " . Forcing restart...");
            }

            if (!ProcessManager.IsRunning(FileNames.NERVA_DAEMON, out Process? process) || forceRestart)
            {
                if (FileNames.DirectoryContainsCliTools(GlobalData.CliToolsDir))
                {
                    DaemonProcess.ForceClose();
                    Logger.LogDebug("Main.KDR", "Starting daemon process");
                    ProcessManager.StartExternalProcess(GlobalMethods.GetDaemonPath(), DaemonProcess.GenerateCommandLine());
                    _isInitialDaemonConnectionSuccess = false;
                }
                else
                {
                    Logger.LogDebug("Main.KDR", "CLI tools not found");
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
            if (!ProcessManager.IsRunning(FileNames.NERVA_WALLET_RPC, out Process? process))
            {
                if (FileNames.DirectoryContainsCliTools(GlobalData.CliToolsDir))
                {
                    WalletProcess.ForceClose();
                    Logger.LogDebug("Main.KWPR", "Starting wallet process");
                    ProcessManager.StartExternalProcess(GlobalMethods.GetRpcWalletPath(), WalletProcess.GenerateCommandLine());
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
                GlobalData.NetworkStats.StatusSync = "Trying to establish connection with daemon...";
            }

            GetInfoResponse infoRes = await GetInfo.CallServiceAsync();

            if (!string.IsNullOrEmpty(infoRes.version))
            {
                _lastDaemonResponseTime = DateTime.Now;
                if (_isInitialDaemonConnectionSuccess == false)
                {
                    // This will be used to get rid of establishing connection message and to StartWalletUiUpdate 
                    _isInitialDaemonConnectionSuccess = true;
                }

                GlobalData.NetworkStats.NetHeight = (infoRes.target_height > infoRes.height ? infoRes.target_height : infoRes.height);
                GlobalData.NetworkStats.YourHeight = infoRes.height;
                GlobalData.NetworkStats.NetHash = Math.Round(((infoRes.difficulty / 60.0d) / 1000.0d), 2) + " kH/s";

                DateTime miningStartTime = GlobalMethods.UnixTimeStampToDateTime(infoRes.start_time);
                GlobalData.NetworkStats.RunTime = (DateTime.Now.ToUniversalTime() - miningStartTime).ToString(@"%d\.hh\:mm\:ss");

                GlobalData.NetworkStats.ConnectionsIn = infoRes.incoming_connections_count;
                GlobalData.NetworkStats.ConnectionsOut = infoRes.outgoing_connections_count;
                GlobalData.NetworkStats.Difficulty = infoRes.difficulty;

                GlobalData.NetworkStats.Version = infoRes.version;
                GlobalData.NetworkStats.StatusSync = "";
                if (infoRes.target_height != 0 && infoRes.height < infoRes.target_height)
                {
                    GlobalData.NetworkStats.StatusSync += " | Synchronizing (Height " + infoRes.height + " of " + infoRes.target_height + ")";
                }
                else
                {
                    GlobalData.NetworkStats.StatusSync += "  |  Sync OK";
                }

                GlobalData.NetworkStats.StatusSync += "  |  Status " + infoRes.status;


                //Logger.LogDebug("Main.DUU", "GetInfo Response Height: " + infoRes.height);


                MiningStatusResponse miningRes = await Rpc.Daemon.MiningStatus.CallServiceAsync();
                if (miningRes.active)
                {
                    GlobalData.NetworkStats.MinerStatus = StatusMiner.Mining;
                    GlobalData.NetworkStats.MiningAddress = GlobalMethods.GetShorterString(miningRes.address, 12);

                    if (miningRes.speed > 1000)
                    {
                        GlobalData.NetworkStats.YourHash = miningRes.speed / 1000.0d + " kH/s";
                    }
                    else
                    {
                        GlobalData.NetworkStats.YourHash = miningRes.speed + " h/s";
                    }

                    if (GlobalData.NetworkStats.Difficulty > 0)
                    {
                        double blockMinutes = ((GlobalData.NetworkStats.Difficulty / 60.0d) / miningRes.speed);

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


                List<GetConnectionsResponse> connectResp = await GetConnections.CallServiceAsync();

                // TODO: For now just recreate items in grid. Consider, removing disconnected and adding new ones to List instead.
                GlobalData.Connections = [];

                foreach (GetConnectionsResponse connection in connectResp)
                {
                    GlobalData.Connections.Add(new Connection
                    {
                        Address = connection.address,
                        Height = connection.height,
                        LiveTime = TimeSpan.FromSeconds(connection.live_time).ToString(@"hh\:mm\:ss"),
                        State = connection.state,
                        IsIncoming = connection.incoming,
                        InOutIcon = connection.incoming ? _inImage : _outImage
                    });
                }

                UpdateMainView();
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
            GetAccountsResponse resGetAccounts = await GetAccounts.CallAsync(GlobalData.ApplicationSettings.Wallet.Rpc);

            if(resGetAccounts.Error.IsError)
            {
                Logger.LogError("Main.WUU", "GetAccounts Error Code: " + resGetAccounts.Error.Code + ", Message: " + resGetAccounts.Error.Message);
            }
            else
            {
                GlobalData.WalletStats.TotalBalanceLocked = GlobalMethods.XnvFromAtomicUnits(resGetAccounts.total_balance, 4);
                GlobalData.WalletStats.TotalBalanceUnlocked = GlobalMethods.XnvFromAtomicUnits(resGetAccounts.total_unlocked_balance, 4);

                GlobalData.WalletStats.Subaddresses = [];

                foreach (Account account in resGetAccounts.subaddress_accounts)
                {
                    Wallet newWallet = new()
                    {
                        Index = account.account_index,
                        BalanceLocked = GlobalMethods.XnvFromAtomicUnits(account.balance, 1),
                        BalanceUnlocked = GlobalMethods.XnvFromAtomicUnits(account.unlocked_balance, 1),
                        Address = GlobalMethods.GetShorterString(account.base_address, 12),
                        Label = account.label,
                        WalletIcon = _walletImage
                    };

                    GlobalData.WalletStats.Subaddresses.Add(newWallet.Index, newWallet);
                }

                UpdateWalletView();
            }


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

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            GetTransfersResponse resTransfers = await GetTransfers.CallAsync(GlobalData.ApplicationSettings.Wallet.Rpc, reqTransfers);
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
                    if(transfer.Type.Equals("in"))
                    {
                        transfer.Icon = _inImage;
                    }
                    else if(transfer.Type.Equals("out"))
                    {
                        transfer.Icon = _outImage;
                    }
                    else if(transfer.Type.Equals("block"))
                    {
                        transfer.Icon = _blockImage;
                    }

                    if(GlobalData.TransfersStats.Transactions.ContainsKey(transfer.TransactionId))
                    {
                        // TODO: Figure out why you have duplicates. Maybe TransactionId is not unique on its own
                    }
                    else
                    {
                        GlobalData.TransfersStats.Transactions.Add(transfer.TransactionId, transfer);

                        // TODO: Maybe do this in a different way
                        if(transfer.Height > GlobalData.NewestTransactionHeight)
                        {
                            GlobalData.NewestTransactionHeight = transfer.Height;
                        }
                    }
                }

                UpdateTransfersView();
            }
        }
        catch (Exception ex)
        {
            Logger.LogException("Main.WUU", ex);
        }
    }
    #endregion // Master Process Methods
}