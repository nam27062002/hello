// DragonProgression.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 07/09/2015.
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
/// Auxiliar class to be able to manage a dragon's XP/Level progression in cool way.
/// XP values represent the required amount of XP to reach a given level.
/// For level 0 it should always be 0.
/// XP can't be directly set, except when loading from persistence. It can only be 
/// added via the AddXp() method.
/// </summary>
[Serializable]
public class DragonProgression : SerializableClass {
	//------------------------------------------------------------------//
	// MEMBERS															//
	//------------------------------------------------------------------//
	// To be set on the inspector only
	[SerializeField] private float m_maxXp = 1000f; // Determine the XP required to reach the maximum value
	[SerializeField] private AnimationCurve m_curve = AnimationCurve.Linear(0, 0, DragonData.NUM_LEVELS - 1, 1000f);	// Will be used by the inspector to easily setup the values for each level
	[SerializeField] private float[] m_levelsXp = new float[DragonData.NUM_LEVELS];	// Will be interpolated using the max value and the curve

	//------------------------------------------------------------------//
	// PROPERTIES														//
	//------------------------------------------------------------------//
	// XP
	private float m_xp = 0;
	public float xp { get { return m_xp; }}
	public float xpToNextLevel { get { return m_levelsXp[nextLevel] - m_levelsXp[level]; }}	// Should be safe, nextLevel is protected and level should never be > lastLevel
	public Range xpRange { get { return new Range(m_levelsXp[level], m_levelsXp[nextLevel]); }}	// Should be safe, nextLevel is protected and level should never be > lastLevel
		
	// Level
	private int m_level = 0;
	public int level { get { return m_level; }}	// Should never be > lastLevel
	public int nextLevel { get { return Mathf.Min(m_level + 1, lastLevel); }}
	public int lastLevel { get { return m_levelsXp.Length - 1; }}

	// Progress [0..1]
	public float progressByXp { get { return Mathf.InverseLerp(0f, m_maxXp, m_xp); }}
	public float progressByLevel { get { return Mathf.InverseLerp(0, lastLevel, m_level); }}
	public float progressCurrentLevel { 
		get { 
			// If we've already reached last level, progress is always 1
			if(level >= lastLevel) return 1f;
			return Mathf.InverseLerp(m_levelsXp[m_level], m_levelsXp[nextLevel], m_xp);
		}
	}

	// Internal
	// Only to be set once
	[NonSerialized] private DragonData m_owner = null;	// [AOC] Avoid recursive serialization!!
	/*public DragonData owner {
		get { return m_owner; }
		set {
			if(m_owner != null) return;	// Owner already set, ignore setter
			m_owner = value;
		}
	}*/

	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Parametrized constructor.
	/// </summary>
	/// <param name="_owner">The dragon data this progression belongs to.</param>
	/// <param name="_maxXP">The initial XP value for the highest level.</param>
	public DragonProgression(DragonData _owner, float _maxXP) {
		m_owner = _owner;
		m_maxXp = _maxXP;
		m_curve = AnimationCurve.Linear(0, 0f, lastLevel, m_maxXp);
	}

	//------------------------------------------------------------------//
	// PUBLIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Add experience to this progression.
	/// Doesn't check for level ups, must be manually done by calling the IsLevelUpReady() method.
	/// </summary>
	/// <param name="_xpToAdd">The amount of xp to be added..</param>
	public void AddXp(float _xpToAdd) {
		// Experience can't be subtracted
		if(_xpToAdd <= 0) return;

		// Just do it
		m_xp += _xpToAdd;
	}

	/// <summary>
	/// Obtain the first level corresponding to a given XP value.
	/// </summary>
	/// <returns>The first level matching the given XP value.</returns>
	/// <param name="_xp">The XP value to be checked.</param>
	public int GetLevelFromXp(float _xp) {
		for(int i = 1; i < m_levelsXp.Length; i++) {
			if(_xp < m_levelsXp[i]) {
				return i - 1;
			}
		}
		return lastLevel;	// We're at the last level
	}

	/// <summary>
	/// Check whether the current amount of experience matches a level above our current one.
	/// </summary>
	/// <returns><c>true</c> if there is a pending level up.</returns>
	public bool IsLevelUpReady() {
		// For sure not if we're already in the last level
		if(m_level >= lastLevel) return false;

		// Check next level
		if(m_xp >= m_levelsXp[m_level + 1]) {
			return true;
		}

		// No level ups pending
		return false;
	}

	/// <summary>
	/// Level up this dragon as much levels as possible.
	/// </summary>
	/// <returns>The amount of levels progressed.</returns>
	public int LevelUp() {
		int levelUpCount = 0;
		while(IsLevelUpReady()) {
			// Everything ok! Do it
			m_level++;
			levelUpCount++;

			// Dispatch global event
			Messenger.Broadcast<DragonData>(GameEvents.DRAGON_LEVEL_UP, m_owner);
		}

		return levelUpCount;
	}

	//------------------------------------------------------------------//
	// PERSISTENCE														//
	//------------------------------------------------------------------//
	/// <summary>
	/// Initialize the progression with the given xp and level.
	/// </summary>
	/// <param name="_xp">The xp value to be used as current xp.</param>
	/// <param name="_level">The level to be used as current level.</param>
	public void Load(float _xp, int _level) {
		// Check and apply xp
		m_xp = Mathf.Max(0f, _xp);

		// Check and apply level
		m_level = Mathf.Clamp(_level, 0, lastLevel);
	}
}
