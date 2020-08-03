// LoadingDots.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 07/03/2017.
// Copyright (c) 2017 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Simple dot loading controller.
/// </summary>
public class LoadingDots : MonoBehaviour {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	
	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed
	[SerializeField] private Image[] m_dots = new Image[0];
	[Separator]
	[SerializeField] private Range m_scaleFromTo = new Range(1f, 1.5f);
	[SerializeField] private Color m_colorFrom = new Color(1f, 1f, 1f, 0.5f);
	[SerializeField] private Color m_colorTo = new Color(1f, 1f, 1f, 1f);
	[Separator]
	[SerializeField] private float m_durationIn = 0.1f;
	[SerializeField] private float m_durationOut = 0.15f;
	[SerializeField] private float m_delayBetweenDots = -0.1f;	// Negative for overlapping
	[SerializeField] private float m_finalDelay = 0.5f;
	[Separator]
	[SerializeField] private bool m_ignoreTimeScale = true;
	[SerializeField] private Ease m_easeIn = Ease.Linear;
	[SerializeField] private Ease m_easeOut = Ease.Linear;
	[Separator]
	[Comment("Toggle only for instances that are frequently enabled/disabled")]
	[SerializeField] private bool m_cacheSequence = false;

	// Internal
	private Sequence m_sequence = null;
	
	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {
		// If caching sequence, create it now
		if(m_cacheSequence) {
			CreateSequence();
		}
	}

	/// <summary>
	/// Component has been enabled.
	/// </summary>
	private void OnEnable() {
		// Create sequence if not already done
		if(m_sequence == null) {
			CreateSequence();
		}

		// Restart sequence
		m_sequence.Restart();
	}

	/// <summary>
	/// Component has been disabled.
	/// </summary>
	private void OnDisable() {
		// Pause/Destroy sequence (depending on cache flag)
		if(m_sequence != null) {
			if(m_cacheSequence) {
				m_sequence.Pause();
			} else {
				DestroySequence();
			}
		}
	}

	/// <summary>
	/// Destructor.
	/// </summary>
	private void OnDestroy() {
		DestroySequence();
	}

	/// <summary>
	/// A value has changed on the inspector.
	/// </summary>
	private void OnValidate() {
		// Re-create sequence to see changes in real-time
		if(isActiveAndEnabled && Application.isPlaying) {
			CreateSequence();
		}
	}

	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Re-creates the sequence.
	/// </summary>
	private void CreateSequence() {
		// If the sequence is already created, kill it
		DestroySequence();

		// Create a new sequence
		m_sequence = DOTween.Sequence()
			.SetAutoKill(false)
			.SetLoops(-1, LoopType.Restart)
			.SetUpdate(UpdateType.Normal, m_ignoreTimeScale);

		// Animate dots
		float t = 0f;
		for(int i = 0; i < m_dots.Length; i++) {
			if(m_dots[i] == null) continue;

			// Set initial values
			m_dots[i].transform.localScale = Vector3.one * m_scaleFromTo.min;
			m_dots[i].color = m_colorFrom;

			// In
			m_sequence.Insert(t, m_dots[i].transform.DOScale(m_scaleFromTo.max, m_durationIn).SetEase(m_easeIn));
			m_sequence.Insert(t, m_dots[i].DOColor(m_colorTo, m_durationIn).SetEase(m_easeIn));
			t += m_durationIn;

			// Out
			m_sequence.Insert(t, m_dots[i].transform.DOScale(m_scaleFromTo.min, m_durationOut).SetEase(m_easeOut));
			m_sequence.Insert(t, m_dots[i].DOColor(m_colorFrom, m_durationOut).SetEase(m_easeOut));
			t += m_durationOut;

			// Delay/Overlapping or final pause if it's the last dot
			if(i == m_dots.Length - 1) {
				m_sequence.AppendInterval(m_finalDelay);
				t += m_finalDelay;
			} else {
				t += m_delayBetweenDots;
			}
		}

		// Launch sequence
		m_sequence.Restart();
	}

	/// <summary>
	/// Destroy the sequence.
	/// </summary>
	private void DestroySequence() {
		if(m_sequence != null) {
			m_sequence.Complete();  // Make sure sequence is at its end-state to restore object's default values so the new sequence can take them
			m_sequence.Kill();
			m_sequence = null;
		}
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
}