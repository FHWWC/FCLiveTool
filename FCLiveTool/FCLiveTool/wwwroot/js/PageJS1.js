try {
    const { ajax } = require("jquery");
    const { textContent } = require("min-document");
}
catch (ex) {

}

var currentvideo = "";
var RM3UIndex = 1;
var RegexIndex = "";
var DM3Ufilename = "";

async function LoadM3U8(urls,fname) {
    document.querySelector('#testtest').innerText = "";
    document.querySelector('#RM3UPanel').hidden = false;
    document.querySelector('.saveedit').hidden = false;

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

        var reg;
        switch (RM3UIndex) {
            case 1:
                reg = /(?:.*?tvg-logo="([^"]*)")?(?:.*?tvg-name="([^"]*)")?.*\r?\n?((http|https):\/\/\S+\.m3u8)/gi;
                RegexIndex = [1,2,3];
                break;
            case 2:
                reg = /((tvg-logo="([^"]*)")(.*?))?\,(.+?)(,)?(\n)?(?=((http|https):\/\/\S+\.m3u8))/gi;
                RegexIndex = [3,5,8];
                break;
        }


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


        //添加台标
        var tdiv2 = document.createElement("div");
        tdiv2.style.width = "200px";
        tdiv2.style.margin = "0 10px";
        tdiv2.style.float = "left";

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


        //添加链接。
        var tlinks = document.createElement("a");
        tlinks.href = "#";
        tlinks.style.textDecoration = "none";

        if (urls[i][RegexIndex[1]] == "" || urls[i][RegexIndex[1]] == undefined) {
            tlinks.innerText = "---   " + turls.substring(turls.lastIndexOf("/"));
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
        tdelBtn.className = "vlistDown dellogo";
        tdelBtn.title = tbta;

        tdelBtn.onclick = function () {
            var tdel = abottoba(this.title);
            currentvideo= currentvideo.replace(decodeURIComponent(tdel[2]), "");
            this.parentElement.remove();
        }

        tdiv.appendChild(tdelBtn);


        //添加编辑按钮
/*
         var tdelBtn = document.createElement("input");
        tdelBtn.className = "vlistDown dellogo";
 */


        //添加预览按钮
        var tpreBtn = document.createElement("a");
        tpreBtn.className = "vlistDown prevlogo";
        tpreBtn.href = "#";
        tpreBtn.title = tbta;

        tpreBtn.onclick = function () {
            var myVideo = videojs('my-player');
            var taa = abottoba(this.title);
            myVideo.src(decodeURIComponent(taa[2]));
            myVideo.play();
        }

        //tpreBtn.innerText = "预览";
        //tpreBtn.style.backgroundImage = "url('/img/Download.png') no-repeat";

        tdiv.appendChild(tpreBtn);


        document.querySelector('#testtest').appendChild(tdiv);
    }
}

function ChangeReadM3U(index,value) {
    if (isNaN(index)) {
        RM3UIndex = 1;
    }
    else {
        RM3UIndex = index;
    }
    //document.querySelector(".RM3UBtnL").value = document.querySelectorAll(".RM3UList")[RM3UIndex-1].innerText;
    document.querySelector(".RM3UBtnL").value = value;

    if (currentvideo == "") {
        return;
    }
    LoadM3U8(currentvideo);
}

function SaveToDownM3U() {
    var tdown = document.createElement("a");
    var blob = new Blob([currentvideo]);
    tdown.download = DM3Ufilename;
    tdown.href = URL.createObjectURL(blob);
    tdown.click();
    URL.revokeObjectURL(blob);
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


function abottoba(value) {
    var tvalue = value.split("@@@");
    return [window.atob(tvalue[0]), window.atob(tvalue[1]), window.atob(tvalue[2])];
}
