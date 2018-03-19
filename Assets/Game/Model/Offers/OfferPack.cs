// OfferPack.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 20/02/2018.
// Copyright (c) 2018 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using System;
using System.Collections.Generic;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Representation of a single offer pack.
/// </summary>
[Serializable]
public class OfferPack {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	public enum WhereToShow {
		SHOP_ONLY,
		DRAGON_SELECTION,
		DRAGON_SELECTION_AFTER_RUN,
		PLAY_SCREEN
	}

	public enum PayerType {
		ANYONE,
		PAYER,
		NON_PAYER
	}
	
	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	private DefinitionNode m_def = null;
	public DefinitionNode def {
		get { return m_def; }
	}

	private List<OfferPackItem> m_items = new List<OfferPackItem>();
	public List<OfferPackItem> items {
		get { return m_items; }
	}

	// Pack setup
	// See https://mdc-web-tomcat17.ubisoft.org/confluence/display/ubm/%5BHD%5D+Offers+1.0#id-[HD]Offers1.0-Segmentationdetails
	// Mandatory params
	private Version m_minAppVersion = new Version();
	public Version minAppVersion {
		get { return m_minAppVersion; }
	}

	private int m_order = 0;
	public int order {
		get { return m_order; }
	}

	// Only required for Featured packs
	private bool m_featured = false;
	public bool featured {
		get { return m_featured; }
	}

	private int m_frequency = 0;
	private int m_maxViews = 0;
	private WhereToShow m_whereToShow = WhereToShow.SHOP_ONLY;

	private DateTime m_startDate = new DateTime();
	public DateTime startDate {
		get { return m_startDate; }
	}

	private DateTime m_endDate = new DateTime();
	public DateTime endDate {
		get { return m_endDate; }
	}

	private bool m_isTimed = false;
	public bool isTimed {
		get { return m_isTimed; }
	}

	// Optional params
	private string[] m_countriesAllowed = new string[0];
	private string[] m_countriesExcluded = new string[0];
	private int m_gamesPlayed = 0;
	private PayerType m_payerType = PayerType.ANYONE;
	private float m_minSpent = 0f;
	private string[] m_dragonOwned = new string[0];
	private string[] m_dragonUnlocked = new string[0];
	private Range m_scBalanceRange = new Range(0, float.MaxValue);
	private Range m_hcBalanceRange = new Range(0, float.MaxValue);
	private int m_openedEggs = 0;
	private int m_petsOwnedCount = 0;
	private string[] m_petsOwned = new string[0];
	private RangeInt m_progressionRange = new RangeInt();
	private string[] m_skinsUnlocked = new string[0];
	private string[] m_skinsOwned = new string[0];

	// Internal vars
	private int m_viewsCount = 0;	// Only auto-triggered views
	private DateTime m_lastViewTimestamp = new DateTime();

	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Default constructor.
	/// </summary>
	public OfferPack() {

	}

	/// <summary>
	/// Destructor
	/// </summary>
	~OfferPack() {

	}

	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialize this offer pack with the given definition.
	/// </summary>
	/// <param name="_def">Definition to be parsed.</param>
	public void InitFromDefinition(DefinitionNode _def) {
		// Reset to default values
		Reset();

		// Store def
		m_def = _def;

		// Parse definition
		m_minAppVersion = Version.Parse(_def.Get("minAppVersion"));
		m_order = _def.GetAsInt("order", 0);

		// Items
		List<DefinitionNode> itemDefs = _def.GetChildNodesByTag("Item");
		for(int i = 0; i < itemDefs.Count; ++i) {
			OfferPackItem item = new OfferPackItem();
			item.InitFromDefinition(itemDefs[i]);
			m_items.Add(item);
		}

		// Params
		List<DefinitionNode> paramDefs = _def.GetChildNodesByTag("Param");
		const string KEY = "paramValue";
		DefinitionNode def = null;
		for(int i = 0; i < paramDefs.Count; ++i) {
			def = paramDefs[i];
			switch(def.sku) {
				// Only for featured packs
				case "featured":	m_featured = def.GetAsBool(KEY);	break;
				case "frequency": 	m_frequency = def.GetAsInt(KEY);	break;
				case "maxViews": 	m_maxViews = def.GetAsInt(KEY);		break;
				case "zone": {
					switch(def.Get(KEY)) {
						case "dragonSelection":			m_whereToShow = WhereToShow.DRAGON_SELECTION;			break;
						case "dragonSelectionAfterRun":	m_whereToShow = WhereToShow.DRAGON_SELECTION_AFTER_RUN;	break;
						case "playScreen":				m_whereToShow = WhereToShow.PLAY_SCREEN;				break;
					}
				} break;
				case "startDate":	m_startDate = TimeUtils.TimestampToDate(def.GetAsLong(KEY) * 1000);	break;
				case "endDate": {
					m_endDate = TimeUtils.TimestampToDate(def.GetAsLong(KEY) * 1000);
					m_isTimed = true;
				} break;

				// Optional params
				case "countriesAllowed": 	m_countriesAllowed = def.GetAsArray<string>(KEY);	break;
				case "countriesExcluded": 	m_countriesExcluded = def.GetAsArray<string>(KEY);	break;
				case "gamesPlayed": 		m_gamesPlayed = def.GetAsInt(KEY);					break;
				case "payerType": {
					switch(def.Get(KEY)) {
						case "payer":		m_payerType = PayerType.PAYER;			break;
						case "nonPayer":	m_payerType = PayerType.NON_PAYER;		break;
					}
				} break;
				case "minSpent": 		m_minSpent = def.GetAsFloat(KEY);					break;
				case "dragonOwned": 	m_dragonOwned = def.GetAsArray<string>(KEY);		break;
				case "dragonUnlocked": 	m_dragonUnlocked = def.GetAsArray<string>(KEY);		break;
				case "scBalanceRange":	m_scBalanceRange = ParseRange(def.Get(KEY));		break;
				case "hcBalanceRange":	m_hcBalanceRange = ParseRange(def.Get(KEY));		break;
				case "openedEggs":		m_openedEggs = def.GetAsInt(KEY);					break;
				case "petsOwnedCount":	m_petsOwnedCount = def.GetAsInt(KEY);				break;
				case "petsOwned":		m_petsOwned = def.GetAsArray<string>(KEY);			break;
				case "progressionRange": m_progressionRange = ParseRangeInt(def.Get(KEY));	break;
				case "skinsUnlocked":	m_skinsUnlocked = def.GetAsArray<string>(KEY);		break;
				case "skinsOwned":		m_skinsOwned = def.GetAsArray<string>(KEY);			break;
			}
		}

		// Featured offers are always timed
		if(m_featured) m_isTimed = true;
	}

