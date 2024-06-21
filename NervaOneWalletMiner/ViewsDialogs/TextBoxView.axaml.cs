using Avalonia.Controls;
using Avalonia.Interactivity;
using NervaOneWalletMiner.Objects;
using System;
using NervaOneWalletMiner.Helpers;

namespace NervaOneWalletMiner.ViewsDialogs
{
    public partial class TextBoxView : Window
    {
        bool _isTextRequired = false;

        // Not used but designer will complain without it
        public TextBoxView()
        {
            InitializeComponent();
        }

        public TextBoxView(string title, string labelValue, string textValue, string textWatermark, bool isTextRequired = true, bool isTextPassword = false)
        {
            try
            {
                InitializeComponent();
                Icon = GlobalMethods.GetWindowIcon();

                _isTextRequired = isTextRequired;

                Title = title;
                lblValue.Content = labelValue;
                tbxValue.Text = textValue;
                tbxValue.Watermark = textWatermark;

                if (isTextPassword)
                {
                    tbxValue.PasswordChar = '*';
                }
            }
            catch (Exception ex)
            {
                Logger.LogException("TBD.CONS", ex);
            }
        }

        public void OkButton_Clicked(object sender, RoutedEventArgs args)
        {
            try
            {
                var tbxValue = this.Get<TextBox>("tbxValue");

                if(_isTextRequired && string.IsNullOrEmpty(tbxValue.Text))
                {
                    // TODO: Inform user that text is required
                }
                else
                {
                    DialogResult result = new()
                    {
                        IsOk = true,
                        TextBoxValue = tbxValue.Text!,
                    };

                    Close(result);
                }
            }
            catch (Exception ex)
            {
                Logger.LogException("TBD.OKBC", ex);
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
                Logger.LogException("TBD.CLBC", ex);
            }
        }
    }
}
