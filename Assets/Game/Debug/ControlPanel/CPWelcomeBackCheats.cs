// CPMissionsCheats.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 17/01/2018.
// Copyright (c) 2018 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Allow several operations related to the mission system from the Control Panel.
/// </summary>
public class CPWelcomeBackCheats : MonoBehaviour {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed
	
	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {
		
	}

	/// <summary>
	/// Component has been enabled.
	/// </summary>
	private void OnEnable() {

	}

	/// <summary>
	/// Component has been disabled.
	/// </summary>
	private void OnDisable() {

	}

	/// <summary>
	/// Update loop.
	/// </summary>
	private void Update() {
		// Update timer text

	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// The activate Welcome back button has been pressed
	/// </summary>
	public void OnActivateWelcomeBack() {
		
		// Activate the WB feature
		WelcomeBackManager.instance.OnForceStart();
		
		// Save persistence
		PersistenceFacade.instance.Save_Request(false);
	}
	
	/// <summary>
	/// The activate Welcome back button has been pressed
	/// </summary>
	public void OnEndWelcomeBack() {
		
		// Activate the WB feature
		WelcomeBackManager.instance.OnForceEnd();
		
		// Save persistence
		PersistenceFacade.instance.Save_Request(false);
	}


}