// ParticleSystemIgnoreTimescale.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 20/02/2017.
// Copyright (c) 2017 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Simple script to allow particle systems to ignore global timescale.
/// From http://answers.unity3d.com/questions/778990/unscaled-time-for-particle-system.html
/// </summary>
[RequireComponent(typeof(ParticleSystem))]
public class ParticleSystemIgnoreTimescale : MonoBehaviour {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	
	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Internal
	private ParticleSystem m_ps = null;
	private ParticleSystem.MainModule m_psMainModule = new ParticleSystem.MainModule();
	private bool m_wasEmitting = true;
	private bool m_wasPaused = false;

	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {
		// Get PS reference
		m_ps = GetComponent<ParticleSystem>();
		Debug.Assert(m_ps != null, "Required component missing!", this);
		m_psMainModule = m_ps.main;
	}

	/// <summary>
	/// Called every frame
	/// </summary>
	private void Update() {
		// Ignore if particle system is not playing
		if(m_ps.isStopped) return;

		// If TimeScale is not fully paused, just compensate PS's simulation speed
		// Otherwise do a simulation run on the PS using unscaled delta time
		ParticleSystem.MainModule psMainModule = m_ps.main;
		if(Time.timeScale > 0f) {
			// Just compensate playback speed
			psMainModule.simulationSpeed = 1f/Time.timeScale;
		} else {
			// Store previous state
			m_wasEmitting = m_ps.isEmitting;
			m_wasPaused = m_ps.isPaused;

			// Reset playback speed and do the simulation with unscaled time
			psMainModule.simulationSpeed = 1f;
			m_ps.Simulate(Time.unscaledDeltaTime, true, false);

			// Restore previous state (Simulate pauses the PS)
			if(!m_wasEmitting) {
				m_ps.Play(true);
				m_ps.Stop(true, ParticleSystemStopBehavior.StopEmitting);
			} else if(!m_wasPaused) {
				m_ps.Play(true);
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