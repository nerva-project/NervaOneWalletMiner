using Avalonia.Controls;
using Avalonia.Interactivity;
using NervaOneWalletMiner.Helpers;
using System;

namespace NervaOneWalletMiner.Views
{
    public partial class AboutView : UserControl
    {
        public AboutView()
        {
            InitializeComponent();
            imgCoinIcon.Source = GlobalMethods.GetLogo();

            tbkVersion.Text = "Version " + GlobalData.Version + " running on " + Environment.OSVersion.Platform + " " + Environment.OSVersion.Version + " with " + Environment.ProcessorCount + " CPU threads.";
        }

        public async void OpenGitHub_Clicked(object sender, RoutedEventArgs args)
        {
            try
            {
                await TopLevel.GetTopLevel(this)!.Launcher.LaunchUriAsync(new Uri(GlobalData.NervaOneGitHubLink));
            }
            catch (Exception ex)
            {
                Logger.LogException("ABO.OGHC", ex);
            }
        }

        public async void OpenDiscord_Clicked(object sender, RoutedEventArgs args)
        {
            try
            {
                await TopLevel.GetTopLevel(this)!.Launcher.LaunchUriAsync(new Uri(GlobalData.NervaDiscordLink));
            }
            catch (Exception ex)
            {
                Logger.LogException("ABO.ODC1", ex);
            }
        }

        public async void OpenTelegram_Clicked(object sender, RoutedEventArgs args)
        {
            try
            {
                await TopLevel.GetTopLevel(this)!.Launcher.LaunchUriAsync(new Uri(GlobalData.NervaTelegramLink));
            }
            catch (Exception ex)
            {
                Logger.LogException("ABO.OTC1", ex);
            }
        }
    }
}