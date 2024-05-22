using Avalonia.Media.Imaging;
using Avalonia.Platform;
using NervaOneWalletMiner.Objects.Constants;
using NervaOneWalletMiner.Objects.Settings;
using NervaOneWalletMiner.Objects.Settings.CoinSpecific;
using NervaOneWalletMiner.Rpc;
using NervaOneWalletMiner.Rpc.Daemon;
using NervaOneWalletMiner.Rpc.Common;
using NervaOneWalletMiner.Rpc.Daemon.Downloads;
using NervaOneWalletMiner.Rpc.Wallet;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Runtime.InteropServices;
using System.Threading;

namespace NervaOneWalletMiner.Helpers
{
    public static class GlobalMethods
    {
        #region Directories, Paths and Names
        public static string GetDataDir()
        {
            string dataDirectory;

            // Get data directory
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                string? homeDir = Environment.GetEnvironmentVariable("HOME");
                if (homeDir != null)
                {
                    dataDirectory = Path.Combine(homeDir, GlobalData.AppName);
                }
                else
                {
                    throw new DirectoryNotFoundException("Non-Windows dir not found");
                }
            }
            else
            {
                string? appdataDir = Environment.GetEnvironmentVariable("APPDATA");
                if (appdataDir != null)
                {
                    dataDirectory = Path.Combine(appdataDir, GlobalData.AppName);
                }
                else
                {
                    throw new DirectoryNotFoundException("Windows dir not found");
                }
            }

            // Create data directory if it does not exist
            if (!Directory.Exists(dataDirectory))
            {
                Directory.CreateDirectory(dataDirectory);
            }

            if(!Directory.Exists(dataDirectory))
            {
                throw new Exception("Data directory not set up. Application cannot continue");
            }

            return dataDirectory;
        }

        public static string GetLogDir()
        {
            string logDirectory;

            if (Directory.Exists(GlobalData.DataDir))
            {
                // Create logs directory if it does not exist
                logDirectory = Path.Combine(GlobalData.DataDir, GlobalData.LogsDirName);
                if (!Directory.Exists(logDirectory))
                {
                    Directory.CreateDirectory(logDirectory);
                }
            }
            else
            {
                throw new Exception("Data directory not set up. Application cannot continue");
            }

            return logDirectory;
        }

        public static string GetWalletDir()
        {
            string walletDirectory;

            if (Directory.Exists(GlobalData.DataDir))
            {
                // Create logs directory if it does not exist
                walletDirectory = Path.Combine(GlobalData.DataDir, GlobalData.MainCoinsDirName, GlobalData.CoinDirName, GlobalData.WalletDirName);
                if (!Directory.Exists(walletDirectory))
                {
                    Directory.CreateDirectory(walletDirectory);
                }
            }
            else
            {
                throw new Exception("Data directory not set up. Application cannot continue");
            }

            return walletDirectory;
        }

