﻿using UnityEngine;
using System.Collections.Generic;
public class ContentManager  
{
    //////////////////////////////////////////////////////////////////////////

    private class ContentDeltaDelegate : ContentDeltaManager.ContentDeltaListener
    {        
        public override void onContentDeltaInitialised(bool bWasSuccessful)
        {            
            if (FeatureSettingsManager.IsDebugEnabled)
                Log("onContentDeltaInitialised: succeded: " + bWasSuccessful);
        }

        public override void onContentDeltaAllDownloaded(bool bWasSuccessful, long iDownloadedSize)
        {
            if (FeatureSettingsManager.IsDebugEnabled)            
                Log("onContentDeltaAllDownloaded: succeded: " + bWasSuccessful);

            if (bWasSuccessful)
            {
                /*if (FeatureSettingsManager.IsDebugEnabled)
                {
                    string tid = "TID_PLAY";
                    DefinitionNode _def = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.SETTINGS, "initialSettings");
                    int hardCurrency = _def.GetAsInt("hardCurrency");
                    Log("Before incentiviseFBLogin = " + PersistenceFacade.Rules_GetPCAmountToIncentivizeSocial() + " TID_PLAY = " + LocalizationManager.SharedInstance.Get(tid) + " hardCurrency = " + hardCurrency);
                }*/

                DefinitionsManager.SharedInstance.Reload();
                LocalizationManager.SharedInstance.ReloadLanguage();

                /*if (FeatureSettingsManager.IsDebugEnabled)
                {
                    string tid = "TID_PLAY";
                    DefinitionNode _def = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.SETTINGS, "initialSettings");
                    int hardCurrency = _def.GetAsInt("hardCurrency");
                    Log("After incentiviseFBLogin = " + PersistenceFacade.Rules_GetPCAmountToIncentivizeSocial() + " TID_PLAY = " + LocalizationManager.SharedInstance.Get(tid) + " hardCurrency = " + hardCurrency);
                }*/
            }            
        }

        public override void onContentReady()
        {
            if (FeatureSettingsManager.IsDebugEnabled)
                Log("OnContentReady" );

            OnContentReady();
        }
    }

    private static ContentDeltaDelegate m_kContentDeltaDelegate = null;

    //////////////////////////////////////////////////////////////////////////

    public static bool m_ready = false;
	public static bool ready
	{
		get
		{
			return m_ready;
		}
	}

    public static bool UseCachedAssetsLUTFromServer 
    {
        get
        {                        
#if UNITY_EDITOR
            return false;
#else
            return FeatureSettingsManager.instance.IsContentDeltasCachedEnabled;
#endif
        }
    }

    public static bool UseDeltaContent
    {
        get
        {                     
#if UNITY_EDITOR
            return false;
#else
            return FeatureSettingsManager.instance.IsContentDeltasEnabled;
#endif
        }
    }

	public static void InitContent(bool bAvoidDeltaContent = false, bool _configureServerManager = true)
	{
		if (_configureServerManager) {
        	GameServerManager.SharedInstance.Configure();
		}

        InitDefinitions();
        InitLanguages();

		if (_configureServerManager) {
	        // Content Delta Manager has to be initialised regardless 'UseDeltaContent' because it initialises the version that is sent as the X-Version of the client in all commands sent to the server
	        InitContentDeltaManager();
		}

        if (UseDeltaContent)
		{            
            ContentDeltaManager.SharedInstance.RequestAssetsLUT(ServerManager.SharedInstance.GetServerConfig().m_strApplicationParole);
        }  
        else
        {
            OnContentReady();
        }       
	}

    private static void InitContentDeltaManager()
    {
        m_kContentDeltaDelegate = new ContentDeltaDelegate();
        ContentDeltaManager.SharedInstance.SetListener(m_kContentDeltaDelegate);
        ContentDeltaManager.SharedInstance.Initialise("AssetsLUT/assetsLUT", UseCachedAssetsLUTFromServer);                
    }

