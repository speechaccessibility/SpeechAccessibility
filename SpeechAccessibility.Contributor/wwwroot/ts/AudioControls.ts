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

import * as buffer from "buffer";

export class AudioControls {
    private _chunks: Blob[] = [];
    private _mediaRecorder?: MediaRecorder;
    private _stream?: MediaStream;
    public unsavedAudio: Blob = new Blob();

    // constructor params
    private readonly _codec: string = undefined
    private readonly _playStartButtonId: string;
    private readonly _playStopButtonId: string;
    private readonly _recordStartButtonId: string;
    private readonly _recordStopButtonId: string;

    private readonly _waveformCanvasId: string;
    private readonly _waveformBackgroundCSS: string = 'black'
    private readonly _waveformForegroundCSS: string = 'white'

    private readonly _maxRecordingTimeMsec: number = 120000
    private readonly _recordingSampleRateMsec: number = 250

    availableInputDevices: MediaDeviceInfo[] = [];

    private _recordingTimeExceededId: NodeJS.Timeout;
    private _isRecording: boolean = false
    private _isPlaying: boolean = false

    /*
     * The display audio functions will "scale" the audio a mic pics up to "fit"
     * the size of the display area.  This means that initial mic sounds and
     * "quiet" background noise can show up in the display as possibly large
     *  peaks.  audio_display_threshold defines a +/- range of values that
     *  will be visually ignored.  This does not modify the audio data. It
     *  only modifies the data used to display a "waveform".
     *
     * "audio_display_threshold" is a floating point number that will "zero out"
     * audio values that are between negative(audio_display_threshold) and
     * positive(audio_display_threshold).  Note, this is ONLY for the
     * display values. Actual recorded audio is unchanged.
     */
    private audio_display_threshold: number = 0.005

    /*
     * Only display the last audio_display_value_length amount of data in
     * the visual display area.
     */
    private audio_display_value_length: number = 1024 * 16 // 16k

    //
    // Events
    public eventStartedRecording = new CustomEvent(
        'AudioControls.RecordingStarted'
    )
    public eventStopRecording = new CustomEvent(
        'AudioControls.RecordingStopped'
    )
    public eventStartPlaying = new CustomEvent(
        'AudioControls.PlayingStarted'
    )
    public eventStopPlaying = new CustomEvent(
        'AudioControls.PlayingStopped'
    )
    public recordingTimeExceeded = new CustomEvent(
        'AudioControls.RecordingTimeExceeded'
    )

