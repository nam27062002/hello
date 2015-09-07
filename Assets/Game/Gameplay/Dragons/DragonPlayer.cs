// DragonPlayer.cs
// Hungry Dragon
// 
// Created by Marc Saña Forrellach on 05/08/2015.
// Copyright (c) 2015 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;
using System.Collections;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// Main access point to a dragon controlled by the player.
/// Contains references to its most used components as well as some common stats
/// such as health, energy, etc.
/// </summary>
public class DragonPlayer : MonoBehaviour {
	//------------------------------------------------------------------//
	// PROPERTIES														//
	//------------------------------------------------------------------//
	[Header("Type and general data")]
	[SerializeField] [Range(0, 1)] private int m_type = 0; 
	public int type { get { return m_type; } }


	[Header("Progression")]
	[SerializeField] [Range(0, 10)] private int m_level = 0;  
	public int level { get { return m_level; } }
	
	[SerializeField] private float m_eatTime = 0.5f;
	public float eatTime { get { return m_eatTime; } }

	[SerializeField] private float m_speed = 100f;
	public float speed { get { return m_speed; } }

	[SerializeField] private float m_boostMultiplier = 2.5f;
	public float boostMultiplier { get { return m_boostMultiplier; } }


	[Header("Life")]
	[SerializeField] private float m_maxLife = 100f;
	public float maxLife { get { return m_maxLife; } }

	[SerializeField] private float m_lifeDrainPerSecond = 10f;
	public float lifeDrainPerSecond { get { return m_lifeDrainPerSecond; } }

	[SerializeField] private float m_lifeWarningThreshold = 0.2f;	// Percentage of maxLife
	public float lifeWarningThreshold { get { return m_lifeWarningThreshold; } }


	[Header("Energy")]
	[SerializeField] private float m_maxEnergy = 50f;
	public float maxEnergy { get { return m_maxEnergy; } }

	[SerializeField] private float m_energyDrainPerSecond = 10f;
	public float energyDrainPerSecond { get { return m_energyDrainPerSecond; } }

	[SerializeField] private float m_energyRefillPerSecond = 25f;
	public float energyRefillPerSecond { get { return m_energyRefillPerSecond; } }

	[SerializeField] private float m_energyMinRequired = 25f;
	public float energyMinRequired { get { return m_energyMinRequired; } }


	[Header("Fury")]
	[SerializeField] private float m_maxFury = 160f;
	public float maxFury { get { return m_maxFury; } }

	[SerializeField] private float m_furyDuration = 15f; //seconds
	public float furyDuration { get { return m_furyDuration; } }

	[Header("Stats")]
	private float m_life;
	public float life { get { return m_life; } }

	private float m_energy;
	public float energy { get { return m_energy; } }
	
	private float[] m_fury = new float[2];//we'll use a secondary variable to store all the fury got while in Rush mode 
	private bool m_furyActive = false;
	public float fury { get { return m_fury[0]; } }
		
	private float m_speedMultiplier;

	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	void Awake () {
		m_life = m_maxLife;
		m_energy = m_maxEnergy;
		m_fury[0] = 0;
		m_fury[1] = 0;
		m_furyActive = false;

		m_speedMultiplier = 1f;
	}

	//------------------------------------------------------------------//
	// PUBLIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Add/remove health to the dragon.
	/// </summary>
	/// <param name="_offset">The amount of health to be added/removed.</param>
	public void AddLife(float _offset) {
		m_life = Mathf.Min(m_maxLife, Mathf.Max(0, m_life + _offset)); 
	}

	/// <summary>
	/// Add/remove energy to the dragon.
	/// </summary>
	/// <param name="_offset">The amount of energy to be added/removed.</param>
	public void AddEnergy(float _offset) {
		m_energy = Mathf.Min(m_maxEnergy, Mathf.Max(0, m_energy + _offset)); 
	}
		
	/// <summary>
	/// Add/remove fury to the dragon.
	/// </summary>
	/// <param name="_offset">The amount of fury to be added/removed.</param>
	public void AddFury(float _offset) {
		if (m_furyActive && _offset >= 0) {
			m_fury[1] = Mathf.Min(m_maxFury, Mathf.Max(0, m_fury[1] + _offset)); 
		} else {
			m_fury[0] = Mathf.Min(m_maxFury, Mathf.Max(0, m_fury[0] + _offset)); 
		}
	}

	/// <summary>
	/// Start fury rush.
	/// </summary>
	public void StartFury() {
		m_furyActive = true;
	}

	/// <summary>
	/// End fury rush.
	/// </summary>
	public void StopFury() {
		//when player used all the fury, we swap all the fury we got while throwing fire
		m_furyActive = false;
		m_fury[0] = m_fury[1];
		m_fury[1] = 0;
	}

	/// <summary>
	/// Sets the speed multiplier.
	/// </summary>
	/// <param name="_value">The new speed multiplier.</param>
	public void SetSpeedMultiplier(float _value) {
		m_speedMultiplier = _value;
	}

	/// <summary>
	/// Compute and get the accumulated speed multiplier.
	/// </summary>
	/// <returns>The current speed multiplier.</returns>
	public float GetSpeedMultiplier() {
		//we'll return the current speed factor. The dragon can be slowed down because it grabbed something or speed up by boost
		//calculate the final value, multiplying all the factors
		return m_speedMultiplier;
	}

	//------------------------------------------------------------------//
	// INTERNAL METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Initialize the dragon based on level.
	/// </summary>
	private void SetupFromLevel() {
		// add formulas and stuff to calculate values
		// and remove properties from inspector
	}
}
