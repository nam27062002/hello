// MenuDragonLevelBar.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 18/11/2015.
// Copyright (c) 2015 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Simple controller for a dragon level bar in the menu.
/// </summary>
public class MenuDragonLevelBar : DragonXPBar {
	//------------------------------------------------------------------------//
	// PROPERTIES															  //
	//------------------------------------------------------------------------//

	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	override protected void Awake() {
		base.Awake();
	}

	/// <summary>
	/// First update
	/// </summary>
	protected void OnEnable() {
		// Subscribe to external events
		Messenger.AddListener<string>(GameEvents.MENU_DRAGON_SELECTED, OnDragonSelected);
		
		// Do a first refresh
		Refresh(InstanceManager.menuSceneController.selectedDragon);
	}

	/// <summary>
	/// Destructor
	/// </summary>
	protected void OnDisable() {
		// Unsubscribe from external events
		Messenger.RemoveListener<string>(GameEvents.MENU_DRAGON_SELECTED, OnDragonSelected);
	}

	/// <summary>
	/// A new dragon has been selected.
	/// </summary>
	/// <param name="_sku">The sku of the selected dragon.</param>
	private void OnDragonSelected(string _sku) {
		// Refresh after some delay to let the animation finish
		Refresh(_sku, 0.25f);
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Info button has been pressed.
	/// </summary>
	public void OnInfoButton() {
		// Open the dragon info popup and initialize it with the current dragon's data
		PopupDragonInfo popup = PopupManager.OpenPopupInstant(PopupDragonInfo.PATH).GetComponent<PopupDragonInfo>();
		popup.Init(m_dragonData);
	}
}
