
using System.Threading.Tasks;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;

namespace FCLiveToolApplication.Services
{
    public class DialogService : IDialogService
    {
        public Task DisplayAlert(string title, string message, string cancel)
        {
            return MainThread.InvokeOnMainThreadAsync(() => Shell.Current.DisplayAlert(title, message, cancel));
        }

        public Task<bool> DisplayAlert(string title, string message, string accept, string cancel)
        {
            return MainThread.InvokeOnMainThreadAsync(() => Shell.Current.DisplayAlert(title, message, accept, cancel));
        }

        public Task<string?> DisplayPromptAsync(
            string title,
            string message,
            string accept = "确定",
            string cancel = "取消",
            string placeholder = "",
            int maxLength = -1,
            Keyboard? keyboard = null,
            string initialValue = "")
        {
            return MainThread.InvokeOnMainThreadAsync(() =>
                Shell.Current.DisplayPromptAsync(title, message, accept, cancel, placeholder, maxLength, keyboard, initialValue));
        }

        public Task<string?> DisplayActionSheet(string title, string cancel, string destruction, params string[] buttons)
        {
            return MainThread.InvokeOnMainThreadAsync(() =>
                Shell.Current.DisplayActionSheet(title, cancel, destruction, buttons));
        }
    }
}