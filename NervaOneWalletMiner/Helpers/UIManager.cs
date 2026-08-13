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
using System.Linq;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using System.Collections.ObjectModel;
using Avalonia.Threading;
using System.Threading.Tasks;

namespace NervaOneWalletMiner.Helpers
{
    public static class UIManager
    {
        public static bool _isTransfersUpdateComplete = true;
        public static bool _askedToQuickSync = false;

        public static readonly Bitmap _walletImage = new Bitmap(AssetLoader.Open(new Uri("avares://" + GlobalData.AppAssemblyName + "/Assets/wallet.png")));
        public static readonly Bitmap _inImage = new Bitmap(AssetLoader.Open(new Uri("avares://" + GlobalData.AppAssemblyName + "/Assets/transfer_in.png")));
        public static readonly Bitmap _outImage = new Bitmap(AssetLoader.Open(new Uri("avares://" + GlobalData.AppAssemblyName + "/Assets/transfer_out.png")));
        public static readonly Bitmap _blockImage = new Bitmap(AssetLoader.Open(new Uri("avares://" + GlobalData.AppAssemblyName + "/Assets/transfer_block.png")));
        public static readonly Bitmap _pendingImage = new Bitmap(AssetLoader.Open(new Uri("avares://" + GlobalData.AppAssemblyName + "/Assets/transfer_pending.png")));

        private static ViewModelBase _mainView = new();

        // Wallet and Transfers empty states both need to know if any wallets exist on disk. Master timer
        // runs every few seconds so directory is scanned periodically, and right away when wallet closes
        private static readonly int _walletFilesScanSeconds = 10;
        private static DateTime _lastWalletFilesScan = DateTime.MinValue;
        private static volatile bool _isWalletFilesScanNeeded = true;
        private static volatile int _walletFilesCount = 0;


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
                { SplitViewPages.CoinSetup, new CoinSetupViewModel() },
                { SplitViewPages.AddressInfo, new AddressInfoViewModel() },
                { SplitViewPages.CreateWallet, new CreateWalletViewModel() },
                { SplitViewPages.OpenWallet, new OpenWalletViewModel() },
                { SplitViewPages.TransferFunds, new TransferFundsViewModel() },
                { SplitViewPages.TransactionDetails, new TransactionDetailsViewModel() },
                { SplitViewPages.AddressBookEntry, new AddressBookEntryViewModel() },
                { SplitViewPages.RestoreFromKeys, new RestoreFromKeysViewModel() },
                { SplitViewPages.RestoreFromSeed, new RestoreFromSeedViewModel() },
                { SplitViewPages.RestoreFromDumpFile, new RestoreFromDumpFileViewModel() },
                { SplitViewPages.SweepBelow, new SweepBelowViewModel() },
                { SplitViewPages.DisplayKeysSeed, new DisplayKeysSeedViewModel() },
                { SplitViewPages.ViewLogs, new ViewLogsViewModel() },
                { SplitViewPages.PublicNodeSetup, new PublicNodeSetupViewModel() },

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
                { SplitViewPages.CoinSetup, new CoinSetupViewModel() },
                { SplitViewPages.AddressInfo, new AddressInfoViewModel() },
                { SplitViewPages.CreateWallet, new CreateWalletViewModel() },
                { SplitViewPages.OpenWallet, new OpenWalletViewModel() },
                { SplitViewPages.TransferFunds, new TransferFundsViewModel() },
                { SplitViewPages.TransactionDetails, new TransactionDetailsViewModel() },
                { SplitViewPages.AddressBookEntry, new AddressBookEntryViewModel() },
                { SplitViewPages.RestoreFromKeys, new RestoreFromKeysViewModel() },
                { SplitViewPages.RestoreFromSeed, new RestoreFromSeedViewModel() },
                { SplitViewPages.RestoreFromDumpFile, new RestoreFromDumpFileViewModel() },
                { SplitViewPages.SweepBelow, new SweepBelowViewModel() },
                { SplitViewPages.DisplayKeysSeed, new DisplayKeysSeedViewModel() },
                { SplitViewPages.ViewLogs, new ViewLogsViewModel() },
                { SplitViewPages.PublicNodeSetup, new PublicNodeSetupViewModel() },

                { SplitViewPages.MainView, _mainView }
            };

            if (GlobalData.AppSettings.Daemon[GlobalData.AppSettings.ActiveCoin].IsWalletOnly)
            {
                ((MainViewModel)GlobalData.ViewModelPages[SplitViewPages.MainView]).CurrentPage = GlobalData.ViewModelPages[SplitViewPages.Wallet];
            }
            else
            {
                ((MainViewModel)GlobalData.ViewModelPages[SplitViewPages.MainView]).CurrentPage = GlobalData.ViewModelPages[SplitViewPages.Daemon];
            }            

            MasterProcess.StartMasterUpdateProcess();

