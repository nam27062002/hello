// DragonSelectionTutorial.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 19/04/2016.
// Copyright (c) 2016 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using System.Collections;
using DG.Tweening;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Dragon selection tutorial script, should be added to the same object as the 
/// dragon selection screen.
/// </summary>
public class DragonSelectionTutorial : MonoBehaviour {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	private enum State {
		IDLE,
		DELAY,
		RUNNING,
		BACK_DELAY,
		BACK
	};
	
	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed setup
	[SerializeField] private float m_delay = 1f;
	[SerializeField] private float m_duration = 10f;
	[SerializeField] private float m_backdelay = 1f;
	[SerializeField] private float m_backDuration = 1.5f;
	[SerializeField] private CustomEase.EaseType m_ease = CustomEase.EaseType.quartInOut_01;

	// External references
	[Space]
	[SerializeField] private CanvasGroup m_uiCanvasGroup = null;

	// Internal references
	private MenuDragonScroller m_scroller = null;

	// Internal logic
	private DeltaTimer m_timer = new DeltaTimer();
	private State m_state = State.IDLE;
	private float m_targetDelta = 0f;
	
	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {
		// Get external references
		m_scroller = InstanceManager.menuSceneController.dragonScroller;

		// Subscribe to external events. We want to receive these events even when disabled, so do it in the Awake/Destroy instead of the OnEnable/OnDisable.
		Messenger.AddListener<NavigationScreenSystem.ScreenChangedEventData>(EngineEvents.NAVIGATION_SCREEN_CHANGED, OnScreenChanged);
	}

	/// <summary>
	/// 
	/// Destructor.
	/// </summary>
	private void OnDestroy() {
		// Unsubscribe from external events.
		Messenger.RemoveListener<NavigationScreenSystem.ScreenChangedEventData>(EngineEvents.NAVIGATION_SCREEN_CHANGED, OnScreenChanged);
	}

	/// <summary>
	/// Called every frame.
	/// </summary>
	private void Update() {
		// Only if running
		switch(m_state) {
			case State.IDLE: {
				// Nothing to do
			} break;

			case State.DELAY: {
				// Timer finished?
				if(m_timer.IsFinished()) {
					// Yes! Start scrolling
					m_state = State.RUNNING;
					m_timer.Start(m_duration * 1000);
				}
			} break;

			case State.RUNNING: {
				// Timer finished?
				if(m_timer.IsFinished()) {
					// Yes! Pause before going back
					m_state = State.BACK_DELAY;
					m_timer.Start(m_backDuration * 1000);
				} else {
					// Timer not finished, scroll
					m_scroller.cameraAnimator.delta = m_timer.GetDelta(m_ease);
				}
			} break;

			case State.BACK_DELAY: {
				// Timer finished?
				if(m_timer.IsFinished()) {
					// Yes! Start scroll back animation
					m_state = State.BACK;
					m_timer.Start(m_backDuration * 1000);

					// Compute target delta
					DefinitionNode def = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.DRAGONS, UsersManager.currentUser.currentDragon);
					int menuOrder = (def == null) ? 0 : def.GetAsInt("order");
					m_targetDelta = m_scroller.cameraAnimator.cameraPath.path.GetDelta(menuOrder);
				}
			} break;

			case State.BACK: {
				// Timer finished?
				if(m_timer.IsFinished()) {
					// Yes! Stop tutorial
					StopTutorial();

					// Make sure we have the initial dragon selected
					m_scroller.FocusDragon(UsersManager.currentUser.currentDragon, true);

					// Show tier info popup
					DOVirtual.DelayedCall(0.25f, () => PopupManager.OpenPopupInstant(PopupInfoTiers.PATH));

					// Update tutorial flag and save persistence
					UsersManager.currentUser.SetTutorialStepCompleted(TutorialStep.DRAGON_SELECTION);
					PersistenceManager.Save();
				} else {
					// Timer not finished, scroll
					m_scroller.cameraAnimator.delta = Mathf.Lerp(1f, m_targetDelta, m_timer.GetDelta(m_ease));	// [AOC] Reverse scroll!
				}
			} break;
		}
	}

	//------------------------------------------------------------------------//
	// CUSTOM METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Starts the tutorial, if not already running.
	/// </summary>
	private void StartTutorial() {
		if(m_state == State.IDLE) {
			// Lock input
			InputLocker.Lock();

			// Hide HUD and UI
			InstanceManager.menuSceneController.hud.GetComponent<ShowHideAnimator>().ForceHide(false);
			if(m_uiCanvasGroup != null) m_uiCanvasGroup.alpha = 0;

			// Instant scroll to first dragon
			m_scroller.cameraAnimator.delta = 0f;

			// Start timer
			m_timer.Start(m_delay * 1000);

			// Toggle state!
			m_state = State.DELAY;
		}
	}

	/// <summary>
	/// Stops the tutorial. Doesn't update profile's persistence!
	/// </summary>
	private void StopTutorial() {
		if(m_state != State.IDLE) {
			// Unlock input
			InputLocker.Unlock();

			// Show UI back
			InstanceManager.menuSceneController.hud.GetComponent<ShowHideAnimator>().ForceShow(true);
			if(m_uiCanvasGroup != null) m_uiCanvasGroup.DOFade(1f, 0.25f);

			// Control vars
			m_state = State.IDLE;
			m_timer.Finish();
		}
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// The current menu screen has changed.
	/// </summary>
	/// <param name="_event">Event data.</param>
	public void OnScreenChanged(NavigationScreenSystem.ScreenChangedEventData _event) {
		// Only if it comes from the main screen navigation system
		if(_event.dispatcher != InstanceManager.menuSceneController.screensController) return;

		// If leaving the dragon selection screen, force the tutorial to stop (shouldn't happen)
		if(_event.toScreenIdx != (int)MenuScreens.DRAGON_SELECTION) {
			// Stop the tutorial if it's running
			StopTutorial();
			return;
		}

		// If the tutorial wasn't completed, launch it now
		if(!UsersManager.currentUser.IsTutorialStepCompleted(TutorialStep.DRAGON_SELECTION) && !DebugSettings.isPlayTest) {		// Skip tutorial for the playtests
			StartTutorial();
		}
	}
}