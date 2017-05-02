// CPProgressionCheats.cs
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

		// Backup all preferences that must be kept between resets
		int resetProgressCount = PlayerPrefs.GetInt("RESET_PROGRESS_COUNT", 0);

        // Reset all preferences
        PlayerPrefs.DeleteAll();

		// Restore preferences that must be kept between resets
		PlayerPrefs.SetInt("RESET_PROGRESS_COUNT", resetProgressCount + 1);

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
		long toAdd = System.Math.Max(amount - UsersManager.currentUser.pc, -UsersManager.currentUser.pc);	// Min 0 pc! This will exclude negative amounts :)
		UsersManager.currentUser.AddPC(toAdd);

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
		string selectedDragonSku = InstanceManager.menuSceneController.selectedDragon;
		DragonData data = DragonManager.GetDragonData(selectedDragonSku);
		if(!data.isOwned) {
			UIFeedbackText.CreateAndLaunch("Only for owned dragons!", new Vector2(0.5f, 0.5f), ControlPanel.panel.parent as RectTransform, "CPFeedbackText");
			return;
		}

		// Add xp
		float amount = data.progression.GetXpRangeForLevel(data.progression.level).distance * 0.3f;
		data.progression.AddXp(amount, true);
		UIFeedbackText.CreateAndLaunch("+" + amount, new Vector2(0.5f, 0.5f), ControlPanel.panel.parent as RectTransform, "CPFeedbackText");

		// Process unlocked skins for current dragon
		UsersManager.currentUser.wardrobe.ProcessUnlockedSkins(data);

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
		UsersManager.currentUser.goldenEggFragments = Mathf.Max(0, amount);		// No negative values!

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
		// Do it!
		List<DefinitionNode> petDefs = DefinitionsManager.SharedInstance.GetDefinitionsList(DefinitionsCategory.PETS);
		for(int i = 0; i < petDefs.Count; i++) {
			UsersManager.currentUser.petCollection.UnlockPet(petDefs[i].sku);
		}

		// Mark all golden eggs as collected
		UsersManager.currentUser.goldenEggsCollected = 100;	// Dirty, but should do the trick xD

		// Save persistence
		PersistenceManager.Save();
	}

	/// <summary>
	/// Reset all pets to lock state.
	/// Resets golden egg fragments and collected eggs count as well.
	/// </summary>
	public void OnResetAllPets() {
		// Clear equipped pets
		foreach(DragonData dragon in DragonManager.dragonsByOrder) {
			for(int i = 0; i < dragon.pets.Count; i++) {
				UsersManager.currentUser.UnequipPet(dragon.def.sku, i);
			}
		}

		// Clear pet collection
		UsersManager.currentUser.petCollection.Reset();

		// Clear collected eggs and fragments
		UsersManager.currentUser.eggsCollected = 0;
		UsersManager.currentUser.goldenEggFragments = 0;
		UsersManager.currentUser.goldenEggsCollected = 0;

		// Save!
		PersistenceManager.Save();
	}

	/// <summary>
	/// Reset the pets collection to a random amount of locked/unlocked pets.
	/// Resets golden egg fragments and collected eggs count as well.
	/// </summary>
	public void OnResetPetsRandomly() {
		// Clear equipped pets
		foreach(DragonData dragon in DragonManager.dragonsByOrder) {
			for(int i = 0; i < dragon.pets.Count; i++) {
				UsersManager.currentUser.UnequipPet(dragon.def.sku, i);
			}
		}

		// Clear pet collection and eggs and fragments
		UsersManager.currentUser.petCollection.Reset();
		UsersManager.currentUser.eggsCollected = 0;
		UsersManager.currentUser.goldenEggFragments = 0;
		UsersManager.currentUser.goldenEggsCollected = 0;

		// Iterate all the pets list and randomly unlock some of them
		float unlockChance = 0.5f;
		List<DefinitionNode> petDefs = DefinitionsManager.SharedInstance.GetDefinitionsList(DefinitionsCategory.PETS);
		foreach(DefinitionNode petDef in petDefs) {
			// Unlock?
			if(Random.value > unlockChance) continue;	// No, try with next pet

			// Unlock!
			UsersManager.currentUser.petCollection.UnlockPet(petDef.sku);

			// Increase collected eggs (pets come from eggs!)
			UsersManager.currentUser.eggsCollected++;

			// If special, increase collected golden eggs
			if(petDef.Get("rarity") == EggReward.RarityToSku(EggReward.Rarity.SPECIAL)) {
				UsersManager.currentUser.goldenEggsCollected++;
			}
		}

		// Initialize current amount of golden fragments to a random value within the limit
		UsersManager.currentUser.goldenEggFragments = Random.Range(0, EggManager.goldenEggRequiredFragments);

		// Save!
		PersistenceManager.Save();
	}

	/// <summary>
	/// Reset only the special pets to lock state.
	/// Resets golden egg fragments and collected golden eggs count as well.
	/// </summary>
	public void OnResetSpecialPets() {
		// Get all special pets
		List<DefinitionNode> petDefs = DefinitionsManager.SharedInstance.GetDefinitionsByVariable(DefinitionsCategory.PETS, "rarity", EggReward.RarityToSku(EggReward.Rarity.SPECIAL));

		// Clear equipped pets
		foreach(DragonData dragon in DragonManager.dragonsByOrder) {
			for(int i = 0; i < petDefs.Count; i++) {
				UsersManager.currentUser.UnequipPet(dragon.def.sku, petDefs[i].sku);
			}
		}

		// Remove from collection
		for(int i = 0; i < petDefs.Count; i++) {
			UsersManager.currentUser.petCollection.RemovePet(petDefs[i].sku);
		}

		// Clear collected data
		UsersManager.currentUser.goldenEggFragments = 0;
		UsersManager.currentUser.goldenEggsCollected = 0;

		// Save!
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

	/// <summary>
	/// Reset all map upgrades
	/// </summary>
	public void OnResetMapUpgrades() {
		// Not much to do:
		UsersManager.currentUser.mapResetTimestamp = System.DateTime.UtcNow;	// Already expired
		PersistenceManager.Save();
	}
}