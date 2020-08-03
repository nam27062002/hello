// CPFPSRecorder.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 08/08/2019.
// Copyright (c) 2019 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;
using TMPro;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Widget for the Control Panel to record FPS variations.
/// TODO:
/// - Setup to measure a specific amount of frames / time
/// - History
/// </summary>
public class CPFpsRecorder : MonoBehaviour {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	private const float SAMPLING_INTERVAL = 0.25f;	// Interval in seconds where each sample will be taken

	private static readonly Color BUTTON_START_RECORDING_COLOR = Colors.paleGreen;
	private static readonly Color BUTTON_STOP_RECORDING_COLOR = Colors.coral;

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Toggle Button
	[Separator("Toggle Button")]
	[SerializeField] private Button m_toggleButton = null;
	[SerializeField] private TextMeshProUGUI m_buttonText = null;

	// Summary
	[Separator("Summary")]
	[SerializeField] private TextMeshProUGUI m_fpsMinText = null;
	[SerializeField] private TextMeshProUGUI m_fpsMaxText = null;
	[SerializeField] private TextMeshProUGUI m_fpsAvgText = null;
	[SerializeField] private TextMeshProUGUI m_recordingTimeText = null;
	[SerializeField] private TextMeshProUGUI m_sampleCountText = null;

	// Internal logic
	private bool m_recording = false;

	private float m_startTime = 0f;
	private float m_endTime = 0f;
	private int m_sampleCount = 0;

	private float m_accumulatedFps = 0f;
	private float m_minFps = 0f;
	private float m_maxFps = 0f;
	private float m_avgFps = 0f;

	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// First update call.
	/// </summary>
	private void Start() {
		// Init visuals
		RefreshButton();
		RefreshTexts();
	}

	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Take a sample :P
	/// </summary>
	private void TakeSample() {
		// Nothing to do if not recording
		if(!m_recording) return;

		// Gather current fps
		float fps = FeatureSettingsManager.instance.AverageSystemFPS;

		// Update counters
		m_sampleCount++;
		m_endTime = Time.realtimeSinceStartup;
		m_accumulatedFps += fps;
		m_avgFps = m_accumulatedFps / (float)(m_sampleCount);
		m_minFps = Mathf.Min(m_minFps, fps);
		m_maxFps = Mathf.Max(m_maxFps, fps);

		// Update visuals
		RefreshTexts();
	}

	/// <summary>
	/// Start/stop recording.
	/// </summary>
	private void ToggleRecording() {
		// Are we recording?
		if(m_recording) {
			// Yes! Stop recording
			m_recording = false;

			// Stop periodic call
			CancelInvoke("TakeSample");
		} else {
			// No! Reset values and start recording
			// Time
			m_startTime = Time.realtimeSinceStartup;
			m_endTime = m_startTime;
			m_sampleCount = 0;

			// FPS counters
			m_accumulatedFps = 0f;
			m_minFps = float.MaxValue;	// Making sure the first measurement will override this value
			m_maxFps = 0f;              // Making sure the first measurement will override this value
			m_avgFps = 0f;

			// Start recording!
			m_recording = true;

			// Start periodic call
			InvokeRepeating("TakeSample", 0, SAMPLING_INTERVAL);
		}

		// Refresh visuals
		RefreshTexts();
		RefreshButton();
	}

	/// <summary>
	/// Refresh the summary texts.
	/// </summary>
	private void RefreshTexts() {
		m_fpsMinText.text = Mathf.RoundToInt(m_minFps).ToString();
		m_fpsMaxText.text = Mathf.RoundToInt(m_maxFps).ToString();
		m_fpsAvgText.text = Mathf.RoundToInt(m_avgFps).ToString();

		m_recordingTimeText.text = TimeUtils.FormatTime(m_endTime - m_startTime, TimeUtils.EFormat.ABBREVIATIONS, 3);
		m_sampleCountText.text = m_sampleCount.ToString();
	}

	/// <summary>
	/// Refresh the button's visuals.
	/// </summary>
	private void RefreshButton() {
		m_buttonText.text = m_recording ? "STOP RECORDING" : "START RECORDING";
		ColorBlock colors = m_toggleButton.colors;
		colors.normalColor = m_recording ? BUTTON_STOP_RECORDING_COLOR : BUTTON_START_RECORDING_COLOR;
		m_toggleButton.colors = colors;
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Toggle button has been pressed.
	/// </summary>
	public void OnToggleButton() {
		ToggleRecording();
	}
}