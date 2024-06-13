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
            imgCoinIcon.Source = GlobalMethods.GetLogo();

            tbkVersion.Text = "Version " + GlobalData.Version + " running on " + Environment.OSVersion.Platform + " " + Environment.OSVersion.Version + " with " + Environment.ProcessorCount + " CPU threads.";
        }

        public void OpenGitHub_Clicked(object sender, RoutedEventArgs args)
        {
            try
            {
                ProcessStartInfo psi = new ProcessStartInfo
                {
                    FileName = GlobalData.NervaOneGitHubLink,
                    UseShellExecute = true
                };
                Process.Start(psi);
            }
            catch (Exception ex)
            {
                Logger.LogException("ABO.OGHC", ex);
            }
        }

        public void OpenDiscord_Clicked(object sender, RoutedEventArgs args)
        {
            try
            {
                ProcessStartInfo psi = new ProcessStartInfo
                {
                    FileName = GlobalData.NervaDiscordLink,
                    UseShellExecute = true
                };
                Process.Start(psi);
            }
            catch (Exception ex)
            {
                Logger.LogException("ABO.ODC1", ex);
            }
        }

        public void OpenTelegram_Clicked(object sender, RoutedEventArgs args)
        {
            try
            {
                ProcessStartInfo psi = new ProcessStartInfo
                {
                    FileName = GlobalData.NervaTelegramLink,
                    UseShellExecute = true
                };
                Process.Start(psi);
            }
            catch (Exception ex)
            {
                Logger.LogException("ABO.OTC1", ex);
            }
        }
    }
}