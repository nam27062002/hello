// MenuDragonLockButtonConditionally.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 24/05/2016.
// Copyright (c) 2016 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Simple behaviour to lock/unlock the target button depending on the 
/// selected dragon and whether it's owned or not.
/// </summary>
//[RequireComponent(typeof(Button))]
public class MenuDragonLockButtonConditionally : MonoBehaviour {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//

	//------------------------------------------------------------------------//
	// MEMBERS																  //
	//------------------------------------------------------------------------//
	// Setup
	[SerializeField] private bool m_lockIfLocked = false;
	[SerializeField] private bool m_lockIfAvailable = false;
	[SerializeField] private bool m_lockIfOwned = false;

	// External references
	private Button m_button = null;

	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {
		// Get external references
		m_button = GetComponent<Button>();

		// Subscribe to external events
		Messenger.AddListener<string>(GameEvents.MENU_DRAGON_SELECTED, OnDragonSelected);
		Messenger.AddListener<DragonData>(GameEvents.DRAGON_ACQUIRED, OnDragonAcquired);
	}

	/// <summary>
	/// First update.
	/// </summary>
	private void Start() {
		// Apply for the first time with currently selected dragon
		Apply(InstanceManager.menuSceneController.selectedDragon);
	}
	
	/// <summary>
	/// Called every frame.
	/// </summary>
	private void Update() {

	}

	/// <summary>
	/// Destructor.
	/// </summary>
	private void OnDestroy() {
		// Unsubscribe from external events
		Messenger.RemoveListener<string>(GameEvents.MENU_DRAGON_SELECTED, OnDragonSelected);
		Messenger.RemoveListener<DragonData>(GameEvents.DRAGON_ACQUIRED, OnDragonAcquired);
	}

	//------------------------------------------------------------------//
	// INTERNAL METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Apply button lock based on given parameters.
	/// </summary>
	/// <param name="_sku">The sku of the dragon to be considered.</param>
	private void Apply(string _sku) {
		// Check whether the button should be locked or not
		bool toLock = false;
		DragonData dragon = DragonManager.GetDragonData(_sku);
		if(dragon == null) return;

		// Ownership status
		switch(dragon.lockState) {
			case DragonData.LockState.LOCKED:		toLock = m_lockIfLocked;	break;
			case DragonData.LockState.AVAILABLE:	toLock = m_lockIfAvailable;	break;
			case DragonData.LockState.OWNED:		toLock = m_lockIfOwned;		break;
		}

		// Just do it
		m_button.interactable = !toLock;
	}

	//------------------------------------------------------------------//
	// CALLBACKS														//
	//------------------------------------------------------------------//
	/// <summary>
	/// The selected dragon on the menu has changed.
	/// </summary>
	/// <param name="_sku">The sku of the newly selected dragon.</param>
	public void OnDragonSelected(string _sku) {
		// Just update visibility
		Apply(_sku);
	}

	/// <summary>
	/// A dragon has been acquired
	/// </summary>
	/// <param name="_data">The data of the acquired dragon.</param>
	public void OnDragonAcquired(DragonData _data) {
		// It should be the selected dragon, but check anyway
		if(_data.def.sku != InstanceManager.menuSceneController.selectedDragon) {
			return;
		}

		// Update visibility
		Apply(_data.def.sku);
	}
}