    /**
     *  Setup basic audio recording in the browser.
     *
     *  @constructor
     *  @param {string} codec - required - A string representing the mime
     *                  type and codec associated with the browser.
     *  @param {string} recordStartButtonId - required - DOM Element ID of the
     *                  Recording Start button.
     *  @param {string?} recordStopButtonId - optional - DOM Element ID of the
     *                  Recording Stop button. If not defined it will use the
     *                  recordStartButtonId value.
     *  @param {string} playStartButtonId - required - DOM Element ID of the
     *                  Play Start button. If not defined playback using
     *                  this module will be disabled.
     *  @param {string?} playStopButtonId - optional - DOM Element ID of the
     *                  Play Stop button. If not defined it will use the
     *                  recordStartButtonId value.
     *  @param {string?} waveformCanvasId - optional - DOM Element ID of the
     *                  HTML canvas used for the graphical display of the
     *                  recorded audio waveform. If not defined waveform
     *                  rendering will be disabled.
     *  @param {string?} waveformBackgroundCSS - optional - A valid CSS color
     *                  string value for the background: "rgba(0,0,0,1)",
     *                  "black", etc. Default is "black".
     *  @param {string?} waveformForegroundCSS - optional - A valid CSS color
     *                  string value for the foreground color: "rgba(0,0,0,1)",
     *                  "black", etc. Default is "white".
     *  @param {number?} maxRecordingTimeMsec - optional - The maximum
     *                  number of milliseconds to record before forcing a
     *                  stop and raising a stop event. A value of zero
     *                  means unlimited recording time -- which often means
     *                  that a browser tab can run out of memory, FYI. Default
     *                  is 1 minute = 1000*60 = 60000.
     *  @param {number?} recordingSampleRateMsec - optional - The number of
     *                  milliseconds between recording data "saves". This
     *                  influences how frequently the waveform display is
     *                  redrawn. Default is 100 milliseconds.
     *  @param {number?} audio_display_threshold - optional - Default = 0.005. A
     *                  floating point value less than 1. This only effects
     *                  visual display data. Display data have values
     *                  between -1.0 and 1.0. A microphone may pick up very
     *                  quiet sounds that may show up in the display. Values
     *                  between negative(audio_display_threshold) and
     *                  positive(audio_display_threshold) will be set to zero.
     *  @param {number?} audio_display_value_length - optional - Default =
     *                  16K. The maximum amount of audio data to display during
     *                  each render cycle. Trying to display all the data
     *                  each time quickly becomes impossible. The data
     *                  selected for display is the last/most recent
     *                  audio_display_value_length amount of data per render
     *                  cycle.
     *
     * @throws NoAudioInputs - if there are no audio inputs found.
     */
    constructor(codec: string,
        recordStartButtonId: string | undefined,
        recordStopButtonId: string | undefined = undefined,
        playStartButtonId: string | undefined = undefined,
        playStopButtonId: string | undefined = undefined,
        waveformCanvasId: string | undefined = undefined,
        waveformBackgroundCSS: string = "black",
        waveformForegroundCSS: string = "white",
        maxRecordingTimeMsec: number = 120000,
        recordingSampleRateMsec: number = 100,
        audio_display_threshold: number = 0.005,
        audio_display_value_length: number = 1024 * 16) {
        // codec is REQUIRED
        if (!(codec && codec.trim())) {
            throw new Error(`recordStartButtonId is required.`)
        }
        if (typeof codec != 'string') {
            throw new Error(`codec should be a string.`)
        }
        this._codec = codec.trim()

        // RECORD START/STOP BUTTONS
        // recordStartButtonId is REQUIRED
        if (!(recordStartButtonId && recordStartButtonId.trim())) {
            throw new Error(`recordStartButtonId is required.`)
        }
        if (typeof recordStartButtonId != 'string') {
            throw new Error(`recordStartButtonId should be a string.`)
        }
        recordStartButtonId = recordStartButtonId.trim()
        if (!document.getElementById(recordStartButtonId)) {
            throw new Error(
                `recordStartButtonId value "${recordStartButtonId}" does not `
                + `exist in the DOM.`
            )
        }
        this._recordStartButtonId = recordStartButtonId
        // if recordStopButtonId is undefined make this.recordStopButtonId
        // the same as this.recordStartButtonId
        if (!(recordStopButtonId && recordStopButtonId.trim())) {
            this._recordStopButtonId = this._recordStartButtonId
        } else {
            if (typeof recordStopButtonId != 'string') {
                throw new Error(
                    `recordStopButtonId should be a string or undefined.`
                )
            }
            recordStopButtonId = recordStopButtonId.trim()
            if (!document.getElementById(recordStopButtonId)) {
                throw new Error(
                    `recordStopButtonId value "${recordStopButtonId}" does not `
                    + `exist in the DOM.`
                )
            }
            this._recordStopButtonId = recordStopButtonId
        }

        // PLAYBACK BUTTONS
        if (!(playStartButtonId && playStartButtonId.trim())) {
            // no start button ID so no stop button ID
            this._playStartButtonId = undefined
            this._playStopButtonId = undefined
        } else {
            // there is a start button ID
            if (typeof playStartButtonId != 'string') {
                throw new Error(`playStartButtonId should be a string.`)
            }
            playStartButtonId = playStartButtonId.trim()
            if (!document.getElementById(playStartButtonId)) {
                throw new Error(
                    `playStartButtonId value "${playStartButtonId}" does not `
                    + `exist in the DOM.`
                )
            }
            this._playStartButtonId = playStartButtonId
            if (!(playStopButtonId && playStopButtonId.trim())) {
                // no stop button
                this._playStopButtonId = this._playStartButtonId
            } else {
                if (typeof playStopButtonId != 'string') {
                    throw new Error(`playStopButtonId should be a string.`)
                }
                playStopButtonId = playStopButtonId.trim()
                if (!document.getElementById(playStopButtonId)) {
                    throw new Error(
                        `playStopButtonId value "${playStopButtonId}" does not `
                        + `exist in the DOM.`
                    )
                }
                this._playStopButtonId = playStopButtonId
            }
        }

        // WAVEFORM CANVAS
        if (!(waveformCanvasId && waveformCanvasId.trim())) {
            // no start button ID so no stop button ID
            this._waveformCanvasId = undefined
        } else {
            if (typeof waveformCanvasId != 'string') {
                throw new Error(`waveformCanvasId should be a string.`)
            }
            waveformCanvasId = waveformCanvasId.trim()
            if (!document.getElementById(waveformCanvasId)) {
                throw new Error(
                    `waveformCanvasId value "${waveformCanvasId}" does not `
                    + `exist in the DOM.`
                )
            }
            this._waveformCanvasId = waveformCanvasId

            if (waveformBackgroundCSS) {
                if (typeof waveformBackgroundCSS != 'string') {
                    throw new Error(`waveformBackgroundCSS should be a string.`)
                }
                this._waveformBackgroundCSS = waveformBackgroundCSS.trim()
            }
            if (waveformForegroundCSS) {
                if (typeof waveformForegroundCSS != 'string') {
                    throw new Error(`waveformForegroundCSS should be a string.`)
                }
                this._waveformForegroundCSS = waveformForegroundCSS.trim()
            }
        }

        // MAX RECORDING TIME
        if (typeof maxRecordingTimeMsec != 'number') {
            throw new Error(
                `maxRecordingTimeMsec should be a number in `
                + `milliseconds.`
            )
        }
        this._maxRecordingTimeMsec = Math.trunc(maxRecordingTimeMsec)

        if (typeof recordingSampleRateMsec != 'number') {
            throw new Error(
                `recordingSampleRateMsec should be a number in `
                + `milliseconds.`
            )
        }
        if (recordingSampleRateMsec) {
            this._recordingSampleRateMsec = Math.trunc(recordingSampleRateMsec)
            if (this._recordingSampleRateMsec === 0) {
                throw new Error("Recording Sample Rate value is zero.")
            }
        }

        // AUDIO DISPLAY THRESHOLD
        if (typeof audio_display_threshold != 'number') {
            throw new Error(
                `audio_display_threshold should be a floating point number.`
            )
        }
        if (audio_display_threshold > 1.0 || audio_display_threshold < -1.0) {
            throw new Error(
                `audio_display_threshold only has a useful range between -1.0 `
                + `and 1.0. Default is 0.005.`
            )
        }
        this.audio_display_threshold = audio_display_threshold

        // AUDIO DISPLAY VALUE LENGTH
        if (typeof audio_display_value_length != 'number') {
            throw new Error(
                `audio_display_value_length should be an integer > 0.`
            )
        }
        if (audio_display_value_length < 1) {
            throw new Error(
                `audio_display_value_length only has a useful integer range > 0.`
            )
        }
        this.audio_display_value_length = Math.trunc(audio_display_value_length)

        //
        // get list of available input devices. The "list" is probably not
        // useful but the number of potential devices is.
        navigator.mediaDevices.enumerateDevices().then(devices => {
            this.availableInputDevices = devices.filter(
                (d) => d.kind === 'audioinput'
            )

            if (!this.availableInputDevices.length) {
                throw new NoAudioInputs("No input devices found.")
            }
        })

    
        this.initializeRecording().catch(
            error => {
                $('#errorDiv').append(
                    `<p>No audio input devices found. An audio device is required to participate in this study. Please attach a microphone and reload this page.</p>`
                )
                throw new NoAudioInputs(error);
            }
        );
    }

