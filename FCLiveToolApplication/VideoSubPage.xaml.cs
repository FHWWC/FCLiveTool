using CommunityToolkit.Maui.Views;
using FCLiveToolApplication.Popup;
using System.Xml.Serialization;

namespace FCLiveToolApplication;

public partial class VideoSubPage : ContentPage
{
    public VideoSubPage()
    {
        InitializeComponent();
    }
    public static VideoSubPage videoSubPage;
    private void ContentPage_Loaded(object sender, EventArgs e)
    {
        if (videoSubPage != null)
        {
            return;
        }

        videoSubPage=this;
        ReadLocalSubList();
    }
    public async void ReadLocalSubList()
    {
        int permResult = await new APPPermissions().CheckAndReqPermissions();
        if (permResult!=0)
        {
            await DisplayAlert("提示信息", "请授权读取和写入权限，程序需要保存和读取文件！如果不授权，后续涉及文件读写的操作将无法正常使用！", "确定");
            return;
        }

        string dataPath = new APPFileManager().GetOrCreateAPPDirectory("AppData\\VideoSubList");
        if (dataPath != null)
        {
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(List<VideoSubList>));
            try
            {
                if (File.Exists(dataPath+"\\VideoSubList.log"))
                {
                    var localStr = File.ReadAllText(dataPath+"\\VideoSubList.log");
                    if (!string.IsNullOrWhiteSpace(localStr))
                    {
                        var tlist = (List<VideoSubList>)xmlSerializer.Deserialize(new StringReader(localStr));

                        await MainThread.InvokeOnMainThreadAsync(() =>
                        {
                            VideoSubList.ItemsSource = tlist;
                        });
                    }
                }
            }
            catch (Exception)
            {
                await DisplayAlert("提示信息", "读取本地数据时出错！", "确定");
            }

        }

    }
    private void VideoSubListAddItemBtn_Clicked(object sender, EventArgs e)
    {
        VideoSubListPopup videoSubListPopup = new VideoSubListPopup(0);
        //videoSubListPopup.MainGrid.HeightRequest = Window.Height/1.5;
        videoSubListPopup.MainGrid.WidthRequest =Window.Width/1.5;
        this.ShowPopup(videoSubListPopup);
    }

    private void VSLEditBtn_Clicked(object sender, EventArgs e)
    {
        Button button=sender as Button;

        VideoSubListPopup videoSubListPopup = new VideoSubListPopup(1,button.CommandParameter.ToString());
        //videoSubListPopup.MainGrid.HeightRequest = Window.Height/1.5;
        videoSubListPopup.MainGrid.WidthRequest =Window.Width/1.5;
        this.ShowPopup(videoSubListPopup);
    }

    private async void VSLRemoveBtn_Clicked(object sender, EventArgs e)
    {
        Button button = sender as Button;

        if(!await DisplayAlert("提示信息","你要删除订阅 "+ button.CommandParameter.ToString()+" 吗？", "确定", "取消"))
        {
            return;
        }
        if (string.IsNullOrWhiteSpace(button.CommandParameter.ToString()))
        {
            await DisplayAlert("提示信息", "参数错误！请重试！", "确定");
            return;
        }


        string dataPath = new APPFileManager().GetOrCreateAPPDirectory("AppData\\VideoSubList");
        if (dataPath != null)
        {
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(List<VideoSubList>));
            try
            {
                if (File.Exists(dataPath+"\\VideoSubList.log"))
                {
                    var tlist = (List<VideoSubList>)xmlSerializer.Deserialize(new StringReader(File.ReadAllText(dataPath+"\\VideoSubList.log")));

                    var items = tlist.FirstOrDefault(p => p.SubName==button.CommandParameter.ToString());
                    if (tlist != null)
                    {
                        tlist.Remove(items);
                        RefreshVSL(tlist);

                        using (StringWriter sw = new StringWriter())
                        {
                            xmlSerializer.Serialize(sw, tlist);
                            File.WriteAllText(dataPath+"\\VideoSubList.log", sw.ToString());
                        }

                        await DisplayAlert("提示信息", "删除订阅成功！", "确定");

                    }
                    else
                    {
                        await DisplayAlert("提示信息", "未查找到当前订阅名称对应的本地数据，请重试！", "确定");
                    }
                }
                else
                {
                    await DisplayAlert("提示信息", "源文件丢失！请重新创建！", "确定");
                }
            }
            catch (Exception)
            {
                await DisplayAlert("提示信息", "操作数据时出错，请刷新重试！", "确定");
            }

        }
        else
        {
            await DisplayAlert("提示信息", "源文件丢失！请重新创建！", "确定");
        }

    }

    private async void VSLEnabledUpdateToogle_Toggled(object sender, ToggledEventArgs e)
    {
        Switch button = sender as Switch;
        if (button.BindingContext is null)
        {
            return;
        }

        string itemSubName = (button.BindingContext as VideoSubList).SubName;
        if (string.IsNullOrWhiteSpace(itemSubName))
        {
            await DisplayAlert("提示信息", "参数错误！请重试！", "确定");
            return;
        }


        string dataPath = new APPFileManager().GetOrCreateAPPDirectory("AppData\\VideoSubList");
        if (dataPath != null)
        {
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(List<VideoSubList>));
            try
            {
                if (File.Exists(dataPath+"\\VideoSubList.log"))
                {
                    var tlist = (List<VideoSubList>)xmlSerializer.Deserialize(new StringReader(File.ReadAllText(dataPath+"\\VideoSubList.log")));

                    var items = tlist.FirstOrDefault(p => p.SubName==itemSubName);
                    if (tlist != null)
                    {
                        items.IsEnabledUpdate=button.IsToggled;
                        //不需要刷新，也避免了反复触发
                        //RefreshVSL(tlist);

                        using (StringWriter sw = new StringWriter())
                        {
                            xmlSerializer.Serialize(sw, tlist);
                            File.WriteAllText(dataPath+"\\VideoSubList.log", sw.ToString());
                        }

                    }
                    else
                    {
                        await DisplayAlert("提示信息", "未查找到当前订阅名称对应的本地数据，请重试！", "确定");
                    }
                }
                else
                {
                    await DisplayAlert("提示信息", "源文件丢失！请重新创建！", "确定");
                }
            }
            catch (Exception)
            {
                await DisplayAlert("提示信息", "操作数据时出错，请刷新重试！", "确定");
            }

        }
        else
        {
            await DisplayAlert("提示信息", "源文件丢失！请重新创建！", "确定");
        }

    }

    public async void PopShowMsg(string msg)
    {
        await DisplayAlert("提示信息", msg, "确定");
    }
    public async Task<bool> PopShowMsgAndReturn(string msg)
    {
        return await DisplayAlert("提示信息", msg, "确定", "取消");
    }
    public void RefreshVSL(List<VideoSubList> tlist)
    {
        VideoSubList.ItemsSource = tlist.Take(tlist.Count);
    }
    private void VSLUpdateBtn_Clicked(object sender, EventArgs e)
    {

    }
}