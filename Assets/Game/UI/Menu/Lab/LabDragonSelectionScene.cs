// LabDragonSelectionScene.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 03/10/2018.
// Copyright (c) 2018 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using DG.Tweening;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Main controller for the Lab Dragon Selection 3D scene.
/// </summary>
public class LabDragonSelectionScene : MenuScreenScene {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed references
	[SerializeField] private MenuDragonLoader m_dragonLoader = null;
	public MenuDragonLoader dragonLoader {
		get { return m_dragonLoader; }
	}

	[Tooltip("Will replace the camera snap point for the photo screen when doing photos to special dragons.")]
	[SerializeField] private CameraSnapPoint m_photoCameraSnapPoint = null;
	public CameraSnapPoint photoCameraSnapPoint {
		get { return m_photoCameraSnapPoint; }
	}

	[SerializeField] private CameraSnapPoint m_notOwnedCameraSnapPoint = null;
	[Space] 
    [SerializeField] private ParticleSystem m_loadingDragonParticle = null;
    [SerializeField] private ParticleSystem m_loadedDragonParticle = null;
    [SerializeField] private TweenSequence m_tweenSequence = null;
	[Space]
	[SerializeField] private GameObject m_dragonPurchasedFX = null;
	[SerializeField] private Transform m_dragonPurchasedFXAnchor = null;
	[Space]
	[SerializeField] private GameObject m_leagueTrophyPreview = null;
	public GameObject leagueTrophyPreview {
		get { return m_leagueTrophyPreview; }
	}

	// Internal references
	private ScreenData m_labScreenData = null;

	private GameObject m_loadingUI = null;
	public GameObject loadingUI {
		set { m_loadingUI = value; }
	}

	// Camera snap points backups
	private CameraSnapPoint m_defaultCameraSnapPoint = null;
	
	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {
		// Gather references
		m_labScreenData = InstanceManager.menuSceneController.GetScreenData(MenuScreen.LAB_DRAGON_SELECTION);
		m_defaultCameraSnapPoint = m_labScreenData.cameraSetup;

		// Subscribe to external events
		Messenger.AddListener<SceneController.Mode, SceneController.Mode>(MessengerEvents.GAME_MODE_CHANGED, OnGameModeChanged);
		Messenger.AddListener<string>(MessengerEvents.MENU_DRAGON_SELECTED, OnDragonSelected);
		Messenger.AddListener<IDragonData>(MessengerEvents.DRAGON_ACQUIRED, OnDragonAcquired);
		Messenger.AddListener<MenuScreen, MenuScreen>(MessengerEvents.MENU_SCREEN_TRANSITION_START, OnMenuScreenTransitionStart);
		Messenger.AddListener<MenuScreen, MenuScreen, bool>(MessengerEvents.MENU_CAMERA_TRANSITION_START, OnMenuCameraTransitionStart);
		m_dragonLoader.onDragonLoaded += OnDragonPreviewLoaded;

		// Destroy any loaded dragon preview
		UnloadDragonPreview();
        if ( m_tweenSequence != null )
        {
            m_tweenSequence.OnFinished.AddListener(RescaleParticles);
        }
        
	}

