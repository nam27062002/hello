// CPVersion.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 26/11/2015.
// Copyright (c) 2015 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;
using TMPro;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// Simple widget to display number version.
/// </summary>
[RequireComponent(typeof(TextMeshProUGUI))]
public class CPVersion : MonoBehaviour {
	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// First update call.
	/// </summary>
	private void Start() {
		// Just initialize text
		TextMeshProUGUI versionText = GetComponent<TextMeshProUGUI>();
		versionText.text = "Version " + GameSettings.internalVersion;
	}
}