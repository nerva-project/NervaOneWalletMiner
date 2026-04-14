using Avalonia.Controls;
using Avalonia.Interactivity;
using NervaOneWalletMiner.Objects;
using System;
using NervaOneWalletMiner.Helpers;

namespace NervaOneWalletMiner.ViewsDialogs
{
    public partial class TextBoxView : UserControl
    {
        bool _isTextRequired = false;
        string _title = string.Empty;

        // Not used but designer will complain without it
        public TextBoxView()
        {
            InitializeComponent();
        }

        public TextBoxView(string title, string labelValue, string textValue, string textWatermark, bool isTextRequired = true, bool isTextPassword = false, string okButtonText = "OK")
        {
            try
            {
                InitializeComponent();

                _isTextRequired = isTextRequired;
                _title = title;

                tbkTitle.Text = title;
                lblValue.Content = labelValue;
                tbxValue.Text = textValue;
                tbxValue.Watermark = textWatermark;
                btnOk.Content = okButtonText;

                if (isTextPassword)
                {
                    tbxValue.PasswordChar = '*';
                }

                Loaded += (_, _) => tbxValue.Focus();
            }
            catch (Exception ex)
            {
                Logger.LogException("TBD.CONS", ex);
            }
        }

        public async void OkButton_Clicked(object sender, RoutedEventArgs args)
        {
            try
            {
                var tbxValue = this.Get<TextBox>("tbxValue");

                if (_isTextRequired && string.IsNullOrEmpty(tbxValue.Text))
                {
                    await DialogService.ShowAsync<DialogResult>(new MessageBoxView(_title, "Value is required.", true));
                }
                else
                {
                    DialogResult result = new()
                    {
                        IsOk = true,
                        TextBoxValue = tbxValue.Text!,
                    };

                    DialogService.Close(result);
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

                DialogService.Close(result);
            }
            catch (Exception ex)
            {
                Logger.LogException("TBD.CLBC", ex);
            }
        }
    }
}
