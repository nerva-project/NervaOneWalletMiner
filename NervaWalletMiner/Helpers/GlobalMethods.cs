using Avalonia.Media.Imaging;
using Avalonia.Platform;
using NervaWalletMiner.Objects.Constants;
using NervaWalletMiner.Objects.Settings;
using NervaWalletMiner.Rpc.Daemon;
using NervaWalletMiner.Rpc.Wallet;
using NervaWalletMiner.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

namespace NervaWalletMiner.Helpers
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
        public static Dictionary<string, SettingsDaemon> GetDaemonSettings()
        {
            Dictionary<string, SettingsDaemon> daemonSettings = [];

            daemonSettings.Add(Coin.XNV, new SettingsDaemon(17566) { BlockSeconds = 60.0, LogLevel = 1 });
            daemonSettings.Add(Coin.XMR, new SettingsDaemon(18081) { BlockSeconds = 120.0, LogLevel = 0 });

            return daemonSettings;
        }

        public static Dictionary<string, SettingsWallet> GetWalletSettings()
        {
            Dictionary<string, SettingsWallet> walletSettings = [];

            walletSettings.Add(Coin.XNV, new SettingsWallet() { DisplayUnits = "XNV" });
            walletSettings.Add(Coin.XMR, new SettingsWallet() { DisplayUnits = "XMR" });

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

                    // TODO: Change this. App.config overwrites GetDaemonSettings with 0
                    if (GlobalData.AppSettings.Daemon[Coin.XMR].BlockSeconds != 120.0)
                    {
                        GlobalData.AppSettings.Daemon[Coin.XMR].BlockSeconds = 120.0;
                    }
                    if (GlobalData.AppSettings.Daemon[Coin.XMR].LogLevel != 0)
                    {
                        GlobalData.AppSettings.Daemon[Coin.XMR].LogLevel = 0;
                    }
                    if (GlobalData.AppSettings.Wallet[Coin.XMR].DisplayUnits != "XMR")
                    {
                        GlobalData.AppSettings.Wallet[Coin.XMR].DisplayUnits = "XMR";
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

                    // TODO: Change this. App.config overwrites GetDaemonSettings() with default 0
                    if (GlobalData.AppSettings.Daemon[Coin.XNV].BlockSeconds != 60.0)
                    {
                        GlobalData.AppSettings.Daemon[Coin.XNV].BlockSeconds = 60.0;
                    }
                    break;
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
                    logo = new Bitmap(AssetLoader.Open(new Uri("avares://NervaWalletMiner/Assets/xmr/logo.png")));
                    break;
                default:
                    // XNV or anything else not supported
                    logo = new Bitmap(AssetLoader.Open(new Uri("avares://NervaWalletMiner/Assets/xnv/logo.png")));
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
            return RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
        }

        public static bool IsLinux()
        {
            return RuntimeInformation.IsOSPlatform(OSPlatform.Linux);
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