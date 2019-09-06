// SeasonManager.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 27/11/2017.
// Copyright (c) 2017 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Globalization;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Simple manager to centralize seasonal stuff ("christmas", "halloween", etc).
/// </summary>
public class SeasonManager : Singleton<SeasonManager> {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	public const string NO_SEASON_SKU = "none";
	private const string ACTIVE_SEASON_CACHE_KEY = "SeasonManager.activeSeasonSku";

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	private string m_activeSeason = NO_SEASON_SKU;
	public static string activeSeason {
        get {
            SeasonManager sm = instance;
            if (sm != null) {
                return sm.m_activeSeason;
            } else {
                return "";
            }
        } 
	}

	//------------------------------------------------------------------------//
	// METHODS																  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Default constructor.
	/// </summary>
	public SeasonManager() {
		// Initialize current season
		RefreshActiveSeason();
	}

	/// <summary>
	/// Refresh current season taking ControlPanel in account.
	/// </summary>
	public void RefreshActiveSeason() {
		string oldSeason = m_activeSeason;
		string forcedSeasonSku = CPSeasonSelector.forcedSeasonSku;
		switch(forcedSeasonSku) {
			// Default behaviour
			case CPSeasonSelector.DEFAULT_SEASON_SKU: {
				// Load active season from content
				// If content is not ready, load it from cache
				if(!ContentManager.ready) {
					// Load from cache
					m_activeSeason = PlayerPrefs.GetString(ACTIVE_SEASON_CACHE_KEY, NO_SEASON_SKU);
				} else {
					// Load from content - first definition with the "active" field set to true
					DefinitionNode activeSeasonDef = null;
					List<DefinitionNode> seasonDefs = DefinitionsManager.SharedInstance.GetDefinitionsList(DefinitionsCategory.SEASONS);
					for(int i = 0; i < seasonDefs.Count && activeSeasonDef == null; ++i) {
						if(seasonDefs[i].GetAsBool("active")) {
							activeSeasonDef = seasonDefs[i];

							/*// [AOC] TODO!! Whenever the addressable groups are implemented
							// Make sure required asset groups for this season are downloaded and ready
							List<string> requiredAssetGroups = activeSeasonDef.GetAsList<string>("requiredAssetGroups");
							if(!HDAddressablesManager.Instance.IsResourceGroupListAvailable(requiredAssetGroups)) {
							
								// Not all assets are available, don't trigger this season
								activeSeasonDef = null;
							}*/
						}
					}

					// Store new season
					m_activeSeason = activeSeasonDef == null ? NO_SEASON_SKU : activeSeasonDef.sku;

					// Cache for future use
					PlayerPrefs.SetString(ACTIVE_SEASON_CACHE_KEY, m_activeSeason);
				}
			} break;

			// No season
			case NO_SEASON_SKU: {
				m_activeSeason = NO_SEASON_SKU;
			} break;

			// Some other season
			default: {
				// Directly retrieve target season def from content
				DefinitionNode activeSeasonDef = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.SEASONS, forcedSeasonSku);

				// If selected season wasn't found, reset cheat
				if(activeSeasonDef == null) {
					CPSeasonSelector.forcedSeasonSku = CPSeasonSelector.DEFAULT_SEASON_SKU;
					RefreshActiveSeason();
				} else {
					m_activeSeason = activeSeasonDef.sku;
				}
			} break;
		}

		// Broadcast event
		if(oldSeason != m_activeSeason) {
			SeasonChangedEventInfo eventInfo = new SeasonChangedEventInfo();
			eventInfo.oldSeasonSku = oldSeason;
			eventInfo.newSeasonSku = m_activeSeason;
			Broadcaster.Broadcast(BroadcastEventType.SEASON_CHANGED, eventInfo);
		}
	}

	public static bool IsSeasonActive()
	{
		return activeSeason != NO_SEASON_SKU;
	}
    
    public static bool IsNewYear()
    {
        bool ret = false;
        System.DateTime dateTime = System.DateTime.Now;
        
        // New Year
        if ( (dateTime.Day >= 31 && dateTime.Month >= 12) || (dateTime.Day <= 1 && dateTime.Month <= 1 ))
        {
            ret = true;
        }
        return ret;
    }
    
    public static bool IsChineseNewYear()
    {
        bool ret = false;
        
        /*
        // Do not delete this. With the new .Net version at some point should work
        ChineseLunisolarCalendar chinese   = new ChineseLunisolarCalendar();
        GregorianCalendar        gregorian = new GregorianCalendar();

        DateTime utcNow = DateTime.Now;
        // Get Chinese New Year of current UTC date/time
        DateTime chineseNewYear = chinese.ToDateTime( utcNow.Year, 1, 1, 0, 0, 0, 0 );
        // Convert back to Gregorian (you could just query properties of `chineseNewYear` directly, but I prefer to use `GregorianCalendar` for consistency:
        // Int32 year  = gregorian.GetYear( chineseNewYear );
        // Int32 month = gregorian.GetMonth( chineseNewYear );
        // Int32 day   = gregorian.GetDayOfMonth( chineseNewYear );
        
        int chineseDay = gregorian.GetDayOfYear( chineseNewYear );

        ret = (utcNow.DayOfYear > (chineseDay - 4)) && (utcNow.DayOfYear <= chineseDay);
        */
        
        System.DateTime dateTime = System.DateTime.Now;
        ret = dateTime.Month == 2 && dateTime.Day >= 4 && dateTime.Day <= 10;
        return ret;
        
    }
    
    public static bool IsFireworksDay()
    {
        bool ret = false;
        ret = DebugSettings.specialDates;
        ret = ret || IsNewYear() || IsChineseNewYear();
        return ret;
    }

	public static string GetBloodParticlesName() 
	{
		DefinitionNode activeSeasonDef = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.SEASONS, activeSeason);

		// If selected season wasn't found, reset cheat
		if(activeSeasonDef == null) {
			return null;
		} else {
			return activeSeasonDef.GetAsString("bloodParticles", null);
		}
	}
}