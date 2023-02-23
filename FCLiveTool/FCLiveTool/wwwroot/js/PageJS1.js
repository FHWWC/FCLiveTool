try {
    const { ajax } = require("jquery");
    const { textContent } = require("min-document");
}
catch (ex) {
    console.log(ex);
}

var currentvideo = "";
var RM3UIndex = 1;
var RegexIndex = "";
var DM3Ufilename = "";
var mcount;
var searchcount;
var M3U8PC = 1;
var SearchRList = new Array();
var isSearchMode = false;


//解析M3U8入口点
function LoadM3U8(urls, fname, recreg) {
    document.querySelector('#testtest').innerText = "";
    document.querySelector('#RM3UPanel').hidden = false;
    document.querySelector('.MToolsPanel').style.display = "block";
    document.querySelector("#CMPageText").value = 1;
    M3U8PC = 1;
    isSearchMode = false;

    //查看是否有推荐的解析方案
    if (recreg != undefined) {
        if (isNaN(recreg)) {
            RM3UIndex = 1;
        }
        else {
            RM3UIndex = recreg;
        }
        //目前暂不处理匹配规则的相关复选框

    }

    if (urls != "") {
        //给相关全局变量赋值
        currentvideo = urls;
        if (fname != undefined) {
            fname = decodeURIComponent(fname);
            if (fname.includes("?")) {
                DM3Ufilename = fname.substring(fname.lastIndexOf("/") + 1, fname.lastIndexOf("?"));
            }
            else {
                DM3Ufilename = fname.substring(fname.lastIndexOf("/") + 1);
            }

        }

        var array = DoRegex(urls);

        mcount = array.length;
        if (mcount < 1) {
            document.querySelector('#testtest').innerText = "No M3U8 file!";
            ShowNotice("无法解析", "当前表达式解析不到M3U8链接，请在右下的下拉框中更换其他解析方案！");
            HideCM3U8();

            return false;
        }
        else if (urls.length > 100) {
            document.querySelector(".CM3U8Panel").style.display = "block";
        }

        document.querySelector(".loadingdialog").style.display = "block";
        document.querySelector("#loadingdialogMSG").innerText = "解析中，请稍后...";
        document.querySelector("#M3U8Count").innerText = " " + array.length + " ";

        LoadVideo(array, 1, array.length, false);

        //如果需要将所有内容加载完后隐藏提示，则在这里执行、
        //document.querySelector(".loadingdialog").style.display = "";
    }
    else {
        document.querySelector('#testtest').innerText = "Error!";
        ShowNotice("请勿修改源数据", "Error! 如果您什么也没做，请重新获取列表！");
        HideCM3U8();

        return false;
    }

}

