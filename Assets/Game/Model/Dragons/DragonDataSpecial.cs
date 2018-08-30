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
		public int maxLevel = 9;

		public float baseValue = 100f;
		public float valueInc = 10f;
		public float value {
			get { return GetValueForLevel(level); }
		}

		/// <summary>
		/// Compute the value corresponding to a given level.
		/// </summary>
		/// <returns>The value of the skill for the requested level.</returns>
		/// <param name="_level">Level.</param>
		public float GetValueForLevel(int _level) {
			return baseValue + valueInc * _level;
		}
	}

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Progression
	// [AOC] TODO!!
	private int m_level = 0;
	public int level {
		get { return m_level; }
	}
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
		get { return GetStat(Stat.HEALTH).value; }
	}

	public override float maxForce {
		get { return m_specialTierDef.GetAsFloat("force"); }
	}

	public override float maxEatSpeedFactor {
		get { return m_specialTierDef.GetAsFloat("eatSpeedFactor"); }
	}

	public override float baseEnergy {
		get { return m_specialTierDef.GetAsFloat("energyBase"); }
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

		// Type
		m_type = Type.SPECIAL;
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