    /**
     * If there is at least one input device potentially signal the user to
     * grant access.
     *
     * @throws Exceptions are possible from navigator.mediaDevices.getUserMedia
     */
    private async initializeRecording() {
        //
        // possibly request permission to use an input device.
        await navigator.mediaDevices.getUserMedia(
            {
                audio: true,
                video: false
            }
        ).then(
            stream => {
                if (!stream) {
                    throw new Error('No MediaStream "stream" found.');
                }
                this._stream = stream;

                //
                // setup primary event listeners
                const recordStartButton = document.getElementById(
                    this._recordStartButtonId
                )
                recordStartButton.addEventListener(
                    "click",
                    () => {
                        if (this._isRecording) {
                            return false
                        }
                        this.startRecording().catch(
                            (error) => {
                                $('div#errors').append(
                                    `<p>AudioControls: startRecording: ${error}</p>`
                                )
                                throw new Error(
                                    `startRecording error: ${error}`
                                )
                            }
                        )
                    }
                )

                const recordStopButton = document.getElementById(
                    this._recordStopButtonId
                )
                recordStopButton.addEventListener(
                    "click",
                    () => {
                        if (!this._isRecording) {
                            return false
                        }
                        this._mediaRecorder.stop();
                    }
                )

                const playStartButton = document.getElementById(
                    this._playStartButtonId
                )
                playStartButton.addEventListener(
                    "click",
                    () => {
                        this.playRecording();
                    }
                )
            }
        ).catch(
            error => {
                if ("Permission denied" == error.message) {
                    $('#errorDiv').append(
                        `<p>Microphone access is blocked. Please allow access to the microphone in your browser settings and reload this page.</p>`
                    )
                }
                else
                {
                    $('div#errors').append(
                        `<p>AudioControls: mediaDevices.getUserMedia: ${error}</p>`
                    )

                    throw new Error(error);
                }
              
            }
        );
    }

