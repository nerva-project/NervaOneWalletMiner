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

                Icon = GlobalMethods.GetWindowIcon();

                // TODO: Change view Height based on number of Message text lines

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