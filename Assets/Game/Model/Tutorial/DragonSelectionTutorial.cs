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

	private const string SCROLL_TO_DRAGON_SKU = "dragon_classic";

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed setup
	[SerializeField] private float m_delay = 1f;
	[SerializeField] private float m_backDelay = 1f;
	[Space]
	[Tooltip("World Units per Second")]
	[SerializeField] private float m_forwardSpeed = 35f;
	[Tooltip("World Units per Second")]
	[SerializeField] private float m_backSpeed = 200f;
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

	private float m_initialDelta = 0f;
	private float m_lastDelta = 1f;
	private float m_finalDelta = 0f;

	// Durations will be computed based on speed and distance
	private float m_forwardDuration = 10f;
	private float m_backDuration = 1.5f;

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
		Messenger.AddListener<MenuScreen, MenuScreen>(MessengerEvents.MENU_SCREEN_TRANSITION_START, OnScreenChanged);
	}

	/// <summary>
	/// 
	/// Destructor.
	/// </summary>
	private void OnDestroy() {
		// Unsubscribe from external events.
		Messenger.RemoveListener<MenuScreen, MenuScreen>(MessengerEvents.MENU_SCREEN_TRANSITION_START, OnScreenChanged);
	}

	/// <summary>
	/// Called every frame.
	/// </summary>
	private void FixedUpdate() {
		// Only if running
		switch(m_state) {
			case State.IDLE: {
					// Nothing to do
				}
				break;

			case State.DELAY: {
					// Timer finished?
					if(m_timer.IsFinished()) {
						// Yes! Start scrolling
						m_state = State.RUNNING;
						m_timer.Start(m_forwardDuration * 1000);

                        // Lock all the UI input while the animation is runnin
                        Messenger.Broadcast<bool>(MessengerEvents.UI_LOCK_INPUT, true);
                    }
				}
				break;

			case State.RUNNING: {
					// Timer finished?
					if(m_timer.IsFinished()) {
						// Yes! Pause before going back
						m_state = State.BACK_DELAY;
						m_timer.Start(m_backDelay * 1000);
						m_scroller.cameraAnimator.delta = m_lastDelta;
					} else {
						// Timer not finished, scroll from initial delta to last delta
						m_scroller.cameraAnimator.delta = Mathf.Lerp(m_initialDelta, m_lastDelta, m_timer.GetDelta(m_easeForward));
					}
				}
				break;

			case State.BACK_DELAY: {
					// Timer finished?
					if(m_timer.IsFinished()) {
						// Yes! Start scroll back animation
						m_state = State.BACK;
						m_timer.Start(m_backDuration * 1000);
					}
				}
				break;

			case State.BACK: {
					// Timer finished?
					if(m_timer.IsFinished()) {
						m_scroller.cameraAnimator.delta = m_finalDelta;

						// Yes! Stop tutorial
						StopTutorial();

						// Make sure we have the initial dragon selected
						m_scroller.FocusDragon(UsersManager.currentUser.CurrentDragon, true);

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

                        // Unock all the UI when the animation is finished
                        Messenger.Broadcast<bool>(MessengerEvents.UI_LOCK_INPUT, false);

                        // Tracking!
                        HDTrackingManager.Instance.Notify_Funnel_FirstUX(FunnelData_FirstUX.Steps._06b_animation_done);

						PersistenceFacade.instance.Save_Request();
					} else {
						// Timer not finished, scroll from last delta to final delta
						m_scroller.cameraAnimator.delta = Mathf.Lerp(m_lastDelta, m_finalDelta, m_timer.GetDelta(m_easeBackward));
					}


				}
				break;
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
		Messenger.Broadcast<bool>(MessengerEvents.UI_LOCK_INPUT, true);

		// Hide HUD and UI
		InstanceManager.menuSceneController.hud.animator.ForceHide(false);
		if(m_uiCanvasGroup != null) {
			m_uiCanvasGroup.alpha = 0;

			// Particle Systems are not affected by canvas groups, so manually pause them
			ParticleSystem[] particles = m_uiCanvasGroup.GetComponentsInChildren<ParticleSystem>();
			m_pausedParticles.Clear();
			for(int i = 0; i < particles.Length; ++i) {
				if(particles[i].gameObject.activeSelf) {
					particles[i].gameObject.SetActive(false);
					m_pausedParticles.Add(particles[i]);
				}
			}
		}

		// Compute deltas
		// 1) Initial delta is always the first dragon
		m_initialDelta = 0f;

		// 2) Last delta is the last visible dragon (teased included)
		// [AOC] As of 1.18, remove shadowed dragons, so we're gonna scroll to a fixed dragon
		int dragonsToView = DragonManager.GetDragonData(SCROLL_TO_DRAGON_SKU).GetOrder();
#if false
		int dragonsToView = 9;
		List<IDragonData> dragonsByOrder = DragonManager.GetDragonsByOrder(IDragonData.Type.CLASSIC);
		for(int i = dragonsByOrder.Count - 1; i >= 0; --i) {
			// First non-hidden dragon (including teased dragons)
			if(dragonsByOrder[i].isRevealed || dragonsByOrder[i].isTeased) {
				// Get delta corresponding to this dragon and break the loop!
				m_lastDelta = m_scroller.cameraAnimator.cameraPath.path.GetDelta(i);
				dragonsToView = i;
				break;
			}
		}
#else
		m_lastDelta = m_scroller.cameraAnimator.cameraPath.path.GetDelta(dragonsToView);
#endif

		// Load as many dragons as needed
		m_scroller.LoadTutorialDragonsScroll(dragonsToView + 1);	// One more to let it view 

		// 3) Final delta is the current selected dragon (most of the times will be the first one)
		DefinitionNode def = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.DRAGONS, UsersManager.currentUser.CurrentDragon);
		int menuOrder = (def == null) ? 0 : def.GetAsInt("order");
		m_finalDelta = m_scroller.cameraAnimator.cameraPath.path.GetDelta(menuOrder);	// Taking advantadge that we have exactly one control point per dragon

		// Compute durations based on distance to run
		float pathLength = m_scroller.cameraAnimator.cameraPath.path.length;
		m_forwardDuration = Mathf.Abs(m_lastDelta - m_initialDelta) * pathLength / m_forwardSpeed;
		m_backDuration = Mathf.Abs(m_lastDelta - m_finalDelta) * pathLength / m_backSpeed;



        // Toggle state!
        m_state = State.DELAY;
		m_scroller.cameraAnimator.delta = m_initialDelta;	// Instant scroll to initial delta (first dragon)
		m_timer.Start(m_delay * 1000);	// Start timer with the initial delay
	}

	/// <summary>
	/// Stops the tutorial. Doesn't update profile's persistence!
	/// </summary>
	private void StopTutorial() {
		// Ignore if already in the IDLE state
		if(m_state == State.IDLE) return;

		// Lock all input
		Messenger.Broadcast<bool>(MessengerEvents.UI_LOCK_INPUT, false);

		// Show HUD back
		// Do some null checks to avoid potential issues
		// https://console.firebase.google.com/project/hungry-dragon-45530774/crashlytics/app/android:com.ubisoft.hungrydragon/issues/f18f3031ae5663897300068d6427460c?time=last-seven-days&sessionId=5DDD2D460345000146178AE590E84A81_DNE_7_v2
		if(InstanceManager.menuSceneController != null) {
			if(InstanceManager.menuSceneController.hud != null) {
				if(InstanceManager.menuSceneController.hud.animator != null) {
					InstanceManager.menuSceneController.hud.animator.ForceShow(true);
				}
			}
		}

		// Show UI back
		if(m_uiCanvasGroup != null) {
			m_uiCanvasGroup.DOFade(1f, 0.25f);
		}

		// Restore paused particle systems
		if(m_pausedParticles != null) {
			for(int i = 0; i < m_pausedParticles.Count; ++i) {
				if(m_pausedParticles[i] != null) {
					m_pausedParticles[i].gameObject.SetActive(true);
				}
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
	/// The current menu screen has changed (animation starts now).
	/// </summary>
	/// <param name="_from">Source screen.</param>
	/// <param name="_to">Target screen.</param>
	private void OnScreenChanged(MenuScreen _from, MenuScreen _to) {
		// If leaving the dragon selection screen, force the tutorial to stop (shouldn't happen)
		if(_to != MenuScreen.DRAGON_SELECTION) {
			// Stop the tutorial if it's running
			StopTutorial();
			return;
		}

		// If the tutorial wasn't completed, launch it now
		if(!UsersManager.currentUser.IsTutorialStepCompleted(TutorialStep.DRAGON_SELECTION))
		{		
			StartTutorial();
		}
	}
}