using Avalonia.Controls.Selection;
using Avalonia.Controls;
using NervaOneWalletMiner.Objects.Constants;
using NervaOneWalletMiner.Objects.DataGrid;
using NervaOneWalletMiner.Rpc.Daemon.Requests;
using NervaOneWalletMiner.Rpc.Daemon.Responses;
using NervaOneWalletMiner.Rpc.Wallet.Requests;
using NervaOneWalletMiner.Rpc.Wallet.Responses;
using NervaOneWalletMiner.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Avalonia.Media.Imaging;
using Avalonia.Platform;

namespace NervaOneWalletMiner.Helpers
{
    public static class UIManager
    {
        public static bool _isTransfersUpdateComplete = true;
        public static bool _askedToQuickSync = false;

        public static readonly Bitmap _inImage = new Bitmap(AssetLoader.Open(new Uri("avares://NervaOneWalletMiner/Assets/transfer_in.png")));
        public static readonly Bitmap _outImage = new Bitmap(AssetLoader.Open(new Uri("avares://NervaOneWalletMiner/Assets/transfer_out.png")));
        public static readonly Bitmap _blockImage = new Bitmap(AssetLoader.Open(new Uri("avares://NervaOneWalletMiner/Assets/transfer_block.png")));

        private static ViewModelBase _mainView = new();


        // TODO: I don't like this. Come up with a different way
        public static void SetMainView(ViewModelBase mainView)
        {
            _mainView = mainView;
        }

        public static void SetUpFirstRun()
        {
            // One page to rule them all
            GlobalData.ViewModelPages = new()
            {
                { SplitViewPages.Daemon, new PickCoinViewModel() },
                { SplitViewPages.Wallet, new PickCoinViewModel() },
                { SplitViewPages.Transfers, new PickCoinViewModel() },
                { SplitViewPages.AddressBook, new PickCoinViewModel() },
                { SplitViewPages.DaemonSetup, new PickCoinViewModel() },
                { SplitViewPages.WalletSetup, new PickCoinViewModel() },
                { SplitViewPages.Settings, new PickCoinViewModel() },
                { SplitViewPages.About, new PickCoinViewModel() },

                { SplitViewPages.MainView, _mainView }
            };

            ((MainViewModel)GlobalData.ViewModelPages[SplitViewPages.MainView]).CurrentPage = GlobalData.ViewModelPages[SplitViewPages.Daemon];
        }

        public static void SetUpPages()
        {
            // Set up split view pages
            GlobalData.ViewModelPages = new()
            {
                { SplitViewPages.Daemon, new DaemonViewModel() },
                { SplitViewPages.Wallet, new WalletViewModel() },
                { SplitViewPages.Transfers, new TransfersViewModel() },
                { SplitViewPages.AddressBook, new AddressBookViewModel() },
                { SplitViewPages.DaemonSetup, new DaemonSetupViewModel() },
                { SplitViewPages.WalletSetup, new WalletSetupViewModel() },
                { SplitViewPages.Settings, new SettingsViewModel() },
                { SplitViewPages.About, new AboutViewModel() },

                { SplitViewPages.MainView, _mainView }
            };

            ((MainViewModel)GlobalData.ViewModelPages[SplitViewPages.MainView]).CurrentPage = GlobalData.ViewModelPages[SplitViewPages.Daemon];

            MasterProcess.StartMasterUpdateProcess();

            UpdateMainView();            
        }

