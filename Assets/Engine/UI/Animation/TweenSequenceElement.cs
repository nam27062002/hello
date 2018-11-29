// TweenSequenceElement.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 06/09/2017.
// Copyright (c) 2017 Ubisoft. All rights reserved.

//#define LOG

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.Events;
using System;
using DG.Tweening;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Single element of a tween sequence. Just stores data to configure the tween.
/// </summary>
[Serializable]
public class TweenSequenceElement {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	// Don't change order! All serialized values will get lost!
	public enum Type {
		IDLE = 0,   // Use it to program a callback in the sequence

		FADE,
		SCALE,
		MOVE
	}

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed setup
	public string name = "";
	public Type type = Type.IDLE;
	public Transform target = null;

	// Seconds
	public float startTime = 0f;
	public float endTime = 0.25f;
	public float duration { get { return Mathf.Max(endTime - startTime, 0f); } }    // At least 0!

	public bool from = true;
	public Ease ease = Ease.Linear;

	public float floatValue = 0f;
	public Vector3 vectorValue = Vector3.zero;

	public UnityEvent OnStart = new UnityEvent();
	public UnityEvent OnEnd = new UnityEvent();

	// Original values backup
	private bool m_originalValuesSaved = false;
	private float m_originalFloatValue = 0f;
	private Vector3 m_originalVectorValue = Vector3.zero;

	// Debug
#if LOG
	TweenSequence m_parentSequence = null;
#endif

	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Create and insert a new to the target sequence tween using this element's data.
	/// </summary>
	/// <param name="_seq">Target sequence.</param>
	public void InsertTo(TweenSequence _seq) {
		// Ignore if given sequence is not valid or not properly initialized
		if(_seq == null || _seq.sequence == null) return;

		// [AOC] InsertCallback() workaround
		//		 Use parent sequence's transform as target if none defined and type is IDLE
		if(type == Type.IDLE && target == null) {
			target = _seq.transform;
		}

		// Debug
#if LOG
		m_parentSequence = _seq;
		Debug.Log(Colors.fuchsia.Tag(
			"Inserting " + GetDebugName() + " into " + m_parentSequence.name
		));
#endif

		// If original values have never been saved, do it now!
		if(!m_originalValuesSaved) {
			SaveOriginalValues();
		}

		// Aux vars
		Tweener tween = null;

		// Depends on type!
		switch(type) {
			case Type.FADE: {
				// If the object doesn't have a canvas group, add it now!
				CanvasGroup targetGroup = target.ForceGetComponent<CanvasGroup>();
				tween = targetGroup.DOFade(floatValue, duration);
			} break;

			case Type.SCALE: {
				tween = target.DOScale(vectorValue, duration);
			} break;

			case Type.MOVE: {
				tween = target.DOBlendableLocalMoveBy(vectorValue, duration);
			} break;

			case Type.IDLE: {
				/*
				// Super-special case: we won't be creating any tween, just inserting the start and end callbacks
				_seq.sequence.InsertCallback(startTime, OnTweenStart);
				_seq.sequence.InsertCallback(endTime, OnTweenEnd);
				return; // Don't do anything else!
				*/

				// [AOC] For some damn reason the previous logic triggers the Start callback twice
				//		 Let's do a dummy move tween instead towards the parent sequence's transform instead
				tween = target.DOBlendableLocalMoveBy(GameConstants.Vector3.zero, duration);
			} break;
		}

		// Shared stuff
		if(tween != null) {
			// From
			if(from) tween.From();

			// Ease
			tween.SetEase(ease);

			// Callbacks
			tween.OnStart(OnTweenStart);
			tween.OnComplete(OnTweenEnd);

			// Insert tween to the sequence!
			_seq.sequence.Insert(startTime, tween);
		}
	}

	/// <summary>
	/// Save current values of the target as the original ones.
	/// </summary>
	public void SaveOriginalValues() {
		// Depends on type!
		switch(type) {
			case Type.FADE: {
				// If the object doesn't have a canvas group, add it now!
				CanvasGroup targetGroup = target.ForceGetComponent<CanvasGroup>();
				m_originalFloatValue = targetGroup.alpha;
			} break;

			case Type.SCALE: {
				m_originalVectorValue = target.localScale;
			} break;

			case Type.IDLE:		// [AOC] InsertCallback() workaround
			case Type.MOVE: {
				m_originalVectorValue = target.localPosition;
			} break;
		}
		m_originalValuesSaved = true;
	}

	/// <summary>
	/// Apply the original values to the target.
	/// </summary>
	public void RestoreOriginalValues() {
		// Only if we have saved them!
		if(!m_originalValuesSaved) return;

		// Depends on type
		switch(type) {
			case Type.FADE: {
				// If the object doesn't have a canvas group, add it now!
				CanvasGroup targetGroup = target.ForceGetComponent<CanvasGroup>();
				targetGroup.alpha = m_originalFloatValue;
			} break;

			case Type.SCALE: {
				target.localScale = m_originalVectorValue;
			} break;

			case Type.IDLE:		// [AOC] InsertCallback() workaround
			case Type.MOVE: {
				target.localPosition = m_originalVectorValue;
			} break;
		}
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Tween started.
	/// </summary>
	private void OnTweenStart() {
#if LOG
		Debug.Log(Color.green.Tag(m_parentSequence.name + ":" + m_parentSequence.sequence.Elapsed() + " | " + GetDebugName() + " | START"));
#endif
		OnStart.Invoke();
	}

	/// <summary>
	/// Tween ended.
	/// </summary>
	private void OnTweenEnd() {
#if LOG
		//Debug.Log(Colors.orange.Tag(m_parentSequence.name + " | " + GetDebugName() + " | END"));
#endif
		OnEnd.Invoke();
	}

	/// <summary>
	/// Debug Only! Get name for printing.
	/// </summary>
	private string GetDebugName() {
#if LOG
		if(!string.IsNullOrEmpty(this.name)) return this.name;

		string n = this.type.ToString();

		if(this.target != null) n += "." + this.target.name;

		return n;
#else
		return string.Empty;
#endif
	}
}