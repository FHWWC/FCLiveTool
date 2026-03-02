using System.Xml.Serialization;

namespace FCLiveToolApplication.Popup;

public partial class VideoSubListPopup : CommunityToolkit.Maui.Views.Popup
{
	public VideoSubListPopup(int popupType,string vsn="")
	{
		InitializeComponent();

        ReceiveVideoSubName = vsn;
        PopupType = popupType;
	}

    public VideoSubList CurrentItem = null;
    public string ReceiveVideoSubName;
    /// <summary>
    /// 0代表新建订阅，1代表编辑已有订阅。
    /// </summary>
    public int PopupType = 0;

    private async void MainGrid_Loaded(object sender, EventArgs e)
    {
        int permResult = await new APPPermissions().CheckAndReqPermissions();
        if (permResult!=0)
        {
#if ANDROID
            VideoSubPageAndroid.videoSubPageAndroid.PopShowMsg("请授权读取和写入权限，程序需要保存和读取文件！如果不授权，后续涉及文件读写的操作将无法正常使用！");
#else
            VideoSubPage.videoSubPage.PopShowMsg("请授权读取和写入权限，程序需要保存和读取文件！如果不授权，后续涉及文件读写的操作将无法正常使用！");
#endif
            await CloseAsync();
        }

        if(PopupType==0)
        {
            SubListManagerTitle.Text="添加订阅";

            string dataPath = new APPFileManager().GetOrCreateAPPDirectory("AppData\\VideoSubList");
            if (dataPath != null)
            {
                try
                {
                    if (!File.Exists(dataPath+"\\VideoSubList.log"))
                    {
                        File.Create(dataPath+"\\VideoSubList.log");
                    }

                    CurrentItem=new VideoSubList();

                }
                catch (Exception)
                {
#if ANDROID
                    VideoSubPageAndroid.videoSubPageAndroid.PopShowMsg("读取本地数据时出错！");
#else
                    VideoSubPage.videoSubPage.PopShowMsg("读取本地数据时出错！");
#endif
                    await CloseAsync();
                }

            }
            else
            {
#if ANDROID
                VideoSubPageAndroid.videoSubPageAndroid.PopShowMsg("源文件丢失！请重新创建！");
#else
                VideoSubPage.videoSubPage.PopShowMsg("源文件丢失！请重新创建！");
#endif
                await CloseAsync();
            }
        }
        else
        {
            if (string.IsNullOrWhiteSpace(ReceiveVideoSubName))
            {
#if ANDROID
                VideoSubPageAndroid.videoSubPageAndroid.PopShowMsg("参数错误！请重试！");
#else
                VideoSubPage.videoSubPage.PopShowMsg("参数错误！请重试！");
#endif
                await CloseAsync();
            }

            SubListManagerTitle.Text="编辑订阅";
            VideoSubNameTb.IsEnabled=false;

            string dataPath = new APPFileManager().GetOrCreateAPPDirectory("AppData\\VideoSubList");
            if (dataPath != null)
            {
                XmlSerializer xmlSerializer = new XmlSerializer(typeof(List<VideoSubList>));
                try
                {
                    if (File.Exists(dataPath+"\\VideoSubList.log"))
                    {
                        var tlist = (List<VideoSubList>)xmlSerializer.Deserialize(new StringReader(File.ReadAllText(dataPath+"\\VideoSubList.log")));

                        CurrentItem = tlist.FirstOrDefault(p => p.SubName==ReceiveVideoSubName);
                        if (CurrentItem != null)
                        {
                            VideoSubNameTb.Text=CurrentItem.SubName;
                            VideoTagTb.Text=CurrentItem.SubTag;
                            VideoURLTb.Text=CurrentItem.SubURL;
                            VideoEnabledUpdate.IsToggled=CurrentItem.IsEnabledUpdate;
                            VideoUATb.Text=CurrentItem.UserAgent;
                        }
                        else
                        {
#if ANDROID
                            VideoSubPageAndroid.videoSubPageAndroid.PopShowMsg("未查找到当前订阅名称对应的本地数据，请重试！");
#else
                            VideoSubPage.videoSubPage.PopShowMsg("未查找到当前订阅名称对应的本地数据，请重试！");
#endif
                            await CloseAsync();
                        }


                    }
                    else
                    {
#if ANDROID
                        VideoSubPageAndroid.videoSubPageAndroid.PopShowMsg("源文件丢失！请重新创建！");
#else
                        VideoSubPage.videoSubPage.PopShowMsg("源文件丢失！请重新创建！");
#endif
                        await CloseAsync();
                    }
                }
                catch (Exception)
                {
#if ANDROID
                    VideoSubPageAndroid.videoSubPageAndroid.PopShowMsg("读取本地数据时出错！");
#else
                    VideoSubPage.videoSubPage.PopShowMsg("读取本地数据时出错！");
#endif
                    await CloseAsync();
                }

            }
            else
            {
#if ANDROID
                VideoSubPageAndroid.videoSubPageAndroid.PopShowMsg("源文件丢失！请重新创建！");
#else
                VideoSubPage.videoSubPage.PopShowMsg("源文件丢失！请重新创建！");
#endif
                await CloseAsync();
            }
        }

    }

