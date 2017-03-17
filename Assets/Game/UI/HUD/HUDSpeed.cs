// HUDScore.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 26/10/2015.
// Copyright (c) 2015 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using TMPro;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// Simple controller for a score counter in the hud.
/// </summary>
[RequireComponent(typeof(TextMeshProUGUI))]
public class HUDSpeed : MonoBehaviour {
	//------------------------------------------------------------------//
	// PROPERTIES														//
	//------------------------------------------------------------------//
	private TextMeshProUGUI m_valueTxt;
	private DragonMotion m_dragonMotion;
	
	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() 
	{
		// Get external references
		m_valueTxt = GetComponent<TextMeshProUGUI>();
		m_valueTxt.text = "0";
	}

	IEnumerator Start() 
	{
		while( !InstanceManager.gameSceneControllerBase.IsLevelLoaded())
		{
			yield return null;
		}

		m_dragonMotion = InstanceManager.player.GetComponent<DragonMotion>();
	}

	void Update()
	{
		UpdateSpeed();	
	}

	private void UpdateSpeed() 
	{
		// Do it!
		if ( m_dragonMotion != null )
		{
			m_valueTxt.text = m_dragonMotion.lastSpeed.ToString(".##");
		}
	}
}