	/// <summary>
	/// Destructor.
	/// </summary>
	private void OnDestroy() {
		// Unsubscribe from external events
		Messenger.RemoveListener<SceneController.Mode, SceneController.Mode>(MessengerEvents.GAME_MODE_CHANGED, OnGameModeChanged);
		Messenger.RemoveListener<string>(MessengerEvents.MENU_DRAGON_SELECTED, OnDragonSelected);
		Messenger.RemoveListener<IDragonData>(MessengerEvents.DRAGON_ACQUIRED, OnDragonAcquired);
		Messenger.RemoveListener<MenuScreen, MenuScreen>(MessengerEvents.MENU_SCREEN_TRANSITION_START, OnMenuScreenTransitionStart);
		Messenger.RemoveListener<MenuScreen, MenuScreen, bool>(MessengerEvents.MENU_CAMERA_TRANSITION_START, OnMenuCameraTransitionStart);
		m_dragonLoader.onDragonLoaded -= OnDragonPreviewLoaded;
	}

	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Load preview of a given dragon.
	/// Will unload previously loaded dragon preview.
	/// </summary>
	/// <param name="_sku">Dragon's sku.</param>
	private void LoadDragonPreview(string _sku, bool _force) {
		// Toggle loading UI on (will be disabled once the dragon preview is loaded)
		if(m_loadingUI != null) m_loadingUI.gameObject.SetActive(true);
        if (m_loadingDragonParticle != null) m_loadingDragonParticle.Play();

		// Load the new dragon with its current skin!
		IDragonData dragonData = DragonManager.GetDragonData(_sku);

        if (_force) {
            m_dragonLoader.UnloadDragon(); 
        }

		m_dragonLoader.LoadDragon(_sku, dragonData.persistentDisguise);
	}

	/// <summary>
	/// Unload current dragon's preview.
	/// </summary>
	private void UnloadDragonPreview() {
		// Toggle loading UI on
		if(m_loadingUI != null) m_loadingUI.gameObject.SetActive(false);

		// Unload dragon preview
		m_dragonLoader.UnloadDragon();
	}

	/// <summary>
	/// Launches the dragon purchased FX on the selected dragon.
	/// </summary>
	public void LaunchDragonPurchasedFX() {
		// Check required stuff
		if(m_dragonPurchasedFX == null) return;

		// Create a new instance of the FX and put it on the dragon loader
		GameObject newObj = Instantiate<GameObject>(
			m_dragonPurchasedFX, 
			m_dragonPurchasedFXAnchor,
			false
		);

		// Auto-destroy after the FX has finished
		DestroyInSeconds destructor = newObj.AddComponent<DestroyInSeconds>();
		destructor.lifeTime = 9f;   // Sync with FX duration!

		// Trigger SFX
		AudioController.Play("hd_unlock_dragon");
	}

	/// <summary>
	/// Select the camera snap point to use based on selected dragon ownership status.
	/// Will modify the ScreenData for this screen in the transitions manager.
	/// </summary>
	public void SelectCameraSnapPoint() {
		// Is selected dragon owned?
		if(InstanceManager.menuSceneController.selectedDragonData.isOwned) {
			m_labScreenData.cameraSetup = m_defaultCameraSnapPoint;
		} else {
			m_labScreenData.cameraSetup = m_notOwnedCameraSnapPoint;
		}
	}

