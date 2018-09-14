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
#if (UNITY_IOS || UNITY_EDITOR_OSX)
using UnityEngine.Apple.ReplayKit;
#endif
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
#if (UNITY_IOS || UNITY_EDITOR_OSX)
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	public enum State {
		OFF,

		INIT,
		PREVIEW,
		COUNTDOWN,
		RECORDING,
		SHARING,
		FINISH,

		COUNT
	}

	private const float TONGUE_REMINDER_TIME = 20f;
	private const float MAX_RECORDING_TIME = 10f;
	private const float COUNTDOWN = 3f;

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
	[SerializeField] private ShowHideAnimator m_countdownGroup = null;
	[SerializeField] private ShowHideAnimator m_recordingModeGroup = null;

	// Other components
	[Space]
	[SerializeField] private Animator m_countdownAnim = null;
	[SerializeField] private TextMeshProUGUI m_countdownText = null;

	[Space]
	[SerializeField] private Slider m_recordingTimeBar = null;

	// Public properties
	private State m_state = State.OFF;
	public State state {
		get { return m_state; }
	}

	// Internal references
	private Camera[] m_mainSceneCameras = null;
	private GameObject m_animojiSceneInstance = null;
	private HDTongueDetector m_animojiSceneController = null;
	private UnityARVideo m_unityARVideo = null;
	private UnityARFaceAnchorManager m_unityARFaceAnchorManager = null;
	private ARCameraTracker m_ARCameraTracker = null;

	// Internal logic
	private float m_tongueReminderTimer = 0f;
	private float m_countdownTimer = 0f;
	private float m_recordingTimer = 0f;
	private float m_sharingTimer = 0f;

	//------------------------------------------------------------------------//
	// STATIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Check whether Animoji is supported for current device and a specific dragon.
	/// </summary>
	/// <returns><c>true</c> if current device, OS and given dragon support animoji.</returns>
	/// <param name="_dragonSku">Dragon sku to be checked.</param>
	public static bool IsSupported(string _dragonSku) {
		return IsDeviceSupported() && IsDragonSupported(_dragonSku);
	}

	/// <summary>
	/// Check whether Animoji is supported for a specific dragon.
	/// </summary>
	/// <returns><c>true</c> if given dragon supports animoji.</returns>
	/// <param name="_dragonSku">Dragon sku to be checked.</param>
	public static bool IsDragonSupported(string _dragonSku) {
		return HDTongueDetector.IsDragonSupported(_dragonSku);
	}

	/// <summary>
	/// Check whether Animoji is supported for current device.
	/// </summary>
	/// <returns><c>true</c> if current device and OS support animoji.</returns>
	public static bool IsDeviceSupported() {
		// Editor
#if(UNITY_EDITOR)
		return true;
#elif (UNITY_IOS || UNITY_ANDROID || UNITY_EDITOR_OSX)
		// AR supported?
		if(!ARKitManager.SharedInstance.IsARKitAvailable()) return false;

		// AR face tracking supported?
		if(!s_ARFaceTrackingConfig.IsSupported) return false;

		// ReplayKit supported?
		if(!ReplayKit.APIAvailable) return false;

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

		// Initialize recording progress bar
		m_recordingTimeBar.minValue = 0f;
		m_recordingTimeBar.maxValue = MAX_RECORDING_TIME;
		m_recordingTimeBar.value = 0f;
	}

	/// <summary>
	/// Called every frame
	/// </summary>
	private void Update() {
		if (m_state == State.OFF) {
			// if state equals OFF returns
			return;
		}

		// Tongue reminder - only in PREVIEW state
		if(m_state == State.PREVIEW) {
			// Update tongue reminder timer
			if(m_tongueReminderTimer > 0f) {
				m_tongueReminderTimer -= Time.deltaTime;
			}

			// Show the right UI
			RefreshInfoUI();
		}

		// Countdown timer - only in COUNTDOWN state
		if(m_state == State.COUNTDOWN) {
			// Update timer
			if(m_countdownTimer > 0f) {
				// Detect when we change second
				float newTime = m_countdownTimer - Time.deltaTime;
				if(Mathf.FloorToInt(m_countdownTimer) != Mathf.FloorToInt(newTime)) {
					// Update text and trigger anim
					SetCountdown(newTime, true);

					// SFX (except for last tick)
					if(newTime > 0f) {
						AudioController.Play("hd_padlock");
					}
				}

				// Update timer
				m_countdownTimer = newTime;

				// Countdown ended?
				if(m_countdownTimer <= 0f) {
					m_countdownTimer = 0f;
					ChangeState(State.RECORDING);
				}
			}
		}

		// Recording timer - only in RECORDING state
		if(m_state == State.RECORDING) {
			// Update timer
			m_recordingTimer -= Time.deltaTime;

			// Update progress bar
			m_recordingTimeBar.value = m_recordingTimeBar.maxValue - m_recordingTimer;	// Move forward

			// If timer has ended, change state
			if(m_recordingTimer <= 0f) {
				m_recordingTimer = 0f;
				ChangeState(State.SHARING);
			}
		}

		// Sharing window - only in SHARING state
		if(m_state == State.SHARING) {
			// Is video file ready?
			if(ReplayKit.recordingAvailable) {
				// Yes! Open native share dialog
				ControlPanel.Log(Colors.paleYellow.Tag("RECORD AVAILABLE!"));
				m_animojiSceneController.ShowPreview();

				// We don't really have a way to know when the native dialog finishes, so instantly move back to the PREVIEW state
				// See https://forum.unity.com/threads/replaykit-detect-preview-controller-finished.450509/
				ChangeState(State.PREVIEW);
			} else {
				// No! Update timeout timer
				m_sharingTimer -= Time.deltaTime;
				if(m_sharingTimer <= 0f) {
					// Timeout! Skip video sharing
					ControlPanel.Log(Colors.red.Tag ("SHARING TIME OUT!"));
					ChangeState(State.PREVIEW);
				}
			}
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
				GameObject scenePrefab = Resources.Load<GameObject>(HDTongueDetector.SCENE_PREFAB_PATH);
				Debug.Assert(scenePrefab != null, "COULDN'T LOADE ANIMOJI SCENE PREFAB (" + HDTongueDetector.SCENE_PREFAB_PATH + ")", this);

				// Instantiate it
				m_animojiSceneInstance = GameObject.Instantiate<GameObject>(scenePrefab);

				// Get animoji controller reference
				m_animojiSceneController = m_animojiSceneInstance.GetComponentInChildren<HDTongueDetector>();
				Debug.Assert(m_animojiSceneController != null, "Couldn't find HDTongueDetector!", this);

				m_unityARVideo = m_animojiSceneInstance.GetComponentInChildren<UnityARVideo> ();
				Debug.Assert (m_unityARVideo != null, "Couldn't find UnityARVideo", this);

				m_ARCameraTracker = m_animojiSceneInstance.GetComponentInChildren<ARCameraTracker> ();
				Debug.Assert (m_ARCameraTracker != null, "Couldn't find UnityARVideo", this);

				m_unityARFaceAnchorManager = m_animojiSceneInstance.GetComponentInChildren<UnityARFaceAnchorManager> ();
				Debug.Assert (m_unityARFaceAnchorManager != null, "Couldn't find UnityARFaceAnchorManager", this);

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

				// Turn music off
				AudioController.PauseMusic(0.5f);
			} break;

			case State.COUNTDOWN: {
				// Flow control
				Debug.Assert(oldState == State.PREVIEW, "FSM Exception! Can't transition from state " + oldState + " to state " + _newState, this);

				// Toggle views
				SelectUI(true);

				// Reset and initialize countdown timer
				m_countdownTimer = COUNTDOWN;
				SetCountdown(m_countdownTimer, false);
			} break;

			case State.RECORDING: {
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

				// We will change back to the preview state whenever the video file is ready
				// Add a timeout in case there was some error recording the video
				m_sharingTimer = 5f;

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
#endif
			} break;

			case State.FINISH: {
				// Discard any recorded video
				ReplayKit.Discard ();

				// Toggle views
				SelectUI(true);

				// Turn game back on
				ToggleMainCameras(true);

				// Unload Animoji Scene
				m_animojiSceneController.onFaceAdded.RemoveListener(OnFaceDetected);
				m_animojiSceneController.onTongueLost.RemoveListener(OnTongueLost);
//				GameObject.Destroy (m_animojiSceneController.gameObject);
//				m_animojiSceneController = null;				

				Debug.Log (">>>>>> Destroying UnityARVideo component");
				GameObject.Destroy (m_unityARVideo.gameObject);
				m_unityARVideo = null;

				Debug.Log (">>>>>> Destroying DragonAnimoji component");
				GameObject.Destroy (m_animojiSceneController.m_dragonAnimojiInstance.gameObject);
				m_animojiSceneController.m_dragonAnimojiInstance = null;
				Debug.Log (">>>>>> Destroying HDTongueDetector component");
				m_animojiSceneController.UnsubscribeDelegates ();
				GameObject.Destroy (m_animojiSceneController.gameObject);
				m_animojiSceneController = null;
				Debug.Log (">>>>>> Destroying PF_AnimojiSceneSetup root");
				GameObject.Destroy(m_animojiSceneInstance);
				m_animojiSceneInstance = null;
				Debug.Log (">>>>>> Destroying UnityARFaceAnchorManager component");
				GameObject.Destroy (m_unityARFaceAnchorManager.gameObject);
				m_unityARFaceAnchorManager = null;
				Debug.Log (">>>>>> Destroying ARCameraTracker component");
				GameObject.Destroy (m_ARCameraTracker.gameObject);
				m_ARCameraTracker = null;
//				UnityARSessionNativeInterface.GetARSessionNativeInterface ().RunWithConfigAndOptions (sessionConfig, UnityARSessionRunOption.ARSessionRunOptionRemoveExistingAnchors | UnityARSessionRunOption.ARSessionRunOptionResetTracking);

				// Switch back to original orientation
				Screen.orientation = ScreenOrientation.AutoRotation;

				// Restore music
				AudioController.UnpauseMusic(0.5f);

				// Show game HUD
				InstanceManager.menuSceneController.hud.animator.ForceShow(true);

				// Close the AR session
				ARKitManager.SharedInstance.FinishingARSession();

				// Finalize AR Game Manager
				ARGameManager.SharedInstance.UnInitialise();

				// Target frame rate restored to 30fps
				Application.targetFrameRate = 30;

				// Go to OFF state after some delay
				UbiBCN.CoroutineManager.DelayedCall(() => {
//					GameObject.Destroy(m_animojiSceneInstance);
//					m_animojiSceneInstance = null;
					ChangeState(State.OFF);
					Debug.Log (">>>>>>>>>>>>Animoji screen controller: delayed call : changestate(OFF);");
				}, 0.25f);
				Debug.Log (">>>>>>>>>>>>Animoji screen controller: Finish state end");
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
		m_tongueReminderGroup.Set(
			faceDetected && 
			!tongueDetected && 
			tongueReminderTimeout &&
			m_state != State.RECORDING		// Not while recording!
		);
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
			m_tongueReminderGroup.ForceSet(m_state == State.COUNTDOWN, _animate);	// Always show tongue reminder in COUNTDOWN state
		}

		// Init screen
		m_busyGroup.Set(m_state == State.INIT || m_state == State.FINISH, _animate);

		// Preview mode
		m_previewModeGroup.Set(m_state == State.PREVIEW, _animate);
		
		// Countdown UI
		m_countdownGroup.Set(m_state == State.COUNTDOWN, _animate);

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

	/// <summary>
	/// Update countdown text and optionally trigger animation.
	/// </summary>
	/// <param name="_seconds">Remaining seconds.</param>
	/// <param name="_triggerAnim">Launch animation?</param>
	private void SetCountdown(float _seconds, bool _triggerAnim) {
		// Set text
		m_countdownText.text = StringUtils.FormatNumber(
			Mathf.FloorToInt(_seconds) + 1	// Don't show 0
		);

		// Trigger anim
		m_countdownAnim.SetTrigger("launch");
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
		ChangeState(State.COUNTDOWN);
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
#endif
}