    /**
     * StartRecording sets up a MediaRecorder instance to begin recording.
     * If this is called with existing recorded data the existing data is
     * ERASED before re-starting MediaRecorder.
     *
     * This sets up event handlers for MediaRecorder 'dataavailable' and
     * 'stop' events and issues the MediaRecorder 'start()' routine.
     *
     * @DOM_events AudioControls.RecordingStarted - when the first chunk of
     *  audio data is available.
     *
     * @returns {Promise}
     */
    private async startRecording(): Promise<unknown> {
        return new Promise(
            () => {
                this._chunks = [];

                if (!this._codec) {
                    throw new Error("Codec was not specified.")
                }

                this._mediaRecorder = new MediaRecorder(
                    this._stream,
                    {
                        mimeType: this._codec
                    }
                );
                this._mediaRecorder.addEventListener(
                    'dataavailable',
                    (audioData) => {
                        if (this._chunks.length == 0) {
                            // send the start recording event to
                            // the start button
                            this._isRecording = true
                            document.getElementById(
                                this._recordStartButtonId
                            ).dispatchEvent(
                                this.eventStartedRecording
                            )

                            // set a max recording timeout
                            if (this._maxRecordingTimeMsec > 0) {
                                this._recordingTimeExceededId = setTimeout(
                                    () => {
                                        this._mediaRecorder.stop()

                                        if (this._maxRecordingTimeMsec) {
                                            document.getElementById(
                                                this._recordStopButtonId
                                            ).dispatchEvent(
                                                this.recordingTimeExceeded
                                            )
                                        }
                                    },
                                    this._maxRecordingTimeMsec
                                )
                            }
                        }

                        this._chunks.push(audioData.data);

                        if (this._waveformCanvasId) {
                            this.decodeAndDisplayAudio();
                        }
                    }
                );
                this._mediaRecorder.addEventListener(
                    'stop',
                    () => {
                        this.onStop()
                    }
                );

                //
                // start recording here
                this._mediaRecorder.start(this._recordingSampleRateMsec);
            }
        );
    }

    /**
     * onStop - Handle the MediaRecorder 'stop' event.
     *
     * @DOM_Events 'AudioControls.RecordingStopped': when playing has finished.
     */
    private onStop() {
        if (!this._isRecording) {
            throw new Error('Stop Recording called when Start Recording was'
                + ' not active.')
        }

        //
        // stop any active tracks. Count the audio tracks in case
        // there are also video tracks.  We only care about audio.

        //
        // How many audio tracks are there? Should be at least one.
        this._stream.getTracks().filter(
            (d) => d.kind === 'audio'
        ).forEach(
            (track) => {
                track.addEventListener(
                    'stop',
                    () => {
                        let trackLabel = track.label
                        // console.log(`Track "${track.label}" is stopped.`)
                    }
                )
                track.stop()
            }
        )

        //
        // this.unsavedAudio is the raw, un-submitted audio data.
        if (this._chunks.length == 0) {
            throw new Error('There are no audio chunks.')
        }
        this.unsavedAudio = new Blob(
            this._chunks,
            {
                'type': this._codec
            }
        );

        this._isRecording = false

        clearTimeout(this._recordingTimeExceededId)
        this._recordingTimeExceededId = undefined

        // send the stop recording event to
        // the stop button
        document.getElementById(
            this._recordStopButtonId
        ).dispatchEvent(
            this.eventStopRecording
        )
    }

