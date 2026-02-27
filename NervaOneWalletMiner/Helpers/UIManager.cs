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

namespace NervaOneWalletMiner.Helpers
{
    public static class UIManager
    {
        public static bool _isTransfersUpdateComplete = true;
        public static bool _askedToQuickSync = false;

        public static readonly Bitmap _walletImage = new Bitmap(AssetLoader.Open(new Uri("avares://" + GlobalData.AppNameMain + "/Assets/wallet.png")));
        public static readonly Bitmap _inImage = new Bitmap(AssetLoader.Open(new Uri("avares://" + GlobalData.AppNameMain + "/Assets/transfer_in.png")));
        public static readonly Bitmap _outImage = new Bitmap(AssetLoader.Open(new Uri("avares://" + GlobalData.AppNameMain + "/Assets/transfer_out.png")));
        public static readonly Bitmap _blockImage = new Bitmap(AssetLoader.Open(new Uri("avares://" + GlobalData.AppNameMain + "/Assets/transfer_block.png")));
        public static readonly Bitmap _pendingImage = new Bitmap(AssetLoader.Open(new Uri("avares://" + GlobalData.AppNameMain + "/Assets/transfer_pending.png")));

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

            if (GlobalData.AppSettings.Daemon[GlobalData.AppSettings.ActiveCoin].IsWalletOnly)
            {
                ((MainViewModel)GlobalData.ViewModelPages[SplitViewPages.MainView]).CurrentPage = GlobalData.ViewModelPages[SplitViewPages.Wallet];
            }
            else
            {
                ((MainViewModel)GlobalData.ViewModelPages[SplitViewPages.MainView]).CurrentPage = GlobalData.ViewModelPages[SplitViewPages.Daemon];
            }            

            MasterProcess.StartMasterUpdateProcess();

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

        public static void UpdateDaemonStatus(string message)
        {
            if (GlobalData.ViewModelPages.ContainsKey(SplitViewPages.MainView))
            {
                if (((MainViewModel)GlobalData.ViewModelPages[SplitViewPages.MainView]).DaemonStatus != message)
                {
                    ((MainViewModel)GlobalData.ViewModelPages[SplitViewPages.MainView]).DaemonStatus = message;
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

        #region Actual UI Update
        public static void UpdateDaemonView()
        {
            try
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


                if (((DaemonViewModel)GlobalData.ViewModelPages[SplitViewPages.Daemon]).Connections.Count == 0)
                {
                    ObservableCollection<Connection> initialConnections = [.. GlobalData.NetworkStats.Connections.Values];

                    if (initialConnections.Count > 0)
                    {
                        Dispatcher.UIThread.Invoke(() =>
                        {
                            ((DaemonViewModel)GlobalData.ViewModelPages[SplitViewPages.Daemon]).Connections = [.. initialConnections];
                        });
                    }
                }
                else
                {
                // Trying to avoid loop within a loop
                List<Connection> deleteConnections = [];
                HashSet<string> checkedAddresses = [];


                    foreach (Connection connection in ((DaemonViewModel)GlobalData.ViewModelPages[SplitViewPages.Daemon]).Connections)
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
                            // Connections to remove
                            deleteConnections.Add(connection);
                        }
                    }

                    foreach (Connection connection in deleteConnections)
                    {
                        Dispatcher.UIThread.Invoke(() =>
                        {
                            ((DaemonViewModel)GlobalData.ViewModelPages[SplitViewPages.Daemon]).Connections.Remove(connection);
                        });
                    }

                    foreach (string key in GlobalData.NetworkStats.Connections.Keys)
                    {
                        if (!checkedAddresses.Contains(key))
                        {
                            // Need to add new wallet
                            Dispatcher.UIThread.Invoke(() =>
                            {
                                ((DaemonViewModel)GlobalData.ViewModelPages[SplitViewPages.Daemon]).Connections.Add(GlobalData.NetworkStats.Connections[key]);
                            });
                        }
                    }
                }

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
                if (GlobalData.AppSettings.Daemon[GlobalData.AppSettings.ActiveCoin].IsWalletOnly)
                {
                    UpdateDaemonStatus(GlobalData.NetworkStats.StatusSync);
                    UpdateDaemonVersion("Remote Node");
                }
                else
                {
                    UpdateDaemonStatus("Connections: " + GlobalData.NetworkStats.ConnectionsOut + "(out) + " + GlobalData.NetworkStats.ConnectionsIn + "(in)" + (string.IsNullOrEmpty(GlobalData.NetworkStats.StatusSync) ? "" : " | " + GlobalData.NetworkStats.StatusSync));
                    UpdateDaemonVersion(GlobalData.NetworkStats.Version.ToLower().StartsWith("v") ? GlobalData.NetworkStats.Version : "v: " + GlobalData.NetworkStats.Version);
                }
            }
            catch (Exception ex)
            {
                Logger.LogException("UIM.UPSB", ex);
            }
        }
        
