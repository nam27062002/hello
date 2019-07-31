// DragonDataSpecial.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 27/08/2018.
// Copyright (c) 2018 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using System;
using System.Collections.Generic;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Dragon data implementation for the Special Dragons.
/// </summary>
[Serializable]
public class DragonDataSpecial : IDragonData {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	public const string TYPE_CODE = "special";

	// [AOC] TODO!! Move to content
	public static readonly DragonTier MIN_TIER_TO_UNLOCK = DragonTier.TIER_1;

	public enum Stat {
		HEALTH = 0,
		SPEED,
		ENERGY,

		COUNT
	}

    //------------------------------------------------------------------------//
    // MEMBERS AND PROPERTIES												  //
    //------------------------------------------------------------------------//
    // Progression
    private int m_level;
    public int Level
    {
        get { return m_level; }
    }

    private int m_maxLevel; // Calculated base in the specialDragonUpgradesDefinitions table
    public int MaxLevel
    {
        get { return m_maxLevel; }
    }

    // Stats
    private DragonStatData[] m_stats = new DragonStatData[(int)Stat.COUNT];
	private long m_statUpgradePriceBase = 0;
	private long m_statUpgradePriceCoefA = 0;
	private long m_statUpgradePriceCoefB = 0;
    private List<DefinitionNode> m_upgrades;

    public bool allStatsMaxed {
		get {
			for(int i = 0; i < m_stats.Length; ++i) {
				if(m_stats[i].level < m_stats[i].maxLevel) {
					return false;	// No need to keep checking
				}
			}
			return true;
		}
	}

	// Tier
	private DefinitionNode m_specialTierDef = null;
	public DefinitionNode specialTierDef {
		get { return m_specialTierDef; }
	}

	private List<DefinitionNode> m_specialTierDefsByOrder = null;
	public List<DefinitionNode> specialTierDefsByOrder {
		get { return m_specialTierDefsByOrder; }
	}

    private List<DefinitionNode> m_specialPowerDefsByOrder = null;
    public List<DefinitionNode> specialPowerDefsByOrder {
        get { return m_specialPowerDefsByOrder; }
    }

	// Power
	public int powerLevel = 0;
	public DefinitionNode biggestPowerDef {
		get {
			if(powerLevel == 0) return null;
			return m_specialPowerDefsByOrder[powerLevel - 1]; 
		}
	}

	//------------------------------------------------------------------------//
	// PARENT OVERRIDE PROPERTIES											  //
	//------------------------------------------------------------------------//
	// Stats - ranges to interpolate between min level and max level
	#region Base Stats
	public override float maxHealth {
		get {
            float baseValue = m_specialTierDef.GetAsFloat("health");
            return  baseValue +  baseValue * (GetStat(Stat.HEALTH).value);
        }
	}

	public override float maxForce {
		get {
            float baseValue = m_specialTierDef.GetAsFloat("force");
            return baseValue + baseValue * (GetStat(Stat.SPEED).value);
        }
	}

	public override float maxEatSpeedFactor {
		get { return m_specialTierDef.GetAsFloat("eatSpeedFactor"); }
	}

	public override float baseEnergy {
		get {
            float baseValue = m_specialTierDef.GetAsFloat("energyBase");
            return baseValue + baseValue * (GetStat(Stat.ENERGY).value); 
        }
	}
	#endregion

	// Scale
	#region Scale
	public override float scale {
		get { return m_specialTierDef.GetAsFloat("scale") + m_scaleOffset; }
	}

    public float scaleMenu {
        get { return m_specialTierDef.GetAsFloat("scaleMenu", 1f); }
    }

	public override float minScale {
		// [AOC] TODO!!
		get { return 1f; }
	}

	public override float maxScale {
		// [AOC] TODO!!
		get { return 1f; }
	}
	#endregion

	// Fury
	#region Fury
	public override float furyMax{ 
        get{ return m_specialTierDef.GetAsFloat("furyMax"); }
    }
    public override float furyBaseDuration{ 
        get{ return m_specialTierDef.GetAsFloat("furyBaseDuration"); }
    }
    public override float furyScoreMultiplier{ 
        get{ return m_specialTierDef.GetAsFloat("furyScoreMultiplier", 2); }
    }
    public override float furyBaseLength{ 
        get{ return m_specialTierDef.GetAsFloat("furyBaseLength"); }
    }
	#endregion

