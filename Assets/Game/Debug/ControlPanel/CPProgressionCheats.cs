﻿// CPProgressionCheats.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 21/04/2016.
// Copyright (c) 2016 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using System.Collections.Generic;
using TMPro;
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

        // Clear persistence and sets the default persistence
        PersistenceManager.Clear();

        UsersManager.Reset();

        // We need to load the default persistence in order to make UserManager.currentUser load the default data
        FGOL.Save.SaveGameManager.Instance.Load(Authenticator.Instance.User);

        // Reset all preferences
        PlayerPrefs.DeleteAll();

		// If required, tutorial will be auto-completed next time we reload the profile
		Prefs.SetBoolPlayer("skipTutorialCheat", _skipTutorial);

		// Initialize debug settings, they have been reset when reseting the preferences
		DebugSettings.Init();

		// Restart game
		FlowManager.Restart();
	}

	/// <summary>
	/// Set the amount of coins on the profile.
	/// </summary>
	public void OnSetCoins() {
		// Get amount from linked input field
		TMP_InputField input = GetComponentInChildren<TMP_InputField>();
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
		TMP_InputField input = GetComponentInChildren<TMP_InputField>();
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
		UIFeedbackText.CreateAndLaunch("+" + amount, new Vector2(0.5f, 0.5f), ControlPanel.panel.parent as RectTransform, "CPFeedbackText");

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

	/// <summary>
	/// Set the amount of golden egg fragments.
	/// </summary>
	public void OnSetGoldenEggFragments() {
		// Get amount from linked input field
		TMP_InputField input = GetComponentInChildren<TMP_InputField>();
		if(input == null) Debug.Log("Requires a nested Input Field!");
		int amount = int.Parse(input.text);

		// Update profile - make sure amount is valid
		UserProfile currentUser = UsersManager.instance.m_currentUser;
		currentUser.goldenEggFragments = Mathf.Clamp(amount, 0, EggManager.goldenEggRequiredFragments);		// Clamp between limits!

		// Save persistence
		PersistenceManager.Save();
	}

	/// <summary>
	/// Unlock and buy all dragons.
	/// </summary>
    public void OnAcquireAllDragons() {
        List<DragonData> dragons = DragonManager.GetDragonsByLockState(DragonData.LockState.ANY);
        if (dragons != null) {
            int i;
            int count = dragons.Count;
            for (i = 0; i < count; i++) {
                if (!dragons[i].isOwned) {
                    dragons[i].Acquire();
                }
            }
        }
    }

	/// <summary>
	/// Unlock all pets.
	/// </summary>
	public void OnUnlockAllPets() {
		List<DefinitionNode> petDefs = new List<DefinitionNode>();
		DefinitionsManager.SharedInstance.GetDefinitions(DefinitionsCategory.PETS, ref petDefs);
		for(int i = 0; i < petDefs.Count; i++) {
			UsersManager.currentUser.petCollection.UnlockPet(petDefs[i].sku);
		}
		PersistenceManager.Save();
	}

    /// <summary>
    /// Simulates daily chest collection (no menu refresh for now, reload menu for that).
    /// </summary>
    public void OnAddDailyChest() {
		// Find the first non-collected chest
		Chest ch = null;
		foreach(Chest chest in ChestManager.dailyChests) {
			if(!chest.collected) {
				ch = chest;
				break;
			}
		}

		// Mark it as collected and process rewards
		if(ch != null) {
			ch.ChangeState(Chest.State.PENDING_REWARD);
			ChestManager.ProcessChests();
		}
	}

	/// <summary>
	/// Simulate daily chests timer expiration.
	/// </summary>
	public void OnResetDailyChests() {
		ChestManager.Reset();
	}
}