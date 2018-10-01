// LabDragonSelectionScreen.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 27/09/2018.
// Copyright (c) 2018 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Screen controller for the Lab Dragon Selection screen.
/// </summary>
public class LabDragonSelectionScreen : MonoBehaviour {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed references
	[SerializeField] private Localizer m_dragonNameText = null;
	[SerializeField] private Localizer m_dragonDescText = null;
	
	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {

	}

	/// <summary>
	/// First update call.
	/// </summary>
	private void Start() {

	}

	/// <summary>
	/// Component has been enabled.
	/// </summary>
	private void OnEnable() {
		// Subscribe to external events
		Messenger.AddListener<string>(MessengerEvents.MENU_DRAGON_SELECTED, OnDragonSelected);

		// Do a first refresh
		RefreshDragonInfo(InstanceManager.menuSceneController.selectedDragonData);
	}

	/// <summary>
	/// Component has been disabled.
	/// </summary>
	private void OnDisable() {
		// Unsubscribe from external events
		Messenger.RemoveListener<string>(MessengerEvents.MENU_DRAGON_SELECTED, OnDragonSelected);
	}

	/// <summary>
	/// Called every frame.
	/// </summary>
	private void Update() {

	}

	/// <summary>
	/// Destructor.
	/// </summary>
	private void OnDestroy() {

	}

	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Refresh with data from a target dragon.
	/// </summary>
	/// <param name="_dragonData">Data to be used to initialize the dragon info.</param>
	private void RefreshDragonInfo(IDragonData _dragonData) {
		// Skip if dragon data is not valid
		if(_dragonData == null) return;

		// Dragon name
		if(m_dragonNameText != null) {
			//m_dragonNameText.Localize(_dragonData.def.GetAsString("tidName"));
			// [AOC] TODO!! TIDs not already defined, use sku for now
			m_dragonNameText.Localize(_dragonData.sku);
		}

		// Dragon desc
		if(m_dragonDescText != null) {
			m_dragonDescText.Localize(_dragonData.def.GetAsString("tidDesc"));
		}
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Back button has been pressed.
	/// </summary>
    public void OnBackButton() {
		SceneController.SetMode(SceneController.Mode.DEFAULT);
    }

	/// <summary>
	/// A new dragon has been selected.
	/// </summary>
	/// <param name="_sku">The sku of the selected dragon.</param>
	private void OnDragonSelected(string _sku) {
		// Get new dragon's data from the dragon manager and do the refresh logic
		RefreshDragonInfo(DragonManager.GetDragonData(_sku));
	}
}