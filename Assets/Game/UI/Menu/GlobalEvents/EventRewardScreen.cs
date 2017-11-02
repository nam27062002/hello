using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EventRewardScreen : MonoBehaviour {	
	//------------------------------------------------------------------//
	// CONSTANTS														//
	//------------------------------------------------------------------//
	private enum State {
		ANIMATING,
		IDLE
	}

	private enum Step {
		INIT = 0,
		INTRO,
		GLOBAL_REWARD,		// As much times as needed
		NO_GLOBAL_REWARD,	// When the global score hasn't reached the threshold for the minimum reward, show a special screen
		TOP_REWARD_INTRO,	// When the player has been classified for the top reward
		TOP_REWARD,			// When the player has been classified for the top reward
		FINISH
	}

	//------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES											//
	//------------------------------------------------------------------//
	// Step screens
	[SerializeField] private ShowHideAnimator m_introScreen = null;
	[SerializeField] private ShowHideAnimator m_globalRewardScreen = null;
	[SerializeField] private ShowHideAnimator m_noGlobalRewardScreen = null;
	[SerializeField] private ShowHideAnimator m_topRewardIntroScreen = null;

	// Other references
	[Space]
	[SerializeField] private ShowHideAnimator m_tapToContinue = null;
	[SerializeField] private RewardInfoUI m_rewardInfo = null;
	[SerializeField] private DragControlRotation m_rewardDragController = null;

	// Individual elements references
	[Space]
	[SerializeField] private GlobalEventsProgressBar m_progressBar = null;
	[SerializeField] private Image m_eventIcon = null;
	[Space]
	[SerializeField] private Localizer m_topRewardText = null;
	[SerializeField] private GlobalEventsRewardInfo m_topRewardInfo = null;

	// Internal references
	private RewardSceneController m_sceneController = null;

	// Internal logic
	private GlobalEvent m_event;
	private Step m_step;
	private State m_state;
	private int m_givenGlobalRewards;

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
	/// Component has been disabled.
	/// </summary>
	void OnDisable() {
		
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
	/// Start the reward flow with current event in the manager.
	/// </summary>
	public void StartFlow() {
		// Make sure all required references are set
		ValidateReferences();

		// Clear 3D scene
		m_sceneController.Clear();

		// Store current event for faster access
		m_event = GlobalEventManager.currentEvent;

		// Stack all rewards into the pending rewards stack
		// Add reward to the stack - In the right order!
		if(m_event != null) {
			// Top contribution reward
			if(m_event.topContributor) {
				UsersManager.currentUser.PushReward(m_event.topContributorsRewardSlot.reward);
			}

			// Global rewards
			for(int i = m_event.rewardLevel - 1; i >= 0; --i) {
				UsersManager.currentUser.PushReward(m_event.rewardSlots[i].reward);
			}

			// Immediately save persistence in case the rewards opening gets interrupted
			PersistenceFacade.instance.Save_Request(true);

			// Mark event as collected
			m_event.FinishRewardCollection();	// Mark event as collected immediately after rewards have been pushed to the stack, to prevent exploits
		}

		// Initialize progress bar
		m_givenGlobalRewards = 0;
		if(m_event != null) {
			m_progressBar.RefreshRewards(m_event);
		}
		m_progressBar.RefreshProgress(0);

		// Set initial state
		m_step = Step.INIT;
		m_state = State.IDLE;

		// Hide all screens
		for(int i = 0; i < (int)Step.FINISH; ++i) {
			ShowHideAnimator screen = GetScreen((Step)i);
			if(screen != null) {
				screen.ForceHide(false);
			}
		}

		// Hide other UI elements
		m_tapToContinue.ForceHide(false);
		m_rewardDragController.gameObject.SetActive(false);

		// If already in the screen, start the flow immediately
		if(this.isActiveAndEnabled) {
			AdvanceStep();
		}
	}

	//------------------------------------------------------------------//
	// INTERNAL METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Make sure all required references are initialized.
	/// </summary>
	private void ValidateReferences() {
		// 3d scene for this screen
		if(m_sceneController == null) {
			MenuSceneController sceneController = InstanceManager.menuSceneController;
			Debug.Assert(sceneController != null, "This component must be only used in the menu scene!");
			MenuScreenScene menuScene = sceneController.screensController.GetScene((int)MenuScreens.EVENT_REWARD);
			if (menuScene != null) {
				// Get scene controller and initialize
				m_sceneController = menuScene.GetComponent<RewardSceneController>();
				if(m_sceneController != null) {
					// Initialize
					m_sceneController.InitReferences(m_rewardDragController, m_rewardInfo);

					// Subscribe to listeners
					m_sceneController.OnAnimStarted.AddListener(OnSceneAnimStarted);
					m_sceneController.OnAnimFinished.AddListener(OnSceneAnimFinished);
				}
			}
		}
	}

	/// <summary>
	/// Get the screen associated to a specific step.
	/// </summary>
	/// <returns>The screen linked to the given step.</returns>
	/// <param name="_step">Step whose screen we want.</param>
	private ShowHideAnimator GetScreen(Step _step) {
		switch(_step) {
			case Step.INTRO:			return m_introScreen;			break;
			case Step.GLOBAL_REWARD:	return m_globalRewardScreen;	break;
			case Step.NO_GLOBAL_REWARD:	return m_noGlobalRewardScreen;	break;
			case Step.TOP_REWARD_INTRO: return m_topRewardIntroScreen;	break;
		}
		return null;
	}

	/// <summary>
	/// Shows the screen associated to a specific step-
	/// </summary>
	/// <param name="_step">Step to be displayed.</param>
	private void ShowScreen(Step _step) {
		for(int i = 0; i < (int)Step.FINISH; ++i) {
			ShowHideAnimator screen = GetScreen((Step)i);
			if(screen != null) {
				screen.Set(i == (int)_step);	// Only show target screen
			}
		}
	}

	/// <summary>
	/// Go to next reward slot.
	/// </summary>
	private void AdvanceStep() {
		// Check current step to decide where to go next
		Step oldStep = m_step;
		Step nextStep = Step.INTRO;
		switch(m_step) {
			case Step.INTRO: {
				// Do we have global rewards?
				if(m_event.rewardLevel > 0) {
					nextStep = Step.GLOBAL_REWARD;
				} else {
					nextStep = Step.NO_GLOBAL_REWARD;
				}
			} break;

			case Step.GLOBAL_REWARD: {
				// There are still rewards to collect?
				if(m_givenGlobalRewards < m_event.rewardLevel) {
					nextStep = Step.GLOBAL_REWARD;
				} else {
					// No! Has top reward?
					if(m_event.topContributor) {
						nextStep = Step.TOP_REWARD_INTRO;
					} else {
						nextStep = Step.FINISH;
					}
				}
			} break;

			case Step.NO_GLOBAL_REWARD: {
				// Has top reward?
				if(m_event.topContributor) {
					nextStep = Step.TOP_REWARD_INTRO;
				} else {
					nextStep = Step.FINISH;
				}
			} break;

			case Step.TOP_REWARD_INTRO: {
				nextStep = Step.TOP_REWARD;
			} break;

			case Step.TOP_REWARD: {
				nextStep = Step.FINISH;
			} break;
		}

		// Hide tap to continue text
		m_tapToContinue.Hide();
		m_state = State.ANIMATING;

		// Store new step and show the right screen
		m_step = nextStep;
		ShowScreen(m_step);

		// Only display reward UI in target steps
		bool showRewardUI = (nextStep == Step.GLOBAL_REWARD || nextStep == Step.TOP_REWARD);
		m_rewardInfo.showHideAnimator.Set(showRewardUI);
		m_rewardDragController.gameObject.SetActive(showRewardUI);

		// Perform different stuff depending on new step
		switch(nextStep) {
			case Step.INTRO: {
				// Set event icon
				m_eventIcon.sprite = Resources.Load<Sprite>(UIConstants.MISSION_ICONS_PATH + m_event.objective.icon);

				// Clear 3D scene
				m_sceneController.Clear();

				// Change state after some delay
				UbiBCN.CoroutineManager.DelayedCall(
					() => { 
						m_state = State.IDLE;
					}, 
					0.5f
				);
			} break;

			case Step.GLOBAL_REWARD: {
				// Animate progress bar
				m_progressBar.RefreshProgress(m_event.rewardSlots[m_givenGlobalRewards].targetPercentage, 0.5f);

				// Tell the scene to open the next reward (should be already stacked)
				m_sceneController.OpenReward();

				// Increase global reward level
				m_givenGlobalRewards++;
			} break;

			case Step.NO_GLOBAL_REWARD: {
				// Clear 3D scene
				m_sceneController.Clear();

				// Restore tap to continue after some delay
				UbiBCN.CoroutineManager.DelayedCall(
					() => { 
						m_state = State.IDLE;
						m_tapToContinue.Show(); 
					}, 
					0.5f
				);
			} break;

			case Step.TOP_REWARD_INTRO: {
				// Clear 3D scene
				m_sceneController.Clear();

				// Initialize text
				float topPercentile = m_event.topContributorsRewardSlot.targetPercentage * 100f;
				m_topRewardText.Localize(
					m_topRewardText.tid, 
					StringUtils.FormatNumber(topPercentile, 2)
				);

				// Initialize reward info
				m_topRewardInfo.InitFromReward(m_event.topContributorsRewardSlot);

				// Change state after some delay
				UbiBCN.CoroutineManager.DelayedCall(
					() => {
						m_state = State.IDLE;
					},
					0.5f
				);
			} break;

			case Step.TOP_REWARD: {
				// Tell the scene to open the next reward (should be already stacked)
				m_sceneController.OpenReward();
			} break;

			case Step.FINISH: {
				// Purge event list
				GlobalEventManager.ClearRewardedEvents();
				GlobalEventManager.ClearCurrentEvent();

				// Request new event data
				GlobalEventManager.TMP_RequestCustomizer();

				// Save!
				PersistenceFacade.instance.Save_Request();

				// Go back to main screen
				InstanceManager.menuSceneController.screensController.GoToScreen((int)MenuScreens.DRAGON_SELECTION);
			} break;
		}

		//Debug.Log("<color=green>Step changed from " + oldStep + " to " + nextStep + " (" + m_givenGlobalRewards + ")</color>");
	}

	//------------------------------------------------------------------//
	// CALLBACKS														//
	//------------------------------------------------------------------//
	/// <summary>
	/// The "Tap To Continue" button has been pressed.
	/// </summary>
	public void OnContinueButton() {
		// Ignore if we're still animating some step (prevent spamming)
		if(m_state == State.ANIMATING) return;

		// Next step!
		AdvanceStep();
	}

	/// <summary>
	/// An animation for a reward has started in the 3d scene!
	/// </summary>
	private void OnSceneAnimStarted() {
		// [AOC] TODO!! Show currency counters, photo button, etc. based on reward type

		// If it's the first time we're getting golden fragments, show info popup
		Metagame.Reward currentReward = m_sceneController.currentReward;
		if(currentReward.WillBeReplaced()) {
			if(currentReward.replacement.currency == UserProfile.Currency.GOLDEN_FRAGMENTS) {
				if(!UsersManager.currentUser.IsTutorialStepCompleted(TutorialStep.GOLDEN_FRAGMENTS_INFO)) {
					// Show popup after some extra delay
					UbiBCN.CoroutineManager.DelayedCall(
						() => { 
							// Tracking
							string popupName = System.IO.Path.GetFileNameWithoutExtension(PopupInfoGoldenFragments.PATH);
							HDTrackingManager.Instance.Notify_InfoPopup(popupName, "automatic");

							PopupManager.OpenPopupInstant(PopupInfoGoldenFragments.PATH);
							UsersManager.currentUser.SetTutorialStepCompleted(TutorialStep.GOLDEN_FRAGMENTS_INFO, true);
						},
						1.5f, 	// Enough time for the replacement animation!
						false
					);
				}
			}
		}
	}

	/// <summary>
	/// The reward animation has finished on the 3d scene.
	/// </summary>
	private void OnSceneAnimFinished() {
		// Change logic state
		m_state = State.IDLE;

		// Show tap to continue text
		m_tapToContinue.Show();
	}

	/// <summary>
	/// The menu screen change animation is about to start.
	/// </summary>
	/// <param name="_from">Screen we come from.</param>
	/// <param name="_to">Screen we're going to.</param>
	private void OnMenuScreenTransitionStart(MenuScreens _from, MenuScreens _to) {
		// Leaving this screen
		if(_from == MenuScreens.EVENT_REWARD && _to != MenuScreens.EVENT_REWARD) {
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
		if(_to == MenuScreens.EVENT_REWARD) {
			// Hide HUD!
			InstanceManager.menuSceneController.hud.animator.Hide();

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
		if(_to == MenuScreens.EVENT_REWARD) {
			// Enable drag control
			m_rewardDragController.gameObject.SetActive(m_rewardDragController.target != null);
		}
	}

	/// <summary>
	/// Screen is about to be displayed.
	/// </summary>
	public void OnShowPreAnimation() {
		// If in the INIT step, start the flow!
		if(m_step == Step.INIT) {
			AdvanceStep();
		}
	}
}
