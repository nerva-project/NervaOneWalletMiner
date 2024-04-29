using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using NervaWalletMiner.Objects;

namespace NervaWalletMiner.Helpers
{
    public static class ProcessManager
    {
        private static string ExeNameToProcessName(string exe) => Path.GetFileNameWithoutExtension(exe);

        public static void Kill(string exe)
        {
            try
            {
                //Logger.LogDebug("PM.KIL", "Exe: " + exe);
                List<Process> processList = GetRunningByName(exe);

                if (processList.Count == 0)
                {
                    Logger.LogDebug("PM.KIL", $"No instances of {exe} to kill");
                    return;
                }

                foreach (Process process in processList)
                {
                    Logger.LogDebug("PM.KIL", $"Killing running instance of {exe} with id {process.Id}");

/*
#if UNIX
                    UnixNative.Kill(process.Id, Signum.SIGABRT);
#else
                    process.Kill();
#endif
*/

                    // TODO: Need to test this on Linux and MacOS!
                    process.Kill();

                    Logger.LogDebug("PM.KIL", $"Process {process.Id} killed");
                }
            }
            catch (Exception ex)
            {
                Logger.LogException("PM.KIL", "Could not kill process", ex);
            }
        }

        public static bool IsRunning(string exe, out Process? process)
        {
            process = null;

            try
            {
                //Logger.LogDebug("PM.IR", "Exe: " + exe);
                List<Process> processList = GetRunningByName(exe);

                //Logger.LogDebug("PM.IR", "Process count: " + processList.Count);
                if (processList.Count == 0)
                {
                    return false;
                }

                process = processList[0];

                if (process == null || process.HasExited)
                {
                    Logger.LogDebug("PM.IR", $"CLI tool {exe} exited unexpectedly. Restarting");
                    process = null;
                    return false;
                }
                else
                {
                    return true;
                }
            }
            catch (Exception ex)
            {
                Logger.LogException("PM.IR", ex);
                return false;
            }
        }

        public static List<Process> GetRunningByName(string exe)
        {
            List<Process> processList = [];

            try
            {
                string processName = ExeNameToProcessName(exe);
                //Logger.LogDebug("PM.GEBN", "Exe: " + exe + " | Process Name: " + processName);

                IList<Process> runningProcesses = Process.GetProcesses();

                foreach (Process process in runningProcesses)
                {
                    try
                    {
                        // macOS seems to be limited to 15 chars for process.ProcessName so use process.MainModule.ModuleName instead:
                        // Found nerva: nerva-wallet-rp | ID: 949 | MWT:  | MMFN: /Users/devmac/.nerva-gui/cli/nerva-wallet-rpc | MMMN: nerva-wallet-rpc

                        //if(process.ProcessName.Contains("nerva"))
                        //{
                        //    Logger.LogDebug("PM.GEBN", "Found nerva: " + process.ProcessName + " | ID: " + process.Id + " | MWT: " + process.MainWindowTitle + " | MMFN: " + process.MainModule.FileName + " | MMMN: " + process.MainModule.ModuleName);
                        //}

                        if (process.ProcessName.Contains(processName.Length > 13 ? processName.Substring(0, 12) : processName))
                        {
                            // We're looking at all processes and some will not have MainModule so we need above check first
                            if (process.MainModule.ModuleName.Contains(processName))
                            {
                                //Logger.LogDebug("PM.GEBN", "Found process: " + process.ProcessName + " | ID: " + process.Id + " | MWT: " + process.MainWindowTitle + " | MMFN: " + process.MainModule.FileName + " | MMMN: " + process.MainModule.ModuleName);
                                processList.Add(process);
                            }
                        }
                    }
                    catch (Exception ex1)
                    {
                        Logger.LogException("PM.GRBN1", ex1);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogException("PM.GRBN", ex);
            }
            return processList;
        }

        public static void StartExternalProcess(string exePath, string args)
        {
            Logger.LogDebug("PM.SEP", $"Starting process {ExeNameToProcessName(exePath)} {args}");

            _ = Process.Start(new ProcessStartInfo(exePath, args)
            {
                UseShellExecute = false,
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                CreateNoWindow = true
            });
        }

        public static string GenerateCommandLine(string exePath, SettingsRpc d)
        {
            string arg = $"--log-file \"{GlobalMethods.CycleLogFile(exePath)}\"";

            if (GlobalData.ApplicationSettings.Daemon.IsTestnet)
            {
                Logger.LogDebug("PM.GCL", "Connecting to testnet");
                arg += " --testnet";
            }

            arg += $" --rpc-bind-port {d.Port}";
            arg += " --log-level " + d.LogLevel;

            return arg;
        }
    }
}