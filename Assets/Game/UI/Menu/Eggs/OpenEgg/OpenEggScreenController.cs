// OpenEggScreenController.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 03/03/2016.
// Copyright (c) 2016 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Text;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// Global controller for the Open Egg screen in the main menu.
/// </summary>
public class OpenEggScreenController : MonoBehaviour {
	//------------------------------------------------------------------//
	// CONSTANTS														//
	//------------------------------------------------------------------//
	public enum State {
		IDLE,
		INTRO,
		OPENING
	}
	
	//------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES											//
	//------------------------------------------------------------------//
	// Exposed References
	[Separator("Info")]
	[SerializeField] private Localizer m_tapInfoText = null;

	[Separator("Buttons")]
	[SerializeField] private GameObject m_callToActionButton = null;
	[SerializeField] private Localizer m_callToActionText = null;
	[SerializeField] private ShowHideAnimator m_finalPanel = null;

	[Separator("Rewards")]
	[SerializeField] private EggRewardInfo m_rewardInfo = null;

	// Reference to 3D scene
	private OpenEggSceneController m_scene = null;

	// Internal
	private State m_state = State.IDLE;

	// FX
	private GameObject m_flashFX = null;
	
	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {
		// Prepare the flash FX image
		//m_flashFX = new GameObject("FlashFX");
		if(m_flashFX != null) {
			// Transform - full screen rect transform
			RectTransform rectTransform = m_flashFX.AddComponent<RectTransform>();
			rectTransform.SetParent(this.transform, false);
			rectTransform.anchorMin = Vector2.zero;
			rectTransform.anchorMax = Vector2.one;
			rectTransform.offsetMin = Vector2.zero;
			rectTransform.offsetMax = Vector2.zero;

			// Image
			Image image = m_flashFX.AddComponent<Image>();
			image.color = Colors.white;
			image.raycastTarget = false;

			// Start hidden
			m_flashFX.SetLayerRecursively("UI");
			m_flashFX.SetActive(false);
		}
	}

	/// <summary>
	/// Component has been enabled.
	/// </summary>
	private void OnEnable() {
		// Subscribe to external events.
		Messenger.AddListener<Egg>(GameEvents.EGG_OPENED, OnEggCollected);
		Messenger.AddListener<NavigationScreenSystem.ScreenChangedEventData>(EngineEvents.NAVIGATION_SCREEN_CHANGED, OnNavigationScreenChanged);
	}

	/// <summary>
	/// Called every frame
	/// </summary>
	private void Update() {
		
	}

	/// <summary>
	/// Raises the disable event.
	/// </summary>
	private void OnDisable() {
		// Reset state
		m_state = State.IDLE;

		// Clear 3D scene
		if(m_scene != null) m_scene.Clear();

		// Unsubscribe to external events.
		Messenger.RemoveListener<Egg>(GameEvents.EGG_OPENED, OnEggCollected);
		Messenger.RemoveListener<NavigationScreenSystem.ScreenChangedEventData>(EngineEvents.NAVIGATION_SCREEN_CHANGED, OnNavigationScreenChanged);
	}

	/// <summary>
	/// Destructor
	/// </summary>
	private void OnDestroy() {
		
	}

	//------------------------------------------------------------------//
	// PUBLIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Launch an open flow with a given Egg instance.
	/// </summary>
	/// <param name="_egg">The egg to be opened.</param>
	public void StartFlow(Egg _egg) {
		// Check params
		if(_egg == null) return;
		if(_egg.state != Egg.State.READY) return;
		if(m_state != State.IDLE) return;

		// Make sure all required references are set
		ValidateReferences();

		// Clear 3D scene
		m_scene.Clear();

		// Hide HUD and buttons
		bool animate = this.gameObject.activeInHierarchy;	// If the screen is not visible, don't animate
		InstanceManager.GetSceneController<MenuSceneController>().hud.GetComponent<ShowHideAnimator>().ForceHide(animate);
		m_tapInfoText.GetComponent<ShowHideAnimator>().ForceHide(animate);
		m_finalPanel.ForceHide(animate);

		// Hide Flash FX
		if(m_flashFX != null) m_flashFX.SetActive(false);

		// Hide reward elements
		m_rewardInfo.Hide();

		// Change egg state
		_egg.ChangeState(Egg.State.OPENING);

		// Initialize egg view
		m_scene.InitEggView(_egg);

		// Launch intro!
		m_scene.LaunchIntro();
		m_state = State.INTRO;
	}

	//------------------------------------------------------------------//
	// INTERNAL METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Make sure all required references are initialized.
	/// </summary>
	private void ValidateReferences() {
		// 3d scene for this screen
		if(m_scene == null) {
			MenuSceneController sceneController = InstanceManager.GetSceneController<MenuSceneController>();
			Debug.Assert(sceneController != null, "This component must be only used in the menu scene!");
			MenuScreenScene menuScene = sceneController.screensController.GetScene((int)MenuScreens.OPEN_EGG);
			if(menuScene != null) {
				// Get scene controller and initialize
				m_scene = menuScene.GetComponent<OpenEggSceneController>();
				if(m_scene != null) {
					m_scene.OnIntroFinished.AddListener(OnIntroFinished);
					m_scene.OnEggOpenFinished.AddListener(LaunchRewardAnimation);
				}
			}
		}
	}

