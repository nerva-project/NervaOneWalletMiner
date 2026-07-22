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

        private void StatusBarView_SizeChanged(object? sender, SizeChangedEventArgs e)
        {
            try
            {
                // Status bar spans the whole window while the other views sit inside the SplitView content, so this is
                // the 450 breakpoint used elsewhere plus the compact pane and content margin they lose
                if (e.NewSize.Width < 520)
                {
                    // Narrow: abbreviated height, percent instead of raw heights while syncing
                    lblDaemonStatus.IsVisible = false;
                    lblDaemonStatusShort.IsVisible = true;
                    lblWalletStatus.IsVisible = false;
                    lblWalletStatusShort.IsVisible = true;
                }
                else
                {
                    // Wide: full wording and both heights
                    lblDaemonStatus.IsVisible = true;
                    lblDaemonStatusShort.IsVisible = false;
                    lblWalletStatus.IsVisible = true;
                    lblWalletStatusShort.IsVisible = false;
                }
            }
            catch (Exception ex)
            {
                Logger.LogException("STB.SBSC", ex);
            }
        }
    }
}