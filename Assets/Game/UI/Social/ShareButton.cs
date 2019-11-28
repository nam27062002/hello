// ShareButton.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 26/11/2019.
// Copyright (c) 2019 Ubisoft. All rights reserved.

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
public class ShareButton : MonoBehaviour {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//

	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// First update call.
	/// </summary>
	public void Start() {
		// Disable ourselves if we can't be displayed
		if(!CanBeDisplayed()) this.gameObject.SetActive(false);
	}

	/// <summary>
	/// Check whether the share button can be displayed or not.
	/// </summary>
	/// <returns></returns>
	public static bool CanBeDisplayed() {
		return UsersManager.currentUser.gamesPlayed >= GameSettings.ENABLE_SHARE_BUTTONS_AT_RUN;
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
}