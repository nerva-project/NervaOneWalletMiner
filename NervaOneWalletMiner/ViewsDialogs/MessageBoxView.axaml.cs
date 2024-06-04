using Avalonia.Controls;
using Avalonia.Interactivity;
using NervaOneWalletMiner.Helpers;
using NervaOneWalletMiner.Objects;
using System;

namespace NervaOneWalletMiner.ViewsDialogs
{
    public partial class MessageBoxView : Window
    {
        public MessageBoxView()
        {
            InitializeComponent();
        }

        public MessageBoxView(string title, string textMessage)
        {
            InitializeComponent();

            Title = title;
            tbkMessage.Text = textMessage;
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
                Logger.LogException("MBWal.OBC", ex);
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
                Logger.LogException("MBWal.CBC", ex);
            }
        }
    }
}