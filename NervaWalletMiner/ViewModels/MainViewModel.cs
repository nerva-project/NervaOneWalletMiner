using Avalonia.Controls;
using Avalonia.Controls.Selection;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using NervaWalletMiner.Helpers;
using NervaWalletMiner.Objects;
using NervaWalletMiner.Rpc;
using NervaWalletMiner.Rpc.Daemon;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Input;
using static NervaWalletMiner.Rpc.Daemon.MiningStatus;

namespace NervaWalletMiner.ViewModels;

public class MainViewModel : ViewModelBase
{
    public static System.Timers.Timer? _daemonUpdateTimer;
    public const int _daemonTimerInterval = 5000;
    public static bool _killMasterProcess = false;
    public static DateTime _cliToolsRunningLastCheck = DateTime.MinValue;
    public DateTime _lastDaemonResponseTime = DateTime.Now;


    public static Dictionary<string, ViewModelBase> ViewModelPagesDictionary = new();

    public static readonly Bitmap _inImage = new Bitmap(AssetLoader.Open(new Uri("avares://NervaWalletMiner/Assets/transfer_in.png")));
    public static readonly Bitmap _outImage = new Bitmap(AssetLoader.Open(new Uri("avares://NervaWalletMiner/Assets/transfer_out.png")));

    private bool? _isPaneOpen = false;
    private ViewModelBase _CurrentPage;
    public SelectionModel<ListBoxItem> Selection { get; }
    public ICommand TriggerPaneCommand { get; }
    

    public MainViewModel()
    {
        // Set up split view pages
        ViewModelPagesDictionary.Add(SplitViewPages.Home, new HomeViewModel());
        ViewModelPagesDictionary.Add(SplitViewPages.Wallet, new WalletViewModel());
        ViewModelPagesDictionary.Add(SplitViewPages.Transfers, new TransfersViewModel());
        ViewModelPagesDictionary.Add(SplitViewPages.Settings, new SettingsViewModel());              

        TriggerPaneCommand = ReactiveCommand.Create(TriggerPane);

        _CurrentPage = ViewModelPagesDictionary[SplitViewPages.Home];

        Selection = new SelectionModel<ListBoxItem>();
        Selection.SelectionChanged += SelectionChanged;

        StartMasterUpdateProcess();

        UpdateView();
    }

    public bool? IsPaneOpen
    {
        get => _isPaneOpen;
        set => this.RaiseAndSetIfChanged(ref _isPaneOpen, value);
    }

    public ViewModelBase CurrentPage
    {
        get { return _CurrentPage; }
        private set { this.RaiseAndSetIfChanged(ref _CurrentPage, value); }
    }

    private string _DaemonStatus = "";
    public string DaemonStatus
    {
        get => _DaemonStatus;
        set => this.RaiseAndSetIfChanged(ref _DaemonStatus, value);
    }

    private string _DaemonVersion = "";
    public string DaemonVersion
    {
        get => _DaemonVersion;
        set => this.RaiseAndSetIfChanged(ref _DaemonVersion, value);
    }

    void SelectionChanged(object sender, SelectionModelSelectionChangedEventArgs e)
    {
        // TODO: Figoure out better way of doing this
        switch(((ListBoxItem)e.SelectedItems[0]!).Name)
        {
            case SplitViewPages.Wallet:
                CurrentPage = ViewModelPagesDictionary[SplitViewPages.Wallet];
                break;
            case SplitViewPages.Transfers:
                CurrentPage = ViewModelPagesDictionary[SplitViewPages.Transfers];
                break;
            case SplitViewPages.Settings:
                CurrentPage = ViewModelPagesDictionary[SplitViewPages.Settings];
                break;
            default:
                CurrentPage = ViewModelPagesDictionary[SplitViewPages.Home];
                break;
        }
    }

    private void TriggerPane()
    {
        IsPaneOpen = !IsPaneOpen;
    }

