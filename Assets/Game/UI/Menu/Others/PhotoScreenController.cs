// PhotoScreenController.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 01/03/2017.
// Copyright (c) 2017 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Main controller for the photo menu screen.
/// </summary>
public class PhotoScreenController : MonoBehaviour {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	// Should be set before navigating to the screen
	public enum Mode {
		DRAGON = 0,
		EGG_REWARD,

		COUNT
	}

	[Serializable]
	public class ModeSetup {
		public GameObject uiContainer = null;
		public DragControlRotation dragControl = null;
		public DragControlZoom zoomControl = null;
	}
	
	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed
	[SerializeField] private ModeSetup[] m_modes = new ModeSetup[(int)Mode.COUNT];

	[Separator("Shared Objects")]
	[SerializeField] private GameObject m_bottomBar = null;

	[Separator("AR and Animoji")]
	[SerializeField] private GameObject m_arButton = null;
	[SerializeField] private PhotoScreenARFlow m_arFlow = null;
	[SerializeField] private GameObject m_animojiButton = null;

    [Separator("Others")]
    [SerializeField] private AssetsDownloadFlow m_assetsDownloadFlow = null;


    // Public properties
    private Mode m_mode = Mode.DRAGON;
	public Mode mode {
		get { return m_mode; }
		set { SetMode(value); }
	}

	// AR Internal
	private bool m_isARAvailable = false;
	private bool m_isAnimojiAvailable = false;

	// Internal references
	private Canvas m_rootUICanvas = null;

