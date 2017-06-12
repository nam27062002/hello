// MenuDragonSkinUINotification.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 02/05/2017.
// Copyright (c) 2016 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using System.Collections.Generic;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Control a UI notification for the dragon selection skins button.
/// </summary>
public class MenuDragonSkinUINotification : MonoBehaviour {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	
	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// References
	[SerializeField] private UINotification m_notification = null;
	
	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// First update call.
	/// </summary>
	private void Start() {
		// Start with hidden notification
		m_notification.Hide(false);

		// Refresh
		Refresh();
	}

	/// <summary>
	/// Component enabled.
	/// </summary>
	private void OnEnable() {
		// Refresh each time the component is enabled
		// [AOC] MiniHack! Add some delay to give time for the isNew flag to be set
		m_notification.Set(false, false);
		CoroutineManager.DelayedCall(Refresh, 0.25f, false);

		// Subscribe to external events
		Messenger.AddListener<string>(GameEvents.MENU_DRAGON_SELECTED, OnDragonSelected);
	}

	/// <summary>
	/// Component disabled.
	/// </summary>
	private void OnDisable() {
		// Unsubscribe from external events
		Messenger.RemoveListener<string>(GameEvents.MENU_DRAGON_SELECTED, OnDragonSelected);
	}

	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Check whether to show the notification or not.
	/// </summary>
	public void Refresh() {
		// Skip if not properly initialized
		if(m_notification == null) return;

		// Notification visible if any of the skins of the currently selected dragon are marked as 'new'
		DragonData selectedDragonData = DragonManager.GetDragonData(InstanceManager.menuSceneController.selectedDragon);

		// Get all the disguises of the selected dragon
		List<DefinitionNode> defList = DefinitionsManager.SharedInstance.GetDefinitionsByVariable(DefinitionsCategory.DISGUISES, "dragonSku", selectedDragonData.def.sku);

		// Notification visible if any of the skins of the selected dragon are in the "NEW" state
		bool newSkins = false;
		for(int i = 0; i < defList.Count; i++) {
			// Is it a new skin?
			if(UsersManager.currentUser.wardrobe.GetSkinState(defList[i].sku) == Wardrobe.SkinState.NEW) {
				// Break loop!
				newSkins = true;
				break;
			}
		}

		// Set notification's visibility
		m_notification.Set(newSkins);
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// The selected dragon on the menu has changed.
	/// </summary>
	/// <param name="_dragonSku">Sku of the newly selected dragon.</param>
	private void OnDragonSelected(string _dragonSku) {
		Refresh();
	}
}