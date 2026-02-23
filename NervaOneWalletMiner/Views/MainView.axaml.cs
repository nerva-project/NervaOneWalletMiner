using Avalonia.Controls;
using NervaOneWalletMiner.Helpers;
using System;

namespace NervaOneWalletMiner.Views;

public partial class MainView : UserControl
{
    public MainView()
    {
		try
		{
            InitializeComponent();

            if (GlobalData.AppSettings.Daemon[GlobalData.AppSettings.ActiveCoin].IsWalletOnly)
            {
                wallet.IsSelected = true;                
            }
            else
            {
                daemon.IsSelected = true;
            }

            ShowHideToolTips();
        }
		catch (Exception ex)
		{
            Logger.LogException("MAV.CONS", ex);
        }        
    }

    private void spvMain_PaneOpenedClosed(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        try
        {
            ShowHideToolTips();
        }
        catch (Exception ex)
        {
            Logger.LogException("MAV.MPOC", ex);
        }
    }

    private void ShowHideToolTips()
    {
        try
        {
            if (spvMain.IsPaneOpen)
            {
                ToolTip.SetTip(daemon, null);
                ToolTip.SetTip(wallet, null);
                ToolTip.SetTip(transfers, null);
                ToolTip.SetTip(address_book, null);
                ToolTip.SetTip(daemon_setup, null);
                ToolTip.SetTip(wallet_setup, null);
                ToolTip.SetTip(settings, null);
                ToolTip.SetTip(about, null);
                ToolTip.SetTip(btnLines, "Collapse");
            }
            else
            {
                ToolTip.SetTip(daemon, "Daemon");
                ToolTip.SetTip(wallet, "Wallet");
                ToolTip.SetTip(transfers, "Transfers");
                ToolTip.SetTip(address_book, "Address Book");
                ToolTip.SetTip(daemon_setup, "Daemon Setup");
                ToolTip.SetTip(wallet_setup, "Wallet Setup");
                ToolTip.SetTip(settings, "Settings");
                ToolTip.SetTip(about, "About");
                ToolTip.SetTip(btnLines, "Expand");
            }
        }
        catch (Exception ex)
        {
            Logger.LogException("MAV.SHTT", ex);
        }
    }
}