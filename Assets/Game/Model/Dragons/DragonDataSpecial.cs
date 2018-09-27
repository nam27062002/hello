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

	public class StatData {
		// Level [0..N-1]
		public int level = 0;
            // max level that can be achieved on this stat
		public int maxLevel = 9;
            // Bonus range
        public Range bonusRange = new Range();

		public float value {
			get { return GetValueForLevel(level); }
		}

		/// <summary>
		/// Compute the value corresponding to a given level.
		/// </summary>
		/// <returns>The value of the skill for the requested level.</returns>
		/// <param name="_level">Level.</param>
		public float GetValueForLevel(int _level) {
			return bonusRange.Lerp(level / (float)maxLevel);
		}
	}

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//

	public int maxLevel = (int)Stat.COUNT * 10;	// [AOC] HARDCODED!! Should go to content
	
	// Stats
	// [AOC] TODO!!
	private StatData[] m_stats = new StatData[(int)Stat.COUNT];

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
            return  baseValue +  baseValue * (GetStat(Stat.HEALTH).value / 100.0f);
        }
	}

	public override float maxForce {
		get {
            float baseValue = m_specialTierDef.GetAsFloat("force");
            return baseValue + baseValue * (GetStat(Stat.SPEED).value / 100.0f);
        }
	}

	public override float maxEatSpeedFactor {
		get { return m_specialTierDef.GetAsFloat("eatSpeedFactor"); }
	}

	public override float baseEnergy {
		get {
            float baseValue = m_specialTierDef.GetAsFloat("energyBase");
            return baseValue + baseValue * (GetStat(Stat.ENERGY).value / 100.0f); 
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
        SetTier(DragonTier.TIER_0);
        LoadStatDef( _def );

        // Type
		m_type = Type.SPECIAL;
	}
    
    private void LoadStatDef( DefinitionNode _def )
    {
        StatData healthStat = GetStat(Stat.HEALTH);
        healthStat.maxLevel = _def.GetAsInt("hpBonusSteps", 10);
        healthStat.bonusRange = _def.GetAsRange("hpBonus");
        
        StatData speedStat = GetStat(Stat.SPEED);
        speedStat.maxLevel = _def.GetAsInt("speedBonusSteps", 10);
        speedStat.bonusRange = _def.GetAsRange("speedBonus");
        
        StatData energyData = GetStat(Stat.ENERGY);
        energyData.maxLevel = _def.GetAsInt("boostBonusSteps", 10);
        energyData.bonusRange = _def.GetAsRange("boostBonus");
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
	/// Get the data corresponding to a specific skill.
	/// </summary>
	/// <returns>The skill data.</returns>
	/// <param name="_skill">Skill.</param>
	public StatData GetStat(Stat _skill) {
		return m_stats[(int)_skill];
	}
    
    public void SetTier(DragonTier _tier)
    {
        // m_specialTierDef = 
        string tierSku = TierToSku(_tier);
        m_tierDef = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.DRAGON_TIERS, tierSku);
        m_tier = _tier;

        m_specialTierDef = GetDragonTierDef(m_def.sku, _tier);

        m_pets.Clear();
        m_pets.Resize(m_tierDef.GetAsInt("maxPetEquipped", 0), string.Empty);   // Enforce pets list size to number of slots
        
        m_disguise = "";
        m_persistentDisguise = m_disguise;
    }
    
    public static DefinitionNode GetDragonTierDef(string specialDragonSku, DragonTier _tier)
    {
        DefinitionNode ret = null;
        string tierSku = TierToSku(_tier); 
        List<DefinitionNode> defs = DefinitionsManager.SharedInstance.GetDefinitionsByVariable(DefinitionsCategory.SPECIAL_DRAGON_TIERS, "specialDragon", specialDragonSku);
        int max = defs.Count;
        for (int i = 0; i < max && ret == null ; i++)
        {
            if (defs[i].Get("tier").Equals(tierSku))
                ret = defs[i];
        }
        return ret;
    }
    
    public void RefershPowerLevelValue()
    {
        m_powerLevel = 0;
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