        public static void SelectionChanged(object? sender, SelectionModelSelectionChangedEventArgs e)
        {
            // TODO: Figoure out better way of doing this
            switch (((ListBoxItem)e.SelectedItems[0]!).Name)
            {
                case SplitViewPages.Wallet:
                    ((MainViewModel)GlobalData.ViewModelPages[SplitViewPages.MainView]).CurrentPage = GlobalData.ViewModelPages[SplitViewPages.Wallet];
                    break;
                case SplitViewPages.Transfers:
                    ((MainViewModel)GlobalData.ViewModelPages[SplitViewPages.MainView]).CurrentPage = GlobalData.ViewModelPages[SplitViewPages.Transfers];
                    break;
                case SplitViewPages.AddressBook:
                    ((MainViewModel)GlobalData.ViewModelPages[SplitViewPages.MainView]).CurrentPage = GlobalData.ViewModelPages[SplitViewPages.AddressBook];
                    break;
                case SplitViewPages.DaemonSetup:
                    ((MainViewModel)GlobalData.ViewModelPages[SplitViewPages.MainView]).CurrentPage = GlobalData.ViewModelPages[SplitViewPages.DaemonSetup];
                    break;
                case SplitViewPages.WalletSetup:
                    ((MainViewModel)GlobalData.ViewModelPages[SplitViewPages.MainView]).CurrentPage = GlobalData.ViewModelPages[SplitViewPages.WalletSetup];
                    break;
                case SplitViewPages.Settings:
                    ((MainViewModel)GlobalData.ViewModelPages[SplitViewPages.MainView]).CurrentPage = GlobalData.ViewModelPages[SplitViewPages.Settings];
                    break;
                case SplitViewPages.About:
                    ((MainViewModel)GlobalData.ViewModelPages[SplitViewPages.MainView]).CurrentPage = GlobalData.ViewModelPages[SplitViewPages.About];
                    break;
                default:
                    ((MainViewModel)GlobalData.ViewModelPages[SplitViewPages.MainView]).CurrentPage = GlobalData.ViewModelPages[SplitViewPages.Daemon];
                    break;
            }
        }

        public static void UpdateDaemonStatus(string message)
        {
            if (GlobalData.ViewModelPages.ContainsKey(SplitViewPages.MainView))
            {
                ((MainViewModel)GlobalData.ViewModelPages[SplitViewPages.MainView]).DaemonStatus = message;
            }            
        }

        public static void UpdateDaemonVersion(string version)
        {
            if (GlobalData.ViewModelPages.ContainsKey(SplitViewPages.MainView))
            {
                ((MainViewModel)GlobalData.ViewModelPages[SplitViewPages.MainView]).DaemonVersion = version;
            }
        }

        public static Bitmap GetCoinIcon()
        {
            if (GlobalData.ViewModelPages.ContainsKey(SplitViewPages.MainView))
            {
                return ((MainViewModel)GlobalData.ViewModelPages[SplitViewPages.MainView]).CoinIcon;
            }
            else
            {
                return GlobalMethods.GetLogo();
            }
        }

        public static void UpdateCoinIcon(Bitmap icon)
        {
            if (GlobalData.ViewModelPages.ContainsKey(SplitViewPages.MainView))
            {
                ((MainViewModel)GlobalData.ViewModelPages[SplitViewPages.MainView]).CoinIcon = icon;
            }
        }

