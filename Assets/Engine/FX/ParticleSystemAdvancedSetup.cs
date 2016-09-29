// ParticleSystemAdvancedSetup.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 27/09/2016.
// Copyright (c) 2016 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using System.Collections.Generic;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Simple component to randomly emit a particle sistem in intervals.
/// </summary>
[RequireComponent(typeof(ParticleSystem))]
public class ParticleSystemAdvancedSetup : MonoBehaviour {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	
	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed setup
	[SerializeField] private bool m_overrideDelay = false;
	[SerializeField] private Range m_delayRange = new Range(0f, 1f);

	[Space]
	[SerializeField] private bool m_overrideColor = false;
	[SerializeField] private List<Color> m_startColors = new List<Color>();

	// Internal
	private ParticleSystem m_ps = null;
	private float m_timer = 0f;
	
	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {
		m_ps = GetComponent<ParticleSystem>();

		// Set new start color
		if(m_overrideColor) {
			m_ps.startColor = m_startColors.GetRandomValue();
		}

		// Start timer with random delay
		if(m_overrideDelay) {
			m_ps.playOnAwake = false;
			m_ps.Stop();
			m_timer = m_delayRange.GetRandom();
		}
	}

	/// <summary>
	/// A change has occurred on the inspector. Validate its values.
	/// </summary>
	private void OnValidate() {
		// There must be at least one start color!
		if(m_overrideColor && m_startColors.Count == 0) {
			m_startColors.Add(Color.white);
		}
	}

	/// <summary>
	/// Called every frame
	/// </summary>
	private void Update() {
		// Skip if not enabled
		if(!isActiveAndEnabled) return;

		// Check timer
		if(m_overrideDelay) {
			m_timer -= Time.deltaTime;
			if(m_timer < 0f) {
				// Stop emitting
				m_ps.Stop();

				// Set new start color
				if(m_overrideColor) {
					m_ps.startColor = m_startColors.GetRandomValue();
				}

				// Emit particles
				m_ps.Play();

				// Reset timer
				m_timer = m_ps.duration + m_delayRange.GetRandom();
			}
		}
	}

	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
}