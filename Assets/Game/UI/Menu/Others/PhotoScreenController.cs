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
	[SerializeField] private DOTweenAnimation m_flashFX = null;
	[SerializeField] private List<GameObject> m_objectsToHide = new List<GameObject>();

	[Separator("Dragon Mode")]
	[SerializeField] private Localizer m_dragonName = null;
	[SerializeField] private Localizer m_dragonDesc = null;
	[SerializeField] private Image m_dragonTierIcon = null;

	// Public properties
	private Mode m_mode = Mode.DRAGON;
	public Mode mode {
		get { return m_mode; }
		set { SetMode(value); }
	}

	// Internal
	private Texture2D m_picture = null;
	private List<GameObject> m_objectsToShow = new List<GameObject>();

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
		SetMode(m_mode);	// Apply initial mode
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
		m_picture = null;
	}

	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Does a screenshot and saves it into the picture texture, overriding its previous content.
	/// </summary>
	/// <returns>The coroutine.</returns>
	private IEnumerator TakePicture() {
		// Hide all UI elements
		m_objectsToShow.Clear();	// Only those that were actually active will be restored
		for(int i = 0; i < m_objectsToHide.Count; i++) {
			if(m_objectsToHide[i].activeSelf) {
				m_objectsToHide[i].SetActive(false);
				m_objectsToShow.Add(m_objectsToHide[i]);
			}
		}

		// Hide HUD as well
		InstanceManager.menuSceneController.hud.gameObject.SetActive(false);

		// Wait until the end of the frame so the "hide" is actually applied
		yield return new WaitForEndOfFrame();

		// Take the screenshot!
		// [AOC] We're not using Application.Screenshot() since we want to have the screenshot in a texture rather than on an image in disk, for sharing and previewing it
		//		 From FGOL
		// Aux vars
		int width = Screen.width;
		int height = Screen.height;

		// If texture is not created, do it now
		if(m_picture == null) {
			m_picture = new Texture2D(width, height, TextureFormat.RGB24, false);
		}

		// Read screen contents into the texture
		m_picture.ReadPixels(new Rect(0, 0, width, height), 0, 0);
		m_picture.Apply();

		// Launch Flash FX! (AFTER the screenshot, of course! :D)
		m_flashFX.gameObject.SetActive(true);
		m_flashFX.DORestart();

		// Give it some time
		yield return new WaitForSeconds(0.25f);

		// Restore disabled objects
		for(int i = 0; i < m_objectsToShow.Count; i++) {
			m_objectsToShow[i].SetActive(true);
		}

		// Restore HUD as well
		InstanceManager.menuSceneController.hud.gameObject.SetActive(true);

		// Open "Share" popup
		PopupPhotoShare popup = PopupManager.OpenPopupInstant(PopupPhotoShare.PATH).GetComponent<PopupPhotoShare>();
		popup.Init(m_picture);
	}

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

		// Initialize elements based on current mode
		switch(m_mode) {
			case Mode.DRAGON: {
				// Initialize dragon info
				DragonData dragonData = DragonManager.GetDragonData(menuController.selectedDragon);
				if(m_dragonName != null) m_dragonName.Localize(dragonData.def.GetAsString("tidName"));
				if(m_dragonDesc != null) m_dragonDesc.Localize(dragonData.def.GetAsString("tidDesc"));
				if(m_dragonTierIcon != null) m_dragonTierIcon.sprite = ResourcesExt.LoadFromSpritesheet(UIConstants.UI_SPRITESHEET_PATH, dragonData.tierDef.GetAsString("icon"));
			} break;

			case Mode.EGG_REWARD: {
				// Nothing to do for now
			} break;
		}

		// Disable drag controller
		currentMode.dragControl.gameObject.SetActive(false);
		currentMode.zoomControl.gameObject.SetActive(false);
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
				MenuScreenScene scene3D = menuController.screensController.GetScene((int)MenuScreens.PHOTO);
				MenuDragonPreview dragonPreview = scene3D.GetComponent<MenuDragonScroller>().GetDragonPreview(menuController.selectedDragon);
				currentMode.dragControl.target = dragonPreview.transform;
			} break;

			case Mode.EGG_REWARD: {
				// Initialize with egg reward view
				MenuScreenScene scene3D = menuController.screensController.GetScene((int)MenuScreens.OPEN_EGG);
				currentMode.dragControl.target = scene3D.GetComponent<OpenEggSceneController>().rewardView.transform;
			} break;
		}

		// Disable camera snap point so we're able to zoom!
		menuController.screensController.currentCameraSnapPoint.enabled = false;

		// Initialize zoom controller with main camera
		currentMode.zoomControl.gameObject.SetActive(true);
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

		// Re-enable camera snap point so we're able to zoom!
		InstanceManager.menuSceneController.screensController.currentCameraSnapPoint.enabled = true;
	}

	/// <summary>
	/// Take the picture!
	/// </summary>
	public void OnTakePictureButton() {
		// Do it in a coroutine to wait until the end of the frame
		StartCoroutine(TakePicture());
	}
}