        public static void UpdateWalletView()
        {
            try
            {
                if (GlobalData.IsWalletOpen)
                {
                    if (!((WalletViewModel)GlobalData.ViewModelPages[SplitViewPages.Wallet]).TotalCoins.Equals(GlobalData.WalletStats.BalanceTotal.ToString()))
                    {
                        ((WalletViewModel)GlobalData.ViewModelPages[SplitViewPages.Wallet]).TotalCoins = GlobalData.WalletStats.BalanceTotal.ToString();
                    }

                    if (!((WalletViewModel)GlobalData.ViewModelPages[SplitViewPages.Wallet]).UnlockedCoins.Equals(GlobalData.WalletStats.BalanceUnlocked.ToString()))
                    {
                        ((WalletViewModel)GlobalData.ViewModelPages[SplitViewPages.Wallet]).UnlockedCoins = GlobalData.WalletStats.BalanceUnlocked.ToString();
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
                        ObservableCollection<Account> initialAccounts = [.. GlobalData.WalletStats.Subaddresses.Values];

                        if (initialAccounts.Count > 0)
                        {
                            Dispatcher.UIThread.Invoke(() =>
                            {
                                ((WalletViewModel)GlobalData.ViewModelPages[SplitViewPages.Wallet]).WalletAddresses = [.. initialAccounts];
                            });
                        }
                    }
                    else
                    {
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
                                if (wallet.BalanceTotal != (GlobalData.WalletStats.Subaddresses[wallet.Index].BalanceTotal))
                                {
                                    wallet.BalanceTotal = GlobalData.WalletStats.Subaddresses[wallet.Index].BalanceTotal;
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
                            Dispatcher.UIThread.Invoke(() =>
                            {
                                ((WalletViewModel)GlobalData.ViewModelPages[SplitViewPages.Wallet]).WalletAddresses.Remove(wallet);
                            });
                        }

                        foreach (uint index in GlobalData.WalletStats.Subaddresses.Keys)
                        {
                            if (!checkedIndexes.Contains(index))
                            {
                                // Need to add new wallet
                                Dispatcher.UIThread.Invoke(() =>
                                {
                                    ((WalletViewModel)GlobalData.ViewModelPages[SplitViewPages.Wallet]).WalletAddresses.Add(GlobalData.WalletStats.Subaddresses[index]);
                                });
                            }
                        }
                    }

                    ((WalletViewModel)GlobalData.ViewModelPages[SplitViewPages.Wallet]).OpenCloseWallet = StatusWallet.CloseWallet;

                    // Status Bar
                    string statusBarMessage = GlobalData.OpenedWalletName + " | Account(s): " + ((WalletViewModel)GlobalData.ViewModelPages[SplitViewPages.Wallet]).WalletAddresses.Count + " | Balance: " + GlobalData.WalletStats.BalanceTotal + " " + GlobalData.AppSettings.Wallet[GlobalData.AppSettings.ActiveCoin].DisplayUnits + (GlobalData.CoinSettings[GlobalData.AppSettings.ActiveCoin].IsWalletHeightSupported ? " | Height: " + GlobalData.WalletHeight : string.Empty);
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
                        ((WalletViewModel)GlobalData.ViewModelPages[SplitViewPages.Wallet]).WalletAddresses = [];
                    }

                    ((WalletViewModel)GlobalData.ViewModelPages[SplitViewPages.Wallet]).OpenCloseWallet = StatusWallet.OpenWallet;

                    // Status Bar
                    if (((MainViewModel)GlobalData.ViewModelPages[SplitViewPages.MainView]).WalletStatus != GlobalData.WalletClosedMessage)
                    {
                        ((MainViewModel)GlobalData.ViewModelPages[SplitViewPages.MainView]).WalletStatus = GlobalData.WalletClosedMessage;
                    }
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
                if (GlobalData.IsWalletOpen)
                {
                    if (((TransfersViewModel)GlobalData.ViewModelPages[SplitViewPages.Transfers]).Transactions.Count == 0)
                    {
                        ObservableCollection<Transfer> initialTransfers = [.. GlobalData.TransfersStats.Transactions.Values];

                        if (initialTransfers.Count > 0)
                        {
                            Dispatcher.UIThread.Invoke(() =>
                            {
                                ((TransfersViewModel)GlobalData.ViewModelPages[SplitViewPages.Transfers]).Transactions = [.. initialTransfers.OrderByDescending(t => t.Height)];
                            });

                            // Need to clear transfers AFTER we process them otherwise we might clear them before we process them
                            GlobalData.TransfersStats.Transactions = [];
                        }

                        if (((TransfersViewModel)GlobalData.ViewModelPages[SplitViewPages.Transfers]).Transactions.Count > 0)
                        {
                            // This will also save after initial open BUT it will cover restoring wallet
                            newTransfersCount = ((TransfersViewModel)GlobalData.ViewModelPages[SplitViewPages.Transfers]).Transactions.Count;
                        }
                    }
                    else
                    {
                        List<Transfer> newTransfers = [];

                        foreach (string newTransferKey in GlobalData.TransfersStats.Transactions.Keys)
                        {
                            // Check if transaction already exists in datagrid and add it to the top if it does not
                            bool isFound = false;
                            foreach (Transfer transfer in ((TransfersViewModel)GlobalData.ViewModelPages[SplitViewPages.Transfers]).Transactions)
                            {
                                if (transfer.TransactionId + transfer.Type == newTransferKey)
                                {
                                    isFound = true;
                                    break;
                                }
                            }

                            if (!isFound)
                            {
                                newTransfers.Add(GlobalData.TransfersStats.Transactions[newTransferKey]);
                            }
                        }

                        if (newTransfers.Count > 0)
                        {
                            foreach (Transfer transfer in newTransfers.OrderBy(t => t.Height))
                            {
                                Dispatcher.UIThread.Invoke(() =>
                                {
                                    ((TransfersViewModel)GlobalData.ViewModelPages[SplitViewPages.Transfers]).Transactions.Insert(0, transfer);
                                });
                            }
                            
                            // Need to clear transfers AFTER we process them otherwise we might clear them before we process them
                            GlobalData.TransfersStats.Transactions = [];
                            newTransfersCount = newTransfers.Count;
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
                    // If wallet is closed/was closed by the user, clear fields
                    if (((TransfersViewModel)GlobalData.ViewModelPages[SplitViewPages.Transfers]).Transactions.Count != 0)
                    {
                        ((TransfersViewModel)GlobalData.ViewModelPages[SplitViewPages.Transfers]).Transactions = [];
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
                if (GlobalData.IsCliToolsDownloading)
                {
                    GlobalData.NetworkStats = new()
                    {
                        StatusSync = "Downloading client tools. Please wait...",
                        Connections = []
                    };
                }
                else if (!GlobalData.IsCliToolsFound)
                {
                    GlobalData.NetworkStats = new()
                    {
                        StatusSync = "Client tools missing.",
                        Connections = []
                    };
                }
                else if(GlobalData.AppSettings.Daemon[GlobalData.AppSettings.ActiveCoin].IsWalletOnly)
                {
                    GlobalData.NetworkStats = new()
                    {
                        StatusSync = "Using " + GlobalData.AppSettings.Wallet[GlobalData.AppSettings.ActiveCoin].PublicNodeAddress,
                        Connections = []
                    };
                }
                else if (GlobalData.IsDaemonRestarting)
                {
                    GlobalData.NetworkStats = new()
                    {
                        StatusSync = "Restarting daemon...",
                        Connections = []
                    };
                }
                else if (!GlobalData.IsInitialDaemonConnectionSuccess)
                {
                    GlobalData.NetworkStats = new()
                    {
                        StatusSync = "Establishing connection with daemon...",
                        Connections = []
                    };
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

                        // Used for Connections Guard
                        if(GlobalData.NetworkStats.ConnectionsIn + GlobalData.NetworkStats.ConnectionsOut >= GlobalData.ConnectGuardMinimumGoodCount)
                        {
                            GlobalData.ConnectGuardLastGoodTime = DateTime.Now;
                        }

                        GlobalData.NetworkStats.Version = infoRes.Version;
                        GlobalData.NetworkStats.StatusSync = "";
                        if (infoRes.TargetHeight != 0 && infoRes.Height < infoRes.TargetHeight)
                        {
                            GlobalData.NetworkStats.StatusSync += "Sync (Height " + infoRes.Height + " of " + infoRes.TargetHeight + ")";

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
                            GlobalData.NetworkStats.StatusSync += "Sync OK | Status " + infoRes.Status;
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
                        if (!connectResp.Error.IsError)
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

                    GlobalData.IsGetAndSetDaemonDataComplete = true;
                }
            }
            catch (Exception ex)
            {
                GlobalData.IsGetAndSetDaemonDataComplete = true;
                Logger.LogException("UIM.GSDD", ex);
            }
        }

        public static async void GetAndSetWalletData()
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

                        GlobalData.WalletStats.Subaddresses = [];

                        foreach (Account account in response.SubAccounts)
                        {
                            if (!GlobalData.WalletStats.Subaddresses.ContainsKey(account.Index))
                            {
                                account.WalletIcon = _walletImage;
                                GlobalData.WalletStats.Subaddresses.Add(account.Index, account);
                            }
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

        public static async void GetAndSetTransfersData()
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
                        foreach (Transfer transfer in response.Transfers)
                        {
                            if (!GlobalData.TransfersStats.Transactions.ContainsKey(transfer.TransactionId + transfer.Type))
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

                                GlobalData.TransfersStats.Transactions.Add(transfer.TransactionId + transfer.Type, transfer);

                                if (transfer.Height > GlobalData.NewestTransactionHeight)
                                {
                                    GlobalData.NewestTransactionHeight = transfer.Height;
                                }

                                if (transfer.BlockHash.Equals(GlobalData.NewestTransactionBlockHash))
                                {
                                    GlobalData.NewestTransactionBlockHash = transfer.BlockHash;
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
        #endregion // Get Data for UI
    }
}