﻿// HUDCoinsTextController.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 15/09/2015.
// Copyright (c) 2015 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// Simple controller to update a textfield with the current amount of pc of the player.
/// </summary>
[RequireComponent(typeof(Text))]
public class HUDPCTextController : MonoBehaviour {
	//------------------------------------------------------------------//
	// CONSTANTS														//
	//------------------------------------------------------------------//

	//------------------------------------------------------------------//
	// MEMBERS															//
	//------------------------------------------------------------------//
	// References
	private Text m_text = null;

	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {
		// Get required references
		m_text = GetComponent<Text>();
		DebugUtils.Assert(m_text != null, "Required component!");
	}

	/// <summary>
	/// First update.
	/// </summary>
	private void Start() {
		// Initialize text
		UpdateText();
	}
	
	/// <summary>
	/// Called every frame.
	/// </summary>
	private void Update() {
		// Just keep text updated
		UpdateText();
	}

	/// <summary>
	/// Destructor.
	/// </summary>
	private void OnDestroy() {

	}

	//------------------------------------------------------------------//
	// INTERNAL UTILS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Update the textfield with the current value.
	/// </summary>
	private void UpdateText() {
		m_text.text = StringUtils.FormatNumber(UserProfile.pc);
	}
}

