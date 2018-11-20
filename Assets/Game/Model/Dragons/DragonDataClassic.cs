// DragonDataClassic.cs
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
/// Dragon Data implementation for the Special Dragons.
/// </summary>
[Serializable]
public class DragonDataClassic : IDragonData {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	public const string TYPE_CODE = "normal";
	
	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Progression
	[SerializeField] private DragonProgression m_progression = null;    // Will be exposed via a custom editor
	public DragonProgression progression { get { return m_progression; } }

	//------------------------------------------------------------------------//
	// PARENT OVERRIDE PROPERTIES											  //
	//------------------------------------------------------------------------//
	// Stats - ranges to interpolate between min level and max level
	private Range m_healthRange = new Range();
	public override float maxHealth { 
		get { return GetMaxHealthAtLevel(progression.level); } 
	}

	private Range m_forceRange = new Range();	//TONI
	public override float maxForce { 
		get { return GetMaxForceAtLevel(progression.level); } 
	}

	private Range m_eatSpeedFactorRange = new Range();
	public override float maxEatSpeedFactor { 
		get { return GetMaxEatSpeedFactorAtLevel(progression.level); } 
	}

	private Range m_energyBaseRange = new Range();
	public override float baseEnergy { 
		get { return GetMaxEnergyBaseAtLevel(progression.level); } 
	}

	private Range m_scaleRange = new Range(1f, 1f);
	public override float scale { 
		get { return GetScaleAtLevel(progression.level); } 
	}

	public override float minScale {
		get { return GetScaleAtLevel(0); }
	}

	public override float maxScale {
		get { return GetScaleAtLevel(progression.maxLevel); }
	}
    
    // Fury
    public override float furyMax{ 
        get{ return m_def.GetAsFloat("furyMax"); }
    }
    public override float furyBaseDuration{ 
        get{ return m_def.GetAsFloat("furyBaseDuration"); }
    }
    public override float furyScoreMultiplier{ 
        get{ return m_def.GetAsFloat("furyScoreMultiplier", 2); }
    }
    public override float furyBaseLength{ 
        get{ return m_def.GetAsFloat("furyBaseLength", 2); }
    }
    
    // Movement
    public override float mass{ 
        get{ return m_def.GetAsFloat("mass"); }
    }
    public override float friction{ 
        get{ return m_def.GetAsFloat("friction"); }
    }
    public override float gravityModifier{ 
        get{ return m_def.GetAsFloat("gravityModifier"); }
    }
    public override float airGravityModifier{ 
        get{ return m_def.GetAsFloat("airGravityModifier"); }
    }
    public override float waterGravityModifier{ 
        get{ return m_def.GetAsFloat("waterGravityModifier"); }
    }
    public override float boostMultiplier{ 
        get{ return m_def.GetAsFloat("boostMultiplier"); }
    }
    
    public override float defaultSize{
        get{ return m_def.GetAsFloat("defaultSize"); }
    }
    
    public override float cameraFrameWidthModifier{ 
        get{ return m_def.GetAsFloat("cameraFrameWidthModifier"); }
    }
    
    public override float healthDrain{ 
        get { return m_def.GetAsFloat("healthDrain"); } 
    }
    public override float healthDrainAmpPerSecond{ 
        get { return m_def.GetAsFloat("healthDrainAmpPerSecond"); } 
    }
    public override float sessionStartHealthDrainTime{ 
        get { return m_def.GetAsFloat("sessionStartHealthDrainTime"); } 
    }
    public override float sessionStartHealthDrainModifier{ 
        get { return m_def.GetAsFloat("sessionStartHealthDrainModifier"); } 
    }
    public override float healthDrainSpacePlus{ 
        get { return m_def.GetAsFloat("healthDrainSpacePlus"); } 
    }
    public override float damageAnimationThreshold{ 
        get { return m_def.GetAsFloat("damageAnimationThreshold"); } 
    }
    public override float dotAnimationThreshold{ 
        get { return m_def.GetAsFloat("dotAnimationThreshold"); } 
    }
    
    // Energy
    public override float energyDrain{ 
        get { return m_def.GetAsFloat("energyDrain"); } 
    }
    public override float energyRefillRate{ 
        get { return m_def.GetAsFloat("energyRefillRate"); } 
    }
    
    // Alcohol
    public override float maxAlcohol{ 
        get { return m_def.GetAsFloat("maxAlcohol"); } 
    }
    public override float alcoholDrain{ 
        get { return m_def.GetAsFloat("alcoholDrain", 1); } 
    }
    
