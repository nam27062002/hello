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
public class DragonData_OLD : IUISelectorItem {
	//------------------------------------------------------------------//
	// CONSTANTS														//
	//------------------------------------------------------------------//
	public const string GAME_PREFAB_PATH = "Game/Dragons/";
	public const string MENU_PREFAB_PATH = "UI/Menu/Dragons/";

	// Dragons can be unlocked with coins when the previous tier is completed (all dragons in it at max level), or directly with PC.
	public enum LockState {
		ANY = -1,	// Any of the below states
		HIDDEN,		// Player must purchase the target Dragons to be able to see the Shadow of this dragon
		TEASE,		// Requirements to see the shadow of this dragon have been completed
		SHADOW,		// Player must purchase the target Dragons to reveal this dragon
		REVEAL,		// Requirements to reveal this dragon have been completed
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
	[SerializeField] private bool m_teased = false;
	[SerializeField] private bool m_revealed = false;

	public LockState lockState { get { return GetLockState(); }}
	public bool isLocked { get { return lockState == LockState.LOCKED; }}
	public bool isOwned { get { return m_owned; }}
	public bool isTeased { get { return m_teased; }}
	public bool isRevealed { get { return m_revealed; }}

	[SerializeField] private DragonProgression m_progression = null;	// Will be exposed via a custom editor
	public DragonProgression progression { get { return m_progression; }}

	// Stats
	private Range m_healthRange = new Range();
	public float maxHealth { get { return GetMaxHealthAtLevel(progression.level); }}

	//TONI
	private Range m_forceRange = new Range();
	public float maxForce { get { return GetMaxForceAtLevel(progression.level); }}

	private float m_mass = 1f;
	public float mass {
		get { return m_mass; }
	}

	private float m_friction = 1f;
	public float friction {
		get { return m_friction; }
	}

	public float maxSpeed {
		get { return (maxForce / m_friction) / m_mass; }	// Copied from DragonMotion to show stats on the menu
	}

	private Range m_eatSpeedFactorRange = new Range();
	public float maxEatSpeedFactor { get { return GetMaxEatSpeedFactorAtLevel(progression.level); }}

	private Range m_energyBaseRange = new Range();
	public float baseEnergy { get { return GetMaxEnergyBaseAtLevel(progression.level); }}
	//private float m_baseEnergy = 0f;
	//public float baseEnergy { get { return m_baseEnergy; }}

	private Range m_scaleRange = new Range(1f, 1f);
	public float scale { get { return GetScaleAtLevel(progression.level); }}

	// Pets
	// One entry per pet slot, will be empty if no pet is equipped in that slot
	[SerializeField] private List<string> m_pets;
	public List<string> pets { get { return m_pets; } }

	// Disguise
	// [AOC] We need 2 of these: the temporal disguise (i.e. for preview only) and the actual equipped disguise (the one that will be persisted)
	[SerializeField] private string m_disguise;
	public string diguise { 
		get { return m_disguise; } 
		set { m_disguise = value; } 
	}

	[SerializeField] private string m_persistentDisguise;
	public string persistentDisguise { 
		get { return m_persistentDisguise; } 
		set { m_persistentDisguise = value; } 
	}

	private List<string> m_shadowFromDragons = new List<string>();
	private List<string> m_revealFromDragons = new List<string>();
	public List<string> revealFromDragons { get { return m_revealFromDragons; } }

	// Tracking
	private int m_gamesPlayed = 0;
	public int gamesPlayed {
		get { return m_gamesPlayed; }
		set { m_gamesPlayed = value; }
	}

	// Debug
	private float m_scaleOffset = 0f;

    public int m_powerLevel = 0;

	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	// [AOC] DONE
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
		//m_progression = new DragonProgression(this);

		string shadowFromDragons = m_def.GetAsString("shadowFromDragon");
		if (!string.IsNullOrEmpty(shadowFromDragons)) {
			m_shadowFromDragons.AddRange(shadowFromDragons.Split(';'));
		}
		m_teased = m_shadowFromDragons.Count == 0;

		string revealFromDragons = m_def.GetAsString("revealFromDragon");
		if (!string.IsNullOrEmpty(revealFromDragons)) {
			m_revealFromDragons.AddRange(revealFromDragons.Split(';'));
		}
		m_revealed = m_revealFromDragons.Count == 0;

