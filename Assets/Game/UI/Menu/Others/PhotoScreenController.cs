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
	[SerializeField] private DOTweenAnimation m_flashFX = null;
	[SerializeField] private List<GameObject> m_objectsToHide = new List<GameObject>();

	[Separator("Dragon Mode")]
	[SerializeField] private Localizer m_dragonName = null;
	[SerializeField] private Localizer m_dragonDesc = null;
	[SerializeField] private Image m_dragonTierIcon = null;

	[Separator("Egg Reward Mode")]
	[SerializeField] private Localizer m_eggRewardName = null;
	[SerializeField] private TextMeshProUGUI m_eggRewardDesc = null;
	[SerializeField] private Image m_eggRewardIcon = null;

	[Separator("Share Data")]
	[SerializeField] private Image m_qrContainer = null;

	[Separator("AR and Animoji")]
	[SerializeField] private GameObject m_arButton = null;
	[SerializeField] private PhotoScreenARFlow m_arFlow = null;
	[SerializeField] private GameObject m_animojiButton = null;

	// Public properties
	private Mode m_mode = Mode.DRAGON;
	public Mode mode {
		get { return m_mode; }
		set { SetMode(value); }
	}

	// Internal
	private Texture2D m_picture = null;
	private List<GameObject> m_objectsToRestore = new List<GameObject>();
	private string m_targetName = "";   // Localized name of the target of the picture: Dragon name, pet name, etc.

	// AR Internal
	private bool m_isARAvailable = false;
	private bool m_isAnimojiAvailable = false;

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

		// Load qr code
		m_qrContainer.sprite = Resources.Load<Sprite>(GameSettings.shareData.qrCodePath);
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
		// Hide QR code container
		m_qrContainer.gameObject.SetActive(false);
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
		m_objectsToRestore.Clear();	// Only those that were actually active will be restored
		for(int i = 0; i < m_objectsToHide.Count; i++) {
			if(m_objectsToHide[i].activeSelf) {
				m_objectsToHide[i].SetActive(false);
				m_objectsToRestore.Add(m_objectsToHide[i]);
			}
		}

		// Hide HUD as well
		InstanceManager.menuSceneController.hud.gameObject.SetActive(false);

		// If we are in AR, hide AR UI as well
		if(m_isARAvailable && m_arFlow.isActiveAndEnabled) {
			if(m_arFlow.currentScreen != null) {
				m_arFlow.currentScreen.gameObject.SetActive(false);
				m_objectsToRestore.Add(m_arFlow.currentScreen.gameObject);
			}
		}

		// Display QR code
		m_qrContainer.gameObject.SetActive(true);
		m_objectsToRestore.Add(m_qrContainer.gameObject);	// Hide it again once capture is done

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
		for(int i = 0; i < m_objectsToRestore.Count; i++) {
			m_objectsToRestore[i].SetActive(!m_objectsToRestore[i].activeSelf);
		}

		// Restore HUD as well
		InstanceManager.menuSceneController.hud.gameObject.SetActive(true);

		// Figure out default message depending on mode
		string caption = "";
		switch(m_mode) {
			case Mode.DRAGON: {
				caption = LocalizationManager.SharedInstance.Localize("TID_IMAGE_CAPTION", GameSettings.shareData.url);
			} break;

			case Mode.EGG_REWARD: {
				MenuScreenScene scene3D = InstanceManager.menuSceneController.GetScreenData(MenuScreen.OPEN_EGG).scene3d;
				Metagame.Reward currentReward = scene3D.GetComponent<RewardSceneController>().currentReward;
				switch(currentReward.type) {
					case Metagame.RewardPet.TYPE_CODE: {
						caption = LocalizationManager.SharedInstance.Localize("TID_IMAGE_CAPTION_PET", GameSettings.shareData.url);
					} break;
				}
			} break;
		}

		// Open "Share" popup
		PopupPhotoShare popup = PopupManager.OpenPopupInstant(PopupPhotoShare.PATH).GetComponent<PopupPhotoShare>();
		popup.Init(m_picture, caption, m_targetName);
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

		// Initialize elements based on current mode
		switch(m_mode) {
			case Mode.DRAGON: {
				// Initialize dragon info
				IDragonData dragonData = DragonManager.GetDragonData(menuController.selectedDragon);
				m_targetName = dragonData.def.GetLocalized("tidName");
				if(m_dragonName != null) m_dragonName.Localize(dragonData.def.GetAsString("tidName"));
				if(m_dragonDesc != null) m_dragonDesc.Localize(dragonData.def.GetAsString("tidDesc"));
				if(m_dragonTierIcon != null) m_dragonTierIcon.sprite = ResourcesExt.LoadFromSpritesheet(UIConstants.UI_SPRITESHEET_PATH, dragonData.tierDef.GetAsString("icon"));
			} break;

			case Mode.EGG_REWARD: {
				// Defaults
				m_eggRewardName.gameObject.SetActive(true);
				m_eggRewardDesc.gameObject.SetActive(true);
				m_eggRewardIcon.gameObject.SetActive(true);

				// Depends on reward type
				MenuScreenScene scene3D = InstanceManager.menuSceneController.GetScreenData(MenuScreen.OPEN_EGG).scene3d;
				Metagame.Reward currentReward = scene3D.GetComponent<RewardSceneController>().currentReward;
				switch(currentReward.type) {
					case Metagame.RewardPet.TYPE_CODE: {
						// Aux vars
						DefinitionNode rarityDef = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.RARITIES, currentReward.def.Get("rarity"));
						DefinitionNode powerDef = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.POWERUPS, currentReward.def.GetAsString("powerup"));

						// Pet name
						m_targetName = currentReward.def.GetLocalized("tidName");
						m_eggRewardName.Localize(
							m_eggRewardName.tid,
							m_targetName,
							UIConstants.GetRarityColor(currentReward.rarity).ToHexString("#", false),
							currentReward.rarity == Metagame.Reward.Rarity.COMMON ? "" : "(" + rarityDef.GetLocalized("tidName") + ")"	// Don't show for common
						);

						// Power description and icon
						m_eggRewardDesc.text = DragonPowerUp.GetDescription(powerDef, false, true);	// Custom formatting depending on powerup type, already localized
						m_eggRewardIcon.sprite = Resources.Load<Sprite>(UIConstants.POWER_ICONS_PATH + powerDef.GetAsString("icon"));
					} break;

					default: {
						m_targetName = "";
						m_eggRewardName.gameObject.SetActive(false);
						m_eggRewardDesc.gameObject.SetActive(false);
						m_eggRewardIcon.gameObject.SetActive(false);
					} break;
				}
			} break;
		}

		// Disable drag controller
		currentMode.dragControl.gameObject.SetActive(false);
		currentMode.zoomControl.gameObject.SetActive(false);

        // Initialize AR stuff
        m_arButton.SetActive(m_mode == Mode.DRAGON && m_isARAvailable);
		m_arFlow.gameObject.SetActive(false);

		// Allow animoji?
		m_isAnimojiAvailable = (m_mode == Mode.DRAGON) && AnimojiScreenController.IsSupported(InstanceManager.menuSceneController.selectedDragon);
		m_animojiButton.SetActive(m_isAnimojiAvailable);
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
        if (!MenuNavigationButton.checkMultitouchAvailability()) return;

		// Do it in a coroutine to wait until the end of the frame
		StartCoroutine(TakePicture());
    }

    /// <summary>
    /// The back button has been pressed.
    /// </summary>
    public void OnBackButton() {
        if (!MenuNavigationButton.checkMultitouchAvailability()) return;

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
        if (!MenuNavigationButton.checkMultitouchAvailability()) return;

        // Start AR flow
        if (!m_arFlow.isActiveAndEnabled) {
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
        if (!MenuNavigationButton.checkMultitouchAvailability()) return;
        // Terminate AR flow
        m_arFlow.EndFlow();
    }

    /// <summary>
    /// AR flow wants to take a picture.
    /// </summary>
    private void OnARTakePicture() {
        if (!MenuNavigationButton.checkMultitouchAvailability()) return;
        // Use the same picture functionality as in normal mode
        OnTakePictureButton();
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
}
