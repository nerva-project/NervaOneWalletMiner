using Avalonia.Controls;
using Avalonia.Controls.Selection;
using NervaWalletMiner.Helpers;
using NervaWalletMiner.Views;
using ReactiveUI;
using System.Diagnostics;
using System.Windows.Input;

namespace NervaWalletMiner.ViewModels;

public class MainViewModel : ViewModelBase
{
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
}