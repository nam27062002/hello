// IncubatorEggAnchor.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 29/02/2016.
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
/// Controls the anchor of the eggs on the incubator.
/// </summary>
public class IncubatorEggAnchor : MonoBehaviour {
	//------------------------------------------------------------------//
	// CONSTANTS														//
	//------------------------------------------------------------------//

	//------------------------------------------------------------------//
	// MEMBERS															//
	//------------------------------------------------------------------//
	// Exposed
	[SerializeField] [Range(0, 2)] private int m_slotIdx = 0;	// Change range if EggManager.INVENTORY_SIZE changes
	[SerializeField] private GameObject m_emptySlotView = null;

	// Internal
	private EggController m_eggView = null;
	public EggController eggView {
		get { return m_eggView; }
	}

	// Properties
	public Egg targetEgg {
		get { return EggManager.inventory[m_slotIdx]; }
	}

	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// First update.
	/// </summary>
	private void Start() {
		// Initialize by forcing a refresh
		Refresh();

		// Subscribe to external events
		Messenger.AddListener<Egg, Egg.State, Egg.State>(GameEvents.EGG_STATE_CHANGED, OnEggStateChanged);
	}

	/// <summary>
	/// Destructor,
	/// </summary>
	private void OnDestroy() {
		// Unsubscribe from external events
		Messenger.RemoveListener<Egg, Egg.State, Egg.State>(GameEvents.EGG_STATE_CHANGED, OnEggStateChanged);
	}

	/// <summary>
	/// Draw scene gizmos for this object.
	/// </summary>
	private void OnDrawGizmos() {
		Gizmos.color = Colors.orange;
		Gizmos.DrawSphere(transform.position, 1f);
		Gizmos.color = Colors.white;
	}

	//------------------------------------------------------------------//
	// OTHER METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Refresh this slot with the latest data from the manager.
	/// </summary>
	public void Refresh() {
		// Background
		m_emptySlotView.SetActive(targetEgg == null || targetEgg.state == Egg.State.COLLECTED);

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
			m_eggView = _newEgg.CreateView();
			m_eggView.transform.SetParent(this.transform, false);
		}
	}

	//------------------------------------------------------------------//
	// CALLBACKS														//
	//------------------------------------------------------------------//
	/// <summary>
	/// An egg has been added to the incubator.
	/// </summary>
	/// <param name="_egg">The egg.</param>
	private void OnEggStateChanged(Egg _egg, Egg.State _from, Egg.State _to) {
		// Does it match our egg?
		if(_egg == EggManager.inventory[m_slotIdx]) {
			// Refresh view
			// [AOC] TODO!! Trigger different FX depending on state
			Refresh();
		}
	}
}

