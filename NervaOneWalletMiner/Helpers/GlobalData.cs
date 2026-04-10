using Avalonia.Controls;
using Avalonia.Media.Imaging;
using NervaOneWalletMiner.Objects.Constants;
using NervaOneWalletMiner.Objects.Settings;
using NervaOneWalletMiner.Objects.Settings.CoinSpecific;
using NervaOneWalletMiner.Objects.Stats;
using NervaOneWalletMiner.Rpc.Daemon;
using NervaOneWalletMiner.Rpc.Wallet;
using NervaOneWalletMiner.ViewModels;
using System;
using System.Collections.Generic;

namespace NervaOneWalletMiner.Helpers
{
    public static class GlobalData
    {
        public const string AppNameMain = "NervaOne";
        public const string AppNameDesktop = "NervaOneDesktop";
        public const string Version = "0.8.5.3";

        public const string CliToolsDirName = "cli";
        public const string WalletDirName = "wallets";
        public const string LogsDirName = "logs";        
        public const string MainCoinsDirName = "coins";
        public const string ExportDirName = "exports";

        public const string AppConfigFileName = "app.config";
        public const string AddressBookFileName = "address.book";
        public const string WalletDumpFileName = "dump.txt";

        public static string WalletClosedMessage = "Wallet offline - open via Wallet screen";
        public static string CoinDirName = Coin.XNV;
        public static int CpuThreadCount = Environment.ProcessorCount;
               
        // Used for About page
        public const string NervaOneGitHubLink = "https://github.com/nerva-project/NervaOneWalletMiner";
        public const string NervaDiscordLink = "https://discord.gg/KxjqZytC5Y";
        public const string NervaTelegramLink = "https://t.me/NervaCrypto";

        public static Dictionary<string, ViewModelBase> ViewModelPages = [];
        
        public static Dictionary<string, Bitmap> CoinLogoDictionary = GlobalMethods.LoadCoinLogos();
        public static Dictionary<string, WindowIcon> WindowIconsDictionary = GlobalMethods.LoadCoinWindowIcons();

        public static Random RandomGenerator = new();

        // Those will be saved to and read from app.config
        public static ApplicationSettings AppSettings = new();
        public static volatile bool IsConfigFound = false;

        public static IWalletService WalletService = new WalletServiceXNV();
        public static IDaemonService DaemonService = new DaemonServiceXNV();
        public static Dictionary<string, ICoinSettings> CoinSettings = GlobalMethods.GetDefaultCoinSettings();

        public static readonly string MainDataDir = GlobalMethods.GetDataDir();
        public static readonly string AppDataDir = GlobalMethods.GetAppDataDir();
        public static readonly string LogsDir = GlobalMethods.GetLogsDir();
        public static readonly string ExportsDir = GlobalMethods.GetExportsDir();
        public static readonly string ConfigFileNameWithPath = GlobalMethods.GetConfigFileNameWithPath();

        public static volatile bool IsDaemonRestarting = false;
        public static volatile bool IsInitialDaemonConnectionSuccess = false;
        public static DateTime LastDaemonResponseTime = DateTime.MinValue;
        public static DateTime LastDaemonRestartAttempt = DateTime.MinValue;

        public static volatile bool IsManualStoppedMining = false;
        public static volatile bool IsNoConnectionsStoppedMining = false;
        public static volatile bool IsHashRateMonitoringStoppedMining = false;

        public static volatile bool IsWalletOpen = false;
        public static volatile bool IsWalletJustOpened = true;
        public static string OpenedWalletName = string.Empty;
        public static DateTime WalletPassProvidedTime = DateTime.MinValue;
        
        // This is used by UI to control if user needs to enter password before they do some sensitive wallet task. It stores password Hash
        public static string WalletPasswordHash = string.Empty;

        // Grid and other UI data
        public static StatsDaemon NetworkStats = new();
        public static StatsWallet WalletStats = new();
        public static StatsTransfers TransfersStats = new();
        public static AddressBook AddressBook = new();

        public static volatile bool IsWalletUpdateComplete = true;
        public static volatile bool IsGetAndSetTransfersDataComplete = true;
        public static volatile bool IsGetAndSetWalletDataComplete = true;
        public static volatile bool IsGetAndSetDaemonDataComplete = true;

        public static volatile bool IsCliToolsFound = true;
        public static volatile bool IsCliToolsDownloading = false;

        // Connections Guard
        public static DateTime ConnectGuardLastGoodTime = DateTime.Now;
        public static int ConnectGuardRestartCount = 1;
        public static ulong ConnectGuardMinimumGoodCount = 2;
        public static int ConnectGuardMinutes = 10;
        public static int ConnectGuardBlocksToPop = 1000;        

        public static string CliToolsDir = GlobalMethods.GetCliToolsDir();
        public static string WalletDir = GlobalMethods.GetWalletDir();

        public static ulong NewestTransactionHeight = 0;
        public static string NewestTransactionBlockHash = string.Empty;
        public static ulong WalletHeight = 0;
       
        public static string WalletProcessName = GlobalMethods.GetWalletProcessName(AppSettings.ActiveCoin);
        public static string DaemonProcessName = GlobalMethods.GetDaemonProcessName(AppSettings.ActiveCoin);

        public static Bitmap Logo = GlobalMethods.GetLogo();
        
        // Views reaload when you switch pages so you will have the same event registered many times. This will prevent it
        public static volatile bool AreWalletEventsRegistered = false;
        public static volatile bool AreDaemonEventsRegistered = false;
    }
}