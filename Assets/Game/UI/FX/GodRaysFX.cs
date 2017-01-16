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
	[Tooltip("One per rarity, matching order")]
	[SerializeField] private ParticleSystem[] m_rarityPS = new ParticleSystem[(int)EggReward.Rarity.COUNT];

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

	/// <summary>
	/// A change has been done in the inspector.
	/// </summary>
	private void OnValidate() {
		// Make sure the rarity array has exactly the same length as rarities in the game.
		m_rarityPS.Resize((int)EggReward.Rarity.COUNT);
	}

	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Start (or restart) the FX with a given rarity.
	/// </summary>
	/// <param name="_rarity">The rarity to be used to initialize the FX.</param>
	public void StartFX(EggReward.Rarity _rarity) {
		// Toggle proper sub-system based on given rarity
		for(int i = 0; i < m_rarityPS.Length; i++) {
			m_rarityPS[i].gameObject.SetActive(i == (int)_rarity);
		}

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