function LoadVideo(urls, start, end, issearch) {

    document.querySelector('#testtest').innerText = "";

    //根据情况调整分页数
    if (urls.length <= 100) {
        HideCM3U8();
    }

    if (!Number.isInteger(start) || !Number.isInteger(end)) {
        return;
    }

    if (!issearch) {
        if (end - start > 99) {
            start = 1;
            end = 100;
        }
    }


    if (urls.length == end) {
        document.querySelector('#CMGoNext').disabled = true;
    }
    else {
        document.querySelector('#CMGoNext').disabled = false;
    }
    if (start == 1) {
        document.querySelector('#CMGoBack').disabled = true;
    }
    else {
        document.querySelector('#CMGoBack').disabled = false;
    }

    document.querySelector("#CMPageText").value = Math.ceil(start / 100);


    for (var i = start - 1; i < end; i++) {
        try {
            var tdiv = document.createElement("div");
            tdiv.className = "vlistItems";
            tdiv.innerHTML += "<p class='vlistItem whitetext' >直播信号  </p>";


            //添加台标
            var tdiv2 = document.createElement("div");
            tdiv2.className = "vlistLogo";

            var tIMG = document.createElement("img");
            if (urls[i][RegexIndex[0]] == "" || urls[i][RegexIndex[0]] == undefined) {
                tIMG.src = "/img/TVSICON.png";
            }
            else {
                tIMG.src = urls[i][RegexIndex[0]];
            }
            tIMG.height = "30";

            tdiv2.appendChild(tIMG);
            tdiv.appendChild(tdiv2);


            var turls = urls[i][RegexIndex[2]];
            //添加HTTPS标签
            var thtag = document.createElement("img");

            if (turls.includes("https://") == true) {
                //thtag = document.createElement("img");
                thtag.src = "/img/HTTPSTag.png";
            }
            thtag.width = "40";

            tdiv.appendChild(thtag);



            //如果是搜索模式，判断一下是否匹配搜索关键字
            if (issearch) {
                var tsearchv = document.querySelector(".m3u8searchTb").value;
                if (tsearchv != "") {
                    if (urls[i][RegexIndex[1]].includes(tsearchv)) {
                        SearchRList.push(urls[i]);
                    }

                }

            }


            //添加台标文本以及链接。
            var tlinks = document.createElement("a");
            tlinks.className = "vlistlink";
            tlinks.href = "#";

            if (urls[i][RegexIndex[1]] == "" || urls[i][RegexIndex[1]] == undefined) {
                tlinks.innerText = "文件名:   " + turls.substring(turls.lastIndexOf("/"));
            }
            else {
                tlinks.innerText = urls[i][RegexIndex[1]];
            }

            //var link = urls[i][RegexIndex[2]].replace("http:", "https:");
            tlinks.title = turls;
            tlinks.onclick = function () {
                //window.open(this.title);
                var tdlink = this.title;

                var tForm = document.createElement("form");
                tForm.action = tdlink;
                tForm.target = "_blank";
                tForm.method = "POST";
                document.body.appendChild(tForm);
                tForm.submit();
                document.body.removeChild(tForm);

                /*
                             if (tdlink.includes("http://") == true) {
                                CopyText(tdlink);
                                alert("已复制M3U8链接到剪贴板。");
                            }
                            else {
                                var tForm = document.createElement("form");
                                tForm.action = tdlink;
                                tForm.target = "_blank";
                                tForm.method = "POST";
                                document.body.appendChild(tForm);
                                tForm.submit();
                                document.body.removeChild(tForm);
                            }
                 */
            }

            tdiv.appendChild(tlinks);



            //添加失效标签
            /*
                     $.ajax({
                        type: "post",
                        url: turls,
                        dataType: "jsonp"
                    }).always(function (xhr) {
                        //alert(xhr.status)
                    });
             */


            //window.btoa(encodeURIComponent(urls[i][RegexIndex[1]])) + "@@@" + window.btoa(encodeURIComponent(urls[i][RegexIndex[0]])) + "@@@" + window.btoa(encodeURIComponent(urls[i][RegexIndex[2]]));
            var tbta = window.btoa(encodeURIComponent(tlinks.innerText)) + "@@@" + window.btoa(encodeURIComponent(tIMG.src)) + "@@@" + window.btoa(encodeURIComponent(turls));
            //添加删除按钮
            var tdelBtn = document.createElement("input");
            tdelBtn.className = "vlistRight dellogo hover-lightpink focus-white";
            tdelBtn.title = tbta;
            tdelBtn.type = "button";

            tdelBtn.onclick = function () {
                var tdel = abottoba(this.title);

                currentvideo = currentvideo.replace(decodeURIComponent(tdel[0]) + "\n" + decodeURIComponent(tdel[2]), "");
                this.parentElement.remove();
            }

            tdiv.appendChild(tdelBtn);


            //添加编辑按钮
            var tediBtn = document.createElement("input");
            tediBtn.className = "vlistRight editlogo hover-lightyellow focus-white";
            tediBtn.title = tbta;
            tediBtn.type = "button";

            tediBtn.onclick = function () {
                var tedi = abottoba(this.title);
                document.querySelector("#OldM3U8Name").innerText = decodeURIComponent(tedi[0]);
                document.querySelector("#EditTemp").innerText = this.title;
                document.querySelector(".editpanel").style.display = "block";
            }

            tdiv.appendChild(tediBtn);


            //添加预览按钮
            var tpreBtn = document.createElement("a");
            tpreBtn.className = "vlistRight prevlogo hover-lightblue focus-white";
            tpreBtn.href = "#";
            tpreBtn.title = tbta;

            tpreBtn.onclick = function () {
                var myVideo = videojs('my-player');
                myVideo.src(decodeURIComponent(taa[2]));
                myVideo.play();
            }

            tdiv.appendChild(tpreBtn);


            document.querySelector('#testtest').appendChild(tdiv);
        }
        catch (ex) {
            console.log(i);
            //忽略，仅调试时使用
        }
    }

    //如果处于搜索状态，那么需要重新调用LoadVideo，读取通过关键词筛选出来的数据
    if (issearch) {
        LoadVideo(SearchRList, 1, SearchRList.length, false);
        searchcount = SearchRList.length;
    }

    document.querySelector(".loadingdialog").style.display = "";
}

