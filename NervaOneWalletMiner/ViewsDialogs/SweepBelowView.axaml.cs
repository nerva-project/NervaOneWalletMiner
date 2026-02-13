using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using NervaOneWalletMiner.Helpers;
using NervaOneWalletMiner.Objects;
using System;

namespace NervaOneWalletMiner.ViewsDialogs;

public partial class SweepBelowView : Window
{
    public SweepBelowView()
    {
        InitializeComponent();
    }

    public void RunButton_Clicked(object sender, RoutedEventArgs args)
    {
        try
        {
            //TODO: Get user values and call Wallet RPC method
        }
        catch (Exception ex)
        {
            Logger.LogException("SBD.RNBC", ex);
        }
    }

    public void CancelButton_Clicked(object sender, RoutedEventArgs args)
    {
        try
        {
            DialogResult result = new()
            {
                IsCancel = true
            };

            Close(result);
        }
        catch (Exception ex)
        {
            Logger.LogException("SBD.CLBC", ex);
        }
    }
}