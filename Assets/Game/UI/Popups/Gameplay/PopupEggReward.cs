// PopupEggReward.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 12/01/2016.
// Copyright (c) 2016 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;
using System;
using UnityEngine.UI;
using DG.Tweening;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// Egg reward popup at the end of a game.
/// For now we will give it quite a lot of control over the logic, but maybe 
/// some stuff should be done elsewhere.
/// </summary>
[RequireComponent(typeof(PopupController))]
public class PopupEggReward : MonoBehaviour {
	//------------------------------------------------------------------//
	// CONSTANTS														//
	//------------------------------------------------------------------//
	public static readonly string PATH = "UI/Popups/Collectibles/PF_PopupEggReward";

	//------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES											//
	//------------------------------------------------------------------//
	// Internal references
	private EggUIScene3D m_eggScene3D = null;

	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Initialization
	/// </summary>
	private void Awake() {
		// Create a new dummy egg and initialize the preview
		Egg newEgg = Egg.CreateFromSku(Egg.SKU_STANDARD_EGG);
		m_eggScene3D = EggUIScene3D.CreateFromEgg(newEgg);
		RawImage img = this.GetComponentInChildren<RawImage>();
		m_eggScene3D.InitRawImage(ref img);
	}
	
	/// <summary>
	/// Default destructor.
	/// </summary>
	private void OnDestroy() {
		// Destroy egg 3D scene
		if(m_eggScene3D != null) {
			UIScene3DManager.Remove(m_eggScene3D);
			m_eggScene3D = null;
		}
	}

	//------------------------------------------------------------------//
	// CALLBACKS														//
	//------------------------------------------------------------------//
	/// <summary>
	/// Ok button pressed.
	/// </summary>
	public void OnOkButton() {
		// This popup shouldn't be opened if we didn't found an egg, but just in case
		if(EggManager.collectibleEgg != null && EggManager.collectibleEgg.collected) {
			// Add the egg to the inventory
			EggManager.AddEggToInventory(Egg.CreateFromSku(Egg.SKU_STANDARD_EGG));
			PersistenceManager.Save();
		
			// Consume the collected egg
			EggManager.ClearCollectibleEgg();

			// Go straight to incubator when going back to the menu
			GameVars.menuInitialScreen = MenuScreens.INCUBATOR;
		}

		// Close this popup, the results screen controller will know what to do next
		GetComponent<PopupController>().Close(true);
	}
}