    // Misc
    public override float statsBarRatio{ 
        get { return m_def.GetAsFloat("statsBarRatio"); } 
    }
    
    
    public override float petScale{ 
        get{ return m_def.GetAsFloat("petScale"); } 
    }
    
    
    public override string gamePrefab {
        get{
            return m_def.GetAsString("gamePrefab");
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

        m_tierDef = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.DRAGON_TIERS, _def.GetAsString("tier"));
        m_tier = (DragonTier)m_tierDef.GetAsInt("order");
        m_pets = new List<string>();
        m_pets.Resize(m_tierDef.GetAsInt("maxPetEquipped", 0), string.Empty);   // Enforce pets list size to number of slots
        
		// Type
		m_type = Type.CLASSIC;

		// Stats
		m_healthRange = m_def.GetAsRange("health");
		//TONI
		m_forceRange = m_def.GetAsRange("force");
		m_eatSpeedFactorRange = m_def.GetAsRange("eatSpeedFactor");
		m_energyBaseRange = m_def.GetAsRange("energyBase");
		//m_baseEnergy = m_def.GetAsFloat("energyBase");

		m_scaleRange = m_def.GetAsRange("scale");

		// Progression
		m_progression = new DragonProgression(this);
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
		// Dragon is considered locked if THE previous dragon is NOT maxed out
		int order = GetOrder();
		if(order > 0) {     // First dragon should always be owned
			// Check previous dragon's progression
			List<IDragonData> dragons = DragonManager.GetDragonsByOrder(Type.CLASSIC);
			if(!(dragons[order - 1] as DragonDataClassic).progression.isMaxLevel) {
				// Can the dragon be acquired?
				if(!m_unlockAvailable) {
					bool canBeUnlocked = true;

					// Check all required dragons are owned
					for(int i = 0; i < m_unlockFromDragons.Count; ++i) {
						canBeUnlocked = canBeUnlocked && DragonManager.IsDragonOwned(m_unlockFromDragons[i]);
					}

					if(canBeUnlocked) {
						m_unlockAvailable = true;	// No need to check again in this run
						return LockState.LOCKED;
					} else {
						// [AOC] Include discounted dragon condition here?
						return LockState.LOCKED_UNAVAILABLE;
					}
				}
				return LockState.LOCKED;
			}
		}

		// d) Dragon available for to purchase with SC
		return LockState.AVAILABLE;
	}

	//------------------------------------------------------------------------//
	// STATS METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Compute the max health at a specific level.
	/// </summary>
	/// <returns>The dragon max health at the given level.</returns>
	/// <param name="_level">The level at which we want to know the max health value.</param>
	public float GetMaxHealthAtLevel(int _level) {
		float levelDelta = Mathf.InverseLerp(0, progression.maxLevel, _level);
		return m_healthRange.Lerp(levelDelta);
	}

	//TONI
	public float GetMaxForceAtLevel(int _level) {
		float levelDelta = Mathf.InverseLerp(0, progression.maxLevel, _level);
		return m_forceRange.Lerp(levelDelta);
	}

	public float GetMaxEatSpeedFactorAtLevel(int _level) {
		float levelDelta = Mathf.InverseLerp(0, progression.maxLevel, _level);
		return m_eatSpeedFactorRange.Lerp(levelDelta);
	}

	public float GetMaxEnergyBaseAtLevel(int _level) {
		float levelDelta = Mathf.InverseLerp(0, progression.maxLevel, _level);
		return m_energyBaseRange.Lerp(levelDelta);
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

	//------------------------------------------------------------------//
	// PERSISTENCE														//
	//------------------------------------------------------------------//
	/// <summary>
	/// Reset persistence values. Make sure any new value is added to this method as well.
	/// </summary>
	public override void ResetLoadedData() {
		// Parent
		base.ResetLoadedData();

		// Custom data
		m_progression.Load(0, 0);
	}

	/// <summary>
	/// Load state from a persistence object.
	/// </summary>
	/// <param name="_data">The data object loaded from persistence.</param>
	public override void Load(SimpleJSON.JSONNode _data) {
		// Parent
		base.Load(_data);

		// Custom data
		progression.Load(_data["xp"].AsFloat, _data["level"].AsInt);
	}

	/// <summary>
	/// Fill a persistence save data object with the current data for this dragon.
	/// </summary>
	/// <param name="_data">Data object to be filled.</param>
	protected override void Save(ref SimpleJSON.JSONClass _data) {
		// Parent
		base.Save(ref _data);

		// Custom data
		_data.Add("xp", progression.xp.ToString(PersistenceFacade.JSON_FORMATTING_CULTURE));
		_data.Add("level", progression.level.ToString(PersistenceFacade.JSON_FORMATTING_CULTURE));
	}
}