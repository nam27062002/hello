// IDragonData.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 07/09/2015.
// Copyright (c) 2015 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;

using System;
using System.Linq;
using System.Collections.Generic;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Base class for a DragonData object.
/// Definition of a dragon, together with its current values.
/// Every dragon ID must be linked to one DragonData in the DragonManager prefab.
/// </summary>
[Serializable]
public abstract class IDragonData : IUISelectorItem {
		//------------------------------------------------------------------//
	// CONSTANTS														//
	//------------------------------------------------------------------//
	public const string GAME_PREFAB_PATH = "Game/Dragons/";
	public const string MENU_PREFAB_PATH = "UI/Menu/Dragons/";

	public enum Type {
		CLASSIC,
		SPECIAL
	}

	// Dragons can be unlocked with coins when the previous tier is completed (all dragons in it at max level), or directly with PC.
	public enum LockState {
		ANY = -1,   // Any of the below states
		HIDDEN,     // Player must purchase the target Dragons to be able to see the Shadow of this dragon
		TEASE,      // Requirements to see the shadow of this dragon have been completed
		SHADOW,     // Player must purchase the target Dragons to reveal this dragon
		REVEAL,     // Requirements to reveal this dragon have been completed
		LOCKED_UNAVAILABLE,	// Dragon is revealed but can only be acquired via special offers
		LOCKED,     // Previous tier hasn't been completed
		AVAILABLE,  // Previous tier has been completed but the dragon hasn't been purchased
		OWNED       // Dragon has been purchased and can be used
	}

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Members are serialized for debugging purposes, but dragon data is a volatile class
	// Definition
	[SerializeField] protected DefinitionNode m_def = null;
	public DefinitionNode def { get { return m_def; } }

	[SerializeField] protected string m_sku = string.Empty;
	public string sku { get { return m_sku; } }

	[SerializeField] protected Type m_type = Type.CLASSIC;
	public Type type { get { return m_type; } }

	[SerializeField] protected DefinitionNode m_tierDef = null;
	public DefinitionNode tierDef { get { return m_tierDef; } }
	protected DragonTier m_tier;  // Cached value
	public DragonTier tier { get { return m_tier; } }


    //-- Economy --------------
    private long m_priceSC = 0;
    private float m_priceSCModifier = 0f;
    public void AddPriceSCModifer(float _value) { m_priceSCModifier += _value; }

    public long priceSC { get { return m_priceSC; } }
    public long priceSCModified { get { return m_priceSC + Mathf.FloorToInt(m_priceSC * m_priceSCModifier / 100.0f); } }
    public float priceSCModifier { get { return m_priceSCModifier; } }

    private long m_pricePC = 0;
    private float m_pricePCModifier = 0f;
    public void AddPricePCModifer(float _value) { m_pricePCModifier += _value; }

    public long pricePC { get { return m_pricePC; } }
    public long pricePCModified { get { return m_pricePC + Mathf.FloorToInt(m_pricePC * m_pricePCModifier / 100.0f); } }
    public float pricePCModifier { get { return m_pricePCModifier; } }
    //--------------------------


    // Progression
    [SerializeField] protected bool m_owned = false;
	[SerializeField] protected bool m_teased = false;
	[SerializeField] protected bool m_revealed = false;
	[SerializeField] protected bool m_unlockAvailable = false;

	public LockState lockState { get { return GetLockState(); } }
	public bool isLocked { get { return lockState == LockState.LOCKED; } }
	public bool isOwned { get { return m_owned; } }
	public bool isTeased { get { return m_teased; } }
	public bool isRevealed { get { return m_revealed; } }
	public bool isUnlockAvailable { get { return m_unlockAvailable; } }

	protected List<string> m_shadowFromDragons = new List<string>();
	public List<string> shadowFromDragons {
		get { return m_shadowFromDragons; }
	}

	protected List<string> m_revealFromDragons = new List<string>();
	public List<string> revealFromDragons {
		get { return m_revealFromDragons; } 
	}

	protected List<string> m_unlockFromDragons = new List<string>();
	public List<string> unlockFromDragons {
		get { return m_unlockFromDragons; }
	}

	// Pets
	// One entry per pet slot, will be empty if no pet is equipped in that slot
	[SerializeField] protected List<string> m_pets;
	public List<string> pets { get { return m_pets; } }