	/// <summary>
	/// Reset to default values.
	/// </summary>
	public void Reset() {
		// Items and def
		m_items.Clear();
		m_def = null;

		// Mandatory params
		m_minAppVersion.Set(0, 0, 0);
		m_order = 0;

		// Mandatory for featured packs
		m_featured = false;
		m_isTimed = false;
		m_frequency = 0;
		m_maxViews = 0;
		m_whereToShow = WhereToShow.SHOP_ONLY;
		m_startDate = DateTime.MinValue;
		m_endDate = DateTime.MinValue;

		// Optional params
		m_countriesAllowed = new string[0];
		m_countriesExcluded = new string[0];
		m_gamesPlayed = 0;
		m_payerType = PayerType.ANYONE;
		m_minSpent = 0f;
		m_dragonOwned = new string[0];
		m_dragonUnlocked = new string[0];
		m_scBalanceRange = new Range(0, float.MaxValue);
		m_hcBalanceRange = new Range(0, float.MaxValue);
		m_openedEggs = 0;
		m_petsOwnedCount = 0;
		m_petsOwned = new string[0];
		m_progressionRange = new RangeInt();
		m_skinsUnlocked = new string[0];
		m_skinsOwned = new string[0];

		// Internal vars
		m_viewsCount = 0;
		m_lastViewTimestamp = new DateTime();
	}

	/// <summary>
	/// Validate all segmentation parameters to see if the offer can be displayed to the current player.
	/// </summary>
	/// <returns><c>true</c> if this offer can be displayed to the current player, <c>false</c> otherwise.</returns>
	public bool CanBeDisplayed() {
		return CanBeApplied();
	}

	/// <summary>
	/// Check whether the featured popup should be displayed or not, and do it.
	/// </summary>
	/// <returns><c>true</c> if all conditions to display the popup are met and the popup will be opened, <c>false</c> otherwise.</returns>
	/// <param name="_areaToCheck">Area to check.</param>
	public bool ShowPopupIfPossible(WhereToShow _areaToCheck) {
		// Not if not featured
		if(!m_featured) return false;

		// Not if segmentation fails
		if(!CanBeDisplayed()) return false;

		// Check max views
		if(m_viewsCount >= m_maxViews) return false;

		// Check area
		if(m_whereToShow == WhereToShow.DRAGON_SELECTION) {
			// Special case for dragon selection screen: count both initial time and after run
			if(_areaToCheck != WhereToShow.DRAGON_SELECTION && _areaToCheck != WhereToShow.DRAGON_SELECTION_AFTER_RUN) return false;
		} else {
			// Standard case: just make sure we're in the right place
			if(_areaToCheck != m_whereToShow) return false;
		}

		// Check frequency
		DateTime serverTime = GameServerManager.SharedInstance.GetEstimatedServerTime();
		TimeSpan timeSinceLastView = serverTime - m_lastViewTimestamp;
		if(timeSinceLastView.TotalMinutes < m_frequency) return false;

		// All checks passed!
		// Show popup
		PopupManager.OpenPopupInstant(PopupFeaturedOffer.PATH);

		// Update control vars and return
		m_viewsCount++;
		m_lastViewTimestamp = serverTime;
		return true;
	}

