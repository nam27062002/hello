﻿// DragonData.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 07/09/2015.
// Copyright (c) 2015 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;
using System;
using System.Collections.Generic;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// Definition of a dragon, together with its current values.
/// Every dragon ID must be linked to one DragonData in the DragonManager prefab.
/// </summary>
[Serializable]
public class DragonData {
	//------------------------------------------------------------------//
	// CONSTANTS														//
	//------------------------------------------------------------------//
	// All dragons have the same amount of levels
	[System.Obsolete]
	public static readonly int NUM_LEVELS = 10;

	/// <summary>
	/// Auxiliar serializable class to save/load to persistence.
	/// </summary>
	[Serializable]
	public class SaveData {
		// Only dynamic data is relevant
		[SkuList(typeof(DragonDef), false)] public string sku;
		public float xp = 0;
		public int level = 0;
		public int biteSkillLevel = 0;
		public int speedSkillLevel = 0;
		public int boostSkillLevel = 0;
		public int fireSkillLevel = 0;
		public bool owned = false;
		public string[] equip;
	}

	// Dragons can be unlocked with coins when the previous tier is completed (all dragons in it at max level), or directly with PC.
	public enum LockState {
		ANY = -1,	// Any of the below states
		LOCKED,		// Previous tier hasn't been completed
		AVAILABLE,	// Previous tier has been completed but the dragon hasn't been purchased
		OWNED		// Dragon has been purchased and can be used
	}

	//------------------------------------------------------------------//
	// PROPERTIES														//
	//------------------------------------------------------------------//
	// Definition
	[SerializeField] private DragonDef m_def = null;
	public DragonDef def { get { return m_def; }}

	// Progression
	[SerializeField] private bool m_owned = false;
	public LockState lockState { get { return GetLockState(); }}
	public bool isLocked { get { return lockState == LockState.LOCKED; }}
	public bool isOwned { get { return m_owned; }}

	[SerializeField] private DragonProgression m_progression = null;	// Will be exposed via a custom editor
	public DragonProgression progression { get { return m_progression; }}

	// Level-dependant stats
	public float maxHealth { get { return GetMaxHealthAtLevel(progression.level); }}
	public float scale { get { return GetScaleAtLevel(progression.level); }}

	// Skills
	[SerializeField] private DragonSkill m_biteSkill = null;
	public DragonSkill biteSkill { get { return m_biteSkill; }}

	[SerializeField] private DragonSkill m_speedSkill = null;
	public DragonSkill speedSkill { get { return m_speedSkill; }}

	[SerializeField] private DragonSkill m_boostSkill = null;
	public DragonSkill boostSkill { get { return m_boostSkill; }}

	[SerializeField] private DragonSkill m_fireSkill = null;
	public DragonSkill fireSkill { get { return m_fireSkill; }}

	// Items
	[SerializeField] private Dictionary<Equipable.AttachPoint, string> m_equip;
	public Dictionary<Equipable.AttachPoint, string> equip { get { return m_equip; } }

	// Debug
	private float m_scaleOffset = 0;

	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Initialization using a definition. Should be called immediately after the constructor.
	/// </summary>
	/// <param name="_def">The definition of this dragon.</param>
	public void Init(DragonDef _def) {
		// Store definition
		m_def = _def;

		// Progression
		m_progression = new DragonProgression(this);
		
		// Skills
		m_biteSkill = new DragonSkill(this, _def.biteSkill);
		m_speedSkill = new DragonSkill(this, _def.speedSkill);
		m_boostSkill = new DragonSkill(this, _def.boostSkill);
		m_fireSkill = new DragonSkill(this, _def.fireSkill);

		// Items
		m_equip = new Dictionary<Equipable.AttachPoint, string>();

		// Other values
		m_scaleOffset = 0;
	}

	//------------------------------------------------------------------//
	// PUBLIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Gets the skill.
	/// </summary>
	/// <returns>The skill.</returns>
	/// <param name="_sku">The sku of the wanted skill.</param>
	public DragonSkill GetSkill(string _sku) {
		// [AOC] Quick'n'dirty
		if(m_biteSkill.def.sku == _sku) {
			return m_biteSkill;
		} else if(m_speedSkill.def.sku == _sku) {
			return m_speedSkill;
		} else if(m_boostSkill.def.sku == _sku) {
			return m_boostSkill;
		} else if(m_fireSkill.def.sku == _sku) {
			return m_fireSkill;
		}
		return null;
	}