    private static void InitDefinitions()
    {
        Dictionary<string, string[]> kDefinitionFiles = new Dictionary<string, string[]>();

        // Settings
        kDefinitionFiles.Add(DefinitionsCategory.LOCALIZATION, new string[] { "Rules/localizationDefinitions" });
		kDefinitionFiles.Add(DefinitionsCategory.FONT_GROUPS, new string[] { "Rules/fontGroupsDefinitions" });
        kDefinitionFiles.Add(DefinitionsCategory.SETTINGS, new string[]{
            "Rules/gameSettings",
            "Rules/dragonSettings",
            "Rules/initialSettings",
            "Rules/missingRessourcesVariablesDefinitions"
        });

		kDefinitionFiles.Add(DefinitionsCategory.SEASONS, new string[] { "Rules/seasonsDefinitions" });

        kDefinitionFiles.Add(DefinitionsCategory.PARTICLE_MANAGER_SETTINGS_LEVEL_0_AREA1, new string[] { "Rules/PM_level_0_area1" });
        kDefinitionFiles.Add(DefinitionsCategory.PARTICLE_MANAGER_SETTINGS_LEVEL_0_AREA2, new string[] { "Rules/PM_level_0_area2" });
        kDefinitionFiles.Add(DefinitionsCategory.PARTICLE_MANAGER_SETTINGS_LEVEL_0_AREA3, new string[] { "Rules/PM_level_0_area3" });
		kDefinitionFiles.Add(DefinitionsCategory.POOL_MANAGER_SETTINGS_LEVEL_0_AREA2, new string[] { "Rules/NPC_Pools_level_0_area2" });
		kDefinitionFiles.Add(DefinitionsCategory.POOL_MANAGER_SETTINGS_LEVEL_0_AREA1, new string[] { "Rules/NPC_Pools_level_0_area1" });
		kDefinitionFiles.Add(DefinitionsCategory.POOL_MANAGER_SETTINGS_LEVEL_0_AREA3, new string[] { "Rules/NPC_Pools_level_0_area3" });

        // kDefinitionFiles.Add(DefinitionsCategory.SETTINGS, );

        // Progression
        kDefinitionFiles.Add(DefinitionsCategory.LEVELS, new string[] { "Rules/levelDefinitions" });

        // Missions
        kDefinitionFiles.Add(DefinitionsCategory.MISSIONS, new string[] { "Rules/missionsDefinitions" });
        kDefinitionFiles.Add(DefinitionsCategory.MISSION_TYPES, new string[] { "Rules/missionTypeDefinitions" });
        kDefinitionFiles.Add(DefinitionsCategory.MISSION_DIFFICULTIES, new string[] { "Rules/missionDifficultyDefinitions" });
        kDefinitionFiles.Add(DefinitionsCategory.MISSION_MODIFIERS, new string[]{
            "Rules/missionDifficultyModifiersDefinitions",
            "Rules/missionDragonModifiersDefinitions",
            "Rules/missionOtherModifiersDefinitions"
        });

        // Dragons
        kDefinitionFiles.Add(DefinitionsCategory.DRAGONS, new string[] { "Rules/dragonDefinitions" });
        kDefinitionFiles.Add(DefinitionsCategory.DRAGON_TIERS, new string[] { "Rules/dragonTierDefinitions" });
        kDefinitionFiles.Add(DefinitionsCategory.DRAGON_PROGRESSION, new string[] { "Rules/dragonProgressionDefinitions" });
        kDefinitionFiles.Add(DefinitionsCategory.DRAGON_HEALTH_MODIFIERS, new string[] { "Rules/dragonHealthModifiersDefinitions" });

        // Entites
        kDefinitionFiles.Add(DefinitionsCategory.PETS, new string[] { "Rules/petDefinitions" });
        kDefinitionFiles.Add(DefinitionsCategory.PET_MOVEMENT, new string[] { "Rules/petMovementDefinitions" });
        kDefinitionFiles.Add(DefinitionsCategory.PET_CATEGORIES, new string[] { "Rules/petCategoryDefinitions" });
        kDefinitionFiles.Add(DefinitionsCategory.ENTITIES, new string[] { "Rules/entityDefinitions" });
        kDefinitionFiles.Add(DefinitionsCategory.DECORATIONS, new string[] { "Rules/decorationDefinitions" });
        kDefinitionFiles.Add(DefinitionsCategory.ENTITY_CATEGORIES, new string[] { "Rules/entityCategoryDefinitions" });
        kDefinitionFiles.Add(DefinitionsCategory.FREEZE_CONSTANTS, new string[] { "Rules/freezeConstantDefinitions" });

        // Game
        kDefinitionFiles.Add(DefinitionsCategory.SCORE_MULTIPLIERS, new string[] { "Rules/scoreMultiplierDefinitions" });
        kDefinitionFiles.Add(DefinitionsCategory.SURVIVAL_BONUS, new string[] { "Rules/survivalBonusDefinitions" });

        // Metagame
        kDefinitionFiles.Add(DefinitionsCategory.EGGS, new string[] { "Rules/eggDefinitions" });
        kDefinitionFiles.Add(DefinitionsCategory.GOLDEN_EGGS, new string[] { "Rules/goldenEggDefinitions" });
        kDefinitionFiles.Add(DefinitionsCategory.EGG_REWARDS, new string[] { "Rules/eggRewardDefinitions" });
        kDefinitionFiles.Add(DefinitionsCategory.CHEST_REWARDS, new string[] { "Rules/chestRewardDefinitions" });
		kDefinitionFiles.Add(DefinitionsCategory.PREREG_REWARDS, new string[] { "Rules/preRegRewardsDefinitions" });
        kDefinitionFiles.Add(DefinitionsCategory.RARITIES, new string[] { "Rules/rarityDefinitions" });
        kDefinitionFiles.Add(DefinitionsCategory.HUNGRY_LETTERS, new string[] { "Rules/hungryLettersDefinitions" });

		kDefinitionFiles.Add(DefinitionsCategory.DYNAMIC_GATCHA, new string[] {"Rules/dynamicGatchaDefinition"});
		kDefinitionFiles.Add(DefinitionsCategory.LIVE_EVENTS_MODIFIERS, new string[] {"Rules/modsDefinitions"});

        // Disguises
        kDefinitionFiles.Add(DefinitionsCategory.DISGUISES, new string[] { "Rules/disguisesDefinitions" });

        kDefinitionFiles.Add(DefinitionsCategory.HOLD_PREY_TIER, new string[] { "Rules/holdPreyTierSettingsDefinitions" });

        // Power Ups
        kDefinitionFiles.Add(DefinitionsCategory.POWERUPS, new string[] { "Rules/powerUpsDefinitions" });

        // Quality Settings
        kDefinitionFiles.Add(DefinitionsCategory.FEATURE_PROFILE_SETTINGS, new string[] { "Rules/featureProfileSettings" });
        kDefinitionFiles.Add(DefinitionsCategory.FEATURE_DEVICE_SETTINGS, new string[] { "Rules/featureDeviceSettings" });
        kDefinitionFiles.Add(DefinitionsCategory.DEVICE_RATING_SETTINGS, new string[] { "Rules/deviceRatingSettings" });

        // Achievements
        kDefinitionFiles.Add(DefinitionsCategory.ACHIEVEMENTS, new string[] { "Rules/achievementsDefinitions" });

        // Economy
        kDefinitionFiles.Add(DefinitionsCategory.SHOP_PACKS, new string[]{"Rules/shopPacksDefinitions"});
		kDefinitionFiles.Add(DefinitionsCategory.OFFER_PACKS, new string[]{"Rules/offerPacksDefinitions"});
        kDefinitionFiles.Add(DefinitionsCategory.CURRENCY_TIERS, new string[]{"Rules/missingRessourcesTiersDefinitions"});

        // ADD HERE ANY NEW DEFINITIONS FILE!



        DefinitionsManager.SharedInstance.Initialise(ref kDefinitionFiles, !UseDeltaContent);
    }

