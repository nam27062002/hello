// IncubatorSlot.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 01/03/2016.
// Copyright (c) 2016 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// Controls a single slot on the incubator menu.
/// </summary>
public class IncubatorSlot : MonoBehaviour {
	//------------------------------------------------------------------//
	// CONSTANTS														//
	//------------------------------------------------------------------//

	//------------------------------------------------------------------//
	// MEMBERS															//
	//------------------------------------------------------------------//
	// Exposed setup
	[SerializeField] [Range(0, 2)] private int m_slotIdx = 0;	// Change range if EggManager.INVENTORY_SIZE changes
	public int slotIdx { 
		get { return m_slotIdx; }
	}

	// Scene references
	private GameObject m_3dView = null;
	private Text m_text = null;
	private IncubatorEggController m_eggView = null;

	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {
		// Get scene references
		m_3dView = this.FindObjectRecursive("View3D");
		m_text = this.FindComponentRecursive<Text>("Text");
	}

	/// <summary>
	/// First update call.
	/// </summary>
	private void Start() {
		// Perform a first refresh
		Refresh();
	}

	/// <summary>
	/// Component has been enabled.
	/// </summary>
	private void OnEnable() {
		// Subscribe to external events
		Messenger.AddListener<Egg, int>(GameEvents.EGG_ADDED_TO_INVENTORY, OnEggAddedToInventory);
		Messenger.AddListener<Egg>(GameEvents.EGG_INCUBATION_STARTED, OnEggIncubationStarted);
	}

	/// <summary>
	/// Component has been disabled.
	/// </summary>
	private void OnDisable() {
		// Unsubscribe from external events
		Messenger.RemoveListener<Egg, int>(GameEvents.EGG_ADDED_TO_INVENTORY, OnEggAddedToInventory);
		Messenger.RemoveListener<Egg>(GameEvents.EGG_INCUBATION_STARTED, OnEggIncubationStarted);
	}

	/// <summary>
	/// Refresh this slot with the latest data from the manager.
	/// </summary>
	public void Refresh() {
		// Aux vars
		Egg targetEgg = EggManager.inventory[slotIdx];

		// Text
		m_text.gameObject.SetActive(targetEgg == null);

		// Egg preview
		LoadEggPreview(targetEgg);
	}

	/// <summary>
	/// Loads/Unloads the egg preview.
	/// </summary>
	/// <param name="_newEgg">The data of the egg to be loaded. Set to <c>null</c> to destroy current preview.</param>
	private void LoadEggPreview(Egg _newEgg) {
		// 3 theoretical cases:
		// a) No egg was loaded but slot is filled
		// b) An egg was loaded but slot is empty
		// c) The slot is filled with a different egg than the loaded one (shouldn't happen, but add it just in case)

		// Skip if already loaded
		if(m_eggView != null && m_eggView.eggData == _newEgg) return;

		// Unload current view if any
		if(m_eggView != null) {
			GameObject.Destroy(m_eggView.gameObject);
			m_eggView = null;
		}

		// Load new view if any
		if(_newEgg != null) {
			GameObject newEggObj = _newEgg.CreateInstance();
			newEggObj.transform.SetParent(m_3dView.transform, false);
			newEggObj.SetLayerRecursively("3dOverUI");
			m_eggView = newEggObj.GetComponent<IncubatorEggController>();
		}
	}

	//------------------------------------------------------------------//
	// CALLBACKS														//
	//------------------------------------------------------------------//
	/// <summary>
	/// An egg has been added to the inventory.
	/// </summary>
	/// <param name="_newEgg">The egg added to the inventory.</param>
	/// <param name="_slotIdx">The inventory slot that has been updated.</param>
	private void OnEggAddedToInventory(Egg _newEgg, int _slotIdx) {
		// If slot index is the same as this one, refresh view
		if(_slotIdx == m_slotIdx) Refresh();
	}

	/// <summary>
	/// An egg has been added to the incubator.
	/// </summary>
	/// <param name="_egg">The egg.</param>
	private void OnEggIncubationStarted(Egg _egg) {
		// Does it match our egg?
		if(m_eggView != null && m_eggView.eggData == _egg) {
			// Lose preview reference (it has been moved to the incubator)
			m_eggView = null;

			// Refresh view
			Refresh();
		}
	}
}

