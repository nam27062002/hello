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
    public static void InitContent(bool bAvoidDeltaContent = false)
	{
		Dictionary<string, string[]> kDefinitionFiles = new Dictionary<string,string[]>();

		// Settings
		kDefinitionFiles.Add(DefinitionsCategory.LOCALIZATION, new string[]{"Rules/localizationDefinitions"});
		kDefinitionFiles.Add(DefinitionsCategory.SETTINGS, new string[]{"Rules/gameSettings", "Rules/dragonSettings", "Rules/initialSettings"});
		kDefinitionFiles.Add(DefinitionsCategory.PARTICLE_MANAGER_SETTINGS_LEVEL_0_AREA1, new string[]{"Rules/PM_level_0_area1"});
		kDefinitionFiles.Add(DefinitionsCategory.PARTICLE_MANAGER_SETTINGS_LEVEL_0_AREA2, new string[]{"Rules/PM_level_0_area2"});
		// kDefinitionFiles.Add(DefinitionsCategory.SETTINGS, );

		// Progression
		kDefinitionFiles.Add(DefinitionsCategory.LEVELS, new string[]{"Rules/levelDefinitions"});
		kDefinitionFiles.Add(DefinitionsCategory.MISSIONS, new string[]{"Rules/missionsDefinitions"});
		kDefinitionFiles.Add(DefinitionsCategory.MISSION_TYPES, new string[]{"Rules/missionTypeDefinitions"});
		kDefinitionFiles.Add(DefinitionsCategory.MISSION_DIFFICULTIES, new string[]{"Rules/missionDifficultyDefinitions"});
		kDefinitionFiles.Add(DefinitionsCategory.MISSION_MODIFIERS, new string[]{
			"Rules/missionDifficultyModifiersDefinitions",
			"Rules/missionDragonModifiersDefinitions",
			"Rules/missionOtherModifiersDefinitions"
		});

		// Dragons
		kDefinitionFiles.Add(DefinitionsCategory.DRAGONS, new string[]{"Rules/dragonDefinitions"});
		kDefinitionFiles.Add(DefinitionsCategory.DRAGON_TIERS, new string[]{"Rules/dragonTierDefinitions"});
		kDefinitionFiles.Add(DefinitionsCategory.DRAGON_PROGRESSION, new string[]{"Rules/dragonProgressionDefinitions"});
		kDefinitionFiles.Add(DefinitionsCategory.DRAGON_HEALTH_MODIFIERS, new string[]{"Rules/dragonHealthModifiersDefinitions"});

		// Entites
		kDefinitionFiles.Add(DefinitionsCategory.PETS, 				new string[]{"Rules/petDefinitions"});
		kDefinitionFiles.Add(DefinitionsCategory.PET_MOVEMENT, 		new string[]{"Rules/petMovementDefinitions"});
		kDefinitionFiles.Add(DefinitionsCategory.ENTITIES, 			new string[]{"Rules/entityDefinitions"});
		kDefinitionFiles.Add(DefinitionsCategory.DECORATIONS, 		new string[]{"Rules/decorationDefinitions"});
		kDefinitionFiles.Add(DefinitionsCategory.ENTITY_CATEGORIES, new string[]{"Rules/entityCategoryDefinitions"});
		kDefinitionFiles.Add(DefinitionsCategory.FREEZE_CONSTANTS, 	new string[]{"Rules/freezeConstantDefinitions"});

		// Game
		kDefinitionFiles.Add(DefinitionsCategory.SCORE_MULTIPLIERS, new string[]{"Rules/scoreMultiplierDefinitions"});
		kDefinitionFiles.Add(DefinitionsCategory.SURVIVAL_BONUS, new string[]{"Rules/survivalBonusDefinitions"});

		// Metagame
		kDefinitionFiles.Add(DefinitionsCategory.EGGS, new string[]{"Rules/eggDefinitions"});
		kDefinitionFiles.Add(DefinitionsCategory.GOLDEN_EGGS, new string[]{"Rules/goldenEggDefinitions"});
		kDefinitionFiles.Add(DefinitionsCategory.EGG_REWARDS, new string[]{"Rules/eggRewardDefinitions"});
		kDefinitionFiles.Add(DefinitionsCategory.CHEST_REWARDS, new string[]{"Rules/chestRewardDefinitions"});
		kDefinitionFiles.Add(DefinitionsCategory.RARITIES, new string[]{"Rules/rarityDefinitions"});
		kDefinitionFiles.Add(DefinitionsCategory.HUNGRY_LETTERS, new string[]{"Rules/hungryLettersDefinitions"});
		kDefinitionFiles.Add(DefinitionsCategory.SHOP_PACKS, new string[]{"Rules/shopPacksDefinitions"});

		// Disguises
		kDefinitionFiles.Add(DefinitionsCategory.DISGUISES, new string[]{"Rules/disguisesDefinitions"});

		kDefinitionFiles.Add(DefinitionsCategory.HOLD_PREY_TIER, new string[]{"Rules/holdPreyTierSettingsDefinitions"});

		// Power Ups
		kDefinitionFiles.Add(DefinitionsCategory.POWERUPS, new string[]{"Rules/powerUpsDefinitions"});

        // Quality Settings
        kDefinitionFiles.Add(DefinitionsCategory.FEATURE_PROFILE_SETTINGS, new string[] { "Rules/featureProfileSettings" });
        kDefinitionFiles.Add(DefinitionsCategory.FEATURE_DEVICE_SETTINGS, new string[] { "Rules/featureDeviceSettings" });
        kDefinitionFiles.Add(DefinitionsCategory.DEVICE_RATING_SETTINGS, new string[] { "Rules/deviceRatingSettings" });

        // ADD HERE ANY NEW DEFINITIONS FILE!



        DefinitionsManager.SharedInstance.Initialise(ref kDefinitionFiles, bAvoidDeltaContent);
		m_ready = true;

		// Warn all other managers and definition consumers
		Messenger.Broadcast(EngineEvents.DEFINITIONS_LOADED);



        LocalizationManager.LanguageItemData[] kLanguagesData = null;
        Dictionary <string, DefinitionNode> kLanguagesSKUs = DefinitionsManager.SharedInstance.GetDefinitions (DefinitionsCategory.LOCALIZATION);
        if (kLanguagesSKUs.Count > 0)
        {
            kLanguagesData = new LocalizationManager.LanguageItemData[kLanguagesSKUs.Count];

            int iSKUIdx = 0;

            foreach(KeyValuePair<string, DefinitionNode> kEntry in kLanguagesSKUs)
            {
                kLanguagesData[iSKUIdx] = new LocalizationManager.LanguageItemData();

                kLanguagesData[iSKUIdx].m_strSKU = kEntry.Value.Get("sku");

                int iOrder = 0;
                if (int.TryParse(kEntry.Value.Get("order"), out iOrder))
                {
                    kLanguagesData[iSKUIdx].m_iOrder = iOrder;
                }

                kLanguagesData[iSKUIdx].m_strISOCode  = kEntry.Value.Get("isoCode");

                bool bInAndroid = false;
                if (bool.TryParse(kEntry.Value.Get("android"), out bInAndroid))
                {
                    kLanguagesData[iSKUIdx].m_bInAndroid = bInAndroid;
                }

                bool bInIOS = false;
                if (bool.TryParse(kEntry.Value.Get("iOS"), out bInIOS))
                {
                    kLanguagesData[iSKUIdx].m_bInIOS = bInIOS;
                }
                    
                kLanguagesData[iSKUIdx].m_strLanguageFile = kEntry.Value.Get("txtFilename");
                kLanguagesData[iSKUIdx].m_strLanguageTID = kEntry.Value.Get("tidName");

                iSKUIdx++;
            }
        }

        LocalizationManager.SharedInstance.Initialise (ref kLanguagesData, "lang_english", "Localization");
		LocalizationManager.SharedInstance.debugMode = (LocalizationManager.DebugMode)PlayerPrefs.GetInt(DebugSettings.LOCALIZATION_DEBUG_MODE);	// [AOC] Initialize localization manager debug mode
	}
}
