// MenuDragonPaginator.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 27/04/2016.
// Copyright (c) 2016 Ubisoft. All rights reserved.

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
/// Quick'n'dirty page marker to follow the dragon scrolling.
/// We'll reuse the TabSystem, even if it's a bit of an overkill.
/// </summary>
[RequireComponent(typeof(HorizontalOrVerticalLayoutGroup))]
public class MenuDragonPaginator : TabSystem {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	
	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed setup
	[SerializeField] private GameObject m_buttonPrefab = null;
	[SerializeField] private Tab m_dummyTab = null;
	
	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {
		// Check required fields
		Debug.Assert(m_buttonPrefab != null, "Required field!");
		Debug.Assert(m_dummyTab != null, "Required field!");

		// Create a button for each dragon
		// Re-use the loop to initialize selected dragon
		for(int i = 0; i < DragonManager.dragonsByOrder.Count; i++) {
			// Create a new instance of the prefab as a child of this object
			// Will be auto-positioned by the Layout component
			GameObject newInstanceObj = GameObject.Instantiate<GameObject>(m_buttonPrefab);
			newInstanceObj.transform.SetParent(this.transform, false);

			// Save button as one of the tab buttons and add a dummy associated tab
			m_tabButtons.Add(newInstanceObj.GetComponent<Button>());
			m_screens.Add(m_dummyTab);
		}
	}

	/// <summary>
	/// First update call.
	/// </summary>
	override protected void Start() {
		// Let parent do its magic
		base.Start();

		// Remove listeners from buttons since in this particular case we don't want to be able to navigate through them
		for(int i = 0; i < m_tabButtons.Count; i++) {
			m_tabButtons[i].onClick.RemoveAllListeners();
		}

		// Select initial tab
		Initialize();
	}

	/// <summary>
	/// Component has been enabled.
	/// </summary>
	private void OnEnable() {
		// Subscribe to external events
		Messenger.AddListener<string>(GameEvents.MENU_DRAGON_SELECTED, OnDragonSelected);

		// Make sure selected tab is the right one
		Initialize();
	}

	/// <summary>
	/// Component has been disabled.
	/// </summary>
	private void OnDisable() {
		// Unsubscribe to external events
		Messenger.RemoveListener<string>(GameEvents.MENU_DRAGON_SELECTED, OnDragonSelected);
	}

	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Internal initialization, to be called every time the dragon selection screen is opened.
	/// Because the OnEnable() is called before the Start(), and the NavigationScreenSystem
	/// hides all screens at Start(), we can't do this during the OnEnable() event (it would
	/// be overriden by the NavigationScreenSystem's Start()). We can't do it either at the Start(),
	/// since we want this to happen every time the dragon selection screen is opened.
	/// </summary>
	private void Initialize() {
		// Reset all buttons
		for(int i = 0; i < m_tabButtons.Count; i++) {
			m_tabButtons[i].interactable = true;
		}

		// Clear selected tab to make sure everything is properly initialized
		GoToScreen(SCREEN_NONE, NavigationScreen.AnimType.NONE);

		// Find out and select initial tab
		int selectedIdx = 0;
		string selectedSku = InstanceManager.GetSceneController<MenuSceneController>().selectedDragon;
		for(int i = 0; i < DragonManager.dragonsByOrder.Count; i++) {
			if(DragonManager.dragonsByOrder[i].def.sku == selectedSku) {
				selectedIdx = i;
				break;
			}
		}
		GoToScreen(selectedIdx, NavigationScreen.AnimType.NONE);
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// The selected dragon has changed.
	/// </summary>
	/// <param name="_id">The id of the selected dragon.</param>
	public void OnDragonSelected(string _sku) {
		// Select the matching tab
		for(int i = 0; i < DragonManager.dragonsByOrder.Count; i++) {
			if(_sku == DragonManager.dragonsByOrder[i].def.sku) {
				GoToScreen(i, NavigationScreen.AnimType.NONE);
				break;
			}
		}
	}
}