        public static void UpdateMainView()
        {
            // Daemon View
            ((DaemonViewModel)GlobalData.ViewModelPages[SplitViewPages.Daemon]).NetHeight = GlobalData.NetworkStats.NetHeight.ToString();
            ((DaemonViewModel)GlobalData.ViewModelPages[SplitViewPages.Daemon]).YourHeight = GlobalData.NetworkStats.YourHeight.ToString();
            ((DaemonViewModel)GlobalData.ViewModelPages[SplitViewPages.Daemon]).NetHash = GlobalData.NetworkStats.NetHash;
            ((DaemonViewModel)GlobalData.ViewModelPages[SplitViewPages.Daemon]).RunTime = GlobalData.NetworkStats.RunTime;

            ((DaemonViewModel)GlobalData.ViewModelPages[SplitViewPages.Daemon]).MinerMessage = GlobalData.NetworkStats.MinerStatus;
            ((DaemonViewModel)GlobalData.ViewModelPages[SplitViewPages.Daemon]).YourHash = GlobalData.NetworkStats.YourHash;
            ((DaemonViewModel)GlobalData.ViewModelPages[SplitViewPages.Daemon]).BlockTime = GlobalData.NetworkStats.BlockTime;
            ((DaemonViewModel)GlobalData.ViewModelPages[SplitViewPages.Daemon]).MiningAddress = GlobalData.NetworkStats.MiningAddress;

            ((DaemonViewModel)GlobalData.ViewModelPages[SplitViewPages.Daemon]).Connections = GlobalData.Connections;

            if (GlobalData.NetworkStats.MinerStatus.Equals(StatusMiner.Mining))
            {
                // Mining so disable number of threads and show Stop Mining
                if (((DaemonViewModel)GlobalData.ViewModelPages[SplitViewPages.Daemon]).StartStopMining.Equals(StatusMiner.StartMining))
                {
                    ((DaemonViewModel)GlobalData.ViewModelPages[SplitViewPages.Daemon]).StartStopMining = StatusMiner.StopMining;
                }
                if (((DaemonViewModel)GlobalData.ViewModelPages[SplitViewPages.Daemon]).IsNumThreadsEnabled)
                {
                    ((DaemonViewModel)GlobalData.ViewModelPages[SplitViewPages.Daemon]).IsNumThreadsEnabled = false;
                }
            }
            else
            {
                // Not mining so enable number of threads and set Start Mining
                if (((DaemonViewModel)GlobalData.ViewModelPages[SplitViewPages.Daemon]).StartStopMining.Equals(StatusMiner.StopMining))
                {
                    ((DaemonViewModel)GlobalData.ViewModelPages[SplitViewPages.Daemon]).StartStopMining = StatusMiner.StartMining;
                }
                if (!((DaemonViewModel)GlobalData.ViewModelPages[SplitViewPages.Daemon]).IsNumThreadsEnabled)
                {
                    ((DaemonViewModel)GlobalData.ViewModelPages[SplitViewPages.Daemon]).IsNumThreadsEnabled = true;
                }
            }

            // Status Bar
            UpdateDaemonStatus("Connections: " + GlobalData.NetworkStats.ConnectionsOut + "(out) + " + GlobalData.NetworkStats.ConnectionsIn + "(in)" + GlobalData.NetworkStats.StatusSync);
            UpdateDaemonVersion("Version: " + GlobalData.NetworkStats.Version);
        }

