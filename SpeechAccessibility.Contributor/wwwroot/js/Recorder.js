/*
 MIT License

 Copyright (c) 2022 The Speech Accessibility Project,
 The Beckman Institute,
 University of Illinois, Urbana-Champaign

 Permission is hereby granted, free of charge, to any person obtaining a copy
 of this software and associated documentation files (the "Software"), to deal
 in the Software without restriction, including without limitation the rights
 to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 copies of the Software, and to permit persons to whom the Software is
 furnished to do so, subject to the following conditions:

 The above copyright notice and this permission notice shall be included in all
 copies or substantial portions of the Software.

 THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 FITNESS FOR A PARTICULAR PURPOSE AND NON-INFRINGEMENT. IN NO EVENT SHALL THE
 AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 SOFTWARE.
 */
import { AudioControls } from "./AudioControls.js?v=2";

const base = $('#consentDiv');


$(function () {
    try {

    checkifRecordingSupported()
    $('#recordingDiv').show();
     recording_interface()

    } catch (e) {
        console.log(`main: ${e}`);   
        let myData = new FormData()

        myData.set(
            'error', e.stack
        )
        $.ajax({
            url: '/Home/LogError',
            type: 'POST',
            cache: false,
            contentType: false,
            processData: false,
            data: myData,
        })
    }
    
});