    private static void InitLanguages()
    {
        LocalizationManager.LanguageItemData[] kLanguagesData = null;
        Dictionary<string, DefinitionNode> kLanguagesSKUs = DefinitionsManager.SharedInstance.GetDefinitions(DefinitionsCategory.LOCALIZATION);
        if (kLanguagesSKUs.Count > 0)
        {
            kLanguagesData = new LocalizationManager.LanguageItemData[kLanguagesSKUs.Count];

            int iSKUIdx = 0;

            foreach (KeyValuePair<string, DefinitionNode> kEntry in kLanguagesSKUs)
            {
                kLanguagesData[iSKUIdx] = new LocalizationManager.LanguageItemData();

                kLanguagesData[iSKUIdx].m_strSKU = kEntry.Value.Get("sku");

                int iOrder = 0;
                if (int.TryParse(kEntry.Value.Get("order"), out iOrder))
                {
                    kLanguagesData[iSKUIdx].m_iOrder = iOrder;
                }

                kLanguagesData[iSKUIdx].m_strISOCode = kEntry.Value.Get("isoCode");

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

        LocalizationManager.SharedInstance.Initialise(ref kLanguagesData, "lang_english", "Localization");
        LocalizationManager.SharedInstance.debugMode = (LocalizationManager.DebugMode)PlayerPrefs.GetInt(DebugSettings.LOCALIZATION_DEBUG_MODE);	// [AOC] Initialize localization manager debug mode
    }

    private static void OnContentReady()
    {
        m_ready = true;

        // Warn all other managers and definition consumers
		Messenger.Broadcast(MessengerEvents.DEFINITIONS_LOADED);
    }

    #region log
    private const bool LOG_USE_COLOR = false;
    private const string LOG_CHANNEL = "[ContentManager] ";    

    public static void Log(string msg)
    {
        msg = LOG_CHANNEL + msg;
        if (LOG_USE_COLOR)
        {
            msg = "<color=cyan>" + msg + " </color>";
        }
        
        Debug.Log(msg);        
    }

    public static void LogError(string msg)
    {
        Debug.LogError(LOG_CHANNEL + msg);
    }
    #endregion
}
