// MenuDragonSceneController.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 11/05/2016.
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
/// Main controller of the dragon selection screen.
/// </summary>
public class MenuDragonScreenController : MonoBehaviour {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//

	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Component has been enabled.
	/// </summary>
	private void OnEnable() {
		// Subscribe to external events.
		Messenger.AddListener<NavigationScreenSystem.ScreenChangedEventData>(EngineEvents.NAVIGATION_SCREEN_CHANGED, OnNavigationScreenChanged);
	}

	/// <summary>
	/// Raises the disable event.
	/// </summary>
	private void OnDisable() {
		// Unsubscribe to external events.
		Messenger.RemoveListener<NavigationScreenSystem.ScreenChangedEventData>(EngineEvents.NAVIGATION_SCREEN_CHANGED, OnNavigationScreenChanged);
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Navigation screen has changed (animation starts now).
	/// </summary>
	/// <param name="_event">Event data.</param>
	private void OnNavigationScreenChanged(NavigationScreenSystem.ScreenChangedEventData _event) {
		// Only if it comes from the main screen navigation system
		if(_event.dispatcher != InstanceManager.menuSceneController.screensController) return;

		// If leaving this screen
		if(_event.fromScreenIdx == (int)MenuScreens.DRAGON_SELECTION) {
			// Remove "new" flag from incubating eggs
			for(int i = 0; i < EggManager.INVENTORY_SIZE; i++) {
				if(EggManager.inventory[i] != null) {
					EggManager.inventory[i].isNew = false;
				}
			}

			// Save persistence to store current dragon
			PersistenceManager.Save(true);
		}
	}
}