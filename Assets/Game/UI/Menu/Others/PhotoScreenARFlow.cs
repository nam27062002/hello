// PhotoScreenARFlow.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 01/08/2018.
// Copyright (c) 2018 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

using System.Collections.Generic;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Based on IARSurface.cs, control the AR flow in the Photo Screen.
/// </summary>
public class PhotoScreenARFlow : NavigationScreenSystem {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	public const float DEFAULT_ZOOM_VALUE = 2.0f;

	public enum State {
		OFF,

		INIT,
		DETECTING_SURFACE,
		DETECTED_SURFACE,
		FINISH,

		COUNT
	}

	public enum Screen {
		INIT,
		DETECTING_SURFACE,
		DETECTED_SURFACE,

		COUNT
	}

	public class StateChangedEvent : UnityEvent<State, State> { }

	//------------------------------------------------------------------------//
	// EVENTS																  //
	//------------------------------------------------------------------------//
	public StateChangedEvent onStateChanged = new StateChangedEvent();
	public UnityEvent onExit = new UnityEvent();
	public UnityEvent onTakePicture = new UnityEvent();

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed References
	[Separator]
	[SerializeField] private GameObject m_zoomIndicator = null;
	[SerializeField] private ShowHideAnimator m_confirmSurfaceButtonAnim = null;
	[SerializeField] private GameObject m_background = null;
	//[SerializeField] private Localizer m_infoText = null;

	// Public properties
	private State m_state = State.OFF;
	public State state {
		get { return m_state; }
	}

	private MenuDragonLoader m_dragonLoader = null;
	public MenuDragonLoader dragonLoader {
		get { return m_dragonLoader; }
	}

	// Internal logic
	private float m_stateTimer = 0f;

	// Internal references
	private PhotoScreenAR.ARGameListener m_arGameListener = null;
	private Camera[] m_mainSceneCameras = null;
	private Camera[] m_contentCameras = null;

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
	/// Called every frame.
	/// </summary>
	private void Update() {
		// Update timer
		if(m_stateTimer > 0f) {
			m_stateTimer -= Time.deltaTime;
		}

		// Based on current state
		switch(m_state) {
			case State.DETECTING_SURFACE: {
				// Do we have a valid surface?
				if(ARKitManager.SharedInstance.AreARSurfacesFound()) {
					// Show confirm surface if the surface is valid
					m_confirmSurfaceButtonAnim.ForceSet(
						ARKitManager.SharedInstance.IsPossibleARPivotSet()
					);

					// Hide zoom indicator
					m_zoomIndicator.SetActive(false);
				} else {
					// Hide button and show zoom indicator
					m_confirmSurfaceButtonAnim.ForceHide();
					m_zoomIndicator.SetActive(true);
				}
			} break;

			case State.FINISH: {
				if(m_stateTimer <= 0f) {
					// Close the AR session
					ARKitManager.SharedInstance.FinishingARSession();

					// Finalize AR Game Manager
					ARGameManager.SharedInstance.UnInitialise();

					// Restore affected objects
					if(ARKitManager.s_pInstance != null) {
						ARKitManager.SharedInstance.ResetAffectedARObjectsTransform();
					}

					// Restore main cameras
					ToggleMainCameras(true);

					// Unload dragon preview
					if(m_dragonLoader != null) {
						m_dragonLoader.UnloadDragon();
					}

					// Go to OFF state
					ChangeState(State.OFF);
				}
			} break;
		}
	}

	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Starts the AR flow.
	/// </summary>
	public void StartFlow() {
		// Enable ourselves
		this.gameObject.SetActive(true);

		// Do it!
		ChangeState(State.INIT);
	}

	/// <summary>
	/// Ends the AR flow.
	/// </summary>
	public void EndFlow() {
		// Ignore if already ending
		if(m_state == PhotoScreenARFlow.State.FINISH) return;

		// Go to finish state.
		ChangeState(State.FINISH);
	}