    /*
     * Play the saved audio through an AudioContext.
     *
     * @DOM_Events 'AudioControls.StartedPlaying': when playing starts.
     * @DOM_Events 'AudioControls.FinishedPlaying': when playing has finished.
     */
    private async playRecording() {
        try {
            if (!(this.unsavedAudio || this.unsavedAudio.size === 0)) {
                throw new Error(
                    'Received request to play but there was no saved audio.')
            }
            if (this._isPlaying) {
                return
            }

            const audioCtx = new AudioContext();

            new Blob(
                this._chunks,
                { 'type': this._codec }
            ).arrayBuffer().then(
                buffer => {
                    audioCtx.decodeAudioData(
                        buffer
                    ).then(
                        buf => {
                            const source = audioCtx.createBufferSource();
                            source.buffer = buf;
                            source.connect(audioCtx.destination);
                            source.addEventListener(
                                'ended',
                                () => {
                                    document.getElementById(
                                        this._playStopButtonId
                                    ).dispatchEvent(
                                        this.eventStopPlaying
                                    )
                                }
                            );

                            document.getElementById(
                                this._playStopButtonId
                            ).dispatchEvent(
                                this.eventStartPlaying
                            )
                            this._isPlaying = true

                            //
                            // start playback
                            source.start(0);

                            this._isPlaying = false
                        }
                    ).catch(
                        (error) => {
                            $('div#errors').append(
                                `<p>AudioControls: playRecording decodeAudioData: ${error}</p>`
                            )
                            throw new Error(error)
                        }
                    )
                }
            ).catch(
                (error) => {
                    $('div#errors').append(
                        `<p>AudioControls: playRecording arrayBuffer: ${error}</p>`
                    )
                    throw new Error(error)
                }
            )
        } catch (e) {
            throw new Error(`playRecording: ${e}`)
        }
    }

