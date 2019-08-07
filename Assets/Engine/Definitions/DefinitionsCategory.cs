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
	public const string FONT_GROUPS = "FONT_GROUPS";
	public const string SETTINGS = "SETTINGS";		// Contains several xml files with different signatures: gameSettings, dragonSettings, initialSettings,...
	public const string PARTICLE_MANAGER_SETTINGS_LEVEL_0_AREA1 = "PARTICLE_MANAGER_SETTINGS_LEVEL_0_AREA1";
	public const string PARTICLE_MANAGER_SETTINGS_LEVEL_0_AREA2 = "PARTICLE_MANAGER_SETTINGS_LEVEL_0_AREA2";
	public const string PARTICLE_MANAGER_SETTINGS_LEVEL_0_AREA3 = "PARTICLE_MANAGER_SETTINGS_LEVEL_0_AREA3";
	public const string POOL_MANAGER_SETTINGS_LEVEL_0_AREA1 = "POOL_MANAGER_SETTINGS_LEVEL_0_AREA1";
	public const string POOL_MANAGER_SETTINGS_LEVEL_0_AREA2 = "POOL_MANAGER_SETTINGS_LEVEL_0_AREA2";
	public const string POOL_MANAGER_SETTINGS_LEVEL_0_AREA3 = "POOL_MANAGER_SETTINGS_LEVEL_0_AREA3";
	public const string SEASONS = "SEASONS";
    public const string NOTIFICATIONS = "NOTIFICATIONS";

	// Progression
	public const string LEVELS = "LEVELS";

	// Mission
	public const string MISSIONS = "MISSIONS";
	public const string MISSION_TYPES = "MISSION_TYPES";
	public const string MISSION_DIFFICULTIES = "MISSION_DIFFICULTIES";
	public const string MISSION_MODIFIERS = "MISSION_MODIFIERS";

    public const string MISSION_SPECIAL_DIFFICULTIES = "MISSION_SPECIAL_DIFFICULTIES";
    public const string MISSION_SPECIAL_MODIFIERS = "MISSION_SPECIAL_MODIFIERS";

	// Dragons
	public const string DRAGONS = "DRAGONS";
	public const string DRAGON_TIERS = "DRAGON_TIERS";
	public const string DRAGON_PROGRESSION = "DRAGON_PROGRESSION";
	public const string DRAGON_HEALTH_MODIFIERS = "DRAGON_HEALTH_MODIFIERS";
	public const string DRAGON_STATS = "DRAGON_STATS";
    public const string SPECIAL_DRAGON_TIERS = "SPECIAL_DRAGON_TIERS";
    public const string SPECIAL_DRAGON_POWERS = "SPECIAL_DRAGON_POWERS";
    public const string SPECIAL_DRAGON_STATS_UPGRADES = "SPECIAL_DRAGON_STATS_UPGRADES";

    // Entities
    public const string PETS 				= "PETS";
	public const string PET_MOVEMENT 		= "PET_MOVEMENT";
	public const string PET_CATEGORIES 		= "PET_CATEGORIES";
	public const string ENTITIES 			= "ENTITIES";
	public const string DECORATIONS 		= "DECORATIONS";
	public const string ENTITY_CATEGORIES 	= "ENTITY_CATEGORIES";
	public const string FREEZE_CONSTANTS	= "FREEZE_CONSTANTS";
    public const string EQUIPABLE           = "EQUIPABLE";

	// Game
	public const string SCORE_MULTIPLIERS = "SCORE_MULTIPLIERS";
	public const string SURVIVAL_BONUS = "SURVIVAL_BONUS";
	public const string LEVEL_SPAWN_POINTS = "LEVEL_SPAWN_POINTS";
	public const string LEVEL_PROGRESSION = "LEVEL_PROGRESSION";

	// Metagame
	public const string EGGS = "EGGS";	
	public const string EGG_REWARDS = "EGG_REWARDS";
	public const string CHEST_REWARDS = "CHEST_REWARDS";
	public const string PREREG_REWARDS = "PREREG_REWARDS";
	public const string RARITIES = "RARITIES";
	public const string HUNGRY_LETTERS = "HUNGRY_LETTERS";
    // public const string INTERSTITIALS_PROFILES = "INTERSTITIALS_PROFILES";
    public const string INTERSTITIALS_SETUP = "INTERSTITIALS_SETUP";
	public const string DYNAMIC_GATCHA = "DYNAMIC_GATCHA";
	public const string LIVE_EVENTS_MODIFIERS = "LIVE_EVENTS_MODIFIERS";
    public const string LEAGUES = "LEAGUES";
	public const string DAILY_REWARDS = "DAILY_REWARDS";
    public const string DAILY_REWARD_MODIFIERS = "DAILY_REWARD_MODIFIERS";

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

    // Achievements
	public const string ACHIEVEMENTS = "ACHIEVEMENTS";

	// Economy
	public const string SHOP_PACKS = "SHOP_PACKS";
	public const string OFFER_PACKS = "OFFER_PACKS";
	public const string CURRENCY_TIERS = "CURRENCY_TIERS";

	// UI
	public const string SHARE_LOCATIONS = "SHARE_LOCATIONS";
    public const string ICONS = "ICONS";
};
    