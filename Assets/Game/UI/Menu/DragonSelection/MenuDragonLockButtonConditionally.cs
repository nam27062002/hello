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
    [Comment("<color=orange>Lock per dragon state:</color>")]
    [SerializeField] private bool m_lockIfShadowed = false;
	[SerializeField] private bool m_lockIfLocked = false;
	[SerializeField] private bool m_lockIfAvailable = false;
	[SerializeField] private bool m_lockIfOwned = false;

    [Comment("<color=orange>Lock per dragon type:</color>")]
    [SerializeField] private bool m_lockIfClassic = false;
    [SerializeField] private bool m_lockIfSpecial = false;

    // External references
    private Button m_button = null;
	public Button button {
		get { 
			if(m_button == null) m_button = GetComponent<Button>();
			return m_button; 
		}
	}

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
		Messenger.AddListener<string>(MessengerEvents.MENU_DRAGON_SELECTED, OnDragonSelected);
		Messenger.AddListener<IDragonData>(MessengerEvents.DRAGON_ACQUIRED, OnDragonAcquired);
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
		Messenger.RemoveListener<string>(MessengerEvents.MENU_DRAGON_SELECTED, OnDragonSelected);
		Messenger.RemoveListener<IDragonData>(MessengerEvents.DRAGON_ACQUIRED, OnDragonAcquired);
	}

	//------------------------------------------------------------------//
	// OTHER METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Apply button lock based on given parameters.
	/// </summary>
	/// <param name="_sku">The sku of the dragon to be considered.</param>
	public void Apply(string _sku) {
		// Check whether the button should be locked or not
		bool toLock = false;
		IDragonData dragon = DragonManager.GetDragonData(_sku);
		if(dragon == null) return;

		// Ownership status
		switch(dragon.lockState) {
			case IDragonData.LockState.TEASE:	
			case IDragonData.LockState.SHADOW:
			case IDragonData.LockState.REVEAL:				toLock = m_lockIfShadowed;  break;

			case IDragonData.LockState.LOCKED:
			case IDragonData.LockState.LOCKED_UNAVAILABLE:	toLock = m_lockIfLocked;	break;

			case IDragonData.LockState.AVAILABLE:			toLock = m_lockIfAvailable;	break;

			case IDragonData.LockState.OWNED:				toLock = m_lockIfOwned;		break;
		}

        // Check type conditions
        if (m_lockIfClassic)
        {
            toLock |= (dragon.type == IDragonData.Type.CLASSIC);
        }

        if (m_lockIfSpecial)
        {
            toLock |= (dragon.type == IDragonData.Type.SPECIAL);
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
	public void OnDragonAcquired(IDragonData _data) {
		// It should be the selected dragon, but check anyway
		if(_data.def.sku != InstanceManager.menuSceneController.selectedDragon) {
			return;
		}

		// Update visibility
		Apply(_data.def.sku);
	}
}