	//------------------------------------------------------------------//
	// ANIMATIONS														//
	//------------------------------------------------------------------//
	/// <summary>
	/// Launches the open egg animation!
	/// </summary>
	private void LaunchOpenAnimation() {
		// This option should only be available on the OPENING state and with a valid egg
		if(m_state != State.OPENING) return;
		if(m_scene.eggView == null) return;

		// Do a full-screen flash FX (TEMP)
		if(m_flashFX != null) {
			Color rarityColor = UIConstants.GetRarityColor(m_scene.eggData.rewardData.rarity);		// Color based on reward's rarity :)
			m_flashFX.SetActive(true);
			m_flashFX.GetComponent<Image>().color = rarityColor;
			m_flashFX.GetComponent<Image>().DOFade(0f, 2f).SetEase(Ease.OutExpo).SetRecyclable(true).OnComplete(() => { m_flashFX.SetActive(false); });
		}

		// Show/Hide buttons and HUD
		m_tapInfoText.GetComponent<ShowHideAnimator>().Hide();
		m_finalPanel.Show();

		// Do the 3D anim
		m_scene.LaunchOpenEggAnim();

		// Change logic state
		m_state = State.IDLE;
	}

	/// <summary>
	/// Launches the animation of the reward components.
	/// </summary>
	private void LaunchRewardAnimation() {
		// Show HUD
		InstanceManager.GetSceneController<MenuSceneController>().hud.GetComponent<ShowHideAnimator>().Show();

		// Aux vars
		EggReward rewardData = m_scene.eggData.rewardData;
		DefinitionNode rarityDef = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.RARITIES, rewardData.def.Get("rarity"));

		// Initialize title and launch animation
		m_rewardInfo.InitAndAnimate(rewardData);

		// Initialize power icon based on reward type
		// Initialize other stuff based on reward type
		switch(rewardData.type) {
			case "pet": {
				// Call to action text
				m_callToActionText.Localize("TID_EGG_SHOW_REWARD");
			} break;

			default: {
				
			} break;
		}

		// Don't show call to action button if the reward is a duplicate
		m_callToActionButton.SetActive(!rewardData.duplicated);

		// Initialize and launch 3D reward view
		m_scene.LaunchRewardAnim();
	}

	//------------------------------------------------------------------//
	// CALLBACKS														//
	//------------------------------------------------------------------//
	/// <summary>
	/// The intro has finished!
	/// </summary>
	private void OnIntroFinished() {
		// Show info text
		m_tapInfoText.GetComponent<ShowHideAnimator>().Show();

		// Change logic state
		m_state = State.OPENING;
	}

	/// <summary>
	/// The egg open animation has finished!
	/// </summary>
	private void OnEggOpenFinished() {
		// Launch the reward animation
		LaunchRewardAnimation();
	}

	/// <summary>
	/// Depending on opened egg's reward, perform different actions.
	/// </summary>
	public void OnCallToActionButton() {
		// This option should only be available on the IDLE state and with a valid egg
		if(m_state != State.IDLE) return;
		if(m_scene.eggData == null) return;
		if(m_scene.eggData.state != Egg.State.COLLECTED) return;

		// Depending on opened egg's reward, perform different actions
		MenuScreensController screensController = InstanceManager.sceneController.GetComponent<MenuScreensController>();
		switch(m_scene.eggData.rewardData.type) {
			case "pet": {
				// Go to the pets screen
				PetsScreenController petScreen = screensController.GetScreen((int)MenuScreens.PETS).GetComponent<PetsScreenController>();
				petScreen.Initialize(m_scene.eggData.rewardData.itemDef.sku);
				screensController.GoToScreen((int)MenuScreens.PETS);
			} break;
		}
	}

	/// <summary>
	/// An egg has been opened and its reward collected.
	/// </summary>
	/// <param name="_egg">The egg.</param>
	private void OnEggCollected(Egg _egg) {
		// Must have a valid egg
		if(m_scene.eggData == null) return;

		// If it matches our curent egg, launch its animation!
		if(_egg == m_scene.eggData) {
			// Launch animation!
			LaunchOpenAnimation();
		}
	}

	/// <summary>
	/// Navigation screen has changed (animation starts now).
	/// </summary>
	/// <param name="_event">Event data.</param>
	private void OnNavigationScreenChanged(NavigationScreenSystem.ScreenChangedEventData _event) {
		// Only if it comes from the main screen navigation system
		if(_event.dispatcher != InstanceManager.GetSceneController<MenuSceneController>().screensController) return;

		// If leaving this screen, launch all the hide animations that are not automated
		if(_event.fromScreenIdx == (int)MenuScreens.OPEN_EGG) {
			// Hide reward elements
			m_rewardInfo.Hide();

			// Clear 3D scene
			if(m_scene != null) m_scene.Clear();

			// Restore HUD
			InstanceManager.GetSceneController<MenuSceneController>().hud.GetComponent<ShowHideAnimator>().Show();
		}

		// If entering this screen, force some show/hide animations that conflict with automated ones
		if(_event.fromScreenIdx == (int)MenuScreens.OPEN_EGG) {
			// At this point automated ones have already been launched, so we override them
			m_tapInfoText.GetComponent<ShowHideAnimator>().Hide(false);
		}
	}
}