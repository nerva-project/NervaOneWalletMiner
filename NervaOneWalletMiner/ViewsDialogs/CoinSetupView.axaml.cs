using Avalonia.Controls;
using Avalonia.Interactivity;
using NervaOneWalletMiner.Helpers;
using NervaOneWalletMiner.Objects;
using NervaOneWalletMiner.ViewsDialogs;
using System;

namespace NervaOneWalletMiner;

public partial class CoinSetupView : Window
{
    Window GetWindow() => TopLevel.GetTopLevel(this) as Window ?? throw new NullReferenceException("Invalid Owner");

    public CoinSetupView()
    {
        try
        {
            InitializeComponent();

            imgCoinIcon.Source = GlobalMethods.GetLogo();

            tbxCliDownloadUrl.Text = GlobalMethods.GetCliToolsDownloadLink(GlobalData.AppSettings.ActiveCoin);
            tbxRemoteNode.Text = GlobalData.CoinSettings[GlobalData.AppSettings.ActiveCoin].PublicNodeUrlDefault;
        }
        catch (Exception ex)
        {
            Logger.LogException("CSD.CONS", ex);
        }

    }

    private void NodeType_IsCheckedChanged(object sender, RoutedEventArgs e)
    {
        try
        {
            if (sender is RadioButton radioButton)
            {
                switch(radioButton.Name)
                {
                    case "FullNode":
                        if (radioButton.IsChecked!.Value)
                        {
                            tbxRemoteNode.IsEnabled = false;
                        }
                        break;
                    case "RemoteNode":
                        if (radioButton.IsChecked!.Value)
                        {
                            tbxRemoteNode.IsEnabled = true;
                        }
                        break;
                }

            }
        }
        catch (Exception ex)
        {
            Logger.LogException("CSD.NTCC", ex);
        }
    }

    public void OkButton_Clicked(object sender, RoutedEventArgs args)
    {
        try
        {
            if (string.IsNullOrEmpty(tbxCliDownloadUrl.Text))
            {
                MessageBoxView window = new(Title!, "Client Tools Download URL is required.", true);
                window.ShowDialog(GetWindow());
            }
            else if((bool)RemoteNode.IsChecked! && string.IsNullOrEmpty(tbxRemoteNode.Text))
            {
                MessageBoxView window = new(Title!, "Remote Node is required.", true);
                window.ShowDialog(GetWindow());
            }
            else
            {
                if((bool)RemoteNode.IsChecked!)
                {
                    GlobalData.AppSettings.Daemon[GlobalData.AppSettings.ActiveCoin].IsWalletOnly = true;
                    GlobalData.AppSettings.Wallet[GlobalData.AppSettings.ActiveCoin].PublicNodeAddress = tbxRemoteNode.Text!.Trim();
                    GlobalMethods.SaveConfig();
                }

                DialogResult result = new()
                {
                    IsOk = true,
                    TextBoxValue = tbxCliDownloadUrl.Text!,
                };

                Close(result);
            }
        }
        catch (Exception ex)
        {
            Logger.LogException("CSD.OKBC", ex);
        }
    }
}