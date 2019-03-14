// IAssetsDownloadFlowUpdatablePopup.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 13/03/2019.
// Copyright (c) 2019 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Auxiliar popup to the Assets Download Flow.
/// </summary>
public class AssetsDownloadFlowUpdatablePopup : AssetsDownloadFlowPopup {
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
	/// First update.
	/// </summary>
	private void Start() {
		// Program periodic update
		InvokeRepeating("PeriodicUpdate", 0f, AssetsDownloadFlowSettings.updateInterval);
	}

	/// <summary>
	/// Update at regular intervals.
	/// </summary>
	private void PeriodicUpdate() {
		// Refresh popup's content
		Refresh();
	}
}