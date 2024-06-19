using Avalonia.Media.Imaging;
using Avalonia.Platform;
using NervaOneWalletMiner.Objects.Constants;
using NervaOneWalletMiner.Objects.Settings;
using NervaOneWalletMiner.Objects.Settings.CoinSpecific;
using NervaOneWalletMiner.Rpc.Daemon;
using NervaOneWalletMiner.Rpc.Wallet;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Http;
using NervaOneWalletMiner.Rpc.Wallet.Requests;
using NervaOneWalletMiner.Rpc.Wallet.Responses;
using NervaOneWalletMiner.Objects.DataGrid;
using Avalonia.Input;
using Avalonia.Controls;
using NervaOneWalletMiner.Objects.Stats;
using NervaOneWalletMiner.Rpc.Daemon.Requests;

namespace NervaOneWalletMiner.Helpers
{
    public static class GlobalMethods
    {
        public static readonly Bitmap _walletImage = new Bitmap(AssetLoader.Open(new Uri("avares://NervaOneWalletMiner/Assets/wallet.png")));

        #region Directories, Paths and Names
        public static string GetDataDir()
        {
            string dataDir = string.Empty;

            try
            {
                if (IsWindows())
                {
                    dataDir = Environment.GetEnvironmentVariable("APPDATA")!;
                    if (dataDir == null)
                    {
                        throw new DirectoryNotFoundException("Windows dir not found");
                    }
                }
                else
                {
                    dataDir = Environment.GetEnvironmentVariable("HOME")!;
                    if (dataDir == null)
                    {
                        throw new DirectoryNotFoundException("Non-Windows dir not found");
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogException("GLM.GDDR", ex);
            }

            return dataDir;
        }

        public static string GetAppDataDir()
        {
            string appDataDir = string.Empty;

            try
            {
                appDataDir = Path.Combine(GetDataDir(), GlobalData.AppName);

                // Create data directory if it does not exist
                if (!Directory.Exists(appDataDir))
                {
                    Directory.CreateDirectory(appDataDir);
                }

                if (!Directory.Exists(appDataDir))
                {
                    throw new Exception("Data directory not set up. Application cannot continue");
                }                
            }
            catch (Exception ex)
            {
                Logger.LogException("GLM.GDDR", ex);
            }

            return appDataDir;
        }

        public static string GetLogDir()
        {
            string logDirectory = string.Empty;

            try
            {
                if (Directory.Exists(GlobalData.AppDataDir))
                {
                    // Create logs directory if it does not exist
                    logDirectory = Path.Combine(GlobalData.AppDataDir, GlobalData.LogsDirName);
                    if (!Directory.Exists(logDirectory))
                    {
                        Directory.CreateDirectory(logDirectory);
                    }
                }
                else
                {
                    throw new Exception("Logs directory not set up. Application cannot continue");
                }
            }
            catch (Exception ex)
            {
                Logger.LogException("GLM.GLDR", ex);
            }
            
            return logDirectory;
        }

        public static string GetWalletDir()
        {
            string walletDirectory = string.Empty;

            try
            {
                if (Directory.Exists(GlobalData.AppDataDir))
                {
                    // Create wallet directory if it does not exist
                    walletDirectory = Path.Combine(GlobalData.AppDataDir, GlobalData.MainCoinsDirName, GlobalData.CoinDirName, GlobalData.WalletDirName);
                    if (!Directory.Exists(walletDirectory))
                    {
                        Directory.CreateDirectory(walletDirectory);
                    }
                }
                else
                {
                    throw new Exception("Wallet directory not set up. Application cannot continue");
                }
            }
            catch (Exception ex)
            {
                Logger.LogException("GLM.GWDR", ex);
            }
            
            return walletDirectory;
        }

        public static string GetCliToolsDir()
        {
            string cliToolsDirectory = string.Empty;

            try
            {
                if (Directory.Exists(GlobalData.AppDataDir))
                {
                    // Create client tools directory if it does not exist
                    cliToolsDirectory = Path.Combine(GlobalData.AppDataDir, GlobalData.MainCoinsDirName, GlobalData.CoinDirName, GlobalData.CliToolsDirName);
                    if (!Directory.Exists(cliToolsDirectory))
                    {
                        Directory.CreateDirectory(cliToolsDirectory);
                    }
                }
                else
                {
                    throw new Exception("Client tools directory not set up. Application cannot continue");
                }
            }
            catch (Exception ex)
            {
                Logger.LogException("GLM.GCTD", ex);
            }

            return cliToolsDirectory;
        }

        public static string GetDaemonProcess()
        {
            return Path.Combine(GlobalData.CliToolsDir, GlobalData.DaemonProcessName);
        }

        public static string GetRpcWalletProcess()
        {
            return Path.Combine(GlobalData.CliToolsDir, GlobalData.WalletProcessName);
        }

        public static string GetConfigFileNameWithPath()
        {           
            string dataDir = GetAppDataDir();
            return Path.Combine(dataDir, GlobalData.AppConfigFileName);
        }
        #endregion // Directories, Paths and Names

        #region Coin Specific Setup
        public static Dictionary<string, ICoinSettings> GetDefaultCoinSettings()
        {
            Dictionary<string, ICoinSettings> defaultSettings = [];

            try
            {
                defaultSettings.Add(Coin.XNV, new CoinSettingsXNV());
                defaultSettings.Add(Coin.XMR, new CoinSettingsXMR());
                defaultSettings.Add(Coin.DASH, new CoinSettingsDASH());
            }
            catch (Exception ex)
            {
                Logger.LogException("GLM.GDCS", ex);
            }

            return defaultSettings;
        }

        public static void SetDefaultCoinSpecificSettings()
        {
            if(GlobalData.CoinSettings == null || GlobalData.CoinSettings.Count == 0)
            {
                GlobalData.CoinSettings = GetDefaultCoinSettings();
            }

            try
            {
                // Overwrite default or invalid settings with coin specific ones
                foreach(string coin in GlobalData.CoinSettings.Keys)
                {
                    if (!GlobalData.AppSettings.Daemon.ContainsKey(coin))
                    {
                        GlobalData.AppSettings.Daemon.Add(coin, new SettingsDaemon());
                    }
                    if (!GlobalData.AppSettings.Wallet.ContainsKey(coin))
                    {
                        GlobalData.AppSettings.Wallet.Add(coin, new SettingsWallet());
                    }

                    // Daemon
                    if (GlobalData.AppSettings.Daemon[coin].Rpc.Port < 0)
                    {
                        GlobalData.AppSettings.Daemon[coin].Rpc.Port = GlobalData.CoinSettings[coin].DaemonPort;
                    }

                    if (GlobalData.AppSettings.Daemon[coin].LogLevel < 0)
                    {
                        GlobalData.AppSettings.Daemon[coin].LogLevel = GlobalData.CoinSettings[coin].LogLevelDaemon;
                    }

                    if (string.IsNullOrEmpty(GlobalData.AppSettings.Daemon[coin].DataDir))
                    {
                        GlobalData.AppSettings.Daemon[coin].DataDir = GetDefaultDataDir(coin);
                    }

                    // Wallet
                    if (string.IsNullOrEmpty(GlobalData.AppSettings.Wallet[coin].DisplayUnits))
                    {
                        GlobalData.AppSettings.Wallet[coin].DisplayUnits = GlobalData.CoinSettings[coin].DisplayUnits;
                    }

                    if (GlobalData.AppSettings.Wallet[coin].LogLevel < 0)
                    {
                        GlobalData.AppSettings.Wallet[coin].LogLevel = GlobalData.CoinSettings[coin].LogLevelWallet;
                    }
                    
                    if(GlobalData.CoinSettings[coin].IsDaemonWalletSeparateApp)
                    {
                        // Always generate new port
                        GlobalData.AppSettings.Wallet[coin].Rpc.Port = GlobalData.RandomGenerator.Next(10000, 50000);
                    }
                    else
                    {
                        GlobalData.AppSettings.Wallet[coin].Rpc.Port = GlobalData.CoinSettings[coin].DaemonPort;
                    }                    
                }
            }
            catch (Exception ex)
            {
                Logger.LogException("GLM.SDCS", ex);
            }
        }

        public static void SetCoin(string newCoin)
        {
            try
            {
                Logger.LogDebug("GLM.STCN", "Setting up: " + newCoin.ToUpper());

                switch (newCoin)
                {
                    case Coin.XMR:                        
                        GlobalData.CoinDirName = Coin.XMR;
                        GlobalData.AppSettings.ActiveCoin = Coin.XMR;

                        GlobalData.DaemonService = new DaemonServiceXMR();
                        GlobalData.WalletService = new WalletServiceXMR();
                        break;

                    case Coin.DASH:                        
                        GlobalData.CoinDirName = Coin.DASH;
                        GlobalData.AppSettings.ActiveCoin = Coin.DASH;
                        
                        GlobalData.DaemonService = new DaemonServiceDASH();
                        GlobalData.WalletService = new WalletServiceDASH();

                        if (string.IsNullOrEmpty(GlobalData.AppSettings.Daemon[GlobalData.AppSettings.ActiveCoin].Rpc.UserName))
                        {
                            GlobalData.AppSettings.Daemon[GlobalData.AppSettings.ActiveCoin].Rpc.UserName = GenerateRandomString(24);
                            GlobalData.AppSettings.Daemon[GlobalData.AppSettings.ActiveCoin].Rpc.Password = GenerateRandomString(24);
                        }

                        if (string.IsNullOrEmpty(GlobalData.AppSettings.Wallet[GlobalData.AppSettings.ActiveCoin].Rpc.UserName))
                        {
                            GlobalData.AppSettings.Wallet[GlobalData.AppSettings.ActiveCoin].Rpc.UserName = GlobalData.AppSettings.Daemon[GlobalData.AppSettings.ActiveCoin].Rpc.UserName;
                            GlobalData.AppSettings.Wallet[GlobalData.AppSettings.ActiveCoin].Rpc.Password = GlobalData.AppSettings.Daemon[GlobalData.AppSettings.ActiveCoin].Rpc.Password;
                        }
                        break;

                    default:
                        // XNV or anything else not supported
                        GlobalData.CoinDirName = Coin.XNV;
                        GlobalData.AppSettings.ActiveCoin = Coin.XNV;

                        GlobalData.DaemonService = new DaemonServiceXNV();
                        GlobalData.WalletService = new WalletServiceXNV();
                        break;
                }
                
                GlobalData.CliToolsDir = GetCliToolsDir();
                GlobalData.WalletDir = GetWalletDir();

                GlobalData.WalletProcessName = GetWalletProcessName(GlobalData.AppSettings.ActiveCoin);
                GlobalData.DaemonProcessName = GetDaemonProcessName(GlobalData.AppSettings.ActiveCoin);
                GlobalData.Logo = GetLogo();

                Logger.LogDebug("GLM.STCN", "Finished setting up: " + GlobalData.AppSettings.ActiveCoin.ToUpper());
            }
            catch (Exception ex)
            {
                Logger.LogException("GLM.STCN", ex);
            }            
        }

        public static string GetCliToolsDownloadLink(string coin)
        {
            string cliDownloadLink = string.Empty;

            try
            {
                Architecture arch = GetCpuArchitecture();

                if (IsLinux())
                {
                    switch (arch)
                    {
                        case Architecture.X64:
                            cliDownloadLink = GlobalData.CoinSettings[coin].CliLin64Url;
                            break;
                        case Architecture.Arm:
                        case Architecture.Arm64:
                            cliDownloadLink = GlobalData.CoinSettings[coin].CliLinArmUrl;
                            break;
                        default:
                            cliDownloadLink = GlobalData.CoinSettings[coin].CliLin32Url;
                            break;
                    }
                }
                else if (IsOsx())
                {
                    switch (arch)
                    {
                        case Architecture.Arm:
                        case Architecture.Arm64:
                            cliDownloadLink = GlobalData.CoinSettings[coin].CliMacArmUrl;
                            break;
                        default:
                            cliDownloadLink = GlobalData.CoinSettings[coin].CliMacIntelUrl;
                            break;
                    }
                }
                else
                {
                    switch (arch)
                    {
                        case Architecture.X64:
                            cliDownloadLink = GlobalData.CoinSettings[coin].CliWin64Url;
                            break;
                        default:
                            cliDownloadLink = GlobalData.CoinSettings[coin].CliWin32Url;
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogException("GLM.GCDL", ex);
            }
            
            return cliDownloadLink;
        }

        public static string GetDefaultDataDir(string coin)
        {
            string defaultDataDir = string.Empty;

            try
            {
                Architecture arch = GetCpuArchitecture();

                if (IsLinux())
                {
                    defaultDataDir = GlobalData.CoinSettings[coin].DataDirLin;
                }
                else if (IsOsx())
                {
                    defaultDataDir = GlobalData.CoinSettings[coin].DataDirMac;
                }
                else
                {
                    defaultDataDir = GlobalData.CoinSettings[coin].DataDirWin;
                }
            }
            catch (Exception ex)
            {
                Logger.LogException("GLM.GDCD", ex);
            }

            return defaultDataDir;
        }

        public static async void SetUpCliTools(string downloadUrl, string cliToolsPath)
        {
            try
            {
                // Check if we already downloaded the CLI package
                string destFileWithPath = Path.Combine(cliToolsPath, Path.GetFileName(downloadUrl));

                if (File.Exists(destFileWithPath))
                {
                    Logger.LogDebug("GLM.SUCT", "Extracting existing CLI tools: " + destFileWithPath);

                    ExtractFile(cliToolsPath, destFileWithPath);
                }
                else
                {
                    Logger.LogDebug("GLM.SUCT", "Downloading CLI tools. URL: " + downloadUrl);

                    bool isSuccess = await DownloadFileToFolder(downloadUrl, cliToolsPath);

                    if (isSuccess)
                    {
                        Logger.LogDebug("GLM.SUCT", "Extracting CLI tools after download: " + destFileWithPath);
                        ExtractFile(cliToolsPath, destFileWithPath);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogException("GLM.SUCT", ex);
            }            
        }

        public static async Task<bool> DownloadFileToFolder(string downloadUrl, string destinationDir)
        {
            bool isSuccess = false;

            try
            {
                if (!Directory.Exists(destinationDir))
                {
                    Directory.CreateDirectory(destinationDir);
                }

                string destFile = Path.Combine(destinationDir, Path.GetFileName(downloadUrl));

                Logger.LogDebug("GLM.DFTF", "Downloading file: " + downloadUrl + " to: " + destFile);
                using (HttpClient client = new())
                {
                    using (var clientStream = await client.GetStreamAsync(downloadUrl))
                    {
                        using (var fileStream = new FileStream(destFile, FileMode.Create))
                        {
                            await clientStream.CopyToAsync(fileStream);
                            Logger.LogDebug("GLM.DFTF", "Setting success for: " + downloadUrl);
                            isSuccess = true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogException("GLM.DFTF", ex);
            }

            return isSuccess;
        }

        private static void ExtractFile(string destDir, string destFile)
        {
            try
            {                
                while (ProcessManager.IsRunning(GlobalData.DaemonProcessName))
                {
                    Logger.LogDebug("GLM.EXFL", "Closing Daemon process");
                    ProcessManager.Kill(GlobalData.DaemonProcessName);
                    Thread.Sleep(1000);
                }

                while (ProcessManager.IsRunning(GlobalData.WalletProcessName))
                {
                    Logger.LogDebug("GLM.EXFL", "Closing Wallet process");
                    ProcessManager.Kill(GlobalData.WalletProcessName);
                    Thread.Sleep(1000);
                }

                Logger.LogDebug("GLM.EXFL", "Extracting CLI tools");

                ZipArchive archive = ZipFile.Open(destFile, ZipArchiveMode.Read);
                foreach (var entry in archive.Entries)
                {
                    if (!string.IsNullOrEmpty(entry.Name))
                    {
                        Logger.LogDebug("GLM.EXFL", "Extracting: " + entry.Name);
                        string extFile = Path.Combine(destDir, entry.Name);
                        entry.ExtractToFile(extFile, true);
#if UNIX
                        UnixNative.Chmod(extFile, 33261);
#endif
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogException("GLM.EXFL", ex);
            }
        }

        public static string GetDaemonProcessName(string coin)
        {
            string daemonProcess = string.Empty;

            try
            {
                switch (coin)
                {
                    case Coin.XMR:
                        daemonProcess = GlobalMethods.IsWindows() ? "monerod.exe" : "monerod";
                        break;
                    case Coin.DASH:
                        daemonProcess = GlobalMethods.IsWindows() ? "dashd.exe" : "dashd";
                        break;
                    default:
                        // XNV or anything else not supported
                        daemonProcess = GlobalMethods.IsWindows() ? "nervad.exe" : "nervad";
                        break;
                }
            }
            catch (Exception ex)
            {
                Logger.LogException("GLM.GDPN", ex);
            }

            return daemonProcess;
        }

        public static string GetWalletProcessName(string coin)
        {
            string walletProcess = string.Empty;

            try
            {
                switch (coin)
                {
                    case Coin.XMR:
                        walletProcess = IsWindows() ? "monero-wallet-rpc.exe" : "monero-wallet-rpc";
                        break;
                    case Coin.DASH:
                        walletProcess = IsWindows() ? "dashd.exe" : "dashd";
                        break;
                    default:
                        // XNV or anything else not supported
                        walletProcess = IsWindows() ? "nerva-wallet-rpc.exe" : "nerva-wallet-rpc";
                        break;
                }
            }
            catch (Exception ex)
            {
                Logger.LogException("GLM.GWPN", ex);
            }

            return walletProcess;
        }

        public static Dictionary<string, Bitmap> LoadCoinLogos()
        {
            Dictionary<string, Bitmap> logoDictionary = [];

            try
            {
                if (!logoDictionary.ContainsKey(Coin.XNV))
                {
                    logoDictionary.Add(Coin.XNV, new Bitmap(AssetLoader.Open(new Uri("avares://NervaOneWalletMiner/Assets/xnv/logo.png"))));
                }

                if (!logoDictionary.ContainsKey(Coin.XMR))
                {
                    logoDictionary.Add(Coin.XMR, new Bitmap(AssetLoader.Open(new Uri("avares://NervaOneWalletMiner/Assets/xmr/logo.png"))));
                }

                if (!logoDictionary.ContainsKey(Coin.DASH))
                {
                    logoDictionary.Add(Coin.DASH, new Bitmap(AssetLoader.Open(new Uri("avares://NervaOneWalletMiner/Assets/dash/logo.png"))));
                }
            }
            catch (Exception ex)
            {
                Logger.LogException("GLM.LCLO", ex);
            }

            return logoDictionary;
        }

        public static Bitmap GetLogo()
        {
            return GlobalData.CoinLogoDictionary[GlobalData.AppSettings.ActiveCoin];        
        }

        public static Dictionary<string, WindowIcon> LoadCoinWindowIcons()
        {
            Dictionary<string, WindowIcon> iconDictionary = [];

            try
            {
                if (!iconDictionary.ContainsKey(Coin.XNV))
                {
                    iconDictionary.Add(Coin.XNV, new WindowIcon(AssetLoader.Open(new Uri("avares://NervaOneWalletMiner/Assets/xnv/logo.png"))));
                }

                if (!iconDictionary.ContainsKey(Coin.XMR))
                {
                    iconDictionary.Add(Coin.XMR, new WindowIcon(AssetLoader.Open(new Uri("avares://NervaOneWalletMiner/Assets/xmr/logo.png"))));
                }

                if (!iconDictionary.ContainsKey(Coin.DASH))
                {
                    iconDictionary.Add(Coin.DASH, new WindowIcon(AssetLoader.Open(new Uri("avares://NervaOneWalletMiner/Assets/dash/logo.png"))));
                }
            }
            catch (Exception ex)
            {
                Logger.LogException("GLM.LCWI", ex);
            }

            return iconDictionary;
        }

        public static WindowIcon GetWindowIcon()
        {
            return GlobalData.WindowIconsDictionary[GlobalData.AppSettings.ActiveCoin];
        }
        #endregion // Coin Specific Setup

        public static string GetShorterString(string? text, int shorterLength)
        {
            if (string.IsNullOrEmpty(text))
            {
                return string.Empty;
            }

            int charsOnEachSide = shorterLength / 2;
            return text.Length > shorterLength ? text.Substring(0, charsOnEachSide) + "..." + text.Substring(text.Length - charsOnEachSide, charsOnEachSide) : text;
        }

        public static DateTime UnixTimeStampToDateTime(ulong utcTimeStamp)
        {
            return new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc).AddSeconds(utcTimeStamp);
        }

        public static ulong DateTimeToUnixTimeStamp(DateTime dateTime)
        {
            return (ulong)(dateTime.ToUniversalTime() - new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds;
        }

        public static void SaveConfig()
        {
            try
            {
                var contentsToWriteToFile = Newtonsoft.Json.JsonConvert.SerializeObject(GlobalData.AppSettings);
                using (TextWriter writer = new StreamWriter(GlobalData.ConfigFileNameWithPath))
                {
                    writer.Write(contentsToWriteToFile);
                }                    
            }
            catch (Exception ex)
            {
                Logger.LogException("GLM.SVCF", ex);
            }
        }

        public static void LoadConfig()
        {
            try
            {
                if(File.Exists(GlobalData.ConfigFileNameWithPath))
                {
                    GlobalData.IsConfigFound = true;

                    using (TextReader reader = new StreamReader(GlobalData.ConfigFileNameWithPath))
                    {
                        var fileContents = reader.ReadToEnd();
                        ApplicationSettings settings = Newtonsoft.Json.JsonConvert.DeserializeObject<ApplicationSettings>(fileContents)!;
                        if (settings != null)
                        {
                            GlobalData.AppSettings = settings;
                        }
                    }
                }
                else
                {
                    GlobalData.IsConfigFound = false;
                }
            }
            catch (Exception ex)
            {
                Logger.LogException("GLM.LDCF", ex);
            }
        }

        public static void LoadAddressBook()
        {
            try
            {
                string addressBookFile = Path.Combine(GlobalData.WalletDir, GlobalData.AddressBookFileName);
                if (File.Exists(addressBookFile))
                {
                    using (TextReader reader = new StreamReader(addressBookFile))
                    {
                        var fileContents = reader.ReadToEnd();
                        AddressBook addressBook = Newtonsoft.Json.JsonConvert.DeserializeObject<AddressBook>(fileContents)!;
                        if (addressBook != null)
                        {
                            GlobalData.AddressBook = addressBook;
                        }
                    }
                }
                else
                {
                    GlobalData.AddressBook = new();
                }
            }
            catch (Exception ex)
            {
                Logger.LogException("GLM.LDAB", ex);
            }
        }

        public static void SaveAddressBook()
        {
            try
            {
                string addressBookFile = Path.Combine(GlobalData.WalletDir, GlobalData.AddressBookFileName);
                var contentsToWriteToFile = Newtonsoft.Json.JsonConvert.SerializeObject(GlobalData.AddressBook);
                using (TextWriter writer = new StreamWriter(addressBookFile))
                {
                    writer.Write(contentsToWriteToFile);
                }
            }
            catch (Exception ex)
            {
                Logger.LogException("GLM.SVAB", ex);
            }
        }

        public static string GenerateRandomString(int length)
        {
            Random random = new Random();

            char[] charArray = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();
            string text = string.Empty;
            for (int i = 0; i < length; i++)
            {
                text += charArray[random.Next(0, charArray.Length)];
            }

            return text;
        }

        public static string CycleLogFile(string path)
        {
            string logFile = path + ".log";
            string oldLogFile = logFile + ".old";

            try
            {
                if (File.Exists(oldLogFile))
                {
                    File.Delete(oldLogFile);
                }

                if (File.Exists(logFile))
                {
                    File.Move(logFile, oldLogFile);
                }
            }
            catch (Exception)
            {
                Logger.LogError("GLM.CLFL", "Cannot cycle log file. New log will be written to " + logFile);
                return logFile;
            }

            return logFile;
        }

        public static bool IsWindows()
        {
            return RuntimeInformation.IsOSPlatform(OSPlatform.Windows);            
        }

        public static bool IsLinux()
        {
            return RuntimeInformation.IsOSPlatform(OSPlatform.Linux);
        }

        public static bool IsOsx()
        {
            return RuntimeInformation.IsOSPlatform(OSPlatform.OSX);
        }

        public static Architecture GetCpuArchitecture()
        {
            return RuntimeInformation.OSArchitecture;
        }

        public static bool DirectoryContainsCliTools(string path)
        {
            if (!Directory.Exists(path))
            {
                return false;
            }

            bool hasDaemon = File.Exists(Path.Combine(path, GlobalData.DaemonProcessName));
            bool hasRpcWallet = File.Exists(Path.Combine(path, GlobalData.WalletProcessName));

            return (hasRpcWallet && hasDaemon);
        }

        public static List<string> GetSupportedLanguages()
        {
            List<string> languages = [];
            try
            {
                languages.Add(Language.English);
                languages.Add(Language.German);
                languages.Add(Language.Spanish);
                languages.Add(Language.French);
                languages.Add(Language.Italian);
                languages.Add(Language.Dutch);
                languages.Add(Language.Portuguese);
                languages.Add(Language.Russian);
                languages.Add(Language.Chinese_Simplified);
                languages.Add(Language.Esperanto);
                languages.Add(Language.Lojban);
            }
            catch (Exception ex)
            {
                Logger.LogException("GLM.GTSL", ex);
            }

            return languages;
        }

        public static async void SaveWallet()
        {
            try
            {
                SaveWalletResponse resStore = await GlobalData.WalletService.SaveWallet(GlobalData.AppSettings.Wallet[GlobalData.AppSettings.ActiveCoin].Rpc, new SaveWalletRequest());

                if (resStore.Error.IsError)
                {
                    Logger.LogError("GLM.SVWT", "Error saving wallet: " + GlobalData.OpenedWalletName + ". Code: " + resStore.Error.Code + ", Message: " + resStore.Error.Message);
                }
                else
                {
                    Logger.LogDebug("GLM.SVWT", "Wallet " + GlobalData.OpenedWalletName + " saved!");
                }
            }
            catch (Exception ex)
            {
                Logger.LogException("GLM.SVWT", ex);
            }
        }

        public static async void WalletUiUpdate()
        {
            try
            {
                // Get accounts for Wallets view
                GetAccountsResponse resGetAccounts = await GlobalData.WalletService.GetAccounts(GlobalData.AppSettings.Wallet[GlobalData.AppSettings.ActiveCoin].Rpc, new GetAccountsRequest());

                if (resGetAccounts.Error.IsError)
                {
                    Logger.LogError("GLM.WUIU", "GetAccounts Error Code: " + resGetAccounts.Error.Code + ", Message: " + resGetAccounts.Error.Message);
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
                Logger.LogException("GLM.WUIU", ex);
            }
        }

        public static string GenerateRandomHexString(int length, bool upperCase = false)
        {
            char[] array = ("0123456789" + (upperCase ? "ABCDEF" : "abcdef")).ToCharArray();
            string text = string.Empty;
            for (int i = 0; i < length; i++)
            {
                text += array[GlobalData.RandomGenerator.Next(0, array.Length)];
            }

            return text;
        }

        public static void CopyToClipboard(Avalonia.Visual visual, string text)
        {
            try
            {
                var clipboard = TopLevel.GetTopLevel(visual)?.Clipboard;
                var dataObject = new DataObject();
                dataObject.Set(DataFormats.Text, text);

                if (clipboard != null)
                {
                    clipboard.SetDataObjectAsync(dataObject);
                }
            }
            catch (Exception ex)
            {
                Logger.LogException("GLM.CTCL", ex);
            }
        }

        public static async void RestartWithQuickSync()
        {
            try
            {
                bool isSuccess = await DownloadFileToFolder(GlobalData.CoinSettings[GlobalData.AppSettings.ActiveCoin].QuickSyncUrl, GlobalData.CliToolsDir);
                if (isSuccess)
                {
                    Logger.LogDebug("GLM.RSQS", "Restarting CLI with QuickSync");
                    ProcessManager.Kill(GlobalData.WalletProcessName);
                    ProcessManager.Kill(GlobalData.DaemonProcessName);

                    GlobalData.IsDaemonRestarting = true;
                    string quickSyncFile = Path.Combine(GlobalData.CliToolsDir, Path.GetFileName(GlobalData.CoinSettings[GlobalData.AppSettings.ActiveCoin].QuickSyncUrl));
                    ProcessManager.StartExternalProcess(GetDaemonProcess(), GlobalData.CoinSettings[GlobalData.AppSettings.ActiveCoin].GenerateDaemonOptions(GlobalData.AppSettings.Daemon[GlobalData.AppSettings.ActiveCoin]) + " --quicksync \"" + quickSyncFile + "\"");
                }
                else
                {
                    Logger.LogError("GLM.RSQS", "Failed to download file: " + GlobalData.CoinSettings[GlobalData.AppSettings.ActiveCoin].QuickSyncUrl + " to " + GlobalData.CliToolsDir);
                }
            }
            catch (Exception ex)
            {
                Logger.LogException("GLM.RSQS", ex);
            }
        }

        #region Exit Application
        public static void Shutdown()
        {
            try
            {
                if (GlobalData.IsWalletOpen)
                {
                    ForceWalletClose();
                }

                // TODO: Added this because DASH has the same process for daemon and wallet
                if (GlobalData.DaemonProcessName != GlobalData.WalletProcessName)
                {
                    Logger.LogDebug("GLM.STDN", "Forcing wallet process close");
                    ProcessManager.Kill(GlobalData.WalletProcessName);
                }

                if (GlobalData.AppSettings.Daemon[GlobalData.AppSettings.ActiveCoin].StopOnExit)
                {
                    ForceDaemonStop();

                    Logger.LogDebug("GLM.STDN", "Forcing daemon process close");
                    ProcessManager.Kill(GlobalData.DaemonProcessName);
                }
            }
            catch (Exception ex)
            {
                Logger.LogException("GLM.STDN", ex);
            }

            Logger.LogInfo("GLM.STDN", "PROGRAM TERMINATED");
            Environment.Exit(0);
        }

        public static async void ForceWalletClose()
        {
            try
            {
                Logger.LogDebug("GLM.FWCL", "Closing wallet: " + GlobalData.OpenedWalletName);
                _ = await GlobalData.WalletService.CloseWallet(GlobalData.AppSettings.Daemon[GlobalData.AppSettings.ActiveCoin].Rpc, new Rpc.Wallet.Requests.CloseWalletRequest());
            }
            catch (Exception ex)
            {
                Logger.LogException("GLM.FWCL", ex);
            }
        }

        public static async void ForceDaemonStop()
        {
            try
            {
                Logger.LogDebug("GLM.FDSP", "Stopping Daemon");
                _ = await GlobalData.DaemonService.StopDaemon(GlobalData.AppSettings.Daemon[GlobalData.AppSettings.ActiveCoin].Rpc, new StopDaemonRequest());
            }
            catch (Exception ex)
            {
                Logger.LogException("GLM.FDSP", ex);
            }
        }
        #endregion // Exit Application
    }
}