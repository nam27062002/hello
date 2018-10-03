// ReplayKitTest.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 26/09/2018.
// Copyright (c) 2018 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;
#if UNITY_IOS
using UnityEngine.Apple.ReplayKit;
#endif

using System.Text;
using System.Collections.Generic;

using TMPro;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// 
/// </summary>
public class ReplayKitTest : MonoBehaviour {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	private class LogEntry {
		public string key = "";
		public string message = "";
		public int startFrame = 0;
		public int endFrame = 0;

		override public string ToString() {
			return string.Format("[{2}] [{0}-{1}] {3}", startFrame, endFrame, key, message);
		}
	}

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed
	[SerializeField] private TextMeshProUGUI m_logText = null;
	[SerializeField] private TextMeshProUGUI m_isRecordingText = null;
	[SerializeField] private TextMeshProUGUI m_recordAvailableText = null;
	[Space]
	[SerializeField] private Button m_startRecordingButton = null;
	[SerializeField] private Button m_stopRecordingButton = null;
	[SerializeField] private Button m_previewButton = null;

	// Internal
	private List<LogEntry> m_log = new List<LogEntry>();
	private LogEntry m_lastEntry = null;
	private StringBuilder m_sb = new StringBuilder();
	
	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {
		m_startRecordingButton.onClick.AddListener(OnStartRecording);
		m_stopRecordingButton.onClick.AddListener(OnStopRecording);
		m_previewButton.onClick.AddListener(OnShowPreview);
	}

	/// <summary>
	/// First update call.
	/// </summary>
	private void Start() {

	}

	/// <summary>
	/// Component has been enabled.
	/// </summary>
	private void OnEnable() {

	}

	/// <summary>
	/// Component has been disabled.
	/// </summary>
	private void OnDisable() {

	}

	/// <summary>
	/// Called every frame.
	/// </summary>
	private void Update() {
#if UNITY_IOS
        // Set buttons visibility
        m_startRecordingButton.interactable = !ReplayKit.isRecording;
		m_stopRecordingButton.interactable = ReplayKit.isRecording;
		m_previewButton.interactable = ReplayKit.recordingAvailable;

		// Show status vars
		m_isRecordingText.text = "isRecording: " + ReplayKit.isRecording;
		m_recordAvailableText.text = "recordingAvailable: " + ReplayKit.recordingAvailable;

		// Log lastError
		Log(ParseReplayKitLastErrorCode(), ReplayKit.lastError);
#else
        // Set buttons visibility
        m_startRecordingButton.interactable = false;
        m_stopRecordingButton.interactable = false;
        m_previewButton.interactable = false;

        // Show status vars
        m_isRecordingText.text = "isRecording: false";
        m_recordAvailableText.text = "recordingAvailable: false";

#endif
    }

    /// <summary>
    /// Destructor.
    /// </summary>
    private void OnDestroy() {
		m_startRecordingButton.onClick.RemoveListener(OnStartRecording);
		m_stopRecordingButton.onClick.RemoveListener(OnStopRecording);
		m_previewButton.onClick.RemoveListener(OnShowPreview);
	}

	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// 
	/// </summary>
	/// <param name="_key">Key.</param>
	/// <param name="_message">Message.</param>
	private void Log(string _key, string _message) {
		// Reuse last entry?
		if(m_lastEntry != null && m_lastEntry.key == _key) {
			// If key matches last entry, just update endframe
			m_lastEntry.endFrame = Time.frameCount;
		} else {
			// Create a new entry
			m_lastEntry = new LogEntry();
			m_lastEntry.key = _key;
			m_lastEntry.message = _message;
			m_lastEntry.startFrame = Time.frameCount;
			m_lastEntry.endFrame = Time.frameCount;
			m_log.Add(m_lastEntry);
		}

		// Refresh log text
		RefreshLog();
	}

	/// <summary>
	/// 
	/// </summary>
	private void RefreshLog() {
		m_sb.Length = 0;
		for(int i = 0; i < m_log.Count; ++i) {
			m_sb.AppendLine(m_log[i].ToString());
		}

		m_logText.text = m_sb.ToString();
	}

	/// <summary>
	/// 
	/// </summary>
	/// <param name="_text">Text.</param>
	private void ShowFeedback(string _text) {
		UIFeedbackText.CreateAndLaunch(
			_text,
			GameConstants.Vector2.center,
			this.GetComponentInParent<Canvas>().transform as RectTransform
		).text.color = Colors.red;
	}

	/// <summary>
	/// Parses the replay kit last error.
	/// </summary>
	/// <returns>Last parsed error code. Empty string if no error or error unknown.</returns>
	private string ParseReplayKitLastErrorCode() {
        // Get last error
#if UNITY_IOS
        string lastError = ReplayKit.lastError;
#else
        string lastError = "";
#endif

        // Protect from null
        if (string.IsNullOrEmpty(lastError)) {
			return "0";
		}

		// [AOC] GOING TO HELL!! Only way to know is consulting the ReplayKit.lastError string and compare with known error codes
		// Possible Errors (from https://github.com/tijme/reverse-engineering/blob/master/Billy%20Ellis%20ARM%20Explotation/iPhoneOS9.3.sdk/System/Library/Frameworks/ReplayKit.framework/Headers/RPError.h):
		// RPRecordingErrorUnknown = -5800,
		// RPRecordingErrorUserDeclined = -5801, // The user declined app recording.
		// RPRecordingErrorDisabled = -5802, // App recording has been disabled via parental controls.
		// RPRecordingErrorFailedToStart = -5803, // Recording failed to start
		// RPRecordingErrorFailed = -5804, // Failed during recording
		// RPRecordingErrorInsufficientStorage = -5805, // Insufficient storage for recording.
		// RPRecordingErrorInterrupted = -5806, // Recording interrupted by other app
		// RPRecordingErrorContentResize = -5807 // Recording interrupted by multitasking and Content Resizing
		string[] codes = new string[] { "-5800", "-5801", "-5802", "-5803", "-5804", "-5805", "-5806", "-5807" };
		for(int i = 0; i < codes.Length; ++i) {
			if(lastError.Contains(codes[i])) {
				// Error found! Return its code
				return codes[i];
			}
		}
		return "0";
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Start recording button has been pressed.
	/// </summary>
	public void OnStartRecording() {
#if UNITY_IOS
        if(ReplayKit.isRecording) {
			ShowFeedback("Already Recording!");
			return;
		}

		if(ReplayKit.recordingAvailable) ReplayKit.Discard();

		ReplayKit.StartRecording();
#endif
	}

	/// <summary>
	/// Stop recording button has been pressed.
	/// </summary>
	public void OnStopRecording() {
#if UNITY_IOS
		if(!ReplayKit.isRecording) {
			ShowFeedback("No Recording in progress!");
			return;
		}

		ReplayKit.StopRecording();
#endif
    }

    /// <summary>
    /// Show preview button has been pressed.
    /// </summary>
    public void OnShowPreview() {
#if UNITY_IOS
        if (!ReplayKit.recordingAvailable) {
			ShowFeedback("Recording Not Available!");
			return;
		}

		ReplayKit.Preview();
#endif
    }
}