	/// <summary>
	/// Change logic state!
	/// </summary>
	/// <param name="_newState">State to change to.</param>
	private void ChangeState(State _newState) {
		// Stuff to do when leaving a state
		switch(m_state) {
			default: {
				
			} break;
		}

		// Perform state change
		State oldState = m_state;
		m_state = _newState;

		// Stuff to do when entering a state
		switch(m_state) {
			case State.OFF: {
				// Hide ourselves
				GoToScreen(NavigationScreenSystem.SCREEN_NONE);
				this.gameObject.SetActive(false);
			} break;

			case State.INIT: {
				// Toggle target screen
				GoToScreen((int)Screen.INIT);

				// Listen to AR Game Manager events
				if(m_arGameListener == null) {
					m_arGameListener = new PhotoScreenAR.ARGameListener(this);
					ARGameManager.SharedInstance.SetListener(m_arGameListener);
				}

				// If ARKit is not initialized, do it now!
				if(ARKitManager.SharedInstance.GetARState() != ARKitManager.eARState.E_AR_PLAYING) {
					ARGameManager.SharedInstance.Initialise();
					ARGameManager.SharedInstance.onPressedButtonAR();
				}
			} break;

			case State.DETECTING_SURFACE: {
				// Toggle target screen
				GoToScreen((int)Screen.DETECTING_SURFACE);

				// Disable main cameras
				ToggleMainCameras(false);
				ToggleContentCameras(false);

				// Hide affected objects
				ARKitManager.SharedInstance.SetAffectedARObjectsEnabled(false);

				// Notify AR manager
				ARKitManager.SharedInstance.StartSurfaceDetection();
			} break;

			case State.DETECTED_SURFACE: {
				// Toggle target screen
				GoToScreen((int)Screen.DETECTED_SURFACE);

				// Fix surface
				ARKitManager.SharedInstance.SelectCurrentPositionAsARPivot();

				// Show content!
				ToggleContentCameras(true);
				ARKitManager.SharedInstance.SetAffectedARObjectsEnabled(true);
				ARKitManager.SharedInstance.ResetAffectedARObjectsTransform();
			} break;

			case State.FINISH: {
				// Hide ourselves after a short delay
				m_stateTimer = 0.25f;
			} break;
		}

		// Notify listeners
		onStateChanged.Invoke(oldState, m_state);
	}

	//------------------------------------------------------------------------//
	// INTERNAL UTILS														  //
	//------------------------------------------------------------------------//
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
	/// Enable/disable AR content cameras.
	/// </summary>
	/// <param name="_enabled">Toggle on or off?.</param>
	private void ToggleContentCameras(bool _enabled) {
		if(m_contentCameras != null) {
			for(int i = 0; i < m_contentCameras.Length; ++i) {
				if(m_contentCameras[i] != null) {
					m_contentCameras[i].enabled = _enabled;
				}
			}
		}
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Exit button has been pressed.
	/// </summary>
	public void OnExitButton() {
		// Notify listeners
		onExit.Invoke();
	}

	/// <summary>
	/// The take picture button has been pressed.
	/// </summary>
	public void OnTakePictureButton() {
		// Tell AR Manager we've fixed the zoom
		ARKitManager.SharedInstance.SelectedZoom();

		// Notify listeners
		onTakePicture.Invoke();
	}

	/// <summary>
	/// The confirm surface button has been pressed.
	/// </summary>
	public void OnConfirmSurfaceButton() {
		// Change state
		ChangeState(State.DETECTED_SURFACE);
	}

	/// <summary>
	/// The change surface button has been pressed.
	/// </summary>
	public void OnChangeSurfaceButton() {
		// Change state
		ChangeState(State.DETECTING_SURFACE);
	}

	/// <summary>
	/// The camera permission has been set.
	/// </summary>
	/// <param name="_cameraPermissionGranted">Whether we can use the camera or not.</param>
	public void OnCameraPermission(bool _cameraPermissionGranted) {
		// Is camera permission granted?
		if(_cameraPermissionGranted) {
			// Tell the AR Game Manager which objects to hide
			List<GameObject> hiddenARObjects = new List<GameObject>();
			//kHiddenARObjects.Add(GameObject.Find("MenuScene3D/PF_MenuCameraSetup/Camera3D"));
			hiddenARObjects.Add(InstanceManager.menuSceneController.mainCamera.gameObject);
			ARKitManager.SharedInstance.SetHiddenARObjects(hiddenARObjects);

			// Tell the AR Game manager which objects to manipulate
			List<GameObject> affectedARObjects = new List<GameObject>();
			GameObject arena = GameObject.Find("MenuScene3D/ARBasePrefab/Arena");
			if(arena != null) {
				// Initialize with current dragon
				m_dragonLoader = arena.FindComponentRecursive<MenuDragonLoader>();
				if(m_dragonLoader != null) {
					m_dragonLoader.LoadDragon(InstanceManager.menuSceneController.selectedDragon);
				}
				affectedARObjects.Add(arena);

				// Find content cameras as well
				m_contentCameras = arena.transform.parent.GetComponentsInChildren<Camera>();
			}
			ARKitManager.SharedInstance.SetAffectedARObjects(affectedARObjects, 0.025f);

			// Go to next step
			ChangeState(State.DETECTING_SURFACE);
		} else {
			// Cancel the whole flow
			EndFlow();
		}
	}
}