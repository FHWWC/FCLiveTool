using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using System.Xml.Serialization;

namespace FCLiveToolApplication
{
    public class HamListClass
    {
        public ImageSource ItemLogo { get; set; }
        public string ItemTitle { get; set; }
    }
    public class VideoList
    {
        public string SourceName { get; set; }
        public string SourceLink { get; set; }
        public string Description { get; set; }
        public string RecommendReg { get; set; }
        public string LiveType { get; set; }
    }
    public class VideoDetailList
    {
        public int Id { get; set; }
        public string _LogoLink;
        public string LogoLink { get; set; }
        public string SourceName { get; set; }
        public string SourceLink { get; set; }
        //客户端专用
        public string FullM3U8Str { get; set; }
        public bool isHTTPS { get; set; }
        public bool isIPV6 { get; set; }
        public string FileName { get; set; }
        public string HTTPStatusCode { get; set; }
        public Color HTTPStatusTextBKG { get; set; }
        //更多参数
        private string groupTitle;
        public string GroupTitle { get { return "分组标题："+groupTitle; } set { groupTitle=value; } }
        private string tvgID;
        public string TVGID { get { return "频道ID    ："+tvgID; } set { tvgID=value; } }
        private string tvgCountry;
        public string TVGCountry { get { return "频道地区："+tvgCountry; } set { tvgCountry=value; } }
        private string tvgLanguage;
        public string TVGLanguage { get { return "频道语言："+tvgLanguage; } set { tvgLanguage=value; } }
    }
    public class RecentVList
    {
        public int Id { get; set; }

        public string SourceName { get; set; }
        public string LogoLink { get; set; }
        public string SourceLink { get; set; }
        public string Description { get; set; }
        public DateTime AddDT { get; set; }
        //客户端专用
        public string PastTime { get; set; }
    }
    public class LocalM3U8List
    {
        public string ItemId { get; set; }
        public string FilePath { get; set; }
        public string FileName { get; set; }
        public string FullFilePath { get; set; }
    }
    public class LocalM3UList
    {
        public string ItemId { get; set; }
        public string FilePath { get; set; }
        public string FileName { get; set; }
        public string FullFilePath { get; set; }
    }
    public abstract class VideoEditList
    {    /// <summary>
         /// 当前项使用的数据模板ID
         /// </summary>
         /// <value>
         /// ID对应的数据模板：
         /// <para>1：VideoEditListEXT；2：VideoEditListTVG；3：VideoEditListEXT_Readonly；4：VideoEditListSourceLink；5：VideoEditListOtherString。</para>
         /// </value>
        public int ItemTypeId { get; set; }
        /// <summary>
        /// 当前项的名称
        /// </summary>
        /// <value>
        /// 名称在集合里面是唯一的。
        /// </value>
        public string ItemName { get; set; }
        public bool IsSelected { get; set; }
    }
    public class VideoEditListEXT : VideoEditList
    {
        public string EXTTag { get; set; }
    }
    public class VideoEditListEXT_Readonly : VideoEditList
    {
        public string EXTTag { get; set; }
    }
    public class VideoEditListTVG : VideoEditList
    {
        public string AllStr { get; set; }
        public string GroupTitle { get; set; }
        public string TVGGroup { get; set; }
        public string TVGID { get; set; }
        public string TVGLogo { get; set; }
        public string TVGCountry { get; set; }
        public string TVGLanguage { get; set; }
        public string TVGName { get; set; }
        public string TVGURL { get; set; }

    }
    public class VideoEditListSourceLink : VideoEditList
    {
        public string SourceLink { get; set; }
    }
    public class VideoEditListOtherString : VideoEditList
    {
        public string AllStr { get; set; }
    }

    public class VideoAnalysisList
    {
        public string ItemId { get; set; }
        public string AllowCache { get; set; }
        public string TargetDuration { get; set; }
        public string MediaSequence { get; set; }
        public List<M3U8_TS_PARM> TS_PARM { get; set; }
        //非M3U8文件内参数
        public string M3U8URL { get; set; }
        public string FileName { get; set; }
        public string FullURL { get; set; }
        public int FileFromIndex { get; set; }
        public bool IsSelected { get; set; }

    }
    public class M3U8_TS_PARM
    {
        public int ItemId { get; set; }
        public double Time { get; set; }
        public string FullURL { get; set; }
        public string FileName { get; set; }
    }
    public class DownloadTempFileList
    {
        public int ItemId { get; set; }
        public string FileName { get; set; }
        public string FilePath { get; set; }
        public long Filesize { get; set; }
        public string FullLink { get; set; }
    }
    public class DownloadVideoFileList
    {
        public int ItemId { get; set; }
        public string SaveFileName { get { return CurrentVALIfm.FileName; } }
        public string SaveFilePath { get; set; }
        public string M3U8FullLink { get { return CurrentVALIfm.FullURL; } }
        public int TS_FileCount { get { return CurrentVALIfm.TS_PARM.Count; } }
        public VideoManager CurrentActiveObject { get; set; }
        public VideoAnalysisList CurrentVALIfm { get; set; }
        public bool IsSelected { get; set; }
    }

