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
        }
		catch (Exception ex)
		{
            Logger.LogException("MAV.CONS", ex);
        }        
    }
}