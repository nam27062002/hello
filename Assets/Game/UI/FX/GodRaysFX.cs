// PetGodRaysFX.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 09/01/2017.
// Copyright (c) 2017 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Simple class to control the God Rays FX in the open-egg screen.
/// </summary>
[RequireComponent(typeof(ParticleSystem))]
public class GodRaysFX : MonoBehaviour {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	
	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed
	[SerializeField] private ParticleSystem m_commonPS = null;
	[SerializeField] private ParticleSystem m_rarePS = null;
	[SerializeField] private ParticleSystem m_epicPS = null;

	// Internal
	private ParticleSystem m_basePS = null;
	
	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {
		// Get missing refs
		m_basePS = GetComponent<ParticleSystem>();
	}

	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Start (or restart) the FX with a given rarity.
	/// </summary>
	/// <param name="_raritySku">The sku of the rarity to be used to initialize the FX.</param>
	public void StartFX(string _raritySku) {
		// Toggle proper sub-system based on given rarity
		m_commonPS.gameObject.SetActive(_raritySku == "common");
		m_rarePS.gameObject.SetActive(_raritySku == "rare");
		m_epicPS.gameObject.SetActive(_raritySku == "epic");

		// Relaunch effect
		StopFX();
		m_basePS.Play(true);
	}

	/// <summary>
	/// Stop the FX. No effect if already stopped.
	/// </summary>
	public void StopFX() {
		// Just do it on base PS
		m_basePS.Stop(true);
		m_basePS.Clear(true);
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
}