    private void SubmitBtn_Clicked(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(VideoSubNameTb.Text))
        {
#if ANDROID
            VideoSubPageAndroid.videoSubPageAndroid.PopShowMsg("订阅名称不能为空！");
#else
            VideoSubPage.videoSubPage.PopShowMsg("订阅名称不能为空！");
#endif
            return;
        }    
        if (string.IsNullOrWhiteSpace(VideoURLTb.Text))
        {
#if ANDROID
            VideoSubPageAndroid.videoSubPageAndroid.PopShowMsg("订阅地址不能为空！");
#else
            VideoSubPage.videoSubPage.PopShowMsg("订阅地址不能为空！");
#endif
            return;
        }


        if (PopupType==0)
        {
            string dataPath = new APPFileManager().GetOrCreateAPPDirectory("AppData\\VideoSubList");
            if (dataPath != null)
            {
                XmlSerializer xmlSerializer = new XmlSerializer(typeof(List<VideoSubList>));
                try
                {
                    if (File.Exists(dataPath+"\\VideoSubList.log"))
                    {
                        var localStr = File.ReadAllText(dataPath+"\\VideoSubList.log");
                        List<VideoSubList> tlist = new List<VideoSubList>();
                        if(!string.IsNullOrWhiteSpace(localStr))
                        {
                            tlist = (List<VideoSubList>)xmlSerializer.Deserialize(new StringReader(localStr));
                        }
          
                        var items = tlist.FirstOrDefault(p => p.SubName==VideoSubNameTb.Text);
                        if (items != null)
                        {
#if ANDROID
                            VideoSubPageAndroid.videoSubPageAndroid.PopShowMsg("当前输入的订阅名称已被使用，请更换名称！");
#else
                            VideoSubPage.videoSubPage.PopShowMsg("当前输入的订阅名称已被使用，请更换名称！");
#endif
                            return;
                        }
                        else
                        {
                            CurrentItem.SubName=VideoSubNameTb.Text;
                            CurrentItem.SubTag=VideoTagTb.Text;
                            CurrentItem.SubURL=VideoURLTb.Text;
                            CurrentItem.IsEnabledUpdate=VideoEnabledUpdate.IsToggled;
                            CurrentItem.UserAgent=VideoUATb.Text;

                            tlist.Add(CurrentItem);

#if ANDROID
                            VideoSubPageAndroid.videoSubPageAndroid.RefreshVSL(tlist);
#else
                            VideoSubPage.videoSubPage.RefreshVSL(tlist);
#endif

                            using (StringWriter sw = new StringWriter())
                            {
                                xmlSerializer.Serialize(sw, tlist);
                                File.WriteAllText(dataPath+"\\VideoSubList.log", sw.ToString());
                            }

#if ANDROID
                            VideoSubPageAndroid.videoSubPageAndroid.PopShowMsg("添加成功！");
#else
                            VideoSubPage.videoSubPage.PopShowMsg("添加成功！");
#endif
                            CloseAsync();
                        }

                    }
                    else
                    {
#if ANDROID
                        VideoSubPageAndroid.videoSubPageAndroid.PopShowMsg("源文件丢失！请重新创建！");
#else
                        VideoSubPage.videoSubPage.PopShowMsg("源文件丢失！请重新创建！");
#endif
                        CloseAsync();
                    }

                }
                catch (Exception)
                {
#if ANDROID
                    VideoSubPageAndroid.videoSubPageAndroid.PopShowMsg("更新数据时出错，请刷新重试！");
#else
                    VideoSubPage.videoSubPage.PopShowMsg("更新数据时出错，请刷新重试！");
#endif
                    CloseAsync();
                }

            }
            else
            {
#if ANDROID
                VideoSubPageAndroid.videoSubPageAndroid.PopShowMsg("源文件丢失！请重新创建！");
#else
                VideoSubPage.videoSubPage.PopShowMsg("源文件丢失！请重新创建！");
#endif
                CloseAsync();
            }
        }
        else
        {
            string dataPath = new APPFileManager().GetOrCreateAPPDirectory("AppData\\VideoSubList");
            if (dataPath != null)
            {
                XmlSerializer xmlSerializer = new XmlSerializer(typeof(List<VideoSubList>));
                try
                {
                    if (File.Exists(dataPath+"\\VideoSubList.log"))
                    {
                        var tlist = (List<VideoSubList>)xmlSerializer.Deserialize(new StringReader(File.ReadAllText(dataPath+"\\VideoSubList.log")));

                        var items = tlist.FirstOrDefault(p => p.SubName==ReceiveVideoSubName);
                        if (items != null)
                        {
                            items.SubTag=VideoTagTb.Text;
                            items.SubURL=VideoURLTb.Text;
                            items.IsEnabledUpdate=VideoEnabledUpdate.IsToggled;
                            items.UserAgent=VideoUATb.Text;

#if ANDROID
                            VideoSubPageAndroid.videoSubPageAndroid.RefreshVSL(tlist);
#else
                            VideoSubPage.videoSubPage.RefreshVSL(tlist);
#endif

                            using (StringWriter sw = new StringWriter())
                            {
                                xmlSerializer.Serialize(sw, tlist);
                                File.WriteAllText(dataPath+"\\VideoSubList.log", sw.ToString());
                            }

#if ANDROID
                            VideoSubPageAndroid.videoSubPageAndroid.PopShowMsg("修改成功！");
#else
                            VideoSubPage.videoSubPage.PopShowMsg("修改成功！");
#endif
                            CloseAsync();
                        }
                        else
                        {
#if ANDROID
                            VideoSubPageAndroid.videoSubPageAndroid.PopShowMsg("未查找到当前订阅名称对应的本地数据，请重试！");
#else
                            VideoSubPage.videoSubPage.PopShowMsg("未查找到当前订阅名称对应的本地数据，请重试！");
#endif
                            CloseAsync();
                        }

                    }
                    else
                    {
#if ANDROID
                        VideoSubPageAndroid.videoSubPageAndroid.PopShowMsg("源文件丢失！请重新创建！");
#else
                        VideoSubPage.videoSubPage.PopShowMsg("源文件丢失！请重新创建！");
#endif
                        CloseAsync();
                    }
                }
                catch (Exception)
                {
#if ANDROID
                    VideoSubPageAndroid.videoSubPageAndroid.PopShowMsg("更新数据时出错，请刷新重试！");
#else
                    VideoSubPage.videoSubPage.PopShowMsg("更新数据时出错，请刷新重试！");
#endif
                    CloseAsync();
                }

            }
            else
            {
#if ANDROID
                VideoSubPageAndroid.videoSubPageAndroid.PopShowMsg("源文件丢失！请重新创建！");
#else
                VideoSubPage.videoSubPage.PopShowMsg("源文件丢失！请重新创建！");
#endif
                CloseAsync();
            }
        }

    }

    private async void CancelBtn_Clicked(object sender, EventArgs e)
    {
#if ANDROID
        if (await VideoSubPageAndroid.videoSubPageAndroid.PopShowMsgAndReturn("操作还未保存，你确定要取消操作吗？"))
#else
        if (await VideoSubPage.videoSubPage.PopShowMsgAndReturn("操作还未保存，你确定要取消操作吗？"))
#endif
        {
            await CloseAsync();
        }
    }
}