	// Disguise
	// [AOC] We need 2 of these: the temporal disguise (i.e. for preview only) and the actual equipped disguise (the one that will be persisted)
	[SerializeField] protected string m_disguise;
	public string diguise {
		get { return m_disguise; }
		set { m_disguise = value; }
	}

	[SerializeField] protected string m_persistentDisguise;
	public string persistentDisguise {
		get { return m_persistentDisguise; }
		set { m_persistentDisguise = value; }
	}

	// Tracking
	protected int m_gamesPlayed = 0;
	public int gamesPlayed {
		get { return m_gamesPlayed; }
		set { m_gamesPlayed = value; }
	}

	// Debug
	protected float m_scaleOffset = 0f;

	//------------------------------------------------------------------------//
	// ABSTRACT PROPERTIES												 	  //
	//------------------------------------------------------------------------//
	// Stats
	public abstract float maxHealth { get; }
	public abstract float maxForce { get; }
	public abstract float maxEatSpeedFactor { get; }
	public abstract float baseEnergy { get; }
	public abstract float scale { get; }
	public abstract float minScale { get; }
	public abstract float maxScale { get; }
    
        // Fury
    public abstract float furyMax { get; }
    public abstract float furyBaseDuration { get; }
    public abstract float furyScoreMultiplier { get; }
    public abstract float furyBaseLength { get; }

        // Movement
    public abstract float mass{ get; }
    public abstract float friction{ get; }
    public abstract float gravityModifier{ get; }
    public abstract float airGravityModifier{ get; }
    public abstract float waterGravityModifier{ get; }
    public abstract float boostMultiplier{ get; }
    public virtual float maxSpeed {
        get { return (maxForce / friction) / mass; }    // Copied from DragonMotion to show stats on the menu
    }
    public float energyRequiredToBoost{
        get{ return m_def.GetAsFloat("energyRequiredToBoost", 0.2f); }
    }
    public float energyRestartThreshold{
         get{ return m_def.GetAsFloat("energyRestartThreshold", 1); }
    }
        // Camera
    public abstract float defaultSize{ get; }
    public abstract float cameraFrameWidthModifier{ get; }    
    
        // Health related data
    public abstract float healthDrain{ get; }
    public abstract float healthDrainAmpPerSecond{ get; }
    public abstract float sessionStartHealthDrainTime{ get; }
    public abstract float sessionStartHealthDrainModifier{ get; }
    public abstract float healthDrainSpacePlus{ get; }
    public abstract float damageAnimationThreshold{ get; }
    public abstract float dotAnimationThreshold{ get; }
    
       // Energy
    public abstract float energyDrain{ get; }
    public abstract float energyRefillRate{ get; }
    
        // Alcohol
    public abstract float maxAlcohol{ get; }
    public abstract float alcoholDrain{ get; }
    
        // Misc
    public abstract float statsBarRatio{ get; }
    public virtual string tidBoostAction { get{ return m_def.GetAsString("tidBoostAction", "TID_INGAME_HUD_BOOST"); } }
    public virtual string tidBoostReminder { get{ return m_def.GetAsString("tidBoostReminder", "TID_FEEDBACK_TUTO_HOLD_TO_BOOST"); } }
    public abstract float petScale{ get; }
    
        // supersize
    public abstract float superSizeUpMultiplier{ get; }    
    public abstract float superSpeedUpMultiplier{ get; }    
    public abstract float superBiteUpMultiplier{ get; }    
    public abstract bool superInvincible{ get; }    
    public abstract bool superInfiniteBoost{ get; }    
    public abstract bool superEatEverything{ get; }    
    public abstract float superModeDuration{ get; }
    
    
    // Other Abstract attributes
    public abstract string gamePrefab{ get; }
	//------------------------------------------------------------------------//
	// IUISelectorItem IMPLEMENTATION										  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// 
	/// </summary>
	public bool CanBeSelected() {
		return GetLockState() > LockState.HIDDEN;
	}

	//------------------------------------------------------------------------//
	// ABSTRACT METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Gets the current lock state of this dragon.
	/// </summary>
	/// <returns>The lock state for this dragon.</returns>
	public abstract LockState GetLockState();

