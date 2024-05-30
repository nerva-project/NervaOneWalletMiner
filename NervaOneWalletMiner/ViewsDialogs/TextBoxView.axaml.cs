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

        public TextBoxView()
        {
            InitializeComponent();
        }

        public TextBoxView(string title, string textValue, string textWatermark, string labelValue, bool isTextRequired)
        {
            InitializeComponent();

            _isTextRequired = isTextRequired;

            Title = title;
            tbxValue.Text = textValue;
            tbxValue.Watermark = textWatermark;
            lblValue.Content = labelValue;
        }

        public void OkButtonClicked(object sender, RoutedEventArgs args)
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
                Logger.LogException("TBWal.OBC", ex);
            }
        }

        public void CancelButtonClicked(object sender, RoutedEventArgs args)
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
                Logger.LogException("TBWal.CBC", ex);
            }
        }
    }
}
