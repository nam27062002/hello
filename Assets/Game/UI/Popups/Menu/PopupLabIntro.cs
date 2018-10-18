// PopupLabIntro.cs
// 
// Created by Alger Ortín Castellví on 25/04/2017.
// Copyright (c) 2017 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Tiers info popup.
/// </summary>
[RequireComponent(typeof(PopupController))]
public class PopupLabIntro : MonoBehaviour {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	public const string PATH = "UI/Popups/Menu/PF_PopupLabIntro";
	public const string DISPLAYED_KEY = "PopupLabIntro.DISPLAYED";

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// The popup is about to be closed.
	/// </summary>
	public void OnClosePreAnimation() {
		// Mark as displayed!
		Prefs.SetBoolPlayer(DISPLAYED_KEY, true);
	}
}
