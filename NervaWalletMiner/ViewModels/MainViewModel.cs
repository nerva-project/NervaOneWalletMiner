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

namespace NervaWalletMiner.ViewModels;

public class MainViewModel : ViewModelBase
{
    public static System.Timers.Timer? _masterTimer;
    public const int _masterTimerInterval = 5000;      // TODO: Change to 2000
    public static bool _killMasterProcess = false;
    public static DateTime _cliToolsRunningLastCheck = DateTime.MinValue;
    private bool isInitialDaemonConnectionSuccess = false;

    public static readonly Bitmap _inImage = new Bitmap(AssetLoader.Open(new Uri("avares://NervaWalletMiner/Assets/transfer_in.png")));
    public static readonly Bitmap _outImage = new Bitmap(AssetLoader.Open(new Uri("avares://NervaWalletMiner/Assets/transfer_out.png")));

    private bool? _isPaneOpen = true;
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

        _Connections = [];

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
        switch(((ListBoxItem)e.SelectedItems[0]).Name)
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

    // TODO: Getting exception when this was in HomeViewModel.cs: Could not find a matching property accessor "NetworkInfo" on ... MainViewModel
    private string? _NetHeight;
    public string? NetHeight
    {
        get => _NetHeight;
        set => this.RaiseAndSetIfChanged(ref _NetHeight, value);
    }

    private string? _YourHeight;
    public string? YourHeight
    {
        get => _YourHeight;
        set => this.RaiseAndSetIfChanged(ref _YourHeight, value);
    }

    private string? _NetHash;
    public string? NetHash
    {
        get => _NetHash;
        set => this.RaiseAndSetIfChanged(ref _NetHash, value);
    }

    private string? _RunTime;
    public string? RunTime
    {
        get => _RunTime;
        set => this.RaiseAndSetIfChanged(ref _RunTime, value);
    }

    private string? _MinerStatus;
    public string? MinerStatus
    {
        get => _MinerStatus;
        set => this.RaiseAndSetIfChanged(ref _MinerStatus, value);
    }

    private string? _YourHash;
    public string? YourHash
    {
        get => _YourHash;
        set => this.RaiseAndSetIfChanged(ref _YourHash, value);
    }

    private string? _BlockTime;
    public string? BlockTime
    {
        get => _BlockTime;
        set => this.RaiseAndSetIfChanged(ref _BlockTime, value);
    }

    private string? _MiningAddress;
    public string? MiningAddress
    {
        get => _MiningAddress;
        set => this.RaiseAndSetIfChanged(ref _MiningAddress, value);
    }

    private List<Connection> _Connections;
    public List<Connection> Connections
    {
        get => _Connections;
        set => this.RaiseAndSetIfChanged(ref _Connections, value);
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
            Logger.LogException("App.ODF", ex);
        }
    }

    // TODO: Figure out better way of doing this
    public void UpdateView()
    {        
        NetHeight = GlobalData.NetworkStats.NetHeight.ToString();
        YourHeight = GlobalData.NetworkStats.YourHeight.ToString();
        NetHash = GlobalData.NetworkStats.NetHash;
        RunTime = GlobalData.NetworkStats.RunTime;

        MinerStatus = GlobalData.NetworkStats.MinerStatus;
        YourHash = GlobalData.NetworkStats.YourHash;
        BlockTime = GlobalData.NetworkStats.BlockTime;
        MiningAddress = GlobalData.NetworkStats.MiningAddress;

        Connections = GlobalData.Connections;       
    }

    // TODO: Move this somewhere else.
    public void StartMasterUpdateProcess()
    {
        try
        {
            Logger.LogDebug("App.SMUP", "Start Master Update Process");

            if (_masterTimer == null)
            {
                _masterTimer = new System.Timers.Timer();
                _masterTimer.Interval = _masterTimerInterval;
                _masterTimer.Elapsed += (s, e) => MasterUpdateProcess();
                _masterTimer.Start();

                Logger.LogDebug("App.SMUP", "Master timer will start in 2 seconds");
            }
        }
        catch (Exception ex)
        {
            Logger.LogException("App.SMUP", ex);
        }
    }

    private void MasterUpdateProcess()
    {
        try
        {
            if (_masterTimer != null)
            {
                _masterTimer.Stop();
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

            if (!_killMasterProcess && isInitialDaemonConnectionSuccess)
            {
                //WalletUiUpdate();
            }

            UpdateView();
        }
        catch (Exception ex)
        {
            Logger.LogException("App.MUP", ex);
        }
        finally
        {
            // Restart timer
            if (_masterTimer == null)
            {
                Logger.LogError("App.MUP", "Timer is NULL. Recreating. Why?");
                _masterTimer = new System.Timers.Timer();
                _masterTimer.Interval = _masterTimerInterval;
                _masterTimer.Elapsed += (s, e) => MasterUpdateProcess();
            }

            if (!_killMasterProcess)
            {
                _masterTimer.Start();
            }
        }
    }

    public static async void DaemonUiUpdate()
    {
        try
        {
            GetInfoResponse infoRes = await GetInfo.CallServiceAsync();

            if(!string.IsNullOrEmpty(infoRes.version))
            {
                GlobalData.NetworkStats.NetHeight = (infoRes.target_height > infoRes.height ? infoRes.target_height : infoRes.height);
                GlobalData.NetworkStats.YourHeight = infoRes.height;
                GlobalData.NetworkStats.NetHash = Math.Round(((infoRes.difficulty / 60.0d) / 1000.0d), 2) + " kH/s";

                DateTime miningStartTime = GlobalMethods.UnixTimeStampToDateTime(infoRes.start_time);
                GlobalData.NetworkStats.RunTime = (DateTime.Now.ToUniversalTime() - miningStartTime).ToString(@"%d\.hh\:mm\:ss");

                GlobalData.NetworkStats.ConnectionsIn = infoRes.incoming_connections_count;
                GlobalData.NetworkStats.ConnectionsOut = infoRes.outgoing_connections_count;

                Logger.LogDebug("App.DUU", "GetInfo Response Height: " + infoRes.height);


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
            }
        }
        catch (Exception ex)
        {
            Logger.LogException("App.DUU", ex);
        }
    }
}