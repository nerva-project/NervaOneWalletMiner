using Avalonia.Media.Imaging;
using NervaOneWalletMiner.Objects.Constants;
using NervaOneWalletMiner.Objects.DataGrid;
using NervaOneWalletMiner.Objects.Settings;
using NervaOneWalletMiner.Objects.Settings.CoinSpecific;
using NervaOneWalletMiner.Objects.Stats;
using NervaOneWalletMiner.Rpc.Daemon;
using NervaOneWalletMiner.Rpc.Daemon.Downloads;
using NervaOneWalletMiner.Rpc.Wallet;
using System;
using System.Collections.Generic;

namespace NervaOneWalletMiner.Helpers
{
    public static class GlobalData
    {
        public const string AppName = "NervaOneWalletMiner";
        public const string Version = "0.5.0";

        public const string CliToolsDirName = "cli";
        public const string WalletDirName = "wallets";
        public const string LogsDirName = "logs";
        public const string MainCoinsDirName = "coins";
        public static string CoinDirName = Coin.XNV;

        public static Random RandomGenerator = new Random();

        // Those will be saved to and read from app.config
        public static ApplicationSettings AppSettings = new();

        public static Dictionary<string, ISettingsDefault> DefaultSettings = GlobalMethods.GetDefaultSettings();

        public static readonly string DataDir = GlobalMethods.GetDataDir();
        public static readonly string LogDir = GlobalMethods.GetLogDir();
        public static readonly string ConfigFilePath = GlobalMethods.GetConfigFilePath();

        public static bool IsWalletOpen = false;
        public static bool IsWalletJustOpened = true;
        public static string OpenedWalletName = string.Empty;

        public static StatsDaemon NetworkStats = new();
        public static StatsWallet WalletStats = new();
        public static StatsTransfers TransfersStats = new();
        public static List<Connection> Connections = new List<Connection>();

        public static ulong NewestTransactionHeight = 0;
        public static ulong WalletHeight = 0;

        public static int CpuThreadCount = Environment.ProcessorCount;

        // Coin specific
        public static string CliToolsDir = GlobalMethods.GetCliToolsDir();
        public static string WalletDir = GlobalMethods.GetWalletDir();

        public static IWalletService WalletService = new WalletServiceXNV();
        public static IDaemonService DaemonService = new DaemonServiceXNV();
        public static IDownload DownloadLinks = new DownloadXNV();

        public static string WalletProcessName = GlobalMethods.GetWalletProcessName();
        public static string DaemonProcessName = GlobalMethods.GetDaemonProcessName();

        public static Bitmap Logo = GlobalMethods.GetLogo();

        public static string WalletClosedMessage = "Wallet offline - see Wallet screen to open";
    }
}