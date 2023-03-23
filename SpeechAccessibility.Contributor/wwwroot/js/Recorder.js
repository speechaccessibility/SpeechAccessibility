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
import { AudioControls } from "./AudioControls.js";

const base = $('#consentDiv');


$(function () {
    try {

    checkifRecordingSupported()
    $('#recordingDiv').show();
     recording_interface()

    } catch (e) {
        console.log(`main: ${e}`);
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
                let button = $('#recordButton')
                let nextButton = $('#nextButton');
                let recordingStatus = document.getElementById('recordingStatus')
                nextButton.attr('hidden', true)
                let text = button.text()
                if (button.text() === 'Stop Recording') {
                    return false
                }
                event.stopPropagation()
                event.preventDefault()
                recordingStatus.innerHTML = "Recording started";
                button.text('Stop Recording')
            }
        )
        recordButton.addEventListener(
            'AudioControls.RecordingStopped',
            (event) => {
                let button = $('#recordButton')
                let nextButton = $('#nextButton');
                let recordingStatus = document.getElementById('recordingStatus')
                let text = button.text()
                if (button.text() === 'Record' || button.text==='Rerecord') {
                    return false
                }
                recordingStatus.innerHTML = "Recording Stopped"
                event.stopPropagation()
                event.preventDefault()         

                //send the recording to the controller to be saved
                var contributorId = document.getElementById('contributorId').value;
                var promptId = document.getElementById('promptId').value;
                var categoryId = document.getElementById('categoryId').value;
                var blockId = document.getElementById('blockId').value;
                let rerecordMessage = document.getElementById('rerecordMessage')
                var subCategoryId = document.getElementById('subCategoryId').value;

                if (subCategoryId == 5)
                {
                    promptId = document.querySelector('input[name="selectedPromptId"]:checked').value;
                }

                retryCount++;
            
                let filename = contributorId + "_" + promptId + "_" + blockId;
                let phonationPromptCount = Cookies.get('phonationPromptCount');

                //They will record the phonation prompt three times
                //Adding the count at the end of the file name to distinguish them from each other
                if (subCategoryId =='4')
                 {
                     filename+="_"+phonationPromptCount;
                 }
                ////
                //// codec is the string returned from isBrowserSupported().
                //// It probably should return "webm" for now but I don't 
                //// really know what other browsers might need.
                ////
                let fileType = codec.split(';')[0].split('/')[1]
                filename += `.${fileType}`

                ////
                //// create the file upload element
                ////
                let myData = new FormData()

                myData.set(
                    'file',
                    myAudioControls.getAudioBlob(),
                    filename
                )
                myData.set('filename', filename)

                myData.set('contributorId', contributorId);
                myData.set('promptId', promptId);
                myData.set('categoryId', categoryId);
                myData.set('retryCount', retryCount);
                myData.set('blockId', blockId);

                let my_url = Cookies.get('url')

                ////
                //// send the recorded audio
                ////
                //// note the 'cache', 'contentType', and 'processData' jQuery values.
                //// these appear to be necessary when sending binary data.
                ////
                $.ajax({
                    url: my_url,
                    type: 'POST',
                    cache: false,
                    contentType: false,
                    processData: false,
                    data: myData,
                    success: updateButtons(retryCount, categoryId, subCategoryId, button, rerecordMessage, nextButton),
                }
                )

                myAudioControls = new AudioControls(codec,
                    'recordButton',
                    undefined,
                    'playButton',
                    undefined,
                    'waveform'
                );

            }
        )     
        recordButton.addEventListener(
            'AudioControls.RecordingTimeExceeded',
            (event) => {
                //alert('Recording has met the max time limit. You may proceed to the next prompt. ');
                document.getElementById('maxTimeDialog').showModal();
            })

        let codec = isBrowserSupported();

        let myAudioControls = new AudioControls(codec,
            'recordButton',
            undefined,
            'playButton',
            undefined,
            'waveform'
        );

        const other_thing = 4;
    } catch (e) {
        throw `permissions.query: ${e}`;
    }
}

function updateButtons(retryCount, categoryId, subCategoryId, button, rerecordMessage, nextButton) {

    nextButton.removeAttr('hidden');      
    nextButton.focus();

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