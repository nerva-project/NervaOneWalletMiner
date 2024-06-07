using Avalonia.Controls;
using NervaOneWalletMiner.Helpers;
using System;

namespace NervaOneWalletMiner.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
		try
		{
            InitializeComponent();
        }
		catch (Exception ex)
		{
            Logger.LogException("MAW.CONS", ex);
        }
        
    }
}