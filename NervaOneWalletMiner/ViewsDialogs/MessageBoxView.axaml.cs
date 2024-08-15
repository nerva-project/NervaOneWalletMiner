using Avalonia.Controls;
using Avalonia.Interactivity;
using NervaOneWalletMiner.Helpers;
using NervaOneWalletMiner.Objects;
using System;

namespace NervaOneWalletMiner.ViewsDialogs
{
    public partial class MessageBoxView : Window
    {
        // Not used but designer will complain without it
        public MessageBoxView()
        {
            InitializeComponent();         
        }

        public MessageBoxView(string title, string textMessage, bool hideCancelButton)
        {
            try
            {
                InitializeComponent();
                Icon = GlobalMethods.GetWindowIcon();

                Title = title;
                tbkMessage.Text = textMessage;

                if (hideCancelButton)
                {
                    btnCancel.IsVisible = false;                    
                }
                else
                {
                    btnCancel.IsVisible = true;
                }

                // Change height based on number of lines. Assume 70 characters per line
                double numberOfLines = Math.Ceiling(textMessage.Length / 70.0);
                Height = 100 + (numberOfLines * 18);
            }
            catch (Exception ex)
            {
                Logger.LogException("MBD.CONS", ex);
            }
        }

        public void OkButton_Clicked(object sender, RoutedEventArgs args)
        {
            try
            {
                DialogResult result = new()
                {
                    IsOk = true
                };

                Close(result);
            }
            catch (Exception ex)
            {
                Logger.LogException("MBD.OKBC", ex);
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
                Logger.LogException("MBD.CLBC", ex);
            }
        }
    }
}