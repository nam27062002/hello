﻿// MenuDragonPaginator.cs
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
public class MenuDragonPaginator : TabSystem {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	
	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed setup
	[SerializeField] private Tab m_dummyTab = null;
	
	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {
		// Check required fields
		Debug.Assert(m_dummyTab != null, "Required field!");

		// Initialize buttons - assume they are in tier order
        for(int i = 0; i < m_tabButtons.Count; ++i) {
			// Add callback
			if(i == (int)DragonTier.TIER_6) {
				// Special tier
				m_tabButtons[i].button.onClick.AddListener(OnSpecialDragonsClick);
			} else {
				// Classic dragons tiers (XS, S, M, L, XL, XXL)
				DragonTier tier = (DragonTier)i;    // Can't use "i" directly with a lambda expression http://stackoverflow.com/questions/3168375/using-the-iterator-variable-of-foreach-loop-in-a-lambda-expression-why-fails
				m_tabButtons[i].button.onClick.AddListener(
					() => { OnTierButtonClick(tier); }  // Way to add a listener with parameters (basically call a delegate function without parameters which in turn will call our actual callback with the desired parameter)
				);
			}

			// Assign dummy screen
			m_screens.Add(m_dummyTab);
		}
    }

	/// <summary>
	/// First update call.
	/// </summary>
	override protected void Start() {
		// Let parent do its magic
		base.Start();

		// Select initial tab
		Initialize();
	}

	/// <summary>
	/// Component has been enabled.
	/// </summary>
	private void OnEnable() {
		// Subscribe to external events
		Messenger.AddListener<IDragonData>(MessengerEvents.DRAGON_TEASED, OnDragonTeased);
		Messenger.AddListener<string>(MessengerEvents.MENU_DRAGON_SELECTED, OnDragonSelected);

		// Make sure selected tab is the right one
		Initialize();
	}

	/// <summary>
	/// Component has been disabled.
	/// </summary>
	private void OnDisable() {
		// Unsubscribe from external events
		Messenger.RemoveListener<IDragonData>(MessengerEvents.DRAGON_TEASED, OnDragonTeased);
		Messenger.RemoveListener<string>(MessengerEvents.MENU_DRAGON_SELECTED, OnDragonSelected);
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
			m_tabButtons[i].SetSelected(false, false);
		}

        // Clear selected tab to make sure everything is properly initialized
        GoToScreen(SCREEN_NONE, NavigationScreen.AnimType.NONE);

		// Find out and select initial tab
		// Luckily, tier indexes match the order of the buttons, so we can do this really fast
		string selectedSku = InstanceManager.menuSceneController.selectedDragon;
		IDragonData selectedDragon = DragonManager.GetDragonData(selectedSku);
		if(selectedDragon != null) {
			GoToScreen((int)selectedDragon.tier, NavigationScreen.AnimType.NONE);
		}
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// The selected dragon has changed.
	/// </summary>
	/// <param name="_id">The id of the selected dragon.</param>
	public void OnDragonSelected(string _sku) {

        int screenIndex = -1;

        if (DragonManager.GetDragonData(_sku).type == IDragonData.Type.CLASSIC)
        {
            // Luckily, in classic dragons, tier indexes match the order of the buttons, so we can do this really fast
            screenIndex = (int) DragonManager.GetDragonData(_sku).tier;
        }
        else
        {
            // Special dragons. Select the last tab.
            screenIndex = m_tabButtons.Count - 1;
        }

        GoToScreen(screenIndex, NavigationScreen.AnimType.NONE);
	}

	private void OnDragonTeased(IDragonData _data) {
		m_tabButtons[(int)_data.tier].GetComponent<NavigationShowHideAnimator>().Show(true);
	}

	/// <summary>
	/// A tier button has been pressed.
	/// </summary>
	/// <param name="_tierDef">The tier that has been clicked.</param>
	public void OnTierButtonClick(DragonTier _tier) {
		// Select first dragon of the target tier
		List<IDragonData> dragons = DragonManager.GetDragonsByOrder(IDragonData.Type.CLASSIC);
		for(int i = 0; i < dragons.Count; i++) {
			// Does this dragon belong to the target tier?
			if(dragons[i].tier == _tier) {
				// Yes!! Select it and return
				InstanceManager.menuSceneController.SetSelectedDragon(dragons[i].sku);

				// Play audio corresponding to this tier
				AudioController.Play(UIConstants.GetDragonTierSFX(_tier));
				return;
			}
		}
	}

    /// <summary>
    /// Special dragons button pressed. Select the first special dragon.
    /// </summary>
    public void OnSpecialDragonsClick()
    {
        // Select first dragon of the lab
        List<IDragonData> dragons = DragonManager.GetDragonsByOrder(IDragonData.Type.SPECIAL);
        InstanceManager.menuSceneController.SetSelectedDragon(dragons[0].sku);

        // Play audio corresponding to this tier
        //AudioController.Play(UIConstants.GetDragonTierSFX(_tier));
        return;
    }
}