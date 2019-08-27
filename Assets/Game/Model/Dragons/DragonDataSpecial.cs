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

    public bool IsMaxed
    {
        get { return m_maxLevel == m_level; }
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
        RefreshSpecialTier();

        // Tier will be initialized once, and wont change anymore (special dragons are always tier_6)
        SetTier(DragonTier.TIER_6);

        InitStats();


        // Eco vars
        m_statUpgradePriceBase = _def.GetAsLong("stepPrice", 0);
		m_statUpgradePriceCoefA = _def.GetAsLong("priceCoefA", 1);
		m_statUpgradePriceCoefB = _def.GetAsLong("priceCoefB", 1);

        // Upgrades
        m_upgrades = DefinitionsManager.SharedInstance.GetDefinitionsList(DefinitionsCategory.SPECIAL_DRAGON_STATS_UPGRADES);
        DefinitionsManager.SharedInstance.SortByProperty(ref m_upgrades, "order", DefinitionsManager.SortType.NUMERIC);

        // Max level = total of stats levelUps plus power levelUps 
        int statsLevelUp = 3 * m_upgrades.Count;
        int powerLevelUp = m_specialPowerDefsByOrder.Count;
        m_maxLevel = statsLevelUp + powerLevelUp; 

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

        // c) If the last classic dragon is maxed, then is available via SC
        // d) If the minimum required classic dragon is owned, then is available via HC
        if (IsAvailableViaSC() || IsAvailableViaHC())
        { 
            return LockState.AVAILABLE;
        }

		// e) Dragon locked
		return LockState.LOCKED_UNAVAILABLE;

	}


    /// <summary>
	/// Whether the dragon is unlockable using SC
	/// </summary>
	/// <returns>True if is unlockable</returns>
    public bool IsAvailableViaSC()
    {
        // If the previous dragon is maxed, then the dragon is available via SC
        IDragonData previous = DragonManager.GetPreviousDragonData(sku);

        // Is it owned ?
        if (previous != null && previous.isOwned)
        {
            // Has reached maximum level?
            switch (previous.type)
            {
                case Type.CLASSIC:
                    {
                        return (previous as DragonDataClassic).progression.isMaxed;
                        break;
                    }
                    
                case Type.SPECIAL:
                    {
                        return (previous as DragonDataSpecial).IsMaxed;
                        break;
                    }
            }
        }

        // Not owned:
        return false;
    }

    /// <summary>
    /// Whether the dragon is unlockable using HC
    /// </summary>
    /// <returns>True if is unlockable</returns>
    public bool IsAvailableViaHC()
    {
        // If the minimum required classic dragon is owned, then is available via HC

        // Required dragon is stored in the content
        string unlockFromDragon = def.Get("unlockFromDragon");
        if (unlockFromDragon != null)
        {
            IDragonData requiredDragon = DragonManager.GetDragonData(unlockFromDragon);

            // If the player owns the required dragon or a superior one, then the special dragon is available via HC
            if (DragonManager.biggestOwnedDragon.tier >= requiredDragon.tier)
            {
                return true;
            }
        }

        return false;

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
    /// Get the cost (in SC or HC) of upgrading a stat in this dragon 
    /// </summary>
    /// <returns>The upgrade price in the proper currency.</returns>
    /// <param name="_stat">The stat to upgrade</param>
    public Price GetNextStatUpgradePrice (Stat _stat)
    {
        // Get current stat level
        int statLevel = m_stats[(int)_stat].level;

        // Make sure this upgrade exists in the content table
        if (statLevel >= m_upgrades.Count) return null;

        // Take the upgrade that corresponds the next level 
        DefinitionNode nextUpgrade = m_upgrades[statLevel];

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
    /// Get the cost (in SC or HC) of upgrading the next power in this dragon 
    /// </summary>
    /// <returns>The upgrade price in the proper currency.</returns>
    /// <param name="_dragonLevel">The level of the dragon where this powerup is accquired</param>
    public Price GetNextPowerUpgradePrice()
    {

        DefinitionNode nextPower = GetNextPowerUpgrade();

        if (nextPower == null) return null;

        int priceSC = nextPower.GetAsInt("priceSC");
        int priceHC = nextPower.GetAsInt("priceHC");

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
    /// Get the next power upgrade based on the current level of the dragon
    /// </summary>
    /// <returns>The upgrade price in the proper currency.</returns>
    public DefinitionNode GetNextPowerUpgrade ()
    {
        // Make sure there is a power upgrade in the next level
        DefinitionNode nextPower = m_specialPowerDefsByOrder.Find(d => d.GetAsInt("upgradeLevelToUnlock") > m_level);

        // No more powers, ciao!
        if (nextPower == null) return null;

        return nextPower;
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

        // Check if the dragon reached the next special tier
        RefreshSpecialTier();

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
        // Check if the dragon reached the next special tier
        RefreshSpecialTier();

        // Refresh Power Level
        RefreshPowerLevel();

        // Refresh Disguise
        RefreshDisguise();

        Messenger.Broadcast<DragonDataSpecial>(MessengerEvents.SPECIAL_DRAGON_LEVEL_UPGRADED, this);
        

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

        m_disguise = GetDefaultDisguise(m_def.sku).sku;
		m_persistentDisguise = m_disguise;

	}


    /// <summary>
    /// Update the special tier definition based on the current level of the dragon
    /// </summary>
    public void RefreshSpecialTier ()
    {
        // Update the special tier definition
        m_specialTierDef = GetSpecialTierDefByLevel(m_def.sku, m_level);

        // Enforce pets list size to number of slots
        m_pets.Resize(m_specialTierDef.GetAsInt("petsSlotsAvailable", 1), string.Empty);
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
    /// Returns the special tier definition based on the level of the dragon
    /// </summary>
    /// <param name="_specialDragonSku">Special dragon whose tier definition we want.</param>
	/// <param name="_level">Current level of the dragon</param>
    public static DefinitionNode GetSpecialTierDefByLevel(string _specialDragonSku, int _level) {

		List<DefinitionNode> defs = DefinitionsManager.SharedInstance.GetDefinitionsByVariable(DefinitionsCategory.SPECIAL_DRAGON_TIERS, "specialDragon", _specialDragonSku);
		int levelReq;

        // Iterate all special tiers
        for (int i = defs.Count - 1; i >= 0; i--)
        {
            levelReq = defs[i].GetAsInt("upgradeLevelToUnlock");
            
            // Has reached the required level for this tier?
            if (_level >= levelReq)
            {
                return defs[i];
            }
		}

        // Shouldn't happen but just in case, return the starting tier
        return defs[0];

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

    /// <summary>
    /// Load the pets persistence.
    /// </summary>
    /// <param name="_dragonPersistenceData">Dragon persistence data.</param>
    protected override void LoadPets(SimpleJSON.JSONNode _dragonPersistenceData)
    {
        // Enforce pets list size to number of slots
        m_pets.Resize(m_specialTierDef.GetAsInt("petsSlotsAvailable", 1), string.Empty);

        if (_dragonPersistenceData.ContainsKey("pets"))
        {
            SimpleJSON.JSONArray equip = _dragonPersistenceData["pets"].AsArray;
            for (int i = 0; i < equip.Count && i < m_pets.Count; i++)
            {
                m_pets[i] = equip[i];
            }
        }
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
		RefreshSpecialTier();

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