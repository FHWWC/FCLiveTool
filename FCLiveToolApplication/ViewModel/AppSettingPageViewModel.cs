using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FCLiveToolApplication.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
#if WINDOWS
using Windows.UI.ViewManagement;
#endif

namespace FCLiveToolApplication.ViewModel
{
    public partial class AppSettingPageViewModel:ObservableObject
    {
        [ObservableProperty]
        private string defaultPlayM3U8Name = "";
        [ObservableProperty]
        private string defaultPlayM3U8URL = "";

        [ObservableProperty]
        public bool startAutoPlayToogle;
        [ObservableProperty]
        private int themeType;

        /// <summary>
        /// 0为跟随系统，1为强制浅色，2为强制深色
        /// </summary>
        /// <param name="value"></param>
        partial void OnThemeTypeChanged(int value)
        {
            switch (value)
            {
                case 0:
#if WINDOWS 

                    var uiSettings = new UISettings();
                    if (uiSettings.GetColorValue(UIColorType.Background) == Windows.UI.Color.FromArgb(255, 0, 0, 0))
                    {
                        Application.Current.UserAppTheme = AppTheme.Dark;
                    }
                    else
                    {
                        Application.Current.UserAppTheme = AppTheme.Light;
                    }
#else
bool isDark = Application.Current.RequestedTheme == AppTheme.Dark
              || Application.Current.UserAppTheme == AppTheme.Dark;
if (isDark)
{
    Application.Current.UserAppTheme = AppTheme.Dark;
}
else
{
    Application.Current.UserAppTheme = AppTheme.Light;
}
#endif
                    Preferences.Set("AppThemeType", 0);
                    break;
                case 1:
                    Application.Current.UserAppTheme = AppTheme.Light;
                    Preferences.Set("AppThemeType", 1);
                    break;
                case 2:
                    Application.Current.UserAppTheme = AppTheme.Dark;
                    Preferences.Set("AppThemeType", 2);
                    break;
            }
        }

        partial void OnStartAutoPlayToogleChanged(bool value)
        {
            if (value)
            {
                Preferences.Set("StartAutoPlayM3U8", true);
            }
            else
            {
                Preferences.Set("StartAutoPlayM3U8", false);
            }
        }

        private readonly IDialogService _dialogService;
        public AppSettingPageViewModel() : this(new DialogService())
        {

        }

        public AppSettingPageViewModel(IDialogService dialogService)
        {
            _dialogService = dialogService ?? new DialogService();

            DefaultPlayM3U8Name=Preferences.Get("DefaultPlayM3U8Name", "");
            DefaultPlayM3U8URL=Preferences.Get("DefaultPlayM3U8URL", "");
            StartAutoPlayToogle= Preferences.Get("StartAutoPlayM3U8", true);
            ThemeType= Preferences.Get("AppThemeType", 0);
        }
        [RelayCommand]
        private async Task ShowDPInput()
        {
            string urlnewvalue = await _dialogService.DisplayPromptAsync("设置默认值", "第1步 请输入新的直播源URL：", "更新", "取消", "URL...", -1, Keyboard.Text, "");
            if (string.IsNullOrWhiteSpace(urlnewvalue))
            {
                if (urlnewvalue!=null)
                    await _dialogService.DisplayAlert("提示信息", "请输入正确的内容！", "确定");
                return;
            }
            if (!urlnewvalue.Contains("://"))
            {
                await _dialogService.DisplayAlert("提示信息", "输入的内容不符合URL规范！", "确定");
                return;
            }

            string namenewvalue = await _dialogService.DisplayPromptAsync("设置默认值", "第2步 请输入新的名称：", "更新", "取消", "名称...", -1, Keyboard.Text, "");
            if (string.IsNullOrWhiteSpace(namenewvalue))
            {
                if (namenewvalue!=null)
                    await _dialogService.DisplayAlert("提示信息", "请输入正确的内容！", "确定");
                return;
            }

            Preferences.Set("DefaultPlayM3U8URL", urlnewvalue);
            DefaultPlayM3U8URL=urlnewvalue;
            Preferences.Set("DefaultPlayM3U8Name", namenewvalue);
            DefaultPlayM3U8Name=namenewvalue;

            await _dialogService.DisplayAlert("提示信息", "设置成功！再次打开APP后即可生效。", "确定");
        }

  
    }
}
