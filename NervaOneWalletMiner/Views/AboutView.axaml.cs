using Avalonia.Controls;
using Avalonia.Interactivity;
using NervaOneWalletMiner.Helpers;
using System.Diagnostics;
using System;

namespace NervaOneWalletMiner.Views
{
    public partial class AboutView : UserControl
    {
        public AboutView()
        {
            InitializeComponent();
        }

        public void OpenGitHub_Clicked(object sender, RoutedEventArgs args)
        {
            try
            {
                Process.Start(GlobalData.NervaOneGitHubLink);
            }
            catch (Exception ex)
            {
                Logger.LogException("Abo.OGHC", ex);
            }
        }

        public void OpenDiscord_Clicked(object sender, RoutedEventArgs args)
        {
            try
            {
                Process.Start(GlobalData.NervaDiscordLink);
            }
            catch (Exception ex)
            {
                Logger.LogException("Abo.ODC", ex);
            }
        }

        public void OpenTelegram_Clicked(object sender, RoutedEventArgs args)
        {
            try
            {
                Process.Start(GlobalData.NervaTelegramLink);
            }
            catch (Exception ex)
            {
                Logger.LogException("Abo.ODC", ex);
            }
        }
    }
}