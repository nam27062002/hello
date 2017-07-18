// GlobalEventsPanel.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 28/06/2017.
// Copyright (c) 2017 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;
using TMPro;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Panel corresponding to a specific event state.
/// </summary>
public class GlobalEventsPanel : MonoBehaviour {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	
	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	public GlobalEventsScreenController.Panel panelId {
		get; set;
	}

	//------------------------------------------------------------------------//
	// OVERRIDE CANDIDATES													  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Refresh displayed data.
	/// </summary>
	public virtual void Refresh() {
		// To be implemented by heirs, if needed
	}
}