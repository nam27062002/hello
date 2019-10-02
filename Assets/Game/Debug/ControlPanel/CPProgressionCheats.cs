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

		// Reset some managers
		GlobalEventManager.ClearCurrentEvent();

		// Reset dragons
		OnResetAllDragons();
		OnResetAllPets();

        // Clear persistence and sets the default persistence
        PersistenceFacade.instance.LocalDriver.OverrideWithDefault(null);

        UsersManager.Reset();        

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

        // Downloadables folder is deleted
        AssetBundlesManager.Instance.DeleteAllDownloadables();

		// Restart game
		FlowManager.Restart();
	}

	/// <summary>
	/// Read the input textfield and set the amount of a target currency on the profile.
	/// </summary>
	/// <param name="_currency">Target currency.</param>
	/// <param name="_operation">Target operation.</param>
	public void OnSetCurrency(UserProfile.Currency _currency, CPOperation _operation) {
		// Use longs to allow big numbers
		// Get amount from linked input field
		TMP_InputField input = GetComponentInChildren<TMP_InputField>();
		if(input == null) Debug.Log("Requires a nested Input Field!");
		long amount = long.Parse(input.text);

		// Compute amount to add
		switch(_operation) {
			case CPOperation.ADD: {
				UsersManager.currentUser.EarnCurrency(_currency, (ulong)amount, false, HDTrackingManager.EEconomyGroup.CHEAT);
			} break;

			case CPOperation.REMOVE: {
				UsersManager.currentUser.SpendCurrency(_currency, (ulong)amount);
			} break;

			case CPOperation.SET: {
				// Apply the amount respecting the current free/paid ratio
				UserProfile.CurrencyData data = UsersManager.currentUser.GetCurrencyData(_currency);
				float ratio = data.amount > 0 ? (float)data.freeAmount/(float)data.amount : 1f;
				UsersManager.currentUser.SetCurrency(_currency, (long)(amount * ratio), (long)(amount * (1f - ratio)));
			} break;
		}

        // Save persistence
        PersistenceFacade.instance.Save_Request(false);
    }

	/// <summary>
	/// Specialized versions to be used as button callbacks.
	/// </summary>
	public void OnAddCoins() 	{ OnSetCurrency(UserProfile.Currency.SOFT, CPOperation.ADD); }
	public void OnRemoveCoins() { OnSetCurrency(UserProfile.Currency.SOFT, CPOperation.REMOVE); }
	public void OnSetCoins() 	{ OnSetCurrency(UserProfile.Currency.SOFT, CPOperation.SET); }

	public void OnAddPC() 		{ OnSetCurrency(UserProfile.Currency.HARD, CPOperation.ADD); }
	public void OnRemovePC() 	{ OnSetCurrency(UserProfile.Currency.HARD, CPOperation.REMOVE); }
	public void OnSetPC() 		{ OnSetCurrency(UserProfile.Currency.HARD, CPOperation.SET); }

	public void OnSetGoldenFragments() 		{ OnSetCurrency(UserProfile.Currency.GOLDEN_FRAGMENTS, CPOperation.SET); }
	public void OnAddGoldenFragments() 		{ OnSetCurrency(UserProfile.Currency.GOLDEN_FRAGMENTS, CPOperation.ADD); }
	public void OnRemoveGoldenFragments() 	{ OnSetCurrency(UserProfile.Currency.GOLDEN_FRAGMENTS, CPOperation.REMOVE); }

	public void OnAddKeys() 	{ OnSetCurrency(UserProfile.Currency.KEYS, CPOperation.ADD); }
	public void OnRemoveKeys() 	{ OnSetCurrency(UserProfile.Currency.KEYS, CPOperation.REMOVE); }
	public void OnSetKeys() 	{ OnSetCurrency(UserProfile.Currency.KEYS, CPOperation.SET); }

	/// <summary>
	/// Add xp to the currently selected dragon (if owned). Debug purposes only.
	/// Will add 1/3 of the current level.
	/// </summary>
	public void OnAddXPButton() {
		// If not in the menu, show feedback message and return
		if(!CheckScene()) return;

		// Get current selected dragon data
		string selectedDragonSku = InstanceManager.menuSceneController.selectedDragon;
		IDragonData data = DragonManager.GetDragonData(selectedDragonSku);
		if(!data.isOwned) {
			UIFeedbackText.CreateAndLaunch("Only for owned dragons!", new Vector2(0.5f, 0.5f), ControlPanel.panel.parent as RectTransform, "CPFeedbackText");
			return;
		}

		if(data.type != IDragonData.Type.CLASSIC) {
			UIFeedbackText.CreateAndLaunch("Only for classic dragons!", new Vector2(0.5f, 0.5f), ControlPanel.panel.parent as RectTransform, "CPFeedbackText");
			return;
		}

		// Add xp
		DragonDataClassic dataClassic = data as DragonDataClassic;
		float amount = dataClassic.progression.GetXpRangeForLevel(dataClassic.progression.level).distance * 0.3f;
		dataClassic.progression.AddXp(amount, true);
		UIFeedbackText.CreateAndLaunch("+" + amount, new Vector2(0.5f, 0.5f), ControlPanel.panel.parent as RectTransform, "CPFeedbackText");

		// Refresh skin state for this dragon
		Wardrobe wardrobe = UsersManager.currentUser.wardrobe;
		List<DefinitionNode> skinDefs = DefinitionsManager.SharedInstance.GetDefinitionsByVariable(DefinitionsCategory.DISGUISES, "dragonSku", data.def.sku);
		for(int i = 0; i < skinDefs.Count; i++) {
			// Should the skin be unlocked?
			UsersManager.currentUser.wardrobe.ProcessUnlockedSkins(dataClassic);
			bool unlocked = (dataClassic.progression.level >= skinDefs[i].GetAsInt("unlockLevel"));

			// Apply new state based on lock status
			Wardrobe.SkinState oldState = wardrobe.GetSkinState(skinDefs[i].sku);
			Wardrobe.SkinState newState = Wardrobe.SkinState.LOCKED;
			if(unlocked) {
				// Depends on previous state
				switch(oldState) {
					case Wardrobe.SkinState.OWNED: newState = Wardrobe.SkinState.OWNED; break;
					case Wardrobe.SkinState.LOCKED: newState = Wardrobe.SkinState.NEW; break;
					case Wardrobe.SkinState.NEW: newState = Wardrobe.SkinState.NEW; break;
					case Wardrobe.SkinState.AVAILABLE: newState = Wardrobe.SkinState.AVAILABLE; break;
				}
			} else {
				newState = Wardrobe.SkinState.LOCKED;
			}

			// Apply new state
			wardrobe.SetSkinState(skinDefs[i].sku, newState);
		}

        // Save persistence
        PersistenceFacade.instance.Save_Request(false);

        // Simulate a dragon selected event so everything is refreshed
        Messenger.Broadcast<string>(MessengerEvents.MENU_DRAGON_SELECTED, selectedDragonSku);
	}

	/// <summary>
	/// Initialize XP slider with data from the currently selected dragon.
	/// </summary>
	public void OnInitXPSlider() {
		// Find linked slider
		Slider slider = GetComponentInChildren<Slider>();
		if(slider == null) Debug.Log("Requires a nested Slider!");

		// If in the menu and selected dragon is not owned, disable slider
		IDragonData targetDragon = null;
		if(InstanceManager.menuSceneController != null) {
			// Get current selected dragon data
			IDragonData data = DragonManager.GetDragonData(InstanceManager.menuSceneController.selectedDragon);
			if(data.isOwned) targetDragon = data;
		} else {
			// Not in the menu, use current dragon's data
			targetDragon = DragonManager.CurrentDragon;
		}

		// Disable slider if dragon cannot be changed
		bool validDragon = (targetDragon != null && targetDragon.type == IDragonData.Type.CLASSIC);
		slider.interactable = validDragon;

		// Initialize slider
		if(!validDragon) {
			slider.minValue = 0f;
			slider.maxValue = 1f;
			slider.value = 0f;
		} else {
			DragonDataClassic dataClassic = targetDragon as DragonDataClassic;
			slider.minValue = 0f;
			slider.maxValue = dataClassic.progression.GetXpRangeForLevel(dataClassic.progression.maxLevel).max;
			slider.value = dataClassic.progression.xp;
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
		IDragonData data = DragonManager.GetDragonData(selectedDragonSku);
		if(!data.isOwned) {
			UIFeedbackText.CreateAndLaunch("Only for owned dragons!", new Vector2(0.5f, 0.5f), ControlPanel.panel.parent as RectTransform, "CPFeedbackText");
			return;
		}

		if(data.type != IDragonData.Type.CLASSIC) {
			UIFeedbackText.CreateAndLaunch("Only for classic dragons!", new Vector2(0.5f, 0.5f), ControlPanel.panel.parent as RectTransform, "CPFeedbackText");
			return;
		}

		// Set xp
		DragonDataClassic dataClassic = data as DragonDataClassic;
		dataClassic.progression.SetXp_DEBUG(_xp);

		// Refresh skin state for this dragon
		Wardrobe wardrobe = UsersManager.currentUser.wardrobe;
		List<DefinitionNode> skinDefs = DefinitionsManager.SharedInstance.GetDefinitionsByVariable(DefinitionsCategory.DISGUISES, "dragonSku", data.def.sku);
		for(int i = 0; i < skinDefs.Count; i++) {
			// Should the skin be unlocked?
			UsersManager.currentUser.wardrobe.ProcessUnlockedSkins(data);
			bool unlocked = (dataClassic.progression.level >= skinDefs[i].GetAsInt("unlockLevel"));

			// Apply new state based on lock status
			Wardrobe.SkinState oldState = wardrobe.GetSkinState(skinDefs[i].sku);
			Wardrobe.SkinState newState = Wardrobe.SkinState.LOCKED;
			if(unlocked) {
				// Depends on previous state
				switch(oldState) {
					case Wardrobe.SkinState.OWNED: newState = Wardrobe.SkinState.OWNED; break;
					case Wardrobe.SkinState.LOCKED: newState = Wardrobe.SkinState.NEW; break;
					case Wardrobe.SkinState.NEW: newState = Wardrobe.SkinState.NEW; break;
					case Wardrobe.SkinState.AVAILABLE: newState = Wardrobe.SkinState.AVAILABLE; break;
				}
			} else {
				newState = Wardrobe.SkinState.LOCKED;
			}

			// Apply new state
			wardrobe.SetSkinState(skinDefs[i].sku, newState);
		}

        // Save persistence
        PersistenceFacade.instance.Save_Request(false);

        // Simulate a dragon selected event so everything is refreshed
        Messenger.Broadcast<string>(MessengerEvents.MENU_DRAGON_SELECTED, selectedDragonSku);
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
        PersistenceFacade.instance.Save_Request(false);
    }

	/// <summary>
	/// Unlock and buy all dragons.
	/// </summary>
    public void OnAcquireAllDragons() {
		List<IDragonData> dragons = DragonManager.GetDragonsByLockState(IDragonData.LockState.ANY);
        if (dragons != null) {
            int i;
            int count = dragons.Count;
            for (i = 0; i < count; i++) {
                if (!dragons[i].isOwned) {
                    dragons[i].Acquire();

					if (InstanceManager.menuSceneController is MenuSceneController) {
						MenuDragonSlot slot = InstanceManager.menuSceneController.dragonScroller.GetDragonSlot(dragons[i].def.sku);
						slot.animator.ForceShow(false);
					}
                }
            }

            // Save persistence
            PersistenceFacade.instance.Save_Request(false);
        }
    }

	/// <summary>
	/// Reset all the dragons.
	/// </summary>
	public void OnResetAllDragons() {
		List<IDragonData> dragons = DragonManager.GetDragonsByLockState(IDragonData.LockState.ANY);
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

			// Dragon baby is always owned
			DragonManager.GetDragonData("dragon_baby").Acquire();

            // Save persistence
            PersistenceFacade.instance.Save_Request(false);
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

        // Save persistence
        PersistenceFacade.instance.Save_Request(false);
    }

	/// <summary>
	/// Reset all pets to lock state.
	/// Resets golden egg fragments and collected eggs count as well.
	/// </summary>
	public void OnResetAllPets() {
		// Clear equipped pets
		foreach(KeyValuePair<string, IDragonData> kvp in DragonManager.dragonsBySku) {
			for(int i = 0; i < kvp.Value.pets.Count; i++) {
				UsersManager.currentUser.UnequipPet(kvp.Key, i);
			}
		}

		// Clear pet collection
		UsersManager.currentUser.petCollection.Reset();

		// Clear collected eggs and fragments
		UsersManager.currentUser.eggsCollected = 0;

        // Save!
        PersistenceFacade.instance.Save_Request(false);
    }

	/// <summary>
	/// Reset the pets collection to a random amount of locked/unlocked pets.
	/// Resets golden egg fragments and collected eggs count as well.
	/// </summary>
	public void OnResetPetsRandomly() {
		// Clear equipped pets
		foreach(KeyValuePair<string, IDragonData> kvp in DragonManager.dragonsBySku) {
			for(int i = 0; i < kvp.Value.pets.Count; i++) {
				UsersManager.currentUser.UnequipPet(kvp.Key, i);
			}
		}

		// Clear pet collection and eggs and fragments
		UsersManager.currentUser.petCollection.Reset();
		UsersManager.currentUser.eggsCollected = 0;

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
		}

        // Save!
        PersistenceFacade.instance.Save_Request(false);
    }

   	/// <summary>
	/// Reset all map upgrades
	/// </summary>
	public void OnResetMapUpgrades() {
		// Not much to do:
		UsersManager.currentUser.mapResetTimestamp = GameServerManager.SharedInstance.GetEstimatedServerTime(); // Already expired
        PersistenceFacade.instance.Save_Request(false);
    }

    /// <summary>
    /// Starts the like game flow.
    /// </summary>
    public void StartLikeGameFlow()
    {
    	if ( Application.platform == RuntimePlatform.Android ){
			PopupManager.OpenPopupInstant( PopupAskLikeGame.PATH );
		}else if ( Application.platform == RuntimePlatform.IPhonePlayer ){
			PopupManager.OpenPopupInstant(PopupAskRateUs.PATH);
		}else{
			PopupManager.OpenPopupInstant( PopupAskLikeGame.PATH );
		}
    }

    public void OnResetAchievements()
    {
    	ApplicationManager.instance.GameCenter_ResetAchievements();
    }

	public void PlayAd()
	{
		ApplicationManager.instance.Debug_TestPlayAd();
	}

    /// <summary>
    /// Finish the current happy hour
    /// </summary>
    public void OnEndHappyHour()
    {
        OffersManager.instance.happyHour.EndOffer();
    }
}