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
	[SerializeField] private GameObject m_backButton = null;
	[SerializeField] private GameObject m_buyEggButton = null;
	[SerializeField] private GameObject m_callToActionButton = null;
	[SerializeField] private Localizer m_callToActionText = null;
	[SerializeField] private ShowHideAnimator m_finalPanel = null;

	[Separator("Rewards")]
	[SerializeField] private EggRewardInfo m_rewardInfo = null;
	[SerializeField] private DragControlRotation m_rewardDragController = null;

	[Separator("Animation Parameters")]
	[SerializeField] private float m_openAnimationDelay = 1.75f;
	[SerializeField] private float m_finalPanelDelay = 0f;
	[SerializeField] private float m_finalPanelDelayWhenFragmentsGiven = 2f;
	[SerializeField] private float m_finalPanelDelayWhenCoinsGiven = 1f;
	[SerializeField] private float m_goldenEggOpenDelay = 3f;

	// Reference to 3D scene
	private OpenEggSceneController m_scene = null;

	// Internal
	private State m_state = State.IDLE;
	private bool m_tutorialCompletedPending = false;

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

		// Subscribe to external events.
		Messenger.AddListener<MenuScreens, MenuScreens>(GameEvents.MENU_SCREEN_TRANSITION_START, OnMenuScreenTransitionStart);
		Messenger.AddListener<MenuScreens, MenuScreens>(GameEvents.MENU_SCREEN_TRANSITION_END, OnMenuScreenTransitionEnd);
	}

	/// <summary>
	/// Component has been enabled.
	/// </summary>
	private void OnEnable() {
		Messenger.AddListener<Egg>(GameEvents.EGG_OPENED, OnEggCollected);
		m_rewardInfo.OnAnimFinished.AddListener(OnRewardAnimFinished);
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

		// Unsubscribe to external events.
		Messenger.RemoveListener<Egg>(GameEvents.EGG_OPENED, OnEggCollected);
		m_rewardInfo.OnAnimFinished.RemoveListener(OnRewardAnimFinished);
	}

	/// <summary>
	/// Destructor
	/// </summary>
	private void OnDestroy() {
		// Unsubscribe from external events
		Messenger.RemoveListener<MenuScreens, MenuScreens>(GameEvents.MENU_SCREEN_TRANSITION_START, OnMenuScreenTransitionStart);
		Messenger.RemoveListener<MenuScreens, MenuScreens>(GameEvents.MENU_SCREEN_TRANSITION_END, OnMenuScreenTransitionEnd);
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
		InstanceManager.menuSceneController.hud.animator.ForceHide(animate);
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
			MenuSceneController sceneController = InstanceManager.menuSceneController;
			Debug.Assert(sceneController != null, "This component must be only used in the menu scene!");
			MenuScreenScene menuScene = sceneController.screensController.GetScene((int)MenuScreens.OPEN_EGG);
			if(menuScene != null) {
				// Get scene controller and initialize
				m_scene = menuScene.GetComponent<OpenEggSceneController>();
				if(m_scene != null) {
					// Initialize
					m_scene.InitReferences(m_rewardDragController);

					// Subscribe to listeners
					m_scene.OnIntroFinished.AddListener(OnIntroFinished);
					m_scene.OnEggOpenFinished.AddListener(OnEggOpenFinished);
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

		// Aux vars
		Egg eggData = m_scene.eggData;
		EggReward rewardData = eggData.rewardData;

		// Do a full-screen flash FX (TEMP)
		if(m_flashFX != null) {
			Color rarityColor = UIConstants.GetRarityColor(rewardData.rarity);		// Color based on reward's rarity :)
			m_flashFX.SetActive(true);
			m_flashFX.GetComponent<Image>().color = rarityColor;
			m_flashFX.GetComponent<Image>().DOFade(0f, 2f).SetEase(Ease.OutExpo).SetRecyclable(true).OnComplete(() => { m_flashFX.SetActive(false); });
		}

		// Do the 3D anim
		m_scene.LaunchOpenEggAnim();

		// Change logic state
		m_state = State.IDLE;
	}

	/// <summary>
	/// Launches the animation of the reward components.
	/// </summary>
	private void LaunchRewardAnimation() {
		// Aux vars
		EggReward rewardData = m_scene.eggData.rewardData;
		bool goldenEggCompleted = EggManager.goldenEggCompleted;

		// Show HUD
		InstanceManager.menuSceneController.hud.animator.Show();

		// Photo button only enabled if reward is not a duplicate!
		// Only animate if showing
		ShowHideAnimator photoAnimator = InstanceManager.menuSceneController.hud.photoButton.GetComponent<ShowHideAnimator>();
		if(rewardData.duplicated) {
			photoAnimator.ForceHide(false);
		} else {
			photoAnimator.Show();
		}

		// Initialize stuff based on reward type
		switch(rewardData.type) {
			case "pet": {
				// Call to action text
				m_callToActionText.Localize("TID_EGG_SHOW_REWARD");
			} break;
		}

		// Initialize and show final panel
		// Delay if duplicate, we need to give enough time for the duplicate animation!
		float delay = m_finalPanelDelay;
		if(rewardData.fragments > 0) {
			delay = m_finalPanelDelayWhenFragmentsGiven;
		} else if(rewardData.coins > 0) {
			delay = m_finalPanelDelayWhenCoinsGiven;
		}
		DOVirtual.DelayedCall(delay, () => { m_finalPanel.Show(); }, false);

		// If it's the first time we're getting golden fragments, show info popup
		if(rewardData.fragments > 0 && !UsersManager.currentUser.IsTutorialStepCompleted(TutorialStep.GOLDEN_FRAGMENTS_INFO)) {
			// Show popup after some extra delay
			DOVirtual.DelayedCall(
				delay + 1.5f, 
				() => { 
					PopupManager.OpenPopupInstant(PopupInfoGoldenFragments.PATH);
					UsersManager.currentUser.SetTutorialStepCompleted(TutorialStep.GOLDEN_FRAGMENTS_INFO, true);
				}, 
				false
			);
		}

		// Don't show call to action button if the reward is a duplicate
		m_callToActionButton.SetActive(!rewardData.duplicated);

		// Don't show back button if we've completed a golden egg!
		m_backButton.SetActive(!goldenEggCompleted);

		// Same with egg buy button
		m_buyEggButton.SetActive(!goldenEggCompleted);

		// Initialize and launch 3D reward view
		m_scene.LaunchRewardAnim();

		// Initialize and launch 2D info animation
		m_rewardInfo.gameObject.SetActive(true);
		m_rewardInfo.InitAndAnimate(rewardData);
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
	/// The reward animation has finished.
	/// </summary>
	private void OnRewardAnimFinished() {
		// If a golden egg has been completed, start the open flow
		if(EggManager.goldenEggCompleted) {
			// Create a new egg
			Egg goldenEgg = Egg.CreateFromSku(Egg.SKU_GOLDEN_EGG);
			goldenEgg.ChangeState(Egg.State.READY);

			// Start flow! (After some delay)
			DOVirtual.DelayedCall(0.5f, () => { StartFlow(goldenEgg); });
		}
	}

	/// <summary>
	/// Depending on opened egg's reward, perform different actions.
	/// </summary>
	public void OnCallToActionButton() {
		// This option should only be available on the IDLE state and with a valid egg
		if(m_state != State.IDLE) return;
		if(m_scene.eggData == null) return;
		if(m_scene.eggData.state != Egg.State.COLLECTED) return;

		// Mark reward tutorial as completed
		// [AOC] Delay it until the screen animation has finished so we don't see elements randomly appearing!
		m_tutorialCompletedPending = true;

		// Depending on opened egg's reward, perform different actions
		MenuScreensController screensController = InstanceManager.sceneController.GetComponent<MenuScreensController>();
		switch(m_scene.eggData.rewardData.type) {
			case "pet": {
				// Make sure selected dragon is owned
				InstanceManager.menuSceneController.dragonSelector.SetSelectedDragon(DragonManager.currentDragon.def.sku);	// Current dragon is the last owned selected dragon

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
			// Delay to sync with the egg anim
			DOVirtual.DelayedCall(m_openAnimationDelay, LaunchOpenAnimation, false);

			// Hide UI!
			m_tapInfoText.GetComponent<ShowHideAnimator>().ForceHide();
		}
	}

	/// <summary>
	/// Navigation screen animation has finished.
	/// Must be connected in the inspector.
	/// </summary>
	public void OnClosePostAnimation() {
		// If the tutorial was completed, update flag now!
		if(m_tutorialCompletedPending) {
			UsersManager.currentUser.SetTutorialStepCompleted(TutorialStep.EGG_REWARD, true);
			m_tutorialCompletedPending = false;
		}
	}

	/// <summary>
	/// The menu screen change animation is about to start.
	/// </summary>
	/// <param name="_from">Screen we come from.</param>
	/// <param name="_to">Screen we're going to.</param>
	private void OnMenuScreenTransitionStart(MenuScreens _from, MenuScreens _to) {
		// Leaving this screen
		if(_from == MenuScreens.OPEN_EGG && _to != MenuScreens.OPEN_EGG) {
			// Launch all the hide animations that are not automated
			// Hide reward elements
			m_rewardInfo.Hide();

			// Restore HUD
			InstanceManager.menuSceneController.hud.animator.Show();

			// Disable drag control
			m_rewardDragController.gameObject.SetActive(false);

			// Put photo screen back in dragon mode and restore overriden setup
			if(_to != MenuScreens.PHOTO) {
				// Only if not going into it!
				PhotoScreenController photoScreen = InstanceManager.menuSceneController.GetScreen(MenuScreens.PHOTO).GetComponent<PhotoScreenController>();
				photoScreen.mode = PhotoScreenController.Mode.DRAGON;
			}
		}

		// If entering this screen, force some show/hide animations that conflict with automated ones
		if(_to == MenuScreens.OPEN_EGG) {
			// At this point automated ones have already been launched, so we override them
			m_tapInfoText.GetComponent<ShowHideAnimator>().Hide(false);

			// Put photo screen in EggReward mode and override some setup
			PhotoScreenController photoScreen = InstanceManager.menuSceneController.GetScreen(MenuScreens.PHOTO).GetComponent<PhotoScreenController>();
			photoScreen.mode = PhotoScreenController.Mode.EGG_REWARD;

			// Special stuff if coming back from the photo screen
			if(_from == MenuScreens.PHOTO) {
				// Restore reward info
				m_rewardInfo.Show();

				// Restore photo button
				InstanceManager.menuSceneController.hud.photoButton.GetComponent<ShowHideAnimator>().Show();
			}
		}
	}

	/// <summary>
	/// The menu screen change animation has finished.
	/// </summary>
	/// <param name="_from">Screen we come from.</param>
	/// <param name="_to">Screen we're going to.</param>
	private void OnMenuScreenTransitionEnd(MenuScreens _from, MenuScreens _to) {
		// Entering this screen
		if(_to == MenuScreens.OPEN_EGG) {
			// Enable drag control
			m_rewardDragController.gameObject.SetActive(m_rewardDragController.target != null);
		}
	}
}