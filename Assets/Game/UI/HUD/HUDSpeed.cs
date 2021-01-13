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
	private DragonHealthBehaviour m_dragonHealthBehaviour;
	private float maxHigh;
	private float maxDeep;
	
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
		maxHigh = 0;
		maxDeep = 0;
	}

	IEnumerator Start() 
	{
		while( !InstanceManager.gameSceneControllerBase.IsLevelLoaded())
		{
			yield return null;
		}

		m_dragonMotion = InstanceManager.player.GetComponent<DragonMotion>();
		m_dragonHealthBehaviour = InstanceManager.player.GetComponent<DragonHealthBehaviour>();
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
			if ((m_dragonMotion.position.y - 171f) > maxHigh)
				maxHigh = (m_dragonMotion.position.y - 171f);
			if ((m_dragonMotion.position.y + 157f) < maxDeep)
				maxDeep = (m_dragonMotion.position.y + 157f);			
			m_valueTxt.text = "SPEED: " + m_dragonMotion.lastSpeed.ToString(".##") + "\nMAX HIGH: " + maxHigh.ToString(".#") + "\nMAX DEEP: " + maxDeep.ToString(".#");
			//m_valueTxt.text = m_dragonMotion.lastSpeed.ToString(".##") + "\nMAX DEEP: " + (m_dragonMotion.position.y + 157f).ToString(".#");
			//m_valueTxt.text = "DAMAGE RED.: " + m_dragonHealthBehaviour.damageHUD.ToString(".#")+ "%" + "\nDURATION: " + m_dragonHealthBehaviour.reviveBonusTime.ToString(".#");
		}
	}
}
