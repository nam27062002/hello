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

	// Progression
	public const string LEVELS = "LEVELS";
	public const string MISSIONS = "MISSIONS";
	public const string MISSION_TYPES = "MISSION_TYPES";
	public const string MISSION_DIFFICULTIES = "MISSION_DIFFICULTIES";

	// Dragons
	public const string DRAGONS = "DRAGONS";
	public const string DRAGON_TIERS = "DRAGON_TIERS";
	public const string DRAGON_PROGRESSION = "DRAGON_PROGRESSION";

	// Entities
	public const string PETS 				= "PETS";
	public const string ENTITIES 			= "ENTITIES";
	public const string DECORATIONS 		= "DECORATIONS";
	public const string ENTITY_CATEGORIES 	= "ENTITY_CATEGORIES";

	// Game
	public const string SCORE_MULTIPLIERS = "SCORE_MULTIPLIERS";
	public const string SURVIVAL_BONUS = "SURVIVAL_BONUS";

	// Metagame
	public const string EGGS = "EGGS";
	public const string EGG_REWARDS = "EGG_REWARDS";
	public const string CHEST_REWARDS = "CHEST_REWARDS";
	public const string DISGUISE_REWARDS_DISTRIBUTION = "DISGUISE_REWARDS_DISTRIBUTION";

	// Disguises
	public const string DISGUISES = "DISGUISES";
	public const string DISGUISES_EQUIP = "DISGUISES_EQUIP";
	public const string DISGUISES_POWERUPS = "DISGUISES_POWERUPS";

	// Hold Prey
	public const string HOLD_PREY_TIER = "HOLD_PREY_TIER";

	// Power Ups
	public const string POWERUPS = "POWERUPS";    
};
    