    public void UpdateView()
    {
        ((HomeViewModel)ViewModelPagesDictionary[SplitViewPages.Home]).NetHeight = GlobalData.NetworkStats.NetHeight.ToString();
        ((HomeViewModel)ViewModelPagesDictionary[SplitViewPages.Home]).YourHeight = GlobalData.NetworkStats.YourHeight.ToString();
        ((HomeViewModel)ViewModelPagesDictionary[SplitViewPages.Home]).NetHash = GlobalData.NetworkStats.NetHash;
        ((HomeViewModel)ViewModelPagesDictionary[SplitViewPages.Home]).RunTime = GlobalData.NetworkStats.RunTime;

        ((HomeViewModel)ViewModelPagesDictionary[SplitViewPages.Home]).MinerMessage = GlobalData.NetworkStats.MinerStatus;
        ((HomeViewModel)ViewModelPagesDictionary[SplitViewPages.Home]).YourHash = GlobalData.NetworkStats.YourHash;
        ((HomeViewModel)ViewModelPagesDictionary[SplitViewPages.Home]).BlockTime = GlobalData.NetworkStats.BlockTime;
        ((HomeViewModel)ViewModelPagesDictionary[SplitViewPages.Home]).MiningAddress = GlobalData.NetworkStats.MiningAddress;

        ((HomeViewModel)ViewModelPagesDictionary[SplitViewPages.Home]).Connections = GlobalData.Connections;

        if(GlobalData.NetworkStats.MinerStatus.Equals(MinerStatus.Mining))
        {
            // Mining so disable number of threads and show Stop Mining
            if (((HomeViewModel)ViewModelPagesDictionary[SplitViewPages.Home]).StartStopMining.Equals(MinerStatus.StartMining))
            {
                ((HomeViewModel)ViewModelPagesDictionary[SplitViewPages.Home]).StartStopMining = MinerStatus.StopMining;                
            }
            if(((HomeViewModel)ViewModelPagesDictionary[SplitViewPages.Home]).IsNumThreadsEnabled)
            {
                ((HomeViewModel)ViewModelPagesDictionary[SplitViewPages.Home]).IsNumThreadsEnabled = false;
            }
        }
        else
        {
            // Not mining so enable number of threads and set Start Mining
            if (((HomeViewModel)ViewModelPagesDictionary[SplitViewPages.Home]).StartStopMining.Equals(MinerStatus.StopMining))
            {
                ((HomeViewModel)ViewModelPagesDictionary[SplitViewPages.Home]).StartStopMining = MinerStatus.StartMining;
            }
            if (!((HomeViewModel)ViewModelPagesDictionary[SplitViewPages.Home]).IsNumThreadsEnabled)
            {
                ((HomeViewModel)ViewModelPagesDictionary[SplitViewPages.Home]).IsNumThreadsEnabled = true;
            }
        }

        // Status Bar
        DaemonStatus = "Connections: " + GlobalData.NetworkStats.ConnectionsOut + "(out) + " + GlobalData.NetworkStats.ConnectionsIn + "(in) " + GlobalData.NetworkStats.StatusSync;
        DaemonVersion = "Version: " + GlobalData.NetworkStats.Version;
    }

    // TODO: Move this somewhere else.
    public void StartMasterUpdateProcess()
    {
        try
        {
            Logger.LogDebug("Main.SMUP", "Start Master Update Process");

            if (_daemonUpdateTimer == null)
            {
                _daemonUpdateTimer = new System.Timers.Timer();
                _daemonUpdateTimer.Interval = _daemonTimerInterval;
                _daemonUpdateTimer.Elapsed += (s, e) => MasterUpdateProcess();
                _daemonUpdateTimer.Start();

                Logger.LogDebug("Main.SMUP", "Master timer will start in " + _daemonTimerInterval / 1000 + " seconds");
            }
        }
        catch (Exception ex)
        {
            Logger.LogException("Main.SMUP", ex);
        }
    }

    public void MasterUpdateProcess()
    {
        try
        {
            if (_daemonUpdateTimer != null)
            {
                _daemonUpdateTimer.Stop();
            }

            // If kill master process is issued at any point, skip everything else and do not restrt master timer

            if (_cliToolsRunningLastCheck.AddSeconds(5) < DateTime.Now)
            {
                _cliToolsRunningLastCheck = DateTime.Now;

                if (!_killMasterProcess)
                {
                    KeepDaemonRunning();
                }


                if (!_killMasterProcess)
                {
                    //KeepWalletProcessRunning();
                }
            }


            // Update UI
            if (!_killMasterProcess)
            {
                DaemonUiUpdate();
            }
        }
        catch (Exception ex)
        {
            Logger.LogException("Main.MUP", ex);
        }
        finally
        {
            // Restart timer
            if (_daemonUpdateTimer == null)
            {
                Logger.LogError("Main.MUP", "Timer is NULL. Recreating. Why?");
                _daemonUpdateTimer = new System.Timers.Timer();
                _daemonUpdateTimer.Interval = _daemonTimerInterval;
                _daemonUpdateTimer.Elapsed += (s, e) => MasterUpdateProcess();
            }

            if (!_killMasterProcess)
            {
                _daemonUpdateTimer.Start();
            }
        }
    }

    private void KeepDaemonRunning()
    {
        try
        {
            bool forceRestart = false;
            if (_lastDaemonResponseTime.AddMinutes(5) < DateTime.Now)
            {
                // Daemon not responding.  Kill and restart
                forceRestart = true;
                _lastDaemonResponseTime = DateTime.Now;
                Logger.LogDebug("Main.KDR", "No response from daemon since: " + _lastDaemonResponseTime.ToLongTimeString() + " . Forcing restart...");
            }

            if (!ProcessManager.IsRunning(FileNames.NERVA_DAEMON, out Process? process) || forceRestart)
            {
                if (FileNames.DirectoryContainsCliTools(GlobalData.CliToolsDir))
                {
                    DaemonProcess.ForceClose();
                    Logger.LogDebug("Main.KDR", "Starting daemon process");
                    ProcessManager.StartExternalProcess(GlobalMethods.GetDaemonPath(), DaemonProcess.GenerateCommandLine());
                }
                else
                {
                    Logger.LogDebug("Main.KDR", "CLI tools not found");
                }
            }
        }
        catch (Exception ex)
        {
            Logger.LogException("Main.KDR", ex);
        }
    }    