            UpdateCoinIcon(GlobalMethods.GetLogo());
            UpdateDaemonView();
            UpdateStatusBar();
        }

        public static void SelectionChanged(object? sender, SelectionModelSelectionChangedEventArgs e)
        {
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

        public static void NavigateToCoinSetup()
        {
            GlobalData.ViewModelPages[SplitViewPages.CoinSetup] = new CoinSetupViewModel();
            ((MainViewModel)GlobalData.ViewModelPages[SplitViewPages.MainView]).CurrentPage = GlobalData.ViewModelPages[SplitViewPages.CoinSetup];
        }

        public static void NavigateToAddressInfo(int accountIndex)
        {
            GlobalData.ViewModelPages[SplitViewPages.AddressInfo] = new AddressInfoViewModel(accountIndex);
            ((MainViewModel)GlobalData.ViewModelPages[SplitViewPages.MainView]).CurrentPage = GlobalData.ViewModelPages[SplitViewPages.AddressInfo];
        }

        public static void NavigateToCreateWallet()
        {
            GlobalData.ViewModelPages[SplitViewPages.CreateWallet] = new CreateWalletViewModel();
            ((MainViewModel)GlobalData.ViewModelPages[SplitViewPages.MainView]).CurrentPage = GlobalData.ViewModelPages[SplitViewPages.CreateWallet];
        }

        public static void NavigateToOpenWallet()
        {
            GlobalData.ViewModelPages[SplitViewPages.OpenWallet] = new OpenWalletViewModel();
            ((MainViewModel)GlobalData.ViewModelPages[SplitViewPages.MainView]).CurrentPage = GlobalData.ViewModelPages[SplitViewPages.OpenWallet];
        }

        public static void NavigateToTransferFunds(uint selectedAccountIndex, string toAddress, string paymentId, string returnPage = SplitViewPages.Wallet)
        {
            GlobalData.ViewModelPages[SplitViewPages.TransferFunds] = new TransferFundsViewModel(selectedAccountIndex, toAddress, paymentId, returnPage);
            ((MainViewModel)GlobalData.ViewModelPages[SplitViewPages.MainView]).CurrentPage = GlobalData.ViewModelPages[SplitViewPages.TransferFunds];
        }

        public static void NavigateToTransactionDetails(string transactionId, int accountIndex, decimal amount)
        {
            GlobalData.ViewModelPages[SplitViewPages.TransactionDetails] = new TransactionDetailsViewModel(transactionId, accountIndex, amount);
            ((MainViewModel)GlobalData.ViewModelPages[SplitViewPages.MainView]).CurrentPage = GlobalData.ViewModelPages[SplitViewPages.TransactionDetails];
        }

        public static void NavigateToAddressBook()
        {
            GlobalData.ViewModelPages[SplitViewPages.AddressBook] = new AddressBookViewModel();
            ((MainViewModel)GlobalData.ViewModelPages[SplitViewPages.MainView]).CurrentPage = GlobalData.ViewModelPages[SplitViewPages.AddressBook];
        }

        public static void NavigateToAddressBookEntry(bool isNew, int id = 0, string name = "", string description = "", string address = "", string paymentId = "")
        {
            GlobalData.ViewModelPages[SplitViewPages.AddressBookEntry] = new AddressBookEntryViewModel(isNew, id, name, description, address, paymentId);
            ((MainViewModel)GlobalData.ViewModelPages[SplitViewPages.MainView]).CurrentPage = GlobalData.ViewModelPages[SplitViewPages.AddressBookEntry];
        }

        public static void NavigateToRestoreFromKeys()
        {
            GlobalData.ViewModelPages[SplitViewPages.RestoreFromKeys] = new RestoreFromKeysViewModel();
            ((MainViewModel)GlobalData.ViewModelPages[SplitViewPages.MainView]).CurrentPage = GlobalData.ViewModelPages[SplitViewPages.RestoreFromKeys];
        }

        public static void NavigateToRestoreFromSeed()
        {
            GlobalData.ViewModelPages[SplitViewPages.RestoreFromSeed] = new RestoreFromSeedViewModel();
            ((MainViewModel)GlobalData.ViewModelPages[SplitViewPages.MainView]).CurrentPage = GlobalData.ViewModelPages[SplitViewPages.RestoreFromSeed];
        }

        public static void NavigateToRestoreFromDumpFile()
        {
            GlobalData.ViewModelPages[SplitViewPages.RestoreFromDumpFile] = new RestoreFromDumpFileViewModel();
            ((MainViewModel)GlobalData.ViewModelPages[SplitViewPages.MainView]).CurrentPage = GlobalData.ViewModelPages[SplitViewPages.RestoreFromDumpFile];
        }

        public static void NavigateToSweepBelow()
        {
            GlobalData.ViewModelPages[SplitViewPages.SweepBelow] = new SweepBelowViewModel();
            ((MainViewModel)GlobalData.ViewModelPages[SplitViewPages.MainView]).CurrentPage = GlobalData.ViewModelPages[SplitViewPages.SweepBelow];
        }

        public static void NavigateToDisplayKeysSeed(string message, string returnPage = SplitViewPages.WalletSetup)
        {
            GlobalData.ViewModelPages[SplitViewPages.DisplayKeysSeed] = new DisplayKeysSeedViewModel(message, returnPage);
            ((MainViewModel)GlobalData.ViewModelPages[SplitViewPages.MainView]).CurrentPage = GlobalData.ViewModelPages[SplitViewPages.DisplayKeysSeed];
        }

        public static void NavigateToViewLogs(string initialTab = "app")
        {
            GlobalData.ViewModelPages[SplitViewPages.ViewLogs] = new ViewLogsViewModel { InitialTab = initialTab };
            ((MainViewModel)GlobalData.ViewModelPages[SplitViewPages.MainView]).CurrentPage = GlobalData.ViewModelPages[SplitViewPages.ViewLogs];
        }

        public static void NavigateToPublicNodeSetup()
        {
            GlobalData.ViewModelPages[SplitViewPages.PublicNodeSetup] = new PublicNodeSetupViewModel();
            ((MainViewModel)GlobalData.ViewModelPages[SplitViewPages.MainView]).CurrentPage = GlobalData.ViewModelPages[SplitViewPages.PublicNodeSetup];
        }

        public static void NavigateToPage(string page)
        {
            ((MainViewModel)GlobalData.ViewModelPages[SplitViewPages.MainView]).CurrentPage = GlobalData.ViewModelPages[page];
            ((MainViewModel)GlobalData.ViewModelPages[SplitViewPages.MainView]).SelectNavItem(page);
        }

        public static void NavigateToDefaultPage()
        {
            if (GlobalData.AppSettings.Daemon[GlobalData.AppSettings.ActiveCoin].IsWalletOnly)
            {
                ((MainViewModel)GlobalData.ViewModelPages[SplitViewPages.MainView]).CurrentPage = GlobalData.ViewModelPages[SplitViewPages.Wallet];
            }
            else
            {
                ((MainViewModel)GlobalData.ViewModelPages[SplitViewPages.MainView]).CurrentPage = GlobalData.ViewModelPages[SplitViewPages.Daemon];
            }
        }

        // Messages that fit on a phone use the same text at both status bar widths
        public static void UpdateDaemonStatus(string message)
        {
            UpdateDaemonStatus(message, message);
        }

        public static void UpdateDaemonStatus(string message, string shortMessage)
        {
            if (GlobalData.ViewModelPages.ContainsKey(SplitViewPages.MainView))
            {
                MainViewModel mainViewVm = (MainViewModel)GlobalData.ViewModelPages[SplitViewPages.MainView];

                if (mainViewVm.DaemonStatus != message)
                {
                    mainViewVm.DaemonStatus = message;
                }
                if (mainViewVm.DaemonStatusShort != shortMessage)
                {
                    mainViewVm.DaemonStatusShort = shortMessage;
                }
            }
        }

        public static void UpdateDaemonVersion(string version)
        {
            if (GlobalData.ViewModelPages.ContainsKey(SplitViewPages.MainView))
            {
                if (((MainViewModel)GlobalData.ViewModelPages[SplitViewPages.MainView]).DaemonVersion != version)
                {
                    ((MainViewModel)GlobalData.ViewModelPages[SplitViewPages.MainView]).DaemonVersion = version;
                }
            }
        }

        public static void UpdateDaemonSyncProgress(double progress, bool isSyncing)
        {
            if (GlobalData.ViewModelPages.ContainsKey(SplitViewPages.MainView))
            {
                MainViewModel mainViewVm = (MainViewModel)GlobalData.ViewModelPages[SplitViewPages.MainView];

                if (mainViewVm.DaemonSyncProgress != progress)
                {
                    mainViewVm.DaemonSyncProgress = progress;
                }
                if (mainViewVm.IsDaemonSyncing != isSyncing)
                {
                    mainViewVm.IsDaemonSyncing = isSyncing;
                }
            }
        }

        public static void UpdateWalletSyncProgress(double progress, bool isSyncing)
        {
            if (GlobalData.ViewModelPages.ContainsKey(SplitViewPages.MainView))
            {
                MainViewModel mainViewVm = (MainViewModel)GlobalData.ViewModelPages[SplitViewPages.MainView];

                if (mainViewVm.WalletSyncProgress != progress)
                {
                    mainViewVm.WalletSyncProgress = progress;
                }
                if (mainViewVm.IsWalletSyncing != isSyncing)
                {
                    mainViewVm.IsWalletSyncing = isSyncing;
                }
            }
        }

        public static void UpdateCoinIcon(Bitmap icon)
        {
            if (GlobalData.ViewModelPages.ContainsKey(SplitViewPages.MainView))
            {
                ((MainViewModel)GlobalData.ViewModelPages[SplitViewPages.MainView]).CoinIcon = icon;
            }
        }

        #region Actual UI Update
        public static void UpdateDaemonView()
        {
            try
            {
                DaemonViewModel vm = (DaemonViewModel)GlobalData.ViewModelPages[SplitViewPages.Daemon];

                // All updates on the UI thread so PropertyChanged events are coalesced into one render pass
                Dispatcher.UIThread.Invoke(() =>
                {
                    vm.NetHeight = GlobalData.NetworkStats.NetHeight.ToString();
                    vm.YourHeight = GlobalData.NetworkStats.YourHeight.ToString();
                    vm.NetHash = GlobalData.NetworkStats.NetHashString;
                    vm.RunTime = GlobalData.NetworkStats.RunTime;
                    vm.MinerMessage = GlobalData.NetworkStats.MinerStatus;
                    vm.YourHash = GlobalData.NetworkStats.YourHash;
                    vm.BlockTime = GlobalData.NetworkStats.BlockTime;
                    vm.MiningAddress = GlobalData.NetworkStats.MiningAddress;

                    if (GlobalData.NetworkStats.MinerStatus.Equals(StatusMiner.Mining))
                    {
                        if (vm.StartStopMining.Equals(StatusMiner.StartMining))
                        {
                            vm.StartStopMining = StatusMiner.StopMining;
                        }
                        if (vm.IsNumThreadsEnabled)
                        {
                            vm.IsNumThreadsEnabled = false;
                        }
                        if (!vm.IsMiningActive)
                        {
                            vm.IsMiningActive = true;
                        }
                    }
                    else
                    {
                        if (vm.StartStopMining.Equals(StatusMiner.StopMining))
                        {
                            vm.StartStopMining = StatusMiner.StartMining;
                        }
                        if (!vm.IsNumThreadsEnabled)
                        {
                            vm.IsNumThreadsEnabled = true;
                        }
                        if (vm.IsMiningActive)
                        {
                            vm.IsMiningActive = false;
                        }
                    }

                    if (vm.Connections.Count == 0)
                    {
                        if (GlobalData.NetworkStats.Connections.Count > 0)
                        {
                            vm.Connections = [.. GlobalData.NetworkStats.Connections.Values];
                        }
                    }
                    else
                    {
                        List<Connection> deleteConnections = [];
                        List<Connection> addConnections = [];
                        HashSet<string> checkedAddresses = [];

                        foreach (Connection connection in vm.Connections)
                        {
                            string connectionsKey = connection.Address + connection.IsIncoming;
                            checkedAddresses.Add(connectionsKey);

                            if (GlobalData.NetworkStats.Connections.ContainsKey(connectionsKey))
                            {
                                // Update, only if value changed
                                if (!connection.Height.Equals(GlobalData.NetworkStats.Connections[connectionsKey].Height))
                                {
                                    connection.Height = GlobalData.NetworkStats.Connections[connectionsKey].Height;
                                }
                                if (!connection.LiveTime.Equals(GlobalData.NetworkStats.Connections[connectionsKey].LiveTime))
                                {
                                    connection.LiveTime = GlobalData.NetworkStats.Connections[connectionsKey].LiveTime;
                                }
                                if (!connection.State.Equals(GlobalData.NetworkStats.Connections[connectionsKey].State))
                                {
                                    connection.State = GlobalData.NetworkStats.Connections[connectionsKey].State;
                                }
                            }
                            else
                            {
                                deleteConnections.Add(connection);
                            }
                        }

                        foreach (string key in GlobalData.NetworkStats.Connections.Keys)
                        {
                            if (!checkedAddresses.Contains(key))
                            {
                                addConnections.Add(GlobalData.NetworkStats.Connections[key]);
                            }
                        }

                        foreach (Connection connection in deleteConnections)
                        {
                            vm.Connections.Remove(connection);
                        }
                        foreach (Connection connection in addConnections)
                        {
                            vm.Connections.Add(connection);
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                Logger.LogException("UIM.UPDV", ex);
            }
        }

        public static void UpdateStatusBar()
        {
            try
            {
                if (GlobalData.DaemonState == DaemonState.Downloading)
                {
                    // Don't want to update
                    return;
                }

                // Daemon is still catching up to the network when it is behind the tip. Heights come from get_info
                // so they are only known with a local daemon. Wallet only mode never fetches daemon data
                ulong netHeight = GlobalData.NetworkStats.NetHeight;
                ulong yourHeight = GlobalData.NetworkStats.YourHeight;
                bool isDaemonSyncing = netHeight > 0 && yourHeight > 0 && yourHeight + GlobalData.SyncProgressMinBlocksBehind <= netHeight;
                double daemonProgress = isDaemonSyncing ? GlobalMethods.GetSyncProgress(yourHeight, netHeight) : 0;
                UpdateDaemonSyncProgress(daemonProgress, isDaemonSyncing);

                if (GlobalData.AppSettings.Daemon[GlobalData.AppSettings.ActiveCoin].IsWalletOnly)
                {
                    string remoteStatus = "Remote" + (string.IsNullOrEmpty(GlobalData.NetworkStats.StatusSync) ? "" : " | " + GlobalData.NetworkStats.StatusSync);
                    UpdateDaemonStatus(remoteStatus);
                }
                else
                {
                    string rawVersion = GlobalData.NetworkStats.Version;

                    // Handle Bitcoin's "/Satoshi:31.0.0/" format - extract part between ':' and trailing '/'
                    int colonIndex = rawVersion.IndexOf(':');
                    if (colonIndex >= 0)
                    {
                        rawVersion = rawVersion[(colonIndex + 1)..].TrimEnd('/');
                    }

                    if (!rawVersion.ToLower().StartsWith("v"))
                    {
                        rawVersion = "v" + rawVersion;
                    }

                    // Keep only the numeric part: e.g. "v0.18.4.5-release" -> "v0.18.4.5"
                    int dashIndex = rawVersion.IndexOf('-');
                    string version = dashIndex > 0 ? rawVersion[..dashIndex] : rawVersion;
                    string connections = "↑" + GlobalData.NetworkStats.ConnectionsOut + "  ↓" + GlobalData.NetworkStats.ConnectionsIn;
                    string connectionsShort = "↑" + GlobalData.NetworkStats.ConnectionsOut + " ↓" + GlobalData.NetworkStats.ConnectionsIn;
                    string publicIndicator = GlobalData.AppSettings.Daemon[GlobalData.AppSettings.ActiveCoin].IsPublicNode ? " | Public" : string.Empty;

                    // While syncing the heights are rebuilt here instead of using StatusSync so the narrow width can drop them.
                    // StatusSync still carries the transient messages like "Loading..." at both widths
                    string sync;
                    string syncShort;
                    if (isDaemonSyncing)
                    {
                        string percent = GlobalMethods.GetSyncProgressText(daemonProgress);
                        sync = "Sync (" + yourHeight + " of " + netHeight + ") " + percent;
                        syncShort = "Sync " + percent;
                    }
                    else
                    {
                        sync = string.IsNullOrEmpty(GlobalData.NetworkStats.StatusSync) ? "Connecting to daemon..." : GlobalData.NetworkStats.StatusSync;
                        syncShort = sync;
                    }

                    UpdateDaemonStatus(version + " | " + connections + " | " + sync + publicIndicator,
                        version + " | " + connectionsShort + " | " + syncShort + publicIndicator);
                }
            }
            catch (Exception ex)
            {
                Logger.LogException("UIM.UPSB", ex);
            }
        }
        
        // Cached so Wallet and Transfers updates in same timer tick do not both hit the file system
        private static int GetWalletFilesCount()
        {
            if (_isWalletFilesScanNeeded || _lastWalletFilesScan.AddSeconds(_walletFilesScanSeconds) < DateTime.Now)
            {
                _isWalletFilesScanNeeded = false;
                _lastWalletFilesScan = DateTime.Now;
                _walletFilesCount = GlobalMethods.GetWalletFileNames().Count;
            }

            return _walletFilesCount;
        }

        // Message and button to show on Wallet page when no wallet is open. Brand new users have no wallets
        // and need to create one. Everyone else just needs to open one of theirs
        private static (bool IsNoWallets, string Message) GetWalletEmptyState()
        {
            int walletCount = GetWalletFilesCount();
            string message;

            if (walletCount == 0)
            {
                message = "You do not have a wallet yet.\r\n\r\nCreate a new wallet or restore an existing one to get started.";
            }
            else if (walletCount == 1)
            {
                message = "Your wallet is closed.\r\n\r\nOpen it to see your balance and addresses.";
            }
            else
            {
                message = "You have " + walletCount + " wallets.\r\n\r\nOpen one to see your balance and addresses.";
            }

            return (walletCount == 0, message);
        }

        // Transfers has nothing to show without an open wallet either, but opening one lives on Wallet page
        // so users are pointed there instead of at Wallet Setup, unless they have no wallets at all
        private static (bool IsNoWallets, string Message) GetTransfersEmptyState()
        {
            int walletCount = GetWalletFilesCount();
            string message;

            if (walletCount == 0)
            {
                message = "You do not have a wallet yet.\r\n\r\nSet one up to start sending and receiving.";
            }
            else
            {
                message = "No wallet is open.\r\n\r\nOpen one on Wallet screen to see your transactions.";
            }

            return (walletCount == 0, message);
        }

        // Called when Wallet page is shown so new user sees what to do next without waiting for master timer
        public static void RefreshWalletEmptyState()
        {
            try
            {
                WalletViewModel walletVm = (WalletViewModel)GlobalData.ViewModelPages[SplitViewPages.Wallet];
                (bool isNoWallets, string message) = GetWalletEmptyState();

                walletVm.IsNoWalletsState = isNoWallets;
                walletVm.EmptyStateMessage = message;
                walletVm.IsEmptyStateVisible = !GlobalData.IsWalletOpen;
            }
            catch (Exception ex)
            {
                Logger.LogException("UIM.RWES", ex);
            }
        }

        // Same as above but for Transfers page
        public static void RefreshTransfersEmptyState()
        {
            try
            {
                TransfersViewModel transfersVm = (TransfersViewModel)GlobalData.ViewModelPages[SplitViewPages.Transfers];
                (bool isNoWallets, string message) = GetTransfersEmptyState();

                transfersVm.IsNoWalletsState = isNoWallets;
                transfersVm.EmptyStateMessage = message;
                transfersVm.IsEmptyStateVisible = !GlobalData.IsWalletOpen;
            }
            catch (Exception ex)
            {
                Logger.LogException("UIM.RTES", ex);
            }
        }

        public static void UpdateWalletView()
        {
            try
            {
                WalletViewModel walletVm = (WalletViewModel)GlobalData.ViewModelPages[SplitViewPages.Wallet];
                MainViewModel mainViewVm = (MainViewModel)GlobalData.ViewModelPages[SplitViewPages.MainView];

                if (GlobalData.IsWalletOpen)
                {
                    // Wallet files will need to be rescanned as soon as this wallet closes
                    _isWalletFilesScanNeeded = true;

                    // Compute collection changes on timer thread before marshaling to UI thread
                    List<Account> deleteAccounts = [];
                    List<Account> addAccounts = [];

                    if (walletVm.WalletAddresses.Count > 0)
                    {
                        HashSet<uint> checkedIndexes = [];

                        foreach (Account wallet in walletVm.WalletAddresses)
                        {
                            checkedIndexes.Add(wallet.Index);

                            if (!GlobalData.WalletStats.Subaddresses.ContainsKey(wallet.Index))
                            {
                                deleteAccounts.Add(wallet);
                            }
                        }

                        foreach (uint index in GlobalData.WalletStats.Subaddresses.Keys)
                        {
                            if (!checkedIndexes.Contains(index))
                            {
                                addAccounts.Add(GlobalData.WalletStats.Subaddresses[index]);
                            }
                        }
                    }

                    bool isBtcStyle = GlobalData.CoinSettings[GlobalData.AppSettings.ActiveCoin].IsWalletBtcStyle;
                    string units = GlobalData.AppSettings.Wallet[GlobalData.AppSettings.ActiveCoin].DisplayUnits;
                    string totalLockedLabel = "Total " + units + ":";
                    string totalUnlockedLabel = (isBtcStyle ? "Pending " : "Unlocked ") + units + ":";
                    bool isWalletHeightSupported = GlobalData.CoinSettings[GlobalData.AppSettings.ActiveCoin].IsWalletHeightSupported;

                    // Wallet is still scanning the chain when it is behind the tip. BTC based coins have no wallet
                    // scan height and wallet only mode has no tip height, so neither shows scan progress
                    ulong walletNetHeight = GlobalData.NetworkStats.NetHeight;
                    bool isWalletSyncing = isWalletHeightSupported && walletNetHeight > 0 && GlobalData.WalletHeight > 0 && GlobalData.WalletHeight + GlobalData.SyncProgressMinBlocksBehind <= walletNetHeight;
                    double walletProgress = isWalletSyncing ? GlobalMethods.GetSyncProgress(GlobalData.WalletHeight, walletNetHeight) : 0;

                    // Name and balance always fit so they are the base for both widths. Height is spelled out when
                    // there is room and abbreviated on a phone, where scanning drops it entirely for the percent
                    string statusBarMessage = GlobalData.OpenedWalletName + " | " + GlobalMethods.FormatAmount(GlobalData.WalletStats.BalanceTotal) + " " + units;
                    string statusBarMessageShort = statusBarMessage;

                    if (isWalletHeightSupported)
                    {
                        if (isWalletSyncing)
                        {
                            string percent = GlobalMethods.GetSyncProgressText(walletProgress);
                            statusBarMessage += " | Height: " + GlobalData.WalletHeight + " (" + percent + ")";
                            statusBarMessageShort += " | " + percent;
                        }
                        else
                        {
                            statusBarMessage += " | Height: " + GlobalData.WalletHeight;
                            statusBarMessageShort += " | H: " + GlobalData.WalletHeight;
                        }
                    }

                    Dispatcher.UIThread.Invoke(() =>
                    {
                        string totalFormatted = GlobalMethods.FormatAmount(GlobalData.WalletStats.BalanceTotal);
                        if (!walletVm.TotalCoins.Equals(totalFormatted))
                        {
                            walletVm.TotalCoins = totalFormatted;
                        }

                        string secondBalance = isBtcStyle ? GlobalMethods.FormatAmount(GlobalData.WalletStats.BalancePending) : GlobalMethods.FormatAmount(GlobalData.WalletStats.BalanceUnlocked);
                        if (!walletVm.UnlockedCoins.Equals(secondBalance))
                        {
                            walletVm.UnlockedCoins = secondBalance;
                        }
                        if (!walletVm.TotalLockedLabel.Equals(totalLockedLabel))
                        {
                            walletVm.TotalLockedLabel = totalLockedLabel;
                        }
                        if (!walletVm.TotalUnlockedLabel.Equals(totalUnlockedLabel))
                        {
                            walletVm.TotalUnlockedLabel = totalUnlockedLabel;
                        }

                        if (walletVm.WalletAddresses.Count == 0 && GlobalData.WalletStats.Subaddresses.Count > 0)
                        {
                            walletVm.WalletAddresses = [.. GlobalData.WalletStats.Subaddresses.Values];
                        }
                        else
                        {
                            // Update existing account fields, only if value changed
                            // GlobalData.WalletStats.Subaddresses get cleared in asynchronous call so sometimes even though you check if key exists, it will not exist anymore
                            foreach (Account wallet in walletVm.WalletAddresses)
                            {
                                if (GlobalData.WalletStats.Subaddresses.ContainsKey(wallet.Index))
                                {
                                    if (!wallet.Label.Equals(GlobalData.WalletStats.Subaddresses[wallet.Index].Label))
                                    {
                                        wallet.Label = GlobalData.WalletStats.Subaddresses[wallet.Index].Label;
                                    }
                                    if (!wallet.AddressShort.Equals(GlobalData.WalletStats.Subaddresses[wallet.Index].AddressShort))
                                    {
                                        wallet.AddressShort = GlobalData.WalletStats.Subaddresses[wallet.Index].AddressShort;
                                    }
                                    if (wallet.BalanceTotal != GlobalData.WalletStats.Subaddresses[wallet.Index].BalanceTotal)
                                    {
                                        wallet.BalanceTotal = GlobalData.WalletStats.Subaddresses[wallet.Index].BalanceTotal;
                                    }
                                    if (wallet.BalanceUnlocked != GlobalData.WalletStats.Subaddresses[wallet.Index].BalanceUnlocked)
                                    {
                                        wallet.BalanceUnlocked = GlobalData.WalletStats.Subaddresses[wallet.Index].BalanceUnlocked;
                                    }
                                }
                            }

                            foreach (Account wallet in deleteAccounts)
                            {
                                walletVm.WalletAddresses.Remove(wallet);
                            }
                            foreach (Account wallet in addAccounts)
                            {
                                walletVm.WalletAddresses.Add(wallet);
                            }
                        }

                        if (walletVm.IsEmptyStateVisible)
                        {
                            walletVm.IsEmptyStateVisible = false;
                        }

                        if (mainViewVm.WalletStatus != statusBarMessage)
                        {
                            mainViewVm.WalletStatus = statusBarMessage;
                        }
                        if (mainViewVm.WalletStatusShort != statusBarMessageShort)
                        {
                            mainViewVm.WalletStatusShort = statusBarMessageShort;
                        }

                        UpdateWalletSyncProgress(walletProgress, isWalletSyncing);
                    });
                }
                else
                {
                    // If wallet is closed/was closed by the user, clear fields
                    string totalLockedLabel = "Total " + GlobalData.AppSettings.Wallet[GlobalData.AppSettings.ActiveCoin].DisplayUnits + ":";
                    string totalUnlockedLabel = "Unlocked " + GlobalData.AppSettings.Wallet[GlobalData.AppSettings.ActiveCoin].DisplayUnits + ":";

                    // Work out empty state on timer thread before marshaling to UI thread
                    (bool isNoWalletsState, string emptyStateMessage) = GetWalletEmptyState();

                    Dispatcher.UIThread.Invoke(() =>
                    {
                        if (!string.IsNullOrEmpty(walletVm.TotalCoins))
                        {
                            walletVm.TotalCoins = string.Empty;
                        }
                        if (!string.IsNullOrEmpty(walletVm.UnlockedCoins))
                        {
                            walletVm.UnlockedCoins = string.Empty;
                        }
                        if (!walletVm.TotalLockedLabel.Equals(totalLockedLabel))
                        {
                            walletVm.TotalLockedLabel = totalLockedLabel;
                        }
                        if (!walletVm.TotalUnlockedLabel.Equals(totalUnlockedLabel))
                        {
                            walletVm.TotalUnlockedLabel = totalUnlockedLabel;
                        }
                        if (walletVm.WalletAddresses.Count != 0)
                        {
                            walletVm.WalletAddresses = [];
                        }

                        if (walletVm.IsNoWalletsState != isNoWalletsState)
                        {
                            walletVm.IsNoWalletsState = isNoWalletsState;
                        }
                        if (!walletVm.EmptyStateMessage.Equals(emptyStateMessage))
                        {
                            walletVm.EmptyStateMessage = emptyStateMessage;
                        }
                        if (!walletVm.IsEmptyStateVisible)
                        {
                            walletVm.IsEmptyStateVisible = true;
                        }

                        if (mainViewVm.WalletStatus != GlobalData.WalletClosedMessage)
                        {
                            mainViewVm.WalletStatus = GlobalData.WalletClosedMessage;
                        }
                        if (mainViewVm.WalletStatusShort != GlobalData.WalletClosedMessage)
                        {
                            mainViewVm.WalletStatusShort = GlobalData.WalletClosedMessage;
                        }

                        UpdateWalletSyncProgress(0, false);
                    });
                }
            }
            catch (Exception ex)
            {
                Logger.LogException("UIM.UPWV", ex);
            }
        }

        public static void UpdateTransfersView()
        {
            int newTransfersCount = 0;

            try
            {
                TransfersViewModel transfersViewVm = (TransfersViewModel)GlobalData.ViewModelPages[SplitViewPages.Transfers];

                if (GlobalData.IsWalletOpen)
                {
                    if (transfersViewVm.IsEmptyStateVisible)
                    {
                        Dispatcher.UIThread.Invoke(() =>
                        {
                            transfersViewVm.IsEmptyStateVisible = false;
                        });
                    }

                    if (transfersViewVm.Transactions.Count == 0)
                    {
                        ObservableCollection<Transfer> initialTransfers = [.. GlobalData.TransfersStats.Transactions.Values];

                        if (initialTransfers.Count > 0)
                        {
                            Dispatcher.UIThread.Invoke(() =>
                            {
                                transfersViewVm.Transactions = [.. initialTransfers.OrderByDescending(t => t.Timestamp)];
                            });

                            // Need to clear transfers AFTER we process them otherwise we might clear them before we process them
                            GlobalData.TransfersStats.Transactions = [];

                            // The initial load of a large wallet leaves significant heap pressure from JSON
                            // deserialization intermediates. Hint GC to collect now rather than waiting for
                            // organic pressure — blocking:false lets it run in the background.
                            GC.Collect(2, GCCollectionMode.Forced, blocking: false, compacting: false);
                        }

                        if (transfersViewVm.Transactions.Count > 0)
                        {
                            // This will also save after initial open BUT it will cover restoring wallet
                            newTransfersCount = transfersViewVm.Transactions.Count;
                        }
                    }
                    else
                    {
                        int confirmationThreshold = GlobalData.CoinSettings[GlobalData.AppSettings.ActiveCoin].ConfirmationThreshold;
                        bool isBtcStyle = GlobalData.CoinSettings[GlobalData.AppSettings.ActiveCoin].IsWalletBtcStyle;

                        // Step 1: Classify incoming transactions from the RPC poll
                        // For XMR based coins, get_transfers filters by MinHeight so this staging dict only contains brand-new transactions (transactions already in the UI are not returned again).
                        // For BTC based coins, listsinceblock returns everything every poll, so this dict is always non-empty.
                        // We skip building the lookup entirely when the staging dict is empty (common for XMR).
                        if (GlobalData.TransfersStats.Transactions.Count > 0)
                        {
                            // O(1) lookup of what is already visible in the UI
                            Dictionary<string, Transfer> uiTransactionLookup = transfersViewVm.Transactions
                                .ToDictionary(uiTx => uiTx.TransactionId + uiTx.Type + uiTx.AddressShort, uiTx => uiTx);

                            List<Transfer> brandNewTransactions = [];
                            List<(Transfer UiEntry, Transfer RpcEntry)> pendingConfirmedUpdates = [];

                            foreach (var rpcTx in GlobalData.TransfersStats.Transactions)
                            {
                                if (!uiTransactionLookup.TryGetValue(rpcTx.Key, out Transfer? uiEntry))
                                {
                                    // Transaction not in UI yet — add it
                                    brandNewTransactions.Add(rpcTx.Value);
                                }
                                else if (uiEntry.Height == 0 && rpcTx.Value.Height > 0)
                                {
                                    // Was pending (Height == 0), now confirmed — update block height and display
                                    pendingConfirmedUpdates.Add((uiEntry, rpcTx.Value));
                                }
                            }

                            if (brandNewTransactions.Count > 0 || pendingConfirmedUpdates.Count > 0)
                            {
                                // Sort new transactions oldest-first so Insert(0) ends up newest-on-top
                                List<Transfer> orderedNewTransactions = brandNewTransactions.Count > 0
                                    ? [.. brandNewTransactions.OrderBy(tx => tx.Timestamp)]
                                    : [];

                                Dispatcher.UIThread.Invoke(() =>
                                {
                                    foreach (var (uiEntry, rpcEntry) in pendingConfirmedUpdates)
                                    {
                                        // INotifyPropertyChanged on Height, HeightDisplay, Confirmations fires individual cell refresh
                                        uiEntry.Height = rpcEntry.Height;
                                        uiEntry.HeightDisplay = rpcEntry.HeightDisplay;
                                        uiEntry.Confirmations = rpcEntry.Confirmations;
                                    }

                                    foreach (Transfer newTransaction in orderedNewTransactions)
                                    {
                                        transfersViewVm.Transactions.Insert(0, newTransaction);
                                    }
                                });

                                if (brandNewTransactions.Count > 0)
                                {
                                    // New transactions start unconfirmed — make sure the confirmation scan runs
                                    GlobalData.HasUnconfirmedTransactions = true;
                                    newTransfersCount = brandNewTransactions.Count;
                                }
                            }

                            // Clear staging dict after processing so the next poll starts fresh
                            GlobalData.TransfersStats.Transactions = [];
                        }

                        // Step 2: Update confirmation counts for all coins
                        // Confirmations are calculated locally from current chain height rather than taken from the
                        // RPC response. This works for all coins and handles XMR wallet-only mode correctly:
                        //  XMR Based: WalletHeight = block count from wallet RPC (always available, even without a local daemon). Formula: confirmations = WalletHeight - txHeight.
                        //  BTC Based: NetworkStats.YourHeight = tip block height from daemon RPC. Adding 1 makes it a block count so the same formula applies to both coin styles.
                        // HasUnconfirmedTransactions prevents iterating all rows every poll once all transactions are fully confirmed. It resets to true whenever new transactions arrive.
                        ulong currentChainBlockCount = isBtcStyle
                            ? GlobalData.NetworkStats.YourHeight + 1UL
                            : GlobalData.WalletHeight;

                        if (currentChainBlockCount > 0 && GlobalData.HasUnconfirmedTransactions)
                        {
                            List<(Transfer UiEntry, long UpdatedConfirmations)> confirmationUpdates = [];
                            bool anyTransactionStillUnconfirmed = false;

                            foreach (Transfer uiTransaction in transfersViewVm.Transactions)
                            {
                                if (uiTransaction.Height > 0 && currentChainBlockCount > uiTransaction.Height)
                                {
                                    // Transaction is in a known block — calculate how many blocks have built on top
                                    if (uiTransaction.Confirmations < confirmationThreshold)
                                    {
                                        long calculatedConfirmations = (long)(currentChainBlockCount - uiTransaction.Height);
                                        long cappedConfirmations = Math.Min(calculatedConfirmations, confirmationThreshold);

                                        if (calculatedConfirmations > uiTransaction.Confirmations)
                                        {
                                            confirmationUpdates.Add((uiTransaction, cappedConfirmations));
                                        }

                                        if (cappedConfirmations < confirmationThreshold)
                                        {
                                            anyTransactionStillUnconfirmed = true;
                                        }
                                    }
                                }
                                else if (uiTransaction.Height == 0)
                                {
                                    // Height == 0 means the transaction is still in the mempool (pending)
                                    anyTransactionStillUnconfirmed = true;
                                }
                            }

                            // Update the flag before dispatching so the next poll can skip if everything is confirmed
                            GlobalData.HasUnconfirmedTransactions = anyTransactionStillUnconfirmed;

                            if (confirmationUpdates.Count > 0)
                            {
                                Dispatcher.UIThread.Invoke(() =>
                                {
                                    foreach (var (uiEntry, updatedConfirmations) in confirmationUpdates)
                                    {
                                        // INotifyPropertyChanged fires individual cell refresh
                                        uiEntry.Confirmations = updatedConfirmations;
                                    }
                                });
                            }
                        }
                    }

                    if (!GlobalData.IsWalletJustOpened && newTransfersCount > 0)
                    {
                        if (GlobalData.CoinSettings[GlobalData.AppSettings.ActiveCoin].IsSavingWalletSupported)
                        {
                            Logger.LogDebug("UIM.UPTV", "Auto-saving wallet. New transfers count: " + newTransfersCount);
                            GlobalMethods.SaveWallet();
                        }
                    }
                }
                else
                {
                    // Work out empty state on timer thread before marshaling to UI thread
                    (bool isNoWalletsState, string emptyStateMessage) = GetTransfersEmptyState();

                    Dispatcher.UIThread.Invoke(() =>
                    {
                        if (transfersViewVm.IsNoWalletsState != isNoWalletsState)
                        {
                            transfersViewVm.IsNoWalletsState = isNoWalletsState;
                        }
                        if (!transfersViewVm.EmptyStateMessage.Equals(emptyStateMessage))
                        {
                            transfersViewVm.EmptyStateMessage = emptyStateMessage;
                        }
                        if (!transfersViewVm.IsEmptyStateVisible)
                        {
                            transfersViewVm.IsEmptyStateVisible = true;
                        }
                    });

                    // If wallet is closed/was closed by the user, clear fields
                    if (transfersViewVm.Transactions.Count != 0)
                    {
                        Dispatcher.UIThread.Invoke(() =>
                        {
                            transfersViewVm.Transactions = [];
                        });

                        // The large Transfer collection is now eligible for GC. Hint the runtime to
                        // collect immediately — without allocation pressure GC may not run for minutes.
                        GC.Collect(2, GCCollectionMode.Forced, blocking: false, compacting: false);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogException("UIM.UPTV", ex);
            }
        }
        #endregion // Actual UI Update

        #region Get Data for UI
        public static void HandleNetworkStats()
        {
            try
            {
                if (GlobalData.AppSettings.Daemon[GlobalData.AppSettings.ActiveCoin].IsWalletOnly)
                {
                    GlobalData.NetworkStats = new()
                    {
                        StatusSync = GlobalData.AppSettings.Wallet[GlobalData.AppSettings.ActiveCoin].PublicNodeAddress,
                        Connections = []
                    };
                    return;
                }

                switch (GlobalData.DaemonState)
                {
                    case DaemonState.Downloading:
                        // Status bar is updated directly when downloading/extracting; just keep NetworkStats neutral
                        GlobalData.NetworkStats = new() { Connections = [] };
                        break;
                    case DaemonState.CliToolsMissing:
                        GlobalData.NetworkStats = new() { StatusSync = "Client tools missing.", Connections = [] };
                        break;
                    case DaemonState.Restarting:
                        GlobalData.NetworkStats = new() { StatusSync = "Restarting daemon...", Connections = [] };
                        break;
                    case DaemonState.WarmingUp:
                        GlobalData.NetworkStats = new() { StatusSync = "Loading...", Connections = [] };
                        break;
                    case DaemonState.Connecting:
                        GlobalData.NetworkStats = new() { StatusSync = "Connecting to daemon...", Connections = [] };
                        break;
                }
            }
            catch (Exception ex)
            {
                Logger.LogException("UIM.GSDD", ex);
            }
        }

        public static async void GetAndSetDaemonData()
        {
            try
            {
                if (GlobalData.IsGetAndSetDaemonDataComplete)
                {
                    GlobalData.IsGetAndSetDaemonDataComplete = false;

                    GetInfoResponse infoRes = await GlobalData.DaemonService.GetInfo(GlobalData.AppSettings.Daemon[GlobalData.AppSettings.ActiveCoin].Rpc, new GetInfoRequest());
                    
                    if(infoRes.Status == StatusDaemon.WarmingUp)
                    {
                        GlobalData.LastDaemonResponseTime = DateTime.Now;
                        GlobalData.DaemonState = DaemonState.WarmingUp;
                    }
                    else if(infoRes.Error.IsError)
                    {
                        Logger.LogError("UIM.GSDD", "GetInfo Error | Code: " + infoRes.Error.Code + " | Message: " + infoRes.Error.Message + " | Content: " + infoRes.Error.Content);
                    }
                    else
                    {
                        GlobalData.LastDaemonResponseTime = DateTime.Now;
                        // This will be used to get rid of establishing connection message and to StartWalletUiUpdate
                        GlobalData.DaemonState = DaemonState.Running;

                        GlobalData.NetworkStats.NetHeight = (infoRes.TargetHeight > infoRes.Height ? infoRes.TargetHeight : infoRes.Height);
                        GlobalData.NetworkStats.YourHeight = infoRes.Height;
                        GlobalData.NetworkStats.NetHashRate = infoRes.NetworkHashRate;

                        if ((infoRes.NetworkHashRate / 1000000000000000000.0d) > 1)
                        {
                            GlobalData.NetworkStats.NetHashString = Math.Round((infoRes.NetworkHashRate / 1000000000000000000.0d), 2) + " EH/s";
                        }
                        else if ((infoRes.NetworkHashRate / 1000000000000000.0d) > 1)
                        {
                            GlobalData.NetworkStats.NetHashString = Math.Round((infoRes.NetworkHashRate / 1000000000000000.0d), 2) + " PH/s";
                        }
                        else if ((infoRes.NetworkHashRate / 1000000000000.0d) > 1)
                        {
                            GlobalData.NetworkStats.NetHashString = Math.Round((infoRes.NetworkHashRate / 1000000000000.0d), 2) + " TH/s";
                        }
                        else if ((infoRes.NetworkHashRate / 1000000000.0d) > 1)
                        {
                            GlobalData.NetworkStats.NetHashString = Math.Round((infoRes.NetworkHashRate / 1000000000.0d), 2) + " GH/s";
                        }
                        else if ((infoRes.NetworkHashRate / 1000000.0d) > 1)
                        {
                            GlobalData.NetworkStats.NetHashString = Math.Round((infoRes.NetworkHashRate / 1000000.0d), 2) + " MH/s";
                        }
                        else if ((infoRes.NetworkHashRate / 1000.0d) > 1)
                        {
                            GlobalData.NetworkStats.NetHashString = Math.Round((infoRes.NetworkHashRate / 1000.0d), 2) + " KH/s";
                        }
                        else
                        {
                            GlobalData.NetworkStats.NetHashString = infoRes.NetworkHashRate + " H/s";
                        }

                        DateTime miningStartTime = infoRes.StartTime;
                        GlobalData.NetworkStats.RunTime = (DateTime.Now.ToUniversalTime() - miningStartTime).ToString(@"%d\.hh\:mm\:ss");

                        GlobalData.NetworkStats.ConnectionsIn = infoRes.ConnectionCountIn;
                        GlobalData.NetworkStats.ConnectionsOut = infoRes.ConnectionCountOut;

                        // Used for Connections Guard
                        if(GlobalData.NetworkStats.ConnectionsIn + GlobalData.NetworkStats.ConnectionsOut >= GlobalData.ConnectGuardMinimumGoodCount)
                        {
                            GlobalData.ConnectGuardLastGoodTime = DateTime.Now;
                        }

                        GlobalData.NetworkStats.Version = infoRes.Version;
                        GlobalData.NetworkStats.StatusSync = "";
                        if (infoRes.TargetHeight != 0 && infoRes.Height < infoRes.TargetHeight)
                        {
                            GlobalData.NetworkStats.StatusSync += "Sync (" + infoRes.Height + " of " + infoRes.TargetHeight + ")";

                            // See if user wants to use QuickSync if they're far behind
                            if (!_askedToQuickSync && !string.IsNullOrEmpty(GlobalData.CoinSettings[GlobalData.AppSettings.ActiveCoin].QuickSyncUrl))
                            {
                                _askedToQuickSync = true;
                                double percentSynced = infoRes.Height / Convert.ToDouble(infoRes.TargetHeight);

                                if (percentSynced < 0.9)
                                {
                                    ((MainViewModel)GlobalData.ViewModelPages[SplitViewPages.MainView]).AskIfSyncWithQuickSync(percentSynced);
                                }
                            }
                        }
                        else
                        {
                            GlobalData.NetworkStats.StatusSync += "Sync OK";
                        }

                        if (GlobalData.CoinSettings[GlobalData.AppSettings.ActiveCoin].IsCpuMiningSupported)
                        {
                            MiningStatusResponse miningRes = await GlobalData.DaemonService.GetMiningStatus(GlobalData.AppSettings.Daemon[GlobalData.AppSettings.ActiveCoin].Rpc, new MiningStatusRequest());
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
                        }


                        GetConnectionsResponse connectResp = await GlobalData.DaemonService.GetConnections(GlobalData.AppSettings.Daemon[GlobalData.AppSettings.ActiveCoin].Rpc, new GetConnectionsRequest());
                        
                        if (connectResp.Error.IsError)
                        {
                            Logger.LogError("UIM.GSDD", "GetConnections Error | Code: " + connectResp.Error.Code + " | Message: " + connectResp.Error.Message + " | Content: " + connectResp.Error.Content);
                        }
                        else
                        {
                            GlobalData.NetworkStats.Connections = [];

                            foreach (Connection connection in connectResp.Connections)
                            {
                                if (!string.IsNullOrEmpty(connection.Address))
                                {
                                    if (!GlobalData.NetworkStats.Connections.ContainsKey(connection.Address + connection.IsIncoming))
                                    {
                                        connection.InOutIcon = connection.IsIncoming ? _inImage : _outImage;
                                        GlobalData.NetworkStats.Connections.Add(connection.Address + connection.IsIncoming, connection);
                                    }
                                }
                            }
                        }
                    }

                    MasterProcess._isDaemonDataFresh = true;
                    GlobalData.IsGetAndSetDaemonDataComplete = true;
                }
            }
            catch (Exception ex)
            {
                MasterProcess._isDaemonDataFresh = true;
                GlobalData.IsGetAndSetDaemonDataComplete = true;
                Logger.LogException("UIM.GSDD", ex);
            }
        }

        public static async void CallWalletDataMethodsInSync()
        {
            try
            {
                if (GlobalData.IsWalletUpdateComplete)
                {
                    GlobalData.IsWalletUpdateComplete = false;

                    await GetAndSetWalletData();

                    if (GlobalData.CoinSettings[GlobalData.AppSettings.ActiveCoin].IsWalletHeightSupported)
                    {
                        await GetAndSetWalletHeight();
                    }

                    await GetAndSetTransfersData();

                    GlobalData.IsWalletUpdateComplete = true;
                }
            }
            catch (Exception ex)
            {
                Logger.LogException("UIM.CWDS", ex);
                GlobalData.IsWalletUpdateComplete = true;
            }
        }

        public static async Task GetAndSetWalletData()
        {
            try
            {
                if (GlobalData.IsGetAndSetWalletDataComplete)
                {
                    // Wait for GetAccounts to finish before calling next one or updating UI
                    GlobalData.IsGetAndSetWalletDataComplete = false;

                    // Get accounts for Wallets view
                    GetAccountsResponse response = await GlobalData.WalletService.GetAccounts(GlobalData.AppSettings.Wallet[GlobalData.AppSettings.ActiveCoin].Rpc, new GetAccountsRequest());

                    if (response.Error.IsError)
                    {
                        Logger.LogError("UIM.GSWD", "GetAccounts Error | Code: " + response.Error.Code + " | Message: " + response.Error.Message + " | Content: " + response.Error.Content);
                    }
                    else
                    {
                        GlobalData.WalletStats.BalanceTotal = response.BalanceTotal;
                        GlobalData.WalletStats.BalanceUnlocked = response.BalanceUnlocked;
                        GlobalData.WalletStats.BalancePending = response.BalancePending;

                        GlobalData.WalletStats.Subaddresses = [];

                        foreach (Account account in response.SubAccounts)
                        {
                            if (!GlobalData.WalletStats.Subaddresses.ContainsKey(account.Index))
                            {
                                account.WalletIcon = _walletImage;
                                GlobalData.WalletStats.Subaddresses.Add(account.Index, account);
                            }
                        }

                        if (response.SubAccounts.Count == 0 && GlobalData.CoinSettings[GlobalData.AppSettings.ActiveCoin].IsDefaultAddressAutoCreated)
                        {
                            Logger.LogDebug("UIM.GSWD", "Wallet has no addresses — generating default address");
                            await GlobalData.WalletService.CreateAccount(GlobalData.AppSettings.Wallet[GlobalData.AppSettings.ActiveCoin].Rpc, new CreateAccountRequest { Label = string.Empty });
                        }
                    }

                    GlobalData.IsGetAndSetWalletDataComplete = true;
                }
            }
            catch (Exception ex)
            {
                GlobalData.IsGetAndSetWalletDataComplete = true;
                Logger.LogException("UIM.GSWD", ex);
            }
        }

        public static async Task GetAndSetTransfersData()
        {
            try
            {
                if (GlobalData.IsGetAndSetTransfersDataComplete)
                {
                    // Wait for GetTransactions to finish before calling next one or updating UI
                    GlobalData.IsGetAndSetTransfersDataComplete = false;

                    // Get transactions for Transfers view
                    GetTransfersRequest reqTransfers = new GetTransfersRequest();
                    reqTransfers.IncludeIn = true;
                    reqTransfers.IncludeOut = true;
                    reqTransfers.IncludePending = true;
                    reqTransfers.IncludeFailed = false;
                    reqTransfers.IncludePool = false;
                    reqTransfers.IsFilterByHeight = true;
                    reqTransfers.MinHeight = GlobalData.NewestTransactionHeight;
                    reqTransfers.SinceBlockHash = GlobalData.NewestTransactionBlockHash;
                    reqTransfers.AccountIndex = 0;
                    reqTransfers.SubaddressIndices = [];
                    reqTransfers.IsAllAccounts = true;

                    GetTransfersResponse response = await GlobalData.WalletService.GetTransfers(GlobalData.AppSettings.Wallet[GlobalData.AppSettings.ActiveCoin].Rpc, reqTransfers);
                    
                    if (response.Error.IsError)
                    {
                        Logger.LogError("UIM.GSTD", "GetTransfers Error | Code: " + response.Error.Code + " | Message: " + response.Error.Message + " | Content: " + response.Error.Content);
                    }
                    else
                    {
                        bool isBtcStyle = GlobalData.CoinSettings[GlobalData.AppSettings.ActiveCoin].IsWalletBtcStyle;
                        foreach (Transfer transfer in response.Transfers)
                        {
                            transfer.HeightDisplay = isBtcStyle && transfer.Height == 0 ? "Pending" : transfer.Height.ToString();

                            string txKey = transfer.TransactionId + transfer.Type + transfer.AddressShort;
                            if (!GlobalData.TransfersStats.Transactions.ContainsKey(txKey))
                            {
                                if (transfer.Type.Equals(TransferType.In))
                                {
                                    transfer.Icon = _inImage;
                                }
                                else if (transfer.Type.Equals(TransferType.Out))
                                {
                                    transfer.Icon = _outImage;
                                }
                                else if (transfer.Type.Equals(TransferType.Block))
                                {
                                    transfer.Icon = _blockImage;
                                }
                                else if (transfer.Type.Equals(TransferType.Pending))
                                {
                                    transfer.Icon = _pendingImage;
                                }

                                GlobalData.TransfersStats.Transactions.Add(txKey, transfer);

                                if (transfer.Height > GlobalData.NewestTransactionHeight)
                                {
                                    GlobalData.NewestTransactionHeight = transfer.Height;
                                    if (!string.IsNullOrEmpty(transfer.BlockHash))
                                    {
                                        GlobalData.NewestTransactionBlockHash = transfer.BlockHash;
                                    }
                                }
                            }
                        }                        
                    }

                    GlobalData.IsGetAndSetTransfersDataComplete = true;
                }
            }
            catch (Exception ex)
            {
                GlobalData.IsGetAndSetTransfersDataComplete = true;
                Logger.LogException("UIM.GSTD", ex);
            }
        }

        public static async Task GetAndSetWalletHeight()
        {
            try
            {
                GetHeightResponse response = await GlobalData.WalletService.GetHeight(GlobalData.AppSettings.Wallet[GlobalData.AppSettings.ActiveCoin].Rpc, new GetHeightRequest());

                if (response.Error.IsError)
                {
                    Logger.LogError("UIM.GSWH", "GetHeight Error | Code: " + response.Error.Code + " | Message: " + response.Error.Message + " | Content: " + response.Error.Content);
                }
                else
                {
                    GlobalData.WalletHeight = response.Height;
                }
            }
            catch (Exception ex)
            {
                Logger.LogException("UIM.GSWH", ex);
            }
        }
        #endregion // Get Data for UI
    }
}