    public class CheckNOKErrorCodeList
    {
        public int Id { get; set; }
        public string HTTPStatusCode { get; set; }
        public Color HTTPStatusTextBKG { get; set; }
        public int ErrorCodeCount { get; set; }
    }
    public class SharedVideo
    {
        public string SourceName { get; set; }
        public string SourceLink { get; set; }
        public DateTime UploadTime { get; set; }
        public DateTime LastCheckTime { get; set; }
        public string URLProtocol { get; set; }
        public string Tag { get; set; }
        //客户端专用
        public bool isHTTPS { get { return SourceLink.ToLower().StartsWith("https://") ? true : false; }}
    }
    public class CheckValidModel
    {
        /// <summary>
        /// 0为正常处理了URL；-1为传来的格式不正确；-2为不支持的URL；-3为无法处理当前请求
        /// </summary>
        public string StatusCode { get; set; }
        /// <summary>
        /// 检测结果，当正常处理了URL后才需要获取此值
        /// </summary>
        public string Result { get; set; }
        /// <summary>
        /// 传送的数据
        /// </summary>
        public List<SharedVideo> Content { get; set; }
    }
    public class VideoEditListDataTemplateSelector : DataTemplateSelector
    {
        protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
        {
            if (item is VideoEditListEXT)
            {
                return (DataTemplate)VideoEditPage.videoEditPage.Resources["VideoEditListEXTStyle"];
            }
            else if (item is VideoEditListTVG)
            {
                return (DataTemplate)VideoEditPage.videoEditPage.Resources["VideoEditListTVGStyle"];
            }
            else if(item is VideoEditListEXT_Readonly)
            {
                return (DataTemplate)VideoEditPage.videoEditPage.Resources["VideoEditListEXT_ReadonlyStyle"];
            }
            else if (item is VideoEditListSourceLink)
            {
                return (DataTemplate)VideoEditPage.videoEditPage.Resources["VideoEditListSourceLinkStyle"];
            }
            else if (item is VideoEditListOtherString)
            {
                return (DataTemplate)VideoEditPage.videoEditPage.Resources["VideoEditListOtherStringStyle"];
            }

            return null;
        }
    }
    public class APPPermissions
    {
        /// <summary>
        /// 检查是否有文件读取的权限
        /// </summary>
        /// <returns>状态码。0：已经有权限或已成功获取到权限；1：读取没有权限；2：写入没有权限；3：读写都没有权限或其他异常情况；</returns>
        public async Task<int> CheckAndReqPermissions()
        {
            //Windows操作系统不需要这种单独获取权限
#if WINDOWS
            return 0;
#else
            var checkRead = await Permissions.CheckStatusAsync<Permissions.StorageRead>();
            var checkWrite = await Permissions.CheckStatusAsync<Permissions.StorageWrite>();

            if (checkRead == PermissionStatus.Granted&&checkWrite == PermissionStatus.Granted)
            {
                return 0;
            }
            else
            {
                var reqRead = await Permissions.RequestAsync<Permissions.StorageRead>();
                var reqWrite = await Permissions.RequestAsync<Permissions.StorageWrite>();

                if (reqRead == PermissionStatus.Granted&&reqWrite == PermissionStatus.Granted)
                {
                    return 0;
                }
                else if (reqRead != PermissionStatus.Granted &&reqWrite == PermissionStatus.Granted)
                {
                    return 1;
                }
                else if (reqRead == PermissionStatus.Granted &&reqWrite != PermissionStatus.Granted)
                {
                    return 2;
                }
                else
                {
                    return 3;
                }

            }
#endif
        }

    }
    public class VideoManager
    {
        public List<DownloadTempFileList> TempFileList;
        public bool isContinueDownloadStream = true;
        public bool isEndList = false;
        public const string DEFAULT_USER_AGENT = @"Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/115.0.0.0 Safari/537.36";

