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
	public enum CurrencyOperation {
		ADD,
		REMOVE,
		SET
	}
	
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

		// Reset some managers
		GlobalEventManager.ClearCurrentEvent();

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
	/// Read the input textfield and set the amount of a target currency on the profile.
	/// </summary>
	/// <param name="_currency">Target currency.</param>
	/// <param name="_operation">Target operation.</param>
	public void OnSetCurrency(UserProfile.Currency _currency, CurrencyOperation _operation) {
		// Use longs to allow big numbers
		// Get amount from linked input field
		TMP_InputField input = GetComponentInChildren<TMP_InputField>();
		if(input == null) Debug.Log("Requires a nested Input Field!");
		long amount = long.Parse(input.text);

		// Compute amount to add
		switch(_operation) {
			case CurrencyOperation.ADD: {
				UsersManager.currentUser.EarnCurrency(_currency, (ulong)amount, false, HDTrackingManager.EEconomyGroup.CHEAT);
			} break;

			case CurrencyOperation.REMOVE: {
				UsersManager.currentUser.SpendCurrency(_currency, (ulong)amount);
			} break;

			case CurrencyOperation.SET: {
				// Apply the amount respecting the current free/paid ratio
				UserProfile.CurrencyData data = UsersManager.currentUser.GetCurrencyData(_currency);
				float ratio = data.amount > 0 ? (float)data.freeAmount/(float)data.amount : 1f;
				UsersManager.currentUser.SetCurrency(_currency, (long)(amount * ratio), (long)(amount * (1f - ratio)));
			} break;
		}

        // Save persistence
        PersistenceFacade.instance.Save_Request();
    }

	/// <summary>
	/// Specialized versions to be used as button callbacks.
	/// </summary>
	public void OnAddCoins() 	{ OnSetCurrency(UserProfile.Currency.SOFT, CurrencyOperation.ADD); }
	public void OnRemoveCoins() { OnSetCurrency(UserProfile.Currency.SOFT, CurrencyOperation.REMOVE); }
	public void OnSetCoins() 	{ OnSetCurrency(UserProfile.Currency.SOFT, CurrencyOperation.SET); }

	public void OnAddPC() 		{ OnSetCurrency(UserProfile.Currency.HARD, CurrencyOperation.ADD); }
	public void OnRemovePC() 	{ OnSetCurrency(UserProfile.Currency.HARD, CurrencyOperation.REMOVE); }
	public void OnSetPC() 		{ OnSetCurrency(UserProfile.Currency.HARD, CurrencyOperation.SET); }

	public void OnSetGoldenFragments() 		{ OnSetCurrency(UserProfile.Currency.GOLDEN_FRAGMENTS, CurrencyOperation.SET); }
	public void OnAddGoldenFragments() 		{ OnSetCurrency(UserProfile.Currency.GOLDEN_FRAGMENTS, CurrencyOperation.ADD); }
	public void OnRemoveGoldenFragments() 	{ OnSetCurrency(UserProfile.Currency.GOLDEN_FRAGMENTS, CurrencyOperation.REMOVE); }

	public void OnAddKeys() 	{ OnSetCurrency(UserProfile.Currency.KEYS, CurrencyOperation.ADD); }
	public void OnRemoveKeys() 	{ OnSetCurrency(UserProfile.Currency.KEYS, CurrencyOperation.REMOVE); }
	public void OnSetKeys() 	{ OnSetCurrency(UserProfile.Currency.KEYS, CurrencyOperation.SET); }

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
        PersistenceFacade.instance.Save_Request();

        // Simulate a dragon selected event so everything is refreshed
        Messenger.Broadcast<string>(GameEvents.MENU_DRAGON_SELECTED, selectedDragonSku);
	}

	/// <summary>
	/// Initialize XP slider with data from the currently selected dragon.
	/// </summary>
	public void OnInitXPSlider() {
		// Find linked slider
		Slider slider = GetComponentInChildren<Slider>();
		if(slider == null) Debug.Log("Requires a nested Slider!");

		// If in the menu and selected dragon is not owned, disable slider
		DragonData targetDragon = null;
		if(InstanceManager.menuSceneController != null) {
			// Get current selected dragon data
			DragonData data = DragonManager.GetDragonData(InstanceManager.menuSceneController.selectedDragon);
			if(data.isOwned) targetDragon = data;
		} else {
			// Not in the menu, use current dragon's data
			targetDragon = DragonManager.currentDragon;
		}

		// Disable slider if dragon cannot be changed
		slider.interactable = (targetDragon != null);

		// Initialize slider
		if(targetDragon == null) {
			slider.minValue = 0f;
			slider.maxValue = 1f;
			slider.value = 0f;
		} else {
			slider.minValue = 0f;
			slider.maxValue = targetDragon.progression.GetXpRangeForLevel(targetDragon.progression.maxLevel).max;
			slider.value = targetDragon.progression.xp;
		}
	}

	/// <summary>
	/// Set the global XP for the currently selected dragon (if owned).
	/// </summary>
	public void OnSetXPSlider(float _xp) {
		// If not in the menu, show feedback message and return
		if(!CheckScene()) return;

		// Get current selected dragon data
		string selectedDragonSku = InstanceManager.menuSceneController.selectedDragon;
		DragonData data = DragonManager.GetDragonData(selectedDragonSku);
		if(!data.isOwned) {
			UIFeedbackText.CreateAndLaunch("Only for owned dragons!", new Vector2(0.5f, 0.5f), ControlPanel.panel.parent as RectTransform, "CPFeedbackText");
			return;
		}

		// Set xp
		data.progression.SetXp_DEBUG(_xp);

		// Process unlocked skins for current dragon
		UsersManager.currentUser.wardrobe.ProcessUnlockedSkins(data);

        // Save persistence
        PersistenceFacade.instance.Save_Request();

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
		if(slotIdx >= 0) PersistenceFacade.instance.Save_Request();
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

            // Save persistence
            PersistenceFacade.instance.Save_Request();
        }
    }

	/// <summary>
	/// Reset all the dragons.
	/// </summary>
	public void OnResetAllDragons() {
		List<DragonData> dragons = DragonManager.GetDragonsByLockState(DragonData.LockState.ANY);
		if (dragons != null) {
			int i;
			int count = dragons.Count;
			for (i = 0; i < count; i++) {
				// Reset dragon data
				dragons[i].ResetLoadedData();

				// Reset this dragon skins as well
				List<DefinitionNode> skinDefs = DefinitionsManager.SharedInstance.GetDefinitionsByVariable(DefinitionsCategory.DISGUISES, "dragonSku", dragons[i].def.sku);
				for(int j = 0; j < skinDefs.Count; ++j) {
					// Special case: if unlock level is 0, mark it as owned! (probably dragon's default skin)
					if(skinDefs[j].GetAsInt("unlockLevel") <= 0) {
						UsersManager.currentUser.wardrobe.SetSkinState(skinDefs[j].sku, Wardrobe.SkinState.OWNED);
					} else {
						UsersManager.currentUser.wardrobe.SetSkinState(skinDefs[j].sku, Wardrobe.SkinState.LOCKED);
					}
				}
			}

            // Save persistence
            PersistenceFacade.instance.Save_Request();
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
		UsersManager.currentUser.goldenEggsCollected = 100; // Dirty, but should do the trick xD

        // Save persistence
        PersistenceFacade.instance.Save_Request();
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
		UsersManager.currentUser.goldenEggsCollected = 0;
		UsersManager.currentUser.SetCurrency(UserProfile.Currency.GOLDEN_FRAGMENTS, 0, 0);

        // Save!
        PersistenceFacade.instance.Save_Request();
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
		UsersManager.currentUser.goldenEggsCollected = 0;
		UsersManager.currentUser.SetCurrency(UserProfile.Currency.GOLDEN_FRAGMENTS, 0, 0);

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
		UsersManager.currentUser.SetCurrency(UserProfile.Currency.GOLDEN_FRAGMENTS, (long)Random.Range(0, EggManager.goldenEggRequiredFragments), 0);

        // Save!
        PersistenceFacade.instance.Save_Request();
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
		UsersManager.currentUser.SetCurrency(UserProfile.Currency.GOLDEN_FRAGMENTS, 0, 0);
		UsersManager.currentUser.goldenEggsCollected = 0;

        // Save!
        PersistenceFacade.instance.Save_Request();
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
		UsersManager.currentUser.mapResetTimestamp = GameServerManager.SharedInstance.GetEstimatedServerTime(); // Already expired
        PersistenceFacade.instance.Save_Request();
    }
}