	// Movement
	#region Movement
	public override float mass{ 
        get{ return m_specialTierDef.GetAsFloat("mass"); }
    }
    public override float friction{ 
        get{ return m_specialTierDef.GetAsFloat("friction"); }
    }
    public override float gravityModifier{ 
        get{ return m_specialTierDef.GetAsFloat("gravityModifier"); }
    }
    public override float airGravityModifier{ 
        get{ return m_specialTierDef.GetAsFloat("airGravityModifier"); }
    }
    public override float waterGravityModifier{ 
        get{ return m_specialTierDef.GetAsFloat("waterGravityModifier"); }
    }
    public override float boostMultiplier{ 
        get{ return m_specialTierDef.GetAsFloat("boostMultiplier"); }
    }

    public override float defaultSize{
        get{ return m_specialTierDef.GetAsFloat("defaultSize"); }
    } 
    public override float cameraFrameWidthModifier{ 
        get{ return m_specialTierDef.GetAsFloat("cameraFrameWidthModifier"); }
    }
    
    public override float healthDrain{ 
        get { return m_specialTierDef.GetAsFloat("healthDrain"); } 
    }
    public override float healthDrainAmpPerSecond{ 
        get { return m_specialTierDef.GetAsFloat("healthDrainAmpPerSecond"); } 
    }
    public override float sessionStartHealthDrainTime{ 
        get { return m_specialTierDef.GetAsFloat("sessionStartHealthDrainTime"); } 
    }
    public override float sessionStartHealthDrainModifier{ 
        get { return m_specialTierDef.GetAsFloat("sessionStartHealthDrainModifier"); } 
    }
    public override float healthDrainSpacePlus{ 
        get { return m_specialTierDef.GetAsFloat("healthDrainSpacePlus"); } 
    }
    public override float damageAnimationThreshold{ 
        get { return m_specialTierDef.GetAsFloat("damageAnimationThreshold"); } 
    }
    public override float dotAnimationThreshold{ 
        get { return m_specialTierDef.GetAsFloat("dotAnimationThreshold"); } 
    }
	#endregion

	// Energy
	#region Other Parent Properties
	public override float energyDrain{ 
        get { return m_specialTierDef.GetAsFloat("energyDrain"); } 
    }
    public override float energyRefillRate{ 
        get { return m_specialTierDef.GetAsFloat("energyRefillRate"); } 
    }
        
    // Misc
    public override float statsBarRatio{ 
        get { return m_specialTierDef.GetAsFloat("statsBarRatio"); } 
    }
    
    public override float petScale{ 
        get{ return m_specialTierDef.GetAsFloat("petScale"); } 
    }
    
     // Super Size
    public override float superSizeUpMultiplier{ 
        get{ return m_specialTierDef.GetAsFloat("sizeUpMultiplier", 2); }
    }    
    public override float superSpeedUpMultiplier{
        get{ return m_specialTierDef.GetAsFloat("speedUpMultiplier", 2); }
    }    
    public override float superBiteUpMultiplier{ 
        get{ return m_specialTierDef.GetAsFloat("biteUpMultiplier", 2); }
    }    
    public override bool superInvincible{ 
        get{ return m_specialTierDef.GetAsBool("invincible", true); }
    }    
    public override bool superInfiniteBoost{
        get{ return m_specialTierDef.GetAsBool("infiniteBoost", true); }
    }    
    public override bool superEatEverything{ 
        get{ return m_specialTierDef.GetAsBool("eatEverything", true); }
    }    
    public override float superModeDuration{ 
        get{ return m_specialTierDef.GetAsFloat("modeDuration", 10); }
    }
    
    public override string gamePrefab {
        get{
            return m_specialTierDef.GetAsString("gamePrefab");
        }
    }

    #endregion

    //------------------------------------------------------------------------//
    // CONSTRUCTOR                                                            //
    //------------------------------------------------------------------------//
    public DragonDataSpecial()
    {
        for (int i = 0; i < (int)Stat.COUNT; i++)
        {
            m_stats[i] = new DragonStatData();
			m_stats[i].parentDragon = this;
        }
    }

