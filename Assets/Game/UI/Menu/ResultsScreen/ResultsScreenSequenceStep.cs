// ResultsScreenSequenceStep.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 28/08/2017.
// Copyright (c) 2017 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using DG.Tweening;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Specialization of the basic step for states driven by a tween sequence.
/// </summary>
public abstract class ResultsScreenSequenceStep : ResultsScreenStep {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	
	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed setup
	[SerializeField] protected bool m_skipAllowed = true;
	[Space]
	[SerializeField] protected ShowHideAnimator m_tapToContinue = null;
	[SerializeField] protected TweenSequence m_sequence;

	// Internal
	protected bool m_skipped = false;

	//------------------------------------------------------------------------//
	// PARENT OVERRIDES														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialize this step with the given results screen controller.
	/// </summary>
	/// <param name="_controller">The results screen controller that will be triggering this step.</param>
	override public void Init(ResultsScreenController _controller, ResultsScreenController.Step _stepID) {
		// Call parent
		base.Init(_controller, _stepID);

		// Listen when the sequence is finished
		m_sequence.OnFinished.AddListener(OnSequenceFinished);
	}

	/// <summary>
	/// Initialize and launch this step.
	/// </summary>
	override public void Launch() {
		// Call parent
		base.Launch();

		// Internal vars
		m_skipped = false;

		// If skip if allowed, show tap to continue from the start. Otherwise hide it and must be displayed manually.
		if(m_skipAllowed) {
			m_tapToContinue.ForceShow();
		} else {
			m_tapToContinue.ForceHide(false);	// No anim
		}

		// Launch sequence!
		m_sequence.Launch();
	}

	//------------------------------------------------------------------------//
	// OVERRIDE CANDIDATES													  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Called when skip is triggered.
	/// </summary>
	protected virtual void OnSkip() {
		// To be implemented by heirs if needed
	}

	/// <summary>
	/// Called when continue is triggered (after TapToContinue).
	/// </summary>
	protected virtual void OnContinue() {
		// To be implemented by heirs if needed
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// The tween sequence has finished.
	/// </summary>
	protected virtual void OnSequenceFinished() {
		// Notify listeners
		OnFinished.Invoke();

		// Hide tap to continue to prevent spamming
		m_tapToContinue.Hide();
	}

	/// <summary>
	/// The tap to continue button has been pressed.
	/// </summary>
	public void OnTapToContinue()                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                        {
		// Only if enabled! (to prevent spamming)
		// [AOC] Reuse visibility state to control whether tap to continue is enabled or not)
		if(!m_tapToContinue.visible) return;

		// If sequence was skipped, ignore
		if(m_skipped) return;

		// Nothing to do if sequence is null
		if(m_sequence.sequence == null) return;

		// If sequence is playing, fast forward it.
		if(m_sequence.sequence.IsPlaying()) {
			// Make sure it's allowed!
			if(m_skipAllowed) {
				// Just multiply time scale so the anim goes superfast
				//m_sequence.sequence.timeScale = 4f;

				// Skip to the end of the sequence
				m_sequence.sequence.Complete(true);

				// Notify listeners
				OnSkip();

				// Control vars
				m_skipped = true;
			}
		}

		// Otherwise resume it at the normal speed until it finishes
		else {
			// Hide tap to continue to prevent spamming
			//m_tapToContinue.Hide();

			// Restore tween timescale
			m_sequence.sequence.timeScale = 1f;

			// Launch end sequence
			m_sequence.Play();
		}
	}
}