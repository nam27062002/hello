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
using System.Collections.Generic;

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
	[Space]
	[SerializeField] private CustomEase.EaseType m_easeForward = CustomEase.EaseType.quartInOut_01;
	[SerializeField] private CustomEase.EaseType m_easeBackward = CustomEase.EaseType.quartInOut_01;

	// External references
	[Space]
	[SerializeField] private CanvasGroup m_uiCanvasGroup = null;

	// Public properties
	public bool isPlaying {
		get { return m_state != State.IDLE; }
	}

	// Internal references
	private MenuDragonScroller m_scroller = null;
	private List<ParticleSystem> m_pausedParticles = new List<ParticleSystem>();

	// Internal logic
	private DeltaTimer m_timer = new DeltaTimer();
	private State m_state = State.IDLE;
	private float m_targetDelta = 0f;
	private float m_lastDelta = 1f;
	
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
	private void FixedUpdate() {
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
					m_scroller.cameraAnimator.delta = m_timer.GetDelta(m_easeForward) * m_lastDelta;
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
					UbiBCN.CoroutineManager.DelayedCall(() => {
						// Tracking
						string popupName = System.IO.Path.GetFileNameWithoutExtension(PopupInfoTiers.PATH);
						HDTrackingManager.Instance.Notify_InfoPopup(popupName, "automatic");

						// Open popup
						PopupManager.OpenPopupInstant(PopupInfoTiers.PATH);
					}, 0.25f);

					// Update tutorial flag and save persistence
					UsersManager.currentUser.SetTutorialStepCompleted(TutorialStep.DRAGON_SELECTION);

					// Tracking!
					HDTrackingManager.Instance.Notify_Funnel_FirstUX(FunnelData_FirstUX.Steps._06b_animation_done);

					PersistenceFacade.instance.Save_Request();
                } else {
					// Timer not finished, scroll
					m_scroller.cameraAnimator.delta = Mathf.Lerp(m_lastDelta, m_targetDelta, m_timer.GetDelta(m_easeBackward));	// [AOC] Reverse scroll!
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
	public void StartTutorial() {
		// Ignore if not in the IDLE state
		if(m_state != State.IDLE) return;

		// Lock all input
		Messenger.Broadcast<bool>(EngineEvents.UI_LOCK_INPUT, true);

		// Hide HUD and UI
		InstanceManager.menuSceneController.hud.animator.ForceHide(false);
		if(m_uiCanvasGroup != null) {
			m_uiCanvasGroup.alpha = 0;

			// Particle Systems are not affected by canvas groups, so manually pause them
			ParticleSystem[] particles = m_uiCanvasGroup.GetComponentsInChildren<ParticleSystem>();
			m_pausedParticles.Clear();
			for(int i = 0; i < particles.Length; ++i) {
				if(particles[i].isPlaying) {
					particles[i].Stop();
					particles[i].Clear();
					m_pausedParticles.Add(particles[i]);
				}
			}
		}

		// Instant scroll to first dragon
		m_scroller.cameraAnimator.delta = 0f;

		// Start timer
		m_timer.Start(m_delay * 1000 * 0.5f);

		// Last dragon delta, next dragons are locked until player progress further in the game
		List<DragonData> dragonsByOrder = DragonManager.dragonsByOrder;
		for(int i = dragonsByOrder.Count - 1; i >= 0; --i) {
			// First non-hidden dragon (including teased dragons)
			if(dragonsByOrder[i].isRevealed || dragonsByOrder[i].isTeased) {
				// Get delta corresponding to this dragon and break the loop!
				m_lastDelta = m_scroller.cameraAnimator.cameraPath.path.GetDelta(i);
				break;
			}
		}

		// Toggle state!
		m_state = State.DELAY;
	}

	/// <summary>
	/// Stops the tutorial. Doesn't update profile's persistence!
	/// </summary>
	private void StopTutorial() {
		// Ignore if already in the IDLE state
		if(m_state == State.IDLE) return;

		// Lock all input
		Messenger.Broadcast<bool>(EngineEvents.UI_LOCK_INPUT, false);

		// Show UI back
		InstanceManager.menuSceneController.hud.animator.ForceShow(true);
		if(m_uiCanvasGroup != null) {
			m_uiCanvasGroup.DOFade(1f, 0.25f);

			// Restore paused particle systems
			for(int i = 0; i < m_pausedParticles.Count; ++i) {
				m_pausedParticles[i].Play();
			}
			m_pausedParticles.Clear();
		}

		// Control vars
		m_state = State.IDLE;
		m_timer.Finish();
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