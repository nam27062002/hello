// MenuDragonPetUINotification.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 09/11/2017.
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
/// Control a UI notification for the dragon selection pets button.
/// </summary>
public class MenuDragonPetUINotification : MonoBehaviour {
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
		// [AOC] MiniHack! Add some delay to give time for any external conditions to be set
		m_notification.Set(false, false);
		UbiBCN.CoroutineManager.DelayedCall(Refresh, 0.25f, false);

		// Subscribe to external events
		Messenger.AddListener<string>(MessengerEvents.MENU_DRAGON_SELECTED, OnDragonSelected);
	}

	/// <summary>
	/// Component disabled.
	/// </summary>
	private void OnDisable() {
		// Unsubscribe from external events
		Messenger.RemoveListener<string>(MessengerEvents.MENU_DRAGON_SELECTED, OnDragonSelected);
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

		// Don't show during the FTUXP
		if(!UsersManager.currentUser.IsTutorialStepCompleted(TutorialStep.SECOND_RUN)) {
			m_notification.Hide(true, true);
			return;
		}

		// Notification visible if selected dragon is owned and has empty slots (provided we have enough pets unlocked)
		bool show = true;

		// a) Is dragon owned?
		IDragonData selectedDragonData = DragonManager.GetDragonData(InstanceManager.menuSceneController.selectedDragon);
		show &= selectedDragonData.isOwned;

		// b) Has any empty slot?
		int equippedCount = 0;
		if(show) {	// No need to check if first condition already failed
			for(int i = 0; i < selectedDragonData.pets.Count; ++i) {
				if(!string.IsNullOrEmpty(selectedDragonData.pets[i])) {
					equippedCount++;
				}
			}
			show &= (equippedCount != selectedDragonData.pets.Count);	// Don't show if all slots are equipped
		}

		// c) Does player have enough pets unlocked to fill the empty slots
		if(show) {	// No need to check if first condition already failed
			show &= (UsersManager.currentUser.petCollection.unlockedPetsCount > equippedCount);
		}

		// Set notification's visibility!
		m_notification.Set(show);
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