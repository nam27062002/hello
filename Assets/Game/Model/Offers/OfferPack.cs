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
	#region CONSTANTS
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

	public enum State {
		PENDING_ACTIVATION,
		ACTIVE,
		EXPIRED
	}

	public const int MAX_ITEMS = 3;	// For now
	#endregion
	
	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	#region MEMBERS AND PROPERTIES
	// Pack setup
	private DefinitionNode m_def = null;
	public DefinitionNode def {
		get { return m_def; }
	}

	private List<OfferPackItem> m_items = new List<OfferPackItem>(MAX_ITEMS);
	public List<OfferPackItem> items {
		get { return m_items; }
	}

	private string m_uniqueId = "";
	public string uniqueId {
		get {
			if(!string.IsNullOrEmpty(m_uniqueId)) {
				return m_uniqueId;
			} else if(m_def != null) {
				return m_def.sku;
			} else {
				return string.Empty;
			}
		}
	}

	private State m_state = State.PENDING_ACTIVATION;
	public State state {
		get { return m_state; }
	}

	public bool isActive {
		get { return m_state == State.ACTIVE; }
	}

	// Segmentation parameters
	// See https://mdc-web-tomcat17.ubisoft.org/confluence/display/ubm/%5BHD%5D+Offers+1.0#id-[HD]Offers1.0-Segmentationdetails

	// Mandatory segmentation params
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

	// Timing
	private float m_duration = 0f;
	private DateTime m_startDate = DateTime.MinValue;
	private DateTime m_endDate = DateTime.MinValue;

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
	private int m_minNumberOfPurchases = 0;
	private long m_secondsSinceLastPurchase = 0;

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

	// Activation control
	private DateTime m_activationTimestamp = DateTime.MinValue;
	public DateTime activationTimestmap {
		get { return m_activationTimestamp; }
	}

	private DateTime m_endTimestamp = DateTime.MaxValue;
	public DateTime endTimestamp {
		get { return m_endTimestamp; }
	}

	public TimeSpan remainingTime {
		get { 
			if(isTimed && isActive) {
				return m_endTimestamp - GameServerManager.SharedInstance.GetEstimatedServerTime(); 
			} else {
				return DateTime.MaxValue - GameServerManager.SharedInstance.GetEstimatedServerTime();
			}
		}
	}
	#endregion

	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	#region GENERIC METHODS
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

	/// <summary>
	/// Update loop. Should be called periodically from the manager.
	/// Will look for pack's state changes.
	/// </summary>
	/// <returns>Whether the pack has change its state.</returns>
	public bool UpdateState() {
		// Based on pack's state
		State oldState = m_state;
		switch(m_state) {
			case State.PENDING_ACTIVATION: {
				// Check for activation
				if(CheckActivation() && CheckSegmentation()) {
					ChangeState(State.ACTIVE);

					// Just in case, check for expiration immediately after
					if(CheckExpiration()) {
						ChangeState(State.EXPIRED);
					}
				}

				// Packs expiring before ever being activated (i.e. dragon not owned, excluded countries, etc.)
				else if(CheckExpiration()) {
					ChangeState(State.EXPIRED);
				}
			} break;

			case State.ACTIVE: {
				// Check for expiration
				if(CheckExpiration()) {
					ChangeState(State.EXPIRED);
				}

				// The pack might have gone out of segmentation range (i.e. currency balance). Check it!
				else if(!CheckSegmentation()) {
					ChangeState(State.PENDING_ACTIVATION);
				}
			} break;

			case State.EXPIRED: {
				// Nothing to do (expired packs can't be reactivated)
			} break;
		}

		// Has state changed?
		return (oldState != m_state);
	}

	/// <summary>
	/// Immediately mark this pack as expired!
	/// No checks will be performed.
	/// </summary>
	public void ForceExpiration() {
		ChangeState(State.EXPIRED);
	}
	#endregion

	//------------------------------------------------------------------------//
	// INITIALIZATION METHODS												  //
	//------------------------------------------------------------------------//
	#region INITIALIZATION METHODS
	/// <summary>
	/// Reset to default values.
	/// </summary>
	public void Reset() {
		// Items and def
		m_items.Clear();
		m_def = null;
		m_uniqueId = string.Empty;
		m_state = State.PENDING_ACTIVATION;

		// Mandatory params
		m_minAppVersion.Set(0, 0, 0);
		m_order = 0;

		// Mandatory for featured packs
		m_featured = false;
		m_frequency = 0;
		m_maxViews = 0;
		m_whereToShow = WhereToShow.SHOP_ONLY;

		// Timing
		m_duration = 0f;
		m_startDate = DateTime.MinValue;
		m_endDate = DateTime.MinValue;
		m_isTimed = false;

		// Optional params
		m_countriesAllowed = new string[0];
		m_countriesExcluded = new string[0];
		m_gamesPlayed = 0;
		m_payerType = PayerType.ANYONE;
		m_minSpent = 0f;
		m_minNumberOfPurchases = 0;
		m_secondsSinceLastPurchase = 0;

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

		// Activation control
		m_activationTimestamp = DateTime.MinValue;
		m_endTimestamp = DateTime.MaxValue;
	}

	/// <summary>
	/// Initialize this offer pack with the given definition.
	/// </summary>
	/// <param name="_def">Definition to be parsed.</param>
    public void InitFromDefinition(DefinitionNode _def, bool _updateFromCustomizer) {
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


        // Unique ID
        m_uniqueId = m_def.GetAsString("uniqueId");
        if (String.IsNullOrEmpty(m_uniqueId)) { // 
            //[MSF] at this point, the ID is always empty.
            //So we have to use the sku and the experiment
            //number as an ID.
            m_uniqueId = m_def.customizationCode.ToString() + "_" + m_def.sku;
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

		// Timing
		m_duration = _def.GetAsFloat("durationMinutes", 0f);
		m_startDate = TimeUtils.TimestampToDate(_def.GetAsLong("startDate", 0), false);
		m_endDate = TimeUtils.TimestampToDate(_def.GetAsLong("endDate", 0), false);
		m_isTimed = (m_endDate > m_startDate) || (m_duration > 0f);

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
		m_minSpent = _def.GetAsFloat("minSpent", m_minSpent) * 100;	// Content in USD, we work in cents of USD
		m_minNumberOfPurchases = _def.GetAsInt("minNumberOfPurchases", m_minNumberOfPurchases);
		m_secondsSinceLastPurchase = _def.GetAsLong("minutesSinceLastPurchase", m_secondsSinceLastPurchase / 60L) * 60L;		// Content in minutes, we work in seconds

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

		// Purchase limit
		m_purchaseLimit = _def.GetAsInt("purchaseLimit", m_purchaseLimit);

        // Persisted data
        UsersManager.currentUser.LoadOfferPack(this);
	}

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
		SetValueIfMissing(ref _def, "uniqueId", m_uniqueId.ToString(CultureInfo.InvariantCulture));
		SetValueIfMissing(ref _def, "order", m_order.ToString(CultureInfo.InvariantCulture));

		// Featuring
		SetValueIfMissing(ref _def, "featured", m_featured.ToString(CultureInfo.InvariantCulture));
		SetValueIfMissing(ref _def, "frequency", m_frequency.ToString(CultureInfo.InvariantCulture));
		SetValueIfMissing(ref _def, "maxViews", m_maxViews.ToString(CultureInfo.InvariantCulture));
		SetValueIfMissing(ref _def, "zone", "");

		// Timing
		SetValueIfMissing(ref _def, "durationMinutes", m_duration.ToString(CultureInfo.InvariantCulture));
		SetValueIfMissing(ref _def, "startDate", (TimeUtils.DateToTimestamp(m_startDate, false)).ToString(CultureInfo.InvariantCulture));
		SetValueIfMissing(ref _def, "endDate", (TimeUtils.DateToTimestamp(m_endDate, false)).ToString(CultureInfo.InvariantCulture));

		// Segmentation
		SetValueIfMissing(ref _def, "minAppVersion", m_minAppVersion.ToString(Version.Format.FULL));
		SetValueIfMissing(ref _def, "countriesAllowed", string.Join(";", m_countriesAllowed));
		SetValueIfMissing(ref _def, "countriesExcluded", string.Join(";", m_countriesExcluded));
		SetValueIfMissing(ref _def, "gamesPlayed", m_gamesPlayed.ToString(CultureInfo.InvariantCulture));
		SetValueIfMissing(ref _def, "payerType", "");
		SetValueIfMissing(ref _def, "minSpent", m_minSpent.ToString(CultureInfo.InvariantCulture));
		SetValueIfMissing(ref _def, "minNumberOfPurchases", m_minNumberOfPurchases.ToString(CultureInfo.InvariantCulture));
		SetValueIfMissing(ref _def, "minutesSinceLastPurchase", (m_secondsSinceLastPurchase / 60L).ToString(CultureInfo.InvariantCulture));

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
	#endregion

	//------------------------------------------------------------------------//
	// STATE CONTROL METHODS												  //
	//------------------------------------------------------------------------//
	#region STATE CONTROL METHODS
	/// <summary>
	/// Checks parameters that could cause an activation.
	/// These are parameters that can only be triggered once:
	/// Start date, unlocked content, progression, etc.
	/// </summary>
	/// <returns>Whether this pack should be activated or not.</returns>
	private bool CheckActivation() {
		// Aux vars
		UserProfile profile = UsersManager.currentUser;
		TrackingPersistenceSystem trackingPersistence = HDTrackingManager.Instance.TrackingPersistenceSystem;

		// Start date
		if(GameServerManager.SharedInstance.GetEstimatedServerTime() < m_startDate) return false;

		// Progression
		if(profile.gamesPlayed < m_gamesPlayed) return false;
		if(profile.GetPlayerProgress() < m_progressionRange.min) return false;
		if(profile.eggsCollected < m_openedEggs) return false;

		// Payer profile
		int totalPurchases = (trackingPersistence == null) ? 0 : trackingPersistence.TotalPurchases;
		switch(m_payerType) {
			case PayerType.PAYER: {
				if(totalPurchases == 0) return false;
			} break;
		}

		// Min spent
		float totalSpent = (trackingPersistence == null) ? 0f : trackingPersistence.TotalSpent;
		if(m_minSpent > totalSpent) return false;

		// Min number of purchases
		if(m_minNumberOfPurchases > totalPurchases) return false;

		// Dragons
		for(int i = 0; i < m_dragonUnlocked.Length; ++i) {
			if(DragonManager.GetDragonData(m_dragonUnlocked[i]).lockState <= IDragonData.LockState.LOCKED) return false;
		}
		for(int i = 0; i < m_dragonOwned.Length; ++i) {
			if(!DragonManager.IsDragonOwned(m_dragonOwned[i])) return false;
		}

		// Pets
		if(profile.petCollection.unlockedPetsCount < m_petsOwnedCount) return false;
		for(int i = 0; i < m_petsOwned.Length; ++i) {
			if(!profile.petCollection.IsPetUnlocked(m_petsOwned[i])) return false;
		}

		// Skins
		for(int i = 0; i < m_skinsUnlocked.Length; ++i) {
			if(profile.wardrobe.GetSkinState(m_skinsUnlocked[i]) == Wardrobe.SkinState.LOCKED) return false;
		}
		for(int i = 0; i < m_skinsOwned.Length; ++i) {
			if(profile.wardrobe.GetSkinState(m_skinsOwned[i]) != Wardrobe.SkinState.OWNED) return false;
		}

		// All checks passed!
		return true;
	}

	/// <summary>
	/// Checks parameters that could cause an expiration.
	/// These are parameters that can only be triggered once:
	/// End date / duration, locked content, progression, purchase limit, etc.
	/// </summary>
	/// <returns>Whether this pack has expired.</returns>
	private bool CheckExpiration() {
		// Order is relevant!
		// Aux vars
		UserProfile profile = UsersManager.currentUser;

		// Multiple packs may have the same unique ID, with the intention to make 
		// them mutually exclusive.
		// If another pack with the same unique ID is active, mark this one as expired!
		// Resolves issue https://mdc-tomcat-jira100.ubisoft.org/jira/browse/HDK-2026
		for(int i = 0; i < OffersManager.activeOffers.Count; ++i) {
			// Skip if it's ourselves
			if(OffersManager.activeOffers[i] == this) continue;

			// Is there a pack with the same unique ID already active?
			if(OffersManager.activeOffers[i].uniqueId == this.uniqueId) {
				// Yes! Mark offer as expired ^^
				return true;
			}
		}

		// Timers
		if(m_isTimed) {
			DateTime serverTime = GameServerManager.SharedInstance.GetEstimatedServerTime();

			// End date
			if(m_endDate > DateTime.MinValue && serverTime > m_endDate) return true;

			// If active, check end timestamp (duration)
			if(isActive && serverTime > m_endTimestamp) return true;
		}

		// Purchase limit (ignore if 0 or negative, unlimited pack)
		if(m_purchaseLimit > 0 && m_purchaseCount >= m_purchaseLimit) return true;

		// Main conditions
		if(m_minAppVersion > GameSettings.internalVersion) return true;

		// Payer profile
		int totalPurchases = (HDTrackingManager.Instance.TrackingPersistenceSystem == null) ? 0 : HDTrackingManager.Instance.TrackingPersistenceSystem.TotalPurchases;
		switch(m_payerType) {
			case PayerType.NON_PAYER: {
				if(totalPurchases > 0) return true;
			} break;
		}

		// Progression
		if(profile.GetPlayerProgress() > m_progressionRange.max) return true;

		// Dragons
		for(int i = 0; i < m_dragonNotOwned.Length; ++i) {
			if(DragonManager.IsDragonOwned(m_dragonNotOwned[i])) return true;
		}

		// Pets
		for(int i = 0; i < m_petsNotOwned.Length; ++i) {
			if(profile.petCollection.IsPetUnlocked(m_petsNotOwned[i])) return true;
		}

		// Skins
		for(int i = 0; i < m_skinsNotOwned.Length; ++i) {
			if(profile.wardrobe.GetSkinState(m_skinsNotOwned[i]) == Wardrobe.SkinState.OWNED) return true;
		}

		// Countries
		string countryCode = DeviceUtilsManager.SharedInstance.GetDeviceCountryCode();
		if(m_countriesAllowed.Length > 0 && m_countriesAllowed.IndexOf(countryCode) < 0) return true;
		if(m_countriesExcluded.IndexOf(countryCode) >= 0) return true;

		// All checks passed!
		return false;
	}

	/// <summary>
	/// Checks parameters that could cause a state change multiple times.
	/// These are parameters that can be triggered multiple times and activate/deactivate the pack:
	/// Currency range, etc.
	/// </summary>
	/// <returns>Whether this pack passes defined segmentation with current user progression.</returns>
	private bool CheckSegmentation() {
		// Progression
		UserProfile profile = UsersManager.currentUser;
		if(!m_scBalanceRange.Contains((float)profile.coins)) return false;
		if(!m_hcBalanceRange.Contains((float)profile.pc)) return false;

		// Time since last purchase
		if(m_secondsSinceLastPurchase > 0) {	// Nothing to check if default
			TrackingPersistenceSystem trackingPersistence = HDTrackingManager.Instance.TrackingPersistenceSystem;
			if(trackingPersistence == null) return false;
			if(trackingPersistence.TotalPurchases > 0) {	// Ignore if player hasn't yet purchased
				long serverTime = GameServerManager.SharedInstance.GetEstimatedServerTimeAsLong() / 1000L;
				long timeSinceLastPurchase = serverTime - trackingPersistence.LastPurchaseTimestamp;
				if(m_secondsSinceLastPurchase > timeSinceLastPurchase) return false;	// Not enough time has passed
			}
		}

		// All checks passed!
		return true;
	}

	/// <summary>
	/// Change the logic state of the pack.
	/// No validation is done.
	/// </summary>
	/// <param name="_newState">New state for the pack.</param>
	private void ChangeState(State _newState) {
		// Actions to be performed when leaving a state
		switch(m_state) {
			case State.PENDING_ACTIVATION: break;
			case State.ACTIVE: break;
			case State.EXPIRED: break;
		}

		// Store new state
		State oldState = m_state;
		m_state = _newState;

		// Actions to be performed when entering a new state
		switch(m_state) {
			case State.PENDING_ACTIVATION: {
				// Nothing to do
			} break;

			case State.ACTIVE: {
				// Make sure we come from pending activation state
				if(oldState == State.PENDING_ACTIVATION) {
					// Set activation timestamp to now
					m_activationTimestamp = GameServerManager.SharedInstance.GetEstimatedServerTime();

					// Compute end timestamp
					// Combine duration and end date, both of which are optional
					bool hasEndDate = (m_endDate != DateTime.MinValue);
					bool hasDuration = m_duration > 0f;
					if(hasEndDate && hasDuration) {
						// Use the lowest between activation+duration and endDate
						DateTime durationTimestamp = m_activationTimestamp.AddMinutes(m_duration);
						if(durationTimestamp < m_endDate) {
							m_endTimestamp = durationTimestamp;
						} else {
							m_endTimestamp = m_endDate;
						}
					} else if(hasEndDate) {
						m_endTimestamp = m_endDate;
					} else if(hasDuration) {
						m_endTimestamp = m_activationTimestamp.AddMinutes(m_duration);
					} else {
						m_endTimestamp = DateTime.MaxValue;		// Infinite offer
					}
				}
			} break;

			case State.EXPIRED: {
				// Nothing to do
			} break;
		}
	}
	#endregion

	//------------------------------------------------------------------------//
	// PUBLIC METHODS														  //
	//------------------------------------------------------------------------//
	#region PUBLIC METHODS
	/// <summary>
	/// Check whether the featured popup should be displayed or not, and do it.
	/// </summary>
	/// <returns>The opened popup if all conditions to display it are met. <c>null</c> otherwise.</returns>
	/// <param name="_areaToCheck">Area to check.</param>
	public PopupController ShowPopupIfPossible(WhereToShow _areaToCheck) {
		// Just in case
		if(m_def == null) return null;

		// Not if not featured
		if(!m_featured) return null;

		// Only active offers!
		if(!isActive) return null;

		// Check max views
		if(m_maxViews > 0 && m_viewsCount >= m_maxViews) return null;

		// Check area
		if(m_whereToShow == WhereToShow.DRAGON_SELECTION) {
			// Special case for dragon selection screen: count both initial time and after run
			if(_areaToCheck != WhereToShow.DRAGON_SELECTION && _areaToCheck != WhereToShow.DRAGON_SELECTION_AFTER_RUN) return null;
		} else {
			// Standard case: just make sure we're in the right place
			if(_areaToCheck != m_whereToShow) return null;
		}

		// Check frequency
		DateTime serverTime = GameServerManager.SharedInstance.GetEstimatedServerTime();
		TimeSpan timeSinceLastView = serverTime - m_lastViewTimestamp;
		if(timeSinceLastView.TotalMinutes < m_frequency) return null;

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
		return popup;
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
	#endregion

	//------------------------------------------------------------------------//
	// STATIC UTILS															  //
	//------------------------------------------------------------------------//
	#region STATIC UTILS
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
	#endregion

	//------------------------------------------------------------------------//
	// PERSISTENCE															  //
	//------------------------------------------------------------------------//
	#region PERSISTENCE
	/// <summary>
	/// In the particular case of the offers, we only need to persist them in specific cases.
	/// </summary>
	/// <returns>Whether the offer should be persisted or not.</returns>
	public bool ShouldBePersisted() {
		// Never if definition is not valid
		if(m_def == null) return false;

		// Yes if it has been purchased at least once
		if(m_purchaseCount > 0) return true;

		// State-dependant
		switch(m_state) {
			case State.PENDING_ACTIVATION: {
				// No need to persist
			} break;

			case State.ACTIVE: {
				// Yes if we are tracking views
				if(m_viewsCount > 0) return true;

				// Yes if it has a limited duration
				if(m_isTimed) return true;
			} break;

			case State.EXPIRED: {
				// No need to persist
			} break;
		}

		// No for the rest of cases
		return false;
	}

	/// <summary>
	/// Load state from a persistence object.
	/// </summary>
	/// <param name="_data">The data object loaded from persistence.</param>
	/// <returns>Whether the mission was successfully loaded</returns>
	public void Load(SimpleJSON.JSONClass _data) {
		string key = "";

		// State
		key = "state";
		if(_data.ContainsKey(key)) {
			m_state = (State)_data[key].AsInt;
		}

		// Purchase count
		key = "purchaseCount";
		if(_data.ContainsKey(key)) {
			m_purchaseCount = _data[key].AsInt;
		}

		// View count
		key = "viewCount";
		if(_data.ContainsKey(key)) {
			m_viewsCount = _data[key].AsInt;
		}

		// Last view timestamp
		key = "lastViewTimestamp";
		if(_data.ContainsKey(key)) {
			m_lastViewTimestamp = DateTime.Parse(_data[key], PersistenceFacade.JSON_FORMATTING_CULTURE);
		}

		// Timestamps
		key = "activationTimestamp";
		if(_data.ContainsKey(key)) {
			m_activationTimestamp = DateTime.Parse(_data[key], PersistenceFacade.JSON_FORMATTING_CULTURE);
		}
		key = "endTimestamp";
		if(_data.ContainsKey(key)) {
			m_endTimestamp = DateTime.Parse(_data[key], PersistenceFacade.JSON_FORMATTING_CULTURE);
		}
	}

	/// <summary>
	/// Create and return a persistence save data json initialized with the data.
	/// </summary>
	/// <returns>A new data json to be stored to persistence by the PersistenceManager.</returns>
	public SimpleJSON.JSONClass Save() {
		// We can potentially have a lot of offer packs, so try to minimize stored data
		// To do so, we will only store relevant data whose value is different than the default one
		// Create new object
		SimpleJSON.JSONClass data = new SimpleJSON.JSONClass();

		// Sku
		// Multiple packs may have the same unique ID, with the intention to make 
		// them mutually exclusive. In order to know which pack with that ID is actually triggered,
		// store the sku of the pack as well.
		// Resolves issue https://mdc-tomcat-jira100.ubisoft.org/jira/browse/HDK-2026
		data.Add("sku", m_def.sku);

		// State
		data.Add("state", ((int)m_state).ToString(CultureInfo.InvariantCulture));

		// Timestamps
		// Only store for timed offers, the rest will be activated upon loading depending on activation triggers
		if(m_isTimed) {
			data.Add("activationTimestamp", m_activationTimestamp.ToString(PersistenceFacade.JSON_FORMATTING_CULTURE));
			data.Add("endTimestamp", m_endTimestamp.ToString(PersistenceFacade.JSON_FORMATTING_CULTURE));
		}

		// Purchase count - only if needed
		if(m_purchaseCount > 0) {
			data.Add("purchaseCount", m_purchaseCount.ToString(PersistenceFacade.JSON_FORMATTING_CULTURE));
		}

		// View count - only if needed
		// Last view timestamp
		if(m_viewsCount > 0) {
			data.Add("viewCount", m_viewsCount.ToString(PersistenceFacade.JSON_FORMATTING_CULTURE));
			data.Add("lastViewTimestamp", m_lastViewTimestamp.ToString(PersistenceFacade.JSON_FORMATTING_CULTURE));
		}

		// Done!
		return data;
	}
	#endregion
}