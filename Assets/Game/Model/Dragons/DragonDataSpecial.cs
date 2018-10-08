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

	/// <summary>
	/// Auxiliar data class representing a single dragon stat.
	/// </summary>
	public class StatData {
		/// <summary>
		/// Stat definition, containing info like icon, tid, etc.
		/// </summary>
		public DefinitionNode def = null;

		/// <summary>
		/// Dragon whom this stat belongs to
		/// </summary>
		public DragonDataSpecial parentDragon = null;

		/// <summary>
		/// Level [0..N-1]
		/// </summary>
		public int level = 0;
        
		/// <summary>
		/// Max level that can be achieved on this stat
		/// </summary>
		public int maxLevel = 9;
        
		/// <summary>
		/// Bonus range
		/// Percentage bonus, i.e. 0.25 -> +25%
		/// </summary>
		public Range valueRange = new Range();

		/// <summary>
		/// Stat value at current level
		/// Percentage bonus, i.e. 0.25 -> +25% 
		/// </summary>
		public float value {
			get { return GetValueForLevel(level); }
		}

		/// <summary>
		/// Current level delta [0..1]
		/// </summary>
		public float progress {
			get { return level / (float)maxLevel; }
		}

		/// <summary>
		/// Amount increased for every level
		/// Percentage bonus, i.e. 0.05 -> +5%
		/// </summary>
		public float valueStep = 0f;

		/// <summary>
		/// Compute the value corresponding to a given level.
		/// </summary>
		/// <returns>The value of the skill for the requested level.</returns>
		/// <param name="_level">Level.</param>
		public float GetValueForLevel(int _level) {
			return valueRange.Lerp(progress);
		}
	}

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//

	public int maxLevel = (int)Stat.COUNT * 10;	// [AOC] HARDCODED!! Should go to content
	
	// Stats
	// [AOC] TODO!!
	private StatData[] m_stats = new StatData[(int)Stat.COUNT];
	private long m_statUpgradePriceBase = 0;
	private long m_statUpgradePriceCoefA = 0;
	private long m_statUpgradePriceCoefB = 0;

	// Tier
	// [AOC] TODO!!
	private DefinitionNode m_specialTierDef = null;

	// Power
	// [AOC] TODO!!

	// Debug
	public int m_powerLevel = 0;

	//------------------------------------------------------------------------//
	// PARENT OVERRIDE PROPERTIES											  //
	//------------------------------------------------------------------------//
	// Stats - ranges to interpolate between min level and max level
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
    
    // Fury
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
    
    // Movement
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
    
    // Energy
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

    //------------------------------------------------------------------------//
    // CONSTRUCTOR                                                            //
    //------------------------------------------------------------------------//
    public DragonDataSpecial()
    {
        for (int i = 0; i < (int)Stat.COUNT; i++)
        {
            m_stats[i] = new StatData();
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
        
        m_pets = new List<string>();
        SetTier(DragonTier.TIER_1);		// [AOC] Special dragons start at tier S!
        LoadStatDef( _def );

		// Eco vars
		m_statUpgradePriceBase = _def.GetAsLong("stepPrice", 0);
		m_statUpgradePriceCoefA = _def.GetAsLong("priceCoefA", 1);
		m_statUpgradePriceCoefB = _def.GetAsLong("priceCoefB", 1);

        // Type
		m_type = Type.SPECIAL;
	}
    
    private void LoadStatDef( DefinitionNode _def )
    {
        StatData healthStat = GetStat(Stat.HEALTH);
        healthStat.maxLevel = _def.GetAsInt("hpBonusSteps", 10);
        healthStat.valueRange = _def.GetAsRange("hpBonus");
        healthStat.valueRange *= 0.01f;
		healthStat.def = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.DRAGON_STATS, "health");
        
        StatData speedStat = GetStat(Stat.SPEED);
        speedStat.maxLevel = _def.GetAsInt("speedBonusSteps", 10);
        speedStat.valueRange = _def.GetAsRange("speedBonus");
        speedStat.valueRange *= 0.01f;
		speedStat.def = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.DRAGON_STATS, "speed");
        
        StatData energyData = GetStat(Stat.ENERGY);
        energyData.maxLevel = _def.GetAsInt("boostBonusSteps", 10);
        energyData.valueRange = _def.GetAsRange("boostBonus");
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
	public StatData GetStat(Stat _stat) {
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
	/// Perform a stat level up.
	/// Will trigger SPECIAL_DRAGON_STAT_UPGRADED.
	/// May also trigger SPECIAL_DRAGON_POWER_UPGRADED and SPECIAL_DRAGON_TIER_UPGRADED events.
	/// </summary>
	/// <param name="_stat">Stat to be increased.</param>
	public void UpgradeStat(Stat _stat) {
		// Get stat data
		StatData statData = GetStat(_stat);

		// Ignore if stat is maxed out
		if(statData.level == statData.maxLevel) return;

		// Cache current values to detect upgrades
		int oldPowerLevel = m_powerLevel;
		DragonTier oldTier = tier;

		// Increase stat level
		statData.level++;

		// Refresh power and tier
		RefreshPowerLevel();
		RefreshTier();

		// Notify listeners
		Messenger.Broadcast<DragonDataSpecial, DragonDataSpecial.Stat>(MessengerEvents.SPECIAL_DRAGON_STAT_UPGRADED, this, _stat);

		// Look for upgrades and notify listeners
		if(oldPowerLevel != m_powerLevel) {
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

		m_pets.Clear();
		m_pets.Resize(m_tierDef.GetAsInt("maxPetEquipped", 0), string.Empty);   // Enforce pets list size to number of slots

		m_disguise = GetDefaultDisguise(m_def.sku).sku;
		m_persistentDisguise = m_disguise;
	}
    
	/// <summary>
	/// Update this dragon's power level based on current dragon level.
	/// </summary>
	public void RefreshPowerLevel()
    {
		// Reset power level
        m_powerLevel = 0;

		// Get dragon's current level
        int level = GetLevel();

		// Check Special Dragon power definitions
		List<DefinitionNode> defs = DefinitionsManager.SharedInstance.GetDefinitionsByVariable(DefinitionsCategory.SPECIAL_DRAGON_POWERS, "specialDragon", m_def.sku);
        int max = defs.Count;
        for (int i = 0; i < max; i++)
        {
            if (defs[i].GetAsInt("upgradeLevelToUnlock") <= level )
            {
                m_powerLevel++;
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
		DefinitionNode biggestTierDef = null;
		List<DefinitionNode> defs = DefinitionsManager.SharedInstance.GetDefinitionsByVariable(DefinitionsCategory.SPECIAL_DRAGON_TIERS, "specialDragon", m_def.sku);
		DefinitionsManager.SharedInstance.SortByProperty(ref defs, "upgradeLevelToUnlock", DefinitionsManager.SortType.NUMERIC);
		for(int i = 0; i < defs.Count; ++i) {
			if(defs[i].GetAsInt("upgradeLevelToUnlock") <= level) {
				biggestTierDef = defs[i];
			}
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

	}

	/// <summary>
	/// Load state from a persistence object.
	/// </summary>
	/// <param name="_data">The data object loaded from persistence.</param>
	public override void Load(SimpleJSON.JSONNode _data) {
		// Parent
		base.Load(_data);

		// Custom data

	}

	/// <summary>
	/// Fill a persistence save data object with the current data for this dragon.
	/// </summary>
	/// <param name="_data">Data object to be filled.</param>
	protected override void Save(ref SimpleJSON.JSONClass _data) {
		// Parent
		base.Save(ref _data);

		// Custom data

	}
}