    /**
     * drawWaveform: code is based on:
     * https://stackoverflow.com/questions/22073716/create-a-waveform-of-the-full-track-with-web-audio-api
     *
     * @param {AudioBuffer} audioBuffer - audio data to convert to waveform.
     */
    private drawWaveform(audioBuffer: AudioBuffer) {
        // Pycharm thinks 'canvas' should be of type HTMLElement instead of
        // HTMLCanvasElement. Putting in this 'ignore' to silence the nagging.
        // @ts-ignore
        const canvas: HTMLCanvasElement = document.getElementById(
            this._waveformCanvasId
        )

        const canvasCtx = canvas.getContext("2d")
        const width = canvas.width;
        const height = canvas.height;

        //
        // set background color to black
        canvasCtx.fillStyle = this._waveformBackgroundCSS
        canvasCtx.fillRect(0, 0, width, height);

        //
        // set foreground color
        canvasCtx.fillStyle = this._waveformForegroundCSS

        //
        // get the raw audio data for this recording timeslice. The
        // MediaRecorder has a sub-second save feature that generates the
        // incoming data.
        let rawAudioData = audioBuffer.getChannelData(0)

        // divide the raw data length by the canvas width to get the per-pixel
        // chunk size
        let rawAudioDataLength = rawAudioData.length
        let offset = 0
        if (rawAudioDataLength > this.audio_display_value_length) {
            offset = rawAudioDataLength - this.audio_display_value_length
        }
        let workingData = rawAudioData.slice(offset, rawAudioDataLength - 1)
        const chunkSize = Math.max(
            1,
            Math.ceil(workingData.length / width)
        )

        // each pixel-chunk of audio data could have values above and below
        // zero. The positive values will contribute to the displayed area
        // above the "origin" line in the output and the negative values
        // will be below the origin.
        //
        // There could be thousands+ of points in a per-pixel chunk. For quick
        // display purposes we really only care about the single largest
        // positive and smallest negative value in a chunk.  These two points
        // define the length of the vertical line (really a rectangle) that will
        // represent the pixel-chunk in the result.
        let positiveValues: number[] = []
        let negativeValues: number[] = []

        for (let x = 0; x < width; x++) {
            // array slice the raw data into pixel-chunks
            const start = x * chunkSize
            const end = start + chunkSize
            const chunk = workingData.slice(start, end)

            // find and save the largest and smallest values in this chunk
            positiveValues.push(Math.max(...chunk))
            negativeValues.push(Math.min(...chunk))
        }
        // we are done with the raw data, let it go
        rawAudioData = null

        // the positiveValues and negativeValues values define the largest
        // vertical space needed to display all the collected points.  Each
        // pair of values: ( positiveValues[x], negativeValues[x] ) define a
        // future vertical line in the display.
        //
        // max is the largest of the positiveValues and min is the smallest
        // of the  negativeValues.  The difference between these two values
        // is the longest possible vertical line that could exist in the
        // incoming data.  These values will be scaled up to just fit in the
        // available height of the canvas.
        let max = Math.max(...positiveValues)
        let min = Math.min(...negativeValues)

        // Find the "Scaling Factor":

        // let's say that the vertical height of the canvas is 100. The
        // "origin" line will be at 50 (half the height). The values in
        // positiveValues & negativeValues could be anything (the PCM
        // standard may specify specific ranges of values but those ranges
        // were not readily apparent). The values have to be "scaled"
        // up/down to fit within the height limit while keeping "zero" on
        // the center line.
        //
        // Find the largest absolute value of min & max. This value is
        // "farthest" from the origin/zero so finding the value that will
        // scale that number up/down to just fit inside its "side" of the
        // origin becomes the scaling factor for all the other values.
        //
        // The scaling factor is then the largest value divided by half the
        // height of the canvas.
        let maxOriginOffset = Math.max(Math.abs(max), Math.abs(min))
        let halfHeight = Math.floor(height / 2)
        let scalingFactor = maxOriginOffset / halfHeight

        // apply the scaling factor to all pixel-chunk values and build a
        // rectangle whose width is 1 (pixel).  The top of the rectangle is the
        // distance from the origin (half the canvas height) to the scaled
        // "positive" value.  The bottom of the rectangle is the distance
        // from the origin to the scaled "negative" value.
        for (let x = 0; x < positiveValues.length; x++) {
            let top = positiveValues[x]
            if (top < this.audio_display_threshold) {
                top = 0.0
            }
            let bottom = negativeValues[x]
            if (bottom > this.audio_display_threshold * -1) {
                bottom = 0.0
            }
            top /= scalingFactor
            bottom /= scalingFactor

            // "x" is the horizontal pixel position along the origin line.
            // "y" is how high above the origin the rectangle will start.
            // "width" is 1 pixel
            // "height" is the distance between the top and bottom values
            canvasCtx.fillRect(
                x,
                halfHeight - top,
                1,
                Math.max(1, top - bottom)
            )
        }
    }

    /**
     * Convert the recorded Blob into audio
     */
    private async decodeAndDisplayAudio() {
        try {
            // convert the blob audio data into an AudioContext and prepare to
            // display a "waveform".
            const audioCtx = new AudioContext();

            new Blob(
                this._chunks,
                { 'type': this._codec }
            ).arrayBuffer().then(
                buffer => {
                    audioCtx.decodeAudioData(
                        buffer
                    ).then(
                        buf => {
                            this.drawWaveform(buf)
                        }
                    ).catch(
                        error => {
                            $('div#errors').append(
                                `<p>AudioControls: decodeAndDisplayAudio decodeAudioData: ${error}</p>`
                            )
                            throw new Error(error)
                        }
                    )
                }
            ).catch(
                error => {
                    $('div#errors').append(
                        `<p>AudioControls: decodeAndDisplayAudio arrayBuffer: ${error}</p>`
                    )
                    throw new Error(error)
                }
            )
        } catch (e) {
            throw new Error(`decodeAndDisplayAudio: ${e}`)
        }
    }

    public getAudioBlob(): Blob {
        if (this._isRecording) {
            return undefined
        }

        return new Blob(
            this._chunks,
            { 'type': this._codec }
        )
    }
}

/**
 * Custom exception for the case when no audio inputs are found.
 */
class NoAudioInputs extends Error {
    constructor(message: string) {
        super(message);
        this.name = 'NoAudioInputs';

        // Set the prototype explicitly.
        Object.setPrototypeOf(this, NoAudioInputs.prototype);
    }
}