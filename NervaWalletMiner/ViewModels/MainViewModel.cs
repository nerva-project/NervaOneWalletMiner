using Avalonia.Controls;
using Avalonia.Controls.Selection;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using NervaWalletMiner.Helpers;
using NervaWalletMiner.Objects;
using NervaWalletMiner.Rpc.Daemon;
using NervaWalletMiner.Views;
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
    public const int _daemonTimerInterval = 5000;      // TODO: Change to 2000
    public static bool _killMasterProcess = false;
    public static DateTime _cliToolsRunningLastCheck = DateTime.MinValue;
    private bool _isInitialDaemonConnectionSuccess = false;

    public static readonly Bitmap _inImage = new Bitmap(AssetLoader.Open(new Uri("avares://NervaWalletMiner/Assets/transfer_in.png")));
    public static readonly Bitmap _outImage = new Bitmap(AssetLoader.Open(new Uri("avares://NervaWalletMiner/Assets/transfer_out.png")));

    private bool? _isPaneOpen = false;
    private UserControl _CurrentPage;
    public SelectionModel<ListBoxItem> Selection { get; }
    public ICommand TriggerPaneCommand { get; }
    public ICommand OpenDebugFolderCommand { get; }

    public MainViewModel()
    {
        OpenDebugFolderCommand = ReactiveCommand.Create(OpenDebugFolder);        

        TriggerPaneCommand = ReactiveCommand.Create(TriggerPane);

        _CurrentPage = new HomeView();

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

    public UserControl CurrentPage
    {
        get { return _CurrentPage; }
        private set { this.RaiseAndSetIfChanged(ref _CurrentPage, value); }
    }

    void SelectionChanged(object sender, SelectionModelSelectionChangedEventArgs e)
    {
        // TODO: Figoure out better way of doing this
        switch(((ListBoxItem)e.SelectedItems[0]!).Name)
        {
            case "wallet":
                CurrentPage = new WalletView();
                break;
            case "transfers":
                CurrentPage = new TransfersView();
                break;
            case "settings":
                CurrentPage = new SettingsView();
                break;
            default:
                CurrentPage = new HomeView();
                break;
        }
    }

    // Daemon View
    private string _StartStopMining = MinerStatus.StartMining;
    public string StartStopMining
    {
        get => _StartStopMining;
        set => this.RaiseAndSetIfChanged(ref _StartStopMining, value);
    }

    private bool _IsNumThreadsEnabled = true;
    public bool IsNumThreadsEnabled
    {
        get => _IsNumThreadsEnabled;
        set => this.RaiseAndSetIfChanged(ref _IsNumThreadsEnabled, value);
    }

    private string _NetHeight = "0";
    public string NetHeight
    {
        get => _NetHeight;
        set => this.RaiseAndSetIfChanged(ref _NetHeight, value);
    }

    private string _YourHeight = "0";
    public string YourHeight
    {
        get => _YourHeight;
        set => this.RaiseAndSetIfChanged(ref _YourHeight, value);
    }

    private string _NetHash = "0";
    public string NetHash
    {
        get => _NetHash;
        set => this.RaiseAndSetIfChanged(ref _NetHash, value);
    }

    private string _RunTime = "0:0:0";
    public string RunTime
    {
        get => _RunTime;
        set => this.RaiseAndSetIfChanged(ref _RunTime, value);
    }

    private string _MinerMessage = "";
    public string MinerMessage
    {
        get => _MinerMessage;
        set => this.RaiseAndSetIfChanged(ref _MinerMessage, value);
    }

    private string _YourHash = "0";
    public string YourHash
    {
        get => _YourHash;
        set => this.RaiseAndSetIfChanged(ref _YourHash, value);
    }

    private string _BlockTime = "∞";
    public string BlockTime
    {
        get => _BlockTime;
        set => this.RaiseAndSetIfChanged(ref _BlockTime, value);
    }

    private string _MiningAddress = "";
    public string MiningAddress
    {
        get => _MiningAddress;
        set => this.RaiseAndSetIfChanged(ref _MiningAddress, value);
    }

    private List<Connection> _Connections = new();
    public List<Connection> Connections
    {
        get => _Connections;
        set => this.RaiseAndSetIfChanged(ref _Connections, value);
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

    // Wallet View
    private string _OpenCloseWallet = "Open Wallet";
    public string OpenCloseWallet
    {
        get => _OpenCloseWallet;
        set => this.RaiseAndSetIfChanged(ref _OpenCloseWallet, value);
    }

    private string _TotalXnv = "";
    public string TotalXnv
    {
        get => _TotalXnv;
        set => this.RaiseAndSetIfChanged(ref _TotalXnv, value);
    }

    private string _UnlockedXnv = "";
    public string UnlockedXnv
    {
        get => _UnlockedXnv;
        set => this.RaiseAndSetIfChanged(ref _UnlockedXnv, value);
    }

    private List<Wallet> _WalletAddresses = new();
    public List<Wallet> WalletAddresses
    {
        get => _WalletAddresses;
        set => this.RaiseAndSetIfChanged(ref _WalletAddresses, value);
    }


    private void TriggerPane()
    {
        IsPaneOpen = !IsPaneOpen;
    }

    private void OpenDebugFolder()
    {
        try
        {
            ProcessStartInfo psi = new ProcessStartInfo
            {
                FileName = GlobalData.LogDir,
                UseShellExecute = true
            };
            Process.Start(psi);
        }
        catch (Exception ex)
        {
            Logger.LogException("Mai.ODF", ex);
        }
    }

    public void UpdateView()
    {
        NetHeight = GlobalData.NetworkStats.NetHeight.ToString();
        YourHeight = GlobalData.NetworkStats.YourHeight.ToString();
        NetHash = GlobalData.NetworkStats.NetHash;
        RunTime = GlobalData.NetworkStats.RunTime;

        MinerMessage = GlobalData.NetworkStats.MinerStatus;
        YourHash = GlobalData.NetworkStats.YourHash;
        BlockTime = GlobalData.NetworkStats.BlockTime;
        MiningAddress = GlobalData.NetworkStats.MiningAddress;
        
        Connections = GlobalData.Connections;

        if(GlobalData.NetworkStats.MinerStatus.Equals(MinerStatus.Mining))
        {
            // Mining so disable number of threads and show Stop Mining
            if (StartStopMining.Equals(MinerStatus.StartMining))
            {
                StartStopMining = MinerStatus.StopMining;                
            }
            if(IsNumThreadsEnabled)
            {
                IsNumThreadsEnabled = false;
            }
        }
        else
        {
            // Not mining so enable number of threads and set Start Mining
            if (StartStopMining.Equals(MinerStatus.StopMining))
            {
                StartStopMining = MinerStatus.StartMining;
            }
            if (!IsNumThreadsEnabled)
            {
                IsNumThreadsEnabled = true;
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
            Logger.LogDebug("Mai.SMUP", "Start Master Update Process");

            if (_daemonUpdateTimer == null)
            {
                _daemonUpdateTimer = new System.Timers.Timer();
                _daemonUpdateTimer.Interval = _daemonTimerInterval;
                _daemonUpdateTimer.Elapsed += (s, e) => MasterUpdateProcess();
                _daemonUpdateTimer.Start();

                Logger.LogDebug("Mai.SMUP", "Master timer will start in " + _daemonTimerInterval / 1000 + " seconds");
            }
        }
        catch (Exception ex)
        {
            Logger.LogException("Mai.SMUP", ex);
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
                    //KeepDaemonRunning();
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
            Logger.LogException("Mai.MUP", ex);
        }
        finally
        {
            // Restart timer
            if (_daemonUpdateTimer == null)
            {
                Logger.LogError("Mai.MUP", "Timer is NULL. Recreating. Why?");
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


                Logger.LogDebug("Mai.DUU", "GetInfo Response Height: " + infoRes.height);


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
            Logger.LogException("Mai.DUU", ex);
        }
    }    
}