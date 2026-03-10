using System.Threading.Tasks;

namespace FCLiveToolApplication.Services
{
    public interface IDialogService
    {
        Task DisplayAlert(string title, string message, string cancel);
        Task<bool> DisplayAlert(string title, string message, string accept, string cancel);
        Task<string?> DisplayPromptAsync(
            string title,
            string message,
            string accept = "确定",
            string cancel = "取消",
            string placeholder = "",
            int maxLength = -1,
            Keyboard? keyboard = null,
            string initialValue = "");
        Task<string?> DisplayActionSheet(string title, string cancel, string destruction, params string[] buttons);
    }
}