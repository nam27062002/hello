using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PendingRewardScreen : MonoBehaviour {	
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
		REWARD,		// As many times as needed
		FINISH
	}

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

	// Internal references
	private RewardSceneController m_sceneController = null;

	// Internal logic
	private Step m_step;
	private State m_state;
	private bool m_showIntro = true;

    private bool m_specialDragonUnlocked = false;

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
	/// Start the pending rewards flow.
	/// </summary>
	/// <param name="_showIntroScreen">Whether to show the intro screen or go straight to the chicha.</param>
	public void StartFlow(bool _showIntroScreen) {
		// Make sure all required references are set
		ValidateReferences();

        m_specialDragonUnlocked = false;

        // Listen to 3D scene events - remove first to avoid receiving the event twice! (shouldn't happen, just in case)
        m_sceneController.OnAnimStarted.RemoveListener(OnSceneAnimStarted);
		m_sceneController.OnAnimFinished.RemoveListener(OnSceneAnimFinished);

		m_sceneController.OnAnimStarted.AddListener(OnSceneAnimStarted);
		m_sceneController.OnAnimFinished.AddListener(OnSceneAnimFinished);

		// Clear 3D scene
		m_sceneController.Clear();

		// Set initial state
		m_step = Step.INIT;
		m_state = State.IDLE;
		m_showIntro = _showIntroScreen;

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
			MenuScreenScene menuScene = sceneController.GetScreenData(MenuScreen.PENDING_REWARD).scene3d;
			if(menuScene != null) {
				// Get scene controller and initialize
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
			case Step.INTRO:	return m_introScreen;		break;
			case Step.REWARD:	return m_rewardScreen;		break;
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

			default: {
				// Coming from INIT or FINISH steps: Show intro?
				if(m_showIntro) {
					nextStep = Step.INTRO;
				} else {
					nextStep = Step.REWARD;
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
		bool showRewardUI = (nextStep == Step.REWARD);
		m_rewardInfo.showHideAnimator.Set(showRewardUI);
		m_rewardDragController.gameObject.SetActive(showRewardUI);

		// Perform different stuff depending on new step
		switch(nextStep) {
			case Step.INTRO: {
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

			case Step.REWARD: {
                    if (UsersManager.currentUser.rewardStack.Count > 0) {
                        Metagame.Reward reward = UsersManager.currentUser.rewardStack.Peek();
                        Metagame.RewardDragon rewardDragon = reward as Metagame.RewardDragon;
                        if (rewardDragon != null) {
                            IDragonData dragonData = DragonManager.GetDragonData(rewardDragon.sku);
                            m_specialDragonUnlocked = dragonData is DragonDataSpecial;
                        }
                    }

                    // Tell the scene to open the next reward (should be already stacked)
                    m_sceneController.OpenReward();
    			} break;

			case Step.FINISH: {
    				// Stop listeneing the 3D scene
    				m_sceneController.OnAnimStarted.RemoveListener(OnSceneAnimStarted);
    				m_sceneController.OnAnimFinished.RemoveListener(OnSceneAnimFinished);

                    // Go back to previous screen
                    if (m_specialDragonUnlocked) {
                        HDLiveDataManager.instance.SwitchToLeague();
                        InstanceManager.menuSceneController.GoToScreen(MenuScreen.DRAGON_SELECTION);
                    } else {
                        InstanceManager.menuSceneController.transitionManager.Back(true);
                    }
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
		if(_from == MenuScreen.PENDING_REWARD && _to != MenuScreen.PENDING_REWARD) {
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
		if(_to == MenuScreen.PENDING_REWARD) {
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
		if(_to == MenuScreen.PENDING_REWARD) {
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
