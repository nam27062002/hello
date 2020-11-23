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
using System;

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

    [SerializeField] private TextMeshProUGUI m_playerType;
	[SerializeField] private TextMeshProUGUI m_lastActivationTime;

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

		// Update cheats panel info
		Refresh();

  
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
		DateTime time = WelcomeBackManager.instance.lastActivationTime;
		bool isValid = time != null && time != DateTime.MinValue;

		// Update timer text
		m_lastActivationTime.text = "Last Activation Time: " + (isValid ? time.ToString() : "-");

	}

    private void Refresh()
    {
		if (WelcomeBackManager.instance.perksDef != null)
		{
			// There is not such thing as player group name, but use SKU just for info purposes
			m_playerType.text = "Player Group SKU: " + WelcomeBackManager.instance.perksDef.GetAsString("sku");
		}
		else
		{
			m_playerType.text = "Player Group SKU: - ";
		}
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

		// Update cheats panel info
		Refresh();

	}
	
	/// <summary>
	/// The activate Welcome back button has been pressed
	/// </summary>
	public void OnEndWelcomeBack() {
		
		// Disable the WB feature
		WelcomeBackManager.instance.OnForceEnd();
		
		// Save persistence
		PersistenceFacade.instance.Save_Request(false);

		// Update cheats panel info
		Refresh();

	}



    /// <summary>
    /// Removes one day to the last save timestamp
    /// </summary>
    public void OnSubstractDay()
    {
		UsersManager.currentUser.saveTimestamp = UsersManager.currentUser.saveTimestamp.AddDays(-1);

	}


    /// <summary>
    /// Adds one day to the last save timestamp
    /// </summary>
    public void OnAddDay ()
    {
		UsersManager.currentUser.saveTimestamp = UsersManager.currentUser.saveTimestamp.AddDays(1);
	}

}