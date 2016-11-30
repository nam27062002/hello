﻿// DragonProgression.cs
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

	//------------------------------------------------------------------//
	// PROPERTIES														//
	//------------------------------------------------------------------//
	// XP
	private float m_xp = 0;
	public float xp { get { return m_xp; }}

	private float[] m_levelsXp = null;	// XP required to unlock each level. Includes redundant level 0 (xp required for level 0 is always 0).
	public float[] levelsXP { get { return m_levelsXp; }}

	public float xpToNextLevel { get { return levelsXP[nextLevel] - levelsXP[level]; }}	// Should be safe, nextLevel is protected and level should never be > lastLevel
	public Range xpRange { get { return GetXpRangeForLevel(level); }}
		
	// Level
	private int m_level = 0;
	public int level { get { return m_level; }}	// [0..N-1], should never be > lastLevel

	public int numLevels { get { return m_levelsXp.Length; }}
	public int nextLevel { get { return Mathf.Min(m_level + 1, lastLevel); }}
	public int lastLevel { get { return numLevels - 1; }}
	public bool isMaxLevel { get { return m_level == lastLevel; }}
	public bool isMaxed { get { return xp >= levelsXP[lastLevel]; }}

	// Progress [0..1]
	public float progressByXp { get { return Mathf.InverseLerp(0f, levelsXP[lastLevel], m_xp); }}
	public float progressByLevel { get { return Mathf.InverseLerp(0, lastLevel, m_level); }}
	public float progressCurrentLevel { 
		get { 
			// If we've already reached last level, progress is always 1
			if(level >= lastLevel) return 1f;
			return Mathf.InverseLerp(levelsXP[m_level], levelsXP[nextLevel], m_xp);
		}
	}

	// Internal
	// Only to be set once
	[NonSerialized] private DragonData m_owner = null;	// [AOC] Avoid recursive serialization!!
	public DragonData owner { get { return m_owner; }}

	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Parametrized constructor.
	/// </summary>
	/// <param name="_owner">The dragon data this progression belongs to.</param>
	public DragonProgression(DragonData _owner) {
		// Store owner dragon
		m_owner = _owner;

		// Get definition based on dragon sku
		DefinitionNode progressionDef = null;
		if(_owner != null) {
			progressionDef = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.DRAGON_PROGRESSION, _owner.def.sku);
		}

		// Init!
		InitFromDef(progressionDef);
	}

	/// <summary>
	/// Initialize the level's XP data with the given dragonDefinition.
	/// </summary>
	/// <param name="_def">The dragon definition to be used.</param>
	public void InitFromDef(DefinitionNode _def) {
		// Check params
		if(_def == null) {
			// Clear current data and return
			m_levelsXp = new float[1];
			m_levelsXp[0] = 0;
			return;
		}

		// Get relevant data from definition
		int numLevels = _def.GetAsInt("numLevels", 1);

		// Reset xp array
		m_levelsXp = new float[numLevels];
		m_levelsXp[0] = 0;	// XP required for level 0 is always 0
		for(int i = 1; i < numLevels; i++) {
			m_levelsXp[i] = _def.GetAsFloat("xpLevel" + i);
		}

		/*
		string strLevels = "";
		string strLevelsXP = "";
		for(int i = 0; i < numLevels; i++) {
			strLevels += "[ " + i + " ]";
			strLevelsXP += "[ " + m_levelsXp[i] + " ]";
		}
		string str = _def.sku + ":\n";
		str += strLevels + "\n";
		str += strLevelsXP + "\n";
		str += "numLevels: " + numLevels + "\n";
		str += "lastLevel: " + lastLevel + "\n";
		Debug.Log(str);
		*/
	}

	//------------------------------------------------------------------//
	// PUBLIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Add experience to this progression.
	/// Optionally check for level ups, otherwise can be manually done by calling the IsLevelUpReady() and LevelUp() methods.
	/// </summary>
	/// <param name="_xpToAdd">The amount of xp to be added.</param>
	/// <param name="_checkLevelUp">Whether to check for level ups or not.</param>
	public void AddXp(float _xpToAdd, bool _checkLevelUp = false) {
		// Experience can't be subtracted
		if(_xpToAdd <= 0) return;

		// Add xp, capping to max level's XP
		m_xp = Mathf.Min(m_xp + _xpToAdd, m_levelsXp[lastLevel]);

		// Check for level ups
		if(_checkLevelUp) {
			LevelUp();
		}
	}

	/// <summary>
	/// Obtain the first level corresponding to a given XP value.
	/// </summary>
	/// <returns>The first level matching the given XP value.</returns>
	/// <param name="_xp">The XP value to be checked.</param>
	public int GetLevelFromXp(float _xp) {
		for(int i = 1; i < levelsXP.Length; i++) {
			if(_xp < levelsXP[i]) {
				return i - 1;
			}
		}
		return lastLevel;	// We're at the last level
	}

	/// <summary>
	/// Get the xp values of a specific level.
	/// </summary>
	/// <returns>The min and max absolute xp values for the given level.</returns>
	/// <param name="_level">The level whose data we want.</param>
	public Range GetXpRangeForLevel(int _level) {
		// Check level
		DebugUtils.Assert(_level >= 0 && _level <= lastLevel, "Level out of bounds!");

		// Special case for last level
		int nextLevel = Mathf.Min(_level + 1, lastLevel);
		return new Range(levelsXP[_level], levelsXP[nextLevel]);
	}

	/// <summary>
	/// Check whether the current amount of experience matches a level above our current one.
	/// </summary>
	/// <returns><c>true</c> if there is a pending level up.</returns>
	public bool IsLevelUpReady() {
		// For sure not if we're already in the last level
		if(m_level >= lastLevel) return false;

		// Check next level
		if(m_xp >= levelsXP[m_level + 1]) {
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
