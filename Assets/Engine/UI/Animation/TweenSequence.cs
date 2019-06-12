// TweenSequence.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 06/09/2017.
// Copyright (c) 2017 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;
using DG.Tweening;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// 
/// </summary>
public class TweenSequence : MonoBehaviour {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	
	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed setup
	[NumericRange(0, float.MaxValue)]
	[SerializeField] private float m_totalDuration = 1f;
	public float totalDuration { 
		get { return m_totalDuration; }
	}

	[SerializeField] private List<TweenSequenceElement> m_elements = new List<TweenSequenceElement>();

	// Events
	public UnityEvent OnFinished = new UnityEvent();

	// Internal
	private Sequence m_sequence = null;
	public Sequence sequence {
		get { return m_sequence; }
	}
	
	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void OnEnable() {

	}

	/// <summary>
	/// Launch the sequence!
	/// </summary>
	/// <param name="_recreate">If set to <c>true</c>, force the sequence recreation.</param>
	public void Launch() {

        // [JOM] Always regenerate the sequence. Otherwise, the tweeners OnStart events wont be 
        // triggered again after the restart -> Bug [HDK-3494]
        GenerateSequence();

		// Restart it!
		m_sequence.Restart();
	}

	/// <summary>
	/// Re-create the sequence with current setup.
	/// </summary>
	private void GenerateSequence() {
		// If we already have a sequence running, kill it!
		KillSequence(true);

		// Create new sequence
		m_sequence = DOTween.Sequence();
		m_sequence.Pause();	// Start paused
		m_sequence.SetAutoKill(false);

		// Insert elements
		for(int i = 0; i < m_elements.Count; ++i) {
			if(m_elements[i] != null) m_elements[i].InsertTo(this);
		}

		// Append callback at the end
		m_sequence.InsertCallback(m_totalDuration, () => OnFinished.Invoke());	// That way we ensure that the sequence has the expected duration
	}

	/// <summary>
	/// Kills the sequence.
	/// </summary>
	/// <param name="_restoreValues">If set to <c>true</c> restore values.</param>
	private void KillSequence(bool _restoreValues) {
		// Ignore if we have no sequence
		if(m_sequence == null) return;

		// Go to the start first to restore initial values
		m_sequence.Rewind();
		m_sequence.Kill();
		m_sequence = null;

		// Restore initial values?
		if(_restoreValues) {
			// Reverse-iterate sequence elements and apply original values
			for(int i = m_elements.Count - 1; i >= 0; --i) {
				if(m_elements[i] != null) m_elements[i].RestoreOriginalValues();
			}
		}
	}

	/// <summary>
	/// Destructor.
	/// </summary>
	private void OnDestroy() {
		KillSequence(false);
	}

	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Play the sequence.
	/// </summary>
	public void Play() {
		m_sequence.Play();
	}

	/// <summary>
	/// Pause the sequence.
	/// </summary>
	public void Pause() {
		m_sequence.Pause();
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
}