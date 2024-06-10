using Avalonia.Controls;
using Avalonia.Threading;
using NervaOneWalletMiner.Helpers;
using NervaOneWalletMiner.Objects;
using NervaOneWalletMiner.ViewsDialogs;
using System;
using System.Threading.Tasks;

namespace NervaOneWalletMiner.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
		try
		{
            InitializeComponent();

            Loaded += MainWindow_Loaded;
        }
		catch (Exception ex)
		{
            Logger.LogException("MAW.CONS", ex);
        }        
    }

    private void MainWindow_Loaded(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (!GlobalMethods.DirectoryContainsCliTools(GlobalData.CliToolsDir))
        {
            // CLI tools missing. Need to download
            string cliToolsLink = GlobalMethods.GetCliToolsDownloadLink();
            Logger.LogDebug("MAW.MAWL", "CLI tools not found. Asking user to confirm download link: " + cliToolsLink);

            var window = new TextBoxView("Get Client Tools", cliToolsLink, string.Empty, "Client Tools Download Link", true);
            window.ShowDialog(this).ContinueWith(CliToolsLinkDialogClosed);
        }
    }

    private async void CliToolsLinkDialogClosed(Task task)
    {
        try
        {
            DialogResult result = ((DialogResult)((Task<object>)task).Result);
            if (result != null && result.IsOk)
            {
                Logger.LogDebug("MAW.CTLC", "CLI tools not found. Attempting to download from: " + result.TextBoxValue);
                if (!string.IsNullOrEmpty(result.TextBoxValue))
                {
                    // Download and extract CLI tools
                    GlobalMethods.SetUpCliTools(result.TextBoxValue, GlobalData.CliToolsDir);
                }
            }
            else
            {
                Logger.LogDebug("MAW.CTLC", "CLI tools download cancelled.");
                await Dispatcher.UIThread.Invoke(async () =>
                {
                    MessageBoxView window = new("Client Tools Missing", "Software cannot run without client tools. Switch coin or restart to download client tools.", true);
                    await window.ShowDialog(this);
                });               
            }
        }
        catch (Exception ex)
        {
            Logger.LogException("MAW.CTLC", ex);
        }
    }
}