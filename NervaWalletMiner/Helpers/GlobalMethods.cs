using System;
using System.IO;
using System.Runtime.InteropServices;

namespace NervaWalletMiner.Helpers
{
    public static class GlobalMethods
    {
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
                walletDirectory = Path.Combine(GlobalData.DataDir, GlobalData.WalletDirName);
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
    }
}