function recording_interface() {
    try {

        var retryCount = document.getElementById('retryCount').value;
   
        let recordButton = document.getElementById('recordButton')
        if (retryCount > 0) {
            $('#recordButton').text('Rerecord')
            let nextButton = $('#nextButton')
            nextButton.removeAttr('hidden')
            if (retryCount == 1) {
                rerecordMessage.innerText = 'You have 2 rerecord attempts left';
            }
            else if (retryCount == 2) {
                rerecordMessage.innerText = 'You have 1 rerecord attempt left';
            }
            
        }
        recordButton.addEventListener(
            'AudioControls.RecordingStarted',
            (event) => {
                //console.log('entering AudioControls.RecordingStarted')
                    let button = $('#recordButton')
                    let nextButton = $('#nextButton');
                    let recordingStatus = document.getElementById('recordingStatus')

                    let clientStartTS = new Date(Date.now()).toLocaleString()
                    document.getElementById("clientStartTS").value = clientStartTS;
                    nextButton.attr('hidden', true)
                    let text = button.text()
                    if (button.text() === 'Stop Recording') {
                        return false
                    }
                    event.stopPropagation()
                    event.preventDefault()
                    recordingStatus.innerHTML = "Recording started";
                    button.text('Stop Recording')  
                    button.attr('hidden', true)
                    var recordingStartedDiv = document.getElementById('recordingStartedDiv');
                    recordingStartedDiv.style.display = "block";
                    delay(1000).then(() => 
                        button.removeAttr('hidden')).then(()=> recordingStartedDiv.style.display="none").finally(() => button.focus())
                //console.log('exiting AudioControls.RecordingStarted')
            }
        )
        recordButton.addEventListener(
            'AudioControls.RecordingStopped',
            (event) => {

                try {
                   
                    
                    //console.log('entering AudioControls.RecordingStopped')
                    let button = $('#recordButton')
                    let nextButton = $('#nextButton');
                    let recordingStatus = document.getElementById('recordingStatus')
                    let text = button.text()
                    if (button.text() === 'Record' || button.text === 'Rerecord') {
                        return false
                    }
                    recordingStatus.innerHTML = "Recording Stopped"
                    event.stopPropagation()
                    event.preventDefault()
               

                    let clientStartDate = document.getElementById("clientStartTS").value;
                    let clientEndDate = new Date(Date.now()).toLocaleString()
                    //send the recording to the controller to be saved
                    var contributorId = document.getElementById('contributorId').value;
                    var promptId = document.getElementById('promptId').value;
                    var categoryId = document.getElementById('categoryId').value;
                    var blockId = document.getElementById('blockId').value;
                    let rerecordMessage = document.getElementById('rerecordMessage')
                    var subCategoryId = document.getElementById('subCategoryId').value;

                    if (subCategoryId == 5) {
                        promptId = document.querySelector('input[name="selectedPromptId"]:checked').value;
                    }

                    retryCount++;

                    let filename = contributorId + "_" + promptId + "_" + blockId;

                    ////
                    //// codec is the string returned from isBrowserSupported().
                    //// It probably should return "webm" for now but I don't 
                    //// really know what other browsers might need.
                    ////

                    filename += `.wav`

                    ////
                    //// create the file upload element
                    ////

                    let form = document.querySelector('#postRecording');
                    let myData = new FormData(form);

                    myData.set(
                        'file',
                        event.detail.wavData,
                        filename
                    )

                    myData.set('filename', filename)

                    myData.set('contributorId', contributorId);
                    myData.set('promptId', promptId);
                    myData.set('categoryId', categoryId);
                    myData.set('retryCount', retryCount);
                    myData.set('blockId', blockId);
                    myData.set('clientStartDate', clientStartDate);
                    myData.set('clientEndDate', clientEndDate);
                    let my_url = Cookies.get('url')

                    ////
                    //// send the recorded audio
                    ////
                    //// note the 'cache', 'contentType', and 'processData' jQuery values.
                    //// these appear to be necessary when sending binary data.
                    ////
         
                    processRecording(my_url, myData, retryCount, categoryId, subCategoryId, button, rerecordMessage, nextButton);

                    var etiologyId = document.getElementById("etiologyId").value;

                    if (etiologyId == '2') {

                        var snd = new Audio("..//recordings/Recording Stopped.wav");
                        snd.play();
                                             
                    }

                   
                }

                catch (e) {
                    console.log(e)
                    let myData = new FormData()
                    
                    var errorMessage = 'RecordingStopped error: ' + e

                    if (event.detail !== null && event.detail.wavData !== null) {
                        errorMessage += ".Wav file size: " + event.detail.wavData.size
                    }
                    else {

                            errorMessage += ". wav file is null."
                    }
                    myData.set(
                        'error',  errorMessage
                    )
                    $.ajax({
                        url: '/Home/LogError',
                        type: 'POST',
                        cache: false,
                        contentType: false,
                        processData: false,
                        data: myData,
                    })
                    var categoryId = document.getElementById('categoryId').value;

                    showErrorMessage(retryCount, categoryId);
                    let button = $('#recordButton');
                    button.text('Record')
                    button.removeAttr('hidden')                    
                    saveDiv.style.display = "none";

                }

               

                myAudioControls = new AudioControls(codec,
                    'recordButton',
                    undefined,
                    'waveform', "black", "white", 180000);

                //console.log('exiting AudioControls.RecordingStarted')
            }
        )     
        recordButton.addEventListener(
            'AudioControls.RecordingTimeExceeded',
            (event) => {               
                document.getElementById('maxTimeDialog').showModal();
            })

        document.addEventListener('AudioControls.Error',
            (event) => {
                var errorDiv = document.getElementById('errorDiv');

                var recordError = false;
                if ('Permission denied' == event.detail || (event.detail != null && event.detail.includes('The request is not allowed by the user agent or the platform in the current context')))
                {                  
                    errorDiv.innerHTML = 'Microphone is blocked. Please allow the microphone access in your browser settings and reload the page.'
                }
                else if ('Requested device not found' == event.detail || 'The object can not be found here.' == event.detail)
                {
                    errorDiv.innerHTML = 'No audio input devices found. Please attach a microphone and reload this page.'
                }
                else if (event.detail != null && event.detail.includes('There was an error starting the MediaRecorder'))
                {
                    recordError = true;
                }
                else {
                    errorDiv.innerHTML = 'Error: ' + event.detail + '<br/> You may contact speechaccessibility@beckman.illinois.edu for assistance.'
                }
                if (!recordError)
                {
                    var containerDiv = document.getElementById("container");
                    containerDiv.hidden = true;

                    const dialog = document.querySelector('dialog[open]')
                    if (dialog != null) {
                        dialog.close();
                    } 
                }               

                let myData = new FormData()
                var contributorId = document.getElementById('contributorId').value;
                var errorMessage = "";
                if (contributorId != null) {
                   errorMessage= contributorId + " ";
                } 
                errorMessage += event.detail;
                myData.set(
                    'error', errorMessage
                )
                $.ajax({
                    url: '/Home/LogError',
                    type: 'POST',
                    cache: false,
                    contentType: false,
                    processData: false,
                    data: myData,
                })

            })

        let codec = isBrowserSupported();

        let myAudioControls = new AudioControls(codec,
            'recordButton',
            undefined,
            'waveform', "black", "white", 180000);

        const other_thing = 4;

    } catch (e) {
        throw `permissions.query: ${e}`;
    }
}



function delay(time) {
    return new Promise(resolve => setTimeout(resolve, time));
}

async function processRecording(my_url, myData, retryCount, categoryId, subCategoryId, button, rerecordMessage, nextButton) {

    try {
    var saveDiv = document.getElementById('saveDiv');
    saveDiv.style.display = "block";
    button.attr('hidden', true)
    nextButton.attr('hidden', true)
    await postRecording(my_url, myData,retryCount,categoryId);

    updateButtons(retryCount, categoryId, subCategoryId, button, rerecordMessage, nextButton,saveDiv);
    } catch (e) {
        let myData = new FormData()

        myData.set(
            'error', "processRecording error: " + e.stack
        )
        $.ajax({
            url: '/Home/LogError',
            type: 'POST',
            cache: false,
            data: myData,
        })

        let button = $('#recordButton');
        button.text('Record')
        button.removeAttr('hidden')
        saveDiv.style.display = "none";

    }
    }

   async function postRecording(my_url,myData,retryCount,categoryId) {
    let result;

        result = await $.ajax({
            url: my_url,
            type: 'POST',
            cache: false,
            contentType: false,
            processData: false,
            data: myData,
        }).fail(function () {
            showErrorMessage(retryCount, categoryId);

        }).done(function () { return result; })

        return result;
    
}

