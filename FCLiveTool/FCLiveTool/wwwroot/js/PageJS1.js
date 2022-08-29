try {
    const { ajax } = require("jquery");
    const { textContent } = require("min-document");
}
catch (ex) {

}

async function LoadM3U8(urls) {
    document.querySelector('#testtest').innerText = "";

    if (urls != "") {
        var reg = /(?:.*?tvg-logo="([^"]*)")?(?:.*?tvg-name="([^"]*)")?.*\r?\n(http?\S+.m3u8)/gi;
        var array = [...urls.matchAll(reg)];

        if (array.length < 1) {
            document.querySelector('#testtest').innerText = "No M3U8 file!";
            return;
        }

        //async function
        return new Promise(resolve => {
            setTimeout(() => {
                LoadVideo(array);
            }, 0);
        });

    }
    else {
        document.querySelector('#testtest').innerText = "Error!";
    }

}


function LoadVideo(urls) {
    for (var i = 0; i < urls.length; i++) {
        var tdiv = document.createElement("div");
        tdiv.className = "vlistItems";
        tdiv.innerHTML += "<p class='vlistItem whitetext' >直播信号  </p>";
        document.querySelector('#testtest').appendChild(tdiv);


        //添加台标
        var tdiv2 = document.createElement("div");
        tdiv2.style.width = "200px";
        tdiv2.style.margin = "0 10px";
        tdiv2.style.float = "left";

        var tIMG = document.createElement("img");
        if (urls[i][1] == "" || urls[i][1] == undefined) {
            tIMG.src = "/img/TVSICON.png";
        }
        else {
            tIMG.src = urls[i][1];
        }
        tIMG.height = "30";

        tdiv2.appendChild(tIMG);
        document.querySelectorAll('.vlistItems')[i].appendChild(tdiv2);


        //添加链接。
        var tlinks = document.createElement("a");
        tlinks.href = "#";
        tlinks.style.textDecoration = "none";

        if (urls[i][2] == "" || urls[i][2] == undefined) {
            tlinks.innerText = "---   " + urls[i][3].substring(urls[i][3].lastIndexOf("/"));
        }
        else {
            tlinks.innerText = urls[i][2];
        }

        //var link = urls[i][3].replace("http:", "https:");
        tlinks.title = urls[i][3];
        tlinks.onclick = function () {
            //window.open(this.title);
            var tdlink = this.title;
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
        }

        document.querySelectorAll('.vlistItems')[i].appendChild(tlinks);


        //添加按钮
        var tpreBtn = document.createElement("a");
        tpreBtn.className = "vlistDown";
        tpreBtn.href = "#";
        //tpreBtn.title = window.btoa(encodeURIComponent(urls[i][2])) + "@@@" + window.btoa(encodeURIComponent(urls[i][1])) + "@@@" + window.btoa(encodeURIComponent(urls[i][3]));
        tpreBtn.title = window.btoa(encodeURIComponent(tlinks.innerText)) + "@@@" + window.btoa(encodeURIComponent(tIMG.src)) + "@@@" + window.btoa(encodeURIComponent(urls[i][3]));

        tpreBtn.onclick = function () {
            var myVideo = videojs('my-player');
            var taa = abottoba(this.title);
            myVideo.src(taa[2]);
            myVideo.play();
        }

        tpreBtn.innerText = "预览";
        //tdownBtn.style.backgroundImage = "url('/img/Download.png')";

        document.querySelectorAll('.vlistItems')[i].appendChild(tpreBtn);

    }
}


function CopyText(value) {
    var newTB = document.createElement("textarea");
    newTB.value = value;
    document.body.appendChild(newTB);

    newTB.select();
    document.execCommand("copy");

    document.body.removeChild(newTB);
}

function RecentSetSource(value) {
    var myVideo = videojs('my-player');
    myVideo.src(value);
    myVideo.play();
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