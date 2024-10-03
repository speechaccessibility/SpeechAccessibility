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

import * as WavFileEncoder from "../lib/wav-file-encoder/dist/WavFileEncoder.js"

export class AudioControls {
	private _chunks: Blob[] = [];
	private _mediaRecorder?: MediaRecorder;
	private _stream?: MediaStream;

	private _recordingContext: AudioContext;

	// constructor params
	private readonly _codec: string = undefined
	private readonly _recordStartButtonId: string;
	private readonly _recordStopButtonId: string;

	private readonly _waveformCanvasId: string;
	private readonly _waveformBackgroundCSS: string = 'black'
	private readonly _waveformForegroundCSS: string = 'white'

	private readonly _maxRecordingTimeMsec: number = 60000
	private readonly _recordingSampleRateMsec: number = 250

	private _recordingTimeExceededId: NodeJS.Timeout;
	private _isRecording: boolean = false

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
	public recordingTimeExceeded = new CustomEvent(
		'AudioControls.RecordingTimeExceeded'
	)
	public eventBrowserNotSupported = new CustomEvent(
		'AudioControls.BrowserNotSupported'
	)
	public eventNoInputDevicesFound = new CustomEvent(
		'AudioControls.NoInputDevicesFound'
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
	 */
	constructor(codec: string,
	            recordStartButtonId: string | undefined,
	            recordStopButtonId: string | undefined = undefined,
	            waveformCanvasId: string | undefined   = undefined,
	            waveformBackgroundCSS: string          = "black",
	            waveformForegroundCSS: string          = "white",
	            maxRecordingTimeMsec: number           = 60000,
	            recordingSampleRateMsec: number        = 100,
	            audio_display_threshold: number        = 0.005,
	            audio_display_value_length: number     = 1024 * 16) {
		// codec is REQUIRED
		if (!(codec && codec.trim())) {
			let msg = `recordStartButtonId is required.`
			console.log(msg)
			throw new Error(msg)
		}
		if (typeof codec != 'string') {
			let msg = `codec should be a string.`
			console.log(msg)
			throw new Error(msg)
		}
		this._codec = codec.trim()

		// RECORD START/STOP BUTTONS
		// recordStartButtonId is REQUIRED
		if (!(recordStartButtonId && recordStartButtonId.trim())) {
			let msg = `recordStartButtonId is required.`
			console.log(msg)
			throw new Error(msg)
		}
		if (typeof recordStartButtonId != 'string') {
			let msg = `recordStartButtonId should be a string.`
			console.log(msg)
			throw new Error(msg)
		}
		recordStartButtonId = recordStartButtonId.trim()
		if (!document.getElementById(recordStartButtonId)) {
			let msg = `recordStartButtonId value "${recordStartButtonId}" `
			          + `does not exist in the DOM.`
			console.log(msg)
			throw new Error(msg)
		}
		this._recordStartButtonId = recordStartButtonId
		// if recordStopButtonId is undefined make this.recordStopButtonId
		// the same as this.recordStartButtonId
		if (!(recordStopButtonId && recordStopButtonId.trim())) {
			this._recordStopButtonId = this._recordStartButtonId
		} else {
			if (typeof recordStopButtonId != 'string') {
				let msg = `recordStopButtonId should be a string or undefined.`
				console.log(msg)
				throw new Error(msg)
			}
			recordStopButtonId = recordStopButtonId.trim()
			if (!document.getElementById(recordStopButtonId)) {
				let msg = `recordStopButtonId value "${recordStopButtonId}" `
				          + `does not exist in the DOM.`
				console.log(msg)
				throw new Error(msg)
			}
			this._recordStopButtonId = recordStopButtonId
		}

		// WAVEFORM CANVAS
		if (!(waveformCanvasId && waveformCanvasId.trim())) {
			// no start button ID so no stop button ID
			this._waveformCanvasId = undefined
		} else {
			if (typeof waveformCanvasId != 'string') {
				let msg = `waveformCanvasId should be a string.`
				console.log(msg)
				throw new Error(msg)
			}
			waveformCanvasId = waveformCanvasId.trim()
			if (!document.getElementById(waveformCanvasId)) {
				let msg = `waveformCanvasId value "${waveformCanvasId}" `
				          + `does not exist in the DOM.`
				console.log(msg)
				throw new Error(msg)
			}
			this._waveformCanvasId = waveformCanvasId

			if (waveformBackgroundCSS) {
				if (typeof waveformBackgroundCSS != 'string') {
					let msg = `waveformBackgroundCSS should be a string.`
					console.log(msg)
					throw new Error(msg)
				}
				this._waveformBackgroundCSS = waveformBackgroundCSS.trim()
			}
			if (waveformForegroundCSS) {
				if (typeof waveformForegroundCSS != 'string') {
					let msg = `waveformForegroundCSS should be a string.`
					console.log(msg)
					throw new Error(msg)
				}
				this._waveformForegroundCSS = waveformForegroundCSS.trim()
			}
		}

		// MAX RECORDING TIME
		if (typeof maxRecordingTimeMsec != 'number') {
			let msg = `maxRecordingTimeMsec should be a number in `
			          + `milliseconds.`
			console.log(msg)
			throw new Error(msg)
		}
		this._maxRecordingTimeMsec = Math.ceil(maxRecordingTimeMsec)

		if (typeof recordingSampleRateMsec != 'number') {
			throw new Error(
				`recordingSampleRateMsec should be a number in `
				+ `milliseconds.`
			)
		}
		if (recordingSampleRateMsec) {
			this._recordingSampleRateMsec = Math.ceil(recordingSampleRateMsec)
			if (this._recordingSampleRateMsec === 0) {
				let msg = `Recording Sample Rate value is zero.`
				console.log(msg)
				throw new Error(msg)
			}
		}

		// AUDIO DISPLAY THRESHOLD
		if (typeof audio_display_threshold != 'number') {
			let msg = `audio_display_threshold should be a floating `
			          + `point number.`
			console.log(msg)
			throw new Error(msg)
		}
		if (audio_display_threshold > 1.0 || audio_display_threshold < -1.0) {
			let msg = `audio_display_threshold only has a useful range `
			          + `between -1.0 and 1.0. Default is 0.005.`
			console.log(msg)
			throw new Error(msg)
		}
		this.audio_display_threshold = audio_display_threshold

		// AUDIO DISPLAY VALUE LENGTH
		if (typeof audio_display_value_length != 'number') {
			let msg = `audio_display_value_length should be an integer > 0.`
			console.log(msg)
			throw new Error(msg)
		}
		if (audio_display_value_length < 1) {
			let msg = `audio_display_value_length only has a useful integer`
			          + ` range > 0.`
			console.log(msg)
			throw new Error(msg)
		}
		this.audio_display_value_length = Math.ceil(audio_display_value_length)

		//
		// get list of available input devices. The "list" is probably not
		// useful but the number of potential devices is.
		let devices: Promise<MediaDeviceInfo[]> = undefined
		try {
			devices = navigator.mediaDevices.enumerateDevices()
		} catch {
			let msg = `Browser Not Supported.`
			console.log(msg)
			document.dispatchEvent(
				new CustomEvent(
					'AudioControls.Error',
					{
						'detail': 'This browser does not support recording. Please try opening this page in a different browser or update this browser to the most recent version to continue'
					}
				)
			);
			throw new Error(msg)
		}

		devices.then(
			devices => {
				//
				// There are devices attached. Are there any audio input
				// devices?
				const availableInputDevices = devices.filter(
					(d) => d.kind === 'audioinput'
				)

				if (!availableInputDevices.length) {
					let msg = `No input devices found.`
					console.log(msg)
					document.dispatchEvent(
						new CustomEvent(
							'AudioControls.Error',
							{
								'detail': 'No input devices found. Please attach a microphone and reload the page'
							}
						)
					);
					throw new Error(msg)
				}

				this.initializeRecording()
					.catch(
						(error) => {
							console.log(error.message)
							document.dispatchEvent(
								new CustomEvent(
									'AudioControls.Error',
									{
										'detail': error.message
									}
								)
							)
							throw new Error(error.message)
						}
					)
			}
		).catch(
			(error) => {
				console.log(error.message)
				switch (error.message) {
					case "Browser Not Supported.":
						document.dispatchEvent(
							this.eventBrowserNotSupported
						)
						break
					case "No input devices found.":
						document.dispatchEvent(
							this.eventNoInputDevicesFound
						)
						break
					default:
						document.dispatchEvent(
							new CustomEvent(
								'AudioControls.Error',
								{
									'detail': error.message
								}
							)
						)
						break
				}
				throw new Error(error.message)
			}
		)
	}

	/**
	 * If there is at least one input device potentially signal the user to
	 * grant access.
	 *
	 * @throws Exceptions are possible from navigator.mediaDevices.getUserMedia
	 */
	private async initializeRecording(): Promise<void> {
		try {
			this._recordingContext = new (
				AudioContext)()
		} catch (e) {
			console.log(e)
			document.dispatchEvent(
				new CustomEvent(
					'AudioControls.Error',
					{
						'detail': `Trying to establish an AudioContext: ${e}`
					}
				)
			);
			throw new Error(e)
		}

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
					let msg = `No MediaStream "stream" found.`
					console.log(msg)
					document.dispatchEvent(
						new CustomEvent(
							'AudioControls.Error',
							{
								'detail': `${msg}`
							}
						)
					);
					throw new Error(msg)
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
								let msg = `startRecording error: ${error.message}.`
								console.log(msg)
								if ('MediaRecorder.start: The MediaStream is inactive' != error.message) {
									document.dispatchEvent(
										new CustomEvent(
											'AudioControls.Error',
											{
												'detail': error.stack
											}
										)
									)
								}
								throw new Error(msg)
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
			}
		).catch(
			(error) => {
				console.log(error.message)
				document.dispatchEvent(
					new CustomEvent(
						'AudioControls.Error',
						{
							'detail': `Trying to GetUserMedia: ${error.message}`
						}
					)
				);
				throw new Error(error.message);
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
	private async startRecording(): Promise<void> {
		return new Promise(
			() => {

				//console.log('entering startRecording')
				this._chunks = [];

				if (!this._codec) {
					let msg = `Codec was not specified.`
					console.log(msg)
					document.dispatchEvent(
						new CustomEvent(
							'AudioControls.Error',
							{
								'detail': `startRecording: no codec: ${msg}`
							}
						)
					);
					throw new Error(msg)
				}

				try {
					this._mediaRecorder = new MediaRecorder(
						this._stream,
						{
							mimeType: this._codec
						}
					);
				} catch (e) {
					console.log(e)
					document.dispatchEvent(
						new CustomEvent(
							'AudioControls.Error',
							{
								'detail': `MediaRecorder: ${e}`
							}
						)
					);
					throw e
				}
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
							this.decodeAndDisplayAudio().catch(
								(error) => {
									console.log(error.message)
									//document.dispatchEvent(
									//	new CustomEvent(
									//		'AudioControls.Error',
									//		{
									//			'detail': `decodeAndDisplayAudio: ${error.message}`
									//		}
									//	)
									//);
									//throw new Error(error.message)
								}
							)
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
				//console.log('exiting startRecording')
			}
		);

	}

	/**
	 * onStop - Handle the MediaRecorder 'stop' event.
	 *
	 * @DOM_Events 'AudioControls.RecordingStopped': when playing has finished.
	 */
	private onStop() {
		//console.log('entering onStop')
		if (!this._isRecording) {
			console.log('Stop Recording called when Start Recording was not'
			            + ' active.')
			return
		}

		//
		// stop any active tracks. Count the audio tracks in case
		// there are also video tracks.  We only care about audio.

		//
		// How many audio tracks are there? Should be at least one.
		let getTracks: MediaStreamTrack[] = undefined
		try {
			getTracks = this._stream.getTracks()
		} catch (e) {
			console.log(e)
			document.dispatchEvent(
				new CustomEvent(
					'AudioControls.Error',
					{
						'detail': `getTracks: ${e}`
					}
				)
			);
			throw e
		}

		let recordingData = {}

		getTracks.filter(
			(track) => track.kind === 'audio'
		).forEach(
			(track, index) => {
				if (!recordingData
					.hasOwnProperty('track information')) {
					recordingData['track information'] = {
						'number of tracks'     : getTracks.length,
						'per track information': []
					}
				}

				recordingData['track information']
					['per track information'].push(
					{
						'track number': index,
						'track label' : track.label
					}
				)

				track.stop()
			}
		)

		this._isRecording = false

		clearTimeout(this._recordingTimeExceededId)
		this._recordingTimeExceededId = undefined

		//
		// this._chunks is the raw, un-submitted audio data.
		if (this._chunks.length == 0) {
			let msg = 'There are no audio chunks.'
			console.log(msg)
			document.dispatchEvent(
				new CustomEvent(
					'AudioControls.Error',
					{
						'detail': msg
					}
				)
			);
			throw new Error(msg)
		}

		let unsavedAudio = new Blob(
			this._chunks,
			{
				'type': 'audio/wav'
			}
		)
		this._chunks = undefined

		unsavedAudio.arrayBuffer().then(
			(buffer) => {
				this._recordingContext.decodeAudioData(
					buffer
				).then(
					(buffer) => {
						//
						// capture recording details
						if (!recordingData
							.hasOwnProperty('WAV information')) {
							recordingData['WAV information'] = {}
						}
						if (!recordingData['WAV information']
							.hasOwnProperty('sampleRate')) {
							recordingData['WAV information']
								['sampleRate'] = buffer.sampleRate
						}
						if (!recordingData['WAV information']
							.hasOwnProperty('length')) {
							recordingData['WAV information']['length'] =
								buffer.length
						}
						if (!recordingData['WAV information']
							.hasOwnProperty('numberOfChannels')) {
							recordingData['WAV information']
								['numberOfChannels'] = buffer.numberOfChannels
						}
						if (!recordingData['WAV information']
							.hasOwnProperty('duration')) {
							recordingData['WAV information']['duration'] =
								buffer.duration
						}

						//
						// Create AudioBuffer of WAV values
						const wavData = WavFileEncoder
							.encodeWavFileFromAudioBuffer(
								buffer,
								WavFileEncoder.WavFileType.float32
							)

						document.getElementById(
							this._recordStopButtonId
						).dispatchEvent(
							//
							// return audio data to the caller.
							new CustomEvent(
								'AudioControls.RecordingStopped',
								{
									'detail': {
										'recordingData': recordingData,
										'wavData'      : new Blob(
											[wavData],
											{type: 'audio/wav'}
										)
									}
								}
							)
						)
					}
				).catch(
					(error) => {
						console.log(error.message)
						//document.dispatchEvent(
						//	new CustomEvent(
						//		'AudioControls.Error',
						//		{
						//			'detail': `decodeAudioData: ${error.message}`
						//		}
						//	)
						//);
						//throw new Error(error.message)
					}
				)
			}
		).catch(
			error => {
				console.log(error.message)
				document.dispatchEvent(
					new CustomEvent(
						'AudioControls.Error',
						{
							'detail': `unsavedAudioBuffer: ${error.message}`
						}
					)
				);
				throw new Error(error.message)
			}

		)
		//console.log('exiting onstop')
	}

	/**
	 * drawWaveform: code is based on:
	 * https://stackoverflow.com/questions/22073716/create-a-waveform-of-the-full-track-with-web-audio-api
	 *
	 * @param {AudioBuffer} audioBuffer - audio data to convert to waveform.
	 */
	private drawWaveform(audioBuffer: AudioBuffer) {
		try {
			// Pycharm thinks 'canvas' should be of type HTMLElement instead of
			// HTMLCanvasElement. Putting in this 'ignore' to silence the
			// nagging.
			// @ts-ignore
			const canvas: HTMLCanvasElement = document.getElementById(
				this._waveformCanvasId
			)

			const canvasCtx = canvas.getContext("2d")
			const width = canvas.width;
			const height = canvas.height;

			const halfHeight = Math.floor(height / 2)

			//
			// set background color to black
			canvasCtx.fillStyle = this._waveformBackgroundCSS
			canvasCtx.fillRect(0, 0, width, height);
			canvasCtx.fillStyle = this._waveformForegroundCSS

			//
			// set foreground color
			canvasCtx.strokeStyle = this._waveformForegroundCSS
			canvasCtx.beginPath()
			canvasCtx.moveTo(0, halfHeight)
			canvasCtx.lineTo(width, halfHeight)
			canvasCtx.stroke()


			//
			// get the raw audio data for this recording timeslice. The
			// MediaRecorder has a sub-second save feature that generates the
			// incoming data.
			//
			// We only care about the first channel for the display.
			if (audioBuffer.numberOfChannels < 1) {
				throw new Error("AudioBuffer has no channels.")
			}

			let rawAudioData = audioBuffer.getChannelData(0)

			//
			// the amount of recorded audio could be HUGE. Trying to display
			// and redisplay all of it "in real time" is insane.  Just
			// display, at most,  the last this.audio_display_value_length
			// amount.
			let rawAudioDataLength = rawAudioData.length

			//
			// offset is how much of the accumulated audio to skip before
			// display
			let offset = 0
			if (rawAudioDataLength > this.audio_display_value_length) {
				offset = rawAudioDataLength - this.audio_display_value_length
			}

			//
			// workingData is the part of the accumulated audio to display.
			// There could be 10's of thousands of datapoints, so we will
			// divide workingData length by the number of pixels.  Each
			// pixel chunk will have a max and min value. Find and plot those.
			let workingData = rawAudioData.slice(offset, rawAudioDataLength - 1)
			const chunkSize = Math.max(
				1,
				Math.ceil(workingData.length / width)
			)

			//
			// find the high and low values in the workingData block
			let workingDataMax = Number.MIN_VALUE
			let workingDataMin = Number.MAX_VALUE
			workingData.forEach(
				(value) => {
					if (value > workingDataMax) {
						workingDataMax = value
					}
					if (value < workingDataMin) {
						workingDataMin = value
					}
				}
			)

			//
			// the scaling factor is based on the largest "absolute" value in
			// this "frame" of audio data.  The largest value should always be
			// between -1 and 1.
			//
			// the scaling factor is finally negated in order to "flip" the
			// perceived direction of positive and negative audio values.
			// This is because the canvas widget has its (0,0) "origin"
			// point in the upper left hand corner.  "Positive" Y values then
			// appear to increase "downwards" which makes numeric sense but
			// not visual sense.  The end effect is that negative values
			// multiplied by the negative scaling factor become "positive"
			// and vice versa.
			let scalingFactor = 1.0 / Math.max(
				Math.abs(workingDataMax), Math.abs(workingDataMin)
			) * -1

			//
			// loop over the incoming data
			for (let x = 0; x < width; x++) {
				// array slice the raw data into pixel-chunks
				const start = x * chunkSize
				const end = start + chunkSize
				//
				// "chunk" should hold floating point values between -1.0
				// and 1.0
				const chunk = workingData.slice(start, end)

				//
				// find the high and low values in this chunk. There might
				// be thousands of values in the chunk only draw a line
				// between the largest and smallest.
				let chunkMax = Number.MIN_VALUE
				let chunkMin = Number.MAX_VALUE
				chunk.forEach(
					(value) => {
						if (value > chunkMax) {
							chunkMax = value
						}
						if (value < chunkMin) {
							chunkMin = value
						}
					}
				)

				let cmx = chunkMax
				if (cmx < this.audio_display_threshold
				    && cmx > (this.audio_display_threshold * -1)) {
					cmx = 0.0
				}

				let cmn = chunkMin
				if (cmn < this.audio_display_threshold
				    && cmn > (this.audio_display_threshold * -1)) {
					cmn = 0.0
				}

				let top = cmx * scalingFactor // Scale the value to "1.0".
				          * halfHeight        // Scale the "1" value to the
				          //     space around the midline.
				          + halfHeight        // Translate the value down to
			                                  //     the midline.
				let bottom = cmn * scalingFactor // Scale the value to "1.0".
				             * halfHeight        // Scale the "1" value to the
				             //     space around the
				             //     midline.
				             + halfHeight        // Translate the value down to
			                                     //     the midline.

				canvasCtx.beginPath()
				canvasCtx.moveTo(x, top)
				canvasCtx.lineTo(x, bottom)
				canvasCtx.stroke()
			}
		} catch (e) {
			document.dispatchEvent(
				new CustomEvent(
					'AudioControls.Error',
					{
						'detail': `drawWaveform: ${e}`
					}
				)
			);
			throw new Error(e)
		}
	}

	/**
	 * Convert the recorded Blob into audio
	 */
	private async decodeAndDisplayAudio(): Promise<void> {
		try {
			// convert the blob audio data into an AudioContext and prepare to
			// display a "waveform".

			new Blob(
				this._chunks,
				{'type': 'audio/wav'}
			).arrayBuffer().then(
				(arrayBuffer) => {
					this._recordingContext.decodeAudioData(
						arrayBuffer
					).then(
						(audioBuffer) => {
							if (!audioBuffer) {
								console.log(`undefined audioBuffer`)
								return
							}
							this.drawWaveform(audioBuffer)
						}
					).catch(
						(error) => {
							console.log(error.message)
							//document.dispatchEvent(
							//	new CustomEvent(
							//		'AudioControls.Error',
							//		{
							//			'detail': `decodeAndDisplayAudio: decodeAudioData: ${error.message}`
							//		}
							//	)
							//);
							//throw new Error(error.message)
						}
					)
				}
			).catch(
				(error) => {
					console.log(error.message)
					//document.dispatchEvent(
					//	new CustomEvent(
					//		'AudioControls.Error',
					//		{
					//			'detail': `decodeAndDisplayAudio reading Blob: ${error.message}`
					//		}
					//	)
					//);
					//throw new Error(error.message)
				}
			)
		} catch (e) {
			console.log(e)
			//document.dispatchEvent(
			//	new CustomEvent(
			//		'AudioControls.Error',
			//		{
			//			'detail': `decodeAndDisplayAudio: ${e}`
			//		}
			//	)
			//);
			//throw new Error(e)
		}
	}
}
