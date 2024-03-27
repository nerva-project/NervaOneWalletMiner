using Avalonia.Controls;
using Avalonia.Controls.Selection;
using NervaWalletMiner.Helpers;
using NervaWalletMiner.Rpc.Daemon;
using NervaWalletMiner.Views;
using ReactiveUI;
using System;
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
    private string? _NetworkInfo;
    public string? NetworkInfo
    {
        get => _NetworkInfo;
        set => this.RaiseAndSetIfChanged(ref _NetworkInfo, value);
    }

    private void TriggerPane()
    {
        IsPaneOpen = !IsPaneOpen;
    }

    private void OpenDebugFolder()
    {
        ProcessStartInfo psi = new ProcessStartInfo
        {
            FileName = GlobalData.LogDir,
            UseShellExecute = true
        };
        Process.Start(psi);
    }

    // TODO: Figure out better way of doing this
    public void UpdateView()
    {
        NetworkInfo = "Hash Rate: " + GlobalData.HashRate + ", Height: " + GlobalData.NetHeight + ", In: " + GlobalData.InConnections + ", Out: " + GlobalData.OutConnections;
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
            GetInfoResponse response = await GetInfo.CallServiceAsync();

            GlobalData.HashRate = response.cumulative_difficulty.ToString();
            GlobalData.NetHeight = response.height.ToString();
            GlobalData.OutConnections = Convert.ToInt32(response.outgoing_connections_count);
            GlobalData.InConnections = Convert.ToInt32(response.incoming_connections_count);

            Logger.LogDebug("App.DUU", "GetInfo Response Height: " + response.height);
        }
        catch (Exception ex)
        {
            Logger.LogException("App.DUU", ex);
        }
    }
}