        /// <summary>
        /// 
        /// </summary>
        /// <param name="M3U8PlayList">M3U8文件内的所有直播信号</param>
        /// <param name="VideoIfm">1：名称；2：URL；（计划改成引用类型，调用后可再次获取）</param>
        /// <returns></returns>
        public async Task<string> DownloadAndReadM3U8File(List<string[]> M3U8PlayList, string[] VideoIfm)
        {
            M3U8PlayList.Clear();

            string[] treturn = new string[2];
            using (Stream stream = await DownloadM3U8FileToStream(VideoIfm[1], treturn))
            {
                if(stream is null)
                {
                    return treturn[0];
                }

                VideoIfm[0]=VideoIfm[0].Replace("\r", "").Replace("\n", "").Replace("\r\n", "");
                try
                {
                    using (StreamReader sr = new StreamReader(stream))
                    {
                        string r = "";
                        string tProperties = "";
                        while ((r=await sr.ReadLineAsync())!=null)
                        {
                            if (r.StartsWith("#"))
                            {
                                tProperties=r;
                                continue;
                            }
                            else if (String.IsNullOrWhiteSpace(r))
                                continue;
                            else if (r.Contains(".m3u8"))
                            {
                                if (!r.Contains("://"))
                                {
                                    r=VideoIfm[1].Substring(0, VideoIfm[1].LastIndexOf("/")+1)+r;
                                }


                                //string m3u8name = r[new Range(r.LastIndexOf("/")+1, r.LastIndexOf(".m3u8")+5)];                                
                                Match m3u8nameResult = Regex.Match(r, @"\/([^\/]+\.m3u8)");
                                string m3u8name = m3u8nameResult.Groups[1].Value=="" ? "未解析到文件名" : m3u8nameResult.Groups[1].Value;

                                Match tPResult = Regex.Match(tProperties, @"(?:,|:)BANDWIDTH=(.*?)(,|\n|$)");
                                string tBandwidth = tPResult.Groups[1].Value.Replace("\"", "");
                                tBandwidth=tBandwidth=="" ? "---" : tBandwidth;
                                Match tPResult2 = Regex.Match(tProperties, @"RESOLUTION=(.*?)(,|\n|$)");
                                string tResolution = tPResult2.Groups[1].Value.Replace("\"", "");
                                tResolution=tResolution=="" ? "---" : tResolution;
                                Match tPResult3 = Regex.Match(tProperties, @"FRAME-RATE=(.*?)(,|\n|$)");
                                string tFrameRate = tPResult3.Groups[1].Value.Replace("\"", "");
                                tFrameRate=tFrameRate=="" ? "---" : tFrameRate;
                                Match tPResult4 = Regex.Match(tProperties, @"CODECS=""(.*?)""(,|\n|$)");
                                string tCodecs = tPResult4.Groups[1].Value.Replace("\"", "");
                                tCodecs=tCodecs=="" ? "---" : tCodecs;
                                Match tPResult5 = Regex.Match(tProperties, @"NAME=(.*?)(,|\n|$)");
                                string tName = tPResult5.Groups[1].Value.Replace("\"", "");
                                tName=tName=="" ? "---" : tName;


                                M3U8PlayList.Add(new string[] { m3u8name, r, tBandwidth, tResolution, tFrameRate, tCodecs, tName });

                            }
                            else if (r.Contains(".ts"))
                            {
                                //备用
                            }
                        }
                    }

                }
                catch (Exception)
                {
                    return "解析出现问题，可能是M3U8文件数据格式有问题。";
                }
            }
               
            return "";
        }
        /// <summary>
        ///  下载M3U8文件并解析所有TS分片文件，该方法会返回一个TS列表，用于下载分片文件
        /// </summary>
        /// <param name="videoAnalysisList">从M3U8文件里解析到的所有信息</param>
        /// <param name="VideoIfm">1：URL；</param>
        /// <param name="FileFrom">文件来源，0表示URL地址；1表示本地文件路径；</param>
        /// <returns></returns>
        public async Task<string> DownloadAndReadM3U8FileForDownloadTS(VideoAnalysisList videoAnalysisList, string[] VideoIfm,int FileFrom)
        {
            bool isNeedAddServer = false;
            videoAnalysisList.FileFromIndex=FileFrom;

            if (FileFrom==0)
            {
                string[] treturn = new string[2];
                using (Stream stream = await DownloadM3U8FileToStream(VideoIfm[0], treturn))
                {
                    if (stream is null)
                    {
                        return treturn[0];
                    }

                    try
                    {
                        using (StreamReader sr = new StreamReader(stream))
                        {
                            string r = "";
                            string tAllowCache = "---";
                            string tTargetDuration = "---";
                            string tMediaSequence = "---";
                            List<M3U8_TS_PARM> tMTP = new List<M3U8_TS_PARM>();
                            double tTime = 0;
                            string tsurl = VideoIfm[0].Substring(0, VideoIfm[0].LastIndexOf("/") + 1);
                            int ts_index = 0;

                            while ((r = await sr.ReadLineAsync()) != null)
                            {
                                //如果下载M3U8文件后里面还是个M3U8文件，则继续下载并解析
                                if (r.Contains(".m3u8"))
                                {
                                    if (!r.Contains("://"))
                                    {
                                        r =tsurl + r;
                                    }

                                    return await DownloadAndReadM3U8FileForDownloadTS(videoAnalysisList, new string[] { r }, FileFrom);
                                }
                                //开始读取带TS分片信息的M3U8
                                else if (r.StartsWith("#EXT-X-ALLOW-CACHE"))
                                {
                                    tAllowCache=r.Replace("#EXT-X-ALLOW-CACHE:", "");
                                }
                                else if (r.StartsWith("#EXT-X-TARGETDURATION"))
                                {
                                    tTargetDuration=r.Replace("#EXT-X-TARGETDURATION:", "");
                                }
                                else if (r.StartsWith("#EXT-X-MEDIA-SEQUENCE"))
                                {
                                    tMediaSequence=r.Replace("#EXT-X-MEDIA-SEQUENCE:", "");
                                }
                                else if (r.Contains("#EXTINF:"))
                                {
                                    var tvalue = r.Replace("#EXTINF:", "");
                                    tvalue = tvalue.Substring(0, tvalue.IndexOf(","));

                                    double.TryParse(tvalue, out tTime);
                                }
                                else if (r.Contains(".ts"))
                                {
                                    if (!r.Contains("://"))
                                    {
                                        r = tsurl+ r;
                                    }

                                    tMTP.Add(new M3U8_TS_PARM() { ItemId=ts_index, FullURL=r, Time=tTime });
                                    ts_index++;
                                }
                                else if (r.Contains("#EXT-X-ENDLIST"))
                                {
                                    //添加一个标识，因为直播已结束，服务器不再提供新的数据流，所以程序将不再向服务器发请求
                                    //tMTP.Add(new M3U8_TS_PARM() { FullURL = "", Time = 0 });
                                    isEndList = true;
                                    //return "直播源已播放完毕";
                                }

                            }


                            videoAnalysisList.AllowCache = tAllowCache;
                            videoAnalysisList.MediaSequence = tMediaSequence;
                            videoAnalysisList.TargetDuration = tTargetDuration;
                            videoAnalysisList.FullURL = VideoIfm[0];
                            videoAnalysisList.M3U8URL = tsurl;
                            videoAnalysisList.FileName = VideoIfm[0].Replace(tsurl, "");
                            videoAnalysisList.TS_PARM = tMTP;
                        }

                    }
                    catch (Exception)
                    {
                        return "解析出现问题，可能是M3U8文件数据格式有问题。";
                    }
                }

            }
            else if(FileFrom==1)
            {
                if (!File.Exists(VideoIfm[0]))
                {
                    return "程序找不到该直播源本地文件，可能是该文件已被删除或移动。";
                }

                try
                {
                    using (StringReader sr = new StringReader(File.ReadAllText(VideoIfm[0])))
                    {
                        string r = "";
                        string tAllowCache = "---";
                        string tTargetDuration = "---";
                        string tMediaSequence = "---";
                        List<M3U8_TS_PARM> tMTP = new List<M3U8_TS_PARM>();
                        double tTime = 0;
                        string tsurl = "";
#if ANDROID
                        tsurl = VideoIfm[0].Substring(0, VideoIfm[0].LastIndexOf("/") + 1);
#else
tsurl = VideoIfm[0].Substring(0, VideoIfm[0].LastIndexOf("\\") + 1);
#endif
                        int ts_index = 0;

                        while ((r = await sr.ReadLineAsync()) != null)
                        {
                            //如果下载M3U8文件后里面还是个M3U8文件，则继续下载并解析
                            if (r.Contains(".m3u8"))
                            {
                                //如果仍然获取不到服务器
                                if (!r.Contains("://"))
                                {
                                    return "当前文件内提供的M3U8的URL是相对地址，程序无法知晓直播源的服务器，所以没有可用的数据源！";
                                }

                                //解析M3U8文件后如果内部不是TS分片文件还是M3U8文件，那么它一定是一个URL，所以FileFrom传0
                                return await DownloadAndReadM3U8FileForDownloadTS(videoAnalysisList, new string[] { r },0);
                            }
                            //开始读取带TS分片信息的M3U8
                            else if (r.StartsWith("#EXT-X-ALLOW-CACHE"))
                            {
                                tAllowCache=r.Replace("#EXT-X-ALLOW-CACHE:", "");
                            }
                            else if (r.StartsWith("#EXT-X-TARGETDURATION"))
                            {
                                tTargetDuration=r.Replace("#EXT-X-TARGETDURATION:", "");
                            }
                            else if (r.StartsWith("#EXT-X-MEDIA-SEQUENCE"))
                            {
                                tMediaSequence=r.Replace("#EXT-X-MEDIA-SEQUENCE:", "");
                            }
                            else if (r.Contains("#EXTINF:"))
                            {
                                var tvalue = r.Replace("#EXTINF:", "");
                                tvalue = tvalue.Substring(0, tvalue.IndexOf(","));

                                double.TryParse(tvalue, out tTime);
                            }
                            else if (r.Contains(".ts"))
                            {
                                if (!r.Contains("://"))
                                {
                                    isNeedAddServer = true;
                                }

                                tMTP.Add(new M3U8_TS_PARM() { ItemId=ts_index, FullURL=r, Time=tTime });
                                ts_index++;
                            }
                            else if (r.Contains("#EXT-X-ENDLIST"))
                            {
                                //添加一个标识，因为直播已结束，服务器不再提供新的数据流，所以程序将不再向服务器发请求
                                //tMTP.Add(new M3U8_TS_PARM() { FullURL = "", Time = 0 });
                                isEndList = true;
                                //return "直播源已播放完毕";
                            }

                        }


                        videoAnalysisList.AllowCache = tAllowCache;
                        videoAnalysisList.MediaSequence = tMediaSequence;
                        videoAnalysisList.TargetDuration = tTargetDuration;
                        videoAnalysisList.FullURL = VideoIfm[0];
                        videoAnalysisList.M3U8URL = tsurl;
                        videoAnalysisList.FileName = VideoIfm[0].Replace(tsurl, "");
                        videoAnalysisList.TS_PARM = tMTP;

                    }
                    
                }
                catch (Exception)
                {
                    return "解析出现问题，可能是M3U8文件数据格式有问题。";
                }

                if (isNeedAddServer)
                {
                    return "CODE_00";
                }

            }
            else
            {
                return "未指定文件来源类型";
            }


            return "";
        }

