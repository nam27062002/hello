// HUDTime.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 02/11/2015.
// Copyright (c) 2015 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;
using System;
using TMPro;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// Simple controller for a time counter in the hud.
/// </summary>
[RequireComponent(typeof(TextMeshProUGUI))]
public class HUDTime : MonoBehaviour {
	//------------------------------------------------------------------//
	// PROPERTIES														//
	//------------------------------------------------------------------//
	private TextMeshProUGUI m_valueTxt;
	private long m_lastSecondsPrinted;
	
	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {
		// Get external references
		m_valueTxt = GetComponent<TextMeshProUGUI>();
		m_valueTxt.text = "00:00";
		m_lastSecondsPrinted = -1;
	}

	/// <summary>
	/// First update call.
	/// </summary>
	private void Start() {
		UpdateTime();
	}

	/// <summary>
	/// Called every frame.
	/// </summary>
	private void Update() {
		UpdateTime();
	}

	//------------------------------------------------------------------//
	// INTERNAL UTILS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Updates the displayed score.
	/// </summary>
	private void UpdateTime() {

		long elapsedSeconds = (long)InstanceManager.gameSceneController.elapsedSeconds;
		if(elapsedSeconds != m_lastSecondsPrinted) {		
			// Do it!
			// Both for game and level editor
			m_valueTxt.text = TimeUtils.FormatTime(elapsedSeconds, TimeUtils.EFormat.DIGITS, 2, TimeUtils.EPrecision.MINUTES);
			m_lastSecondsPrinted = elapsedSeconds;
		}
	}
}
