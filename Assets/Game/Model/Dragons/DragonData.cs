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
using System.Linq;
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
	public const string GAME_PREFAB_PATH = "Game/Dragons/";
	public const string MENU_PREFAB_PATH = "UI/Menu/Dragons/";

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
	private DragonTier m_tier;	// Cached value
	public DragonTier tier { get { return  m_tier; }}

	// Progression
	[SerializeField] private bool m_owned = false;
	public LockState lockState { get { return GetLockState(); }}
	public bool isLocked { get { return lockState == LockState.LOCKED; }}
	public bool isOwned { get { return m_owned; }}

	[SerializeField] private DragonProgression m_progression = null;	// Will be exposed via a custom editor
	public DragonProgression progression { get { return m_progression; }}

	// Stats
	private Range m_healthRange = new Range();
	public float maxHealth { get { return GetMaxHealthAtLevel(progression.level); }}

	private float m_baseEnergy = 0f;
	public float baseEnergy { get { return m_baseEnergy; }}

	private Range m_scaleRange = new Range(1f, 1f);
	public float scale { get { return GetScaleAtLevel(progression.level); }}

	// Pets
	// One entry per pet slot, will be empty if no pet is equipped in that slot
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
	private float m_scaleOffset = 0f;

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
		m_tier = (DragonTier)m_tierDef.GetAsInt("order");
		// Progression
		m_progression = new DragonProgression(this);

		// Stats
		m_healthRange = m_def.GetAsRange("health");
		m_baseEnergy = m_def.GetAsFloat("energyBase");
		m_scaleRange = m_def.GetAsRange("scale");

		// Items
		m_disguise = GetDefaultDisguise(_def.sku).sku;
		m_pets = new List<string>();
		m_pets.Resize(m_tierDef.GetAsInt("maxPetEquipped", 0), string.Empty);	// Enforce pets list size to number of slots

		// Other values
		m_scaleOffset = 0;
	}

	//------------------------------------------------------------------//
	// PUBLIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Compute the max health at a specific level.
	/// </summary>
	/// <returns>The dragon max health at the given level.</returns>
	/// <param name="_level">The level at which we want to know the max health value.</param>
	public float GetMaxHealthAtLevel(int _level) {
		float levelDelta = Mathf.InverseLerp(0, progression.maxLevel, _level);
		return m_healthRange.Lerp(levelDelta);
	}

	/// <summary>
	/// Compute the scale at a specific level.
	/// </summary>
	/// <returns>The dragon scale at the given level.</returns>
	/// <param name="_level">The level at which we want to know the scale value.</param>
	public float GetScaleAtLevel(int _level) {
		float levelDelta = Mathf.InverseLerp(0, progression.maxLevel, _level);
		return m_scaleRange.Lerp(levelDelta) + m_scaleOffset;
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
		// Dragon is considered locked if THE previous dragon is not maxed out
		int order = def.GetAsInt("order");
		if(order > 0) {		// First dragon should always be owned
			// Check previous dragon's progression
			if(!DragonManager.dragonsByOrder[order - 1].progression.isMaxLevel) {
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
	public void ResetLoadedData()
	{	
		m_owned = false;
		m_progression.Load(0,0);
		m_disguise = m_def != null ? GetDefaultDisguise(m_def.sku).sku : "";
		m_pets = Enumerable.Repeat(string.Empty, m_tierDef.GetAsInt("maxPetEquipped", 0)).ToList();	// Use Linq to easily fill the list with the default value
	}

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

		// Disguise
		if ( _data.ContainsKey("disguise") )
			m_disguise = _data["disguise"];
		else
			m_disguise = GetDefaultDisguise(sku).sku;

		// Pets
		// We must have all the slots, enforce list's size
		m_pets.Resize(m_tierDef.GetAsInt("maxPetEquipped", 0), string.Empty);
		if ( _data.ContainsKey("pets") )
		{
			SimpleJSON.JSONArray equip = _data["pets"].AsArray;
			for (int i = 0; i < equip.Count && i < m_pets.Count; i++) 
			{
				m_pets[i] = equip[i];
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
		data.Add("owned", m_owned.ToString(PersistenceManager.JSON_FORMATTING_CULTURE));
		data.Add("xp", progression.xp.ToString(PersistenceManager.JSON_FORMATTING_CULTURE));
		data.Add("level", progression.level.ToString(PersistenceManager.JSON_FORMATTING_CULTURE));
		data.Add("disguise", m_disguise);


		SimpleJSON.JSONArray pets = new SimpleJSON.JSONArray();
		for( int i = 0; i<m_pets.Count; i++ )
		{
			pets.Add( m_pets[i] == null ? string.Empty : m_pets[i] );	// [AOC] Adding a null value here breaks the JSON parsing when loading back :/
		}
		data.Add("pets", pets);

		return data;

	}

	//------------------------------------------------------------------------//
	// STATIC UTILS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Gets the default disguise for the given dragon def.
	/// </summary>
	/// <returns>The definition of the default disguise to be used by the given dragon.</returns>
	/// <param name="_dragonSku">The dragon whose default skin we want.</param>
	public static DefinitionNode GetDefaultDisguise(string _dragonSku) {
		// Get all the disguises for the given dragon
		List<DefinitionNode> defList = DefinitionsManager.SharedInstance.GetDefinitionsByVariable(DefinitionsCategory.DISGUISES, "dragonSku", _dragonSku);

		// Sort by unlock level
		DefinitionsManager.SharedInstance.SortByProperty(ref defList, "unlockLevel", DefinitionsManager.SortType.NUMERIC);

		// There should always be one skin unlocked at level 0, anyway use the first one
		return defList[0];
	}

	public static string TierToSku( DragonTier _tier)
	{
		return "tier_" + ((int)_tier);
	}
}