	/// <summary>
	/// Validate that the pack can still be applied!
	/// </summary>
	/// <returns><c>true</c> if this pack can be applied; otherwise, <c>false</c>.</returns>
	public bool CanBeApplied() {
		// Aux vars
		UserProfile profile = UsersManager.currentUser;

		// Main conditions
		if(m_minAppVersion > GameSettings.internalVersion) return false;

		// If featured, check extra conditions
		if(m_featured || m_isTimed) {
			DateTime serverTime = GameServerManager.SharedInstance.GetEstimatedServerTime();
			if(serverTime < m_startDate) return false;
			if(serverTime > m_endDate) return false;
		}

		// Payer profile
		if(m_gamesPlayed > profile.gamesPlayed) return false;
		int totalPurchases = (HDTrackingManager.Instance.TrackingPersistenceSystem == null) ? 0 : HDTrackingManager.Instance.TrackingPersistenceSystem.TotalPurchases;
		if(m_payerType == PayerType.PAYER && totalPurchases == 0) return false;
		if(m_payerType == PayerType.NON_PAYER && totalPurchases > 0) return false;
		//if(m_minSpent > TODO!!) return false;	// [AOC] TODO!! We don't store total spent in the client. Wait for CRM segmentation
		if(!m_scBalanceRange.Contains((float)profile.coins)) return false;
		if(!m_hcBalanceRange.Contains((float)profile.pc)) return false;
		if(m_openedEggs > profile.eggsCollected) return false;
		if(!m_progressionRange.Contains(profile.GetPlayerProgress())) return false;

		// Dragons
		for(int i = 0; i < m_dragonOwned.Length; ++i) {
			if(!DragonManager.IsDragonOwned(m_dragonOwned[i])) return false;
		}
		for(int i = 0; i < m_dragonUnlocked.Length; ++i) {
			if(DragonManager.GetDragonData(m_dragonUnlocked[i]).lockState <= DragonData.LockState.LOCKED) return false;
		}

		// Pets
		if(m_petsOwnedCount > profile.petCollection.unlockedPetsCount) return false;
		for(int i = 0; i < m_petsOwned.Length; ++i) {
			if(!profile.petCollection.IsPetUnlocked(m_petsOwned[i])) return false;
		}

		// Skins
		for(int i = 0; i < m_skinsOwned.Length; ++i) {
			if(profile.wardrobe.GetSkinState(m_skinsOwned[i]) != Wardrobe.SkinState.OWNED) return false;
		}
		for(int i = 0; i < m_skinsUnlocked.Length; ++i) {
			if(profile.wardrobe.GetSkinState(m_skinsOwned[i]) == Wardrobe.SkinState.LOCKED) return false;
		}

		// Countries
		string countryCode = DeviceUtilsManager.SharedInstance.GetDeviceCountryCode();
		if(m_countriesAllowed.Length > 0 && m_countriesAllowed.IndexOf(countryCode) < 0) return false;
		if(m_countriesExcluded.IndexOf(countryCode) >= 0) return false;

		// All checks passed!
		return true;
	}

	/// <summary>
	/// Apply this pack to current user.
	/// </summary>
	public void Apply() {
		// [AOC] TODO!! Figure out proper flow
		// Push all the rewards to the pending rewards stack and trigger the pending rewards screen
		for(int i = 0; i < m_items.Count; ++i) {
			UsersManager.currentUser.PushReward(m_items[i].reward);
			// [AOC] TODO!! Validate that the pack is only applied once?
		}

		// Close all open popups and go to the pending rewards screen
		// [AOC] TODO!! Don't mix UI flow with logic code! Take this out of here!
		PopupManager.Clear();
		InstanceManager.menuSceneController.GoToScreen(MenuScreen.PENDING_REWARD);
	}

	//------------------------------------------------------------------------//
	// INTERNAL UTILS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Custom range parser. Do it here to avoid changing Calety -_-
	/// </summary>
	/// <returns>New range initialized with data parsed from the input string.</returns>
	/// <param name="_str">String to be parsed. Must be in the format "min:max".</param>
	private Range ParseRange(string _str) {
		string[] tokens = _str.Split(':');
		Range r = new Range();

		float val = r.min;
		if(tokens.Length > 0) {
			float.TryParse(tokens[0], out val);
			r.min = val;
		}

		val = r.max;
		if(tokens.Length > 1) {
			float.TryParse(tokens[1], out val);
			r.max = val;
		}

		return r;
	}

	/// <summary>
	/// Custom range parser. Do it here to avoid changing Calety -_-
	/// </summary>
	/// <returns>New range initialized with data parsed from the input string.</returns>
	/// <param name="_str">String to be parsed. Must be in the format "min:max".</param>
	private RangeInt ParseRangeInt(string _str) {
		// Use float version and just convert to int
		Range r = ParseRange(_str);
		return new RangeInt((int)r.min, (int)r.max);
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
}