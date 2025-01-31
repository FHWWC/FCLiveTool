using CommunityToolkit.Maui.Storage;
using Microsoft.Maui;
using System.Text;

namespace FCLiveToolApplication;

public partial class VideoEditPage : ContentPage
{
    public VideoEditPage()
    {
        InitializeComponent();
    }
    public static VideoEditPage videoEditPage;
    public List<LocalM3UList> CurrentLocalM3UList = new List<LocalM3UList>();
    public string CurrentSaveLocation = "";
    private void ContentPage_Loaded(object sender, EventArgs e)
    {
        if (videoEditPage!=null)
        {
            return;
        }

        videoEditPage=this;
        VideoEditList.ItemTemplate =new VideoEditListDataTemplateSelector();
    }

    private async void SelectLocalM3UFolderBtn_Clicked(object sender, EventArgs e)
    {
        int permResult = await new APPPermissions().CheckAndReqPermissions();
        if (permResult!=0)
        {
            await DisplayAlert("提示信息", "请授权读取和写入权限，程序需要读取文件！", "确定");
            return;
        }

        //每次点击按钮将当前列表数据放入变量里，在添加时进行对比
        if (LocalM3UList.ItemsSource is not null)
        {
            CurrentLocalM3UList=LocalM3UList.ItemsSource.Cast<LocalM3UList>().ToList();
        }
        //VideoWindow.Source=new Uri("C:\\Users\\Lee\\Desktop\\cgtn-f.m3u8");
        var folderPicker = await FolderPicker.PickAsync(FileSystem.AppDataDirectory, CancellationToken.None);

        if (folderPicker.IsSuccessful)
        {
            List<LocalM3UList> mlist = new List<LocalM3UList>();
            LoadM3UFileFromSystem(folderPicker.Folder.Path, ref mlist);
            CurrentLocalM3UList.AddRange(mlist);

            //重新添加序号
            int tmindex = 0;
            CurrentLocalM3UList.ForEach(p =>
            {
                p.ItemId="LMRB"+tmindex;
                tmindex++;
            });
            LocalM3UList.ItemsSource=CurrentLocalM3UList;
        }
        else
        {
            await DisplayAlert("提示信息", "您已取消了操作。", "确定");
        }
    }

    private async void SelectLocalM3UFileBtn_Clicked(object sender, EventArgs e)
    {
        int permResult = await new APPPermissions().CheckAndReqPermissions();
        if (permResult!=0)
        {
            await DisplayAlert("提示信息", "请授权读取和写入权限，程序需要读取文件！", "确定");
            return;
        }

        //每次点击按钮将当前列表数据放入变量里，在添加时进行对比
        if (LocalM3UList.ItemsSource is not null)
        {
            CurrentLocalM3UList=LocalM3UList.ItemsSource.Cast<LocalM3UList>().ToList();
        }

        var fileTypes = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
{
    { DevicePlatform.iOS, new[] { "com.apple.mpegurl" , "application/vnd.apple.mpegurl" } },
    { DevicePlatform.macOS, new[] {  "application/vnd.apple.mpegurl" } },
    { DevicePlatform.Android, new[] { "audio/x-mpegurl"  } },
    { DevicePlatform.WinUI, new[] { ".m3u"} }
});

        var filePicker = await FilePicker.PickMultipleAsync(new PickOptions()
        {
            PickerTitle="选择M3U文件",
            FileTypes=fileTypes
        });