	//------------------------------------------------------------------------//
	// PUBLIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialization using a definition. Should be called immediately after the constructor.
	/// </summary>
	/// <param name="_def">The definition of this dragon.</param>
	public virtual void Init(DefinitionNode _def) {
		// Store definition
		m_def = _def;
		m_sku = m_def.sku;

        m_priceSC = def.GetAsLong("unlockPriceCoins");
        m_pricePC = def.GetAsLong("unlockPricePC");

		string shadowFromDragonsData = m_def.GetAsString("shadowFromDragon");
		if(!string.IsNullOrEmpty(shadowFromDragonsData)) {
			m_shadowFromDragons.AddRange(shadowFromDragonsData.Split(';'));
		}
		m_teased = m_shadowFromDragons.Count == 0;

		string revealFromDragonsData = m_def.GetAsString("revealFromDragon");
		if(!string.IsNullOrEmpty(revealFromDragonsData)) {
			m_revealFromDragons.AddRange(revealFromDragonsData.Split(';'));
		}
		m_revealed = m_revealFromDragons.Count == 0;

		string unlockFromDragonsData = m_def.GetAsString("unlockFromDragon");
		if(!string.IsNullOrEmpty(unlockFromDragonsData)) {
			m_unlockFromDragons.AddRange(unlockFromDragonsData.Split(';'));
		}
		m_unlockAvailable = m_unlockFromDragons.Count == 0;

		// Items
		m_disguise = GetDefaultDisguise(_def.sku).sku;
		m_persistentDisguise = m_disguise;

		// Other values
		m_scaleOffset = 0;
	}

	/// <summary>
	/// Tease this dragon.
	/// Triggers the DRAGON_TEASED event.
	/// </summary>
	public void Tease() {
		m_teased = true;
		PersistenceFacade.instance.Save_Request();
		Messenger.Broadcast<IDragonData>(MessengerEvents.DRAGON_TEASED, this);
	}

	/// <summary>
	/// Reveal this dragon.
	/// </summary>
	public void Reveal() {
		m_teased = true;
		m_revealed = true;
		PersistenceFacade.instance.Save_Request();
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
		m_teased = true;
		m_revealed = true;

		// Dispatch global event
		Messenger.Broadcast<IDragonData>(MessengerEvents.DRAGON_ACQUIRED, this);
	}

	//------------------------------------------------------------------------//
	// SIMPLE SETTER/GETTER METHODS											  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// The order of this dragon.
	/// </summary>
	public int GetOrder() {
		return (def == null) ? -1 : def.GetAsInt("order");
	}

	/// <summary>
	/// Offsets the scale value.
	/// </summary>
	public void SetOffsetScaleValue(float _scale) {
		m_scaleOffset += _scale;
	}

	//------------------------------------------------------------------------//
	// PERSISTENCE															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Reset persistence values. Make sure any new value is added to this method as well.
	/// </summary>
	public virtual void ResetLoadedData() {
		m_owned = false;
		m_teased = m_shadowFromDragons.Count == 0;
		m_revealed = m_revealFromDragons.Count == 0;
		m_unlockAvailable = m_unlockFromDragons.Count == 0;

		m_disguise = m_def != null ? GetDefaultDisguise(m_def.sku).sku : "";
		m_persistentDisguise = m_disguise;
		m_pets = Enumerable.Repeat(string.Empty, m_tierDef.GetAsInt("maxPetEquipped", 0)).ToList(); // Use Linq to easily fill the list with the default value

		m_gamesPlayed = 0;
	}

	/// <summary>
	/// Load state from a persistence object.
	/// </summary>
	/// <param name="_data">The data object loaded from persistence.</param>
	public virtual void Load(SimpleJSON.JSONNode _data) {
		// Make sure the persistence object corresponds to this dragon
		string sku = _data["sku"];
		if(!DebugUtils.Assert(sku.Equals(def.sku), "Attempting to load persistence data corresponding to a different dragon ID, aborting")) {
			return;
		}

		// Just read values from persistence object
		m_owned = _data["owned"].AsBool;
		m_teased = _data["teased"].AsBool;
		m_revealed = _data["revealed"].AsBool;

		// Disguise
		if(_data.ContainsKey("disguise")) {
			m_persistentDisguise = _data["disguise"];
		} else {
			m_persistentDisguise = GetDefaultDisguise(sku).sku;
		}
		m_disguise = m_persistentDisguise;

		// Pets
		LoadPets(_data);

		// Tracking
		if(_data.ContainsKey("gamesPlayed")) {
			m_gamesPlayed = _data["gamesPlayed"].AsInt;
		} else {
			m_gamesPlayed = 0;
		}
	}

