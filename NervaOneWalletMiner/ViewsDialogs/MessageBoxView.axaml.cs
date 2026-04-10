using Avalonia.Controls;
using Avalonia.Interactivity;
using NervaOneWalletMiner.Helpers;
using NervaOneWalletMiner.Objects;
using System;

namespace NervaOneWalletMiner.ViewsDialogs
{
    public partial class MessageBoxView : UserControl
    {
        // Not used but designer will complain without it
        public MessageBoxView()
        {
            InitializeComponent();
        }

        public MessageBoxView(string title, string textMessage, bool hideCancelButton) : this(title, textMessage, hideCancelButton, false) { }

        public MessageBoxView(string title, string textMessage, bool hideCancelButton, bool isYesNoButtons)
        {
            try
            {
                InitializeComponent();

                tbkTitle.Text = title;
                tbkMessage.Text = textMessage;

                if(isYesNoButtons)
                {
                    btnOk.Content = "Yes";
                    btnCancel.Content = "No";
                }

                if (hideCancelButton)
                {
                    btnCancel.IsVisible = false;                    
                }
                else
                {
                    btnCancel.IsVisible = true;
                }

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

                DialogService.Close(result);
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

                DialogService.Close(result);
            }
            catch (Exception ex)
            {
                Logger.LogException("MBD.CLBC", ex);
            }
        }
    }
}
