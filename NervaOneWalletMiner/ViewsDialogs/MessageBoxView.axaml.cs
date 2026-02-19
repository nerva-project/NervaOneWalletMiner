using Avalonia.Controls;
using Avalonia.Interactivity;
using NervaOneWalletMiner.Helpers;
using NervaOneWalletMiner.Objects;
using System;
using System.Linq;

namespace NervaOneWalletMiner.ViewsDialogs
{
    public partial class MessageBoxView : Window
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
                Icon = GlobalMethods.GetWindowIcon();

                Title = title;
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

                // Change height based on number of lines. Assume 70 characters per line
                double numberOfLines = Math.Ceiling(textMessage.Length / 70.0);
                int numberOfNewLines = textMessage.Where(x => (x == '\n')).Count();
                Height = 80 + (numberOfLines * 18) + (numberOfNewLines * 18);
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
