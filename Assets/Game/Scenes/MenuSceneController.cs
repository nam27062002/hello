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
	private string m_selectedDragon = "";
	public string selectedDragon { get { return m_selectedDragon; }}

	private string m_selectedLevel = "";
	public string selectedLevel { get { return m_selectedLevel; }}

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

		// Initialize the selected level in a similar fashion
		m_selectedLevel = UserProfile.currentLevel;		// UserProfile should be loaded and initialized by now
	}

	/// <summary>
	/// Component enabled.
	/// </summary>
	void OnEnable() {
		// Subscribe to external events
		Messenger.AddListener<string>(GameEvents.MENU_DRAGON_SELECTED, OnDragonSelected);
		Messenger.AddListener<DragonData>(GameEvents.DRAGON_ACQUIRED, OnDragonAcquired);
		Messenger.AddListener<string>(GameEvents.MENU_LEVEL_SELECTED, OnLevelSelected);
	}
	
	/// <summary>
	/// Component disabled.
	/// </summary>
	void OnDisable() {
		// Unsubscribe from external events
		Messenger.RemoveListener<string>(GameEvents.MENU_DRAGON_SELECTED, OnDragonSelected);
		Messenger.RemoveListener<DragonData>(GameEvents.DRAGON_ACQUIRED, OnDragonAcquired);
		Messenger.RemoveListener<string>(GameEvents.MENU_LEVEL_SELECTED, OnLevelSelected);
	}

	//------------------------------------------------------------------//
	// CALLBACKS														//
	//------------------------------------------------------------------//
	/// <summary>
	/// Play button has been pressed.
	/// </summary>
	public void OnPlayButton() {
		// If selected level is unlocked, update profile
		if(m_selectedLevel != UserProfile.currentLevel && LevelManager.IsLevelUnlocked(m_selectedLevel)) {
			// Update profile
			UserProfile.currentLevel = m_selectedLevel;
			
			// Save persistence
			PersistenceManager.Save();
		}

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
		Messenger.Broadcast<string>(GameEvents.MENU_DRAGON_SELECTED, selectedDragon);
	}

	/// <summary>
	/// The selected dragon has changed.
	/// </summary>
	/// <param name="_id">The id of the selected dragon.</param>
	public void OnDragonSelected(string _sku) {
		// Update menu selected dragon
		m_selectedDragon = _sku;

		// If owned and different from profile's current dragon, update profile
		if(_sku != UserProfile.currentDragon && DragonManager.GetDragonData(_sku).isOwned) {
			// Update profile
			UserProfile.currentDragon = _sku;
		
			// Save persistence
			PersistenceManager.Save();
		}
	}

	/// <summary>
	/// A dragon has been unlocked.
	/// </summary>
	/// <param name="_data">The dragon that has been unlocked.</param>
	public void OnDragonAcquired(DragonData _data) {
		// Just make it the current dragon
		OnDragonSelected(_data.def.sku);
	}

	/// <summary>
	/// A level has been selected.
	/// </summary>
	/// <param name="_levelSku">The sku of the selected level.</param>
	public void OnLevelSelected(string _levelSku) {
		// Update menu selected level
		m_selectedLevel = _levelSku;
	}
}

