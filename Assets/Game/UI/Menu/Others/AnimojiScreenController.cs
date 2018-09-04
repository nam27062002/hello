// AnimojiScreenController.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 30/08/2018.
// Copyright (c) 2018 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.iOS;
using UnityEngine.Apple.ReplayKit;
using UnityEngine.SceneManagement;

using System;
using System.Collections.Generic;

using DG.Tweening;
using TMPro;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Main controller for the animoji menu screen.
/// </summary>
public class AnimojiScreenController : MonoBehaviour {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	public enum State {
		OFF,

		INIT,
		PREVIEW,
		RECORDING,
		SHARING,
		FINISH,

		COUNT
	}

	private const float TONGUE_REMINDER_TIME = 20f;
	private const float MAX_RECORDING_TIME = 10f;

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Static
	private static ARKitFaceTrackingConfiguration s_ARFaceTrackingConfig = new ARKitFaceTrackingConfiguration(UnityARAlignment.UnityARAlignmentGravity, true);
	public static ARKitFaceTrackingConfiguration arFaceTrackingConfig {
		get { return s_ARFaceTrackingConfig; }
	}

	// Exposed
	// View groups
	[SerializeField] private ShowHideAnimator m_busyGroup = null;
	[SerializeField] private ShowHideAnimator m_faceNotDetectedGroup = null;
	[SerializeField] private ShowHideAnimator m_tongueReminderGroup = null;
	[SerializeField] private ShowHideAnimator m_previewModeGroup = null;
	[SerializeField] private ShowHideAnimator m_recordingModeGroup = null;

	// Other components
	[Space]
	[SerializeField] private TextMeshProUGUI m_recordingTimeText = null;

	// Public properties
	private State m_state = State.OFF;
	public State state {
		get { return m_state; }
	}

	// Internal references
	private Camera[] m_mainSceneCameras = null;
	private GameObject m_animojiSceneInstance = null;
	private HDTongueDetector m_animojiSceneController = null;

	// Internal logic
	private float m_tongueReminderTimer = 0f;
	private float m_recordingTimer = 0f;

	//------------------------------------------------------------------------//
	// STATIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Check whether Animoji is supported for current device and dragon.
	/// </summary>
	/// <returns><c>true</c> if current device, OS and given dragon support animoji.</returns>
	/// <param name="_dragonSku">Dragon sku to be checked.</param>
	public static bool IsSupported(string _dragonSku) {
		// Editor
#if(UNITY_EDITOR)
		// Requested dragon supported?
		if(!HDTongueDetector.IsDragonSupported(_dragonSku)) return false;

		return true;
#elif(UNITY_IOS || UNITY_ANDROID || UNITY_EDITOR_OSX)
		// AR supported?
		if(!ARKitManager.SharedInstance.IsARKitAvailable()) return false;

		// AR face tracking supported?
		if(!s_ARFaceTrackingConfig.IsSupported) return false;

		// ReplayKit supported?
		if(!ReplayKit.APIAvailable) return false;

		// Requested dragon supported?
		if(!HDTongueDetector.IsDragonSupported(_dragonSku)) return false;

		return true;
#else
		return false;
#endif
	}

	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {
		// Gather scene cameras
		m_mainSceneCameras = new Camera[] {
			InstanceManager.menuSceneController.mainCamera
		};
	}

	/// <summary>
	/// Called every frame
	/// </summary>
	private void Update() {
		// Tongue reminder
		// Only in some states
		if(m_state == State.PREVIEW || m_state == State.RECORDING) {
			// Update tongue reminder timer
			if(m_tongueReminderTimer > 0f) {
				m_tongueReminderTimer -= Time.deltaTime;
			}

			// Show the right UI
			RefreshInfoUI();
		}

		// Recording timer
		if(m_state == State.RECORDING) {
			// Update timer
			m_recordingTimer -= Time.deltaTime;

			// If timer has ended, change state
			if(m_recordingTimer <= 0f) {
				m_recordingTimer = 0f;
				ChangeState(State.SHARING);
			}

			// Update text
			m_recordingTimeText.text = TimeUtils.FormatTime(
				m_recordingTimer, 
				TimeUtils.EFormat.DIGITS_0_PADDING, 
				2, 
				TimeUtils.EPrecision.MINUTES, 
				true
			);
		}
	}

