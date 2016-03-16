// HUDHealthBar.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 14/09/2015.
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
/// Simple controller for a health bar in the debug hud.
/// </summary>
public class HUDStatBar : MonoBehaviour {
	//------------------------------------------------------------------//
	// CONSTANTS														//
	//------------------------------------------------------------------//
	public enum Type {
		Health,
		Energy,
		Fury,
		SuperFury
	}

	//------------------------------------------------------------------//
	// MEMBERS															//
	//------------------------------------------------------------------//
	[SerializeField] private Type m_type = Type.Health;

	//------------------------------------------------------------------//
	// PROPERTIES														//
	//------------------------------------------------------------------//
	private Slider m_bar;
	private Text m_valueTxt;
	
	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {
		// Get external references
		m_bar = GetComponentInChildren<Slider>();
		m_valueTxt = gameObject.FindComponentRecursive<Text>("TextValue");
	}

	/// <summary>
	/// Keep values updated
	/// </summary>
	private void Update() {
		// Only if player is alive
		if (InstanceManager.player != null) {
			// Bar value
			m_bar.minValue = 0f;
			m_bar.maxValue = GetMaxValue();
			m_bar.value = GetValue();

			// Text
			if (m_valueTxt != null) {
				m_valueTxt.text = String.Format("{0}/{1}",
				                                StringUtils.FormatNumber(m_bar.value, 0),
				                                StringUtils.FormatNumber(m_bar.maxValue, 0));
			}
		}
	}

	private float GetMaxValue() {
		switch (m_type) {
			case Type.Health: 	return InstanceManager.player.data.maxHealth;
			case Type.Energy:	return InstanceManager.player.data.def.GetAsFloat("energyMax");
			case Type.Fury:		return InstanceManager.player.data.def.GetAsFloat("furyMax");
			case Type.SuperFury:return InstanceManager.player.data.def.GetAsFloat("furyMax");
		}
		return 0;
	}

	private float GetValue() {
		switch (m_type) {
			case Type.Health: 	return InstanceManager.player.health;
			case Type.Energy:	return InstanceManager.player.energy;
			case Type.Fury:		return InstanceManager.player.fury;
			case Type.SuperFury:return InstanceManager.player.superFury;
		}		
		return 0;
	}
}
