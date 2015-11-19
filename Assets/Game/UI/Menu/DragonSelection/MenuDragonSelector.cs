// MenuDragonSelector.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 18/11/2015.
// Copyright (c) 2015 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// Select the current dragon in the menu screen.
/// </summary>
public class MenuDragonSelector : MonoBehaviour {
	//------------------------------------------------------------------//
	// CONSTANTS														//
	//------------------------------------------------------------------//
	public static readonly string EVENT_DRAGON_CHANGED = typeof(MenuDragonSelector).Name +  "_EVENT_DRAGON_CHANGED";

	//------------------------------------------------------------------//
	// MEMBERS															//
	//------------------------------------------------------------------//
	// References

	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {
		// Check required references
	}

	/// <summary>
	/// First update.
	/// </summary>
	private void Start() {

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

	}

	/// <summary>
	/// Changes dragon selected to the given one.
	/// </summary>
	/// <param name="_id">The id of the dragon we want to be the current one.</param>
	public void SetSelectedDragon(DragonId _id) {
		// Update profile
		UserProfile.currentDragon = _id;

		// Save persistence
		PersistenceManager.Save();
		
		// Notify game
		Messenger.Broadcast(EVENT_DRAGON_CHANGED);
	}

	//------------------------------------------------------------------//
	// CALLBACKS														//
	//------------------------------------------------------------------//
	/// <summary>
	/// Select next dragon. To be linked with the "next" button.
	/// </summary>
	public void SelectNextDragon() {
		// Figure out next dragon's id
		DragonId newId = UserProfile.currentDragon + 1;
		if(newId == DragonId.COUNT) newId = DragonId.NONE + 1;

		// Change selection
		SetSelectedDragon(newId);
	}

	/// <summary>
	/// Select previous dragon. To be linked with the "previous" button.
	/// </summary>
	public void SelectPreviousDragon() {
		// Figure out previous dragon's id
		DragonId newId = UserProfile.currentDragon - 1;
		if((int)newId < 0) newId = DragonId.COUNT - 1;

		// Change selection
		SetSelectedDragon(newId);
	}
}

