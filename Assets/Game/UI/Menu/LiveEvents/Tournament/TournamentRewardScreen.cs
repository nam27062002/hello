using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TournamentRewardScreen : MonoBehaviour {	
	//------------------------------------------------------------------//
	// CONSTANTS														//
	//------------------------------------------------------------------//
	private enum State {
		ANIMATING,
		IDLE,
		FLOW_NOT_STARTED
	}

	private enum Step {
		INIT = 0,
		INITIAL_DELAY,
		INTRO,
		REWARD,		// As many times as needed
		FINISH
	}

	private const float INITIAL_DELAY = 0.6f;

	//------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES											//
	//------------------------------------------------------------------//
	// Step screens
	[SerializeField] private ShowHideAnimator m_introScreen = null;
	[SerializeField] private ShowHideAnimator m_rewardScreen = null;

	// Other references
	[Space]
	[SerializeField] private ShowHideAnimator m_tapToContinue = null;
	[SerializeField] private RewardInfoUI m_rewardInfo = null;
	[SerializeField] private DragControlRotation m_rewardDragController = null;

	// Individual elements references
	[Space]
	[SerializeField] private Localizer m_titleText;
	[SerializeField] private TextMeshProUGUI m_goalText = null;
	[SerializeField] private BaseIcon m_tournamentIcon = null;
	[SerializeField] private TournamentLeaderboardView m_leaderboard = null;

	// Internal references
	private RewardSceneController m_sceneController = null;

	// Internal logic
	private HDTournamentManager m_tournamentManager = null;
	private HDTournamentData m_tournamentData = null;
	private HDTournamentDefinition m_tournamentDef = null;

	private Step m_step = Step.INIT;
	private State m_state = State.FLOW_NOT_STARTED;

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
		if (m_state == State.FLOW_NOT_STARTED) {
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
			m_tournamentManager = HDLiveDataManager.tournament;
			m_tournamentData = m_tournamentManager.data as HDTournamentData;
			m_tournamentDef = m_tournamentData.definition as HDTournamentDefinition;

            // Initialize Leaderboard - before we mark the event as finished
            /*if(m_leaderboard != null) {
				m_leaderboard.Refresh();
			}*/
            // [AOC] No need! Leaderboard is self managed!

            
            // Get the icon definition
            string iconSku = m_tournamentDef.m_goal.m_icon;

            // The BaseIcon component will load the proper image or 3d model according to iconDefinition.xml
            m_tournamentIcon.LoadIcon(iconSku);
            m_tournamentIcon.gameObject.SetActive(true);
            
            // Initialize goal text
			if(m_goalText != null) {
				m_goalText.text = m_tournamentManager.GetDescription();
			}

			// Stack all rewards into the pending rewards stack
			// Add reward to the stack - In the right order!
			// From now on, if the flow is interrupted, the tournament will not appear anymore and rewards will appear as pending rewards
			if(m_tournamentManager != null) {
				// Tournament Rewards
				List<HDLiveData.Reward> rewards = m_tournamentManager.GetMyRewards();
				for(int i = 0; i < rewards.Count; ++i) {
					UsersManager.currentUser.PushReward(rewards[i].reward);
				}

				// Mark tournament as collected
				m_tournamentManager.FinishEvent();	// Mark event as collected immediately after rewards have been pushed to the stack, to prevent exploits

                // Immediately save persistence in case the rewards opening gets interrupted
                HDLiveDataManager.instance.SaveEventsToCache();
                PersistenceFacade.instance.Save_Request(true);
			}

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
			m_tapToContinue.ForceHide(false, true);
			m_tapToContinue.gameObject.SetActive(false);	// [AOC] Just in case, make sure it's not visible (it would prevent tapping the egg!)
			m_rewardDragController.gameObject.SetActive(false);

			// If already in the screen, start the flow immediately
			if(this.isActiveAndEnabled) {
				AdvanceStep();
			}
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
			MenuScreenScene menuScene = sceneController.GetScreenData(MenuScreen.TOURNAMENT_REWARD).scene3d;
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
			case Step.INTRO:			return m_introScreen;
			case Step.REWARD: 			return m_rewardScreen;
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
			case Step.INIT: {
				nextStep = Step.INITIAL_DELAY;
			} break;

			case Step.INITIAL_DELAY: {
				nextStep = Step.INTRO;
			} break;

			case Step.INTRO: {
				nextStep = Step.REWARD;
			} break;

			case Step.REWARD: {
				// There are still rewards to collect?
				if(UsersManager.currentUser.rewardStack.Count > 0) {
					nextStep = Step.REWARD;
				} else {
					nextStep = Step.FINISH;
				}
			} break;
		}

		// Hide tap to continue text
		m_tapToContinue.Hide();
		m_state = State.ANIMATING;

		// Store new step and show the right screen
		m_step = nextStep;
		ShowScreen(m_step);

		// Only display reward UI in target steps
		bool showRewardUI = (nextStep == Step.REWARD );
		m_rewardInfo.showHideAnimator.Set(showRewardUI);
		m_rewardDragController.gameObject.SetActive(showRewardUI);

		// Perform different stuff depending on new step
		switch(nextStep) {
			case Step.INITIAL_DELAY: {
				// Clear 3D scene
				m_sceneController.Clear();

				// Change state advance step after some delay
				UbiBCN.CoroutineManager.DelayedCall(
					() => { 
						m_state = State.IDLE;
						AdvanceStep();
					}, 
					INITIAL_DELAY
				);
			} break;

			case Step.INTRO: {
				// Nothing to do, animator will set the IDLE state
			} break;

			case Step.REWARD: {
				// SFX
				AudioController.Play("UI_Light FX");

				// Tell the scene to open the next reward (should be already stacked)
				m_sceneController.OpenReward();
			} break;

			case Step.FINISH: {
				// Stop listeneing the 3D scene
				m_sceneController.OnAnimStarted.RemoveListener(OnSceneAnimStarted);
				m_sceneController.OnAnimFinished.RemoveListener(OnSceneAnimFinished);

				// Purge event list
				m_tournamentManager.ClearEvent();

				// Request new event data
				if(!HDLiveDataManager.TEST_CALLS) {		// Would read the event again from the json xD
					HDLiveDataManager.instance.RequestMyLiveData(true);
				}

				// Save!
				PersistenceFacade.instance.Save_Request();

				m_state = State.FLOW_NOT_STARTED;

                // Go back to main screen
                SceneController.SetMode(SceneController.Mode.DEFAULT);
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
	/// Intro anim finished.
	/// </summary>
	public void OnIntroAnimFinished() {
		// Change logic state
		m_state = State.IDLE;

		// Show tap to continue text
		m_tapToContinue.ForceShow();
	}

	/// <summary>
	/// An animation for a reward has started in the 3d scene!
	/// </summary>
	private void OnSceneAnimStarted() {
		// [AOC] TODO!! Show currency counters, photo button, etc. based on reward type

		// If it's the first time we're getting golden fragments, show info popup
		PopupInfoGoldenFragments.CheckAndShow(
			m_sceneController.currentReward,
			1.5f,   // Enough time for the replacement animation
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
		if(_from == MenuScreen.TOURNAMENT_REWARD && _to != MenuScreen.TOURNAMENT_REWARD) {
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
		if(_to == MenuScreen.TOURNAMENT_REWARD) {
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
		if(_to == MenuScreen.TOURNAMENT_REWARD) {
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
