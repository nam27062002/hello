using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EventRewardScreen : MonoBehaviour {	
	//------------------------------------------------------------------//
	// CONSTANTS														//
	//------------------------------------------------------------------//
	private enum State {
		INIT,
		ANIMATING,
		OPEN_NEXT_REWARD
	}

	//------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES											//
	//------------------------------------------------------------------//
	// Exposed References
	[SerializeField] private ShowHideAnimator m_introScreen = null;
	[SerializeField] private ShowHideAnimator m_globalEventStepScreen = null;

	[Space]
	[SerializeField] private ShowHideAnimator m_tapToContinue = null;
	[SerializeField] private RewardInfoUI m_rewardInfo = null;
	[SerializeField] private DragControlRotation m_rewardDragController = null;

	[Space]
	[SerializeField] private GlobalEventsProgressBar m_progressBar = null;

	// Internal references
	private RewardSceneController m_sceneController = null;

	// Internal logic
	private GlobalEvent m_event;
	private int m_step;
	private State m_state;

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

		// Initialize progress bar
		m_step = 0;
		if(m_event != null) {
			m_progressBar.RefreshRewards(m_event);
		}
		m_progressBar.RefreshProgress(0);

		// Set initial state
		m_state = State.INIT;
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
			MenuScreenScene menuScene = sceneController.screensController.GetScene((int)MenuScreens.REWARD);
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
	/// Go to next reward slot.
	/// </summary>
	private void AdvanceStep() {
		// Advance progress bar
		float duration = 0.5f;
		if(m_step < m_event.rewardSlots.Count - 1) {
			m_progressBar.RefreshProgress(m_event.rewardSlots[m_step].targetPercentage, duration);
		} else {
			m_progressBar.RefreshProgress(1f, duration);
		}

		// Tell the scene to open the next reward
		m_sceneController.OpenReward();

		// Hide tap to continue text
		m_tapToContinue.Hide();

		// Update logic vars
		m_step++;
		m_state = State.ANIMATING;
	}

	//------------------------------------------------------------------//
	// CALLBACKS														//
	//------------------------------------------------------------------//
	/// <summary>
	/// The "collect rewards" button has been pressed.
	/// </summary>
	public void OnRewardButton() {
		// Toggle screens
		m_introScreen.Hide();
		m_globalEventStepScreen.Show();

		// Queue event rewards
		// Reverse order so first reward is collected first
		for(int i = m_event.rewardSlots.Count - 1; i >= 0; --i) {
			UsersManager.currentUser.rewardStack.Push(m_event.rewardSlots[i].reward);
		}

		// Open first reward!
		AdvanceStep();
	}

	/// <summary>
	/// The "Tap To Continue" button has been pressed.
	/// </summary>
	public void OnContinueButton() {
		// Ignore if we're still animating some other reward
		if(m_state == State.ANIMATING) return;

		// If it's the last reward, stop collecting
		if(m_step > m_event.rewardLevel) {
			// Mark event as collected
			m_event.FinishRewardCollection();	// [AOC] TODO!! Mark event as collected immediately after rewards have been pushed to the stack

			// Request new event data
			GlobalEventManager.RequestCurrentEventData();

			// Go back to main screen
			InstanceManager.menuSceneController.screensController.GoToScreen((int)MenuScreens.DRAGON_SELECTION);
		} else {
			// Collect next reward
			AdvanceStep();
		}
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
		m_state = State.OPEN_NEXT_REWARD;

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
		if(_from == MenuScreens.REWARD && _to != MenuScreens.REWARD) {
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
		if(_to == MenuScreens.REWARD) {
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
		if(_to == MenuScreens.REWARD) {
			// Enable drag control
			m_rewardDragController.gameObject.SetActive(m_rewardDragController.target != null);
		}
	}

	/// <summary>
	/// Screen is about to be displayed.
	/// </summary>
	public void OnShowPreAnimation() {
		// If in the INIT state, show the initial screen
		if(m_state == State.INIT) {
			// Show initial screen
			m_introScreen.RestartShow(true);
			m_globalEventStepScreen.ForceHide(false);

			// Change state!
			m_state = State.OPEN_NEXT_REWARD;
		}
	}
}
