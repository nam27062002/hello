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
[RequireComponent(typeof(CanvasGroup))]
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
	private MenuDragonScroller3D m_scroller = null;
	private CanvasGroup m_canvasGroup = null;

	// Internal logic
	private DeltaTimer m_timer = new DeltaTimer();
	private State m_state = State.IDLE;
	
	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {
		// Get external references
		MenuDragonScreenController screenController = InstanceManager.GetSceneController<MenuSceneController>().GetScreen(MenuScreens.DRAGON_SELECTION).GetComponent<MenuDragonScreenController>(); 
		m_scroller = screenController.dragonScroller3D;
		m_canvasGroup = GetComponent<CanvasGroup>();

		// Subscribe to external events. We want to receive these events even when disabled, so do it in the Awake/Destroy instead of the OnEnable/OnDisable.
		Messenger.AddListener<NavigationScreenSystem.ScreenChangedEvent>(EngineEvents.NAVIGATION_SCREEN_CHANGED, OnScreenChanged);
	}

	/// <summary>
	/// 
	/// Destructor.
	/// </summary>
	private void OnDestroy() {
		// Unsubscribe from external events.
		Messenger.RemoveListener<NavigationScreenSystem.ScreenChangedEvent>(EngineEvents.NAVIGATION_SCREEN_CHANGED, OnScreenChanged);
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
					m_scroller.delta = m_timer.GetDelta(m_ease);
				}
			} break;

			case State.BACK_DELAY: {
				// Timer finished?
				if(m_timer.IsFinished()) {
					// Yes! Start scroll back animation
					m_state = State.BACK;
					m_timer.Start(m_backDuration * 1000);
				}
			} break;

			case State.BACK: {
				// Timer finished?
				if(m_timer.IsFinished()) {
					// Yes! Stop tutorial
					StopTutorial();

					// Make sure we have the first dragon selected
					m_scroller.SnapTo(0);

					// Update tutorial flag and save persistence
					UserProfile.SetTutorialStepCompleted(TutorialStep.DRAGON_SELECTION);
					PersistenceManager.Save();
				} else {
					// Timer not finished, scroll
					m_scroller.delta = 1f - m_timer.GetDelta(m_ease);	// [AOC] Reverse scroll!
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
			InstanceManager.GetSceneController<MenuSceneController>().hud.GetComponent<ShowHideAnimator>().ForceHide(false);
			if(m_canvasGroup != null) m_canvasGroup.alpha = 0;

			// Instant scroll to first dragon
			m_scroller.delta = 0f;

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
			InstanceManager.GetSceneController<MenuSceneController>().hud.GetComponent<ShowHideAnimator>().ForceShow(true);
			if(m_canvasGroup != null) m_canvasGroup.DOFade(1f, 0.25f);

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
	public void OnScreenChanged(NavigationScreenSystem.ScreenChangedEvent _event) {
		// Only if it comes from the main screen navigation system
		if(_event.dispatcher != InstanceManager.GetSceneController<MenuSceneController>().screensController) return;

		// If leaving the dragon selection screen, force the tutorial to stop (shouldn't happen)
		if(_event.toScreenIdx != (int)MenuScreens.DRAGON_SELECTION) {
			// Stop the tutorial if it's running
			StopTutorial();
			return;
		}

		// If the tutorial wasn't completed, launch it now
		if(!UserProfile.IsTutorialStepCompleted(TutorialStep.DRAGON_SELECTION)) {
			StartTutorial();
		}
	}
}