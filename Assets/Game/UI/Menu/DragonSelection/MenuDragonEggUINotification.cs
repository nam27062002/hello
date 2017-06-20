// MenuDragonEggUINotification.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 10/03/2016.
// Copyright (c) 2016 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using DG.Tweening;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Control a UI notification for the dragon selection incubator button.
/// </summary>
public class MenuDragonEggUINotification : MonoBehaviour {
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
	private void OnEnable() 
	{
		// Refresh each time the component is enabled
		// [AOC] MiniHack! Add some delay to give time for the isNew flag to be set
		m_notification.Set(false);
		UbiBCN.CoroutineManager.DelayedCall(Refresh, 0.25f, false);
	}

	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Check whether to show the notification or not.
	/// </summary>
	public void Refresh() {
		// Notification visible if any of the eggs in the inventory are marked as new
		if(m_notification != null) 
		{
			bool newEggs = false;
			for(int i = 0; i < EggManager.INVENTORY_SIZE; i++) {
				// Does this slot have an egg, and is it new?
				if(EggManager.inventory != null && EggManager.inventory[i] != null && EggManager.inventory[i].isNew) {
					// Break loop!
					newEggs = true;
					break;
				}
			}

			// Set notification's visibility
			m_notification.Set(newEggs);
		}
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
}