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

	public enum Type {
		PROGRESSION,
		PUSHED,
		ROTATIONAL,
		FREE,
        COUNT
	}

    public const int MAX_ITEMS = 3; // For now
	public const Type DEFAULT_TYPE = Type.PROGRESSION;
    public const UserProfile.Currency DEFAULT_CURRENCY = UserProfile.Currency.REAL;

    #endregion

    //------------------------------------------------------------------------//
    // MEMBERS AND PROPERTIES												  //
    //------------------------------------------------------------------------//
    #region MEMBERS AND PROPERTIES
    // Pack setup
    protected DefinitionNode m_def = null;
	public DefinitionNode def {
		get { return m_def; }
	}

	protected Type m_type = DEFAULT_TYPE;
	public Type type {
		get { return m_type; }
	}

    protected UserProfile.Currency m_currency = DEFAULT_CURRENCY;
    public UserProfile.Currency currency
    {
        get { return m_currency; }
    }

    protected List<OfferPackItem> m_items = new List<OfferPackItem>(MAX_ITEMS);
	public List<OfferPackItem> items {
		get { return m_items; }
	}

	protected string m_uniqueId = "";
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

	protected State m_state = State.PENDING_ACTIVATION;
	public State state {
		get { return m_state; }
	}

	public bool isActive {
		get { return m_state == State.ACTIVE; }
	}

	// Segmentation parameters
	// See https://mdc-web-tomcat17.ubisoft.org/confluence/display/ubm/%5BHD%5D+Offers+1.0#id-[HD]Offers1.0-Segmentationdetails

	// Mandatory segmentation params
	protected Version m_minAppVersion = new Version();
	public Version minAppVersion {
		get { return m_minAppVersion; }
	}

	protected int m_order = 0;
	public int order {
		get { return m_order; }
	}

	// Only required for Featured packs
	protected bool m_featured = false;
	public bool featured {
		get { return m_featured; }
	}

	protected int m_frequency = 1;
	protected int m_maxViews = 0;
	protected WhereToShow m_whereToShow = WhereToShow.SHOP_ONLY;

	// Timing
	protected float m_duration = 0f;
	protected DateTime m_startDate = DateTime.MinValue;
	protected DateTime m_endDate = DateTime.MinValue;

	protected bool m_isTimed = false;
	public bool isTimed {
		get { return m_isTimed; }
	}

	// Optional params
	protected string[] m_countriesAllowed = new string[0];
	protected string[] m_countriesExcluded = new string[0];
	protected int m_gamesPlayed = 0;
	protected PayerType m_payerType = PayerType.ANYONE;
	protected float m_minSpent = 0f;
	protected float m_maxSpent = float.MaxValue / 100f;
	protected int m_minNumberOfPurchases = 0;
	protected long m_secondsSinceLastPurchase = 0;

	protected string[] m_dragonUnlocked = new string[0];
	protected string[] m_dragonOwned = new string[0];
	protected string[] m_dragonNotOwned = new string[0];

	protected Range m_scBalanceRange = new Range(0, float.MaxValue);
	protected Range m_hcBalanceRange = new Range(0, float.MaxValue);
	protected int m_openedEggs = 0;

	protected int m_petsOwnedCount = 0;
	protected string[] m_petsOwned = new string[0];
	protected string[] m_petsNotOwned = new string[0];

	protected RangeInt m_progressionRange = new RangeInt();

	protected string[] m_skinsUnlocked = new string[0];
	protected string[] m_skinsOwned = new string[0];
	protected string[] m_skinsNotOwned = new string[0];

	// Purchase limit
	protected int m_purchaseLimit = 1;
	protected int m_purchaseCount = 0;
	public int purchaseCount {
		get { return m_purchaseCount; }
	}

	// Internal vars
	protected int m_viewsCount = 0;	// Only auto-triggered views
	protected DateTime m_lastViewTimestamp = new DateTime();

	// Activation control
	protected DateTime m_activationTimestamp = DateTime.MinValue;
	public DateTime activationTimestmap {
		get { return m_activationTimestamp; }
	}

	protected DateTime m_endTimestamp = DateTime.MaxValue;
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
	public virtual bool UpdateState() {
		OffersManager.LogPack("UpdateState {0} | {1}", Colors.pink, def.sku, m_state);

		// Based on pack's state
		State oldState = m_state;
		switch(m_state) {
			case State.PENDING_ACTIVATION: {
				// Check for activation
				if(CheckActivation() && CheckSegmentation()) {
					ChangeState(State.ACTIVE);

					// Just in case, check for expiration immediately after
					if(CheckExpiration(true)) {
						ChangeState(State.EXPIRED);
					}
				}

				// Packs expiring before ever being activated (i.e. dragon not owned, excluded countries, etc.)
				else if(CheckExpiration(true)) {
					ChangeState(State.EXPIRED);
				}
			} break;

			case State.ACTIVE: {
				// Check for expiration
				if(CheckExpiration(true)) {
					ChangeState(State.EXPIRED);
				}

				// The pack might have gone out of segmentation range (i.e. currency balance). Check it!
				// [AOC] TODO!! We might wanna keep some packs until they expire even if initial segmentation is no longer valid
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
	public virtual void Reset() {
		// Items and def
		m_items.Clear();
		m_def = null;
		m_type = DEFAULT_TYPE;
		m_uniqueId = string.Empty;
		m_state = State.PENDING_ACTIVATION;

		// Mandatory params
		m_minAppVersion.Set(0, 0, 0);
		m_order = 0;

		// Mandatory for featured packs
		m_featured = false;
		m_frequency = 1;	// Don't use 0 to avoid infinite loops!
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
		m_maxSpent = float.MaxValue / 100f;	// We're working with cents of USD
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
    public virtual void InitFromDefinition(DefinitionNode _def) {
		// Reset to default values
		Reset();

		// Store def
		m_def = _def;

		// Offer Type
		m_type = StringToType(m_def.GetAsString("type"));

        // Currency type
        m_currency = StringToCurrency(m_def.GetAsString("currency"));

		// Choose tracking group for rewards depending on offer type
		HDTrackingManager.EEconomyGroup ecoGroup = HDTrackingManager.EEconomyGroup.SHOP_OFFER_PACK;
		if(m_type == Type.FREE) {
			ecoGroup = HDTrackingManager.EEconomyGroup.SHOP_AD_OFFER_PACK;
		}

		// Items - limited to 3 for now
		for (int i = 1; i <= MAX_ITEMS; ++i) {	// [1..N]
			// Create and initialize new item
			OfferPackItem item = new OfferPackItem();
			item.InitFromDefinition(_def, i, ecoGroup);

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
		m_minSpent = _def.GetAsFloat("minSpent", m_minSpent) * 100f; 	// Content in USD, we work in cents of USD
		m_maxSpent = _def.GetAsFloat("maxSpent", m_maxSpent) * 100f;	// Content in USD, we work in cents of USD
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
	public virtual void ValidateDefinition(DefinitionNode _def) {
		// Items
		// Create a dummy item with default values and use it to validate the definitions
		OfferPackItem item = new OfferPackItem();
		for(int i = 1; i <= MAX_ITEMS; ++i) {
			item.ValidateDefinition(_def, i);
		}

		// General
		SetValueIfMissing(ref _def, "uniqueId", m_uniqueId.ToString(CultureInfo.InvariantCulture));
		SetValueIfMissing(ref _def, "type", TypeToString(DEFAULT_TYPE));
        SetValueIfMissing(ref _def, "currency", CurrencyToString(DEFAULT_CURRENCY));
        SetValueIfMissing(ref _def, "order", m_order.ToString(CultureInfo.InvariantCulture));
		SetValueIfMissing(ref _def, "refPrice", (0).ToString(CultureInfo.InvariantCulture));
		SetValueIfMissing(ref _def, "discount", (0).ToString(CultureInfo.InvariantCulture));

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
		SetValueIfMissing(ref _def, "maxSpent", m_maxSpent.ToString(CultureInfo.InvariantCulture));
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
	public virtual bool CheckActivation() {
		// Aux vars
		UserProfile profile = UsersManager.currentUser;
		TrackingPersistenceSystem trackingPersistence = HDTrackingManager.Instance.TrackingPersistenceSystem;
		DateTime serverTime = GameServerManager.SharedInstance.GetEstimatedServerTime();
		OffersManager.LogPack("CHECK ACTIVATION {0}", Colors.lime, def.sku);

		// Start date
		OffersManager.LogPack("    Start Date... {0} vs {1}", Colors.paleGreen, m_startDate, serverTime);
		if(serverTime < m_startDate) return false;

		// Progression
		OffersManager.LogPack("    Games Played... {0} vs {1}", Colors.paleGreen, m_gamesPlayed, profile.gamesPlayed);
		if(profile.gamesPlayed < m_gamesPlayed) return false;

		int playerProgress = profile.GetPlayerProgress();
		OffersManager.LogPack("    Min Player Progress... {0} vs {1}", Colors.paleGreen, m_progressionRange.min, playerProgress);
		if(playerProgress < m_progressionRange.min) return false;

		OffersManager.LogPack("    Eggs Collected... {0} vs {1}", Colors.paleGreen, m_openedEggs, profile.eggsCollected);
		if(profile.eggsCollected < m_openedEggs) return false;

		// Payer profile
		int totalPurchases = (trackingPersistence == null) ? 0 : trackingPersistence.TotalPurchases;
		OffersManager.LogPack("    Payer Type... {0} (totalPurchases {1})", Colors.paleGreen, m_payerType, totalPurchases);
		switch(m_payerType) {
			case PayerType.PAYER: {
				if(totalPurchases == 0) return false;
			} break;
		}

		// Min/max spent
		float totalSpent = (trackingPersistence == null) ? 0f : trackingPersistence.TotalSpent;

		OffersManager.LogPack("    Min Spent... {0} vs {1}", Colors.paleGreen, m_minSpent, totalSpent);
		if(m_minSpent > totalSpent) return false;

		OffersManager.LogPack("    Max Spent... {0} vs {1}", Colors.paleGreen, m_maxSpent, totalSpent);
		if(totalSpent > m_maxSpent) return false;

		// Min number of purchases
		OffersManager.LogPack("    Min Number Purchases... {0} vs {1}", Colors.paleGreen, m_minNumberOfPurchases, totalPurchases);
		if(m_minNumberOfPurchases > totalPurchases) return false;

		// Dragons
		OffersManager.LogPack("    Unlocked Dragons...", Colors.paleGreen);
		for(int i = 0; i < m_dragonUnlocked.Length; ++i) {
			if(DragonManager.GetDragonData(m_dragonUnlocked[i]).lockState <= IDragonData.LockState.LOCKED) return false;
		}

		OffersManager.LogPack("    Owned Dragons...", Colors.paleGreen);
		for(int i = 0; i < m_dragonOwned.Length; ++i) {
			if(!DragonManager.IsDragonOwned(m_dragonOwned[i])) return false;
		}

		// Pets
		OffersManager.LogPack("    Unlocked Pets... {0} vs {1}", Colors.paleGreen, m_petsOwnedCount, profile.petCollection.unlockedPetsCount);
		if(profile.petCollection.unlockedPetsCount < m_petsOwnedCount) return false;

		OffersManager.LogPack("    Owned Pets...", Colors.paleGreen);
		for(int i = 0; i < m_petsOwned.Length; ++i) {
			if(!profile.petCollection.IsPetUnlocked(m_petsOwned[i])) return false;
		}

		// Skins
		OffersManager.LogPack("    Unlocked Skins...", Colors.paleGreen);
		for(int i = 0; i < m_skinsUnlocked.Length; ++i) {
			if(profile.wardrobe.GetSkinState(m_skinsUnlocked[i]) == Wardrobe.SkinState.LOCKED) return false;
		}

		OffersManager.LogPack("    Owned Skins...", Colors.paleGreen);
		for(int i = 0; i < m_skinsOwned.Length; ++i) {
			if(profile.wardrobe.GetSkinState(m_skinsOwned[i]) != Wardrobe.SkinState.OWNED) return false;
		}

		// All checks passed!
		OffersManager.LogPack("ACTIVATION CHECKS PASSED! {0}", Colors.lime, def.sku);
		return true;
	}

	/// <summary>
	/// Checks parameters that could cause an expiration.
	/// These are parameters that can only be triggered once:
	/// End date / duration, locked content, progression, purchase limit, etc.
	/// </summary>
	/// <returns>Whether this pack has expired.</returns>
	/// <param name="_checkTime">Include expiration by time? Can be manually checked via the CheckExpirationByTime() method.</param>
	public virtual bool CheckExpiration(bool _checkTime) {
		// Order is relevant!
		// Aux vars
		UserProfile profile = UsersManager.currentUser;
		OffersManager.LogPack("CHECK EXPIRATION ({0}) {1}", Colors.red, _checkTime, def.sku);

		// Multiple packs may have the same unique ID, with the intention to make 
		// them mutually exclusive.
		// If another pack with the same unique ID is active, mark this one as expired!
		// Resolves issue https://mdc-tomcat-jira100.ubisoft.org/jira/browse/HDK-2026
		OffersManager.LogPack("    Duplicated IDs...", Colors.coral);
		for(int i = 0; i < OffersManager.activeOffers.Count; ++i) {
			// Skip if it's ourselves
			if(OffersManager.activeOffers[i] == this) continue;

			// Is there a pack with the same unique ID already active?
			if(OffersManager.activeOffers[i].uniqueId == this.uniqueId) {
				// Yes! Mark offer as expired ^^
				return true;
			}
		}

		// Timers - only if requested
		if(_checkTime) {
			if(CheckExpirationByTime()) return true;
		}

		// Purchase limit (ignore if 0 or negative, unlimited pack)
		if(m_purchaseLimit > 0) {
			OffersManager.LogPack("    Purchase Limit... {0} vs {1}", Colors.coral, m_purchaseLimit, m_purchaseCount);
			if(m_purchaseCount >= m_purchaseLimit) return true;
		}

		// Main conditions
		OffersManager.LogPack("    Min App Version... {0} vs {1}", Colors.coral, m_minAppVersion, GameSettings.internalVersion);
		if(m_minAppVersion > GameSettings.internalVersion) return true;

		// Payer profile
		TrackingPersistenceSystem trackingPersistence = HDTrackingManager.Instance.TrackingPersistenceSystem;
		int totalPurchases = (trackingPersistence == null) ? 0 : trackingPersistence.TotalPurchases;
		OffersManager.LogPack("    Payer Type... {0} (totalPurchases {1})", Colors.coral, m_payerType, totalPurchases);
		switch(m_payerType) {
			case PayerType.NON_PAYER: {
				if(totalPurchases > 0) return true;
			} break;
		}

		// Max spent
		float totalSpent = (trackingPersistence == null) ? 0f : trackingPersistence.TotalSpent;
		OffersManager.LogPack("    Max Spent... {0} vs {1}", Colors.coral, m_maxSpent, totalSpent);
		if(totalSpent > m_maxSpent) return true;

		// Progression
		int playerProgress = profile.GetPlayerProgress();
		OffersManager.LogPack("     Max Player Progress... {0} vs {1}", Colors.coral, m_progressionRange.max, playerProgress);
		if(playerProgress > m_progressionRange.max) return true;

		// Dragons
		OffersManager.LogPack("    Dragons Not Owned...", Colors.coral);
		for(int i = 0; i < m_dragonNotOwned.Length; ++i) {
			if(DragonManager.IsDragonOwned(m_dragonNotOwned[i])) return true;
		}

		// Pets
		OffersManager.LogPack("    Pets Not Owned...", Colors.coral);
		for(int i = 0; i < m_petsNotOwned.Length; ++i) {
			if(profile.petCollection.IsPetUnlocked(m_petsNotOwned[i])) return true;
		}

		// Skins
		OffersManager.LogPack("    Skins Not Owned...", Colors.coral);
		for(int i = 0; i < m_skinsNotOwned.Length; ++i) {
			if(profile.wardrobe.GetSkinState(m_skinsNotOwned[i]) == Wardrobe.SkinState.OWNED) return true;
		}

		// Countries
		string countryCode = DeviceUtilsManager.SharedInstance.GetDeviceCountryCode();

		OffersManager.LogPack("    Countries Allowed... {0}", Colors.coral, countryCode);
		if(m_countriesAllowed.Length > 0 && m_countriesAllowed.IndexOf(countryCode) < 0) return true;

		OffersManager.LogPack("    Countries Excluded... {0}", Colors.coral, countryCode);
		if(m_countriesExcluded.IndexOf(countryCode) >= 0) return true;

		// All checks passed!
		OffersManager.LogPack("EXPIRATION CHECKS PASSED! {0}", Colors.red, def.sku);
		return false;
	}

	/// <summary>
	/// Checks the expiration by time exclusively.
	/// </summary>
	/// <returns>Whether this pack has expired by time or not.</returns>
	public virtual bool CheckExpirationByTime() {
		// Never if the offer is not timed
		OffersManager.LogPack("    Is Timed?...", Colors.coral);
		if(!m_isTimed) return false;

		// Get server time
		DateTime serverTime = GameServerManager.SharedInstance.GetEstimatedServerTime();

		// Global end date
		OffersManager.LogPack("    End Date... {0} vs {1}", Colors.coral, m_endDate, serverTime);
		if(m_endDate > DateTime.MinValue && serverTime > m_endDate) return true;

		// If active, check end timestamp (duration)
		OffersManager.LogPack("    End Timestamp... {2} && {0} vs {1}", Colors.coral, m_endTimestamp, serverTime, isActive);
		if(isActive && serverTime > m_endTimestamp) return true;

		// All checks passed!
		OffersManager.LogPack("    Expiration By Time Checks Passed!", Colors.coral);
		return false;
	}

	/// <summary>
	/// Checks parameters that could cause a state change multiple times.
	/// These are parameters that can be triggered multiple times and activate/deactivate the pack:
	/// Currency range, etc.
	/// </summary>
	/// <returns>Whether this pack passes defined segmentation with current user progression.</returns>
	public virtual bool CheckSegmentation() {
		OffersManager.LogPack("CHECK SEGMENTATION {0}", Colors.yellow, def.sku);

		// Progression
		UserProfile profile = UsersManager.currentUser;

		OffersManager.LogPack("    SC Balance... {0} vs {1}", Colors.paleYellow, m_scBalanceRange, profile.coins);
		if(!m_scBalanceRange.Contains((float)profile.coins)) return false;

		OffersManager.LogPack("    PC Balance... {0} vs {1}", Colors.paleYellow, m_hcBalanceRange, profile.pc);
		if(!m_hcBalanceRange.Contains((float)profile.pc)) return false;

		// Time since last purchase
		if(m_secondsSinceLastPurchase > 0) {	// Nothing to check if default
			TrackingPersistenceSystem trackingPersistence = HDTrackingManager.Instance.TrackingPersistenceSystem;
			int totalPurchases = (trackingPersistence == null) ? 0 : trackingPersistence.TotalPurchases;
			if(totalPurchases > 0) {	// Ignore if player hasn't yet purchased
				long serverTime = GameServerManager.SharedInstance.GetEstimatedServerTimeAsLong() / 1000L;
				long timeSinceLastPurchase = serverTime - trackingPersistence.LastPurchaseTimestamp;
				OffersManager.LogPack("    Time Since Last Purchase... {0} vs {1}", Colors.paleYellow, m_secondsSinceLastPurchase, timeSinceLastPurchase);
				if(m_secondsSinceLastPurchase > timeSinceLastPurchase) return false;	// Not enough time has passed
			} else {
				OffersManager.LogPack("    Time Since Last Purchase... {0} vs [No Purchases]", Colors.paleYellow, m_secondsSinceLastPurchase);
				return false;
			}
		}

		// All checks passed!
		OffersManager.LogPack("SEGMENTATION CHECKS PASSED! {0}", Colors.yellow, def.sku);
		return true;
	}

	/// <summary>
	/// Check if all conditions required to activate this pack (state, segmentation,
	/// activation, expiration, etc) are met.
	/// </summary>
	/// <returns>Whether the pack can be activated or not.</returns>
	public virtual bool CanBeActivated() {
		// Skip active and expired packs
		OffersManager.Log("        Checking state... {0}", this.state);
		if(this.state != State.PENDING_ACTIVATION) return false;

		// Skip if segmentation conditions are not met for this pack
		OffersManager.Log("        Checking segmentation...");
		if(!this.CheckSegmentation()) return false;

		// Skip if activation conditions are not met for this pack
		OffersManager.Log("        Checking activation...");
		if(!this.CheckActivation()) return false;

		// Also skip if for some reason the pack has expired!
		// [AOC] TODO!! Should it be removed?
		OffersManager.Log("        Checking expiration...");
		if(this.CheckExpiration(false)) return false;

		// All checks passed!
		return true;
	}

	/// <summary>
	/// Change the logic state of the pack.
	/// No validation is done.
	/// </summary>
	/// <param name="_newState">New state for the pack.</param>
	protected virtual void ChangeState(State _newState) {
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

		OffersManager.LogPack("State Changed from {0} to {1} | {2}", Colors.silver, oldState, _newState, def.sku);
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
	public virtual PopupController EnqueuePopupIfPossible(WhereToShow _areaToCheck) {
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
		// Put popup to the queue and return
		PopupController popup = PopupManager.LoadPopup(PopupFeaturedOffer.PATH);
		popup.GetComponent<PopupFeaturedOffer>().InitFromOfferPack(this);
		PopupManager.EnqueuePopup(popup);
		return popup;
	}

	/// <summary>
	/// Apply this pack to current user.
	/// </summary>
	public virtual void Apply() {
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

	/// <summary>
	/// The pack has been displayed in a featured popup.
	/// </summary>
	public void NotifyPopupDisplayed() {
		// Tracking
		// The experiment name is used as offer name        
        string offerName = OffersManager.GenerateTrackingOfferName( m_def );
		HDTrackingManager.Instance.Notify_OfferShown(false, m_def.GetAsString("iapSku"), offerName, m_def.GetAsString("type"));

		// Update control vars and return
		m_viewsCount++;
		m_lastViewTimestamp = GameServerManager.SharedInstance.GetEstimatedServerTime();
	}
	#endregion

	//------------------------------------------------------------------------//
	// STATIC UTILS															  //
	//------------------------------------------------------------------------//
	#region STATIC UTILS
	/// <summary>
	/// Factory method.
	/// Creates and initializes a new offer pack from the given definition node.
	/// </summary>
	/// <returns>The new pack. Never <c>null</c>, can be of any OfferPack subtype.</returns>
	/// <param name="_def">The definition whom we want to create a new offer pack.</param>
	public static OfferPack CreateFromDefinition(DefinitionNode _def) {
		// Check params
		Debug.Assert(_def != null, "Invalid definition!");

		// Create new pack - depends on type
		OfferPack newPack = null;
		Type type = StringToType(_def.GetAsString("type"));
		switch(type) {
			case Type.ROTATIONAL: {
				newPack = new OfferPackRotational();
			} break;
			case Type.FREE: {
				newPack = new OfferPackFree();
			} break;
			default: {
				newPack = new OfferPack();
			} break;
		}

		// Initialize
		newPack.InitFromDefinition(_def);


		// Done!
		return newPack;
	}

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
		if(!_def.Has(_key) || _def.GetAsString(_key) == OffersManager.settings.emptyValue) {
			_def.SetValue(_key, _value);
		}
	}

	/// <summary>
	/// Convert from enum Type to string representation.
	/// </summary>
	/// <returns>The string representation of the given type.</returns>
	/// <param name="_type">Type to be converted.</param>
	public static string TypeToString(Type _type) {
		switch(_type) {
			case Type.PROGRESSION: 	return "progression";
			case Type.PUSHED: 		return "push";
			case Type.ROTATIONAL: 	return "rotational";
			case Type.FREE:			return "free";
		}
		return TypeToString(DEFAULT_TYPE);
	}

	/// <summary>
	/// Parse a string into a Type.
	/// </summary>
	/// <returns>The type corresponding to the given string.</returns>
	/// <param name="_typeStr">String representation of a type to be parsed.</param>
	public static Type StringToType(string _typeStr) {
		switch(_typeStr) {
			case "progression": return Type.PROGRESSION;
			case "push":		return Type.PUSHED;
			case "rotational":	return Type.ROTATIONAL;
			case "free":		return Type.FREE;
		}
		return DEFAULT_TYPE;
	}

    /// <summary>
    /// Convert from enum Curency to string representation.
    /// </summary>
    /// <returns>The string representation of the given currency.</returns>
    /// <param name="_currency">Type to be converted.</param>
    public static string CurrencyToString(UserProfile.Currency _currency)
    {
        switch (_currency)
        {
            case UserProfile.Currency.REAL: return "real";
            case UserProfile.Currency.HARD: return "pc";
            case UserProfile.Currency.SOFT: return "sc";
			case UserProfile.Currency.NONE:	return "none";
        }
        return CurrencyToString(DEFAULT_CURRENCY);
    }


    /// <summary>
    /// Parse a string into a Currency.
    /// </summary>
    /// <returns>The currency corresponding to the given string.</returns>
    /// <param name="_typeStr">String representation of a currency to be parsed.</param>
    public static UserProfile.Currency StringToCurrency(string _currencyStr)
    {
        switch (_currencyStr)
        {
            case "real": return UserProfile.Currency.REAL;
            case "pc": return UserProfile.Currency.HARD;
            case "sc": return UserProfile.Currency.SOFT;
			case "free":
			case "none": return UserProfile.Currency.NONE;
        }
        return DEFAULT_CURRENCY;
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
    public virtual bool ShouldBePersisted() {
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
	public virtual void Load(SimpleJSON.JSONClass _data) {
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
	public virtual SimpleJSON.JSONClass Save() {
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

        if ( m_type == Type.PUSHED ){
            data.Add("customId",  OffersManager.GenerateTrackingOfferName(m_def));
        }
        

		// Done!
		return data;
	}
	#endregion
}