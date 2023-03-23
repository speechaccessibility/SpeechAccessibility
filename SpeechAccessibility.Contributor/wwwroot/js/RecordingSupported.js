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

$(function () {
    const messageDiv = $('#messageDiv');
    messageDiv.hide();

    try {

        isThereAMicrophone(messageDiv)

        var browserResult = isBrowserSupported()

        if (browserResult == "") {
            $('#messageDiv').attr('hidden',true)
            $('#errorDiv').prepend('<h1>This browser does not support recording. Please try opening this page in a different browser to continue.</h1>')
            $('#recordButton').attr('hidden',true)
        }

    } catch (e) {
        console.log(`main: ${e}`);
    }
});

 function isBrowserSupported() {
    let supportedCodec = [];

    [
        'audio/webm; codecs=opus',
        'audio/webm; codecs=pcm',
        'audio/webm;',
        'audio/ogg; codecs=opus',
        'audio/ogg;',
        'audio/3gpp;',
        'audio/3gpp2;',
        'audio/mp4;',
        'audio/vnd.apple.mpegurl;',

    ].forEach(
        (codec) => {
            let result = MediaRecorder.isTypeSupported(codec)
            if (result) {
                supportedCodec.push(codec)
            }
        }
    )

    return supportedCodec.length
        ? supportedCodec[0]
        : ""
}


function isThereAMicrophone(base) {
    return new Promise(
        () => {
            navigator.mediaDevices.enumerateDevices()
                .then(
                    (devices) => {
                        try {
                            if (!devices) {
                                throw new Error("No Audio Devices found.");
                            }

                            devices = devices.filter((d) => d.kind === 'audioinput');

                            if (!devices.length) {
                                throw new Error("No audio input devices found.");
                            }
                        } catch (e) {
                            throw new Error(`mediaDevices.enumerateDevices: ${e}`);
                        }
                    }
                )
                .then(response => {

                    //messageDiv.prepend('Thank you for your interest in the Speech Accessibility Project.'
                    //    + ' After registering, you will be asked to record 22 screening prompts.'
                    //    + ' Once you have completed recording all 22 prompts, someone will review'
                    //    + ' the recordings to determine your eligibilty for this project.'
                    //    + ' You will receive an email within 5 business days to inform you if you are eligible'
                    //    + ' to participate.')
                    $('#messageDiv').show()

                }).catch(e => {
                    console.log(e);
                    $('#errorDiv').prepend('<h1>No audio input devices found. An audio device is required to participate in this study. Please attach a microphone and reload this page.</h1>')
                    $('#recordButton').attr('hidden', true)

                });

        }
    );

}


