// CPProgressionCheats.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 21/04/2016.
// Copyright (c) 2016 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// All cheats related to progression
/// </summary>
public class CPProgressionCheats : MonoBehaviour {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	
	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	
	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//

	//------------------------------------------------------------------------//
	// INTERNAL METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Make sure we're in the proper scene to execute the cheat.
	/// </summary>
	/// <returns><c>true</c> if we're in the right scene :)</returns>
	private bool CheckScene() {
		// Is this the menu scene?
		if(InstanceManager.sceneController.GetType() != typeof(MenuSceneController)) {
			UIFeedbackText.CreateAndLaunch("Can only be used in the menu!", new Vector2(0.5f, 0.5f), ControlPanel.panel.parent as RectTransform, "CPFeedbackText");
			return false;
		}
		return true;
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Reset full progress and restart application.
	/// <param name="_skipTutorial">Whether to skip the tutorial on the resetted persistence file.</param>
	/// </summary>
	public void OnResetProgress(bool _skipTutorial) {
		// If not in the menu, show feedback message and return
		if(!CheckScene()) return;

		// Close control panel
		ControlPanel.instance.Toggle();

		// Clear persistence
		PersistenceManager.Clear();

		// If required, tutorial will be auto-completed next time we reload the profile
		PlayerPrefs.DeleteAll();
		Prefs.SetBoolPlayer("skipTutorialCheat", _skipTutorial);

		// Restart game
		FlowManager.Restart();
	}

	/// <summary>
	/// Set the amount of coins on the profile.
	/// </summary>
	public void OnSetCoins() {
		// Get amount from linked input field
		InputField input = GetComponentInChildren<InputField>();
		if(input == null) Debug.Log("Requires a nested Input Field!");
		long amount = long.Parse(input.text);

		// Update profile - make sure amount is valid
		// Use longs to allow big numbers
		long toAdd = System.Math.Max(amount - UsersManager.currentUser.coins, -UsersManager.currentUser.coins);	// Min 0 coins! This will exclude negative amounts :)
		UsersManager.currentUser.AddCoins(toAdd);

		// Save persistence
		PersistenceManager.Save();
	}

	/// <summary>
	/// Set the amount of PC on the profile.
	/// </summary>
	public void OnSetPC() {
		// Get amount from linked input field
		InputField input = GetComponentInChildren<InputField>();
		if(input == null) Debug.Log("Requires a nested Input Field!");
		long amount = long.Parse(input.text);

		// Update profile - make sure amount is valid
		UserProfile currentUser = UsersManager.instance.m_currentUser;
		long toAdd = System.Math.Max(amount - currentUser.pc, -currentUser.pc);	// Min 0 pc! This will exclude negative amounts :)
		currentUser.AddPC(toAdd);

		// Save persistence
		PersistenceManager.Save();
	}

	/// <summary>
	/// Add xp to the currently selected dragon (if owned). Debug purposes only.
	/// Will add 1/3 of the current level.
	/// </summary>
	public void OnAddXPButton() {
		// If not in the menu, show feedback message and return
		if(!CheckScene()) return;

		// Get current selected dragon data
		string selectedDragonSku = InstanceManager.GetSceneController<MenuSceneController>().selectedDragon;
		DragonData data = DragonManager.GetDragonData(selectedDragonSku);
		if(!data.isOwned) {
			UIFeedbackText.CreateAndLaunch("Only for owned dragons!", new Vector2(0.5f, 0.5f), ControlPanel.panel.parent as RectTransform, "CPFeedbackText");
			return;
		}

		// Add xp
		float amount = data.progression.GetXpRangeForLevel(data.progression.level).distance * 0.33f;
		data.progression.AddXp(amount, true);

		// Save persistence
		PersistenceManager.Save();

		// Simulate a dragon selected event so everything is refreshed
		Messenger.Broadcast<string>(GameEvents.MENU_DRAGON_SELECTED, selectedDragonSku);
	}

	/// <summary>
	/// Add egg to inventory cheat.
	/// </summary>
	public void OnAddEggToInventory() {
		// If not in the menu, show feedback message and return
		if(!CheckScene()) return;

		// Add it to the inventory
		int slotIdx = EggManager.AddEggToInventory(Egg.CreateFromSku(Egg.SKU_STANDARD_EGG));

		// If successful, save persistence
		if(slotIdx >= 0) PersistenceManager.Save();
	}
}