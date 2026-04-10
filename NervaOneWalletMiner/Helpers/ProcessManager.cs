using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace NervaOneWalletMiner.Helpers
{
    public static class ProcessManager
    {
        private static string ExeNameToProcessName(string exe) => Path.GetFileNameWithoutExtension(exe);

        private static List<int> GetAndroidPidsByName(string processName)
        {
            List<int> pids = [];
            try
            {
                foreach (string pidDir in Directory.GetDirectories("/proc"))
                {
                    string dirName = Path.GetFileName(pidDir);
                    if (!int.TryParse(dirName, out int pid))
                    {
                        continue;
                    }

                    try
                    {
                        string cmdline = File.ReadAllText(Path.Combine(pidDir, "cmdline"));
                        if (cmdline.Contains(processName))
                        {
                            pids.Add(pid);
                        }
                    }
                    catch (Exception)
                    {
                        // /proc entries can vanish or be unreadable - ignore
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogException("PRM.GAPN", ex);
            }
            return pids;
        }

        public static void Kill(string processName)
        {
            try
            {
                if (GlobalMethods.IsAndroid())
                {
                    string name = ExeNameToProcessName(processName);
                    List<int> pids = GetAndroidPidsByName(name);
                    if (pids.Count == 0)
                    {
                        Logger.LogDebug("PRM.KILL", "Android: No instances of " + name + " to kill on");
                    }
                    else
                    {
                        foreach (int pid in pids)
                        {
                            try
                            {
                                Logger.LogDebug("PRM.KILL", "Android: Killing " + name + " with pid " + pid);
                                Process.GetProcessById(pid).Kill();
                                Logger.LogDebug("PRM.KILL", "Android: Killed pid " + pid);
                            }
                            catch (Exception ex)
                            {
                                Logger.LogException("PRM.KILL", "Android: Could not kill pid " + pid, ex);
                            }
                        }
                    }
                    return;
                }

                //Logger.LogDebug("PM.KIL", "Exe: " + exe);
                List<Process> processList = GetRunningByName(processName);

                if (processList.Count == 0)
                {
                    Logger.LogDebug("PRM.KILL", "No instances of " + processName + " to kill");
                    return;
                }

                foreach (Process process in processList)
                {
                    Logger.LogDebug("PRM.KILL", "Killing running instance of " + processName + " with id " + process.Id);

/*
#if UNIX
                    UnixNative.Kill(process.Id, Signum.SIGABRT);
#else
                    process.Kill();
#endif
*/

                    // TODO: Need to test this on Linux and MacOS!
                    process.Kill();

                    Logger.LogDebug("PRM.KILL", "Process " + process.Id + " killed");
                }
            }
            catch (Exception ex)
            {
                Logger.LogException("PRM.KILL", "Could not kill process", ex);
            }
        }

        public static bool IsRunning(string processName)
        {
            try
            {
                if (GlobalMethods.IsAndroid())
                {
                    string name = ExeNameToProcessName(processName);
                    return GetAndroidPidsByName(name).Count > 0;
                }

                //Logger.LogDebug("PM.IR", "Exe: " + processName);
                List<Process> processList = GetRunningByName(processName);

                //Logger.LogDebug("PM.IR", "Process count: " + processList.Count);
                if (processList.Count == 0)
                {
                    return false;
                }

                Process process = processList[0];
                if (process == null || process.HasExited)
                {
                    Logger.LogDebug("PRM.ISRN", "CLI tool " + processName + " exited unexpectedly");
                    return false;
                }
                else
                {
                    return true;
                }
            }
            catch (Exception ex)
            {
                Logger.LogException("PRM.ISRN", ex);
                return false;
            }
        }

        public static List<Process> GetRunningByName(string exe)
        {
            List<Process> processList = [];

            try
            {
                string processName = ExeNameToProcessName(exe);
                //Logger.LogDebug("PRM.GEBN", "Exe: " + exe + " | Process Name: " + processName);

                IList<Process> runningProcesses = Process.GetProcesses();

                foreach (Process process in runningProcesses)
                {
                    try
                    {
                        // macOS seems to be limited to 15 chars for process.ProcessName so use process.MainModule.ModuleName instead:
                        // Found nerva: nerva-wallet-rp | ID: 949 | MWT:  | MMFN: /Users/devmac/.nerva-gui/cli/nerva-wallet-rpc | MMMN: nerva-wallet-rpc

                        //if(process.ProcessName.Contains("nerva"))
                        //{
                        //    Logger.LogDebug("PRM.GEBN", "Found nerva: " + process.ProcessName + " | ID: " + process.Id + " | MWT: " + process.MainWindowTitle + " | MMFN: " + process.MainModule.FileName + " | MMMN: " + process.MainModule.ModuleName);
                        //}

                        if (process.ProcessName.Contains(processName.Length > 13 ? processName.Substring(0, 12) : processName))
                        {
                            // We're looking at all processes and some will not have MainModule so we need above check first
                            if (process.MainModule!.ModuleName.Contains(processName))
                            {
                                //Logger.LogDebug("PRM.GEBN", "Found process: " + process.ProcessName + " | ID: " + process.Id + " | MWT: " + process.MainWindowTitle + " | MMFN: " + process.MainModule.FileName + " | MMMN: " + process.MainModule.ModuleName);
                                processList.Add(process);
                            }
                        }
                    }
                    catch (Exception ex1)
                    {
                        Logger.LogException("PRM.GRN1", ex1);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogException("PRM.GRBN", ex);
            }
            return processList;
        }

        public static void StartExternalProcess(string exePath, string args)
        {
            Logger.LogDebug("PRM.STEP", "Starting process: " + ExeNameToProcessName(exePath) + " with args: " + args);

            string fileName = exePath;
            string arguments = args;

            if (GlobalMethods.IsAndroid())
            {
                fileName = "/system/bin/linker64";
                arguments = exePath + " " + args;
            }

            Process? process = Process.Start(new ProcessStartInfo(fileName, arguments)
            {
                UseShellExecute = false,
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                CreateNoWindow = true,
                WorkingDirectory = GlobalData.CliToolsDir
            });

        }
    }
}