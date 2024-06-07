using System;
using System.IO;
using log4net;
using log4net.Appender;
using log4net.Layout;
using log4net.Repository.Hierarchy;

namespace NervaOneWalletMiner.Helpers
{
    public static class Logger
    {        
        private static readonly ILog Log = LogManager.GetLogger(typeof(Logger));

        public static void SetUpLogger()
        {
            Hierarchy hierarchy = (Hierarchy)LogManager.GetRepository();
            hierarchy.Root.RemoveAllAppenders(); // Remove any appenders

            RollingFileAppender fileAppender = new RollingFileAppender();
            fileAppender.File = Path.Combine(GlobalData.LogDir, "log");
            fileAppender.AppendToFile = true;
            fileAppender.RollingStyle = RollingFileAppender.RollingMode.Date;
            fileAppender.DatePattern = "_yyyMMdd.'log'";
            fileAppender.StaticLogFileName = false;
            fileAppender.LockingModel = new FileAppender.MinimalLock();
            
            PatternLayout pl = new PatternLayout();
            pl.ConversionPattern = "%-5level %date - %message%newline";
            pl.ActivateOptions();
            fileAppender.Layout = pl;
            fileAppender.ActivateOptions();

            log4net.Config.BasicConfigurator.Configure(fileAppender);

            LogInfo("LOG.SULR", "App Version: " + GlobalData.Version + " | OS: " + Environment.OSVersion.Platform + " " + Environment.OSVersion.Version + " | CPUs: " + Environment.ProcessorCount);
            LogInfo("LOG.SULR", "Is Windows: " + GlobalMethods.IsWindows() + " | Is Linux: " + GlobalMethods.IsLinux() + " | Is OSX: " + GlobalMethods.IsOsx() + " | CPU Architecture: " + GlobalMethods.GetCpuArchitecture());
            LogInfo("LOG.SULR", "Using Data Directory: " + GlobalData.DataDir);
        }
        public static void LogException(string origin, Exception exception)
        {
            LogException(origin, "", exception);
        }

        public static void LogException(string origin, string message, Exception exception)
        {
            try
            {
                if (string.IsNullOrEmpty(message))
                {
                    Log.Error(origin, exception);
                }
                else
                {
                    Log.Error(origin + " - " + message, exception);
                }
            }
            catch (Exception ex)
            {
                // Logging failed.  Not much you can do.  Just try to continue.
                ex.Data.Clear();
            }
        }

        public static void LogError(string origin, string message)
        {
            try
            {
                Log.Error(origin + " - " + message);
            }
            catch (Exception ex)
            {
                // Logging failed.  Not much you can do.  Just try to continue.
                ex.Data.Clear();
            }
        }

        public static void LogDebug(string origin, string message)
        {
            try
            {
                Log.Debug(origin + " - " + message);
            }
            catch (Exception ex)
            {
                // Logging failed.  Not much you can do.  Just try to continue.
                ex.Data.Clear();
            }
        }

        public static void LogInfo(string origin, string message)
        {
            try
            {
                Log.Info(origin + " - " + message);
            }
            catch (Exception ex)
            {
                // Logging failed.  Not much you can do.  Just try to continue.
                ex.Data.Clear();
            }
        }
    }
}