	/// <summary>
	/// Change logic state!
	/// </summary>
	/// <param name="_newState">State to change to.</param>
	private void ChangeState(State _newState) {
		ControlPanel.Log(Colors.paleYellow.Tag("Changing state from " + m_state + " to " + _newState));

		// Stuff to do when leaving a state
		switch(m_state) {
			case State.RECORDING: {
				// Make sure we stop recording!
				m_animojiSceneController.StopRecording();
			} break;
		}

		// Perform state change
		State oldState = m_state;
		m_state = _newState;

		// Stuff to do when entering a state
		switch(m_state) {
			case State.OFF: {
				// Toggle views
				SelectUI(false);
			} break;

			case State.INIT: {
				// Toggle views
				SelectUI(false);

				// Switch to portrait orientation
				Screen.orientation = ScreenOrientation.Portrait;

				// Hide game HUD
				InstanceManager.menuSceneController.hud.animator.ForceHide(true);

				// Load Animoji Scene
				GameObject scenePrefab = Resources.Load<GameObject>(HDTongueDetector.PREFAB_PATH);
				Debug.Assert(scenePrefab != null, "COULDN'T LOADE ANIMOJI SCENE PREFAB (" + HDTongueDetector.PREFAB_PATH + ")", this);

				// Instantiate it
				m_animojiSceneInstance = GameObject.Instantiate<GameObject>(scenePrefab);

				// Get animoji controller reference
				m_animojiSceneController = m_animojiSceneInstance.GetComponentInChildren<HDTongueDetector>();
				Debug.Assert(m_animojiSceneController != null, "Couldn't find HDTongueDetector!", this);

				// Initialize controller
				m_animojiSceneController.InitWithDragon(InstanceManager.menuSceneController.selectedDragon);
				m_animojiSceneController.onFaceAdded.AddListener(OnFaceDetected);
				m_animojiSceneController.onTongueLost.AddListener(OnTongueLost);

				// Go to PREVIEW state after some delay
				UbiBCN.CoroutineManager.DelayedCallByFrames(() => {
					ChangeState(State.PREVIEW);
				}, 1);
			} break;

			case State.PREVIEW: {
				// Toggle views
				SelectUI(true);

				// Reset tongue reminder
				m_tongueReminderTimer = TONGUE_REMINDER_TIME;

				// Turn game off
				ToggleMainCameras(false);
			} break;

			case State.RECORDING: {
				// Flow control
				Debug.Assert(oldState == State.PREVIEW, "FSM Exception! Can't transition from state " + oldState + " to state " + _newState, this);

				// Toggle views
				SelectUI(true);

				// Do it!
				m_animojiSceneController.StartRecording();

				// Reset timer
				m_recordingTimer = MAX_RECORDING_TIME;
			} break;

			case State.SHARING: {
				// Flow control
				Debug.Assert(oldState == State.RECORDING, "FSM Exception! Can't transition from state " + oldState + " to state " + _newState, this);

				// Toggle views
				SelectUI(true);

#if UNITY_EDITOR
				// Simulate sharing with some delay
				UIFeedbackText.CreateAndLaunch(
					"Sharing not supported in Editor :)", 
					GameConstants.Vector2.center, 
					this.GetComponentInParent<Canvas>().transform as RectTransform
				).text.color = Colors.orange;

				// Change state after some delay
				UbiBCN.CoroutineManager.DelayedCall(() => {
					ChangeState(State.PREVIEW);
				}, 1f);
#else
				// Open native share dialog
				m_animojiSceneController.ShowPreview();

				// We don't really have a way to know when the native dialog finishes, so instantly move back to the PREVIEW state
				// See https://forum.unity.com/threads/replaykit-detect-preview-controller-finished.450509/
				ChangeState(State.PREVIEW);
#endif
			} break;

			case State.FINISH: {
				// Toggle views
				SelectUI(true);

				// Unload Animoji Scene
				m_animojiSceneController.onFaceAdded.RemoveListener(OnFaceDetected);
				m_animojiSceneController.onTongueLost.RemoveListener(OnTongueLost);
				m_animojiSceneController = null;
				GameObject.Destroy(m_animojiSceneInstance);
				m_animojiSceneInstance = null;

				// Switch back to original orientation
				Screen.orientation = ScreenOrientation.AutoRotation;

				// Turn game back on
				ToggleMainCameras(true);

				// Show game HUD
				InstanceManager.menuSceneController.hud.animator.ForceShow(true);

				// Close the AR session
				ARKitManager.SharedInstance.FinishingARSession();

				// Finalize AR Game Manager
				ARGameManager.SharedInstance.UnInitialise();

				// Go to OFF state after some delay
				UbiBCN.CoroutineManager.DelayedCall(() => {
					ChangeState(State.OFF);
				}, 0.15f);
			} break;
		}
	}

	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Refresh the info UI based on face detection state.
	/// </summary>
	private void RefreshInfoUI() {
		// Check conditions
		bool faceDetected = m_animojiSceneController.faceDetected;
		bool tongueDetected = m_animojiSceneController.tongueDetected;
		bool tongueReminderTimeout = m_tongueReminderTimer <= 0f;

		// Apply
		m_faceNotDetectedGroup.Set(!faceDetected);
		m_tongueReminderGroup.Set(faceDetected && !tongueDetected && tongueReminderTimeout);
	}

