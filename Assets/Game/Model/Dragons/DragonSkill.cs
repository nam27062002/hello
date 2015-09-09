// DragonSkill.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 09/09/2015.
// Copyright (c) 2015 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;
using System;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// Auxiliar class to be able to manage a dragon's skill upgrades.
/// </summary>
[Serializable]
public class DragonSkill {
	//------------------------------------------------------------------//
	// CONSTANTS														//
	//------------------------------------------------------------------//
	public static readonly int NUM_LEVELS = 6;

	// Skill types
	public enum EType {
		BITE,
		SPEED,
		BOOST,
		FIRE,
		COUNT
	};

	//------------------------------------------------------------------//
	// PROPERTIES														//
	//------------------------------------------------------------------//
	// To be set on the inspector only
	// Generic
	[SerializeField] private EType m_type = EType.BITE;
	public EType type { get { return m_type; }}

	[SerializeField] private string m_tidName = "";
	public string tidName { get { return m_tidName; }}
	
	[SerializeField] private string m_tidDescription = "";
	public string tidDescription { get { return m_tidDescription; }}

	// Values
	[SerializeField] private Range m_valueRange = new Range(0f, 1f);
	public float value { get { return m_valueRange.Lerp(progress); }}
	public float nextLevelValue { get { return m_valueRange.Lerp(nextLevelProgress); }}

	// Levels
	private int m_level = 0;
	public int level { get { return m_level; }}
	public int nextLevel { get { return Mathf.Min(m_level + 1, lastLevel); }}
	public int lastLevel { get { return NUM_LEVELS - 1; }}

	// Level unlock prices
	[SerializeField] private long[] m_unlockPrices = new long[NUM_LEVELS];
	public long nextLevelUnlockPrice { get { return m_unlockPrices[nextLevel]; }}

	// Progress
	public float progress { get { return Mathf.InverseLerp(0, lastLevel, level); }}
	private float nextLevelProgress { get { return Mathf.InverseLerp(0, lastLevel, level + 1); }}	// Internal usage

	// Internal
	// Only to be set once
	private DragonData m_owner = null;
	public DragonData owner {
		get { return m_owner; }
		set {
			if(m_owner != null) return;	// Owner already set, ignore setter
			m_owner = value;
		}
	}

	//------------------------------------------------------------------//
	// PUBLIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Unlock next level for this skill. Ignored if last level or not enough resources.
	/// Performs the resources transaction.
	/// </summary>
	/// <returns>Whether the next level was successfully unlocked or not.</returns>
	public bool UnlockNextLevel() {
		// Can the unlock be performed?
		if(CanUnlockNextLevel()) {
			// Subtract cost from user profile
			UserProfile.AddCoins(-nextLevelUnlockPrice);

			// Level up!
			m_level++;

			// Dispatch game event
			Messenger.Broadcast<DragonSkill>(GameEvents.DRAGON_SKILL_UPGRADED, this);

			return true;
		}

		return false;
	}

	/// <summary>
	/// Check whether all the conditions are met to unlock the next level of this skill.
	/// This includes not being the last level and have enough resources.
	/// </summary>
	/// <returns>Whether the next level can be unlocked or not.</returns>
	public bool CanUnlockNextLevel() {
		// Last level?
		if(level >= lastLevel) return false;

		// Enough resources?
		if(UserProfile.coins < nextLevelUnlockPrice) return false;

		// Everything ok!
		return true;
	}

	//------------------------------------------------------------------//
	// PERSISTENCE														//
	//------------------------------------------------------------------//
	/// <summary>
	/// Initialize the progression with the given level.
	/// </summary>
	/// <param name="_level">The level to be used as current level.</param>
	public void Load(float _xp, int _level) {
		// Check and apply level
		m_level = Mathf.Clamp(_level, 0, lastLevel);
	}
}
