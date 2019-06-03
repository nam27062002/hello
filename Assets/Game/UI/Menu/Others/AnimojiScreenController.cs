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
//#if (UNITY_IOS || UNITY_EDITOR_OSX)
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	public enum State {
		OFF,

		INIT,
		CAMERA_PERMISSIONS_REQUEST,
		MICROPHONE_PERMISSIONS_REQUEST,
		PERMISSIONS_OK,        
        PERMISSIONS_ERROR,
        INITIALIZING_CONTROLLER,
        PREVIEW,
		COUNTDOWN,
		RECORDING,
		SHARING,
		FINISH,

		COUNT
	}

	/// <summary>
	/// Permissions listener implementation to interact with Calety's PermissionsManager.
	/// </summary>
	public class PermissionsListener : PermissionsManager.PermissionsListenerBase {
		public AnimojiScreenController parentScreen = null;

		public override void onIOSPermissionResult(PermissionsManager.EIOSPermission ePermission, PermissionsManager.EPermissionStatus eStatus) {
			
			parentScreen.OnIOSPermissionResult(ePermission, eStatus);
		}

		public override void onAndroidPermissionResult(string strPermission, PermissionsManager.EPermissionStatus eStatus) {
			parentScreen.OnAndroidPermissionResult(strPermission, eStatus);
		}
	} 

	private const float TONGUE_REMINDER_TIME = 20f;
	private const float MAX_RECORDING_TIME = 10f;
	private const float COUNTDOWN = 3f;

	// Android permissions
	private const string ANDROID_CAMERA_PERMISSION = "android.permission.CAMERA";    // See https://developer.android.com/reference/android/Manifest.permission#CAMERA
	private const string ANDROID_MICROPHONE_PERMISSION = "android.permission.RECORD_AUDIO";    // See https://developer.android.com/reference/android/Manifest.permission#RECORD_AUDIO

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
	[SerializeField] private ShowHideAnimator m_permissionsErrorGroup = null;

	// Other components
	[Space]
	[SerializeField] private Animator m_countdownAnim = null;
	[SerializeField] private TextMeshProUGUI m_countdownText = null;
	[SerializeField] private GameObject m_recordButton = null;

	[Space]
	[SerializeField] private Slider m_recordingTimeBar = null;

	// Public properties
	private State m_nextState = State.COUNT;	// Using COUNT as "none"
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

	// Permissions handling
	private PermissionsListener m_permissionsListener = null;
	private bool m_microphonePermissionGiven = false;

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
#elif (UNITY_ANDROID)
        return false;