	/// <summary>
	/// Compute the max health at a specific level.
	/// </summary>
	/// <returns>The dragon max health at the given level.</returns>
	/// <param name="_level">The level at which we want to know the max health value.</param>
	public float GetMaxHealthAtLevel(int _level) {
		float levelDelta = Mathf.InverseLerp(0, progression.lastLevel, _level);
		return def.healthRange.Lerp(levelDelta);
	}

	/// <summary>
	/// Compute the scale at a specific level.
	/// </summary>
	/// <returns>The dragon scale at the given level.</returns>
	/// <param name="_level">The level at which we want to know the scale value.</param>
	public float GetScaleAtLevel(int _level) {
		float levelDelta = Mathf.InverseLerp(0, progression.lastLevel, _level);
		return def.scaleRange.Lerp(levelDelta) + m_scaleOffset;
	}

	/// <summary>
	/// Equip an item
	/// </summary>
	public void Equip(Equipable.AttachPoint _point, string _sku) {
		m_equip.Add(_point, _sku);
	}

	/// <summary>
	/// Unequip an item
	/// </summary>
	public void Unequip(Equipable.AttachPoint _point) {
		m_equip.Remove(_point);
	}

	/// <summary>
	/// Offsets speed value. Used for Debug purposes on Preproduction fase.
	/// </summary>
	public void OffsetSpeedValue(float _speed) {
		m_speedSkill.OffsetValue(_speed);
	}

	/// <summary>
	/// Offsets the scale value.
	/// </summary>
	public void OffsetScaleValue(float _scale) {
		m_scaleOffset += _scale;
	}

	/// <summary>
	/// Gets the current lock state of this dragon.
	/// </summary>
	/// <returns>The lock state for this dragon.</returns>
	public LockState GetLockState() {
		// a) Is dragon owned?
		if(m_owned) return LockState.OWNED;

		// b) Is tier unlocked?
		if(DragonManager.IsTierUnlocked(this.def.tier)) {
			return LockState.AVAILABLE;
		}

		// c) Dragon locked
		return LockState.LOCKED;
	}

	/// <summary>
	/// Unlock this dragon (will be OWNED from now on). Doesn't do any currency transaction.
	/// Triggers the DRAGON_ACQUIRED event.
	/// </summary>
	public void Acquire() {
		// Skip if already owned
		if(m_owned) return;

		// Just change owned status
		m_owned = true;

		// Dispatch global event
		Messenger.Broadcast<DragonData>(GameEvents.DRAGON_ACQUIRED, this);
	}

	//------------------------------------------------------------------//
	// PERSISTENCE														//
	//------------------------------------------------------------------//
	/// <summary>
	/// Load state from a persistence object.
	/// </summary>
	/// <param name="_data">The data object loaded from persistence.</param>
	public void Load(SaveData _data) {
		// Make sure the persistence object corresponds to this dragon
		if(!DebugUtils.Assert(_data.sku == def.sku, "Attempting to load persistence data corresponding to a different dragon ID, aborting")) {
			return;
		}

		// Just read values from persistence object
		m_owned = _data.owned;
		progression.Load(_data.xp, _data.level);

		// Skills
		m_biteSkill.Load(_data.biteSkillLevel);
		m_speedSkill.Load(_data.speedSkillLevel);
		m_boostSkill.Load(_data.boostSkillLevel);
		m_fireSkill.Load(_data.fireSkillLevel);

		// Equip
		for (int i = 0; i < _data.equip.Length; i++) {
			string[] tmp = _data.equip[i].Split(':');
			Equipable.AttachPoint point = (Equipable.AttachPoint)Enum.Parse(typeof(Equipable.AttachPoint), tmp[0]);
			m_equip.Add(point, tmp[1]);
		}
	}
	
	/// <summary>
	/// Create and return a persistence save data object initialized with the data.
	/// </summary>
	/// <returns>A new data object to be stored to persistence by the PersistenceManager.</returns>
	public SaveData Save() {
		// Create new object, initialize and return it
		SaveData data = new SaveData();

		data.sku = def.sku;
		data.owned = m_owned;
		data.xp = progression.xp;
		data.level = progression.level;

		data.biteSkillLevel = m_biteSkill.level;
		data.speedSkillLevel = m_speedSkill.level;
		data.boostSkillLevel = m_boostSkill.level;
		data.fireSkillLevel = m_fireSkill.level;

		data.equip = new string[m_equip.Count];
		int count = 0;
		foreach (Equipable.AttachPoint key in m_equip.Keys) {
			string tmp = key + ":" + m_equip[key];
			data.equip[count] = tmp;
			count++;
		}

		return data;
	}
}
