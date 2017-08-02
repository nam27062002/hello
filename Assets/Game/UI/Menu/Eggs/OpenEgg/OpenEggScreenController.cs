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

	[Separator("Animation Parameters")]
	[SerializeField] private float m_finalPanelDelay = 0f;
	[SerializeField] private float m_finalPanelDelayWhenFragmentsGiven = 2f;
	[SerializeField] private float m_finalPanelDelayWhenCoinsGiven = 1f;

	// Reference to 3D scene
	private RewardSceneController m_scene = null;

	// Internal
	private State m_state = State.IDLE;
	private bool m_tutorialCompletedPending = false;

	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {
		// Subscribe to external events.
		Messenger.AddListener<MenuScreens, MenuScreens>(GameEvents.MENU_SCREEN_TRANSITION_START, OnMenuScreenTransitionStart);
		Messenger.AddListener<MenuScreens, MenuScreens>(GameEvents.MENU_SCREEN_TRANSITION_END, OnMenuScreenTransitionEnd);
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

		// Push the egg reward to the stack!
		if(_egg.rewardData == null) _egg.GenerateReward();	// Generate a reward if the egg hasn't one
		UsersManager.currentUser.rewardStack.Push(_egg.rewardData);

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
		// 3d scene for this screen
		if(m_scene == null) {
			MenuSceneController sceneController = InstanceManager.menuSceneController;
			Debug.Assert(sceneController != null, "This component must be only used in the menu scene!");
			MenuScreenScene menuScene = sceneController.screensController.GetScene((int)MenuScreens.OPEN_EGG);
			if(menuScene != null) {
				// Get scene controller and initialize
				m_scene = menuScene.GetComponent<RewardSceneController>();
				if(m_scene != null) {
					// Initialize
					m_scene.InitReferences(m_rewardDragController, m_rewardInfo);

					// Subscribe to listeners
					m_scene.OnAnimStarted.AddListener(OnSceneAnimStarted);
					m_scene.OnAnimFinished.AddListener(OnSceneAnimFinished);
				}
			}
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
			bool goldenEggCompleted = EggManager.goldenEggCompleted;

			// Special initializations when reward is duplicated
			float finalPanelDelay = m_finalPanelDelay;
			ShowHideAnimator photoAnimator = InstanceManager.menuSceneController.hud.photoButton.GetComponent<ShowHideAnimator>();
			if(finalReward.WillBeReplaced()) {
				// Photo button only enabled if reward is not a duplicate!
				photoAnimator.ForceHide(false);

				// Don't show call to action button if the reward is a duplicate
				m_callToActionButton.SetActive(false);

				// Which is the replacement currency?
				if(finalReward.replacement.currency == UserProfile.Currency.GOLDEN_FRAGMENTS) {
					// Give enough time for the duplicate animation!
					finalPanelDelay = m_finalPanelDelayWhenFragmentsGiven;

					// If it's the first time we're getting golden fragments, show info popup
					if(!UsersManager.currentUser.IsTutorialStepCompleted(TutorialStep.GOLDEN_FRAGMENTS_INFO)) {
						// Show popup after some extra delay
						UbiBCN.CoroutineManager.DelayedCall(
							() => { 
								PopupManager.OpenPopupInstant(PopupInfoGoldenFragments.PATH);
								UsersManager.currentUser.SetTutorialStepCompleted(TutorialStep.GOLDEN_FRAGMENTS_INFO, true);
							},
							finalPanelDelay + 1.5f, 
							false
						);
					}
				} else if(finalReward.replacement.currency == UserProfile.Currency.SOFT) {
					// Give enough time for the duplicate animation!
					finalPanelDelay = m_finalPanelDelayWhenCoinsGiven;
				}
			} else {
				// Photo button only enabled if reward is not a duplicate!
				photoAnimator.Show();	// Only animate if showing

				// Don't show call to action button if the reward is a duplicate
				m_callToActionText.Localize("TID_EGG_SHOW_REWARD");
				m_callToActionButton.SetActive(true);
			}

			// Initialize and show final panel
			UbiBCN.CoroutineManager.DelayedCall(
				() => { m_finalPanel.Show(); }, 
				finalPanelDelay - m_finalPanel.tweenDelay, false	// Compensate the delay of the ShowHideAnimator (the delay is meant to screen transitions only)
			);

			// Same with HUD - unless golden egg was completed
			if(!goldenEggCompleted) {
				UbiBCN.CoroutineManager.DelayedCall(
					() => { InstanceManager.menuSceneController.hud.animator.Show(); },
					finalPanelDelay, false
				);
			}

			// Don't show back button if we've completed a golden egg!
			m_backButton.SetActive(!goldenEggCompleted);

			// Same with egg buy button
			m_buyEggButton.SetActive(!goldenEggCompleted);

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
		switch(m_scene.eggData.rewardData.reward.type) {
			case Metagame.RewardPet.TYPE_CODE: {
				// Make sure selected dragon is owned
				InstanceManager.menuSceneController.dragonSelector.SetSelectedDragon(DragonManager.currentDragon.def.sku);	// Current dragon is the last owned selected dragon

				// Go to the pets screen
				PetsScreenController petScreen = screensController.GetScreen((int)MenuScreens.PETS).GetComponent<PetsScreenController>();
				petScreen.Initialize(m_scene.eggData.rewardData.reward.sku);
				screensController.GoToScreen((int)MenuScreens.PETS);
			} break;
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
			// Put photo screen in EggReward mode and override some setup
			PhotoScreenController photoScreen = InstanceManager.menuSceneController.GetScreen(MenuScreens.PHOTO).GetComponent<PhotoScreenController>();
			photoScreen.mode = PhotoScreenController.Mode.EGG_REWARD;

			// Special stuff if coming back from the photo screen
			if(_from == MenuScreens.PHOTO) {
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