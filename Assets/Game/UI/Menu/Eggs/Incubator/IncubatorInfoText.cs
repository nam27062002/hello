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
		Messenger.AddListener<Egg>(GameEvents.EGG_INCUBATION_STARTED, OnEggIncubationStarted);
		Messenger.AddListener(GameEvents.EGG_INCUBATOR_CLEARED, OnIncubatorCleared);
		Messenger.AddListener<Egg, int>(GameEvents.EGG_ADDED_TO_INVENTORY, OnEggAddedToInventory);
	}

	/// <summary>
	/// First update.
	/// </summary>
	private void Start() {
		// Perform a first refresh
		Refresh();

		// Setup initial visibility
		// Only when incubator is empty
		m_anim.Set(EggManager.isIncubatorAvailable, false);
	}

	/// <summary>
	/// Destructor.
	/// </summary>
	private void OnDestroy() {
		// Unsubscribe from external events
		Messenger.RemoveListener<Egg>(GameEvents.EGG_INCUBATION_STARTED, OnEggIncubationStarted);
		Messenger.RemoveListener(GameEvents.EGG_INCUBATOR_CLEARED, OnIncubatorCleared);
		Messenger.RemoveListener<Egg, int>(GameEvents.EGG_ADDED_TO_INVENTORY, OnEggAddedToInventory);
	}

	/// <summary>
	/// Refresh this slot with the latest data from the manager.
	/// </summary>
	public void Refresh() {
		// Skip if incubator is not empty
		if(!EggManager.isIncubatorAvailable) return;

		// Different texts if inventory is empty or not
		if(EggManager.isInventoryEmpty) {
			m_text.Localize("TID_INCUBATOR_EMPTY_INVENTORY_MESSAGE");
		} else {
			m_text.Localize("TID_INCUBATOR_EMPTY_NEST_MESSAGE");
		}
	}

	//------------------------------------------------------------------//
	// CALLBACKS														//
	//------------------------------------------------------------------//
	/// <summary>
	/// An egg has been added to the incubator.
	/// </summary>
	/// <param name="_egg">The egg.</param>
	private void OnEggIncubationStarted(Egg _egg) {
		// Hide!
		m_anim.Hide();
	}

	/// <summary>
	/// The egg on the incubator has been collected.
	/// </summary>
	private void OnIncubatorCleared() {
		// Show and refresh data
		Refresh();
		m_anim.Show();
	}

	/// <summary>
	/// An egg has been added to the inventory.
	/// </summary>
	/// <param name="_egg">The egg.</param>
	/// <param name="_slotIdx">The slot where the egg has been added.</param>
	private void OnEggAddedToInventory(Egg _egg, int _slotIdx) {
		// Refresh! Message might change
		Refresh();
	}
}

