using Avalonia.Controls;
using Avalonia.Media.Imaging;
using NervaOneWalletMiner.Objects.Constants;
using NervaOneWalletMiner.Objects.DataGrid;
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
        public const string AppName = "NervaOneWalletMiner";
        public const string Version = "0.7.3.0";

        public const string CliToolsDirName = "cli";
        public const string WalletDirName = "wallets";
        public const string LogsDirName = "logs";
        public const string MainCoinsDirName = "coins";
        public const string AppConfigFileName = "app.config";
        public const string AddressBookFileName = "address.book";
        public static string CoinDirName = Coin.XNV;

        public static bool IsConfigFound = false;

        public const string NervaOneGitHubLink = "https://github.com/nerva-project/NervaOneWalletMiner";
        public const string NervaDiscordLink = "https://discord.gg/KxjqZytC5Y";
        public const string NervaTelegramLink = "https://t.me/NervaCrypto";

        public static Random RandomGenerator = new Random();

        // Those will be saved to and read from app.config
        public static ApplicationSettings AppSettings = new();        

        public static Dictionary<string, ICoinSettings> CoinSettings = GlobalMethods.GetDefaultCoinSettings();

        public static readonly string DataDir = GlobalMethods.GetDataDir();
        public static readonly string LogDir = GlobalMethods.GetLogDir();
        public static readonly string ConfigFileNameWithPath = GlobalMethods.GetConfigFileNameWithPath();

        public static Dictionary<string, ViewModelBase> ViewModelPages = [];
        public static bool IsInitialDaemonConnectionSuccess = false;
        public static DateTime LastDaemonResponseTime = DateTime.Now;

        public static bool IsWalletOpen = false;
        public static bool IsWalletJustOpened = true;
        public static string OpenedWalletName = string.Empty;

        public static StatsDaemon NetworkStats = new();
        public static StatsWallet WalletStats = new();
        public static StatsTransfers TransfersStats = new();
        public static List<Connection> Connections = [];
        public static AddressBook AddressBook = new();

        public static Dictionary<string, Bitmap> CoinLogoDictionary = GlobalMethods.LoadCoinLogos();
        public static Dictionary<string, WindowIcon> WindowIconsDictionary = GlobalMethods.LoadCoinWindowIcons();

        public static bool IsDaemonRestarting = false;
        public static bool IsManualStopMining = false;

        public static ulong NewestTransactionHeight = 0;
        public static ulong WalletHeight = 0;

        public static int CpuThreadCount = Environment.ProcessorCount;

        // Coin specific
        public static string CliToolsDir = GlobalMethods.GetCliToolsDir();
        public static string WalletDir = GlobalMethods.GetWalletDir();

        public static IWalletService WalletService = new WalletServiceXNV();
        public static IDaemonService DaemonService = new DaemonServiceXNV();

        public static string WalletProcessName = GlobalMethods.GetWalletProcessName();
        public static string DaemonProcessName = GlobalMethods.GetDaemonProcessName();

        public static Bitmap Logo = GlobalMethods.GetLogo();

        public static string WalletClosedMessage = "Wallet offline - see Wallet screen to open";

        // Views reaload when you switch pages so you will have the same event registered many times. This will prevent it
        public static bool AreWalletEventsRegistered = false;
        public static bool AreDaemonEventsRegistered = false;
    }
}