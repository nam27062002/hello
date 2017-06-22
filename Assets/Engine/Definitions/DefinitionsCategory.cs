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
public class DefinitionsCategory 
{
	public const string UNKNOWN = "UNKNOWN";

	// General
	public const string LOCALIZATION = "LOCALIZATION";
	public const string SETTINGS = "SETTINGS";		// Contains several xml files with different signatures: gameSettings, dragonSettings, initialSettings,...
	public const string PARTICLE_MANAGER_SETTINGS_LEVEL_0_AREA1 = "PARTICLE_MANAGER_SETTINGS_LEVEL_0_AREA1";
	public const string PARTICLE_MANAGER_SETTINGS_LEVEL_0_AREA2 = "PARTICLE_MANAGER_SETTINGS_LEVEL_0_AREA2";

	// Progression
	public const string LEVELS = "LEVELS";

	// Mission
	public const string MISSIONS = "MISSIONS";
	public const string MISSION_TYPES = "MISSION_TYPES";
	public const string MISSION_DIFFICULTIES = "MISSION_DIFFICULTIES";
	public const string MISSION_MODIFIERS = "MISSION_MODIFIERS";

	// Global Events
	public const string GLOBAL_EVENT_OBJECTIVES = "GLOBAL_EVENT_OBJECTIVES";
	public const string GLOBAL_EVENT_REWARDS = "GLOBAL_EVENT_REWARDS";

	// Dragons
	public const string DRAGONS = "DRAGONS";
	public const string DRAGON_TIERS = "DRAGON_TIERS";
	public const string DRAGON_PROGRESSION = "DRAGON_PROGRESSION";
	public const string DRAGON_HEALTH_MODIFIERS = "DRAGON_HEALTH_MODIFIERS";

	// Entities
	public const string PETS 				= "PETS";
	public const string PET_MOVEMENT 		= "PET_MOVEMENT";
	public const string ENTITIES 			= "ENTITIES";
	public const string DECORATIONS 		= "DECORATIONS";
	public const string ENTITY_CATEGORIES 	= "ENTITY_CATEGORIES";
	public const string FREEZE_CONSTANTS	= "FREEZE_CONSTANTS";

	// Game
	public const string SCORE_MULTIPLIERS = "SCORE_MULTIPLIERS";
	public const string SURVIVAL_BONUS = "SURVIVAL_BONUS";

	// Metagame
	public const string EGGS = "EGGS";
	public const string GOLDEN_EGGS = "GOLDEN_EGGS";
	public const string EGG_REWARDS = "EGG_REWARDS";
	public const string CHEST_REWARDS = "CHEST_REWARDS";
	public const string RARITIES = "RARITIES";
	public const string HUNGRY_LETTERS = "HUNGRY_LETTERS";
	public const string SHOP_PACKS = "SHOP_PACKS";

	// Disguises
	public const string DISGUISES = "DISGUISES";
	public const string DISGUISES_EQUIP = "DISGUISES_EQUIP";

	// Hold Prey
	public const string HOLD_PREY_TIER = "HOLD_PREY_TIER";

	// Power Ups
	public const string POWERUPS = "POWERUPS";

    // Quality settings
    public const string FEATURE_PROFILE_SETTINGS = "FEATURE_PROFILE_SETTINGS";
    public const string FEATURE_DEVICE_SETTINGS = "FEATURE_DEVICE_SETTINGS";
    public const string DEVICE_RATING_SETTINGS = "DEVICE_RATING_SETTINGS";
};
    