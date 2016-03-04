// IncubatorCheatButton.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 01/03/2016.
// Copyright (c) 2016 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// TEMP!! Cheat button while the UI is under development.
/// </summary>
public class IncubatorCheatButton : MonoBehaviour {
	//------------------------------------------------------------------//
	// CONSTANTS														//
	//------------------------------------------------------------------//

	//------------------------------------------------------------------//
	// MEMBERS															//
	//------------------------------------------------------------------//

	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// First update call.
	/// </summary>
	private void Start() {
		
	}

	//------------------------------------------------------------------//
	// CALLBACKS														//
	//------------------------------------------------------------------//
	/// <summary>
	/// Add egg to inventory cheat.
	/// </summary>
	public void OnAddEgg() {
		// Pick a random egg from the definitions set
		string newEggSku = Definitions.GetDefinitions(Definitions.Category.EGGS).GetRandomValue().sku;

		// Add it to the inventory
		int slotIdx = EggManager.AddEggToInventory(Egg.CreateBySku(newEggSku));

		// If successful, save persistence
		if(slotIdx >= 0) PersistenceManager.Save();
	}
}

