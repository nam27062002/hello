using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class ContentManager  
{
	public static bool m_ready = false;
	public static bool ready
	{
		get
		{
			return m_ready;
		}
	}
	public static void InitContent()
	{
		Dictionary<string, string[]> kDefinitionFiles = new Dictionary<string,string[]>();

		// Settings
		kDefinitionFiles.Add(DefinitionsCategory.LOCALIZATION, new string[]{"Rules/localizationDefinitions"});
		kDefinitionFiles.Add(DefinitionsCategory.SETTINGS, new string[]{"Rules/gameSettings", "Rules/dragonSettings"});
		// kDefinitionFiles.Add(DefinitionsCategory.SETTINGS, );

		// Progression
		kDefinitionFiles.Add(DefinitionsCategory.LEVELS, new string[]{"Rules/levelDefinitions"});
		kDefinitionFiles.Add(DefinitionsCategory.MISSIONS, new string[]{"Rules/missionDefinitions"});
		kDefinitionFiles.Add(DefinitionsCategory.MISSION_TYPES, new string[]{"Rules/missionTypeDefinitions"});
		kDefinitionFiles.Add(DefinitionsCategory.MISSION_DIFFICULTIES, new string[]{"Rules/missionDifficultyDefinitions"});

		// Dragons
		kDefinitionFiles.Add(DefinitionsCategory.DRAGONS, new string[]{"Rules/dragonDefinitions"});
		kDefinitionFiles.Add(DefinitionsCategory.DRAGON_TIERS, new string[]{"Rules/dragonTierDefinitions"});
		kDefinitionFiles.Add(DefinitionsCategory.DRAGON_SKILLS, new string[]{"Rules/dragonSkillDefinitions", "Rules/dragonSkillProgressionDefinitions"});

		// Entites
		kDefinitionFiles.Add(DefinitionsCategory.ENTITIES, new string[]{"Rules/entityDefinitions"});
		kDefinitionFiles.Add(DefinitionsCategory.ENTITY_CATEGORIES, new string[]{"Rules/entityCategoryDefinitions"});

		// Game
		kDefinitionFiles.Add(DefinitionsCategory.SCORE_MULTIPLIERS, new string[]{"Rules/scoreMultiplierDefinitions"});
		kDefinitionFiles.Add(DefinitionsCategory.SURVIVAL_BONUS, new string[]{"Rules/survivalBonusDefinitions"});

		// Metagame
		kDefinitionFiles.Add(DefinitionsCategory.EGGS, new string[]{"Rules/eggDefinitions"});
		kDefinitionFiles.Add(DefinitionsCategory.EGG_REWARDS, new string[]{"Rules/eggRewardDefinitions"});
		kDefinitionFiles.Add(DefinitionsCategory.CHEST_REWARDS, new string[]{"Rules/chestRewardDefinitions"});

		// Disguises
		kDefinitionFiles.Add(DefinitionsCategory.DISGUISES, new string[]{"Rules/disguisesDefinitions"});
		kDefinitionFiles.Add(DefinitionsCategory.DISGUISES_EQUIP, new string[]{"Rules/disguiseEquipDefinitions"});
		kDefinitionFiles.Add(DefinitionsCategory.DISGUISES_POWERUPS, new string[]{"Rules/disguisePowerUpsDefinitions"});

		kDefinitionFiles.Add(DefinitionsCategory.FIRE_SPAWN_EFFECTS, new string[]{"Rules/spawnersDragonBurnDefinitions"});
		kDefinitionFiles.Add(DefinitionsCategory.FIRE_DECORATION_EFFECTS, new string[]{"Rules/entityDragonBurnDefinitions"});

		kDefinitionFiles.Add(DefinitionsCategory.HOLD_PREY_TIER, new string[]{"Rules/holdPreyTierSettingsDefinitions"});

		// Power Ups
		kDefinitionFiles.Add(DefinitionsCategory.POWERUPS, new string[]{"Rules/powerUpsDefinitions"});

		// ADD HERE ANY NEW DEFINITIONS FILE!



		List<string> kRulesListToCalculateCRC = new List<string>();
		DefinitionsManager.SharedInstance.Initialise(ref kDefinitionFiles, ref kRulesListToCalculateCRC );
		m_ready = true;
		// Warn all other managers and definition consumers
		Messenger.Broadcast(EngineEvents.DEFINITIONS_LOADED);
	}
}
