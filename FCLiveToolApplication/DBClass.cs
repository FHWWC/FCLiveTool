using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Maui.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
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
        public string LogoLink { get; set; }
        public string SourceName { get; set; }
        public string SourceLink { get; set; }
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

    public class APPPermissions
    {
        /// <summary>
        /// 检查是否有文件读取的权限
        /// </summary>
        /// <returns>状态码。0：已经有权限或已成功获取到权限；1：读取没有权限；2：写入没有权限；3：读写都没有权限；</returns>
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
    public class VideoManager:ContentPage
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="M3U8PlayList"></param>
        /// <param name="VideoIfm">1：名称；2：URL；</param>
        /// <returns></returns>
        public async Task<string> DownloadAndReadM3U8File(List<string[]> M3U8PlayList, string[] VideoIfm)
        {
            M3U8PlayList.Clear();
            using (HttpClient httpClient = new HttpClient())
            {
                int statusCode;
                httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(@"Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/115.0.0.0 Safari/537.36");
                HttpResponseMessage response = null;

                try
                {
                    //detail.SourceLink 不清除\n和\r符可能会带来影响，这里暂时不清除
                    response = await httpClient.GetAsync(VideoIfm[1]);

                    statusCode=(int)response.StatusCode;
                    if (!response.IsSuccessStatusCode)
                    {
                        return "获取文件失败，请稍后重试！\n"+"HTTP错误代码："+statusCode;
                    }
                }
                catch (Exception)
                {
                    return "无法连接到对方服务器，请检查您的网络或者更换一个直播源！";
                }


                VideoIfm[0]=VideoIfm[0].Replace("\r", "").Replace("\n", "").Replace("\r\n", "");
                try
                {
                    using (StreamReader sr = new StreamReader(await response.Content.ReadAsStreamAsync()))
                    {
                        string r = "";
                        string tProperties = "";
                        while ((r= sr.ReadLine())!=null)
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
                                //该处准备改为正则表达式匹配，因为某些直播源的URL比较复杂会匹配出错
                                Match m3u8nameResult = Regex.Match(r, @"\/([^\/]+\.m3u8)");
                                string m3u8name =m3u8nameResult.Groups[1].Value;

                                Match tPResult = Regex.Match(tProperties, @"RESOLUTION=(.*?)(,|\n)");
                                string tpr = tPResult.Groups[1].Value.Replace("\"", "");

                                M3U8PlayList.Add(new string[] { m3u8name, r, tpr=="" ? "---" : tpr });

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
    }
}
