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
            VideoSubPage.videoSubPage.PopShowMsg("请授权读取和写入权限，程序需要保存和读取文件！如果不授权，后续涉及文件读写的操作将无法正常使用！");
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
                    VideoSubPage.videoSubPage.PopShowMsg("读取本地数据时出错！");
                    await CloseAsync();
                }

            }
            else
            {
                VideoSubPage.videoSubPage.PopShowMsg("源文件丢失！请重新创建！");
                await CloseAsync();
            }
        }
        else
        {
            if (string.IsNullOrWhiteSpace(ReceiveVideoSubName))
            {
                VideoSubPage.videoSubPage.PopShowMsg("参数错误！请重试！");
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
                            VideoSubPage.videoSubPage.PopShowMsg("未查找到当前订阅名称对应的本地数据，请重试！");
                            await CloseAsync();
                        }


                    }
                    else
                    {
                        VideoSubPage.videoSubPage.PopShowMsg("源文件丢失！请重新创建！");
                        await CloseAsync();
                    }
                }
                catch (Exception)
                {
                    VideoSubPage.videoSubPage.PopShowMsg("读取本地数据时出错！");
                    await CloseAsync();
                }

            }
            else
            {
                VideoSubPage.videoSubPage.PopShowMsg("源文件丢失！请重新创建！");
                await CloseAsync();
            }
        }

    }

    private void SubmitBtn_Clicked(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(VideoSubNameTb.Text))
        {
            VideoSubPage.videoSubPage.PopShowMsg("订阅名称不能为空！");
            return;
        }    
        if (string.IsNullOrWhiteSpace(VideoURLTb.Text))
        {
            VideoSubPage.videoSubPage.PopShowMsg("订阅地址不能为空！");
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
                            VideoSubPage.videoSubPage.PopShowMsg("当前输入的订阅名称已被使用，请更换名称！");
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
                            VideoSubPage.videoSubPage.RefreshVSL(tlist);

                            using (StringWriter sw = new StringWriter())
                            {
                                xmlSerializer.Serialize(sw, tlist);
                                File.WriteAllText(dataPath+"\\VideoSubList.log", sw.ToString());
                            }

                            VideoSubPage.videoSubPage.PopShowMsg("添加成功！");
                            CloseAsync();
                        }

                    }
                    else
                    {
                        VideoSubPage.videoSubPage.PopShowMsg("源文件丢失！请重新创建！");
                        CloseAsync();
                    }

                }
                catch (Exception)
                {
                    VideoSubPage.videoSubPage.PopShowMsg("更新数据时出错，请刷新重试！");
                    CloseAsync();
                }

            }
            else
            {
                VideoSubPage.videoSubPage.PopShowMsg("源文件丢失！请重新创建！");
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

                            VideoSubPage.videoSubPage.RefreshVSL(tlist);

                            using (StringWriter sw = new StringWriter())
                            {
                                xmlSerializer.Serialize(sw, tlist);
                                File.WriteAllText(dataPath+"\\VideoSubList.log", sw.ToString());
                            }

                            VideoSubPage.videoSubPage.PopShowMsg("修改成功！");
                            CloseAsync();
                        }
                        else
                        {
                            VideoSubPage.videoSubPage.PopShowMsg("未查找到当前订阅名称对应的本地数据，请重试！");
                            CloseAsync();
                        }

                    }
                    else
                    {
                        VideoSubPage.videoSubPage.PopShowMsg("源文件丢失！请重新创建！");
                        CloseAsync();
                    }
                }
                catch (Exception)
                {
                    VideoSubPage.videoSubPage.PopShowMsg("更新数据时出错，请刷新重试！");
                    CloseAsync();
                }

            }
            else
            {
                VideoSubPage.videoSubPage.PopShowMsg("源文件丢失！请重新创建！");
                CloseAsync();
            }
        }

    }

    private async void CancelBtn_Clicked(object sender, EventArgs e)
    {
        if (await VideoSubPage.videoSubPage.PopShowMsgAndReturn("你要取消操作吗？"))
        {
            await CloseAsync();
        }
    }
}