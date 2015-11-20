// MenuSceneController.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 21/08/2015.
// Copyright (c) 2015 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// Main controller for the menu scene.
/// </summary>
public class MenuSceneController : SceneController {
	//------------------------------------------------------------------//
	// CONSTANTS														//
	//------------------------------------------------------------------//
	public static readonly string NAME = "SC_Menu";

	//------------------------------------------------------------------//
	// MEMBERS															//
	//------------------------------------------------------------------//

	//------------------------------------------------------------------//
	// PROPERTIES														//
	//------------------------------------------------------------------//
	private DragonId m_selectedDragon = DragonId.NONE;
	public DragonId selectedDragon { get { return m_selectedDragon; }}

	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	override protected void Awake() {
		// Call parent
		base.Awake();

		// Initialize selected dragon getting the one from the profile
		m_selectedDragon = UserProfile.currentDragon;	// UserProfile should be loaded and initialized by now
	}

	/// <summary>
	/// Component enabled.
	/// </summary>
	void OnEnable() {
		// Subscribe to external events
		Messenger.AddListener<DragonId>(GameEvents.MENU_DRAGON_SELECTED, OnDragonSelectionChanged);
	}
	
	/// <summary>
	/// Component disabled.
	/// </summary>
	void OnDisable() {
		// Unsubscribe from external events
		Messenger.RemoveListener<DragonId>(GameEvents.MENU_DRAGON_SELECTED, OnDragonSelectionChanged);
	}

	//------------------------------------------------------------------//
	// CALLBACKS														//
	//------------------------------------------------------------------//
	/// <summary>
	/// Play button has been pressed.
	/// </summary>
	public void OnPlayButton() {
		// Go to game!
		// [AOC] No need to block the button, the GameFlow already controls spamming
		FlowManager.GoToGame();
	}

	/// <summary>
	/// Reset persistence button. Debug purposes only.
	/// </summary>
	public void OnResetButton() {
		PersistenceManager.Clear();
		FlowManager.Restart();
	}

	/// <summary>
	/// Add xp to the currently selected dragon (if owned). Debug purposes only.
	/// Will add 1/3 of the current level.
	/// </summary>
	public void OnAddXPButton() {
		// Get current selected dragon data
		DragonData data = DragonManager.GetDragonData(selectedDragon);
		if(!data.isOwned) return;

		// Add xp
		float amount = data.progression.GetXpRangeForLevel(data.progression.level).distance * 0.33f;
		data.progression.AddXp(amount, true);

		// Save persistence
		PersistenceManager.Save();

		// Simulate a dragon selected event so everything is refreshed
		Messenger.Broadcast<DragonId>(GameEvents.MENU_DRAGON_SELECTED, selectedDragon);
	}

	/// <summary>
	/// The selected dragon has changed.
	/// </summary>
	/// <param name="_id">The id of the selected dragon.</param>
	public void OnDragonSelectionChanged(DragonId _id) {
		// Update menu selected dragon
		m_selectedDragon = _id;

		// If owned and different from profile's current dragon, update profile
		if(_id != UserProfile.currentDragon && DragonManager.GetDragonData(_id).isOwned) {
			// Update profile
			UserProfile.currentDragon = _id;
		
			// Save persistence
			PersistenceManager.Save();
		}
	}
}