	/// <summary>
	/// Load the pets persistence.
	/// </summary>
	/// <param name="_dragonPersistenceData">Dragon persistence data.</param>
	protected void LoadPets(SimpleJSON.JSONNode _dragonPersistenceData) {
		// We must have all the slots, enforce list's size
		m_pets.Resize(m_tierDef.GetAsInt("maxPetEquipped", 0), string.Empty);
		if(_dragonPersistenceData.ContainsKey("pets")) {
			SimpleJSON.JSONArray equip = _dragonPersistenceData["pets"].AsArray;
			for(int i = 0; i < equip.Count && i < m_pets.Count; i++) {
				m_pets[i] = equip[i];
			}
		}
	}

	/// <summary>
	/// Create and return a persistence save data object initialized with the data.
	/// </summary>
	/// <returns>A new data object to be stored to persistence by the PersistenceManager.</returns>
	public SimpleJSON.JSONNode Save() {
		// Create new object, initialize and return it
		SimpleJSON.JSONClass data = new SimpleJSON.JSONClass();
		Save(ref data);
		return data;
	}

	/// <summary>
	/// Fill a persistence save data object with the current data for this dragon.
	/// </summary>
	/// <param name="_data">Data object to be filled.</param>
	protected virtual void Save(ref SimpleJSON.JSONClass _data) {
		_data.Add("sku", def.sku);
		_data.Add("owned", m_owned.ToString(PersistenceFacade.JSON_FORMATTING_CULTURE));
		_data.Add("teased", m_teased.ToString(PersistenceFacade.JSON_FORMATTING_CULTURE));
		_data.Add("revealed", m_revealed.ToString(PersistenceFacade.JSON_FORMATTING_CULTURE));
		_data.Add("disguise", m_persistentDisguise);


		SimpleJSON.JSONArray petsData = new SimpleJSON.JSONArray();
		for(int i = 0; i < m_pets.Count; i++) {
			petsData.Add(m_pets[i] == null ? string.Empty : m_pets[i]); // [AOC] Adding a null value here breaks the JSON parsing when loading back :/
		}
		_data.Add("pets", petsData);

		// Tracking
		_data.Add("gamesPlayed", m_gamesPlayed);
	}

	//------------------------------------------------------------------------//
	// STATIC UTILS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Factory method.
	/// </summary>
	/// <returns>A new data for the given dragon definition.</returns>
	/// <param name="_def">Dragon definition used to initialize the dragon data object.</param>
	public static IDragonData CreateFromDef(DefinitionNode _def) {
		// Check type
		IDragonData newData = null;
		string typeCode = _def.GetAsString("type", DragonDataClassic.TYPE_CODE);
		switch(typeCode) {
			case DragonDataClassic.TYPE_CODE: {
				newData = new DragonDataClassic();
			} break;
			case DragonDataSpecial.TYPE_CODE: {
				newData = new DragonDataSpecial();
			} break;
		}

		Debug.Assert(newData != null, "Attempting to create a dragon data of unknown type " + typeCode);

		newData.Init(_def);
		return newData;
	}

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

	/// <summary>
	/// Obtain the sku of a given Dragon Tier.
	/// </summary>
	/// <returns>The sku matching the requested tier.</returns>
	/// <param name="_tier">Tier to be converted.</param>
	public static string TierToSku(DragonTier _tier) {
		return "tier_" + ((int)_tier);
	}

	/// <summary>
	/// Obtain the Tier enum value from its sku.
	/// </summary>
	/// <returns>The tier enum value corresponding to the given tier sku.</returns>
	/// <param name="_tierSku">Tier sku to be converted.</param>
	public static DragonTier SkuToTier(string _tierSku) {
		DefinitionNode tierDefinition = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.DRAGON_TIERS, _tierSku);
		if(tierDefinition != null) {
			return (DragonTier)tierDefinition.GetAsInt("order");
		}
		return DragonTier.COUNT;
	}
}