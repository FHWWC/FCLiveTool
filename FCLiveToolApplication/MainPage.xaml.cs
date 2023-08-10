

using Microsoft.Maui.Controls;

namespace FCLiveToolApplication;

public partial class MainPage : ContentPage
{
    public MainPage()
    {
        InitializeComponent();
    }
    VideoPrevPage videoPrevPage = new VideoPrevPage();
    private void ContentPage_Loaded(object sender, EventArgs e)
    {
        Application.Current.UserAppTheme = AppTheme.Light;
        /*
        Application.Current.RequestedThemeChanged += (s, a) =>
        {
            DisplayAlert("测试标题", "检测到系统主题已更换，但本应用依旧使用暗色主题", "确定");
        };
         */
        List<HamListClass> hamlist = new List<HamListClass>
        {
            new HamListClass() { ItemLogo= "video_page.png", ItemTitle="直播源预览" },
            new HamListClass() { ItemLogo="videoslist_page.png", ItemTitle="直播源列表" }
        };

        HamList.ItemsSource=hamlist;

        videoPrevPage.LoadRecent();
        NaviPage.Content=videoPrevPage.Content;
        
    }
    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        //this.Navigation.RemovePage(this);
        Navigation.PopAsync();
    }
    private void NaviBtn_Clicked(object sender, EventArgs e)
    {
        //如果是Grid布局控件，需要在前台代码中完成赋值
        if (LeftPanel.WidthRequest==50)
        {
            MoveLPAnimation(50,200);
            //Grid.SetColumnSpan(LeftPanel, 2);
        }
        else
        {
            MoveLPAnimation(200, 50);
            //Grid.SetColumnSpan(LeftPanel, 1);          
        }
    }
    private void HamList_ItemTapped(object sender, ItemTappedEventArgs e)
    {
        //最后收回
        if (LeftPanel.WidthRequest==200)
        {
            MoveLPAnimation(200,50);
        }

        switch(e.ItemIndex)
        {
            case 0:
                NaviPage.Content=videoPrevPage.Content;
                videoPrevPage.VideoWindow.Play();
                break;
            case 1:
                NaviPage.Content=new VideoListPage().Content;
                videoPrevPage.VideoWindow.Stop();
                break;
        }
    }
    /// <summary>
    /// 左侧汉堡菜单面板的过渡动画。
    /// </summary>
    /// <param name="fromP">之前的位置</param>
    /// <param name="toP">要移动到的位置</param>
    public void MoveLPAnimation(double fromP,double toP)
    {
        var animation = new Animation(v => LeftPanel.WidthRequest = v, fromP, toP);
        //收回时显示末尾减速动画
        if (fromP>toP)
        {
            animation.Commit(this, "LPAnimationBack", 16, 500, Easing.CubicInOut, (v, c) => { LeftPanel.WidthRequest=toP; c=true; }, () => false);
        }
        //展开时显示末尾弹簧动画
        else if (fromP<toP)
        {
            animation.Commit(this, "LPAnimationStart", 16, 500, Easing.BounceOut, (v, c) => { LeftPanel.WidthRequest=toP; c=true; }, () => false);
        }

        /*
                 var time = Math.Abs(from-to)/5;

                //忽略两个面板宽度的值相等，之前值大于之后值则递减，之前值小于之后值则递增
                if(from>to)
                {
                    for (double i = from; i >=to; i-=time)
                    {
                        LeftPanel.WidthRequest=i;
                        await Task.Delay(10);
                    }
                }
                //展开时显示弹簧动画
                else if(from<to)
                {
                    for (double i = from; i <=to; i+=time)
                    {
                        LeftPanel.WidthRequest=i;
                        await Task.Delay(10);
                    }
                }
         */

    }

    private void AboutBtn_Clicked(object sender, EventArgs e)
    {
 
    }

    private void SettingBtn_Clicked(object sender, EventArgs e)
    {

    }

    private void TapGestureRecognizer_Tapped(object sender, EventArgs e)
    {
        //最后收回
        if (LeftPanel.WidthRequest==200)
        {
            MoveLPAnimation(200, 50);
        }
    }

}

