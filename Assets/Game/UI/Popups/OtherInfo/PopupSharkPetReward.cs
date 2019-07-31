// PopupSharkPetReward.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 26/07/2018.
// Copyright (c) 2018 Ubisoft. All rights reserved.

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
/// Controller for the Pre-registration rewards popup.
/// </summary>
[RequireComponent(typeof(PopupController))]
public class PopupSharkPetReward : PopupInfoPet {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	new public const string PATH = "UI/Popups/OtherInfo/PF_PopupSharkPetReward";
	public const string PET_SKU = "pet_68";

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Internal
	private bool m_collected = false;
	
	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {
		// Initialize Pet Info Popup
		DefinitionNode petDef = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.PETS, PET_SKU);
		Init(petDef);
	}

	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// The popup is about to open.
	/// </summary>
	override public void OnOpenPreAnimation() {
		// Call parent
		base.OnOpenPreAnimation();

		// Hide all menu UI
		InstanceManager.menuSceneController.GetUICanvasGO().SetActive(false);
	}

	/// <summary>
	/// The popup is about to close.
	/// </summary>
	public void OnClosePreAnimation() {
		// Call parent
		base.OnClosePostAnimation();

		// Restore menu HUD
		InstanceManager.menuSceneController.GetUICanvasGO().SetActive(true);
	}

	/// <summary>
	/// The collect button has been pressed. Start the collect flow.
	/// </summary>
	public void OnCollectButton() {
		// Make sure it doesn't happen twice!
		if(m_collected) return;
		m_collected = true;

		// Unlock pet
		UsersManager.currentUser.petCollection.UnlockPet(PET_SKU);

		// Close all open popups
		PopupManager.Clear(true);

		// Make sure selected dragon is owned
		InstanceManager.menuSceneController.SetSelectedDragon(DragonManager.CurrentDragon.def.sku);  // Current dragon is the last owned selected dragon

		// Move to the pets screen, focusing on the rewarded pet
		// Add a frame of delay to make sure everyone has been notified that the selected dragon has changed
		MenuTransitionManager screensController = InstanceManager.menuSceneController.transitionManager;
		UbiBCN.CoroutineManager.DelayedCallByFrames(() => {
			MenuScreen targetPetScreen = InstanceManager.menuSceneController.GetPetScreenForCurrentMode();    // [AOC] Different pet screen if the current dragon is a special one
			PetsScreenController petScreen = screensController.GetScreenData(targetPetScreen).ui.GetComponent<PetsScreenController>();
			petScreen.Initialize(PET_SKU);
			screensController.GoToScreen(targetPetScreen, true);
		}, 1);
	}
}