        if (filePicker is not null&&filePicker.Count()>0)
        {
            filePicker.ToList().ForEach(p =>
            {
                if (CurrentLocalM3UList.Where(p2 => p2.FullFilePath==p.FullPath).Count()<1&&p.FileName.ToLower().EndsWith(".m3u"))
                {
                    CurrentLocalM3UList.Add(new LocalM3UList() { FileName=p.FileName, FilePath=p.FullPath.Replace(p.FileName, ""), FullFilePath=p.FullPath });
                }
            });

            //重新添加序号
            int tmindex = 0;
            CurrentLocalM3UList.ForEach(p =>
            {
                p.ItemId="LMRB"+tmindex;
                tmindex++;
            });
            LocalM3UList.ItemsSource=CurrentLocalM3UList;
        }
        else
        {
            await DisplayAlert("提示信息", "您已取消了操作。", "确定");
        }

    }

    private void ClearLocalM3UBtn_Clicked(object sender, EventArgs e)
    {
        LocalM3UList.ItemsSource=null;
        CurrentLocalM3UList.Clear();
    }

    public void LoadM3UFileFromSystem(string path, ref List<LocalM3UList> list)
    {
        foreach (string item in Directory.EnumerateFileSystemEntries(path).ToList())
        {
            if (File.GetAttributes(item).HasFlag(FileAttributes.Directory))
            {
                LoadM3UFileFromSystem(item, ref list);
            }
            else
            {
                if (item.ToLower().EndsWith(".m3u"))
                {
                    string tname;
#if ANDROID
                    tname = item.Substring(item.LastIndexOf("/")+1);
#else
                    tname = item.Substring(item.LastIndexOf("\\")+1);
#endif
                    //string tfoldername = "."+item.Replace(initFoldername, "").Replace(tname, "");
                    string tfoldername = item.Replace(tname, "");

                    if (CurrentLocalM3UList.Where(p => p.FullFilePath==item).Count()<1)
                    {
                        list.Add(new LocalM3UList() { FileName=tname, FilePath=tfoldername, FullFilePath=item });
                    }

                }

            }

        }
    }
    private async void LocalM3UList_ItemTapped(object sender, ItemTappedEventArgs e)
    {
        CurrentSaveLocation = "";
        int permResult = await new APPPermissions().CheckAndReqPermissions();
        if (permResult!=0)
        {
            await DisplayAlert("提示信息", "请授权读取和写入权限，程序需要读取文件！", "确定");
            return;
        }

        LocalM3UList m3ulist = e.Item as LocalM3UList;
        if (!File.Exists(m3ulist.FullFilePath))
        {
            await DisplayAlert("提示信息", "程序找不到该直播源本地文件，可能是该文件已被删除或移动。", "确定");
            return;
        }

        try
        {
            CurrentSaveLocation = m3ulist.FullFilePath;
            VideoEditList.ItemsSource=await new VideoManager().ReadM3UString(File.ReadAllText(m3ulist.FullFilePath));
        }
        catch (Exception)
        {
            await DisplayAlert("提示信息", "读取M3U文件数据时发生异常", "确定");
        }

#if ANDROID
        VideoEditPanelBtn_Clicked(sender, e);
#endif
    }
    private async void LocalM3URemoveBtn_Clicked(object sender, EventArgs e)
    {
        Button LMRBtn = sender as Button;
        List<LocalM3UList> tlist = LocalM3UList.ItemsSource.Cast<LocalM3UList>().ToList();

        var item = tlist.Where(p => p.ItemId==LMRBtn.CommandParameter.ToString()).FirstOrDefault();
        if (item is null)
        {
            await DisplayAlert("提示信息", "移除时发生异常", "确定");
            return;
        }
        tlist.Remove(item);

        LocalM3UList.ItemsSource=tlist;
    }
    private void LocalM3UList_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == "ItemsSource")
        {
            if (LocalM3UList.ItemsSource is not null&&LocalM3UList.ItemsSource.Cast<LocalM3UList>().Count()>0)
            {
                LocalM3UIfmText.Text="";
            }
            else
            {
                LocalM3UIfmText.Text="当前列表中没有M3U直播源文件，快去添加吧~";
            }
        }
    }

    private void VideoEditList_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == "ItemsSource")
        {
            if (VideoEditList.ItemsSource is not null&&VideoEditList.ItemsSource.Cast<VideoEditList>().Count()>0)
            {
                VideoEditIfmText.Text="";
            }
            else
            {
                VideoEditIfmText.Text="当前解析列表为空，请在左侧列表选一个M3U直播源";
            }
        }
    }

    private void VideoEditListEntry_TextChanged(object sender, TextChangedEventArgs e)
    {
        Entry entry = sender as Entry;
        if (entry != null)
        {
            if (entry.BindingContext is VideoEditListTVG tcontext)
            {
                string oldvalue = e.OldTextValue;
                string newvalue = e.NewTextValue;
                bool isNewStr = false;

                if (string.IsNullOrWhiteSpace(oldvalue)&&string.IsNullOrWhiteSpace(newvalue))
                {
                    return;
                }

                switch (entry.StyleId)
                {
                    case "VELTVG1":
                        if(!tcontext.AllStr.Contains("group-title="))
                        {
                            isNewStr=true;
                        }

                        oldvalue="group-title=\""+oldvalue+"\"";
                        newvalue="group-title=\""+newvalue+"\"";
                        break;
                    case "VELTVG2":
                        if (!tcontext.AllStr.Contains("tvg-group="))
                        {
                            isNewStr=true;
                        }

                        oldvalue="tvg-group=\""+oldvalue+"\"";
                        newvalue="tvg-group=\""+newvalue+"\"";
                        break;
                    case "VELTVG3":
                        if (!tcontext.AllStr.Contains("tvg-id="))
                        {
                            isNewStr=true;
                        }

                        oldvalue="tvg-id=\""+oldvalue+"\"";
                        newvalue="tvg-id=\""+newvalue+"\"";
                        break;
                    case "VELTVG4":
                        if (!tcontext.AllStr.Contains("tvg-logo="))
                        {
                            isNewStr=true;
                        }

                        oldvalue="tvg-logo=\""+oldvalue+"\"";
                        newvalue="tvg-logo=\""+newvalue+"\"";
                        break;
                    case "VELTVG5":
                        if (!tcontext.AllStr.Contains("tvg-country="))
                        {
                            isNewStr=true;
                        }

                        oldvalue="tvg-country=\""+oldvalue+"\"";
                        newvalue="tvg-country=\""+newvalue+"\"";
                        break;
                    case "VELTVG6":
                        if (!tcontext.AllStr.Contains("tvg-language="))
                        {
                            isNewStr=true;
                        }

                        oldvalue="tvg-language=\""+oldvalue+"\"";
                        newvalue="tvg-language=\""+newvalue+"\"";
                        break;
                    case "VELTVG7":
                        if (!tcontext.AllStr.Contains("tvg-name="))
                        {
                            isNewStr=true;
                        }

                        oldvalue="tvg-name=\""+oldvalue+"\"";
                        newvalue="tvg-name=\""+newvalue+"\"";
                        break;
                    case "VELTVG8":
                        if (!tcontext.AllStr.Contains("tvg-url="))
                        {
                            isNewStr=true;
                        }

                        oldvalue="tvg-url=\""+oldvalue+"\"";
                        newvalue="tvg-url=\""+newvalue+"\"";
                        break;
                }

                if(isNewStr)
                {
                    if(string.IsNullOrWhiteSpace(tcontext.AllStr))
                    {
                        tcontext.AllStr="#EXTINF: "+newvalue+",";
                    }
                    else
                    {
                        if (!tcontext.AllStr.StartsWith("#EXTINF:"))
                        {
                            tcontext.AllStr="#EXTINF: "+tcontext.AllStr;
                        }

                        var tindex = tcontext.AllStr.LastIndexOf(",");
                        if (tindex<0)
                        {
                            tcontext.AllStr+=" "+newvalue+",";
                        }
                        else
                        {
                            tcontext.AllStr=tcontext.AllStr.Insert(tindex, " "+newvalue);
                        }
                    }

                }
                else
                {
                    if (!tcontext.AllStr.StartsWith("#EXTINF:"))
                    {
                        tcontext.AllStr="#EXTINF: "+tcontext.AllStr;
                    }

                    tcontext.AllStr=tcontext.AllStr.Replace(oldvalue, newvalue);
                }

            }

            /*
                         var videoEditListTVG = VideoEditList.ItemsSource.Cast<VideoEditList>().Where(p => p.ItemName==test.ItemName).ToList().FirstOrDefault();
                        VideoEditListTVG test2 = videoEditListTVG as VideoEditListTVG;
                        test2.AllStr="你输入了"+entry.Text;
             */
        }
    }

    private async void VideoEditListAddItemBtn_Clicked(object sender, EventArgs e)
    {
        List<VideoEditList> videoEditList=new List<VideoEditList>();
        if (VideoEditList.ItemsSource != null)
        {
            videoEditList = VideoEditList.ItemsSource.Cast<VideoEditList>().ToList();
        }

        string MSelectResult = await DisplayActionSheet("你要添加什么数据？", "取消", null, new string[] { "TVG标签", "EXT标签", "直播源的地址", "M3U文件标头", "其它字符串" });
        if (MSelectResult == "取消"||MSelectResult is null)
        {
            return;
        }
        if (MSelectResult =="M3U文件标头")
        {
            if (videoEditList.Where(p => p.ItemTypeId==3).Count()>0)
            {
                await DisplayAlert("提示信息", "当前列表已经有M3U文件标头，如要添加新的请先移除之前的M3U文件标头。", "确定");
                return;
            }

            videoEditList.Insert(0, new VideoEditListEXT_Readonly() { ItemTypeId=3, EXTTag="#EXTM3U" });
            VideoEditList.ItemsSource=videoEditList;
            return;
        }

        string MSelectResult2 = await DisplayActionSheet("你要在列表哪个地方插入新行？", "取消", null, new string[] { "开头", "结尾", "列表选中的那一行的后面", "每一个已勾选的复选框对应的那一行的后面" });
        if (MSelectResult2 == "取消"||MSelectResult2 is null)
        {
            return;
        }


        switch (MSelectResult2)
        {
            case "开头":
                if(videoEditList.Count<1)
                {
                    await DisplayAlert("提示信息", "当前列表没有M3U文件标头，请先添加一个M3U文件标头！", "确定");
                    return;
                }
                if(videoEditList.Where(p=>p.ItemTypeId==3).Count()<1)
                {
                    videoEditList.Insert(0, GetTypeIDFromVEL(MSelectResult));
                }
                else
                {
                    videoEditList.Insert(1, GetTypeIDFromVEL(MSelectResult));
                }

                break;
            case "结尾":
                if (videoEditList.Count<1)
                {
                    await DisplayAlert("提示信息", "当前列表没有M3U文件标头，请先添加一个M3U文件标头！", "确定");
                    return;
                }

                videoEditList.Insert(videoEditList.Count, GetTypeIDFromVEL(MSelectResult));
                break;
            case "列表选中的那一行的后面":

                if (VideoEditList.SelectedItem is null)
                {
                    await DisplayAlert("提示信息", "你当前没有选择任何项，请点击列表选择一项，而非勾选复选框！", "确定");
                    return;
                }
                if (videoEditList.Count<1)
                {
                    await DisplayAlert("提示信息", "当前列表没有M3U文件标头，请先添加一个M3U文件标头！", "确定");
                    return;
                }

                videoEditList.Insert(videoEditList.IndexOf(VideoEditList.SelectedItem as VideoEditList)+1, GetTypeIDFromVEL(MSelectResult));

                break;
            case "每一个已勾选的复选框对应的那一行的后面":

                var tlist = videoEditList.Where(p => p.IsSelected==true).ToList();
                if (tlist.Count<1)
                {
                    await DisplayAlert("提示信息", "你当前没有勾选任何项，请至少勾选一个复选框！", "确定");
                    return;
                }
                for (int i = tlist.Count-1; i>=0; i--)
                {
                    videoEditList.Insert(videoEditList.IndexOf(tlist[i])+1, GetTypeIDFromVEL(MSelectResult));
                }

                /*
                                 tlist.OrderByDescending(p=>p).ToList().ForEach(p2 =>
                                {
                                    videoEditList.Insert(videoEditList.IndexOf(p2)+1, GetTypeIDFromVEL(MSelectResult));
                                });
                 */
                break;
        }

        VideoEditList.ItemsSource=videoEditList;
    }
    public VideoEditList GetTypeIDFromVEL(string keyword)
    {
        switch (keyword)
        {
            case "EXT标签":
                return new VideoEditListEXT() { ItemTypeId=1, EXTTag="" };
            case "TVG标签":
                return new VideoEditListTVG() { ItemTypeId=2, AllStr="", GroupTitle="", TVGGroup="", TVGID="", TVGLogo="", TVGCountry="", TVGLanguage="", TVGName="", TVGURL="" };
            case "直播源的地址":
                return new VideoEditListSourceLink() { ItemTypeId=4, SourceLink="" };
            case "其它字符串":
                return new VideoEditListOtherString() { ItemTypeId=5, AllStr="" };
        }
        return null;
    }
    private async void VideoEditListRemoveItemBtn_Clicked(object sender, EventArgs e)
    {
        if (VideoEditList.ItemsSource is null)
        {
            return;
        }
        var videoEditList = VideoEditList.ItemsSource.Cast<VideoEditList>().ToList();
        if (videoEditList.Count <1)
        {
            await DisplayAlert("提示信息", "当前列表没有任何项！", "确定");
            return;
        }

        string MSelectResult = await DisplayActionSheet("确定要删除指定内容吗？", "取消", null, new string[] { "删除列表选中的那一行", "删除每一个已勾选的复选框对应的那一行" });
        if (MSelectResult == "取消"||MSelectResult is null)
        {
            return;
        }
        if (MSelectResult.Contains("删除列表"))
        {
            if (VideoEditList.SelectedItem is null)
            {
                await DisplayAlert("提示信息", "你当前没有选择任何项，请点击列表选择一项，而非勾选复选框！", "确定");
                return;
            }
            var tlist= VideoEditList.SelectedItem as VideoEditList;
            if (tlist.ItemTypeId==3)
            {
                bool tresult = await DisplayAlert("提示信息", "检测到你勾选了M3U文件标头，是否确定要移除？", "是", "否");
                if (!tresult)
                {
                    return;
                }
            }
            videoEditList.Remove(tlist);
        }
        else if (MSelectResult.Contains("删除每一个已勾选"))
        {
            var tlist = videoEditList.Where(p => p.IsSelected==true).ToList();
            if (tlist.Count<1)
            {
                await DisplayAlert("提示信息", "你当前没有勾选任何项，请至少勾选一个复选框！", "确定");
                return;
            }

            for(int i = 0; i < tlist.Count; i++)
            {
                if (tlist[i].ItemTypeId==3)
                {
                    bool tresult = await DisplayAlert("提示信息", "检测到你勾选了M3U文件标头，是否确定要移除？", "是", "否");
                    if (!tresult)
                    {
                        continue;
                    }
                }
                videoEditList.Remove(tlist[i]);
            }
        }

        VideoEditList.ItemsSource=videoEditList;
    }

    private async void VideoEditListSaveBtn_Clicked(object sender, EventArgs e)
    {
        if (VideoEditList.ItemsSource is null)
        {
            return;
        }
        var videoEditList = VideoEditList.ItemsSource.Cast<VideoEditList>().ToList();
        if (videoEditList.Count <1)
        {
            await DisplayAlert("提示信息", "当前列表没有任何项！", "确定");
            return;
        }
        int permResult = await new APPPermissions().CheckAndReqPermissions();
        if (permResult!=0)
        {
            await DisplayAlert("提示信息", "请授权读取和写入权限，程序需要保存文件！", "确定");
            return;
        }


        string[] toptions;
        string tmessage;

        //if(CurrentSaveLocation!=""&&DeviceInfo.Platform!=DevicePlatform.Android)
        if(CurrentSaveLocation!="")
        {
            tmessage="你要如何保存？";
            toptions=new string[] { "保存到源文件","另存为..."};
        }
        else
        {
            tmessage="点击按钮继续";
            toptions=new string[] { "另存为..." };
        }

        string MSelectResult = await DisplayActionSheet(tmessage, "取消", null,toptions);
        if (MSelectResult == "取消"||MSelectResult is null)
        {
            await DisplayAlert("提示信息", "您已取消了操作。", "确定");
            return;
        }
        if(MSelectResult =="保存到源文件")
        {
            if (!File.Exists(CurrentSaveLocation))
            {
                await DisplayAlert("提示信息", "程序找不到该直播源本地文件，可能是该文件已被删除或移动。", "确定");
                CurrentSaveLocation="";
                return;
            }

            File.WriteAllText(CurrentSaveLocation, GetVELSaveStr(videoEditList));
            await DisplayAlert("提示信息", "文件已成功保存至：\n"+CurrentSaveLocation, "确定");
        }
        else
        {
            string tsavename;
            var tlist = LocalM3UList.SelectedItem;
            if (tlist!=null)
            {
                tsavename=(tlist as LocalM3UList).FileName;
            }
            else
            {
                tsavename=".m3u";
            }

            VideoEditSaveRing.IsRunning=true;
            new Thread(async()=>
            {
                using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(GetVELSaveStr(videoEditList))))
                {
                    try
                    {
                        await MainThread.InvokeOnMainThreadAsync(async() =>
                        {
                            var fileSaver = await FileSaver.SaveAsync(FileSystem.AppDataDirectory, tsavename, ms, CancellationToken.None);

                            if (fileSaver.IsSuccessful)
                            {
                                await DisplayAlert("提示信息", "文件已成功保存至：\n"+fileSaver.FilePath, "确定");
                            }
                            else
                            {
                                //暂时判断为用户在选择目录时点击了取消按钮
                                await DisplayAlert("提示信息", "您已取消了操作。", "确定");
                            }

                            VideoEditSaveRing.IsRunning=false;
                        });


                    }
                    catch (Exception)
                    {
                        await MainThread.InvokeOnMainThreadAsync(async () =>
                        {
                            await DisplayAlert("提示信息", "保存文件失败！可能是没有权限。", "确定");
                            VideoEditSaveRing.IsRunning=false;
                        });

                    }
                }
            }).Start();
        }


    }

    public string GetVELSaveStr(List<VideoEditList> videoEditList)
    {
        string saveStr = "";
        for(int i=0;i<videoEditList.Count; i++)
        {
            switch (videoEditList[i].ItemTypeId)
            {
                case 1:
                    saveStr+=(videoEditList[i] as VideoEditListEXT).EXTTag;
                    break;
                case 2:
                    saveStr+=(videoEditList[i] as VideoEditListTVG).AllStr;
                    break;
                case 3:
                    saveStr+=(videoEditList[i] as VideoEditListEXT_Readonly).EXTTag;
                    break;
                case 4:
                    saveStr+=(videoEditList[i] as VideoEditListSourceLink).SourceLink;
                    break;
                case 5:
                    saveStr+=(videoEditList[i] as VideoEditListOtherString).AllStr;
                    break;
            }

            if(i<videoEditList.Count-1)
            {
                saveStr+="\r\n";
            }
        }

        return saveStr;
    }

    private void LocalM3UPanelBtn_Clicked(object sender, EventArgs e)
    {
        LocalM3UPanel.IsVisible=true;
        LocalM3UPanel2.IsVisible=true;
        VideoEditPanel.IsVisible=false;
        VideoEditPanel2.IsVisible=false;
    }

    private void VideoEditPanelBtn_Clicked(object sender, EventArgs e)
    {
        LocalM3UPanel.IsVisible=false;
        LocalM3UPanel2.IsVisible=false;
        VideoEditPanel.IsVisible=true;
        VideoEditPanel2.IsVisible=true;
    }
}