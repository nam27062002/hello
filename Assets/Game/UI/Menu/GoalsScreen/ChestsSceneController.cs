// LevelSelectionCameraController.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on DD/MM/2016.
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
/// Main script to control several aspects of the Goals screen 3D scene.
/// </summary>
public class ChestsSceneController : MonoBehaviour {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	
	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed setup
	// Chest references
	[Tooltip("Always 5 slots, please!")]
	[SerializeField] private ChestsScreenSlot[] m_chestSlots = new ChestsScreenSlot[5];
	public ChestsScreenSlot[] chestSlots {
		get { return m_chestSlots; }
	}
	
	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {
		// Subscribe to external events
		Messenger.AddListener(MessengerEvents.CHESTS_RESET, OnChestsReset);
		Messenger.AddListener(MessengerEvents.CHESTS_PROCESSED, OnChestsProcessed);
	}

	/// <summary>
	/// First update call.
	/// </summary>
	private void Start() {
		// Initialize chests
		RefreshChests();
	}

	/// <summary>
	/// Destructor.
	/// </summary>
	private void OnDestroy() {
		// Unsubscribe from external events
		Messenger.RemoveListener(MessengerEvents.CHESTS_RESET, OnChestsReset);
		Messenger.RemoveListener(MessengerEvents.CHESTS_PROCESSED, OnChestsProcessed);
	}

	/// <summary>
	/// A change has occurred on the inspector. Validate its values.
	/// </summary>
	private void OnValidate() {
		// There must be exactly 5 chest slots
		if(m_chestSlots.Length != 5) {
			// Create a new array with exactly 5 slots and copy as many values as we can
			ChestsScreenSlot[] chestSlots = new ChestsScreenSlot[5];
			for(int i = 0; i < m_chestSlots.Length && i < chestSlots.Length; i++) {
				chestSlots[i] = m_chestSlots[i];
			}
			m_chestSlots = chestSlots;
		}
	}

	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Refreshs the chests.
	/// </summary>
	private void RefreshChests() {
		// Chest by chest
		Chest.RewardData rewardData;
		for(int i = 0; i < m_chestSlots.Length; i++) {
			// Skip if chest not initialized
			if(m_chestSlots[i] == null) continue;

			// Initialize with the state of that chest
			m_chestSlots[i].Init(ChestManager.collectedChests > i, ChestManager.GetRewardData(i + 1));
		}
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Chest Manager has reset the chests timer.
	/// </summary>
	private void OnChestsReset() {
		RefreshChests();
	}

	/// <summary>
	/// Chest Manager has processed the chests and given the reward.
	/// </summary>
	private void OnChestsProcessed() {
		RefreshChests();
	}
}