	//------------------------------------------------------------------------//
	// PARENT OVERRIDE METHODS												  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialization using a definition. Should be called immediately after the constructor.
	/// </summary>
	/// <param name="_def">The definition of this dragon.</param>
	public override void Init(DefinitionNode _def) {
		// Call parent
		base.Init(_def);

		// Init tier definitions
		m_specialTierDefsByOrder = DefinitionsManager.SharedInstance.GetDefinitionsByVariable(DefinitionsCategory.SPECIAL_DRAGON_TIERS, "specialDragon", m_def.sku);
		DefinitionsManager.SharedInstance.SortByProperty(ref m_specialTierDefsByOrder, "upgradeLevelToUnlock", DefinitionsManager.SortType.NUMERIC);

        m_specialPowerDefsByOrder = DefinitionsManager.SharedInstance.GetDefinitionsByVariable(DefinitionsCategory.SPECIAL_DRAGON_POWERS, "specialDragon", m_def.sku);
        DefinitionsManager.SharedInstance.SortByProperty(ref m_specialPowerDefsByOrder, "upgradeLevelToUnlock", DefinitionsManager.SortType.NUMERIC);

        m_pets = new List<string>();
        SetTier(DragonTier.TIER_1);		// [AOC] Special dragons start at tier S!
		InitStats();

		// Eco vars
		m_statUpgradePriceBase = _def.GetAsLong("stepPrice", 0);
		m_statUpgradePriceCoefA = _def.GetAsLong("priceCoefA", 1);
		m_statUpgradePriceCoefB = _def.GetAsLong("priceCoefB", 1);

        // Upgrades
        m_upgrades = DefinitionsManager.SharedInstance.GetDefinitionsList(DefinitionsCategory.SPECIAL_DRAGON_UPGRADES);
        DefinitionsManager.SharedInstance.SortByProperty(ref m_upgrades, "order", DefinitionsManager.SortType.NUMERIC);

        // Max level is defined by the upgrades table
        m_maxLevel = m_upgrades.Count;

        // Type
        m_type = Type.SPECIAL;
	}
    
	/// <summary>
	/// Initialize stats data from the current dragon definition.
	/// </summary>
    private void InitStats()
    {
		if(m_def == null) return;

		DragonStatData healthStat = GetStat(Stat.HEALTH);
		healthStat.maxLevel = m_def.GetAsInt("hpBonusSteps", 10);
		healthStat.valueRange = m_def.GetAsRange("hpBonus");
        healthStat.valueRange *= 0.01f;
		healthStat.def = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.DRAGON_STATS, "health");
        