function setVoice(synth, prompt, secondaryOption, selectedOption) {
    var voiceSelect = document.getElementById('voiceSelect');
    var foundSelectedOption = false;

    for (var i = 0; i < voiceSelect.length; i++) {
        if (voiceSelect.options[i].getAttribute("data-name") == selectedOption) {
            var preferredVoice = synth.getVoices().filter(voice => voice.name === selectedOption);
            console.log("preferredVoice: " + preferredVoice[0].name);
            prompt.voice = preferredVoice[0];
            foundSelectedOption = true;
            break;
        }
    }

    if (!foundSelectedOption && secondaryOption != "") {

        for (var i = 0; i < voiceSelect.length; i++) {
            if (voiceSelect.options[i].getAttribute("data-name") == secondaryOption) {
                var preferredVoice = synth.getVoices().filter(voice => voice.name === secondaryOption);
                console.log("preferredVoice: " + preferredVoice[0].name);
                prompt.voice = preferredVoice[0];
                break;
            }
        }

    }
}

function selectVoice() {

    var selectedOption = "";

    var os = getOS();
    // Get the user-agent string
    var browser = getBrowser();

    if (os == "Windows") {
        if (browser == "Edge") {
                selectedOption = "Microsoft Ava Online (Natural) - English (United States)";

        }
        else if (browser == "Firefox") {

            selectedOption = "Microsoft Zira Desktop - English (United States)";

        }
        else {
                selectedOption = "Google US English"
            }

    }
    else if (os == "Mac OS" || os == "iOS") {
        selectedOption = "Samantha";
    }
    return selectedOption;
}



function showErrorMessage(retryCount, categoryId) {
    var dialog = document.getElementById("saveRecordingError");
    var errorText = document.getElementById("saveRecordingText");
    if (retryCount < 3 && categoryId != 1) {
        errorText.innerHTML = "Please try to rerecord. If this error persists, please contact speechaccessibility@beckman.illinois.edu.";
    }
    else {
        errorText.innerHTML = "If this error persists, please contact speechaccessibility@beckman.illinois.edu.";
    }
    dialog.showModal();
}

function updateButtons(retryCount, categoryId, subCategoryId, button, rerecordMessage, nextButton,saveDiv) {

    button.removeAttr('hidden')
    nextButton.removeAttr('hidden');      
    nextButton.focus();
    saveDiv.style.display = "none";

    if (retryCount < 3 && categoryId != 1 && subCategoryId != 4 && subCategoryId != 5) {

        button.text('Rerecord');

        if (retryCount == 1) {
            rerecordMessage.innerText = 'You have 2 rerecord attempts left';
        }
        else if (retryCount == 2) {
            rerecordMessage.innerText = 'You have 1 rerecord attempt left';
        }

    }
    else {
        button.hide();
        rerecordMessage.innerText = '';       

    }   
    
}

function isBrowserSupported() {
    let supportedCodec = [];
    Cookies.set('codec', '', { 'SameSite': 'lax' });

    let codecs = [
        'audio/webm; codecs=pcm',
        'audio/webm; codecs=opus',
        'audio/webm;',
        'audio/ogg; codecs=opus',
        'audio/ogg;',
        'audio/3gpp;',
        'audio/3gpp2;',
        'audio/mp4;',
        'audio/vnd.apple.mpegurl;',
    ]

    codecs.forEach(
        (codec) => {
            let result = MediaRecorder.isTypeSupported(codec)
            if (result) {
                supportedCodec.push(codec)

                Cookies.set('codec', codec, { 'SameSite': 'lax' })
            }
        }
    )

    if (supportedCodec.length === 0) {
        Cookies.set('codec', 'No supported mime type or codec found', { 'SameSite': 'lax' })
        throw new Error('No supported mime type or codec found.')
    }

    return supportedCodec[0]
}


function checkifRecordingSupported(base) {
    return new Promise((resolve, reject) => {
        navigator.mediaDevices.enumerateDevices()
            .then((devices) => {
                try {
                    if (!devices) {
                        console.dir(devices);
                    }

                    devices = devices.filter((d) => d.kind === 'audioinput');

                    if (!devices.length) {
                        base.prepend(
                            '<h5>No audio input devices found.</h5>'
                            + '<h5>Please attach a microphone and reload this page.</h5>'
                        );

                        return;
                    }

                    var supportsRecoring = false;

                    [
                        'audio/ogg; codecs=opus',
                        'audio/webm; codecs=opus',
                        'audio/webm; codecs=pcm'
                    ].forEach(
                        (codec) => {
                            let result = MediaRecorder.isTypeSupported(codec)
                            if (result) {
                                supportsRecoring = true;
                            }
                        }
                    )

                    if (!supportsRecoring)
                    {
                        base.prepend(
                            '<h5>This browser does not support recording.</h5>'
                            + '<h5>Please try a different browser.</h5>'
                        );
                    }
                } catch (e) {
                    throw `mediaDevices.enumerateDevices: ${e}`;
                }
            })
            .catch((reason) => {
                throw reason;
            });
    });
}