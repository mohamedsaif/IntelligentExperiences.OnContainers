(function () {
    'use strict';

    var mediaStream = null;
    var webcamList = [];
    var currentCam = null;
    var photoReady = false;
    var uploadedCount = 0;
    var delay = 15000;
    var isSuspended = false;

    var init = function () {
        navigator.getUserMedia = navigator.getUserMedia || navigator.webkitGetUserMedia || navigator.mozGetUserMedia;

        document.getElementById('switch').addEventListener('click', nextWebCam, false);
        document.getElementById('suspend').addEventListener('click', suspendFrameSending, false);

        if (navigator.mediaDevices && navigator.mediaDevices.enumerateDevices) {
            navigator.mediaDevices.enumerateDevices().then(devicesCallback);
        }
        else if (navigator.getUserMedia) {
            document.getElementById('tooltip').innerHTML = 'Cannot switch web cams because navigator.mediaDevices.enumerateDevices is unsupported by your browser.';

            navigator.getUserMedia({ video: true /*, audio: true */ }, initializeVideoStream, getUserMediaError);
        }
        else {
            writeError('You are using a browser that does not support the Media Capture API');
        }
    };

    var initializeVideoStream = function (stream) {
        mediaStream = stream;

        var video = document.getElementById('videoTag');
        if (typeof (video.srcObject) !== 'undefined') {
            video.srcObject = mediaStream;
        }
        else {
            video.src = URL.createObjectURL(mediaStream);
        }
        if (webcamList.length > 1) {
            document.getElementById('switch').disabled = false;
            //ul.children[currentCam].setAttribute("class", "list-group-item active");
        }
        capture();
    };

    var capture = function () {

        if (!mediaStream) {
            photoReady = false;
            return;
        }

        var video = document.getElementById('videoTag');
        var canvas = document.getElementById('canvasTag');

        var videoWidth = video.videoWidth;
        var videoHeight = video.videoHeight;

        if (canvas.width !== videoWidth || canvas.height !== videoHeight) {
            canvas.width = videoWidth;
            canvas.height = videoHeight;
        }

        var ctx = canvas.getContext('2d');
        ctx.drawImage(video, 0, 0, video.videoWidth, video.videoHeight);
        photoReady = true;
        document.getElementById('photoViewText').innerHTML = 'Last captured frame: ' + Date();
        savePhoto();
    };

    var savePhoto = function () {
        if (photoReady) {
            var canvas = document.getElementById('canvasTag');
            canvas.toBlob(function (blob) {
                if (blob == null) {
                    setTimeout(capture, delay);
                    return;
                }
                //If suspended, skip and try again after delay
                if (isSuspended == true) {
                    setTimeout(capture, delay);
                    return;
                }

                //Upload to server
                var data = new FormData();
                data.append('frame', blob, 'deviceId' + uploadedCount + "." + 'jpg');
                $.ajax({
                    type: "POST",
                    url: "/Home/CaptureFrame",
                    data: data,
                    cache: false,
                    processData: false,
                    contentType: false,
                    success: function (data) {
                        document.getElementById('uploadedViewText').innerHTML = 'Uploaded Count: ' + uploadedCount;
                    },
                    error: function (data) {
                        document.getElementById('uploadedViewText').innerHTML = data;
                    }
                });

                uploadedCount += 1;
                setTimeout(capture, delay);
            });
        }
    };

    var nextWebCam = function () {
        document.getElementById('switch').disabled = true;
        if (currentCam !== null) {
            currentCam++;
            if (currentCam >= webcamList.length) {
                currentCam = 0;
            }
            var video = document.getElementById('videoTag');
            video.srcObject = null;
            video.src = null;
            if (mediaStream) {
                var videoTracks = mediaStream.getVideoTracks();
                videoTracks[0].stop();
                mediaStream = null;
            }
        }
        else {
            currentCam = 0;
        }

        navigator.mediaDevices.getUserMedia({
            video: {
                width: 1280,
                height: 720,
                deviceId: { exact: webcamList[currentCam] }
            }
        }).then(initializeVideoStream)
            .catch(getUserMediaError);
    };

    var deviceChanged = function () {
        navigator.mediaDevices.removeEventListener('devicechange', deviceChanged);
        // Reset the webcam list and re-enumerate
        webcamList = [];
        /*eslint-disable*/
        navigator.mediaDevices.enumerateDevices().then(devicesCallback);
        /*eslint-enable*/
    };

    var devicesCallback = function (devices) {
        //var ul = document.getElementById("cameras");
        //ul.innerHTML = "";
        // Identify all webcams
        for (var i = 0; i < devices.length; i++) {
            if (devices[i].kind === 'videoinput') {
                webcamList[webcamList.length] = devices[i].deviceId;
                //var li = document.createElement("li");
                //li.appendChild(document.createTextNode(devices[i].deviceId));
                //ul.appendChild(li);
            }
        }

        if (webcamList.length > 0) {
            // Start video with the first device on the list
            nextWebCam();
            if (webcamList.length > 1) {
                document.getElementById('switch').disabled = false;
            }
            else {
                document.getElementById('switch').disabled = true;
            }
        }
        else {
            writeError('Webcam not found.');
        }
        navigator.mediaDevices.addEventListener('devicechange', deviceChanged);
    };

    var suspendFrameSending = function () {
        if (isSuspended == true) {
            isSuspended = false;
            document.getElementById('suspend').innerText = 'Suspend Sending Frames'
        }
        else {
            isSuspended = true;
            document.getElementById('suspend').innerText = 'Resume Sending Frames'
        }
    }

    var writeError = function (string) {
        var elem = document.getElementById('error');
        var p = document.createElement('div');
        p.appendChild(document.createTextNode('ERROR: ' + string));
        elem.appendChild(p);
    };

    var getUserMediaError = function (e) {
        if (e.name.indexOf('NotFoundError') >= 0) {
            writeError('Webcam not found.');
        }
        else {
            writeError('The following error occurred: "' + e.name + '" Please check your webcam device(s) and try again.');
        }
    };

    init();

}());