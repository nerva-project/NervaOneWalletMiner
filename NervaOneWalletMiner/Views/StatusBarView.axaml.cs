using Avalonia.Controls;
using NervaOneWalletMiner.Helpers;
using System;

namespace NervaOneWalletMiner.Views
{
    public partial class StatusBarView : UserControl
    {
        public StatusBarView()
        {
            try
            {
                InitializeComponent();
            }
            catch (Exception ex)
            {
                Logger.LogException("STB.CONS", ex);
            }            
        }
    }
}