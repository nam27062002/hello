// IncubatorInfoText.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 02/03/2016.
// Copyright (c) 2016 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;
using System;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// Controls the info text in the incubator menu.
/// </summary>
[RequireComponent(typeof(ShowHideAnimator))]
public class IncubatorInfoText : MonoBehaviour {
	//------------------------------------------------------------------//
	// CONSTANTS														//
	//------------------------------------------------------------------//

	//------------------------------------------------------------------//
	// MEMBERS															//
	//------------------------------------------------------------------//
	// Scene references
	[SerializeField] private Localizer m_text = null;
	private ShowHideAnimator m_anim = null;

	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {
		// Check references
		Debug.Assert(m_text != null, "Required field!");

		// Get external assets
		m_anim = GetComponent<ShowHideAnimator>();

		// Subscribe to external events
		Messenger.AddListener<Egg, Egg.State, Egg.State>(GameEvents.EGG_STATE_CHANGED, OnEggStateChanged);
	}

	/// <summary>
	/// First update.
	/// </summary>
	private void Start() {
		// Perform a first refresh
		Refresh();

		// Setup initial visibility
		m_anim.ForceSet(!EggManager.isInventoryFull, false);
	}

	/// <summary>
	/// Destructor.
	/// </summary>
	private void OnDestroy() {
		// Unsubscribe from external events
		Messenger.RemoveListener<Egg, Egg.State, Egg.State>(GameEvents.EGG_STATE_CHANGED, OnEggStateChanged);
	}

	/// <summary>
	/// Refresh this slot with the latest data from the manager.
	/// </summary>
	public void Refresh() {
		// Set text
		// [AOC] Optionally set different text for different states of the incubator
		if(EggManager.isInventoryFull) {
			m_text.Localize("TID_INCUBATOR_FULL_NEST_MESSAGE");
		} else {
			m_text.Localize("TID_INCUBATOR_EMPTY_INVENTORY_MESSAGE");
		}

		// Set visibility
		m_anim.Set(!EggManager.isInventoryFull);
	}

	//------------------------------------------------------------------//
	// CALLBACKS														//
	//------------------------------------------------------------------//
	/// <summary>
	/// An egg has been added to the incubator.
	/// </summary>
	/// <param name="_egg">The egg.</param>
	private void OnEggStateChanged(Egg _egg, Egg.State _from, Egg.State _to) {
		Refresh();
	}
}