        /// <summary>
        ///  下载M3U8文件并解析所有TS分片文件，该方法会返回一个TS列表，用于下载分片文件
        /// </summary>
        /// <param name="videoAnalysisList">从M3U8文件里解析到的所有信息</param>
        /// <param name="VideoIfm">1：URL；</param>
        /// <param name="FileFrom">文件来源，0表示URL地址；1表示本地文件路径；</param>
        /// <returns></returns>
        public async Task<List<string>> DownloadAndReadM3U8FileForDownloadTS(List<VideoAnalysisList> videoAnalysisList, List<string> VideoIfm, int FileFrom)
        {
            List<string> resultList = new List<string>(); 

            for (int i = 0; i<VideoIfm.Count; i++)
            {
                bool skipAddResult = false;
                bool isNeedAddServer = false;
                videoAnalysisList.Add(new VideoAnalysisList() { FileFromIndex=FileFrom});

                if (FileFrom==0)
                {
                    string[] treturn = new string[2];
                    using (Stream stream = await DownloadM3U8FileToStream(VideoIfm[i], treturn))
                    {
                        if (stream is null)
                        {
                            resultList.Add(treturn[0]);
                            continue;
                        }

                        try
                        {
                            using (StreamReader sr = new StreamReader(stream))
                            {
                                string r = "";
                                string tAllowCache = "---";
                                string tTargetDuration = "---";
                                string tMediaSequence = "---";
                                List<M3U8_TS_PARM> tMTP = new List<M3U8_TS_PARM>();
                                double tTime = 0;
                                string tsurl = VideoIfm[i].Substring(0, VideoIfm[i].LastIndexOf("/") + 1);
                                int ts_index = 0;

                                while ((r = await sr.ReadLineAsync()) != null)
                                {
                                    //如果下载M3U8文件后里面还是个M3U8文件，则继续下载并解析
                                    if (r.Contains(".m3u8"))
                                    {
                                        if (!r.Contains("://"))
                                        {
                                            r =tsurl + r;
                                        }

                                        resultList.Add(await DownloadAndReadM3U8FileForDownloadTS(videoAnalysisList[i], new string[] { r }, FileFrom));
                                        skipAddResult = true;
                                        break;
                                    }
                                    //开始读取带TS分片信息的M3U8
                                    else if (r.StartsWith("#EXT-X-ALLOW-CACHE"))
                                    {
                                        tAllowCache=r.Replace("#EXT-X-ALLOW-CACHE:", "");
                                    }
                                    else if (r.StartsWith("#EXT-X-TARGETDURATION"))
                                    {
                                        tTargetDuration=r.Replace("#EXT-X-TARGETDURATION:", "");
                                    }
                                    else if (r.StartsWith("#EXT-X-MEDIA-SEQUENCE"))
                                    {
                                        tMediaSequence=r.Replace("#EXT-X-MEDIA-SEQUENCE:", "");
                                    }
                                    else if (r.Contains("#EXTINF:"))
                                    {
                                        var tvalue = r.Replace("#EXTINF:", "");
                                        tvalue = tvalue.Substring(0, tvalue.IndexOf(","));

                                        double.TryParse(tvalue, out tTime);
                                    }
                                    else if (r.Contains(".ts"))
                                    {
                                        if (!r.Contains("://"))
                                        {
                                            r = tsurl+ r;
                                        }

                                        tMTP.Add(new M3U8_TS_PARM() { ItemId=ts_index, FullURL=r, Time=tTime });
                                        ts_index++;
                                    }
                                    else if (r.Contains("#EXT-X-ENDLIST"))
                                    {
                                        //添加一个标识，因为直播已结束，服务器不再提供新的数据流，所以程序将不再向服务器发请求
                                        //tMTP.Add(new M3U8_TS_PARM() { FullURL = "", Time = 0 });
                                        isEndList = true;
                                        //return "直播源已播放完毕";
                                    }

                                }


                                if (!skipAddResult)
                                {
                                    videoAnalysisList[i].AllowCache = tAllowCache;
                                    videoAnalysisList[i].MediaSequence = tMediaSequence;
                                    videoAnalysisList[i].TargetDuration = tTargetDuration;
                                    videoAnalysisList[i].FullURL = VideoIfm[i];
                                    videoAnalysisList[i].M3U8URL = tsurl;
                                    videoAnalysisList[i].FileName = VideoIfm[i].Replace(tsurl, "");
                                    videoAnalysisList[i].TS_PARM = tMTP;
                                }

                            }

                        }
                        catch (Exception)
                        {
                            resultList.Add("解析出现问题，可能是M3U8文件数据格式有问题。");
                            continue;
                        }
                    }

                }
                else if (FileFrom==1)
                {
                    if (!File.Exists(VideoIfm[i]))
                    {
                        resultList.Add("程序找不到该直播源本地文件，可能是该文件已被删除或移动。");
                        continue;
                    }

                    try
                    {
                        using (StringReader sr = new StringReader(File.ReadAllText(VideoIfm[i])))
                        {
                            string r = "";
                            string tAllowCache = "---";
                            string tTargetDuration = "---";
                            string tMediaSequence = "---";
                            List<M3U8_TS_PARM> tMTP = new List<M3U8_TS_PARM>();
                            double tTime = 0;
                            //截取本地文件的路径时，需要注意不同操作系统的斜杠会有不同
                            string tsurl = "";
#if ANDROID
                            tsurl = VideoIfm[i].Substring(0, VideoIfm[i].LastIndexOf("/") + 1);

#else
tsurl = VideoIfm[i].Substring(0, VideoIfm[i].LastIndexOf("\\") + 1);
#endif
                            int ts_index = 0;

                            while ((r = await sr.ReadLineAsync()) != null)
                            {
                                //如果下载M3U8文件后里面还是个M3U8文件，则继续下载并解析
                                if (r.Contains(".m3u8"))
                                {
                                    //如果仍然获取不到服务器
                                    if (!r.Contains("://"))
                                    {
                                        resultList.Add("当前文件内提供的M3U8的URL是相对地址，程序无法知晓直播源的服务器，所以没有可用的数据源！");
                                        skipAddResult = true;
                                        
                                        break;
                                    }

                                    //解析M3U8文件后如果内部不是TS分片文件还是M3U8文件，那么它一定是一个URL，所以FileFrom传0
                                    resultList.Add(await DownloadAndReadM3U8FileForDownloadTS(videoAnalysisList[i], new string[] { r }, 0));
                                    skipAddResult = true;

                                    break;
                                }
                                //开始读取带TS分片信息的M3U8
                                else if (r.StartsWith("#EXT-X-ALLOW-CACHE"))
                                {
                                    tAllowCache=r.Replace("#EXT-X-ALLOW-CACHE:", "");
                                }
                                else if (r.StartsWith("#EXT-X-TARGETDURATION"))
                                {
                                    tTargetDuration=r.Replace("#EXT-X-TARGETDURATION:", "");
                                }
                                else if (r.StartsWith("#EXT-X-MEDIA-SEQUENCE"))
                                {
                                    tMediaSequence=r.Replace("#EXT-X-MEDIA-SEQUENCE:", "");
                                }
                                else if (r.Contains("#EXTINF:"))
                                {
                                    var tvalue = r.Replace("#EXTINF:", "");
                                    tvalue = tvalue.Substring(0, tvalue.IndexOf(","));

                                    double.TryParse(tvalue, out tTime);
                                }
                                else if (r.Contains(".ts"))
                                {
                                    if (!r.Contains("://"))
                                    {
                                        isNeedAddServer = true;
                                    }

                                    tMTP.Add(new M3U8_TS_PARM() { ItemId=ts_index, FullURL=r, Time=tTime });
                                    ts_index++;
                                }
                                else if (r.Contains("#EXT-X-ENDLIST"))
                                {
                                    //添加一个标识，因为直播已结束，服务器不再提供新的数据流，所以程序将不再向服务器发请求
                                    //tMTP.Add(new M3U8_TS_PARM() { FullURL = "", Time = 0 });
                                    isEndList = true;
                                    //return "直播源已播放完毕";
                                }

                            }


                            if(!skipAddResult)
                            {
                                videoAnalysisList[i].AllowCache = tAllowCache;
                                videoAnalysisList[i].MediaSequence = tMediaSequence;
                                videoAnalysisList[i].TargetDuration = tTargetDuration;
                                videoAnalysisList[i].FullURL = VideoIfm[i];
                                videoAnalysisList[i].M3U8URL = tsurl;
                                videoAnalysisList[i].FileName = VideoIfm[i].Replace(tsurl, "");
                                videoAnalysisList[i].TS_PARM = tMTP;
                            }

                        }

                    }
                    catch (Exception)
                    {
                        resultList.Add("解析出现问题，可能是M3U8文件数据格式有问题。");
                        continue;
                    }


                    if (isNeedAddServer)
                    {
                        resultList.Add("CODE_00");
                        continue;
                    }

                }
                else
                {
                    //如果传参错误，则没有继续的意义。
                    return resultList;
                }


                if(!skipAddResult)
                {
                    resultList.Add("");
                }

            }

            return resultList;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="M3U8PlayList">M3U8文件内的所有直播信号</param>
        /// <param name="VideoIfm">1：文件名；2：文件的路径；</param>
        /// <returns></returns>
        public async Task<string> ReadLocalM3U8File(List<string[]> M3U8PlayList, string[] VideoIfm)
        {
            M3U8PlayList.Clear();

            if (!File.Exists(VideoIfm[1]))
            {
                return "程序找不到该直播源本地文件，可能是该文件已被删除或移动。";
            }

            try
            {
                using (StringReader sr = new StringReader(File.ReadAllText(VideoIfm[1])))
                {
                    string r = "";
                    string tProperties = "";
                    while ((r=await sr.ReadLineAsync())!=null)
                    {
                        if (r.StartsWith("#"))
                        {
                            tProperties=r;
                            continue;
                        }
                        else if (String.IsNullOrWhiteSpace(r))
                            continue;
                        else if (r.Contains(".m3u8"))
                        {
                            //string m3u8name = r[new Range(r.LastIndexOf("/")+1, r.LastIndexOf(".m3u8")+5)];
                            //暂时给表达式加?
                            Match m3u8nameResult = Regex.Match(r, @"\/?([^\/]+\.m3u8)");
                            string m3u8name = m3u8nameResult.Groups[1].Value=="" ? "未解析到文件名" : m3u8nameResult.Groups[1].Value;

                            Match tPResult = Regex.Match(tProperties, @"(?:,|:)BANDWIDTH=(.*?)(,|\n|$)");
                            string tBandwidth = tPResult.Groups[1].Value.Replace("\"", "");
                            tBandwidth=tBandwidth=="" ? "---" : tBandwidth;
                            Match tPResult2 = Regex.Match(tProperties, @"RESOLUTION=(.*?)(,|\n|$)");
                            string tResolution = tPResult2.Groups[1].Value.Replace("\"", "");
                            tResolution=tResolution=="" ? "---" : tResolution;
                            Match tPResult3 = Regex.Match(tProperties, @"FRAME-RATE=(.*?)(,|\n|$)");
                            string tFrameRate = tPResult3.Groups[1].Value.Replace("\"", "");
                            tFrameRate=tFrameRate=="" ? "---" : tFrameRate;
                            Match tPResult4 = Regex.Match(tProperties, @"CODECS=""(.*?)""(,|\n|$)");
                            string tCodecs = tPResult4.Groups[1].Value.Replace("\"", "");
                            tCodecs=tCodecs=="" ? "---" : tCodecs;
                            Match tPResult5 = Regex.Match(tProperties, @"NAME=(.*?)(,|\n|$)");
                            string tName = tPResult5.Groups[1].Value.Replace("\"", "");
                            tName=tName=="" ? "---" : tName;

                            if (!r.Contains("://"))
                            {
                                r="";
                            }

                            M3U8PlayList.Add(new string[] { m3u8name, r, tBandwidth, tResolution, tFrameRate, tCodecs, tName });

                        }
                        else if (r.Contains(".ts"))
                        {
                            //备用
                        }
                    }
                }

            }
            catch (Exception)
            {
                return "解析出现问题，可能是M3U8文件数据格式有问题。";
            }

            return "";
        }
        public async Task<string> DownloadM3U8Stream(VideoAnalysisList valist, string savepath,bool isMergeBeforeFile)
        {
            TempFileList=new List<DownloadTempFileList>();
            //int FileIndex = 0;
            string filename = valist.FileName.Substring(0, valist.FileName.LastIndexOf("."));
            int FinishCount = 0;
            string dresult = "";
            string dataPath = new APPFileManager().GetOrCreateAPPDirectory("DownloadStreamTemp");

            if(filename.Length>50)
            {
                filename=filename.Substring(0, 50);
            }
            foreach (var m in valist.TS_PARM)
            {
                new Thread(async () =>
                {
                    string url = m.FullURL;
                    long Filesize = 0;
                    string FileID = Guid.NewGuid().ToString();
                    string TempFilepath = "";
#if ANDROID            
                    if (string.IsNullOrWhiteSpace(dataPath))
                    {              
                        dresult= "保存文件失败！可能是没有权限或者当前平台暂不支持保存操作！";
                        isContinueDownloadStream = false;
                        FinishCount++;
                        return;
                    }
                    TempFilepath =dataPath+ string.Format($"{FileID}_{filename}.tmp");

#else
TempFilepath = string.Format($"{savepath}{FileID}_{filename}.tmp");
#endif


                    using (HttpClient httpClient = new HttpClient())
                    {
                        int statusCode;
                        httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(DEFAULT_USER_AGENT);
                        HttpResponseMessage response = null;

                        try
                        {
                            response = await httpClient.GetAsync(url);

                            statusCode = (int)response.StatusCode;
                            if (!response.IsSuccessStatusCode)
                            {
                                dresult= "请求失败！" + "HTTP错误代码：" + statusCode;
                                isContinueDownloadStream = false;
                                FinishCount++;
                                return;
                            }

                            Filesize = response.Content.Headers.ContentLength ?? -1;
                            if (Filesize <= 0)
                            {
                                dresult= "无法从 ContentLength 中获取有效的文件大小！";
                                isContinueDownloadStream = false;
                                FinishCount++;
                                return;
                            }

                        }
                        catch (Exception)
                        {
                            dresult = "请求发生异常！";
                            isContinueDownloadStream = false;
                            FinishCount++;
                            return;
                        }

                        FileStream fs = null;
                        Stream ns = null;
                        try
                        {
                            fs = new FileStream(TempFilepath, FileMode.Create);

                            ns = await response.Content.ReadAsStreamAsync();
                            ns.CopyTo(fs);
                        }
                        catch(Exception ex)
                        {
                            dresult = "向临时文件写入发生异常！";
                            isContinueDownloadStream = false;
                            FinishCount++;
                            return;
                        }
                        finally
                        {
                            fs?.Close();
                            ns?.Close();
                        }


                    }

                    TempFileList.Add(new DownloadTempFileList() { ItemId = m.ItemId, FileName = TempFilepath.Replace(savepath, ""), FilePath = TempFilepath, Filesize = Filesize, FullLink = url });
                
                    FinishCount++;
                }).Start();


            }

            while(true)
            {
                if(FinishCount == valist.TS_PARM.Count)
                {
                    break;
                }
            }

#if ANDROID
            MergeTempFile(dataPath + filename + ".mp4", isMergeBeforeFile);
#else
            MergeTempFile(savepath + filename + ".mp4", isMergeBeforeFile);
#endif

            if (!isEndList&&isContinueDownloadStream)
            {
                double TS_AllTime = valist.TS_PARM.Sum(p=>p.Time)*1000;
                await Task.Delay((int)TS_AllTime-500);

                //本地文件暂时只循环一次
                if(valist.FileFromIndex==0)
                {
                    VideoAnalysisList videoAnalysisList = new VideoAnalysisList();
                    dresult = await DownloadAndReadM3U8FileForDownloadTS(videoAnalysisList, new string[] { valist.FullURL }, valist.FileFromIndex);
                    if (string.IsNullOrEmpty(dresult))
                    {
                        dresult = await DownloadM3U8Stream(videoAnalysisList, savepath, isMergeBeforeFile);
                    }
                }


            }


            return dresult;
        }
        /// <summary>
        /// 将当前播放列表更新到直播源预览页面
        /// </summary>
        /// <param name="playlist">播放列表</param>
        public void UpdatePrevPagePlaylist(List<string[]> playlist)
        {
            VideoPrevPage.videoPrevPage.M3U8PlayList.Clear();
            playlist.ForEach(p =>
            {
                VideoPrevPage.videoPrevPage.M3U8PlayList.Add(p);
            });
        }

        /*
               public string GetReadM3U8FileErrorMsg(int index)
              {
                  switch (index)
                  {
                      case 1:

                          break;
                      case 2:
                          break;
                      case 3:
                          break;
                  }
              }
         */
        /// <summary>
        /// 
        /// </summary>
        /// <param name="m3uStr"></param>
        /// <returns></returns>
        public async Task<List<VideoEditList>> ReadM3UString(string m3uStr)
        {
            List<VideoEditList> videoStrList = new List<VideoEditList>();

            try
            {
                using (StringReader sr = new StringReader(m3uStr))
                {
                    string r = "";
                    while ((r=await sr.ReadLineAsync())!=null)
                    {
                        if (r.StartsWith("#"))
                        {
                            if(r.StartsWith("#EXTM3U"))
                            {
                                videoStrList.Add(new VideoEditListEXT_Readonly() { ItemTypeId=3, EXTTag=r });
                            }
                            else if (r.StartsWith("#EXTINF"))
                            {
                                string groupTitle="";
                                string tvgGroup="";
                                string tvgID="";
                                string tvgLogo = "";
                                string tvgCountry = "";
                                string tvgLanguage = "";
                                string tvgName = "";
                                string tvgURL = "";

                                if(r.Contains("group-title="))
                                {
                                    Match tResult = Regex.Match(r, @"group-title=""(.*?)""");
                                    groupTitle=tResult.Groups[1].Value;
                                }
                                if (r.Contains("tvg-group="))
                                {
                                    Match tResult = Regex.Match(r, @"tvg-group=""(.*?)""");
                                    tvgGroup=tResult.Groups[1].Value;
                                }
                                if (r.Contains("tvg-logo="))
                                {
                                    Match tResult = Regex.Match(r, @"tvg-logo=""(.*?)""");
                                    tvgLogo=tResult.Groups[1].Value;
                                }
                                if (r.Contains("tvg-id="))
                                {
                                    Match tResult = Regex.Match(r, @"tvg-id=""(.*?)""");
                                    tvgID=tResult.Groups[1].Value;
                                }
                                if (r.Contains("tvg-country="))
                                {
                                    Match tResult = Regex.Match(r, @"tvg-country=""(.*?)""");
                                    tvgCountry=tResult.Groups[1].Value;
                                }
                                if (r.Contains("tvg-language="))
                                {
                                    Match tResult = Regex.Match(r, @"tvg-language=""(.*?)""");
                                    tvgLanguage=tResult.Groups[1].Value;
                                }
                                if (r.Contains("tvg-name="))
                                {
                                    Match tResult = Regex.Match(r, @"tvg-name=""(.*?)""");
                                    tvgName=tResult.Groups[1].Value;
                                }
                                if (r.Contains("tvg-url="))
                                {
                                    Match tResult = Regex.Match(r, @"tvg-url=""(.*?)""");
                                    tvgURL=tResult.Groups[1].Value;
                                }

                                videoStrList.Add(new VideoEditListTVG() {ItemTypeId=2, AllStr=r ,GroupTitle=groupTitle, TVGGroup=tvgGroup,TVGID=tvgID,TVGLogo=tvgLogo,TVGCountry=tvgCountry,TVGLanguage=tvgLanguage,TVGName=tvgName,TVGURL=tvgURL});

                            }
                            else
                            {
                                videoStrList.Add(new VideoEditListEXT() {ItemTypeId=1, EXTTag=r });
                            }

                        }
                        //暂时不对空格作出处理
                        else if (String.IsNullOrWhiteSpace(r))
                            continue;
                        else if (r.Contains(".m3u8"))
                        {
                            videoStrList.Add(new VideoEditListSourceLink() {ItemTypeId=4, SourceLink=r });
                        }
                        else
                        {
                            videoStrList.Add(new VideoEditListOtherString() {ItemTypeId=5, AllStr=r });
                        }
                    }
                }

            }
            catch (Exception)
            {

            }

            return videoStrList;
        }

        /*
                 public string CheckAndRandomGUID(List<VideoEditList> videolist)
                {
                    string guid = Guid.NewGuid().ToString();
                    if(videolist.Where(p=>p.ItemName==guid).Count() > 0)
                    {
                       return CheckAndRandomGUID(videolist);
                    }

                    return guid;
                }
         */
        private void MergeTempFile(string finalFilepath,bool isMergeBeforeFile)
        {
            int length = 0;

            /*
                         FileMode fileMode;
                        if (isNeedJoinFile)
                        {
                            fileMode = FileMode.Append;
                        }
                        else
                        {
                            fileMode = FileMode.Create;
                        }
             */

            try
            {
                long fsLength = 0;
                using (FileStream fs = new FileStream(finalFilepath, FileMode.Append))
                {
                    foreach (var item in TempFileList.OrderBy(p => p.ItemId))
                    {
                        string tempFilePath = item.FilePath;
                        if (!File.Exists(tempFilePath)) continue;

                        try
                        {
                            using (FileStream tempStream = new FileStream(tempFilePath, FileMode.Open))
                            {
                                byte[] buffer = new byte[tempStream.Length];

                                while ((length = tempStream.Read(buffer, 0, buffer.Length)) > 0)
                                {
                                    fs.Write(buffer, 0, length);
                                }
                                tempStream.Flush();
                            }

                        }
                        catch
                        {

                        }

                        File.Delete(tempFilePath);
                    }

                    fsLength=fs.Length;
                }

                if (fsLength<=0)
                {
                    File.Delete(finalFilepath);
                }
            }
            catch (Exception)
            {

            }

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="url"></param>
        /// <param name="returns">返回的信息；服务器返回的文件名；</param>
        /// <returns></returns>
        public async Task<Stream> DownloadM3U8FileToStream(string url,string[] returns)
        {
            Stream returnStream = null;
            using (HttpClient httpClient = new HttpClient())
            {
                int statusCode;
                httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(DEFAULT_USER_AGENT);
                HttpResponseMessage response = null;

                try
                {
                    response = await httpClient.GetAsync(url);
                    returns[1]= response.RequestMessage.RequestUri.Segments.LastOrDefault();

                    statusCode = (int)response.StatusCode;
                    if (!response.IsSuccessStatusCode)
                    {
                        returns[0]= "获取文件失败，请稍后重试！\n" + "HTTP错误代码：" + statusCode;
                        return null;
                    }

                    returnStream=await response.Content.ReadAsStreamAsync();
                }
                catch (Exception)
                {
                    returns[0]= "无法连接到对方服务器，请检查您的网络或者更换一个直播源！";
                    return null;
                }

    
            }

            return returnStream;
        }
    }
    public class APPFileManager
    {
        public string GetOrCreateAPPDirectory(string foldername)
        {
            //根据不同平台选择不同的缓存方式
            string dataPath;
#if WINDOWS
            dataPath = Path.Combine(FileSystem.AppDataDirectory+"/"+foldername)+"\\";
#elif ANDROID
            //var test= Directory.CreateDirectory(Android.OS.Environment.GetExternalStoragePublicDirectory(Android.OS.Environment.DataDirectory.AbsolutePath)+"/LiveStreamCache");
            dataPath = Path.Combine(Android.OS.Environment.ExternalStorageDirectory.AbsolutePath, "Android", "data", Android.App.Application.Context.PackageName+"/"+foldername)+"/";
#else
            //暂时不对苹果设备以及其他平台进行直播源缓存

            //await DisplayAlert("提示信息", "当前平台暂不支持保存操作！", "确定");
            return null;
#endif

            try
            {
                Directory.CreateDirectory(dataPath);
            }
            catch (Exception)
            {
                //await DisplayAlert("提示信息", "保存文件失败！可能是没有权限。", "确定");
                return null;
            }

            return dataPath;
        }
    }

    public class MsgManager()
    {
        public string GetRegexOptionTip()
        {
            string showmsg = "由于不同的M3U文件里的数据格式各有不同，甚至一个文件内有多种不同的格式，因此本程序提供了多种解析方案，来尽可能的完整解析M3U文件里的数据。"+"\n"
        +"如果您发现许多直播源没有名称，无法播放以及URL有错误等情况，您可以尝试更换解析方案。"+"\n\n\n"
        +"以下是所有复选框的解释："+"\n\n"
        +"“仅匹配M3U8文件名”：勾选则表示，仅匹配直播源URL的文件名为“M3U8”后缀的文件，其他文件则不获取。"+"\n\n\n"
        +"以下是所有规则的解释："+"\n\n"
        +"规则1"+"\n"
        +"匹配：台标(tvg-logo)，台名(tvg-name)，URL"+"\n\n"
        +"tvg-logo=\"台标\"(可选)"+"\n"
        +"tvg-name=\"台名\"(可选，不建议为空)"+"\n"
        +"\\r或\\n(二选一)"+"\n"
        +"http://或https://(+任意字符).m3u8?参数名=值&参数名=值...(参数全部可选)"+"\n\n"
        +"****************************************"+"\n\n"
        +"规则2"+"\n"
        +"与第一条相反，匹配：台名(tvg-name)，台标(tvg-logo)，URL"+"\n\n"
        +"tvg-name=\"台名\""+"\n"
        +"tvg-logo=\"台标\"(可选)"+"\n"
        +"\\r或\\n(二选一)"+"\n"
        +"http://或https://(+任意字符).m3u8?参数名=值&参数名=值...(参数全部可选)"+"\n\n"
        +"****************************************"+"\n\n"
        +"规则3"+"\n"
        +"匹配：台标(tvg-logo)，台名(两逗号之间文本)，URL"+"\n\n"
        +"tvg-logo=\"台标\""+"\n"
        +","+"\n"
        +"台名"+"\n"
        +",或\\n(二选一)"+"\n"
        +"http://或https://(+任意字符).m3u8?参数名=值&参数名=值...(参数全部可选)"+"\n\n"
        +"****************************************"+"\n\n"
        +"规则4"+"\n"
        +"和第三项相同，台标是可选的"+"\n\n"
        +",(可选)"+"\n"
        +"tvg-logo=\"台标\"(可选)"+"\n"
        +","+"\n"
        +"台名"+"\n"
        +",或\\n(二选一)"+"\n"
        +"http://或https://(+任意字符，不限.m3u8后缀)?参数名=值&参数名=值...(参数全部可选)"+"\n\n"
        +"****************************************"+"\n\n"
        +"规则5"+"\n"
        +"简单粗暴，无附加格式，匹配：台标，台名，URL"+"\n\n"
        +"台标(可选)"+"\n"
        +","+"\n"
        +"台名"+"\n"
        +","+"\n"
        +"http://或https://(+任意字符，不限.m3u8后缀)?参数名=值&参数名=值...(参数全部可选)";

            return showmsg;
        }
    }

    public class RegexManager()
    {
        public int[] UseGroup;
        /// <summary>
        /// 使用正则表达式解析直播源数据，直播源数据是包含若干个M3U8直播源的字符串
        /// </summary>
        /// <param name="videodata">直播源数据</param>
        /// <param name="recreg">正则表达式</param>
        /// <returns>正则表达式匹配出的列表</returns>
        public List<VideoDetailList> DoRegex(string videodata, string recreg)
        {
            MatchCollection match = Regex.Matches(videodata, UseRegex(recreg));

            List<VideoDetailList> result = new List<VideoDetailList>();
            for (int i = 0; i<match.Count; i++)
            {
                VideoDetailList videoDetail = new VideoDetailList()
                {
                    //ID，台标，台名，直播源地址
                    Id=i,
                    LogoLink=match[i].Groups[UseGroup[0]].Value=="" ? "fclive_tvicon.png" : match[i].Groups[UseGroup[0]].Value,
                    _LogoLink=match[i].Groups[UseGroup[0]].Value=="" ? "fclive_tvicon.png" : match[i].Groups[UseGroup[0]].Value,
                    SourceName=match[i].Groups[UseGroup[1]].Value,
                    SourceLink=match[i].Groups[UseGroup[2]].Value,
                    FullM3U8Str = MakeFullStr(match[i], recreg),
                    GroupTitle=Regex.Match(MakeFullStr(match[i], recreg), @"group-title=""(.*?)""").Groups[1].Value,
                    TVGID=Regex.Match(MakeFullStr(match[i], recreg), @"tvg-id=""(.*?)""").Groups[1].Value,
                    TVGCountry=Regex.Match(MakeFullStr(match[i], recreg), @"tvg-country=""(.*?)""").Groups[1].Value,
                    TVGLanguage=Regex.Match(MakeFullStr(match[i], recreg), @"tvg-language=""(.*?)""").Groups[1].Value,
                    isHTTPS=match[i].Groups[UseGroup[2]].Value.ToLower().StartsWith("https://") ? true : false,
                    isIPV6= Regex.Match(match[i].Groups[UseGroup[2]].Value, @"(\[[0-9a-fA-F:]+\])").Success,
                    FileName=Regex.Match(match[i].Groups[UseGroup[2]].Value, @"\/([^\/]+\.m3u8)").Groups[1].Value
                };
                //videoDetail.LogoLink=videoDetail.LogoLink=="" ? "fclive_tvicon.png" : videoDetail.LogoLink;

                result.Add(videoDetail);
            }

            return result;
        }
        /// <summary>
        /// 根据提供的索引来获取对应的正则表达式
        /// <para>如要修改表达式，可能需要同步修改UseGroup，MakeFullStr</para>
        /// </summary>
        /// <param name="index">索引</param>
        /// <returns>索引对应的正则表达式</returns>
        public string UseRegex(string index)
        {
            switch (index)
            {
                case "1":
                    UseGroup =new int[] { 1, 2, 3 };
                    return @"(?:.*?tvg-logo=""([^""]*)"")?(?:.*?tvg-name=""([^""]*)"")?.*\r?\n?((http|https)://\S+\.m3u8(\?(.*?))?(?=\n))";
                //1.2为1的不限制M3U8后缀的版本
                case "1.2":
                    UseGroup =new int[] { 1, 2, 3 };
                    return @"(?:.*?tvg-logo=""([^""]*)"")?(?:.*?tvg-name=""([^""]*)"")?.*\r?\n?((http|https)://\S+(.*?)(?=\n))";
                //只有2方案是先匹配台名后匹配台标
                case "2":
                    UseGroup =new int[] { 2, 1, 3 };
                    return @"(?:.*?tvg-name=""([^""]*)"")(?:.*?tvg-logo=""([^""]*)"")?.*\r?\n?((http|https)://\S+\.m3u8(\?(.*?))?(?=\n))";
                //2.2为2的不限制M3U8后缀的版本
                case "2.2":
                    UseGroup =new int[] { 2, 1, 3 };
                    return @"(?:.*?tvg-name=""([^""]*)"")(?:.*?tvg-logo=""([^""]*)"")?.*\r?\n?((http|https)://\S+(.*?)(?=\n))";
                case "3":
                    UseGroup =new int[] { 4, 6, 9 };
                    return @"((#EXTINF)(.*?)(?:tvg-logo=""([^""]*)""(.*?)?,(.+?)(,)?(\n)?(?=((http|https):\S+\.m3u8(\?(.*?))?(?=\n)))))";
                //UseGroup =new int[] { 3, 5, 8 };
                //return @"((tvg-logo=""([^""]*)"")(.*?))?,(.+?)(,)?(\n)?(?=((http|https)://\S+\.m3u8(\?(.*?))?(?=\n)))";
                //3.2为3的不限制M3U8后缀的版本
                case "3.2":
                    UseGroup = new int[] { 4, 6, 9 };
                    return @"((#EXTINF)(.*?)(?:tvg-logo=""([^""]*)""(.*?)?,(.+?)(,)?(\n)?(?=((http|https):\S+(.*?))(?=\n))))";
                //UseGroup =new int[] { 3, 5, 8 };
                //return @"((tvg-logo=""([^""]*)"")(.*?))?,(.+?)(,)?(\n)?(?=((http|https)://\S+(.*?)(?=\n)))";
                case "4":
                    UseGroup =new int[] { 5, 7, 10 };
                    return @"#EXTINF(.*?)(,?((tvg-logo=""([^""]*)"")(.*?))?,(.+?)(,)?(\n)?(?=((http|https)://\S+(.*?)(?=\n))))";
                //UseGroup =new int[] { 3, 5, 8 };
                //return @",?((tvg-logo=""([^""]*)"")(.*?)),(.+?)(,)?(\n)?(?=((http|https)://\S+(.*?)(?=\n)))";
                case "5":
                    UseGroup =new int[] { 2, 5, 7 };
                    return @"(((http|https)://\S+)(,))?(.*?)(,)((http|https)://\S+(?=\n|\s{1}))";
                default:
                    return "";
            }

        }

        public string MakeFullStr(Match match, string recreg)
        {
            if (recreg.StartsWith("1") || recreg.StartsWith("2") || recreg.StartsWith("5"))
            {
                return match.Groups[0].Value;
            }
            else if (recreg.StartsWith("3"))
            {
                return match.Groups[1].Value+ match.Groups[9].Value;
            }
            else if (recreg.StartsWith("4"))
            {
                return match.Groups[0].Value+ match.Groups[10].Value;
            }

            return "";
        }

        /// <summary>
        /// 获取当前选择的规则索引
        /// </summary>
        /// <returns></returns>
        public string GetRegexOptionIndex(bool isOnlyM3U8, string selectOption)
        {
            switch (selectOption)
            {
                case "1":
                    if (!isOnlyM3U8)
                    {
                        return "1.2";
                    }
                    return "1";
                case "2":
                    if (!isOnlyM3U8)
                    {
                        return "2.2";
                    }
                    return "2";
                case "3":
                    if (!isOnlyM3U8)
                    {
                        return "3.2";
                    }
                    return "3";
                case "4":
                    return "4";
                case "5":
                    return "5";
                default:
                    return "0";
            }

        }
    }

    public static class GlobalParameter
    {
        public const string DefaultPlayM3U8Name = "CGTN (英语)";
        public const string DefaultPlayM3U8URL = @"https://english-livetx.cgtn.com/hls/yypdyyctzb_hd.m3u8";
        public const bool StartAutoPlayM3U8 = true;
        public const bool AppDarkMode = false;
        public const int VideoCheckThreadNum = 50;
        public const string VideoCheckUA = @"Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/115.0.0.0 Safari/537.36";
  }
}
