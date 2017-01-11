﻿using UnityEngine;
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
		// kDefinitionFiles.Add(DefinitionsCategory.SETTINGS, );

		// Progression
		kDefinitionFiles.Add(DefinitionsCategory.LEVELS, new string[]{"Rules/levelDefinitions"});
		kDefinitionFiles.Add(DefinitionsCategory.MISSIONS, new string[]{"Rules/missionDefinitions"});
		kDefinitionFiles.Add(DefinitionsCategory.MISSION_TYPES, new string[]{"Rules/missionTypeDefinitions"});
		kDefinitionFiles.Add(DefinitionsCategory.MISSION_DIFFICULTIES, new string[]{"Rules/missionDifficultyDefinitions"});

		// Dragons
		kDefinitionFiles.Add(DefinitionsCategory.DRAGONS, new string[]{"Rules/dragonDefinitions"});
		kDefinitionFiles.Add(DefinitionsCategory.DRAGON_TIERS, new string[]{"Rules/dragonTierDefinitions"});
		kDefinitionFiles.Add(DefinitionsCategory.DRAGON_PROGRESSION, new string[]{"Rules/dragonProgressionDefinitions"});
		kDefinitionFiles.Add(DefinitionsCategory.DRAGON_HEALTH_MODIFIERS, new string[]{"Rules/dragonHealthModifiersDefinitions"});

		// Entites
		kDefinitionFiles.Add(DefinitionsCategory.PETS, 				new string[]{"Rules/petDefinitions"});
		kDefinitionFiles.Add(DefinitionsCategory.ENTITIES, 			new string[]{"Rules/entityDefinitions"});
		kDefinitionFiles.Add(DefinitionsCategory.DECORATIONS, 		new string[]{"Rules/decorationDefinitions"});
		kDefinitionFiles.Add(DefinitionsCategory.ENTITY_CATEGORIES, new string[]{"Rules/entityCategoryDefinitions"});

		// Game
		kDefinitionFiles.Add(DefinitionsCategory.SCORE_MULTIPLIERS, new string[]{"Rules/scoreMultiplierDefinitions"});
		kDefinitionFiles.Add(DefinitionsCategory.SURVIVAL_BONUS, new string[]{"Rules/survivalBonusDefinitions"});

		// Metagame
		kDefinitionFiles.Add(DefinitionsCategory.EGGS, new string[]{"Rules/eggDefinitions"});
		kDefinitionFiles.Add(DefinitionsCategory.EGG_REWARDS, new string[]{"Rules/eggRewardDefinitions"});
		kDefinitionFiles.Add(DefinitionsCategory.CHEST_REWARDS, new string[]{"Rules/chestRewardDefinitions"});
		kDefinitionFiles.Add(DefinitionsCategory.RARITIES, new string[]{"Rules/rarityDefinitions"});

		// Disguises
		kDefinitionFiles.Add(DefinitionsCategory.DISGUISES, new string[]{"Rules/disguisesDefinitions"});

		kDefinitionFiles.Add(DefinitionsCategory.HOLD_PREY_TIER, new string[]{"Rules/holdPreyTierSettingsDefinitions"});

		// Power Ups
		kDefinitionFiles.Add(DefinitionsCategory.POWERUPS, new string[]{"Rules/powerUpsDefinitions"});
        
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
	}
}
