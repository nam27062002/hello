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
using System.Globalization;
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
	public const string EMPTY_VALUE = "-";

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

	// Purchase limit
	private int m_purchaseLimit = 1;
	private int m_purchaseCount = 0;
	public int purchaseCount {
		get { return m_purchaseCount; }
	}

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
			// Create and initialize new item
			OfferPackItem item = new OfferPackItem();
			item.InitFromDefinition(_def, i);

			// If a reward wasn't generated, the item is either not properly defined or the pack doesn't have this item, don't store it
			if(item.reward == null) continue;
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

		// Purchase limit
		m_purchaseLimit = _def.GetAsInt("purchaseLimit", m_purchaseLimit);

		// Persisted data
		UsersManager.currentUser.LoadOfferPack(this);
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

		// Purchase limit
		m_purchaseLimit = 1;
		m_purchaseCount = 0;

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
		// Just in case
		if(m_def == null) return false;

		// Not if not featured
		if(!m_featured) return false;

		// Not if segmentation fails
		if(!CanBeDisplayed()) return false;

		// Check max views
		if(m_maxViews > 0 && m_viewsCount >= m_maxViews) return false;

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

		// Tracking
		HDTrackingManager.Instance.Notify_OfferShown(false, m_def.GetAsString("iapSku"));

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

		// Purchase limit (ignore if 0 or negative, unlimited pack)
		if(m_purchaseLimit > 0 && m_purchaseCount >= m_purchaseLimit) return false;

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
			if(profile.wardrobe.GetSkinState(m_skinsUnlocked[i]) == Wardrobe.SkinState.LOCKED) return false;
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
		// Validate purchase limit
		if(m_purchaseLimit > 0 && m_purchaseCount >= m_purchaseLimit) return;

		// We want the rewards to be given in a specific order: do so
		List<OfferPackItem> sortedItems = new List<OfferPackItem>(m_items);
		sortedItems.Sort(OfferPackItem.Compare);
			
		// Push all the rewards to the pending rewards stack
		// Reverse order so last rewards pushed are collected first!
		for(int i = sortedItems.Count - 1; i >= 0; --i) {
			UsersManager.currentUser.PushReward(sortedItems[i].reward);
		}

		// Update purchase tracking
		// [AOC] BEFORE Saving persistence!
		m_purchaseCount++;

		// Save persistence
		UsersManager.currentUser.SaveOfferPack(this);
		PersistenceFacade.instance.Save_Request();

		// Notify game
		Messenger.Broadcast<OfferPack>(MessengerEvents.OFFER_APPLIED, this);
	}

	//------------------------------------------------------------------------//
	// EXTERNAL UTILS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Very custom method to make sure a definition corresponding to an offer pack 
	/// contains all required default values.
	/// If a parameter is missing in the definition, it will be added with this pack's
	/// current value for that parameter.
	/// The pack should have the default values (as applied in the Reset() method).
	/// </summary>
	/// <param name="_def">Definition to be filled.</param>
	public void ValidateDefinition(DefinitionNode _def) {
		// Items
		// Create a dummy item with default values and use it to validate the definitions
		OfferPackItem item = new OfferPackItem();
		for(int i = 1; i <= MAX_ITEMS; ++i) {
			item.ValidateDefinition(_def, i);
		}

		// General
		SetValueIfMissing(ref _def, "order", m_order.ToString(CultureInfo.InvariantCulture));

		// Featuring
		SetValueIfMissing(ref _def, "featured", m_featured.ToString(CultureInfo.InvariantCulture));
		SetValueIfMissing(ref _def, "frequency", m_frequency.ToString(CultureInfo.InvariantCulture));
		SetValueIfMissing(ref _def, "maxViews", m_maxViews.ToString(CultureInfo.InvariantCulture));
		SetValueIfMissing(ref _def, "zone", "");
		SetValueIfMissing(ref _def, "startDate", (TimeUtils.DateToTimestamp(m_startDate) / 1000).ToString(CultureInfo.InvariantCulture));
		SetValueIfMissing(ref _def, "endDate", (TimeUtils.DateToTimestamp(m_endDate) / 1000).ToString(CultureInfo.InvariantCulture));

		// Segmentation
		SetValueIfMissing(ref _def, "minAppVersion", m_minAppVersion.ToString(Version.Format.FULL));
		SetValueIfMissing(ref _def, "countriesAllowed", string.Join(";", m_countriesAllowed));
		SetValueIfMissing(ref _def, "countriesExcluded", string.Join(";", m_countriesExcluded));
		SetValueIfMissing(ref _def, "gamesPlayed", m_gamesPlayed.ToString(CultureInfo.InvariantCulture));
		SetValueIfMissing(ref _def, "payerType", "");
		SetValueIfMissing(ref _def, "minSpent", m_minSpent.ToString(CultureInfo.InvariantCulture));

		SetValueIfMissing(ref _def, "dragonUnlocked", string.Join(";", m_dragonUnlocked));
		SetValueIfMissing(ref _def, "dragonOwned", string.Join(";", m_dragonOwned));
		SetValueIfMissing(ref _def, "dragonNotOwned", string.Join(";", m_dragonNotOwned));

		SetValueIfMissing(ref _def, "scBalanceRange", string.Join(":", new string[] {
			m_scBalanceRange.min.ToString(CultureInfo.InvariantCulture),
			m_scBalanceRange.max.ToString(CultureInfo.InvariantCulture)
		}));
		SetValueIfMissing(ref _def, "hcBalanceRange", string.Join(":", new string[] {
			m_hcBalanceRange.min.ToString(CultureInfo.InvariantCulture),
			m_hcBalanceRange.max.ToString(CultureInfo.InvariantCulture)
		}));
		SetValueIfMissing(ref _def, "openedEggs", m_openedEggs.ToString(CultureInfo.InvariantCulture));

		SetValueIfMissing(ref _def, "petsOwnedCount", m_petsOwnedCount.ToString(CultureInfo.InvariantCulture));
		SetValueIfMissing(ref _def, "petsOwned", string.Join(";", m_petsOwned));
		SetValueIfMissing(ref _def, "petsNotOwned", string.Join(";", m_petsNotOwned));

		SetValueIfMissing(ref _def, "progressionRange", string.Join(":", new string[] {
			m_progressionRange.min.ToString(CultureInfo.InvariantCulture),
			m_progressionRange.max.ToString(CultureInfo.InvariantCulture)
		}));

		SetValueIfMissing(ref _def, "skinsUnlocked", string.Join(";", m_skinsUnlocked));
		SetValueIfMissing(ref _def, "skinsOwned", string.Join(";", m_skinsOwned));
		SetValueIfMissing(ref _def, "skinsNotOwned", string.Join(";", m_skinsNotOwned));

		// Purchase limit
		SetValueIfMissing(ref _def, "purchaseLimit", m_purchaseLimit.ToString(CultureInfo.InvariantCulture));
	}

	//------------------------------------------------------------------------//
	// STATIC UTILS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Custom range parser. Do it here to avoid changing Calety -_-
	/// </summary>
	/// <returns>New range initialized with data parsed from the input string.</returns>
	/// <param name="_str">String to be parsed. Must be in the format "min:max".</param>
	public static Range ParseRange(string _str) {
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
	public static RangeInt ParseRangeInt(string _str) {
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
	public static string[] ParseArray(string _str) {
		// Empty array if string is empty
		if(string.IsNullOrEmpty(_str)) return new string[0];

		return _str.Split(new string[] {";"}, StringSplitOptions.None);
	}

	/// <summary>
	/// If a property is missing from a definition node, add it with a given value.
	/// </summary>
	/// <param name="_def">Definition to be modified.</param>
	/// <param name="_key">Key of the property to be checked.</param>
	/// <param name="_value">Value to be set if the property is missing from the definition node.</param>
	public static void SetValueIfMissing(ref DefinitionNode _def, string _key, string _value) {
		// [AOC] This is disgusting because the SetValue() method also performs the Has() operation,
		//		 but proper solution requires changing Calety and all the bureaucracy around it -_-
		if(!_def.Has(_key) || _def.GetAsString(_key) == EMPTY_VALUE) {
			_def.SetValue(_key, _value);
		}
	}

	//------------------------------------------------------------------------//
	// PERSISTENCE															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// In the particular case of the offers, we only need to persist them in specific cases.
	/// </summary>
	/// <returns>Whether the offer should be persisted or not.</returns>
	public bool ShouldBePersisted() {
		// Yes if it has been purchased at least once
		if(m_purchaseCount > 0) return true;

		// [AOC] TODO!! Yes if it has a limited duration and still active (we need to persist the activation date)

		// No for the rest of cases
		return false;
	}

	/// <summary>
	/// Load state from a persistence object.
	/// </summary>
	/// <param name="_data">The data object loaded from persistence.</param>
	/// <returns>Whether the mission was successfully loaded</returns>
	public void Load(SimpleJSON.JSONClass _data) {
		// Purchase count
		string key = "purchaseCount";
		if(_data.ContainsKey(key)) {
			m_purchaseCount = _data[key].AsInt;
		}

		// [AOC] TODO!! End timestamp
	}

	/// <summary>
	/// Create and return a persistence save data json initialized with the data.
	/// </summary>
	/// <returns>A new data json to be stored to persistence by the PersistenceManager.</returns>
	public SimpleJSON.JSONClass Save() {
		// Create new object
		SimpleJSON.JSONClass data = new SimpleJSON.JSONClass();

		// Purchase count
		if(m_purchaseCount > 0) {
			data.Add("purchaseCount", m_purchaseCount.ToString(PersistenceFacade.JSON_FORMATTING_CULTURE));
		}

		// [AOC] TODO!! End timestamp

		// Done!
		return data;
	}

	/// <summary>
	/// Generates a unique ID for this offer pack, which will be used when saving
	/// pack data to the persistence json.
	/// </summary>
	/// <returns>The offer pack unique ID, composed by its sku and customization ID.</returns>
	public string GetPersistenceUniqueID() {
		if(m_def == null) return string.Empty;
		return m_def.sku + m_def.customizationCode.ToString(PersistenceFacade.JSON_FORMATTING_CULTURE);
	}
}