function ChangeReadM3U(index) {
    if (isSearchMode) {
        ShowNotice("无法更改解析方案", "搜索结果不支持更改解析方案，如要更换表达式，请点击搜索框旁的重新加载列表按钮，然后再选择其他解析表达式");
        return;
    }
    var rocb = document.querySelector("#ROCB").checked;

    if (isNaN(index)) {
        if (rocb) {
            RM3UIndex = 1;
        }
        else {
            RM3UIndex = 1.2;
        }
    }
    else {
        if (!rocb) {
            if (index == 1) {
                RM3UIndex = 1.2;
            }
            else if (index == 2) {
                RM3UIndex = 2.2;
            }
            else {
                RM3UIndex = index;
            }
        }
        else {
            RM3UIndex = index;
        }

    }
    //document.querySelector(".RM3UBtnL").value = document.querySelectorAll(".RM3UList")[RM3UIndex-1].innerText;
    document.querySelector(".RM3UBtnL").value = "规则 " + index;

    if (currentvideo == "") {
        return;
    }
    LoadM3U8(currentvideo);
}

function SaveToDownM3U() {
    if (currentvideo == "") {
        return;
    }

    var tdown = document.createElement("a");
    var blob = new Blob([currentvideo]);
    tdown.download = DM3Ufilename;
    tdown.href = URL.createObjectURL(blob);
    tdown.click();
    URL.revokeObjectURL(blob);
}

function GetVD(url, recreg) {
    /*
         var v = RDRD();
        if (v == "" || v == undefined) {
            return;
        }
     */
    document.querySelector(".loadingdialog").style.display = "block";
    document.querySelector("#loadingdialogMSG").innerText = "请求中，请稍后...";

    $.ajax({
        type: "post",
        url: "/api/GetVDetail?url=" + encodeURI(url),
        error: function () {
            ShowNotice("", "出现异常！请稍后重试");

            document.querySelector(".loadingdialog").style.display = "";
        }
    }).then(function (value) {
        var isfailed = true;
        if (value != undefined) {
            switch (value) {
                case "-1":
                    ShowNotice("", "参数错误！");
                    break;
                case "1":
                case "2":
                    ShowNotice("", "获取数据失败！请稍后重试");
                    break;
                default:

                    LoadM3U8(value.replace(/\r\n/g, "\n"), url, recreg) == false ? isfailed = true : isfailed = false;

                    break;
            }
        }
        else {
            ShowNotice("", "获取数据失败！");
        }

        if (isfailed) {
            document.querySelector(".loadingdialog").style.display = "";
        }
        else {
            window.scrollTo(0, document.querySelector("#VLTT").offsetTop);
        }

    });

}