#elif (UNITY_IOS || UNITY_EDITOR_OSX)
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

		// Setup permissions listener
		m_permissionsListener = new PermissionsListener();
		m_permissionsListener.parentScreen = this;
		PermissionsManager.SharedInstance.AddPermissionsListener(m_permissionsListener);
		ControlPanel.Log(Colors.aqua.Tag("[ANIMOJI] PERMISSIONS MANAGER LISTENER INITIALIZED | " + m_permissionsListener));
	}

	/// <summary>
	/// Destructor.
	/// </summary>
	private void OnDestroy() {
        if (ApplicationManager.IsAlive)
        {
            // Clear permissions listener
            PermissionsManager.SharedInstance.RemovePermissionsListener(m_permissionsListener);
            m_permissionsListener = null;
        }
	}

	/// <summary>
	/// Called every frame
	/// </summary>
	private void Update() {
		// If a state change is pending, do it and finish
		if(m_nextState != State.COUNT) {
			State nextState = m_nextState;
			m_nextState = State.COUNT;
			ChangeState(nextState);
			return;
		}

		// Different actions based on current state
		switch(m_state) {
			case State.OFF: {
				// Don't do anything at all!
				return;
			} break;

			case State.CAMERA_PERMISSIONS_REQUEST:
			case State.MICROPHONE_PERMISSIONS_REQUEST: {
				// Wait until permission is granted/denied
			} break;

            case State.INITIALIZING_CONTROLLER: {                
                if (m_animojiSceneController.IsReady) {
                    ChangeStateOnNextFrame(State.PREVIEW);
                }
            } break;

            case State.PREVIEW: {
				// Update tongue reminder timer
				if(m_tongueReminderTimer > 0f) {
					m_tongueReminderTimer -= Time.deltaTime;
				}

				// Show the right UI
				RefreshInfoUI(true);
			} break;

			case State.COUNTDOWN: {
				// Update countdown timer
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
			} break;

			case State.RECORDING: {
				// If player denies permission to record the screen, abort recording
				string errorCode = ParseReplayKitLastError();
				if(!string.IsNullOrEmpty(errorCode)) {
					// Give some feedback
					// Specific message for some error codes
					string message = string.Empty;
					switch(errorCode) {
						case "-5801": {	// Permission denied
							message = string.Empty; // Don't show any message
						} break;

						default: {
							message = LocalizationManager.SharedInstance.Localize("TID_EVENT_RESULTS_UNKNOWN_ERROR") + " (" + errorCode + ")";// "Something went wrong! (-5801)"
						} break;
					}

					// Show feedback
					if(!string.IsNullOrEmpty(message)) {
						UIFeedbackText.CreateAndLaunch(
							message,
							GameConstants.Vector2.center,
							this.GetComponentInParent<Canvas>().transform as RectTransform
						).text.color = Colors.red;
					}
#if UNITY_IOS
                    // Clear ReplayKit
                    ReplayKit.StopRecording();
					ReplayKit.Discard();
#endif

					// Cancel recording (go back to initial state)
					ChangeState(State.PREVIEW);
				} else {
					// No error
					// Update timer
					m_recordingTimer -= Time.deltaTime;

					// Update progress bar
					m_recordingTimeBar.value = m_recordingTimeBar.maxValue - m_recordingTimer;  // Move forward

					// If timer has ended, change state
					if(m_recordingTimer <= 0f) {
						m_recordingTimer = 0f;
						ChangeState(State.SHARING);
					}
				}
			} break;

			case State.SHARING: {
                // Is video file ready?
#if (UNITY_IOS)
                bool recAvailable = ReplayKit.recordingAvailable;
#else
                bool recAvailable = false;
#endif
                if(recAvailable) {
					// Yes! Open native share dialog
					ControlPanel.Log(Colors.paleYellow.Tag("[ANIMOJI] RECORD AVAILABLE!"));
					m_animojiSceneController.ShowPreview();

					// We don't really have a way to know when the native dialog finishes, so instantly move back to the PREVIEW state
					// See https://forum.unity.com/threads/replaykit-detect-preview-controller-finished.450509/
					ChangeState(State.PREVIEW);
				} else {
					// No! Update timeout timer
					m_sharingTimer -= Time.deltaTime;
					if(m_sharingTimer <= 0f) {
						// Timeout! Skip video sharing
						ControlPanel.Log(Colors.red.Tag("[ANIMOJI] SHARING TIME OUT!"));

						// Give some feedback
						UIFeedbackText.CreateAndLaunch(
							LocalizationManager.SharedInstance.Localize("TID_EVENT_RESULTS_UNKNOWN_ERROR"),	// "Something went wrong!"
							GameConstants.Vector2.center,
							this.GetComponentInParent<Canvas>().transform as RectTransform
						).text.color = Colors.red;

						// Go to initial state
						ChangeState(State.PREVIEW);
					}
				}
			} break;
		}
	}

	/// <summary>
	/// Change logic state!
	/// </summary>
	/// <param name="_newState">State to change to.</param>
	private void ChangeState(State _newState) {
		ControlPanel.Log(Colors.paleYellow.Tag("[ANIMOJI] Changing state from " + m_state + " to " + _newState));

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

				// Go to next state after a frame
				ChangeStateOnNextFrame(State.CAMERA_PERMISSIONS_REQUEST);

                // Notify animoji tracking event start
                HDTrackingManagerImp.Instance.Notify_AnimojiStart();
			} break;

			case State.PREVIEW: {
                m_animojiSceneController.onFaceAdded.AddListener(OnFaceDetected);
                m_animojiSceneController.onTongueLost.AddListener(OnTongueLost);

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
				SelectUI(false);	// No animation so no UI is recorded during a fade animation

				// Reset timer and wait for timeout
				m_recordingTimer = MAX_RECORDING_TIME;

				// Tell the controller to start recording
				try {
					m_animojiSceneController.StartRecording(m_microphonePermissionGiven);
				} catch(Exception _e) {
					ControlPanel.Log(Colors.red.Tag("[ANIMOJI] START RECORDING EXCEPTION: " + _e.ToString()));
				}
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
#if (UNITY_IOS)
                ReplayKit.Discard ();
#endif

				// Toggle views
				SelectUI(true);

				// Turn game back on
				ToggleMainCameras(true);

				// Unsubscribe from external events
				if(m_animojiSceneController != null) {
					m_animojiSceneController.onFaceAdded.RemoveListener(OnFaceDetected);
					m_animojiSceneController.onTongueLost.RemoveListener(OnTongueLost);
				}
				
				// Destroy the different AR components
				if(m_unityARVideo != null) {
					ControlPanel.Log("[ANIMOJI] Destroying UnityARVideo component");
					GameObject.Destroy(m_unityARVideo.gameObject);
					m_unityARVideo = null;
				}

				if(m_animojiSceneController != null) {
					if(m_animojiSceneController.m_dragonAnimojiInstance != null) {
						ControlPanel.Log("[ANIMOJI] Destroying DragonAnimoji component");
						GameObject.Destroy(m_animojiSceneController.m_dragonAnimojiInstance.gameObject);
						m_animojiSceneController.m_dragonAnimojiInstance = null;
					}
					
					ControlPanel.Log("[ANIMOJI] Destroying HDTongueDetector component");
					GameObject.Destroy(m_animojiSceneController.gameObject);
					m_animojiSceneController = null;
				}

				if(m_animojiSceneInstance != null) {
					ControlPanel.Log("[ANIMOJI] Destroying PF_AnimojiSceneSetup root");
					GameObject.Destroy(m_animojiSceneInstance);
					m_animojiSceneInstance = null;
				}

				if(m_unityARFaceAnchorManager != null) {
					ControlPanel.Log("[ANIMOJI] Destroying UnityARFaceAnchorManager component");
					GameObject.Destroy(m_unityARFaceAnchorManager.gameObject);
					m_unityARFaceAnchorManager = null;
				}

				if(m_ARCameraTracker != null) {
					ControlPanel.Log("[ANIMOJI] Destroying ARCameraTracker component");
					GameObject.Destroy(m_ARCameraTracker.gameObject);
					m_ARCameraTracker = null;
				}

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

                // Notify animoji tracking event exit
                HDTrackingManagerImp.Instance.Notify_AnimojiExit();

				// Go to OFF state after some delay
				UbiBCN.CoroutineManager.DelayedCall(() => {
//					GameObject.Destroy(m_animojiSceneInstance);
//					m_animojiSceneInstance = null;
					ChangeState(State.OFF);
					ControlPanel.Log("[ANIMOJI] Animoji screen controller: delayed call : changestate(OFF);");
				}, 0.25f);
				ControlPanel.Log("[ANIMOJI] Animoji screen controller: Finish state end");
			} break;

			case State.CAMERA_PERMISSIONS_REQUEST: {
				// Toggle views
				SelectUI(true);

				// Check current permission status and act accordingly
				ProcessCameraPermission();
			} break;

			case State.MICROPHONE_PERMISSIONS_REQUEST: {
				// Toggle views
				SelectUI(true);

				// Check current permission status and act accordingly
				ProcessMicrophonePermission();
			} break;

		case State.PERMISSIONS_OK: {
				// Toggle views
				SelectUI(true);

				// Load Animoji Scene
				GameObject scenePrefab = Resources.Load<GameObject>(HDTongueDetector.SCENE_PREFAB_PATH);
				Debug.Assert(scenePrefab != null, "COULDN'T LOAD ANIMOJI SCENE PREFAB (" + HDTongueDetector.SCENE_PREFAB_PATH + ")", this);

				// Instantiate it
				m_animojiSceneInstance = GameObject.Instantiate<GameObject>(scenePrefab);

				// Get animoji controller reference
				m_animojiSceneController = m_animojiSceneInstance.GetComponentInChildren<HDTongueDetector>();
				Debug.Assert(m_animojiSceneController != null, "Couldn't find HDTongueDetector!", this);

				m_unityARVideo = m_animojiSceneInstance.GetComponentInChildren<UnityARVideo>();
				Debug.Assert(m_unityARVideo != null, "Couldn't find UnityARVideo", this);

				m_ARCameraTracker = m_animojiSceneInstance.GetComponentInChildren<ARCameraTracker>();
				Debug.Assert(m_ARCameraTracker != null, "Couldn't find UnityARVideo", this);

				m_unityARFaceAnchorManager = m_animojiSceneInstance.GetComponentInChildren<UnityARFaceAnchorManager>();
				Debug.Assert(m_unityARFaceAnchorManager != null, "Couldn't find UnityARFaceAnchorManager", this);
				
                // Go to next state after a frame                
                ChangeStateOnNextFrame(State.INITIALIZING_CONTROLLER);
            } break;

            case State.INITIALIZING_CONTROLLER: {
                m_animojiSceneController.InitWithDragon(InstanceManager.menuSceneController.selectedDragon);                
            } break;            

            case State.PERMISSIONS_ERROR: {
				// Toggle views
				SelectUI(true);
			} break;
		}
	}    

	/// <summary>
	/// Change the logic state on the next frame.
	/// If another state change was pending, it will be overriden.
	/// </summary>
	/// <param name="_newState">State to change to.</param>
	private void ChangeStateOnNextFrame(State _newState) {
		m_nextState = _newState;
	}

	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Refresh the info UI based on face detection state.
	/// </summary>
	/// <param name="_animate">Perform animations?</param>
	private void RefreshInfoUI(bool _animate) {
		// Check conditions
		bool faceDetected = m_animojiSceneController.faceDetected;
		bool tongueDetected = m_animojiSceneController.tongueDetected;
		bool tongueReminderTimeout = m_tongueReminderTimer <= 0f;

		// Apply
		m_faceNotDetectedGroup.Set(
			!faceDetected && 
			m_state == State.PREVIEW,	// Not while recording (or near it)
			_animate
		);

		m_tongueReminderGroup.Set(
			faceDetected && 
			!tongueDetected && 
			tongueReminderTimeout &&
			m_state != State.RECORDING,		// Not while recording!
			_animate
		);

#if UNITY_IOS
        string lastError = ReplayKit.lastError;
#else
        string lastError = "";
#endif
        // Don't allow recording if ReplayKit is reporting an error (most likely permission denied)
        m_recordButton.SetActive(string.IsNullOrEmpty(lastError));
    }

    /// <summary>
    /// Show/hide UI groups based on current states.
    /// </summary>
    /// <param name="_animate">Perform animations?</param>
    private void SelectUI(bool _animate) {
		// Face not detected warning and tongue reminder
		if(m_state == State.PREVIEW || m_state == State.RECORDING) {
			RefreshInfoUI(_animate);
		} else {
			m_faceNotDetectedGroup.ForceHide(_animate);
			m_tongueReminderGroup.ForceSet(m_state == State.COUNTDOWN, _animate);	// Always show tongue reminder in COUNTDOWN state
		}

		// Loading screen
		m_busyGroup.Set(
			m_state == State.INIT || 
			m_state == State.FINISH ||
			m_state == State.CAMERA_PERMISSIONS_REQUEST ||
			m_state == State.MICROPHONE_PERMISSIONS_REQUEST ||
			m_state == State.PERMISSIONS_OK
			, _animate
		);

		// Preview mode
		m_previewModeGroup.Set(m_state == State.PREVIEW, _animate);
		
		// Countdown UI
		m_countdownGroup.Set(m_state == State.COUNTDOWN, _animate);

		// Recording mode
		m_recordingModeGroup.Set(m_state == State.RECORDING, _animate);

		// Permissions error
		m_permissionsErrorGroup.Set(m_state == State.PERMISSIONS_ERROR, _animate);
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
		if(_seconds >= 0) {	// Don't show 0
			m_countdownText.text = StringUtils.FormatNumber(
				Mathf.FloorToInt(_seconds) + 1
			);
		} else {
			m_countdownText.text = string.Empty;
		}

		// Trigger anim
		m_countdownAnim.SetTrigger("launch");
	}

	/// <summary>
	/// Parses the replay kit last error.
	/// </summary>
	/// <returns>Last parsed error code. Empty string if no error or error unknown.</returns>
	private string ParseReplayKitLastError() {
        // Get last error
#if UNITY_IOS
        string lastError = ReplayKit.lastError;
#else
        string lastError = "";
#endif
        ControlPanel.Log(Colors.paleYellow.Tag("[ANIMOJI] Replay Kit lastError: " + lastError));
//		ControlPanel.Log(Colors.paleYellow.Tag("[ANIMOJI] Replay Kit isRecording: " + ReplayKit.isRecording));

		// Protect from null
		if(string.IsNullOrEmpty(lastError)) {
			return string.Empty;
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
		return string.Empty;
	}

	//------------------------------------------------------------------------//
	// PERMISSION METHODS													  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Decide what to do based on current camera permission status.
	/// </summary>
	/// <param name="_requestAllowed">Allow requesting permission if not defined.</param>
	private void ProcessCameraPermission(bool _requestAllowed = true) {
		// Get current permission status
		switch(GetCameraPermission()) {
			case PermissionsManager.EPermissionStatus.E_PERMISSION_RESTRICTED:
			case PermissionsManager.EPermissionStatus.E_PERMISSION_GRANTED: {
				// Check mic permission
				ChangeStateOnNextFrame(State.MICROPHONE_PERMISSIONS_REQUEST);
			} break;

			case PermissionsManager.EPermissionStatus.E_PERMISSION_DENIED: {
				// Show error message
				ChangeStateOnNextFrame(State.PERMISSIONS_ERROR);
			} break;

			case PermissionsManager.EPermissionStatus.E_PERMISSION_NOT_DETERMINED: {
				// Request permission (if allowed)
				if(_requestAllowed) {
					RequestCameraPermission();
				} else {
					ChangeStateOnNextFrame(State.PERMISSIONS_ERROR);
				}
			} break;
		}
	}

	/// <summary>
	/// Get current status of the Camera permission depending on current platform.
	/// </summary>
	/// <returns>The camera permission status.</returns>
	private PermissionsManager.EPermissionStatus GetCameraPermission() {
		PermissionsManager.EPermissionStatus permissionStatus = PermissionsManager.EPermissionStatus.E_PERMISSION_NOT_DETERMINED;
#if UNITY_IOS
		permissionStatus = PermissionsManager.SharedInstance.GetIOSPermissionStatus(PermissionsManager.EIOSPermission.Camera);
#elif UNITY_ANDROID
		permissionStatus = PermissionsManager.SharedInstance.GetAndroidPermissionStatus(ANDROID_CAMERA_PERMISSION);
#endif
		ControlPanel.Log(Colors.aqua.Tag("[ANIMOJI] Camera Permission: " + permissionStatus));
		return permissionStatus;
	}

	/// <summary>
	/// Request permission to use the camera in the current platform.
	/// OnCameraPermission() callback will be invoked when done.
	/// </summary>
	private void RequestCameraPermission() {
		ControlPanel.Log(Colors.aqua.Tag("[ANIMOJI] Requesting Camera Permission..."));
#if UNITY_IOS
		PermissionsManager.SharedInstance.RequestIOSPermission(PermissionsManager.EIOSPermission.Camera);
#elif UNITY_ANDROID
		PermissionsManager.SharedInstance.RequestAndroidPermission(ANDROID_CAMERA_PERMISSION);
#endif
	} 

	/// <summary>
	/// Decide what to do based on current microphone permission status.
	/// </summary>
	/// <param name="_requestAllowed">Allow requesting permission if not defined.</param>
	private void ProcessMicrophonePermission(bool _requestAllowed = true) {
		// [AOC] Microphone permission is not blocker, the video will just get recorded without audio
		switch(GetMicrophonePermission()) {
			case PermissionsManager.EPermissionStatus.E_PERMISSION_RESTRICTED:
			case PermissionsManager.EPermissionStatus.E_PERMISSION_GRANTED: {
				m_microphonePermissionGiven = true;
			} break;

			case PermissionsManager.EPermissionStatus.E_PERMISSION_DENIED: {
				m_microphonePermissionGiven = false;
			} break;

			case PermissionsManager.EPermissionStatus.E_PERMISSION_NOT_DETERMINED: {
				// Request permission (if allowed)
				if(_requestAllowed) {
					RequestMicrophonePermission();
					return;	// Don't change state!
				} else {
					m_microphonePermissionGiven = false;
				}
			} break;
		}

		// We're good to go!
		ChangeStateOnNextFrame(State.PERMISSIONS_OK);
	}

	/// <summary>
	/// Get current status of the Microphone permission depending on current platform.
	/// </summary>
	/// <returns>The microphone permission status.</returns>
	private PermissionsManager.EPermissionStatus GetMicrophonePermission() {
		PermissionsManager.EPermissionStatus permissionStatus = PermissionsManager.EPermissionStatus.E_PERMISSION_NOT_DETERMINED;
#if UNITY_IOS
		permissionStatus = PermissionsManager.SharedInstance.GetIOSPermissionStatus(PermissionsManager.EIOSPermission.Microphone);
#elif UNITY_ANDROID
		permissionStatus = PermissionsManager.SharedInstance.GetAndroidPermissionStatus(ANDROID_MICROPHONE_PERMISSION);
#endif
		ControlPanel.Log(Colors.aqua.Tag("[ANIMOJI] Microphone Permission: " + permissionStatus));
		return permissionStatus;
	}

	/// <summary>
	/// Request permission to use the microphone in the current platform.
	/// OnMicrophonePermission() callback will be invoked when done.
	/// </summary>
	private void RequestMicrophonePermission() {
		ControlPanel.Log(Colors.aqua.Tag("[ANIMOJI] Requesting Microphone Permission..."));
#if UNITY_IOS
		PermissionsManager.SharedInstance.RequestIOSPermission(PermissionsManager.EIOSPermission.Microphone);
#elif UNITY_ANDROID
		PermissionsManager.SharedInstance.RequestAndroidPermission(ANDROID_MICROPHONE_PERMISSION);
#endif
	}

	/// <summary>
	/// We've received a permission change for iOS.
	/// </summary>
	/// <param name="_permission">Permission that has been modified.</param>
	/// <param name="_status">New permission status.</param>
	private void OnIOSPermissionResult(PermissionsManager.EIOSPermission _permission, PermissionsManager.EPermissionStatus _status) {
		ControlPanel.Log(Colors.coral.Tag("[ANIMOJI] ON IOS PERMISSION RESULT | " + _permission + " | " + _status));

		// Which permission has been changed?
		switch(_permission) {
			case PermissionsManager.EIOSPermission.Camera: {
				ProcessCameraPermission(false);	// Prevent going into an infinite permission request loop
			} break;

			case PermissionsManager.EIOSPermission.Microphone: {
				ProcessMicrophonePermission(false);	// Prevent going into an infinite permission request loop
			} break;
		}
	}

	/// <summary>
	/// We've received a permission change for Android.
	/// </summary>
	/// <param name="_permission">Permission that has been modified.</param>
	/// <param name="_status">New permission status.</param>
	private void OnAndroidPermissionResult(string _permission, PermissionsManager.EPermissionStatus _status) {
	ControlPanel.Log(Colors.coral.Tag("[ANIMOJI] ON ANDROID PERMISSION RESULT | " + _permission + " | " + _status));

		// Which permission has been changed?
		switch(_permission) {
			case ANDROID_CAMERA_PERMISSION: {
				ProcessCameraPermission(false);	// Prevent going into an infinite permission request loop
			} break;

			case ANDROID_MICROPHONE_PERMISSION: {
				ProcessMicrophonePermission(false);	// Prevent going into an infinite permission request loop
			} break;
		}
	}

	/// <summary>
	/// Go to device settings.
	/// </summary>
	public void OnPermissionSettingsButton() {
		// Go to device settings (after some delay to give time for the flow to be finished)
		UbiBCN.CoroutineManager.DelayedCall(() => {
			// [AOC] Calety does it for us! :)
			PermissionsManager.SharedInstance.OpenPermissionSettings();
		}, 1f);

		// Go back to previous screen to restart permissions settings
		OnBackButton();
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
//#elif UNITY_ANDROID
//#endif
}