        public static string GetCliToolsDir()
        {
            string cliToolsDirectory;

            if (Directory.Exists(GlobalData.DataDir))
            {
                // Create logs directory if it does not exist
                cliToolsDirectory = Path.Combine(GlobalData.DataDir, GlobalData.MainCoinsDirName, GlobalData.CoinDirName, GlobalData.CliToolsDirName);
                if (!Directory.Exists(cliToolsDirectory))
                {
                    Directory.CreateDirectory(cliToolsDirectory);
                }
            }
            else
            {
                throw new Exception("Data directory not set up. Application cannot continue");
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

        public static string GetConfigFilePath()
        {           
            string dataDir = GetDataDir();
            return Path.Combine(dataDir, "app.config");
        }
        #endregion // Directories, Paths and Names

        #region Coin Specific Setup
        public static Dictionary<string, ISettingsDefault> GetDefaultSettings()
        {
            Dictionary<string, ISettingsDefault> defaultSettings = [];

            defaultSettings.Add(Coin.XNV, new SettingsDefaultXNV());
            defaultSettings.Add(Coin.XMR, new SettingsDefaultXMR());

            return defaultSettings;
        }
        public static Dictionary<string, SettingsDaemon> GetDaemonSettings()
        {
            if (GlobalData.DefaultSettings == null || GlobalData.DefaultSettings.Count == 0)
            {
                GlobalData.DefaultSettings = GetDefaultSettings();
            }

            Dictionary<string, SettingsDaemon> daemonSettings = [];

            daemonSettings.Add(Coin.XNV, new SettingsDaemon() { 
                BlockSeconds = GlobalData.DefaultSettings[Coin.XNV].BlockSeconds,
                LogLevel = GlobalData.DefaultSettings[Coin.XNV].LogLevelDaemon,
                Rpc = new RpcBase() { 
                    Port = GlobalData.DefaultSettings[Coin.XNV].DaemonPort
                }
            });

            daemonSettings.Add(Coin.XMR, new SettingsDaemon()
            {
                BlockSeconds = GlobalData.DefaultSettings[Coin.XMR].BlockSeconds,
                LogLevel = GlobalData.DefaultSettings[Coin.XMR].LogLevelDaemon,
                Rpc = new RpcBase()
                {
                    Port = GlobalData.DefaultSettings[Coin.XMR].DaemonPort
                }
            });

            return daemonSettings;
        }

        public static Dictionary<string, SettingsWallet> GetWalletSettings()
        {
            if(GlobalData.DefaultSettings == null || GlobalData.DefaultSettings.Count == 0)
            {
                GlobalData.DefaultSettings = GetDefaultSettings();
            }

            Dictionary<string, SettingsWallet> walletSettings = [];

            walletSettings.Add(Coin.XNV, new SettingsWallet()
            {
                DisplayUnits = GlobalData.DefaultSettings[Coin.XNV].DisplayUnits,
                LogLevel = GlobalData.DefaultSettings[Coin.XNV].LogLevelDaemon,
                Rpc = new RpcBase()
                {
                    Port = (uint)GlobalData.RandomGenerator.Next(10000, 50000)
                }
            });

            walletSettings.Add(Coin.XMR, new SettingsWallet()
            {
                DisplayUnits = GlobalData.DefaultSettings[Coin.XMR].DisplayUnits,
                LogLevel = GlobalData.DefaultSettings[Coin.XMR].LogLevelDaemon,
                Rpc = new RpcBase()
                {
                    Port = (uint)GlobalData.RandomGenerator.Next(10000, 50000)
                }
            });

            return walletSettings;
        }

        public static void SetCoin(string newCoin)
        {
            // TODO: Need to do certain tings when switching coins. RpcConnection? Wallet?

            switch(newCoin)
            {
                case Coin.XMR:
                    GlobalData.CoinDirName = Coin.XMR;
                    GlobalData.AppSettings.ActiveCoin = Coin.XMR;                   
                    GlobalData.CliToolsDir = GetCliToolsDir();
                    GlobalData.WalletDir = GetWalletDir();

                    GlobalData.WalletProcessName = GetWalletProcessName();
                    GlobalData.DaemonProcessName = GetDaemonProcessName();
                    GlobalData.Logo = GetLogo();

                    GlobalData.DaemonService = new DaemonServiceXMR();
                    GlobalData.WalletService = new WalletServiceXMR();
                    GlobalData.DownloadLinks = new DownloadXMR();

                    // TODO: Change this. App.config overwrites GetDaemonSettings with 0
                    if (GlobalData.AppSettings.Daemon[Coin.XMR].BlockSeconds != GlobalData.DefaultSettings[Coin.XMR].BlockSeconds)
                    {
                        GlobalData.AppSettings.Daemon[Coin.XMR].BlockSeconds = GlobalData.DefaultSettings[Coin.XMR].BlockSeconds;
                    }
                    if (GlobalData.AppSettings.Daemon[Coin.XMR].LogLevel != GlobalData.DefaultSettings[Coin.XMR].LogLevelDaemon)
                    {
                        GlobalData.AppSettings.Daemon[Coin.XMR].LogLevel = GlobalData.DefaultSettings[Coin.XMR].LogLevelDaemon;
                    }
                    if (GlobalData.AppSettings.Wallet[Coin.XMR].DisplayUnits != GlobalData.DefaultSettings[Coin.XMR].DisplayUnits)
                    {
                        GlobalData.AppSettings.Wallet[Coin.XMR].DisplayUnits = GlobalData.DefaultSettings[Coin.XMR].DisplayUnits;
                    }
                    if (GlobalData.AppSettings.Wallet[Coin.XMR].LogLevel != GlobalData.DefaultSettings[Coin.XMR].LogLevelWallet)
                    {
                        GlobalData.AppSettings.Wallet[Coin.XMR].LogLevel = GlobalData.DefaultSettings[Coin.XMR].LogLevelWallet;
                    }
                    break;

                default:
                    // XNV or anything else not supported
                    GlobalData.CoinDirName = Coin.XNV;
                    GlobalData.AppSettings.ActiveCoin = Coin.XNV;
                    GlobalData.CliToolsDir = GetCliToolsDir();
                    GlobalData.WalletDir = GetWalletDir();

                    GlobalData.WalletProcessName = GetWalletProcessName();
                    GlobalData.DaemonProcessName = GetDaemonProcessName();
                    GlobalData.Logo = GetLogo();

                    GlobalData.DaemonService = new DaemonServiceXNV();
                    GlobalData.WalletService = new WalletServiceXNV();
                    GlobalData.DownloadLinks = new DownloadXNV();

                    // TODO: Change this. App.config overwrites GetDaemonSettings() with default 0
                    if (GlobalData.AppSettings.Daemon[Coin.XNV].BlockSeconds != GlobalData.DefaultSettings[Coin.XNV].BlockSeconds)
                    {
                        GlobalData.AppSettings.Daemon[Coin.XNV].BlockSeconds = GlobalData.DefaultSettings[Coin.XNV].BlockSeconds;
                    }
                    break;
            }


            // Download CLI tools, if we do not have them already
            if (!DirectoryContainsCliTools(GlobalData.CliToolsDir))
            {                
                string cliToolsLink = GetCliToolsDownloadLink();
                if(!string.IsNullOrEmpty(cliToolsLink))
                {
                    DownloadCLI(cliToolsLink, GlobalData.CliToolsDir);
                }
            }
        }

        public static string GetCliToolsDownloadLink()
        {
            string cliDownloadLink = string.Empty;

            try
            {
                Architecture arch = GetCpuArchitecture();

                if (IsWindows())
                {
                    switch (arch)
                    {
                        case Architecture.X64:
                            cliDownloadLink = GlobalData.DownloadLinks.CliUrlWindows64;
                            break;
                        default:
                            cliDownloadLink = GlobalData.DownloadLinks.CliUrlWindows32;
                            break;
                    }
                }
                else if (IsLinux())
                {
                    switch (arch)
                    {
                        case Architecture.X64:
                            cliDownloadLink = GlobalData.DownloadLinks.CliUrlLinux64;
                            break;
                        case Architecture.Arm:
                        case Architecture.Arm64:
                            cliDownloadLink = GlobalData.DownloadLinks.CliUrlLinuxArm;
                            break;
                        default:
                            cliDownloadLink = GlobalData.DownloadLinks.CliUrlLinux32;
                            break;
                    }
                }
                else if (IsOsx())
                {
                    switch (arch)
                    {
                        case Architecture.Arm:
                        case Architecture.Arm64:
                            cliDownloadLink = GlobalData.DownloadLinks.CliUrlMacArm;
                            break;
                        default:
                            cliDownloadLink = GlobalData.DownloadLinks.CliUrlMacIntel;
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogException("GM.GCTDL", ex);
            }
            
            return cliDownloadLink;
        }

        public static void DownloadCLI(string downloadLink, string cliToolsPath)
        {
                // Check if we already downloaded the CLI package
                string destFile = Path.Combine(cliToolsPath, Path.GetFileName(downloadLink));

                if (File.Exists(destFile))
                {
                    Logger.LogDebug("UM.DC", $"CLI tools found @ {destFile}");

                    ExtractFile(cliToolsPath, destFile);
                }
                else
                {
                    Logger.LogDebug("GM.DC", "Downloading CLI tools. URL: " + downloadLink);
                    using (var client = new WebClient())
                    {
                        client.DownloadFileCompleted += (s, e) =>
                        {
                            if (e.Error == null)
                            {
                                ExtractFile(cliToolsPath, destFile);
                            }
                            else
                            {
                                Logger.LogError("UM.DC", e.Error.Message);                               
                            }
                        };

                        client.DownloadFileAsync(new Uri(downloadLink), destFile);
                    }
                }
        }

        private static void ExtractFile(string destDir, string destFile)
        {
            try
            {
                Logger.LogDebug("GM.EF", "Closing Daemon and Wallet processes");
                while (DaemonProcess.IsRunning())
                {
                    DaemonProcess.ForceClose();
                    Thread.Sleep(1000);
                }

                while (WalletProcess.IsRunning())
                {
                    WalletProcess.ForceClose();
                    Thread.Sleep(1000);
                }

                Logger.LogDebug("GM.EF", "Extracting CLI tools");

                ZipArchive archive = ZipFile.Open(destFile, ZipArchiveMode.Read);
                foreach (var entry in archive.Entries)
                {
                    if (!string.IsNullOrEmpty(entry.Name))
                    {
                        Logger.LogDebug("GM.EF", "Extracting: " + entry.Name);
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
                Logger.LogException("GM.EF", ex);
            }
        }

        public static string GetDaemonProcessName()
        {
            string daemonProcess;

            switch (GlobalData.AppSettings.ActiveCoin)
            {
                case Coin.XMR:
                    daemonProcess = GlobalMethods.IsWindows()? "monerod.exe" : "monerod";
                    break;
                default:
                    // XNV or anything else not supported
                    daemonProcess = GlobalMethods.IsWindows()? "nervad.exe" : "nervad";
                    break;
            }

            return daemonProcess;
        }

        public static string GetWalletProcessName()
        {
            string walletProcess;

            switch (GlobalData.AppSettings.ActiveCoin)
            {
                case Coin.XMR:
                    walletProcess = GlobalMethods.IsWindows() ? "monero-wallet-rpc.exe" : "monero-wallet-rpc";
                    break;
                default:
                    // XNV or anything else not supported
                    walletProcess = GlobalMethods.IsWindows() ? "nerva-wallet-rpc.exe" : "nerva-wallet-rpc";
                    break;
            }

            return walletProcess;
        }

        public static Bitmap GetLogo()
        {
            Bitmap logo;

            switch (GlobalData.AppSettings.ActiveCoin)
            {
                case Coin.XMR:
                    logo = new Bitmap(AssetLoader.Open(new Uri("avares://NervaOneWalletMiner/Assets/xmr/logo.png")));
                    break;
                default:
                    // XNV or anything else not supported
                    logo = new Bitmap(AssetLoader.Open(new Uri("avares://NervaOneWalletMiner/Assets/xnv/logo.png")));
                    break;
            }

            return logo;
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
                using (TextWriter writer = new StreamWriter(GlobalData.ConfigFilePath))
                {
                    writer.Write(contentsToWriteToFile);
                }                    
            }
            catch (Exception ex)
            {
                Logger.LogException("GM.SC", ex);
            }
        }

        public static void LoadConfig()
        {
            try
            {
                if(File.Exists(GlobalData.ConfigFilePath))
                {
                    using (TextReader reader = new StreamReader(GlobalData.ConfigFilePath))
                    {
                        var fileContents = reader.ReadToEnd();
                        ApplicationSettings settings = Newtonsoft.Json.JsonConvert.DeserializeObject<ApplicationSettings>(fileContents)!;
                        if (settings != null)
                        {
                            GlobalData.AppSettings = settings;
                        }
                    }
                }            
            }
            catch (Exception ex)
            {
                Logger.LogException("GM.LC", ex);
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
                Logger.LogError("PM.CLF", $"Cannot cycle log file. New log will be written to {logFile}");
                return logFile;
            }

            return logFile;
        }

        public static bool IsWindows()
        {
            GetCpuArchitecture();

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
                Logger.LogException("PM.GSL", ex);
            }

            return languages;
        }
    }
}