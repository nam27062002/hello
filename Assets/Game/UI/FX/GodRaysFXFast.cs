// GodRaysFXFast.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 25/01/2017.
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
/// Better performance version than the original GodRaysFX.
/// </summary>
[RequireComponent(typeof(ParticleSystem))]
public class GodRaysFXFast : MonoBehaviour {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	
	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed
	[SerializeField] private SpriteRenderer m_colorSprite = null;
	[SerializeField] private ParticleSystem m_sparksPS = null;
	
	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {
		
	}

	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Start (or restart) the FX with a given rarity.
	/// </summary>
	/// <param name="_rarity">The rarity to be used to initialize the FX.</param>
	public void StartFX(EggReward.Rarity _rarity) {
		// Enable ourselves
		this.gameObject.SetActive(true);

		// Tint with the proper color based on rarity
		Color rarityColor = UIConstants.GetRarityColor(_rarity);
		m_colorSprite.color = rarityColor;
		ParticleSystem.MainModule psData = m_sparksPS.main;
		psData.startColor = rarityColor;
	}

	/// <summary>
	/// Stop the FX. No effect if already stopped.
	/// </summary>
	public void StopFX() {
		// Disable ourselves
		this.gameObject.SetActive(false);
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
}