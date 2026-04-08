using Avalonia.Controls;
using System;
using System.Collections.Generic;

namespace NervaOneWalletMiner.Views
{
    public partial class OverlayDialogHost : UserControl
    {
        private readonly Stack<(UserControl dialog, Action<object?> callback)> _stack = new();

        public OverlayDialogHost()
        {
            InitializeComponent();
        }

        public void Push(UserControl dialog, Action<object?> callback)
        {
            _stack.Push((dialog, callback));
            cctDialog.Content = dialog;
            IsVisible = true;
        }

        public void Pop(object? result)
        {
            if (_stack.Count > 0)
            {
                var (_, callback) = _stack.Pop();
                callback(result);

                if (_stack.Count > 0)
                {
                    cctDialog.Content = _stack.Peek().dialog;
                }
                else
                {
                    IsVisible = false;
                    cctDialog.Content = null;
                }
            }
        }
    }
}
