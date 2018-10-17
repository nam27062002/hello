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

	public const DragonTier MIN_TIER_TO_UNLOCK = DragonTier.TIER_2;

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
	public int maxLevel = (int)Stat.COUNT * 10;	// [AOC] HARDCODED!! Should go to content
	
	// Stats
	private DragonStatData[] m_stats = new DragonStatData[(int)Stat.COUNT];
	private long m_statUpgradePriceBase = 0;
	private long m_statUpgradePriceCoefA = 0;
	private long m_statUpgradePriceCoefB = 0;

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
	private List<DefinitionNode> m_specialTierDefsByOrder = null;
	public List<DefinitionNode> specialTierDefsByOrder {
		get { return m_specialTierDefsByOrder; }
	}

	// Power
	public int powerLevel = 0;

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
    
    // Alcohol
    public override float maxAlcohol{ 
        get { return m_specialTierDef.GetAsFloat("maxAlcohol"); } 
    }
    public override float alcoholDrain{ 
        get { return m_specialTierDef.GetAsFloat("alcoholDrain", 1); } 
    }
    
    // Misc
    public override float statsBarRatio{ 
        get { return m_specialTierDef.GetAsFloat("statsBarRatio"); } 
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
        
        m_pets = new List<string>();
        SetTier(DragonTier.TIER_1);		// [AOC] Special dragons start at tier S!
		InitStats();

		// Eco vars
		m_statUpgradePriceBase = _def.GetAsLong("stepPrice", 0);
		m_statUpgradePriceCoefA = _def.GetAsLong("priceCoefA", 1);
		m_statUpgradePriceCoefB = _def.GetAsLong("priceCoefB", 1);

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
	/// Get the cost (in golden fragments) of upgrading a specific stat for this dragon.
	/// </summary>
	/// <returns>The upgrade price in golden fragments.</returns>
	/// <param name="_stat">The stat to be checked.</param>
	public long GetStatUpgradePrice(Stat _stat) {
		// baseCost + (upgradeLevel ^ A ) * B
		return m_statUpgradePriceBase + (long)(Mathf.Pow(GetStat(_stat).level, m_statUpgradePriceCoefA)) * m_statUpgradePriceCoefB;
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

		// Check if this dragon's next level unlocks a new tier
		int nextLevel = GetLevel() + 1;
		DefinitionNode nextTierDef = null;
		for(int i = 0; i < m_specialTierDefsByOrder.Count; ++i) {
			// Is this tier unlocked next level?
			if(m_specialTierDefsByOrder[i].GetAsInt("upgradeLevelToUnlock") == nextLevel) {
				// Is next tier restricted by the classic dragons biggest owned tier?
				nextTierDef = m_specialTierDefsByOrder[i];
				DragonTier requiredClassicTier = SkuToTier(nextTierDef.GetAsString("mainProgressionRestriction"));
				return requiredClassicTier <= DragonManager.biggestOwnedDragon.tier;
			}
		}

		// Nothing preventing stats upgrade
		return true;
	}

	/// <summary>
	/// Perform a stat level up.
	/// Will trigger SPECIAL_DRAGON_STAT_UPGRADED.
	/// May also trigger SPECIAL_DRAGON_POWER_UPGRADED and SPECIAL_DRAGON_TIER_UPGRADED events.
	/// </summary>
	/// <param name="_stat">Stat to be increased.</param>
	public void UpgradeStat(Stat _stat) {
		// Get stat data
		DragonStatData statData = GetStat(_stat);

		// Ignore if stat is maxed out
		if(statData.level == statData.maxLevel) return;

		// Cache current values to detect upgrades
		int oldPowerLevel = powerLevel;
		DragonTier oldTier = tier;

		// Increase stat level
		statData.level++;

		// Refresh power and tier
		RefreshPowerLevel();
		RefreshTier();

		// Notify listeners
		Messenger.Broadcast<DragonDataSpecial, DragonDataSpecial.Stat>(MessengerEvents.SPECIAL_DRAGON_STAT_UPGRADED, this, _stat);

		// Look for upgrades and notify listeners
		if(oldPowerLevel != powerLevel) {
			Messenger.Broadcast<DragonDataSpecial>(MessengerEvents.SPECIAL_DRAGON_POWER_UPGRADED, this);
		}

		if(oldTier != tier) {
			Messenger.Broadcast<DragonDataSpecial>(MessengerEvents.SPECIAL_DRAGON_TIER_UPGRADED, this);
		}
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
	/// Find the classic tier required for the next tier upgrade.
	/// </summary>
	/// <returns>The next required tier. DragonTier.COUNT if none.</returns>
	public DragonTier GetNextRequiredTier() {
		// Find next tier upgrade
		int level = GetLevel();
		for(int i = 0; i < m_specialTierDefsByOrder.Count; ++i) {
			// Is it the next tier upgrade?
			if(m_specialTierDefsByOrder[i].GetAsInt("upgradeLevelToUnlock") > level) {
				// Return required tier
				return SkuToTier(m_specialTierDefsByOrder[i].GetAsString("mainProgressionRestriction"));
			}
		}

		// No next tier upgrade found (probably we're max level)
		return DragonTier.COUNT;
	}
    
	/// <summary>
	/// Update this dragon's power level based on current dragon level.
	/// </summary>
	public void RefreshPowerLevel()
    {
		// Reset power level
        powerLevel = 0;

		// Get dragon's current level
        int level = GetLevel();

		// Check Special Dragon power definitions
		List<DefinitionNode> defs = DefinitionsManager.SharedInstance.GetDefinitionsByVariable(DefinitionsCategory.SPECIAL_DRAGON_POWERS, "specialDragon", m_def.sku);
        int max = defs.Count;
        for (int i = 0; i < max; i++)
        {
            if (defs[i].GetAsInt("upgradeLevelToUnlock") <= level )
            {
                powerLevel++;
            }
        }
    }

	/// <summary>
	/// Update this dragon's tier based on current dragon level.
	/// </summary>
	public void RefreshTier() {
		// Get dragon's current level
		int level = GetLevel();

		// Check Tier definitions for this dragon
        string biggestTierSku = "tier_0";
		for(int i = 0; i < m_specialTierDefsByOrder.Count; ++i) {
			if(m_specialTierDefsByOrder[i].GetAsInt("upgradeLevelToUnlock") <= level) {
				biggestTierSku = m_specialTierDefsByOrder[i].Get("tier");
			}
		}
        
        DefinitionNode biggestTierDefNode = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.DRAGON_TIERS, biggestTierSku);
		// Save new tier
		if(biggestTierDefNode != null) {
			SetTier(biggestTierDefNode);
		}
	}

	/// <summary>
	/// Get this special dragon's accumulated level.
	/// </summary>
	/// <returns>The current level of this special dragon.</returns>
    public int GetLevel()
    {
        int ret = 0;
        for (int i = 0; i < (int)Stat.COUNT; i++)
        {
            ret += m_stats[i].level;
        }
        return ret;
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

		// Power level - Based on level, so just do a refresh
		RefreshPowerLevel();

		// Tier - Based on level, so just do a refresh
		RefreshTier();

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
	}
}