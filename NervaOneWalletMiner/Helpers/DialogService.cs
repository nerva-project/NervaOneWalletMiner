using Avalonia.Controls;
using System;
using System.Threading.Tasks;

namespace NervaOneWalletMiner.Helpers
{
    public static class DialogService
    {
        private static Action<UserControl, Action<object?>>? _pushAction;
        private static Action<object?>? _popAction;

        public static void SetHost(Action<UserControl, Action<object?>> push, Action<object?> pop)
        {
            _pushAction = push;
            _popAction = pop;
        }

        public static async Task<TResult?> ShowAsync<TResult>(UserControl dialog)
        {
            var tcs = new TaskCompletionSource<object?>();
            _pushAction!(dialog, result => tcs.TrySetResult(result));
            var result = await tcs.Task;
            return result is TResult r ? r : default;
        }

        public static async Task ShowAsync(UserControl dialog)
        {
            await ShowAsync<object>(dialog);
        }

        public static void Close(object? result = null)
        {
            _popAction!(result);
        }
    }
}
