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
        }
		catch (Exception ex)
		{
            Logger.LogException("MAV.CONS", ex);
        }
        
    }
}