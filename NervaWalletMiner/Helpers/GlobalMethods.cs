using Avalonia.Media.Imaging;
using Avalonia.Platform;
using NervaWalletMiner.Objects.Constants;
using NervaWalletMiner.Objects.Settings;
using NervaWalletMiner.Rpc.Daemon;
using NervaWalletMiner.Rpc.Wallet;
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

        public static string GetDaemonPath()
        {
            return Path.Combine(GlobalData.CliToolsDir, GlobalData.DaemonProcessName);
        }

        public static string GetRpcWalletPath()
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

            daemonSettings.Add(Coin.XNV, new SettingsDaemon(17566, false));
            daemonSettings.Add(Coin.XMR, new SettingsDaemon(18081, false));

            return daemonSettings;
        }

        public static Dictionary<string, SettingsWallet> GetWalletSettings()
        {
            Dictionary<string, SettingsWallet> daemonSettings = [];

            daemonSettings.Add(Coin.XNV, new SettingsWallet());
            daemonSettings.Add(Coin.XMR, new SettingsWallet());

            return daemonSettings;
        }

        public static Dictionary<string, SettingsMisc> GetMiscSettings()
        {
            Dictionary<string, SettingsMisc> daemonSettings = [];

            daemonSettings.Add(Coin.XNV, new SettingsMisc());
            daemonSettings.Add(Coin.XMR, new SettingsMisc() { Logo = new Bitmap(AssetLoader.Open(new Uri("avares://NervaWalletMiner/Assets/xmr/logo.png"))) });

            return daemonSettings;
        }

        public static void SetCoin(string newCoin)
        {
            switch(newCoin)
            {
                case Coin.XMR:
                    GlobalData.AppSettings.ActiveCoin = Coin.XMR;
                    GlobalData.WalletProcessName = GetWalletProcessName();
                    GlobalData.DaemonProcessName = GetDaemonProcessName();

                    // TODO: Change once interface implemented
                    GlobalData.WalletService = new WalletServiceXNV();
                    GlobalData.DaemonService = new DaemonServiceXNV();                    
                    break;
                default:
                    // XNV or anything else not supported
                    GlobalData.AppSettings.ActiveCoin = Coin.XNV;
                    GlobalData.WalletProcessName = GetWalletProcessName();
                    GlobalData.DaemonProcessName = GetDaemonProcessName();

                    GlobalData.WalletService = new WalletServiceXNV();
                    GlobalData.DaemonService = new DaemonServiceXNV();                   
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

        public static double XnvFromAtomicUnits(ulong balanceAtomic, int decimalPlaces)
        {
            return Math.Round(balanceAtomic / 1000000000000.0d, decimalPlaces);
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
    }
}