	/// <summary>
	/// Show/hide UI groups based on current states.
	/// </summary>
	/// <param name="_animate">Perform animations?</param>
	private void SelectUI(bool _animate) {
		// Face not detected warning and tongue reminder
		if(m_state == State.PREVIEW || m_state == State.RECORDING) {
			RefreshInfoUI();
		} else {
			m_faceNotDetectedGroup.ForceHide(_animate);
			m_tongueReminderGroup.ForceHide(_animate);
		}

		// Init screen
		m_busyGroup.Set(m_state == State.INIT || m_state == State.FINISH, _animate);

		// Preview mode
		m_previewModeGroup.Set(m_state == State.PREVIEW, _animate);

		// Recording mode
		m_recordingModeGroup.Set(m_state == State.RECORDING, _animate);
	}

	/// <summary>
	/// Enable/disable game main cameras.
	/// </summary>
	/// <param name="_enabled">Toggle on or off?.</param>
	private void ToggleMainCameras(bool _enabled) {
		if(m_mainSceneCameras != null) {
			for(int i = 0; i < m_mainSceneCameras.Length; ++i) {
				if(m_mainSceneCameras[i] != null) {
					m_mainSceneCameras[i].enabled = _enabled;
				}
			}
		}
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// About to enter the screen.
	/// </summary>
	public void OnShowPreAnimation() {
		// Start flow!
		ChangeState(State.INIT);
	}

	/// <summary>
	/// Face tracking found.
	/// </summary>
	public void OnFaceDetected() {
		// Reset tongue reminder timer
		m_tongueReminderTimer = TONGUE_REMINDER_TIME;
	}

	/// <summary>
	/// Tongue tracking lost!
	/// </summary>
	public void OnTongueLost() {
		// Reset tongue reminder timer
		m_tongueReminderTimer = TONGUE_REMINDER_TIME;
	}

	/// <summary>
	/// The back button has been pressed.
	/// </summary>
	public void OnBackButton() {
		// Properly finalize flow
		ChangeState(State.FINISH);

		// Go back to previous menu screen
		// [AOC] Photo Screen doesn't allow going back to it, so just force it for now
		//InstanceManager.menuSceneController.transitionManager.Back(true);
		InstanceManager.menuSceneController.transitionManager.GoToScreen(MenuScreen.PHOTO, true);
	}

	/// <summary>
	/// The Record button has been pressed.
	/// </summary>
	public void OnStartRecordingButton() {
		// Just in case, check we are in the proper state
		if(m_state != State.PREVIEW) return;

		// Go to next state
		ChangeState(State.RECORDING);
	}

	/// <summary>
	/// The Stop button has been pressed.
	/// </summary>
	public void OnStopRecordingButton() {
		// Just in case, check we are in the proper state
		if(m_state != State.RECORDING) return;

		// Go to next state
		ChangeState(State.SHARING);
	}
}