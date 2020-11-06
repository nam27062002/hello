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

    [SerializeField] private TextMeshProUGUI m_playerType;
	[SerializeField] private TextMeshProUGUI m_lastLoginTimestamp;

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
        m_playerType.text = "Player Type: " + WelcomeBackManager.instance.playerType.ToString();
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
		m_lastLoginTimestamp.text = "Last Login: " + UsersManager.currentUser.saveTimestamp.ToString();

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
        m_playerType.text = "Player Type: " + WelcomeBackManager.instance.playerType.ToString();
    }
	
	/// <summary>
	/// The activate Welcome back button has been pressed
	/// </summary>
	public void OnEndWelcomeBack() {
		
		// Activate the WB feature
		WelcomeBackManager.instance.OnForceEnd();
		
		// Save persistence
		PersistenceFacade.instance.Save_Request(false);
        
        // Update cheats panel info
        m_playerType.text = "Player Type: " + WelcomeBackManager.instance.playerType.ToString();
        
	}

    /// <summary>
    /// Perform all the checks needed, and if this player is elegible, activate WB.
    /// </summary>
    public void OnTryActivation()
    {
		WelcomeBackManager.instance.CheckActivation();
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