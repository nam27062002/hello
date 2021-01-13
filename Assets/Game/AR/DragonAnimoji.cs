// DragonAnimoji.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 04/09/2018.
// Copyright (c) 2018 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// 
/// </summary>
public class DragonAnimoji : MonoBehaviour {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	
	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Internal refs
	private ParticleSystem[] m_particlesArray = null;

	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	public void Awake() {
		m_particlesArray = GetComponentsInChildren<ParticleSystem>();
	}

	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Toggle fire on/off.
	/// </summary>
	/// <param name="_on">Whether to enable or disable the fire effect.</param>
	public void ToggleFire(bool _on) {
		foreach(ParticleSystem ps in m_particlesArray) {
			if(_on) {
				ps.Play();
			} else {
				ps.Stop();
			}
		}
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
}