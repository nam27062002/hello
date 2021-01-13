// PopupHalloweenPetInfo.cs
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
public class PopupHalloweenPetInfo : PopupInfoPet {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	public const string PATH = "UI/Popups/OtherInfo/PF_PopupHalloweenPetInfo";
	public const string PET_SKU = "pet_69";

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//

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
}