	/// <summary>
	/// Update camera based on current selected dragon.
	/// It is assumed that SelectCameraSnapPoint() has been previously called.
	/// </summary>
	public void RefreshCamera() {
		// Don't if a screen transition is ongoing
		if(InstanceManager.menuSceneController.transitionManager.isTransitioning) return;

		// Lerp position as well
		TweenParams tweenParams = new TweenParams().SetEase(Ease.OutQuad);
		m_labScreenData.cameraSetup.changePosition = true;
		m_labScreenData.cameraSetup.TweenTo(
			InstanceManager.menuSceneController.mainCamera, 
			0.25f, 
			tweenParams
		);
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Active game mode has changed.
	/// </summary>
	/// <param name="_oldMode">Previous game mode.</param>
	/// <param name="_newMode">The new active game mode.</param>
	private void OnGameModeChanged(SceneController.Mode _oldMode, SceneController.Mode _newMode) {
		// Entering lab mode?
		if(_newMode == SceneController.Mode.SPECIAL_DRAGONS) {
			// Load current special dragon
			LoadDragonPreview(InstanceManager.menuSceneController.selectedDragon, true);
		}

		// Leaving lab mode?
		if(_oldMode == SceneController.Mode.SPECIAL_DRAGONS) {
			// Unload special dragon preview
			UnloadDragonPreview();
		}

	}

	/// <summary>
	/// A new dragon has been selected.
	/// </summary>
	/// <param name="_sku">SKU of the newly selected dragon.</param>
	private void OnDragonSelected(string _sku) {
		// Ignore if not in special dragon mode
		if(SceneController.mode != SceneController.Mode.SPECIAL_DRAGONS) return;

		// Load newly selected special dragon
		LoadDragonPreview(_sku, false);

		// Update camera
		SelectCameraSnapPoint();
		RefreshCamera();
	}

	/// <summary>
	/// A dragon has been acquired.
	/// </summary>
	/// <param name="_data">The newly selected dragon.</param>
	private void OnDragonAcquired(IDragonData _data) {
		// Ignore if not in special dragon mode
		if(SceneController.mode != SceneController.Mode.SPECIAL_DRAGONS) return;

        // Refreshing the camera here was moveing it back to the lab scren, causing the next bugs:
		// Issue HDK-3435: buying a classic dragon from an offer pack while in the lab
        // Issue HDK-3845: Weird flow after buying a special dragon in the lab screen
        // So don´t do it.

	}

	/// <summary>
	/// Dragon preview has finished loading.
	/// </summary>
	/// <param name="_loader">The loader that triggered the event.</param>
	private void OnDragonPreviewLoaded(MenuDragonLoader _loader) {
		// Toggle loading UI off
		if(m_loadingUI != null) m_loadingUI.gameObject.SetActive(false);
        if (m_loadedDragonParticle != null) m_loadedDragonParticle.Play();
        if (m_loadingDragonParticle != null) m_loadingDragonParticle.Stop();
        if (m_tweenSequence != null)
        {
            m_dragonLoader.transform.localScale = GameConstants.Vector3.zero;
            m_tweenSequence.Launch();
        }
	}

    void RescaleParticles()
    {
        if (m_dragonLoader != null)
            m_dragonLoader.RescaleParticles();
    }

	/// <summary>
	/// The menu screen change animation is about to start.
	/// </summary>
	/// <param name="_from">Screen we come from.</param>
	/// <param name="_to">Screen we're going to.</param>
	private void OnMenuScreenTransitionStart(MenuScreen _from, MenuScreen _to) {
        // If we are comming from the game we need to force dragon loading
        if ( _from == MenuScreen.NONE && _to == MenuScreen.LAB_DRAGON_SELECTION )
        {
            // Load current special dragon
            LoadDragonPreview(InstanceManager.menuSceneController.selectedDragon, true);
        }
    
		// Check params
		if(_from == MenuScreen.NONE || _to == MenuScreen.NONE) return;

		// Aux vars
		MenuScreenScene fromScene = InstanceManager.menuSceneController.GetScreenData(_from).scene3d;
		MenuScreenScene toScene = InstanceManager.menuSceneController.GetScreenData(_to).scene3d;

		// Entering a screen using this scene
		if(toScene != null && toScene.gameObject == this.gameObject) {
			// Select the right camera snap point for this screen
			SelectCameraSnapPoint();
		}
	}

	/// <summary>
	/// The menu camera is about to start a transition.
	/// </summary>
	/// <param name="_from">Screen we come from.</param>
	/// <param name="_to">Screen we're going to.</param>
	/// <param name="_usingPath">Using a path or lerping?</param>
	private void OnMenuCameraTransitionStart(MenuScreen _from, MenuScreen _to, bool _usingPath) {
		// Don't care at all if not using path
		if(!_usingPath) return;

		// Entering a screen using this scene
		MenuScreenScene toScene = InstanceManager.menuSceneController.GetScreenData(_to).scene3d;
		if(toScene != null && toScene.gameObject == this.gameObject) {
			// Camera snap point has already been selected (on the MenuScreenTransitionStart callback)
			// We just need to update the dynamic path
			// Adjust dynamic path to end at the target snap point
			BezierCurve path = InstanceManager.menuSceneController.transitionManager.dynamicPath;
			BezierPoint finalPoint = path.GetPoint(path.pointCount - 1);
			finalPoint.globalPosition = m_labScreenData.cameraSetup.transform.position;
		}
	}
}