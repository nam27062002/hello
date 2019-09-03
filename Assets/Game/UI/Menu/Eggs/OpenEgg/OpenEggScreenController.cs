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
		OPENING,
		REWARD_IN
	}
	
	//------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES											//
	//------------------------------------------------------------------//
	// Exposed References
	[Separator("Buttons")]
	[SerializeField] private GameObject m_backButton = null;
	[SerializeField] private GameObject m_buyEggButton = null;
	[SerializeField] private GameObject m_callToActionButton = null;
	[SerializeField] private Localizer m_callToActionText = null;
	[SerializeField] private ShowHideAnimator m_finalPanel = null;

	[Separator("Rewards")]
	[SerializeField] private RewardInfoUI m_rewardInfo = null;
	[SerializeField] private DragControlRotation m_rewardDragController = null;

	// Reference to 3D scene
	private RewardSceneController m_scene = null;

	// Internal
	private State m_state = State.IDLE;
	private bool m_goldenFragmentsTutorial = false;

	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {
		// Subscribe to external events.
		Messenger.AddListener<MenuScreen, MenuScreen>(MessengerEvents.MENU_SCREEN_TRANSITION_START, OnMenuScreenTransitionStart);
		Messenger.AddListener<MenuScreen, MenuScreen>(MessengerEvents.MENU_SCREEN_TRANSITION_END, OnMenuScreenTransitionEnd);
	}

	/// <summary>
	/// Component has been enabled.
	/// </summary>
	private void OnEnable() {
		
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
	}

	/// <summary>
	/// Destructor
	/// </summary>
	private void OnDestroy() {
		// Unsubscribe from external events
		Messenger.RemoveListener<MenuScreen, MenuScreen>(MessengerEvents.MENU_SCREEN_TRANSITION_START, OnMenuScreenTransitionStart);
		Messenger.RemoveListener<MenuScreen, MenuScreen>(MessengerEvents.MENU_SCREEN_TRANSITION_END, OnMenuScreenTransitionEnd);
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
		if(m_state != State.IDLE && m_state != State.REWARD_IN) return;

		// Make sure all required references are set
		ValidateReferences();

		// Listen to 3D scene events - remove first to avoid receiving the event twice! (shouldn't happen, just in case)
		m_scene.OnAnimStarted.RemoveListener(OnSceneAnimStarted);
		m_scene.OnAnimFinished.RemoveListener(OnSceneAnimFinished);

		m_scene.OnAnimStarted.AddListener(OnSceneAnimStarted);
		m_scene.OnAnimFinished.AddListener(OnSceneAnimFinished);

		// Clear 3D scene
		m_scene.Clear();

		// Push the egg reward to the stack!
		if(_egg.rewardData == null) _egg.GenerateReward();	// Generate a reward if the egg hasn't one
		UsersManager.currentUser.PushReward(_egg.rewardData);

		// Remove it from the inventory (if appliable)
		// [AOC] At this point the egg is pushed to pending rewards stack, so if the game is interrupted we will get the pending rewards flow. We don't want the egg to also be ready in the inventory! (Exploit)
		EggManager.RemoveEggFromInventory(_egg);

		// Save current profile state in case the open egg flow is interrupted
		PersistenceFacade.instance.Save_Request(true);

		// Tell the scene to start the open reward flow with the latest pushed reward
		m_scene.OpenReward();
	}

	//------------------------------------------------------------------//
	// INTERNAL METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Make sure all required references are initialized.
	/// </summary>
	private void ValidateReferences() {
		// Get 3D scene reference for this screen
		if(m_scene == null) {
			MenuSceneController sceneController = InstanceManager.menuSceneController;
			Debug.Assert(sceneController != null, "This component must be only used in the menu scene!");
			MenuScreenScene menuScene = sceneController.GetScreenData(MenuScreen.OPEN_EGG).scene3d;
			if(menuScene != null) {
				// Get scene controller and initialize
				m_scene = menuScene.GetComponent<RewardSceneController>();
			}
		}

		// Tell the scene it will be working with this screen
		if(m_scene != null) {
			m_scene.InitReferences(m_rewardDragController, m_rewardInfo);
		}
	}

	//------------------------------------------------------------------//
	// CALLBACKS														//
	//------------------------------------------------------------------//
	/// <summary>
	/// The intro has finished!
	/// </summary>
	private void OnIntroFinished() {
		// Change logic state
		m_state = State.OPENING;
	}

	/// <summary>
	/// An animation for a reward has started in the 3d scene!
	/// </summary>
	private void OnSceneAnimStarted() {
		m_goldenFragmentsTutorial = false;

        // What type of reward are we opening?
		// Egg
		if(m_scene.currentReward is Metagame.RewardEgg) {
			// Hide HUD and buttons
			bool animate = this.gameObject.activeInHierarchy;	// If the screen is not visible, don't animate
			InstanceManager.menuSceneController.hud.animator.ForceHide(animate);
			m_finalPanel.ForceHide(animate);

			// Wait for the intro anim to finish (sync delay with Egg intro anim)
			m_state = State.INTRO;
			UbiBCN.CoroutineManager.DelayedCall(OnIntroFinished, 0.25f);
		}

		// Other - should be the actual egg reward
		else {
			// Launch the reward UI animation
			// Aux vars
			Metagame.Reward finalReward = m_scene.eggData.rewardData.reward;
			
			// Special initializations when reward is duplicated
			if(finalReward.WillBeReplaced()) {
				// Show golden fragments tutorial popup?
				m_goldenFragmentsTutorial = PopupInfoGoldenFragments.Check(finalReward);

				// Don't show call to action button if the reward is a duplicate
				m_callToActionButton.SetActive(false);				
			} else {
				// Don't show call to action button if the reward is a duplicate
				m_callToActionText.Localize("TID_EGG_SHOW_REWARD");
                m_callToActionButton.SetActive( DragonManager.maxSpecialDragonTierUnlocked > DragonTier.TIER_0);
			}

			// Don't show back button if we've completed a golden egg!
			// Don't show either if rewarding a pet and tutorial not yet completed (force going to collection)
            bool hideBackButton =  (!finalReward.WillBeReplaced() && !UsersManager.currentUser.IsTutorialStepCompleted(TutorialStep.EGG_REWARD));
			m_backButton.SetActive(!hideBackButton);

			// Same with egg buy button
			m_buyEggButton.SetActive(!hideBackButton);

			// Change logic state
			m_state = State.REWARD_IN;
		}
	}

	/// <summary>
	/// An animation for a reward has finished in the 3d scene!
	/// </summary>
	private void OnSceneAnimFinished() {
		// Change logic state
		m_state = State.IDLE;

		// Show popup after some extra delay
		if(m_goldenFragmentsTutorial) {
			PopupInfoGoldenFragments.Show(PopupLauncher.TrackingAction.INFO_POPUP_AUTO, 0.25f);
			m_goldenFragmentsTutorial = false;
		}

		// Show final panel
		m_finalPanel.Show();

		// Same with HUD - unless golden egg was completed
		InstanceManager.menuSceneController.hud.animator.Show();

		// Stop listeneing the 3D scene
		m_scene.OnAnimStarted.RemoveListener(OnSceneAnimStarted);
		m_scene.OnAnimFinished.RemoveListener(OnSceneAnimFinished);
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
		UsersManager.currentUser.SetTutorialStepCompleted(TutorialStep.EGG_REWARD, true);

		// Depending on opened egg's reward, perform different actions
		MenuTransitionManager screensController = InstanceManager.menuSceneController.transitionManager;
		switch(m_scene.eggData.rewardData.reward.type) {
			case Metagame.RewardPet.TYPE_CODE: {
				// Make sure selected dragon is owned
				InstanceManager.menuSceneController.SetSelectedDragon(DragonManager.CurrentDragon.def.sku);	// Current dragon is always owned

				// Go to the pets screen
				// Add a frame of delay to make sure everyone has been notified that the selected dragon has changed
				UbiBCN.CoroutineManager.DelayedCallByFrames(() => {
					MenuScreen targetPetScreen = MenuScreen.PETS;
                    PetsScreenController petScreen = screensController.GetScreenData(targetPetScreen).ui.GetComponent<PetsScreenController>();
					petScreen.Initialize(m_scene.eggData.rewardData.reward.sku);
					screensController.GoToScreen(targetPetScreen, true, false, false);	// [AOC] Don't allow going back to this screen!
				}, 1);
			} break;
		}
	}

	/// <summary>
	/// The menu screen change animation is about to start.
	/// </summary>
	/// <param name="_from">Screen we come from.</param>
	/// <param name="_to">Screen we're going to.</param>
	private void OnMenuScreenTransitionStart(MenuScreen _from, MenuScreen _to) {
		// Leaving this screen
		if(_from == MenuScreen.OPEN_EGG && _to != MenuScreen.OPEN_EGG) {
			// Launch all the hide animations that are not automated
			// Restore HUD
			InstanceManager.menuSceneController.hud.animator.Show();

			// Disable drag control
			m_rewardDragController.gameObject.SetActive(false);

			// Put photo screen back in dragon mode and restore overriden setup
			if(_to != MenuScreen.PHOTO) {
				// Only if not going into it!
				PhotoScreenController photoScreen = InstanceManager.menuSceneController.GetScreenData(MenuScreen.PHOTO).ui.GetComponent<PhotoScreenController>();
				photoScreen.mode = PhotoScreenController.Mode.DRAGON;
			}
		}

		// If entering this screen, force some show/hide animations that conflict with automated ones
		if(_to == MenuScreen.OPEN_EGG) {
			// Put photo screen in EggReward mode and override some setup
			PhotoScreenController photoScreen = InstanceManager.menuSceneController.GetScreenData(MenuScreen.PHOTO).ui.GetComponent<PhotoScreenController>();
			photoScreen.mode = PhotoScreenController.Mode.EGG_REWARD;

			// Special stuff if coming back from the photo screen
			if(_from == MenuScreen.PHOTO) {
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
	private void OnMenuScreenTransitionEnd(MenuScreen _from, MenuScreen _to) {
		// Entering this screen
		if(_to == MenuScreen.OPEN_EGG) {
			// Enable drag control
			m_rewardDragController.gameObject.SetActive(m_rewardDragController.target != null);
		}
	}
}