    public async void DaemonUiUpdate()
    {
        try
        {
            GetInfoResponse infoRes = await GetInfo.CallServiceAsync();

            if (!string.IsNullOrEmpty(infoRes.version))
            {
                GlobalData.NetworkStats.NetHeight = (infoRes.target_height > infoRes.height ? infoRes.target_height : infoRes.height);
                GlobalData.NetworkStats.YourHeight = infoRes.height;
                GlobalData.NetworkStats.NetHash = Math.Round(((infoRes.difficulty / 60.0d) / 1000.0d), 2) + " kH/s";

                DateTime miningStartTime = GlobalMethods.UnixTimeStampToDateTime(infoRes.start_time);
                GlobalData.NetworkStats.RunTime = (DateTime.Now.ToUniversalTime() - miningStartTime).ToString(@"%d\.hh\:mm\:ss");

                GlobalData.NetworkStats.ConnectionsIn = infoRes.incoming_connections_count;
                GlobalData.NetworkStats.ConnectionsOut = infoRes.outgoing_connections_count;
                GlobalData.NetworkStats.Difficulty = infoRes.difficulty;

                GlobalData.NetworkStats.Version = infoRes.version;
                GlobalData.NetworkStats.StatusSync = "";
                if (infoRes.target_height != 0 && infoRes.height < infoRes.target_height)
                {
                    GlobalData.NetworkStats.StatusSync += " | Synchronizing (Height " + infoRes.height + " of " + infoRes.target_height + ")";
                }
                else
                {
                    GlobalData.NetworkStats.StatusSync += "  |  Sync OK";
                }

                GlobalData.NetworkStats.StatusSync += "  |  Status " + infoRes.status;


                Logger.LogDebug("Main.DUU", "GetInfo Response Height: " + infoRes.height);


                MiningStatusResponse miningRes = await Rpc.Daemon.MiningStatus.CallServiceAsync();
                if (miningRes.active)
                {
                    GlobalData.NetworkStats.MinerStatus = MinerStatus.Mining;
                    GlobalData.NetworkStats.MiningAddress = GlobalMethods.WalletAddressShortForm(miningRes.address);

                    if (miningRes.speed > 1000)
                    {
                        GlobalData.NetworkStats.YourHash = miningRes.speed / 1000.0d + " kH/s";
                    }
                    else
                    {
                        GlobalData.NetworkStats.YourHash = miningRes.speed + " h/s";
                    }

                    if (GlobalData.NetworkStats.Difficulty > 0)
                    {
                        double blockMinutes = ((GlobalData.NetworkStats.Difficulty / 60.0d) / miningRes.speed);

                        if ((blockMinutes / 1440d) > 1)
                        {
                            GlobalData.NetworkStats.BlockTime = String.Format("{0:F1}", Math.Round(blockMinutes, 1) / 1440d) + " days (est)";
                        }
                        else if ((blockMinutes / 60.0d) > 1)
                        {
                            GlobalData.NetworkStats.BlockTime = String.Format("{0:F1}", Math.Round(blockMinutes, 1) / 60.0d) + " hours (est)";
                        }
                        else
                        {
                            GlobalData.NetworkStats.BlockTime = String.Format("{0:F0}", Math.Round(blockMinutes, 0)) + " minutes (est)";
                        }
                    }
                }
                else
                {
                    GlobalData.NetworkStats.MinerStatus = MinerStatus.Inactive;
                    GlobalData.NetworkStats.MiningAddress = "None";
                    GlobalData.NetworkStats.YourHash = "0 h/s";
                    GlobalData.NetworkStats.BlockTime = "∞";
                }


                List<GetConnectionsResponse> connectResp = await GetConnections.CallServiceAsync();

                // TODO: For now just recreate items in grid. Consider, removing disconnected and adding new ones to List instead.
                GlobalData.Connections = new();

                foreach (GetConnectionsResponse connection in connectResp)
                {
                    GlobalData.Connections.Add(new Objects.Connection
                    {
                        Address = connection.address,
                        Height = connection.height,
                        LiveTime = TimeSpan.FromSeconds(connection.live_time).ToString(@"hh\:mm\:ss"),
                        State = connection.state,
                        IsIncoming = connection.incoming,
                        InOutIcon = connection.incoming ? _inImage : _outImage
                    });
                }

                UpdateView();
            }
        }
        catch (Exception ex)
        {
            Logger.LogException("Main.DUU", ex);
        }
    }    
}