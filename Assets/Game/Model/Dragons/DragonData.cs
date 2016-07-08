// DragonData.cs
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
	[SerializeField] private DefinitionNode m_def = null;
	public DefinitionNode def { get { return m_def; }}

	[SerializeField] private DefinitionNode m_tierDef = null;
	public DefinitionNode tierDef { get { return m_tierDef; }}

	// Progression
	[SerializeField] private bool m_owned = false;
	public DragonTier tier { get { return (DragonTier)m_tierDef.GetAsInt("order"); }}
	public LockState lockState { get { return GetLockState(); }}
	public bool isLocked { get { return lockState == LockState.LOCKED; }}
	public bool isOwned { get { return m_owned; }}

	[SerializeField] private DragonProgression m_progression = null;	// Will be exposed via a custom editor
	public DragonProgression progression { get { return m_progression; }}

	// Level-dependant stats
	private Range m_healthRange = new Range();
	public float maxHealth { get { return GetMaxHealthAtLevel(progression.level); }}

	private Range m_scaleRange = new Range(1f, 1f);
	public float scale { get { return GetScaleAtLevel(progression.level); }}

	// Skills
		// Speed Base
	[SerializeField] private DragonSkill m_speedSkill = null;
	public DragonSkill speedSkill { get { return m_speedSkill; }}

		// Boost size
	[SerializeField] private DragonSkill m_energySkill = null;
	public DragonSkill energySkill { get { return m_energySkill; }}

		// Fire Size
	[SerializeField] private DragonSkill m_fireSkill = null;
	public DragonSkill fireSkill { get { return m_fireSkill; }}

	// Pets
	[SerializeField] private List<string> m_pets;
	public List<string> pets { get { return m_pets; } }

	// Disguise
	[SerializeField] private string m_disguise;
	public string diguise 
	{ 
		get { return m_disguise; } 
		set { m_disguise = value; } 
	}

	// Debug
	private float m_scaleOffset = 0;

	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Initialization using a definition. Should be called immediately after the constructor.
	/// </summary>
	/// <param name="_def">The definition of this dragon.</param>
	public void Init(DefinitionNode _def) {
		// Store definition
		m_def = _def;
		m_tierDef = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.DRAGON_TIERS, _def.GetAsString("tier"));

		// Progression
		m_progression = new DragonProgression(this);

		// Level-dependant stats
		m_healthRange = m_def.GetAsRange("health");
		m_scaleRange = m_def.GetAsRange("scale");
		
		// Skills
		m_speedSkill = new DragonSkill(this, "speed");
		m_energySkill = new DragonSkill(this, "energy");
		m_fireSkill = new DragonSkill(this, "fire");

		// Items
		m_pets = new List<string>();
		m_disguise = "";

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
		if(m_speedSkill.def.sku == _sku) {
			return m_speedSkill;
		} else if(m_energySkill.def.sku == _sku) {
			return m_energySkill;
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
		return m_healthRange.Lerp(levelDelta);
	}

	/// <summary>
	/// Compute the scale at a specific level.
	/// </summary>
	/// <returns>The dragon scale at the given level.</returns>
	/// <param name="_level">The level at which we want to know the scale value.</param>
	public float GetScaleAtLevel(int _level) {
		float levelDelta = Mathf.InverseLerp(0, progression.lastLevel, _level);
		return m_scaleRange.Lerp(levelDelta) + m_scaleOffset;
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


		// b) Is dragon locked?
		// Dragon is considered locked if one of the previous dragons is not maxed out
		int order = def.GetAsInt("order");
		for(int i = 0; i < order; i++) {
			// Condition 1: level maxed
			if(!DragonManager.dragonsByOrder[i].progression.isMaxLevel) {
				return LockState.LOCKED;
			}
		}

		// c) Dragon available for to purchase with SC
		return LockState.AVAILABLE;
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
	public void Load(SimpleJSON.JSONNode _data) {
		// Make sure the persistence object corresponds to this dragon
		string sku = _data["sku"];
		if(!DebugUtils.Assert(sku.Equals(def.sku), "Attempting to load persistence data corresponding to a different dragon ID, aborting")) {
			return;
		}

		// Just read values from persistence object
		m_owned = _data["owned"].AsBool;

		progression.Load(_data["xp"].AsFloat, _data["level"].AsInt);

		// Skills
		m_speedSkill.Load(_data["speedSkillLevel"].AsInt);
		m_energySkill.Load(_data["boostSkillLevel"].AsInt);
		m_fireSkill.Load(_data["fireSkillLevel"].AsInt);

		// Disguise
		if ( _data.ContainsKey("disguise") )
			m_disguise = _data["disguise"];
		else
			m_disguise = "";

		// Pets
		m_pets.Clear();
		if ( _data.ContainsKey("pets") )
		{
			SimpleJSON.JSONArray equip = _data["pets"].AsArray;
			for (int i = 0; i < equip.Count; i++) 
			{
				m_pets.Add( equip[i] );
			}
		}

	}
	
	/// <summary>
	/// Create and return a persistence save data object initialized with the data.
	/// </summary>
	/// <returns>A new data object to be stored to persistence by the PersistenceManager.</returns>
	public SimpleJSON.JSONNode Save() 
	{
		// Create new object, initialize and return it
		SimpleJSON.JSONClass data = new SimpleJSON.JSONClass();

		data.Add("sku", def.sku);
		data.Add("owned", m_owned.ToString());
		data.Add("xp", progression.xp.ToString());
		data.Add("level", progression.level.ToString());
		
		data.Add("speedSkillLevel", m_speedSkill.level.ToString());
		data.Add("boostSkillLevel", m_energySkill.level.ToString());
		data.Add("fireSkillLevel", m_fireSkill.level.ToString());

		data.Add("disgguise", m_disguise);


		SimpleJSON.JSONArray pets = new SimpleJSON.JSONArray();
		for( int i = 0; i<m_pets.Count; i++ )
		{
			pets.Add( m_pets[i] );
		}
		data.Add("pets", pets);

		return data;

	}

	public static string TierToSku( DragonTier _tier)
	{
		return "tier_" + ((int)_tier);
	}
}
