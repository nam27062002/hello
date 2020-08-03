using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

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
		DIDNT_CONTRIBUTE,	// When the player didn't contribute to the global event
		GLOBAL_REWARD,		// As much times as needed
		NO_GLOBAL_REWARD,	// When the global score hasn't reached the threshold for the minimum reward, show a special screen
		FINISH
	}

	//------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES											//
	//------------------------------------------------------------------//
	// Step screens
	[SerializeField] private ShowHideAnimator m_introScreen = null;
	[SerializeField] private ShowHideAnimator m_didntContribute = null;
	[SerializeField] private ShowHideAnimator m_globalRewardScreen = null;
	[SerializeField] private ShowHideAnimator m_noGlobalRewardScreen = null;

	// Other references
	[Space]
	[SerializeField] private ShowHideAnimator m_tapToContinue = null;
	[SerializeField] private RewardInfoUI m_rewardInfo = null;
	[SerializeField] private DragControlRotation m_rewardDragController = null;

	// Individual elements references
	[Space]
	[SerializeField] private GlobalEventsPanelActive m_questPanel = null;
	[SerializeField] private BaseIcon m_eventIcon = null;
	[SerializeField] private TextMeshProUGUI m_objectiveText = null;

	// Internal references
	private RewardSceneController m_sceneController = null;

	// Internal logic
	private HDQuestManager m_questManager;
	private Step m_step;
	private State m_state;

	private int m_questRewardIdx = -1;
	private List<int> m_pushedRewardsAmount = new List<int>();

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
	/// Component has been disabled.
	/// </summary>
	void OnDisable() {
		
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
	/// Start the reward flow with current event in the manager.
	/// </summary>
	public void StartFlow() {
		// Make sure all required references are set
		ValidateReferences();

		// Listen to 3D scene events - remove first to avoid receiving the event twice! (shouldn't happen, just in case)
		m_sceneController.OnAnimStarted.RemoveListener(OnSceneAnimStarted);
		m_sceneController.OnAnimFinished.RemoveListener(OnSceneAnimFinished);

		m_sceneController.OnAnimStarted.AddListener(OnSceneAnimStarted);
		m_sceneController.OnAnimFinished.AddListener(OnSceneAnimFinished);

		// Clear 3D scene
		m_sceneController.Clear();

		// Store current event for faster access
		m_questManager = HDLiveDataManager.quest;

		// Set initial state
		m_step = Step.INIT;
		m_state = State.IDLE;

		// Stack all rewards into the pending rewards stack
		// Add reward to the stack - In the right order!
		if(m_questManager != null) {
			// Global rewards
			// Rewards are sorted from smaller to bigger, push them in reverse order to collect smaller ones first
			List<HDLiveData.Reward> rewards = m_questManager.GetMyRewards();
			int[] pushedRewardsAmount = new int[rewards.Count];
			int totalPushedRewards = UsersManager.currentUser.rewardStack.Count;
			for(int i = rewards.Count - 1; i >= 0; --i) {
				UsersManager.currentUser.PushReward(rewards[i].reward);

				// [AOC] Tricky stuff: a reward can immediately push other rewards (i.e. 4 eggs), making it difficult to sync with the actual quest reward level
				//		 Use a list to work around it
				pushedRewardsAmount[i] = UsersManager.currentUser.rewardStack.Count - totalPushedRewards;
				totalPushedRewards = UsersManager.currentUser.rewardStack.Count;
			}
			m_pushedRewardsAmount.Clear();
			m_pushedRewardsAmount.AddRange(pushedRewardsAmount);
			m_questRewardIdx = -1;

			// Be aware when more rewards are pushed during the flow
			Messenger.RemoveListener<Metagame.Reward>(MessengerEvents.PROFILE_REWARD_PUSHED, OnRewardPushed);
			Messenger.RemoveListener<Metagame.Reward>(MessengerEvents.PROFILE_REWARD_POPPED, OnRewardPopped);
			Messenger.AddListener<Metagame.Reward>(MessengerEvents.PROFILE_REWARD_PUSHED, OnRewardPushed);
			Messenger.AddListener<Metagame.Reward>(MessengerEvents.PROFILE_REWARD_POPPED, OnRewardPopped);

			// Mark event as collected
			m_questManager.FinishEvent();

            // Immediately save persistence in case the rewards opening gets interrupted
            HDLiveDataManager.instance.SaveEventsToCache();
            PersistenceFacade.instance.Save_Request(true);
		}

		// Initialize progress bar
		m_questPanel.Refresh();
		m_questPanel.MoveScoreTo(0, 0, 0f);

		// Hide all screens
		for(int i = 0; i < (int)Step.FINISH; ++i) {
			ShowHideAnimator screen = GetScreen((Step)i);
			if(screen != null) {
				screen.ForceHide(false);
			}
		}

		// Hide other UI elements
		m_tapToContinue.ForceHide(false);
		m_tapToContinue.gameObject.SetActive(false);	// [AOC] Just in case, make sure it's not visible (it would prevent tapping the egg!)
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
		// Get 3D scene reference for this screen
		if(m_sceneController == null) {
			MenuSceneController sceneController = InstanceManager.menuSceneController;
			Debug.Assert(sceneController != null, "This component must be only used in the menu scene!");
			MenuScreenScene menuScene = sceneController.GetScreenData(MenuScreen.EVENT_REWARD).scene3d;
			if (menuScene != null) {
				// Get scene controller reference
				m_sceneController = menuScene.GetComponent<RewardSceneController>();
			}
		}

		// Tell the scene it will be working with this screen
		if(m_sceneController != null) {
			m_sceneController.InitReferences(m_rewardDragController, m_rewardInfo);
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
			case Step.DIDNT_CONTRIBUTE: return m_didntContribute;		break;
			case Step.GLOBAL_REWARD:	return m_globalRewardScreen;	break;
			case Step.NO_GLOBAL_REWARD:	return m_noGlobalRewardScreen;	break;
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
				//if(m_questManager.m_questData.m_rewardLevel > 0) {
				//if(UsersManager.currentUser.rewardStack.Count > 0) {
				if(m_pushedRewardsAmount.Count > 0) {
					nextStep = Step.GLOBAL_REWARD;
				} else {
					nextStep = Step.NO_GLOBAL_REWARD;
				}
			} break;
			case Step.DIDNT_CONTRIBUTE:{
				nextStep = Step.FINISH;
			}break;
			case Step.GLOBAL_REWARD: {
				// Are there still rewards to collect?
				if(UsersManager.currentUser.rewardStack.Count > 0) {
					nextStep = Step.GLOBAL_REWARD;
				} else {
					nextStep = Step.FINISH;
				}
			} break;

			case Step.NO_GLOBAL_REWARD: {
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
		bool showRewardUI = (nextStep == Step.GLOBAL_REWARD );
		m_rewardInfo.showHideAnimator.Set(showRewardUI);
		m_rewardDragController.gameObject.SetActive(showRewardUI);

		// Perform different stuff depending on new step
		switch(nextStep) {
			case Step.INTRO: {

                // Get the icon definition
                string iconSku = m_questManager.m_questDefinition.m_goal.m_icon;

                // The BaseIcon component will load the proper image or 3d model according to iconDefinition.xml
                m_eventIcon.LoadIcon(iconSku);
                m_eventIcon.gameObject.SetActive(true);


				// Event description
				if(m_objectiveText != null) m_objectiveText.text = m_questManager.GetGoalDescription();

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
				// Keep track of given rewards
				Debug.Log(Colors.paleYellow.Tag("checking index... (" + m_questRewardIdx + ":" + (m_questRewardIdx < 0 ? "-" : m_pushedRewardsAmount[m_questRewardIdx].ToString()) + ")"));
				if(m_questRewardIdx < 0 || m_pushedRewardsAmount[m_questRewardIdx] <= 0) {
					m_questRewardIdx++;
					Debug.Log(Colors.paleYellow.Tag("INDEX INCREASED " + m_questRewardIdx));
				}

				// SFX
				AudioController.Play("UI_Light FX");

				// Animate progress bar
				m_questPanel.MoveScoreTo(m_questManager.m_questDefinition.m_rewards[m_questRewardIdx].target, 0.5f);

				// Tell the scene to open the next reward (should be already stacked)
				m_sceneController.OpenReward();
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

			case Step.DIDNT_CONTRIBUTE: {
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
			case Step.FINISH: {
				// Stop listeneing the 3D scene
				m_sceneController.OnAnimStarted.RemoveListener(OnSceneAnimStarted);
				m_sceneController.OnAnimFinished.RemoveListener(OnSceneAnimFinished);

				// Stop listening to external events too
				Messenger.RemoveListener<Metagame.Reward>(MessengerEvents.PROFILE_REWARD_PUSHED, OnRewardPushed);
				Messenger.RemoveListener<Metagame.Reward>(MessengerEvents.PROFILE_REWARD_POPPED, OnRewardPopped);

				// Purge event list
				m_questManager.ClearEvent();

				// Request new event data
				if(!HDLiveDataManager.TEST_CALLS) {		// Would read the event again from the json xD
					HDLiveDataManager.instance.RequestMyLiveData(true);
				}

				// Save!
				PersistenceFacade.instance.Save_Request();

                    // Go back to main screen
                HDLiveDataManager.instance.SwitchToQuest();
                InstanceManager.menuSceneController.GoToScreen(MenuScreen.DRAGON_SELECTION);
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

		// SFX
		AudioController.Play("UI_Click");

		// Next step!
		AdvanceStep();
	}

	/// <summary>
	/// An animation for a reward has started in the 3d scene!
	/// </summary>
	private void OnSceneAnimStarted() {
		// [AOC] TODO!! Show currency counters, photo button, etc. based on reward type

		// If it's the first time we're getting golden fragments, show info popup
		PopupInfoGoldenFragments.CheckAndShow(
			m_sceneController.currentReward,
			1.5f,	// Enough time for the replacement animation
			PopupLauncher.TrackingAction.INFO_POPUP_AUTO
		);
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
	private void OnMenuScreenTransitionStart(MenuScreen _from, MenuScreen _to) {
		// Leaving this screen
		if(_from == MenuScreen.EVENT_REWARD && _to != MenuScreen.EVENT_REWARD) {
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
		if(_to == MenuScreen.EVENT_REWARD) {
			// Hide HUD!
			InstanceManager.menuSceneController.hud.animator.Hide();

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
		if(_to == MenuScreen.EVENT_REWARD) {
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

	/// <summary>
	/// A reward has been pushed to the stack.
	/// </summary>
	/// <param name="_reward">Reward.</param>
	private void OnRewardPushed(Metagame.Reward _reward) {
		// Increase counter
		m_pushedRewardsAmount[m_questRewardIdx]++;
		Debug.Log(Colors.paleGreen.Tag(m_questRewardIdx + ": " + m_pushedRewardsAmount[m_questRewardIdx]));
	}

	/// <summary>
	/// A reward has been popped to the stack.
	/// </summary>
	/// <param name="_reward">Reward.</param>
	private void OnRewardPopped(Metagame.Reward _reward) {
		// Decrease counter
		m_pushedRewardsAmount[m_questRewardIdx]--;
		Debug.Log(Colors.coral.Tag(m_questRewardIdx + ": " + m_pushedRewardsAmount[m_questRewardIdx]));
	}
}
