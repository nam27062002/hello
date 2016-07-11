// EquipmentScreenController.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 08/07/2016.
// Copyright (c) 2016 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// 
/// </summary>
public class EquipmentScreenController : MonoBehaviour {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	private enum Subscreen {
		DISGUISES,
		PETS
	};
	
	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed references
	[SerializeField] private DisguisesScreenController m_disguisesScreen = null;
	[SerializeField] private PetsScreenController m_petsScreen = null;

	// Internal
	private Subscreen m_lastActiveScreen = Subscreen.DISGUISES;
	private bool m_started = false;
	
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
		m_started = true;
	}

	/// <summary>
	/// Component has been enabled.
	/// </summary>
	private void OnEnable() {
		// Skip the very first enable call, the screen will immediately be disabled again.
		if(!m_started) return;

		// Restore last active screen and hide the rest
		switch(m_lastActiveScreen) {
			case Subscreen.DISGUISES:	ShowDisguises();	break;
			case Subscreen.PETS: 		ShowPets();			break;
		}
	}

	/// <summary>
	/// Component has been disabled.
	/// </summary>
	private void OnDisable() {

	}

	/// <summary>
	/// Called every frame
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
	/// Shows the disguises screen and hides the pets.
	/// </summary>
	public void ShowDisguises() {
		m_disguisesScreen.Show();
		m_petsScreen.Hide();
		m_lastActiveScreen = Subscreen.DISGUISES;
	}

	/// <summary>
	/// Shows the pets screen and hides the disguises.
	/// </summary>
	public void ShowPets() {
		m_disguisesScreen.Hide();
		m_petsScreen.Show();
		m_lastActiveScreen = Subscreen.PETS;
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
}