	// Internal properties
	private ModeSetup currentMode {
		get { return m_modes[(int)m_mode]; }
	}

	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {
		// Store reference to root UI canvas to disable it when taking a screen capture
		m_rootUICanvas = this.GetComponentInParent<Canvas>();

		// Is AR available?
		m_isARAvailable = false;
#if(UNITY_IOS || UNITY_ANDROID || UNITY_EDITOR_OSX)
		if(ARKitManager.SharedInstance.IsARKitAvailable()) {
			m_isARAvailable = true;
		}
#endif

		// Subscribe to AR events
		if(m_isARAvailable) {
			m_arFlow.onTakePicture.AddListener(OnARTakePicture);
			m_arFlow.onStateChanged.AddListener(OnARStateChanged);
			m_arFlow.onExit.AddListener(OnARExit);
		}
		m_arFlow.gameObject.SetActive(false);

		// Apply initial mode
		SetMode(m_mode);

		// Subscribe to external events
		Messenger.AddListener<MenuScreen, MenuScreen>(MessengerEvents.MENU_SCREEN_TRANSITION_START, OnMenuScreenTransitionStart);
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
	/// Called every frame
	/// </summary>
	private void Update() {

	}

	/// <summary>
	/// Destructor.
	/// </summary>
	private void OnDestroy() {
		// Unsubscribe to external events
		Messenger.AddListener<MenuScreen, MenuScreen>(MessengerEvents.MENU_SCREEN_TRANSITION_START, OnMenuScreenTransitionStart);
	}

	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Change screen mode.
	/// </summary>
	/// <param name="_mode">New mode.</param>
	private void SetMode(Mode _mode) {
		// Toggle stuff on/off
		bool active = false;
		for(int i = 0; i < (int)Mode.COUNT; i++) {
			active = (i == (int)_mode);
			if(m_modes[i].uiContainer != null) m_modes[i].uiContainer.SetActive(active);
			if(m_modes[i].dragControl != null) m_modes[i].dragControl.gameObject.SetActive(active);
			if(m_modes[i].zoomControl != null) m_modes[i].zoomControl.gameObject.SetActive(active);
		}

		// Make sure bottom bar is active
		m_bottomBar.SetActive(true);

		// Store new mode
		m_mode = _mode;
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Screen is about to be open.
	/// </summary>
	/// <param name="_animator">The animator that triggered the event.</param>
	public void OnShowPreAnimation(ShowHideAnimator _animator) {
		// Aux vars
		MenuSceneController menuController = InstanceManager.menuSceneController;

		// Disable drag controller
		currentMode.dragControl.gameObject.SetActive(false);
		currentMode.zoomControl.gameObject.SetActive(false);

        // Initialize AR stuff
        m_arButton.SetActive(m_mode == Mode.DRAGON && m_isARAvailable);
		m_arFlow.gameObject.SetActive(false);

		// Allow animoji?
		m_isAnimojiAvailable = (m_mode == Mode.DRAGON) && AnimojiScreenController.IsSupported(InstanceManager.menuSceneController.selectedDragon);
		m_animojiButton.SetActive(m_isAnimojiAvailable);

        //OTA: Initialize download flow with handle for ALL assets
        m_assetsDownloadFlow.InitWithHandle(HDAddressablesManager.Instance.GetHandleForAllDownloadables());

    }

	/// <summary>
	/// The screen has just finished the open animation.
	/// </summary>
	/// <param name="_animator">The animator that triggered the event.</param>
	public void OnShowPostAnimation(ShowHideAnimator _animator) {
		// Aux vars
		MenuSceneController menuController = InstanceManager.menuSceneController;

		// Initialize drag controller with a target based on current mode
		currentMode.dragControl.gameObject.SetActive(true);
		switch(m_mode) {
			case Mode.DRAGON: {
				// Initialize with current dragon preview
				currentMode.dragControl.target = menuController.selectedDragonPreview.transform;
			} break;

			case Mode.EGG_REWARD: {
				// Initialize with egg reward view
				MenuScreenScene scene3D = menuController.GetScreenData(MenuScreen.OPEN_EGG).scene3d;
				RewardSceneController sceneController = scene3D.GetComponent<RewardSceneController>();
				currentMode.dragControl.target = sceneController.currentRewardSetup.view.transform;

				// Disable godrays for photo!
				if(sceneController.currentRewardSetup.godrays != null) {
					sceneController.currentRewardSetup.godrays.gameObject.SetActive(false);
				}
			} break;
		}

		// Initialize zoom controller with main camera
		currentMode.zoomControl.gameObject.SetActive(true);
		currentMode.zoomControl.camera = null;	// [AOC] Force refresh of camera initial values
		currentMode.zoomControl.camera = menuController.mainCamera;
	}

	/// <summary>
	/// The screen is about to hide.
	/// </summary>
	/// <param name="_animator">The animator that triggered the event.</param>
	public void OnHidePreAnimation(ShowHideAnimator _animator) {
		// Disable drag controller
		currentMode.dragControl.gameObject.SetActive(false);
		currentMode.zoomControl.gameObject.SetActive(false);

		// Restore rarity godrays!
		if(m_mode == Mode.EGG_REWARD) {
			MenuSceneController menuController = InstanceManager.menuSceneController;
			MenuScreenScene scene3D = menuController.GetScreenData(MenuScreen.OPEN_EGG).scene3d;
			RewardSceneController sceneController = scene3D.GetComponent<RewardSceneController>();
			if(sceneController.currentRewardSetup != null && sceneController.currentRewardSetup.godrays != null) {
				sceneController.currentRewardSetup.godrays.gameObject.SetActive(true);
			}
		}
	}

	/// <summary>
	/// Take the picture!
	/// </summary>
	public void OnTakePictureButton() {
        if (!ButtonExtended.checkMultitouchAvailability())
            return;

		// [AOC] New System
		// A bit hacky: check previous screen to figure out which share data to use
		string shareLocationSku = "dragon";
		if(InstanceManager.menuSceneController.transitionManager.prevScreen == MenuScreen.DRAGON_UNLOCK) {
			shareLocationSku = "dragon_acquired";
		}

		// Get the share screen instance and initialize it with current data
		IDragonData dragonData = DragonManager.GetDragonData(InstanceManager.menuSceneController.selectedDragon);
		ShareScreenDragon shareScreen = ShareScreensManager.GetShareScreen(shareLocationSku) as ShareScreenDragon;
		shareScreen.Init(
			shareLocationSku,
			SceneController.GetMainCameraForCurrentScene(),
			dragonData,
			false,
			null
		);
		shareScreen.TakePicture(IShareScreen.CaptureMode.RENDER_TEXTURE);
	}

    /// <summary>
    /// The back button has been pressed.
    /// </summary>
    public void OnBackButton() {
		if (!ButtonExtended.checkMultitouchAvailability ())
			return;				

        // Ignore if we are in AR
        if (!m_arFlow.isActiveAndEnabled) {
			// Go back to previous menu screen
			InstanceManager.menuSceneController.transitionManager.Back(true);
		}
    }

    //------------------------------------------------------------------------//
    // AR CALLBACKS															  //
    //------------------------------------------------------------------------//
    /// <summary>
    /// The AR button has been pressed.
    /// </summary>
    public void OnARButton() {
		if (!ButtonExtended.checkMultitouchAvailability ())
			return;

        // Check for assets download for this specific dragon
        string dragonSKU = InstanceManager.menuSceneController.selectedDragon;
        Downloadables.Handle dragonHandle = HDAddressablesManager.Instance.GetHandleForClassicDragon(dragonSKU);
        if (!dragonHandle.IsAvailable())
        {
            // Initialize download flow with handle for ALL assets
            m_assetsDownloadFlow.InitWithHandle(HDAddressablesManager.Instance.GetHandleForAllDownloadables());
            m_assetsDownloadFlow.OpenPopupByState(PopupAssetsDownloadFlow.PopupType.ANY);

            return;
        }


        // Start AR flow
        if (!m_arFlow.isActiveAndEnabled)
        {
            // Hide bottom bar
            m_bottomBar.gameObject.SetActive(false);

            currentMode.dragControl.gameObject.SetActive(false);
            currentMode.zoomControl.gameObject.SetActive(false);

            // Do it!
            m_arFlow.StartFlow();
        }

    }

    /// <summary>
    /// AR flow wants to finish.
    /// </summary>
    private void OnARExit() {
        // Terminate AR flow
        m_arFlow.EndFlow();
    }

    /// <summary>
    /// AR flow wants to take a picture.
    /// </summary>
    private void OnARTakePicture() {
		// Get the share screen instance and initialize it with current data
		ControlPanel.Log(Color.yellow.Tag("AR TAKE PICTURE"));
		IDragonData dragonData = DragonManager.GetDragonData(InstanceManager.menuSceneController.selectedDragon);
		ShareScreenDragon shareScreen = ShareScreensManager.GetShareScreen("dragon") as ShareScreenDragon;
		shareScreen.Init(
			"dragon",
			null,   // We are gonna be using the SCREEN_CAPTURE method, so we don't need to provide a reference camera
			dragonData,
			false,
			null
		);

		// Disable UI camera so we don't capture any UI whatsoever
		m_rootUICanvas.worldCamera.enabled = false;

		// Take the picture!
		shareScreen.TakePicture(IShareScreen.CaptureMode.SCREEN_CAPTURE);

		// Restore UI camera
		UbiBCN.CoroutineManager.DelayedCallByFrames(
			() => { 
				m_rootUICanvas.worldCamera.enabled = true; 
			}, 5    // Give enough time to take the screenshot
		);   
	}

    /// <summary>
    /// AR flow has changed its state.
    /// </summary>
    /// <param name="_oldState">Old state.</param>
    /// <param name="_newState">New state.</param>
    private void OnARStateChanged(PhotoScreenARFlow.State _oldState, PhotoScreenARFlow.State _newState) {
		// Don't show dragon info while detecting the surface
		currentMode.uiContainer.SetActive(_newState != PhotoScreenARFlow.State.DETECTING_SURFACE);

		// Don't show bottom bar or drag controls while AR is active
		bool arOff = _newState == PhotoScreenARFlow.State.FINISH || _newState == PhotoScreenARFlow.State.OFF;
		m_bottomBar.gameObject.SetActive(arOff);
		currentMode.dragControl.gameObject.SetActive(arOff);
		currentMode.zoomControl.gameObject.SetActive(arOff);
	}

	/// <summary>
	/// The menu screen change animation is about to start.
	/// </summary>
	/// <param name="_from">Screen we come from.</param>
	/// <param name="_to">Screen we're going to.</param>
	private void OnMenuScreenTransitionStart(MenuScreen _from, MenuScreen _to) {
		// Check params
        if (_to == MenuScreen.NONE) return;

        // Aux vars
        MenuSceneController menuSceneController = InstanceManager.menuSceneController;
		MenuScreenScene toScene = menuSceneController.GetScreenData(_to).scene3d;

		// If the scene we are entering has a photo snap point to override, do it now
		if(toScene != null) {
			// Aux vars
			CameraSnapPoint newSnapPoint = null;

			// a) Reward Scene
			if(toScene is RewardSceneController) {
				newSnapPoint = (toScene as RewardSceneController).photoCameraSnapPoint;
			}

			// b) Lab Scene
			else if(toScene is LabDragonSelectionScene) {
				newSnapPoint = (toScene as LabDragonSelectionScene).photoCameraSnapPoint;
			}

			// c) Dragon Selection Scene
			else if(toScene is DragonSelectionScene) {
				newSnapPoint = (toScene as DragonSelectionScene).photoCameraSnapPoint;
			}

			// Apply
			if(newSnapPoint != null) {
				Debug.Log(Colors.paleGreen.Tag(
					"Setting snap point from " +
					menuSceneController.GetScreenData(MenuScreen.PHOTO).cameraSetup.name +
					" to " + newSnapPoint.name
				));
				menuSceneController.GetScreenData(MenuScreen.PHOTO).cameraSetup = newSnapPoint;
			}
		}
	}

    //------------------------------------------------------------------------//
    // Animoji CALLBACKS															  //
    //------------------------------------------------------------------------//
    /// <summary>
    /// The Animoji button has been pressed.
    /// </summary>
    public void OnAnimojiButton()
    {

        if (!ButtonExtended.checkMultitouchAvailability())
            return;

        // Animoji requires the whole downloadable content

        // Check for assets download for ALL content
        Downloadables.Handle handle = HDAddressablesManager.Instance.GetHandleForAllDownloadables();
        if (!handle.IsAvailable())
        {
            // Initialize download flow with handle for ALL assets
            m_assetsDownloadFlow.InitWithHandle(HDAddressablesManager.Instance.GetHandleForAllDownloadables());
            m_assetsDownloadFlow.OpenPopupByState(PopupAssetsDownloadFlow.PopupType.ANY);
        }

        else
        {

            // Navigate to the animoji screen
            MenuTransitionManager m_transitionManager = InstanceManager.menuSceneController.transitionManager;
            Debug.Assert(m_transitionManager != null, "Required component missing!");

            m_transitionManager.GoToScreen(MenuScreen.ANIMOJI, true);

        }
    }
}
