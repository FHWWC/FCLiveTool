using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FCLiveToolApplication.Services;
using Microsoft.Maui.ApplicationModel;
using System.Threading.Tasks;

namespace FCLiveToolApplication.ViewModel
{
    public partial class AboutPageViewModel : ObservableObject
    {
        private readonly IDialogService _dialogService;

        [ObservableProperty]
        private string appVersion = "";

        // 无参构造：供 XAML 或手动 new 使用
        public AboutPageViewModel() : this(new DialogService())
        {
        }

        // 支持 DI 注入的构造函数
        public AboutPageViewModel(IDialogService dialogService)
        {
            _dialogService = dialogService ?? new DialogService();
            AppVersion = VersionTracking.CurrentVersion;
        }

        [RelayCommand]
        private async Task OpenGitHub()
        {
            try
            {
                await Launcher.OpenAsync("https://github.com/FHWWC/FCLiveTool");
            }
            catch
            {
                await _dialogService.DisplayAlert("提示信息", "无法打开浏览器。", "确定");
            }
        }

        [RelayCommand]
        private async Task OpenEmail()
        {
            try
            {
                bool opened = await Launcher.TryOpenAsync("mailto:justineedyoumost@163.com");
                if (!opened)
                {
                    await _dialogService.DisplayAlert("提示信息", "无法打开邮件客户端，如有需要请手动输入邮箱：\n justineedyoumost@163.com", "确定");
                }
            }
            catch
            {
                await _dialogService.DisplayAlert("提示信息", "无法打开邮件客户端，如有需要请手动输入邮箱：\n justineedyoumost@163.com", "确定");
            }
        }

        [RelayCommand]
        private async Task OpenGroup()
        {
            await _dialogService.DisplayAlert("提示信息", "我们的群组：\n TG群 t.me/fclivetoolgroup\n QQ群 877202338", "确定");
        }
    }
}