        public static void UpdateWalletView()
        {
            if (GlobalData.IsWalletOpen)
            {
                if (!((WalletViewModel)GlobalData.ViewModelPages[SplitViewPages.Wallet]).TotalCoins.Equals(GlobalData.WalletStats.TotalBalanceLocked.ToString()))
                {
                    ((WalletViewModel)GlobalData.ViewModelPages[SplitViewPages.Wallet]).TotalCoins = GlobalData.WalletStats.TotalBalanceLocked.ToString();
                }

                if (!((WalletViewModel)GlobalData.ViewModelPages[SplitViewPages.Wallet]).UnlockedCoins.Equals(GlobalData.WalletStats.TotalBalanceUnlocked.ToString()))
                {
                    ((WalletViewModel)GlobalData.ViewModelPages[SplitViewPages.Wallet]).UnlockedCoins = GlobalData.WalletStats.TotalBalanceUnlocked.ToString();
                }

                string totalLockedLabel = "Total " + GlobalData.AppSettings.Wallet[GlobalData.AppSettings.ActiveCoin].DisplayUnits + ":";
                if (!((WalletViewModel)GlobalData.ViewModelPages[SplitViewPages.Wallet]).TotalLockedLabel.Equals(totalLockedLabel))
                {
                    ((WalletViewModel)GlobalData.ViewModelPages[SplitViewPages.Wallet]).TotalLockedLabel = totalLockedLabel;
                }

                string totalUnlockedLabel = "Unlocked " + GlobalData.AppSettings.Wallet[GlobalData.AppSettings.ActiveCoin].DisplayUnits + ":";
                if (!((WalletViewModel)GlobalData.ViewModelPages[SplitViewPages.Wallet]).TotalUnlockedLabel.Equals(totalUnlockedLabel))
                {
                    ((WalletViewModel)GlobalData.ViewModelPages[SplitViewPages.Wallet]).TotalUnlockedLabel = totalUnlockedLabel;
                }

                if (((WalletViewModel)GlobalData.ViewModelPages[SplitViewPages.Wallet]).WalletAddresses.Count == 0 && GlobalData.WalletStats.Subaddresses.Values.Count > 0)
                {
                    ((WalletViewModel)GlobalData.ViewModelPages[SplitViewPages.Wallet]).WalletAddresses = GlobalData.WalletStats.Subaddresses.Values.ToList<Account>();
                }
                else
                {
                    // TODO: ((WalletViewModel)GlobalData.ViewModelPagesDictionary[SplitViewPages.Wallet]).WalletAddresses RaiseAndSetIfChanged not firing on updates. Find another way


                    // Trying to avoid loop within a loop
                    List<Account> deleteWallets = [];
                    HashSet<uint> checkedIndexes = [];

                    foreach (Account wallet in ((WalletViewModel)GlobalData.ViewModelPages[SplitViewPages.Wallet]).WalletAddresses)
                    {
                        checkedIndexes.Add(wallet.Index);

                        if (GlobalData.WalletStats.Subaddresses.ContainsKey(wallet.Index))
                        {
                            // Update, only if value changed
                            // GlobalData.WalletStats.Subaddresses get cleared in asynchronouse call so sometimes even though you check if key exists, it will not exist anymore
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
                        ((WalletViewModel)GlobalData.ViewModelPages[SplitViewPages.Wallet]).WalletAddresses.Remove(wallet);
                    }

                    foreach (uint index in GlobalData.WalletStats.Subaddresses.Keys)
                    {
                        if (!checkedIndexes.Contains(index))
                        {
                            // Need to add new wallet
                            ((WalletViewModel)GlobalData.ViewModelPages[SplitViewPages.Wallet]).WalletAddresses.Add(GlobalData.WalletStats.Subaddresses[index]);
                        }
                    }
                }

                ((WalletViewModel)GlobalData.ViewModelPages[SplitViewPages.Wallet]).OpenCloseWallet = StatusWallet.CloseWallet;

                // Status Bar
                string statusBarMessage = GlobalData.OpenedWalletName + " | Account(s): " + GlobalData.WalletStats.Subaddresses.Count + " | Balance: " + GlobalData.WalletStats.TotalBalanceLocked + " " + GlobalData.AppSettings.Wallet[GlobalData.AppSettings.ActiveCoin].DisplayUnits + " | Height: " + GlobalData.WalletHeight;
                if (((MainViewModel)GlobalData.ViewModelPages[SplitViewPages.MainView]).WalletStatus != statusBarMessage)
                {
                    ((MainViewModel)GlobalData.ViewModelPages[SplitViewPages.MainView]).WalletStatus = statusBarMessage;
                }
            }
            else
            {
                // If wallet is closed/was closed by the user, clear fields
                if (!string.IsNullOrEmpty(((WalletViewModel)GlobalData.ViewModelPages[SplitViewPages.Wallet]).TotalCoins))
                {
                    ((WalletViewModel)GlobalData.ViewModelPages[SplitViewPages.Wallet]).TotalCoins = string.Empty;
                }

                if (!string.IsNullOrEmpty(((WalletViewModel)GlobalData.ViewModelPages[SplitViewPages.Wallet]).UnlockedCoins))
                {
                    ((WalletViewModel)GlobalData.ViewModelPages[SplitViewPages.Wallet]).UnlockedCoins = string.Empty;
                }

                string totalLockedLabel = "Total " + GlobalData.AppSettings.Wallet[GlobalData.AppSettings.ActiveCoin].DisplayUnits + ":";
                if (!((WalletViewModel)GlobalData.ViewModelPages[SplitViewPages.Wallet]).TotalLockedLabel.Equals(totalLockedLabel))
                {
                    ((WalletViewModel)GlobalData.ViewModelPages[SplitViewPages.Wallet]).TotalLockedLabel = totalLockedLabel;
                }

                string totalUnlockedLabel = "Unlocked " + GlobalData.AppSettings.Wallet[GlobalData.AppSettings.ActiveCoin].DisplayUnits + ":";
                if (!((WalletViewModel)GlobalData.ViewModelPages[SplitViewPages.Wallet]).TotalUnlockedLabel.Equals(totalUnlockedLabel))
                {
                    ((WalletViewModel)GlobalData.ViewModelPages[SplitViewPages.Wallet]).TotalUnlockedLabel = totalUnlockedLabel;
                }

                if (((WalletViewModel)GlobalData.ViewModelPages[SplitViewPages.Wallet]).WalletAddresses.Count != 0)
                {
                    ((WalletViewModel)GlobalData.ViewModelPages[SplitViewPages.Wallet]).WalletAddresses = new List<Account>();
                }

                ((WalletViewModel)GlobalData.ViewModelPages[SplitViewPages.Wallet]).OpenCloseWallet = StatusWallet.OpenWallet;

                // Status Bar
                if (((MainViewModel)GlobalData.ViewModelPages[SplitViewPages.MainView]).WalletStatus != GlobalData.WalletClosedMessage)
                {
                    ((MainViewModel)GlobalData.ViewModelPages[SplitViewPages.MainView]).WalletStatus = GlobalData.WalletClosedMessage;
                }
            }
        }

        public static void UpdateTransfersView()
        {
            int newTransfersCount = 0;

            if (GlobalData.IsWalletOpen)
            {
                if (((TransfersViewModel)GlobalData.ViewModelPages[SplitViewPages.Transfers]).Transactions.Count == 0)
                {
                    ((TransfersViewModel)GlobalData.ViewModelPages[SplitViewPages.Transfers]).Transactions = GlobalData.TransfersStats.Transactions.Values.ToList<Transfer>().OrderByDescending(tr => tr.Height).ToList();
                    // Need to reset so we do not process transactions every second until next update
                    GlobalData.TransfersStats.Transactions = [];

                    if (((TransfersViewModel)GlobalData.ViewModelPages[SplitViewPages.Transfers]).Transactions.Count > 0)
                    {
                        // TODO: This will also save after initial open BUT it will cover restoring wallet
                        newTransfersCount = ((TransfersViewModel)GlobalData.ViewModelPages[SplitViewPages.Transfers]).Transactions.Count;
                    }
                }
                else
                {
                    List<Transfer> newTransfers = [];

                    foreach (string newTransferKey in GlobalData.TransfersStats.Transactions.Keys)
                    {
                        // Check if transaction already exists in datagrid and add it to the top if it does not
                        // TODO: If for some reason, you keep getting a lot of transactions (that might already be in memory), this is very expensive and can block updating process
                        if (!((TransfersViewModel)GlobalData.ViewModelPages[SplitViewPages.Transfers]).Transactions.Any(transfer => transfer.TransactionId + "_" + transfer.Type == newTransferKey))
                        {
                            newTransfers.Add(GlobalData.TransfersStats.Transactions[newTransferKey]);
                        }
                    }
                    // Need to reset so we do not process transactions every second until next update
                    GlobalData.TransfersStats.Transactions = [];

                    if (newTransfers.Count > 0)
                    {
                        // TODO: If you scroll in the datagrid, new rows show up, otherwise, they do not. Figure out how to force refresh
                        ((TransfersViewModel)GlobalData.ViewModelPages[SplitViewPages.Transfers]).Transactions.InsertRange(0, newTransfers.OrderByDescending(tr => tr.Height).ToList());
                        newTransfersCount = newTransfers.Count;
                    }
                }

                if (!GlobalData.IsWalletJustOpened && newTransfersCount > 0)
                {
                    Logger.LogDebug("UIM.UPTV", "Auto-saving wallet. New transfers count: " + newTransfersCount);
                    GlobalMethods.SaveWallet();
                }
            }
            else
            {
                // If wallet is closed/was closed by the user, clear fields
                if (((TransfersViewModel)GlobalData.ViewModelPages[SplitViewPages.Transfers]).Transactions.Count != 0)
                {
                    ((TransfersViewModel)GlobalData.ViewModelPages[SplitViewPages.Transfers]).Transactions = [];
                }
            }
        }

        public static async void DaemonUiUpdate()
        {
            try
            {
                if (!GlobalData.IsInitialDaemonConnectionSuccess)
                {
                    GlobalData.NetworkStats = new()
                    {
                        StatusSync = " | Trying to establish connection with daemon..."
                    };
                    GlobalData.Connections = [];
                    UpdateMainView();
                }

                if (GlobalData.IsDaemonRestarting)
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
                    GlobalData.LastDaemonResponseTime = DateTime.Now;
                    if (GlobalData.IsInitialDaemonConnectionSuccess == false)
                    {
                        // This will be used to get rid of establishing connection message and to StartWalletUiUpdate 
                        GlobalData.IsInitialDaemonConnectionSuccess = true;
                    }

                    if (GlobalData.IsDaemonRestarting)
                    {
                        GlobalData.IsDaemonRestarting = false;
                    }

                    GlobalData.NetworkStats.NetHeight = (infoRes.TargetHeight > infoRes.Height ? infoRes.TargetHeight : infoRes.Height);
                    GlobalData.NetworkStats.YourHeight = infoRes.Height;

                    if ((infoRes.NetworkHashRate / 1000000000000000.0d) > 1)
                    {
                        GlobalData.NetworkStats.NetHash = Math.Round((infoRes.NetworkHashRate / 1000000000000000.0d), 2) + " PH/s";
                    }
                    else if ((infoRes.NetworkHashRate / 1000000000000.0d) > 1)
                    {
                        GlobalData.NetworkStats.NetHash = Math.Round((infoRes.NetworkHashRate / 1000000000000.0d), 2) + " TH/s";
                    }
                    else if ((infoRes.NetworkHashRate / 1000000000.0d) > 1)
                    {
                        GlobalData.NetworkStats.NetHash = Math.Round((infoRes.NetworkHashRate / 1000000000.0d), 2) + " GH/s";
                    }
                    else if ((infoRes.NetworkHashRate / 1000000.0d) > 1)
                    {
                        GlobalData.NetworkStats.NetHash = Math.Round((infoRes.NetworkHashRate / 1000000.0d), 2) + " MH/s";
                    }
                    else if ((infoRes.NetworkHashRate / 1000.0d) > 1)
                    {
                        GlobalData.NetworkStats.NetHash = Math.Round((infoRes.NetworkHashRate / 1000.0d), 2) + " KH/s";
                    }
                    else
                    {
                        GlobalData.NetworkStats.NetHash = infoRes.NetworkHashRate + " H/s";
                    }

                    DateTime miningStartTime = infoRes.StartTime;
                    GlobalData.NetworkStats.RunTime = (DateTime.Now.ToUniversalTime() - miningStartTime).ToString(@"%d\.hh\:mm\:ss");

                    GlobalData.NetworkStats.ConnectionsIn = infoRes.ConnectionCountIn;
                    GlobalData.NetworkStats.ConnectionsOut = infoRes.ConnectionCountOut;

                    GlobalData.NetworkStats.Version = infoRes.Version;
                    GlobalData.NetworkStats.StatusSync = "";
                    if (infoRes.TargetHeight != 0 && infoRes.Height < infoRes.TargetHeight)
                    {
                        GlobalData.NetworkStats.StatusSync += " | Sync (Height " + infoRes.Height + " of " + infoRes.TargetHeight + ")";

                        // See if user wants to use QuickSync if they're far behind
                        if (!_askedToQuickSync && !string.IsNullOrEmpty(GlobalData.CoinSettings[GlobalData.AppSettings.ActiveCoin].QuickSyncUrl))
                        {
                            _askedToQuickSync = true;
                            double percentSynced = infoRes.Height / Convert.ToDouble(infoRes.TargetHeight);

                            if (percentSynced < 0.8)
                            {
                                ((MainViewModel)GlobalData.ViewModelPages[SplitViewPages.MainView]).AskIfSyncWithQuickSync(percentSynced);
                            }
                        }                        
                    }
                    else
                    {
                        GlobalData.NetworkStats.StatusSync += " | Sync OK | Status " + infoRes.Status;
                    }


                    //Logger.LogDebug("UIM.DUUT", "GetInfo Response Height: " + infoRes.height);


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

                        if (miningRes.Speed > 0)
                        {
                            double blockMinutes = (double)infoRes.NetworkHashRate / miningRes.Speed;

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

                    if (!connectResp.Error.IsError)
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
                Logger.LogException("UIM.DUUT", ex);
            }
        }

        public static async void TransfersUiUpdate()
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
                        Logger.LogError("UIM.TUUD", "GetTransfers Error Code: " + resTransfers.Error.Code + ", Message: " + resTransfers.Error.Message);
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
                Logger.LogException("UIM.TUUD", ex);
            }
        }
    }
}