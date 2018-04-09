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

	public static readonly string[] ITEM_TYPE_ORDER = {
		Metagame.RewardSoftCurrency.TYPE_CODE,
		Metagame.RewardHardCurrency.TYPE_CODE,
		Metagame.RewardGoldenFragments.TYPE_CODE,
		Metagame.RewardEgg.TYPE_CODE,
		Metagame.RewardPet.TYPE_CODE,
		Metagame.RewardSkin.TYPE_CODE
	};

	public const int MAX_ITEMS = 3;	// For now
	
	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	private DefinitionNode m_def = null;
	public DefinitionNode def {
		get { return m_def; }
	}

	private List<OfferPackItem> m_items = new List<OfferPackItem>(MAX_ITEMS);
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

	private string[] m_dragonUnlocked = new string[0];
	private string[] m_dragonOwned = new string[0];
	private string[] m_dragonNotOwned = new string[0];

	private Range m_scBalanceRange = new Range(0, float.MaxValue);
	private Range m_hcBalanceRange = new Range(0, float.MaxValue);
	private int m_openedEggs = 0;

	private int m_petsOwnedCount = 0;
	private string[] m_petsOwned = new string[0];
	private string[] m_petsNotOwned = new string[0];

	private RangeInt m_progressionRange = new RangeInt();

	private string[] m_skinsUnlocked = new string[0];
	private string[] m_skinsOwned = new string[0];
	private string[] m_skinsNotOwned = new string[0];

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

		// Items - limited to 3 for now
		for(int i = 1; i <= MAX_ITEMS; ++i) {	// [1..N]
			// Find out item definition
			DefinitionNode itemDef = DefinitionsManager.SharedInstance.GetDefinition(
				DefinitionsCategory.OFFER_ITEMS,
				_def.GetAsString("item" + i, "")
			);
			if(itemDef == null) continue;

			// Create and store new item
			OfferPackItem item = new OfferPackItem();
			item.InitFromDefinition(itemDef);
			m_items.Add(item);
		}

		// Params
		// We have just done a Reset(), so variables have the desired default values

		// General
		m_order = _def.GetAsInt("order", m_order);

		// Featuring
		m_featured = _def.GetAsBool("featured", m_featured);
		m_frequency = _def.GetAsInt("frequency", m_frequency);
		m_maxViews = _def.GetAsInt("maxViews", m_maxViews);
		switch(_def.GetAsString("zone", "")) {
			case "dragonSelection":			m_whereToShow = WhereToShow.DRAGON_SELECTION;			break;
			case "dragonSelectionAfterRun":	m_whereToShow = WhereToShow.DRAGON_SELECTION_AFTER_RUN;	break;
			case "playScreen":				m_whereToShow = WhereToShow.PLAY_SCREEN;				break;
			default:						break;	// Already has the default value
		}

		m_startDate = TimeUtils.TimestampToDate(_def.GetAsLong("startDate", 0) * 1000);
		m_endDate = TimeUtils.TimestampToDate(_def.GetAsLong("endDate", 0) * 1000);
		m_isTimed = !m_startDate.Equals(m_endDate);

		// Segmentation
		m_minAppVersion = Version.Parse(_def.GetAsString("minAppVersion", "1.0.0"));
		m_countriesAllowed = ParseArray(_def.GetAsString("countriesAllowed"));
		m_countriesExcluded = ParseArray(_def.GetAsString("countriesExcluded"));
		m_gamesPlayed = _def.GetAsInt("gamesPlayed", m_gamesPlayed);
		switch(_def.GetAsString("payerType", "")) {
			case "payer":		m_payerType = PayerType.PAYER;			break;
			case "nonPayer":	m_payerType = PayerType.NON_PAYER;		break;
			default:			break;	// Already has the default value
		}
		m_minSpent = _def.GetAsFloat("minSpent", m_minSpent);

		m_dragonUnlocked = ParseArray(_def.GetAsString("dragonUnlocked"));
		m_dragonOwned = ParseArray(_def.GetAsString("dragonOwned"));
		m_dragonNotOwned = ParseArray(_def.GetAsString("dragonNotOwned"));

		m_scBalanceRange = ParseRange(_def.GetAsString("scBalanceRange"));
		m_hcBalanceRange = ParseRange(_def.GetAsString("hcBalanceRange"));
		m_openedEggs = _def.GetAsInt("openedEggs", m_openedEggs);

		m_petsOwnedCount = _def.GetAsInt("petsOwnedCount", m_petsOwnedCount);
		m_petsOwned = ParseArray(_def.GetAsString("petsOwned"));
		m_petsNotOwned = ParseArray(_def.GetAsString("petsNotOwned"));

		m_progressionRange = ParseRangeInt(_def.GetAsString("progressionRange"));

		m_skinsUnlocked = ParseArray(_def.GetAsString("skinsUnlocked"));
		m_skinsOwned = ParseArray(_def.GetAsString("skinsOwned"));
		m_skinsNotOwned = ParseArray(_def.GetAsString("skinsNotOwned"));

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

		m_dragonUnlocked = new string[0];
		m_dragonOwned = new string[0];
		m_dragonNotOwned = new string[0];

		m_scBalanceRange = new Range(0, float.MaxValue);
		m_hcBalanceRange = new Range(0, float.MaxValue);
		m_openedEggs = 0;

		m_petsOwnedCount = 0;
		m_petsOwned = new string[0];
		m_petsNotOwned = new string[0];

		m_progressionRange = new RangeInt(0, int.MaxValue);

		m_skinsUnlocked = new string[0];
		m_skinsOwned = new string[0];
		m_skinsNotOwned = new string[0];

		// Internal vars
		m_viewsCount = 0;
		m_lastViewTimestamp = new DateTime();
	}

	/// <summary>
	/// Check timers for this pack.
	/// Only applies for timed offers, the rest will always return true.
	/// </summary>
	/// <returns>Whether this offer has started and not yet expired or not.</returns>
	public bool CheckTimers() {
		if(!m_isTimed) return true;

		// Check both start and end dates
		DateTime serverTime = GameServerManager.SharedInstance.GetEstimatedServerTime();
		if(serverTime < m_startDate) return false;
		if(serverTime > m_endDate) return false;

		return true;
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
		PopupController popup = PopupManager.LoadPopup(PopupFeaturedOffer.PATH);
		popup.GetComponent<PopupFeaturedOffer>().InitFromOfferPack(this);
		popup.Open();

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
			if(!CheckTimers()) return false;
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
		for(int i = 0; i < m_dragonUnlocked.Length; ++i) {
			if(DragonManager.GetDragonData(m_dragonUnlocked[i]).lockState <= DragonData.LockState.LOCKED) return false;
		}
		for(int i = 0; i < m_dragonOwned.Length; ++i) {
			if(!DragonManager.IsDragonOwned(m_dragonOwned[i])) return false;
		}
		for(int i = 0; i < m_dragonNotOwned.Length; ++i) {
			if(DragonManager.IsDragonOwned(m_dragonNotOwned[i])) return false;
		}

		// Pets
		if(m_petsOwnedCount > profile.petCollection.unlockedPetsCount) return false;
		for(int i = 0; i < m_petsOwned.Length; ++i) {
			if(!profile.petCollection.IsPetUnlocked(m_petsOwned[i])) return false;
		}
		for(int i = 0; i < m_petsNotOwned.Length; ++i) {
			if(profile.petCollection.IsPetUnlocked(m_petsNotOwned[i])) return false;
		}

		// Skins
		for(int i = 0; i < m_skinsUnlocked.Length; ++i) {
			if(profile.wardrobe.GetSkinState(m_skinsOwned[i]) == Wardrobe.SkinState.LOCKED) return false;
		}
		for(int i = 0; i < m_skinsOwned.Length; ++i) {
			if(profile.wardrobe.GetSkinState(m_skinsOwned[i]) != Wardrobe.SkinState.OWNED) return false;
		}
		for(int i = 0; i < m_skinsNotOwned.Length; ++i) {
			if(profile.wardrobe.GetSkinState(m_skinsNotOwned[i]) == Wardrobe.SkinState.OWNED) return false;
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
		// [AOC] TODO!! Validate that the pack is only applied once?

		// We want the rewards to be given in a specific order: do so
		List<OfferPackItem> sortedItems = new List<OfferPackItem>(m_items);
		sortedItems.Sort(
			(OfferPackItem _item1, OfferPackItem _item2) => {
				// Depends on type
				return ITEM_TYPE_ORDER.IndexOf(_item1.type).CompareTo(ITEM_TYPE_ORDER.IndexOf(_item2.type));
			}
		);


		// Push all the rewards to the pending rewards stack
		// Reverse order so last rewards pushed are collected first!
		for(int i = sortedItems.Count - 1; i >= 0; --i) {
			UsersManager.currentUser.PushReward(sortedItems[i].reward);
		}
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
		Range r = new Range(0, float.MaxValue);

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
		string[] tokens = _str.Split(':');
		RangeInt r = new RangeInt(0, int.MaxValue);

		int val = r.min;
		if(tokens.Length > 0) {
			int.TryParse(tokens[0], out val);
			r.min = val;
		}

		val = r.max;
		if(tokens.Length > 1) {
			int.TryParse(tokens[1], out val);
			r.max = val;
		}

		return r;
	}

	/// <summary>
	/// Wrapper of the DefinitionNode GetAsArray<T> method working around the fact
	/// that it returns a length 1 array when the field is empty (issue pending
	/// to be fixed in Calety).
	/// </summary>
	/// <returns>The given string as a array of values splited with the ";" character.</returns>
	/// <param name="_str">String to be parsed. Must use ";" as separator.</param>
	private string[] ParseArray(string _str) {
		// Empty array if string is empty
		if(string.IsNullOrEmpty(_str)) return new string[0];

		return _str.Split(new string[] {";"}, StringSplitOptions.None);
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
}