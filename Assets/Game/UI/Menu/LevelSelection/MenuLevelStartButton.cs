// MenuLevelShowConditionally.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 26/04/2016.
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
/// Simple controller for the start game button.
/// </summary>
[RequireComponent(typeof(Button))]
public class MenuLevelStartButton : MonoBehaviour {
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
	/// Component enabled.
	/// </summary>
	private void OnEnable() {
		// Subscribe to external events
		Messenger.AddListener<string>(GameEvents.MENU_LEVEL_SELECTED, OnLevelSelected);

		// Apply for the first time with currently selected level
		Apply(InstanceManager.GetSceneController<MenuSceneController>().selectedLevel);
	}

	/// <summary>
	/// Component disabled.
	/// </summary>
	private void OnDestroy() {
		// Unsubscribe from external events
		Messenger.RemoveListener<string>(GameEvents.MENU_LEVEL_SELECTED, OnLevelSelected);
	}

	//------------------------------------------------------------------//
	// INTERNAL METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Apply visibility based on given parameters.
	/// </summary>
	/// <param name="_sku">The sku of the dragon to be considered.</param>
	private void Apply(string _sku) {
		GetComponent<Button>().interactable = LevelManager.IsLevelUnlocked(_sku);
	}

	//------------------------------------------------------------------//
	// CALLBACKS														//
	//------------------------------------------------------------------//
	/// <summary>
	/// The selected dragon on the menu has changed.
	/// </summary>
	/// <param name="_sku">The sku of the newly selected dragon.</param>
	public void OnLevelSelected(string _sku) {
		// Just update visibility
		Apply(_sku);
	}
}