function CMPageJump() {
    var index = document.querySelector("#CMPageText").value;
    index = parseInt(index);
    if (isNaN(index)) return;


    //根据情况调整分页数
    var pages;
    if (isSearchMode) {
        pages = Math.ceil(SearchRList.length / 100);
    }
    else {
        pages = Math.ceil(mcount / 100);
    }
    if (index > pages || index < 1) {
        ShowNotice("", "输入的页数不正确");
        return;
    }
    //仅用作两个按钮
    M3U8PC = index;

    var array;
    if (isSearchMode) {
        array = SearchRList;
    }
    else {
        array = DoRegex(currentvideo);
    }

    if (pages == index) {
        LoadVideo(array, (index - 1) * 100 + 1, array.length, false);
    }
    else {
        LoadVideo(array, (index - 1) * 100 + 1, (index - 1) * 100 + 100, false);
    }


}

function CMBack() {
    if (M3U8PC > 1) {

        M3U8PC -= 1;

        var array;
        if (isSearchMode) {
            array = SearchRList;
        }
        else {
            array = DoRegex(currentvideo);
        }

        LoadVideo(array, (M3U8PC - 1) * 100 + 1, (M3U8PC - 1) * 100 + 100, false);
    }
}

function CMNext() {
    var pages;
    if (isSearchMode) {
        pages = Math.ceil(SearchRList.length / 100);
    }
    else {
        pages = Math.ceil(mcount / 100);
    }

    if (M3U8PC < pages) {

        M3U8PC += 1;

        var array;
        if (isSearchMode) {
            array = SearchRList;
        }
        else {
            array = DoRegex(currentvideo);
        }

        if (M3U8PC == pages) {
            LoadVideo(array, (M3U8PC - 1) * 100 + 1, array.length, false);
        }
        else {
            LoadVideo(array, (M3U8PC - 1) * 100 + 1, (M3U8PC - 1) * 100 + 100, false);
        }

    }

}

function DoRegex(urls) {

    var reg;
    switch (RM3UIndex) {
        case 1:
            reg = /(?:.*?tvg-logo="([^"]*)")?(?:.*?tvg-name="([^"]*)")?.*\r?\n?((http|https):\/\/\S+\.m3u8(\?(.*?))?(?=\n|,))/gi;
            RegexIndex = [1, 2, 3];
            break;
        //1.2为1的不限制M3U8后缀的版本
        case 1.2:
            reg = /(?:.*?tvg-logo="([^"]*)")?(?:.*?tvg-name="([^"]*)")?.*\r?\n?((http|https):\/\/\S+(.*?)(?=\n|,))/gi;
            RegexIndex = [1, 2, 3];
            break;
        case 2:
            reg = /((tvg-logo="([^"]*)")(.*?))?\,(.+?)(,)?(\n)?(?=((http|https):\/\/\S+\.m3u8(\?(.*?))?(?=\n|,)))/gi;
            RegexIndex = [3, 5, 8];
            break;
        //2.2为2的不限制M3U8后缀的版本
        case 2.2:
            reg = /((tvg-logo="([^"]*)")(.*?))?\,(.+?)(,)?(\n)?(?=((http|https):\/\/\S+(.*?)(?=\n|,)))/gi;
            RegexIndex = [3, 5, 8];
            break;
        case 3:
            reg = /\,?((tvg-logo="([^"]*)")(.*?))\,(.+?)(,)?(\n)?(?=((http|https):\/\/\S+(.*?)(?=\n|,)))/gi;
            RegexIndex = [3, 5, 8];
            break;
        case 4:
            reg = /(((http|https):\/\/\S+)(,))?(.*?)(,)((http|https):\/\/\S+(?=\n|,))/gi;
            RegexIndex = [2, 5, 7];
            break;

    }

    return [...urls.matchAll(reg)];
}

function CMFPBtn() {
    var pages;
    if (isSearchMode) {
        pages = Math.ceil(SearchRList.length / 100);
    }
    else {
        pages = Math.ceil(mcount / 100);
    }

    M3U8PC = 1;

    var array;
    if (isSearchMode) {
        array = SearchRList;
    }
    else {
        array = DoRegex(currentvideo);
    }

    if (pages == 1) {
        LoadVideo(array, 1, array.length, false);
    }
    else {
        LoadVideo(array, 1, 100, false);
    }

}

