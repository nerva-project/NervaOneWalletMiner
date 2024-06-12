using Avalonia.Controls;
using Avalonia.Controls.Selection;
using Avalonia.Media.Imaging;
using NervaOneWalletMiner.Helpers;
using NervaOneWalletMiner.Objects.Constants;
using ReactiveUI;
using System.Windows.Input;

namespace NervaOneWalletMiner.ViewModels;

public class MainViewModel : ViewModelBase
{
    public delegate void CheckAndGetCliAction();
    public event CheckAndGetCliAction? CheckAndGetCliEvent;
    public void CheckAndGetCliTools()
    {
        CheckAndGetCliEvent!.Invoke();
    }

    public delegate void SyncWithQuickSyncAction(double percentSynced);
    public event SyncWithQuickSyncAction? SyncWithQuickSyncEvent;
    public void AskIfSyncWithQuickSync(double percentSynced)
    {
        SyncWithQuickSyncEvent!.Invoke(percentSynced);
    }


    public SelectionModel<ListBoxItem> Selection { get; }
    public ICommand TriggerPaneCommand { get; }


    private bool? _isPaneOpen = false;
    public bool? IsPaneOpen
    {
        get => _isPaneOpen;
        set => this.RaiseAndSetIfChanged(ref _isPaneOpen, value);
    }

    private ViewModelBase _CurrentPage;
    public ViewModelBase CurrentPage
    {
        get { return _CurrentPage; }
        set { this.RaiseAndSetIfChanged(ref _CurrentPage, value); }
    }

    private string _DaemonStatus = "";
    public string DaemonStatus
    {
        get => _DaemonStatus;
        set => this.RaiseAndSetIfChanged(ref _DaemonStatus, value);
    }

    // Status Bar
    private string _DaemonVersion = "";
    public string DaemonVersion
    {
        get => _DaemonVersion;
        set => this.RaiseAndSetIfChanged(ref _DaemonVersion, value);
    }

    private string _WalletStatus = "";
    public string WalletStatus
    {
        get => _WalletStatus;
        set => this.RaiseAndSetIfChanged(ref _WalletStatus, value);
    }

    private Bitmap _CoinIcon = GlobalData.Logo;
    public Bitmap CoinIcon
    {
        get => _CoinIcon;
        set => this.RaiseAndSetIfChanged(ref _CoinIcon, value);
    }

    private void TriggerPane()
    {
        IsPaneOpen = !IsPaneOpen;
    }


    public MainViewModel()
    {        
        // Need MainViewModel in UIManager
        UIManager.SetMainView(this);

        if (GlobalData.IsConfigFound)
        {
            UIManager.SetUpPages();
        }
        else
        {
            UIManager.SetUpFirstRun();            
        }

        _CurrentPage = GlobalData.ViewModelPages[SplitViewPages.Daemon];

        TriggerPaneCommand = ReactiveCommand.Create(TriggerPane);        
        Selection = new SelectionModel<ListBoxItem>();
        Selection.SelectionChanged += UIManager.SelectionChanged;
    }    
}