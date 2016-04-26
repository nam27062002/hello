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
public class IncubatorTutorial : MonoBehaviour {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	private enum State {
		IDLE,
		DELAY,
		RUNNING,
	};
	
	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed setup
	[SerializeField] private float m_delay = 3f;
	[SerializeField] private float m_duration = 10f;
	public Transform m_fingerStart;
	public Transform m_fingerEnd;

	// Internal logic
	private DeltaTimer m_timer = new DeltaTimer();
	private State m_state = State.IDLE;
	private TutorialFinger m_finger;
	
	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() 
	{
		// Subscribe to external events. We want to receive these events even when disabled, so do it in the Awake/Destroy instead of the OnEnable/OnDisable.
		Messenger.AddListener<int, int, bool>(EngineEvents.NAVIGATION_SCREEN_CHANGED_INT, OnScreenChanged);
		Messenger.AddListener<EggController>(GameEvents.EGG_DRAG_ENDED, OnEggDragEnded);
	}

	/// <summary>
	/// 
	/// Destructor.
	/// </summary>
	private void OnDestroy() {
		// Unsubscribe from external events.
		Messenger.RemoveListener<int, int, bool>(EngineEvents.NAVIGATION_SCREEN_CHANGED_INT, OnScreenChanged);
		Messenger.RemoveListener<EggController>(GameEvents.EGG_DRAG_ENDED, OnEggDragEnded);
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
				if(m_timer.Finished()) 
				{
					// Yes! Start scrolling
					m_state = State.RUNNING;
					m_timer.Start(m_duration);

					if ( m_finger == null )
					{
						GameObject go = Instantiate( Resources.Load( TutorialFinger.PATH ) ) as GameObject;
						go.transform.parent = transform;
						m_finger = go.GetComponent<TutorialFinger>();
					}
					m_finger.gameObject.SetActive( true );

					// Search egg to incubate

					// Set animation from egg to incubator
					m_finger.SetupDrag( m_fingerStart, m_fingerEnd);
				}
			} break;

			case State.RUNNING: {
				// Timer finished?
				if(m_timer.Finished()) 
				{
					m_state = State.DELAY;
					m_timer.Start(m_delay);
					m_finger.gameObject.SetActive( false );
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
	private void StartTutorial() 
	{
		if(m_state == State.IDLE) 
		{
			// Start timer
			m_timer.Start(m_delay);

			// Toggle state!
			m_state = State.DELAY;
		}
	}

	/// <summary>
	/// Stops the tutorial. Doesn't update profile's persistence!
	/// </summary>
	private void StopTutorial() {
		if(m_state != State.IDLE) 
		{
			// Control vars
			m_state = State.IDLE;
			m_timer.Finish();

			// Hide hand
			m_finger.gameObject.SetActive( false );
		}
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// The current menu screen has changed.
	/// </summary>
	/// <param name="_fromScreen">Previous screen.</param>
	/// <param name="_toScreen">New screen.</param>
	/// <param name="_animated">Whether it was animated or not.</param>
	public void OnScreenChanged(int _fromScreen, int _toScreen, bool _animated) {
		// Only interested if new screen is the Incubator screen
		if(_toScreen != (int)MenuScreens.INCUBATOR) {
			// Stop the tutorial if it's running
			StopTutorial();
			return;
		}

		Egg targetEgg = EggManager.inventory[0];
		// If the tutorial wasn't completed, launch it now
		if(!UserProfile.IsTutorialStepCompleted(TutorialStep.EGG_INCUBATOR) && EggManager.incubatingEgg == null && targetEgg != null)
		{
			StartTutorial();
		}
	}

	private void OnEggDragEnded(EggController _egg) 
	{
		// if incubating end tutorial
		if(EggManager.incubatingEgg != null)
		{
			UserProfile.SetTutorialStepCompleted(TutorialStep.EGG_INCUBATOR);
			StopTutorial();
		}	
	}
}