function CMLPBtn() {
    var pages;
    if (isSearchMode) {
        pages = Math.ceil(SearchRList.length / 100);
    }
    else {
        pages = Math.ceil(mcount / 100);
    }

    M3U8PC = pages;

    var array;
    if (isSearchMode) {
        array = SearchRList;
    }
    else {
        array = DoRegex(currentvideo);
    }

    LoadVideo(array, (M3U8PC - 1) * 100 + 1, array.length, false);

}

function SearchM3U8() {
    var result = document.querySelector(".m3u8searchTb").value;
    if (result.length < 3) {
        ShowNotice("", "搜索的关键字不能小于3个字符!");
        return;
    }

    SearchRList = new Array();
    isSearchMode = true;

    M3U8PC = 1;
    var array = DoRegex(currentvideo);
    LoadVideo(array, 1, array.length, isSearchMode);
}

function RestoreMSearch() {
    isSearchMode = false;
    LoadM3U8(currentvideo, DM3Ufilename, RM3UIndex)
}

function UpdateM3U8Name()
{
    var newname = document.querySelector(".m3u8editTb").value;
    document.querySelector(".editerror").innerText = "";

    if (newname == "") {
        document.querySelector(".editerror").innerText = "名称不能为空！";
        return;
    }

    //获取值并开始替换
    var oldvalue = abottoba(document.querySelector("#EditTemp").innerText);
    var newvalue = newname + "\n" + decodeURIComponent(oldvalue[2]);
    currentvideo = currentvideo.replace(decodeURIComponent(oldvalue[0]) + "\n" + decodeURIComponent(oldvalue[2]), newvalue);

    document.querySelector(".m3u8editTb").value="";
    document.querySelector(".editpanel").style.display = "";
    //重新加载当前页面
    document.querySelector("#CMPageText").value = M3U8PC;
    CMPageJump();
}

function CancelUpdateMN() {
    document.querySelector(".editpanel").style.display = "";
    document.querySelector(".editerror").innerText = "";
}


//以下是非主要函数
function CopyText(value) {
    var newTB = document.createElement("textarea");
    newTB.value = value;
    document.body.appendChild(newTB);

    newTB.select();
    document.execCommand("copy");

    document.body.removeChild(newTB);
    ShowNotice("提示信息", "已复制下载链接到剪贴板");
}

function RecentSetSource(value) {
    var myVideo = videojs('my-player');
    myVideo.src(value);
    myVideo.play();
}

function RecentOpenLink(link) {
    var tForm = document.createElement("form");
    tForm.action = link;
    tForm.target = "_blank";
    tForm.method = "POST";
    document.body.appendChild(tForm);
    tForm.submit();
    document.body.removeChild(tForm);
}

function ShowNotice(title, content) {
    document.querySelector(".shownotice").style.display = "block";
    if (title != "") {
        document.querySelector("#NoticeTitle").innerText = title;
    }
    document.querySelector("#NoticeContent").innerText = content;
}

function CloseNotice() {
    document.querySelector(".shownotice").style.display = "";
}

function HideCM3U8() {
    document.querySelector(".CM3U8Panel").style.display = "";
}

/*
 function debounce(callBack, time) {
    let timer;

    return function () {
        //this指向debounce
        let context = this;
        //即参数，func,wait
        let args = arguments;

        //如果timer不为null, 清除定时器
        if (timer) clearTimeout(timer);

        //定义callNow = !timer
        var callNow = !timer;
        //定义wait时间后把timer变为null，即在wait时间之后事件才会有效
        timer = setTimeout(() => {
            timer = null;
        }, time)
        //如果callNow为true,即原本timer为null，那么执行func函数
        if (callNow) callBack.apply(context, args)
    }
}
 */

function abottoba(value) {
    var tvalue = value.split("@@@");
    return [window.atob(tvalue[0]), window.atob(tvalue[1]), window.atob(tvalue[2])];
}
