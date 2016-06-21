// Definitions.cs
// 
// Imported by Miguel Angel Linares
// Refactored by Alger Ortín Castellví on 18/02/2016
// Copyright (c) 2016 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.IO;
using System.Text;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Custom definitions categories
/// </summary>
public enum DefinitionsCategory {
	UNKNOWN,

	// General
	LOCALIZATION,
	SETTINGS,		// Contains several xml files with different signatures: gameSettings, dragonSettings...

	// Progression
	LEVELS,
	MISSIONS,
	MISSION_TYPES,
	MISSION_DIFFICULTIES,

	// Dragons
	DRAGONS,
	DRAGON_TIERS,
	DRAGON_SKILLS,	// Contains skillDefinitions and skillProgressionDefinitions. The latter have a definition for each dragon (matching skus).

	// Entities
	ENTITIES,
	ENTITY_CATEGORIES,

	// Game
	SCORE_MULTIPLIERS,
	SURVIVAL_BONUS,

	// Metagame
	EGGS,
	EGG_REWARDS,
	CHEST_REWARDS,

	// Disguises
	DISGUISES,
	DISGUISES_EQUIP,
	DISGUISES_POWERUPS,

	// Fire properties
	FIRE_SPAWN_EFFECTS,
	FIRE_DECORATION_EFFECTS,

	// Hold Prey
	HOLD_PREY_TIER,

	// Power Ups
	POWERUPS,

};
    