		DragonStatData speedStat = GetStat(Stat.SPEED);
		speedStat.maxLevel = m_def.GetAsInt("speedBonusSteps", 10);
		speedStat.valueRange = m_def.GetAsRange("speedBonus");
        speedStat.valueRange *= 0.01f;
		speedStat.def = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.DRAGON_STATS, "speed");
        
		DragonStatData energyData = GetStat(Stat.ENERGY);
		energyData.maxLevel = m_def.GetAsInt("boostBonusSteps", 10);
		energyData.valueRange = m_def.GetAsRange("boostBonus");
        energyData.valueRange *= 0.01f;
		energyData.def = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.DRAGON_STATS, "energy");
    }

	/// <summary>
	/// Gets the current lock state of this dragon.
	/// </summary>
	/// <returns>The lock state for this dragon.</returns>
	public override LockState GetLockState() {
		// a) Is dragon owned?
		if(m_owned) return LockState.OWNED;

		// b) Is dragon hidden or shadowed?
		bool mayBeShadowed = m_revealFromDragons.Count > 0;
		if(mayBeShadowed) {
			if(!m_revealed) {
				bool readyToReveal = true;
				for(int i = 0; i < m_revealFromDragons.Count; ++i) {
					readyToReveal = readyToReveal && DragonManager.IsDragonOwned(m_revealFromDragons[i]);
				}

				if(readyToReveal) {
					return LockState.REVEAL;
				} else {
					bool mayBeHidden = m_shadowFromDragons.Count > 0;
					if(mayBeHidden) {
						if(!m_teased) {
							bool redayToTease = true;
							for(int i = 0; i < m_shadowFromDragons.Count; ++i) {
								redayToTease = redayToTease && DragonManager.IsDragonOwned(m_shadowFromDragons[i]);
							}

							if(redayToTease) return LockState.TEASE;
							else return LockState.HIDDEN;
						}
					}
					return LockState.SHADOW;
				}
			}
		}

		// c) Is dragon locked?
		// Dragon is considered locked if minimum tier for special dragons hasn't been unlocked yet
		if(DragonManager.biggestOwnedDragon.tier < MIN_TIER_TO_UNLOCK) return LockState.LOCKED;

		// d) Dragon available for to purchase with SC
		return LockState.AVAILABLE;
	}

	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Get the data corresponding to a specific stat.
	/// </summary>
	/// <returns>The stat data.</returns>
	/// <param name="_stat">Stat to be obtained.</param>
	public DragonStatData GetStat(Stat _stat) {
		return m_stats[(int)_stat];
	}


    /// <summary>
    /// Get the cost (in SC or HC) of upgrading this dragon (both a stat upgrade or acquiring new power).
    /// </summary>
    /// <returns>The upgrade price in the proper currency.</returns>
    public Price GetNextUpgradePrice ()
    {
        // If the max level has been reached return null
        if (m_level == MaxLevel) return null;

        // Take the upgrade that corresponds the next level 
        DefinitionNode nextUpgrade = m_upgrades[m_level];

        int priceSC = nextUpgrade.GetAsInt("priceSC");
        int priceHC = nextUpgrade.GetAsInt("priceHC");

        if (priceSC > 0)
        {
            // Soft currency
            return new Price(priceSC, UserProfile.Currency.SOFT);
        }

        if (priceHC > 0)
        {
            // Hard currency
            return new Price(priceHC, UserProfile.Currency.HARD);
        }

        // [JOM] For now the upgrades will be only paid in SC and HC. No more golden fragments.

        return null;
    }


	/// <summary>
	/// Check whether this dragon can upgrade stats or not.
	/// Depends on biggest unlocked tier on normal progression.
	/// Doesn't check individual stats max level (use each stat's data for that), just if all stats are maxed.
	/// </summary>
	/// <returns>Whether this dragon can upgrade stats or not.</returns>
	public bool CanUpgradeStats() {
		// Never if all stats are maxed!
		if(allStatsMaxed) return false;

		// Nothing preventing stats upgrade
		return true;
	}

	/// <summary>
	/// Perform a stat level up.
	/// Will trigger SPECIAL_DRAGON_STAT_UPGRADED.
	/// </summary>
	/// <param name="_stat">Stat to be increased.</param>
	public void UpgradeStat(Stat _stat) {
		// Get stat data
		DragonStatData statData = GetStat(_stat);

		// Ignore if stat is maxed out
		if(statData.level == statData.maxLevel) return;

		// Increase stat level
		statData.level++;

        // Increase dragon level
        m_level++;

		// Notify listeners
		Messenger.Broadcast<DragonDataSpecial>(MessengerEvents.SPECIAL_DRAGON_LEVEL_UPGRADED, this);

		// Save persistence!
		PersistenceFacade.instance.Save_Request(true);
	}

    /// <summary>
	/// Perform a power up and a level up.
	/// Will trigger SPECIAL_DRAGON_POWER_UPGRADED events.
	/// </summary>
	public void UpgradePower()
    {
        // Ignore if stat is maxed out
        if (m_level == m_maxLevel) return;

        // Doble check if there is a power acquisition in this upgrade
        if (! IsUnlockingNewPower())
        {
            Debug.LogError("Cannot acquire a new power in this level. Something went wrong."); return;
        }

        // Cache current values to detect upgrades
        int oldPowerLevel = powerLevel;

        // Increase dragon level
        m_level++;
        Messenger.Broadcast<DragonDataSpecial>(MessengerEvents.SPECIAL_DRAGON_LEVEL_UPGRADED, this);

        // Refresh power and tier
        RefreshPowerLevel();
        RefreshDisguise();

        // Look for upgrades and notify listeners
        if (oldPowerLevel != powerLevel)
        {
            Messenger.Broadcast<DragonDataSpecial>(MessengerEvents.SPECIAL_DRAGON_POWER_UPGRADED, this);
        }

        // Save persistence!
        PersistenceFacade.instance.Save_Request(true);
    }

    /// <summary>
    /// Set the tier of this dragon. No checks performed or events triggered.
    /// </summary>
    /// <param name="_tier">New tier.</param>
    public void SetTier(DragonTier _tier)
    {
        string tierSku = TierToSku(_tier);
		DefinitionNode tierDefinition = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.DRAGON_TIERS, tierSku);
		SetTier(tierDefinition);
    }

	/// <summary>
	/// Set the tier of this dragon. No checks performed or events triggered.
	/// </summary>
	/// <param name="_def">New tier definition.</param>
	public void SetTier(DefinitionNode _def) {
		m_tierDef = _def;
		m_tier = (DragonTier)_def.GetAsInt("order");

		m_specialTierDef = GetDragonTierDef(m_def.sku, m_tier);

		// Enforce pets list size to number of slots
		m_pets.Resize(m_tierDef.GetAsInt("maxPetEquipped", 0), string.Empty);

		m_disguise = GetDefaultDisguise(m_def.sku).sku;
		m_persistentDisguise = m_disguise;
	}

	
    
	/// <summary>
	/// Update this dragon's power level based on current dragon level.
	/// </summary>
	public void RefreshPowerLevel()
    {
		// Reset power level
        powerLevel = 0;

		// Check Special Dragon power definitions
        int max = m_specialPowerDefsByOrder.Count;
        for (int i = 0; i < max; i++)
        {
            if (m_specialPowerDefsByOrder[i].GetAsInt("upgradeLevelToUnlock") <= m_level )
            {
                powerLevel++;
            }
        }
    }


    /// <summary>
    /// Check if the next level upgrade will unlock a new power
    /// </summary>
    public bool IsUnlockingNewPower()
    {

        // Check Special Dragon power definitions
        int max = m_specialPowerDefsByOrder.Count;
        for (int i = 0; i < max; i++)
        {
            // Is next level unlockin a power?
            if (m_level + 1 == m_specialPowerDefsByOrder[i].GetAsInt("upgradeLevelToUnlock") )
            {
                return true;
            }
        }

        return false;

    }



	/// <summary>
	/// Update this dragon's tier based on current dragon level.
	/// </summary>
	public void RefreshTier() {
		// Get dragon's current level

		// Check Tier definitions for this dragon
        string biggestTierSku = "tier_0";
		for(int i = 0; i < m_specialTierDefsByOrder.Count; ++i) {
			if(m_specialTierDefsByOrder[i].GetAsInt("upgradeLevelToUnlock") <= m_level) {
				biggestTierSku = m_specialTierDefsByOrder[i].Get("tier");
			}
		}
        
        DefinitionNode biggestTierDefNode = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.DRAGON_TIERS, biggestTierSku);
		// Save new tier
		if(biggestTierDefNode != null) {
			SetTier(biggestTierDefNode);
		}
	}

    
    public void RefreshDisguise(){
        // Get dragon's current level

        // Get disguises
        List<DefinitionNode> skinDefs = DefinitionsManager.SharedInstance.GetDefinitionsByVariable(DefinitionsCategory.DISGUISES, "dragonSku", m_sku);

        // Seach biggest number under level
        DefinitionNode definition = null;
        int unlockLevel = -1;
        for (int i = 0; i < skinDefs.Count; i++)
        {
            int testLevel = skinDefs[i].GetAsInt("unlockLevel");
            if ( testLevel <= m_level && unlockLevel < testLevel)
            {
                testLevel = unlockLevel;
                definition = skinDefs[i];
            }
        }

        if ( definition != null )
        {
            m_disguise = m_persistentDisguise = definition.sku;
        }
        else
        {
            m_disguise = m_persistentDisguise = "";
        }   

    }
    

	//------------------------------------------------------------------------//
	// STATIC UTILS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Get the definition of a special dragon's tier.
	/// </summary>
	/// <returns>The requested special dragon tier definition.</returns>
	/// <param name="_specialDragonSku">Special dragon whose tier definition we want.</param>
	/// <param name="_tier">Requested tier.</param>
	public static DefinitionNode GetDragonTierDef(string _specialDragonSku, DragonTier _tier) {
		DefinitionNode ret = null;
		string tierSku = TierToSku(_tier);
		List<DefinitionNode> defs = DefinitionsManager.SharedInstance.GetDefinitionsByVariable(DefinitionsCategory.SPECIAL_DRAGON_TIERS, "specialDragon", _specialDragonSku);
		int max = defs.Count;
		for(int i = 0; i < max && ret == null; i++) {
			if(defs[i].Get("tier").Equals(tierSku))
				ret = defs[i];
		}
		return ret;
	}

    /// <summary>
    /// Get the first definition (before any upgrade) of a special dragon's tier .
    /// </summary>
    /// <returns>The requested special dragon tier definition.</returns>
    /// <param name="_specialDragonSku">Special dragon whose tier definition we want.</param>
    public static DefinitionNode GetBaseDragonTierDef(string _specialDragonSku)
    {
        List<DefinitionNode> defs = DefinitionsManager.SharedInstance.GetDefinitionsByVariable(DefinitionsCategory.SPECIAL_DRAGON_TIERS, "specialDragon", _specialDragonSku);

        // Sort tier definitions by the unlocking level
        DefinitionsManager.SharedInstance.SortByProperty(ref defs, "upgradeLevelToUnlock", DefinitionsManager.SortType.NUMERIC);
        
        if (defs.Count > 0)
        {
            // Return the first occurrence
            return defs[0];
        }

        return null;
    }

    //------------------------------------------------------------------------//
    // PERSISTENCE															  //
    //------------------------------------------------------------------------//
    /// <summary>
    /// Reset persistence values. Make sure any new value is added to this method as well.
    /// </summary>
    public override void ResetLoadedData() {
		// Parent
		base.ResetLoadedData();

		// Custom data
		// Stats
		for(int i = 0; i < m_stats.Length; ++i) {
			m_stats[i].level = 0;
		}

        // Level
        m_level = 0;
	}

	/// <summary>
	/// Load state from a persistence object.
	/// </summary>
	/// <param name="_data">The data object loaded from persistence.</param>
	public override void Load(SimpleJSON.JSONNode _data) {
		// Parent
		base.Load(_data);

		// Custom data
		// Stats
		SimpleJSON.JSONArray statsData = null;
		if(_data.ContainsKey("stats")) {
			statsData = _data["stats"].AsArray;
		}

		int statsDataCount = statsData == null ? 0 : statsData.Count;
		for(int i = 0; i < m_stats.Length; ++i) {
			if(i < statsData.Count) {
				m_stats[i].Load(statsData[i]);
			}
		}

        // Level 
        if (_data.ContainsKey("level"))
        {
            m_level = _data["level"].AsInt;
        }
        else
        {
            // Do some data migration from old model to the new one
            // Check confluence
            // https://mdc-web-tomcat17.ubisoft.org/confluence/display/ubm/%5BHD%5D+Legendary+Dragons+-+Design+2.0
        }


        // Power level - Based on level, so just do a refresh
        RefreshPowerLevel();

		// Tier - Based on level, so just do a refresh
		RefreshTier();

        // Disguise - Based on level, so just do a refresh
        RefreshDisguise();

		// By changing tier, pet slots might change, so reload pets (the base already did it)
		LoadPets(_data);
	}

	/// <summary>
	/// Fill a persistence save data object with the current data for this dragon.
	/// </summary>
	/// <param name="_data">Data object to be filled.</param>
	protected override void Save(ref SimpleJSON.JSONClass _data) {
		// Parent
		base.Save(ref _data);

		// Custom data
		// Stats
		SimpleJSON.JSONArray statsData = new SimpleJSON.JSONArray();
		for(int i = 0; i < m_stats.Length; ++i) {
			statsData.Add(m_stats[i].Save());
		}
		_data.Add("stats", statsData);

        // Level
        _data.Add("level", m_level);
	}
}