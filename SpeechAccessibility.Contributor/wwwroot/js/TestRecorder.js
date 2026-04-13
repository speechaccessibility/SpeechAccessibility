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


    checkifRecordingSupported()
     recording_interface()
    
});


function recording_interface() {

    let recordButton = document.getElementById('recordButton')

        recordButton.addEventListener(
            'AudioControls.RecordingStarted',
            (event) => {
                //console.log('entering AudioControls.RecordingStarted')
                let button = $('#recordButton')

                let text = button.text()
                if (button.text() === 'Stop Recording') {
                    return false
                }
                event.stopPropagation()
                event.preventDefault()
                button.text('Stop Recording')
            }
        )
        recordButton.addEventListener(
            'AudioControls.RecordingStopped',
            (event) => {

                try {

                    let button = $('#recordButton')
                    let text = button.text()
                    if (button.text() === 'Record') {
                        return false
                    }
                    button.text('Start Recording')
                    event.stopPropagation()
                    event.preventDefault()

                }

                catch (e) {
                    console.log(e)
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
                if ('Permission denied' == event.detail || (event.detail != null && event.detail.includes('The request is not allowed by the user agent or the platform in the current context'))) {
                    errorDiv.innerHTML = 'Microphone is blocked. Please allow the microphone access in your browser settings and reload the page.'
                }
                else if ('Requested device not found' == event.detail || 'The object can not be found here.' == event.detail) {
                    errorDiv.innerHTML = 'No audio input devices found. Please attach a microphone and reload this page.'
                }
                else if (event.detail != null && event.detail.includes('There was an error starting the MediaRecorder')) {
                    recordError = true;
                }
                else {
                    errorDiv.innerHTML = 'Error: ' + event.detail + '<br/> You may contact speechaccessibility@beckman.illinois.edu for assistance.'
                }
                if (!recordError) {
                    var containerDiv = document.getElementById("container");
                    containerDiv.hidden = true;

                    const dialog = document.querySelector('dialog[open]')
                    if (dialog != null) {
                        dialog.close();
                    }
                }
            })

        let codec = isBrowserSupported();

        let myAudioControls = new AudioControls(codec,
            'recordButton',
            undefined,
            'waveform', "black", "white", 180000);

        const other_thing = 4;

    }

    function updateButtons(retryCount, categoryId, subCategoryId, button, rerecordMessage, nextButton, saveDiv) {

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
                  
                }
            }
        )

        if (supportedCodec.length === 0) {
      
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

                        if (!supportsRecoring) {
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