		// Stats
		m_healthRange = m_def.GetAsRange("health");
		//TONI
		m_forceRange = m_def.GetAsRange("force");
		m_friction = m_def.GetAsFloat("friction");
		m_mass = m_def.GetAsFloat("mass");
		m_eatSpeedFactorRange = m_def.GetAsRange ("eatSpeedFactor");
		m_energyBaseRange = m_def.GetAsRange("energyBase");
		//m_baseEnergy = m_def.GetAsFloat("energyBase");

		m_scaleRange = m_def.GetAsRange("scale");

		// Items
		m_disguise = GetDefaultDisguise(_def.sku).sku;
        m_persistentDisguise = m_disguise;
        m_pets = new List<string>();
		m_pets.Resize(m_tierDef.GetAsInt("maxPetEquipped", 0), string.Empty);	// Enforce pets list size to number of slots

		// Other values
		m_scaleOffset = 0;
	}
    
	// [AOC] NOT NEEDED (DEBUG ONLY)
    public void SetTier( DragonTier _tier )
    {
        m_tierDef = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.DRAGON_TIERS,  TierToSku( _tier ) );
        m_tier = (DragonTier)m_tierDef.GetAsInt("order");
    }

	// [AOC] DONE
	public bool CanBeSelected() {
		return GetLockState() > LockState.HIDDEN;
	}


	//------------------------------------------------------------------//
	// PUBLIC METHODS													//
	//------------------------------------------------------------------//
	// [AOC] DONE
	/// <summary>
	/// Compute the max health at a specific level.
	/// </summary>
	/// <returns>The dragon max health at the given level.</returns>
	/// <param name="_level">The level at which we want to know the max health value.</param>
	public float GetMaxHealthAtLevel(int _level) {
		float levelDelta = Mathf.InverseLerp(0, progression.maxLevel, _level);
		return m_healthRange.Lerp(levelDelta);
	}

	// [AOC] DONE
	//TONI
	public float GetMaxForceAtLevel(int _level) {
		float levelDelta = Mathf.InverseLerp(0, progression.maxLevel, _level);
		return m_forceRange.Lerp(levelDelta);
	}

	// [AOC] DONE
	public float GetMaxEatSpeedFactorAtLevel(int _level) {
		float levelDelta = Mathf.InverseLerp(0, progression.maxLevel, _level);
		return m_eatSpeedFactorRange.Lerp(levelDelta);
	}

	// [AOC] DONE
	public float GetMaxEnergyBaseAtLevel(int _level) {
		float levelDelta = Mathf.InverseLerp(0, progression.maxLevel, _level);
		return m_energyBaseRange.Lerp(levelDelta);
	}

	// [AOC] DONE
	/// <summary>
	/// Compute the scale at a specific level.
	/// </summary>
	/// <returns>The dragon scale at the given level.</returns>
	/// <param name="_level">The level at which we want to know the scale value.</param>
	public float GetScaleAtLevel(int _level) {
		float levelDelta = Mathf.InverseLerp(0, progression.maxLevel, _level);
		return m_scaleRange.Lerp(levelDelta) + m_scaleOffset;
	}

	// [AOC] DONE
	/// <summary>
	/// Offsets the scale value.
	/// </summary>
	public void SetOffsetScaleValue(float _scale) {
		m_scaleOffset += _scale;
	}

	// [AOC] DONE
    public int GetOrder() {
        return (def == null) ? -1 : def.GetAsInt("order");
    }

	// [AOC] DONE
	/// <summary>
	/// Gets the current lock state of this dragon.
	/// </summary>
	/// <returns>The lock state for this dragon.</returns>
	public LockState GetLockState() {
		// a) Is dragon owned?
		if (m_owned) return LockState.OWNED;

		// b) Is dragon hidden or shadowed?
		bool mayBeShadowed = m_revealFromDragons.Count > 0;
		if (mayBeShadowed) {
			if (!m_revealed) {
				bool readyToReveal = true;
				for (int i = 0; i < m_revealFromDragons.Count; ++i) {
					readyToReveal = readyToReveal && DragonManager.IsDragonOwned(m_revealFromDragons[i]);
				}

				if (readyToReveal) {
					return LockState.REVEAL;
				} else {
					bool mayBeHidden = m_shadowFromDragons.Count > 0;
					if (mayBeHidden) {
						if (!m_teased) {
							bool redayToTease = true;
							for (int i = 0; i < m_shadowFromDragons.Count; ++i) {
								redayToTease = redayToTease && DragonManager.IsDragonOwned(m_shadowFromDragons[i]);
							}

							if (redayToTease) 	return LockState.TEASE;
							else 				return LockState.HIDDEN;
						}
					}
					return LockState.SHADOW;
				}
			}
		}		
			
		// c) Is dragon locked?
        // Dragon is considered locked if THE previous dragon is not maxed out
        int order = GetOrder();
		if (order > 0) {		// First dragon should always be owned
			// Check previous dragon's progression
			/*if (!DragonManager.classicDragonsByOrder[order - 1].progression.isMaxLevel) {
				return LockState.LOCKED;
			}*/
		}

		// d) Dragon available for to purchase with SC
		return LockState.AVAILABLE;
	}

	// [AOC] DONE
	public void Tease() {
		m_teased = true;

		PersistenceFacade.instance.Save_Request();

		//Messenger.Broadcast<DragonData>(MessengerEvents.DRAGON_TEASED, this);
	}

	// [AOC] DONE
	public void Reveal() {
		m_teased = true;
		m_revealed = true;
		PersistenceFacade.instance.Save_Request();
	}

	// [AOC] DONE
	/// <summary>
	/// Unlock this dragon (will be OWNED from now on). Doesn't do any currency transaction.
	/// Triggers the DRAGON_ACQUIRED event.
	/// </summary>
	public void Acquire() {
		// Skip if already owned
		if(m_owned) return;

		// Just change owned status
		m_owned = true;
		m_teased = true;
		m_revealed = true;

		// Dispatch global event
		//Messenger.Broadcast<DragonData>(MessengerEvents.DRAGON_ACQUIRED, this);
	}

	//------------------------------------------------------------------//
	// PERSISTENCE														//
	//------------------------------------------------------------------//
	// [AOC] DONE
	public void ResetLoadedData()
	{	
		m_owned = false;
		m_teased = m_shadowFromDragons.Count == 0;
		m_revealed = m_revealFromDragons.Count == 0;

		m_progression.Load(0,0);
		m_disguise = m_def != null ? GetDefaultDisguise(m_def.sku).sku : "";
		m_persistentDisguise = m_disguise;
		m_pets = Enumerable.Repeat(string.Empty, m_tierDef.GetAsInt("maxPetEquipped", 0)).ToList();	// Use Linq to easily fill the list with the default value

		m_gamesPlayed = 0;
	}

	// [AOC] DONE
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
		m_teased = _data["teased"].AsBool;
		m_revealed = _data["revealed"].AsBool;

		progression.Load(_data["xp"].AsFloat, _data["level"].AsInt);

		// Disguise
		if ( _data.ContainsKey("disguise") ) {
			m_persistentDisguise = _data["disguise"];
		} else {
			m_persistentDisguise = GetDefaultDisguise(sku).sku;
		}
		m_disguise = m_persistentDisguise;

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

		// Tracking
		if(_data.ContainsKey("gamesPlayed")) {
			m_gamesPlayed = _data["gamesPlayed"].AsInt;
		} else {
			m_gamesPlayed = 0;
		}
	}

	// [AOC] DONE
	/// <summary>
	/// Create and return a persistence save data object initialized with the data.
	/// </summary>
	/// <returns>A new data object to be stored to persistence by the PersistenceManager.</returns>
	public SimpleJSON.JSONNode Save() 
	{
		// Create new object, initialize and return it
		SimpleJSON.JSONClass data = new SimpleJSON.JSONClass();

		data.Add("sku", def.sku);
		data.Add("owned", m_owned.ToString(PersistenceFacade.JSON_FORMATTING_CULTURE));
		data.Add("teased", m_teased.ToString(PersistenceFacade.JSON_FORMATTING_CULTURE));
		data.Add("revealed", m_revealed.ToString(PersistenceFacade.JSON_FORMATTING_CULTURE));
		data.Add("xp", progression.xp.ToString(PersistenceFacade.JSON_FORMATTING_CULTURE));
		data.Add("level", progression.level.ToString(PersistenceFacade.JSON_FORMATTING_CULTURE));
		data.Add("disguise", m_persistentDisguise);


		SimpleJSON.JSONArray pets = new SimpleJSON.JSONArray();
		for( int i = 0; i<m_pets.Count; i++ )
		{
			pets.Add( m_pets[i] == null ? string.Empty : m_pets[i] );	// [AOC] Adding a null value here breaks the JSON parsing when loading back :/
		}
		data.Add("pets", pets);

		// Tracking
		data.Add("gamesPlayed", m_gamesPlayed);

		return data;

	}

	//------------------------------------------------------------------------//
	// STATIC UTILS															  //
	//------------------------------------------------------------------------//
	// [AOC] DONE
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

	// [AOC] DONE
	public static string TierToSku( DragonTier _tier)
	{
		return "tier_" + ((int)_tier);
	}
}
