// RewardScreenBase.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 17/01/2019.
// Copyright (c) 2019 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Base class for all screens controlling a reward flow.
/// </summary>
public abstract class IRewardScreen : MonoBehaviour {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	public enum State {
		ANIMATING,
		IDLE,
		FLOW_NOT_STARTED
	}

	// Basic steps. If more steps are needed by heirs, they should implement their 
	// own enum starting with Step.FINISH + 1 and use int cast to comunicate with the base.
	protected enum Step {
		INIT = 0,
		INITIAL_DELAY,
		INTRO,
		REWARD,     // As many times as needed
		FINISH
	}

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed setup
	[SerializeField] protected float m_initialDelay = 0f;

	// Exposed references
	[Space]
	[SerializeField] protected ShowHideAnimator m_tapToContinue = null;
	[SerializeField] protected RewardInfoUI m_rewardInfo = null;
	[SerializeField] protected DragControlRotation m_rewardDragController = null;

	// Internal references
	protected RewardSceneController m_sceneController = null;

	// Internal logic
	protected State m_state = State.FLOW_NOT_STARTED;
	protected int m_step = (int)Step.INIT;

	//------------------------------------------------------------------------//
	// ABSTRACT PROPERTIES AND METHODS										  //
	// To be implemented by heirs.											  //
	//------------------------------------------------------------------------//
	// Properties
	protected abstract MenuScreen screenID { get; }
	protected abstract int numSteps { get; }	// (int)Step.FINISH if the heir doesn't have any extra steps

	/// <summary>
	/// Get the screen associated to a specific step.
	/// </summary>
	/// <returns>The screen linked to the given step. <c>null</c> if the given step doesn't have any screen associated.</returns>
	/// <param name="_step">Step whose screen we want.</param>
	protected abstract ShowHideAnimator GetStepScreen(int _step);

	/// <summary>
	/// The flow will start as soon as possible.
	/// Use it to initialize and prepare everything.
	/// </summary>
	protected abstract void OnStartFlow();

	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	protected virtual void Awake() {
		// Subscribe to external events.
		Messenger.AddListener<MenuScreen, MenuScreen>(MessengerEvents.MENU_SCREEN_TRANSITION_START, OnMenuScreenTransitionStart);
		Messenger.AddListener<MenuScreen, MenuScreen>(MessengerEvents.MENU_SCREEN_TRANSITION_END, OnMenuScreenTransitionEnd);
	}

	/// <summary>
	/// Destructor.
	/// </summary>
	protected virtual void OnDestroy() {
		// Unsubscribe from external events
		Messenger.RemoveListener<MenuScreen, MenuScreen>(MessengerEvents.MENU_SCREEN_TRANSITION_START, OnMenuScreenTransitionStart);
		Messenger.RemoveListener<MenuScreen, MenuScreen>(MessengerEvents.MENU_SCREEN_TRANSITION_END, OnMenuScreenTransitionEnd);
	}

	//------------------------------------------------------------------//
	// FLOW CONTROL METHODS												//
	//------------------------------------------------------------------//
	/// <summary>
	/// Start the reward flow with current event in the manager.
	/// </summary>
	public void StartFlow() {
		if(m_state == State.FLOW_NOT_STARTED) {
			// Make sure all required references are set
			ValidateReferences();

			// Listen to 3D scene events - remove first to avoid receiving the event twice! (shouldn't happen, just in case)
			m_sceneController.OnAnimStarted.RemoveListener(OnSceneAnimStarted);
			m_sceneController.OnAnimFinished.RemoveListener(OnSceneAnimFinished);

			m_sceneController.OnAnimStarted.AddListener(OnSceneAnimStarted);
			m_sceneController.OnAnimFinished.AddListener(OnSceneAnimFinished);

			// Clear 3D scene
			m_sceneController.Clear();

			// Let heirs perform the initialization
			OnStartFlow();

			// Set initial state
			m_step = (int)Step.INIT;
			m_state = State.IDLE;

			// Hide all screens
			for(int i = 0; i < numSteps; ++i) {
				ShowHideAnimator screen = GetStepScreen(i);
				if(screen != null) {
					screen.ForceHide(false);
				}
			}

			// Hide other UI elements
			m_tapToContinue.ForceHide(false, true);
			m_tapToContinue.gameObject.SetActive(false);    // [AOC] Just in case, make sure it's not visible (it would prevent tapping the egg!)
			m_rewardDragController.gameObject.SetActive(false);

			// If already in the screen, start the flow immediately
			if(this.isActiveAndEnabled) {
				AdvanceStep();
			}
		}
	}

	/// <summary>
	/// Called when advancing step to select the step to go to.
	/// Override for custom steps.
	/// </summary>
	/// <returns>The next step.</returns>
	protected virtual int SelectNextStep() {
		// Check current step to decide where to go next
		int nextStep = (int)Step.INTRO;	// Default
		switch(m_step) {
			case (int)Step.INIT: {
				nextStep = (int)Step.INITIAL_DELAY;
			} break;

			case (int)Step.INITIAL_DELAY: {
				nextStep = (int)Step.INTRO;
			} break;

			case (int)Step.INTRO: {
				nextStep = (int)Step.REWARD;
			} break;

			case (int)Step.REWARD: {
				// There are still rewards to collect?
				if(UsersManager.currentUser.rewardStack.Count > 0) {
					nextStep = (int)Step.REWARD;
				} else {
					nextStep = (int)Step.FINISH;
				}
			} break;
		}
		return nextStep;
	}

