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

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// Simple controller for a time counter in the hud.
/// </summary>
[RequireComponent(typeof(Text))]
public class HUDTime : MonoBehaviour {
	//------------------------------------------------------------------//
	// PROPERTIES														//
	//------------------------------------------------------------------//
	private Text m_valueTxt;
	
	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {
		// Get external references
		m_valueTxt = GetComponent<Text>();
		m_valueTxt.text = "00:00";
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
		// Do it!
		m_valueTxt.text = TimeUtils.FormatTime((InstanceManager.sceneController as GameSceneController).elapsedSeconds, TimeUtils.EFormat.DIGITS, 2, TimeUtils.EPrecision.MINUTES);
	}
}