	/// <summary>
	/// Called when launching a new step. Check <c>m_step</c> to know which state is being launched.
	/// Override for custom steps.
	/// </summary>
	/// <param name="_prevStep">Previous step.</param>
	/// <param name="_newStep">The step we're launching.</param>
	protected virtual void OnLaunchNewStep(int _prevStep, int _newStep) {
		// Perform different stuff depending on new step
		switch(_newStep) {
			case (int)Step.INITIAL_DELAY: {
				// Clear 3D scene
				m_sceneController.Clear();

				// Change state advance step after some delay
				UbiBCN.CoroutineManager.DelayedCall(
					() => {
						SetAnimatingState(State.IDLE);
						AdvanceStep();
					}, m_initialDelay
				);
			} break;

			case (int)Step.INTRO: {
				// Nothing to do, animator will set the IDLE state
			} break;

			case (int)Step.REWARD: {
				// SFX
				AudioController.Play("UI_Light FX");

				// Tell the scene to open the next reward (should be already stacked)
				m_sceneController.OpenReward();
			} break;

			case (int)Step.FINISH: {
				// Stop listeneing the 3D scene
				m_sceneController.OnAnimStarted.RemoveListener(OnSceneAnimStarted);
				m_sceneController.OnAnimFinished.RemoveListener(OnSceneAnimFinished);

				// Reset state
				SetAnimatingState(State.FLOW_NOT_STARTED);
			} break;
		}
	}

	/// <summary>
	/// Go to next reward slot.
	/// </summary>
	public void AdvanceStep() {
		// Check current step to decide where to go next
		int oldStep = m_step;
		int nextStep = SelectNextStep();

		// Hide tap to continue text
		ToggleTapToContinue(false);
		SetAnimatingState(State.ANIMATING);

		// Store new step and show the right screen
		m_step = nextStep;
		ShowScreen(m_step);

		// Only display reward UI in target steps
		bool showRewardUI = (nextStep == (int)Step.REWARD);
		m_rewardInfo.showHideAnimator.Set(showRewardUI);
		m_rewardDragController.gameObject.SetActive(showRewardUI);

		// Perform different stuff depending on new step
		OnLaunchNewStep(oldStep, m_step);

		//Debug.Log("<color=green>Step changed from " + oldStep + " to " + nextStep + " (" + m_givenGlobalRewards + ")</color>");
	}

	/// <summary>
	/// Toggles the tap to continue.
	/// </summary>
	/// <param name="_toggle">Show or hide?</param>
	public void ToggleTapToContinue(bool _toggle) {
		m_tapToContinue.ForceSet(_toggle);
	}

	/// <summary>
	/// Define the animating state.
	/// </summary>
	/// <param name="_state">State of the animation.</param>
	public void SetAnimatingState(State _state) {
		m_state = _state;
	}

	//------------------------------------------------------------------//
	// INTERNAL METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Make sure all required references are initialized.
	/// </summary>
	protected virtual void ValidateReferences() {
		// Get 3D scene reference for this screen
		if(m_sceneController == null) {
			MenuSceneController sceneController = InstanceManager.menuSceneController;
			Debug.Assert(sceneController != null, "This component must be only used in the menu scene!");
			MenuScreenScene menuScene = sceneController.GetScreenData(MenuScreen.TOURNAMENT_REWARD).scene3d;
			if(menuScene != null) {
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
	/// Shows the screen associated to a specific step.
	/// </summary>
	/// <param name="_step">Step to be displayed.</param>
	protected void ShowScreen(int _step) {
		for(int i = 0; i < numSteps; ++i) {
			ShowHideAnimator screen = GetStepScreen(i);
			if(screen != null) {
				screen.Set(i == _step);    // Only show target screen
			}
		}
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// The "Tap To Continue" button has been pressed.
	/// </summary>
	public virtual void OnContinueButton() {
		// Ignore if we're still animating some step (prevent spamming)
		if(m_state == State.ANIMATING) return;

		// SFX
		AudioController.Play("UI_Click");

		// Next step!
		AdvanceStep();
	}

	/// <summary>
	/// Intro anim finished.
	/// To be connected in the UI.
	/// </summary>
	// [AOC] DPRECATED Replace with a SetState() + ToggleTapToContinue()
	/*public virtual void OnIntroAnimFinished() {
		// Change logic state
		m_state = State.IDLE;

		// Show tap to continue text
		ToggleTapToContinue(true);
	}*/

	/// <summary>
	/// An animation for a reward has started in the 3d scene!
	/// </summary>
	protected virtual void OnSceneAnimStarted() {
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
	protected virtual void OnSceneAnimFinished() {
		// Change logic state
		SetAnimatingState(State.IDLE);

		// Show tap to continue text
		ToggleTapToContinue(true);
	}
	/// <summary>
	/// Screen is about to be displayed.
	/// </summary>
	public virtual void OnShowPreAnimation() {
		// If in the INIT step, start the flow!
		if(m_step == (int)Step.INIT) {
			AdvanceStep();
		}
	}

	/// <summary>
	/// The menu screen change animation is about to start.
	/// </summary>
	/// <param name="_from">Screen we come from.</param>
	/// <param name="_to">Screen we're going to.</param>
	protected virtual void OnMenuScreenTransitionStart(MenuScreen _from, MenuScreen _to) {
		// Leaving this screen
		if(_from == screenID && _to != screenID) {
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
		if(_to == screenID) {
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
	protected virtual void OnMenuScreenTransitionEnd(MenuScreen _from, MenuScreen _to) {
		// Entering this screen
		if(_to == screenID) {
			// Enable drag control
			m_rewardDragController.gameObject.SetActive(m_rewardDragController.target != null);
		}
	}
}