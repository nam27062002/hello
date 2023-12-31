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
using System.Linq;
using Metagame;

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
        REMOVE_ADS,
        FREE,
        REFERRAL,
        SC,
        HC,
		DRAGON_DISCOUNT,
        WELCOME_BACK,
        COUNT
	}

    public const string PROGRESSION = "progression";
    public const string PUSH = "push";
    public const string ROTATIONAL = "rotational";
    public const string REMOVE_ADS = "removeads";
    public const string FREE = "free";
	public const string REFERRAL = "referral";
    public const string SC = "sc";
    public const string HC = "hc";
	public const string DRAGON_DISCOUNT = "dragon_discount";
    public const string WELCOME_BACK = "welcomeback";

    public const string DRAGON_LAST_PROGRESSION = "dragon_last_progression";

	public const string PLATFORM_IOS = "ios";
	public const string PLATFORM_ANDROID = "android";

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

	protected string m_groupId = "";
	public string groupId {
		get {
			if(!string.IsNullOrEmpty(m_groupId)) {
				return m_groupId;
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

	protected bool m_forcedStateChangePending = false;
	protected State m_forcedStateChange = State.PENDING_ACTIVATION;

	public bool isActive {
		get { return m_state == State.ACTIVE; }
	}

	protected string m_shopCategory = string.Empty;
    public string shopCategory { 
        get  {  return m_shopCategory;  }
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
	protected int m_durationRuns = 0;
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
	protected int m_minSpent = 0;
	protected int m_maxSpent = int.MaxValue;
	protected RangeInt m_maxPurchasePrice = null;
	protected int m_minNumberOfPurchases = 0;

	protected long m_secondsSinceLastPurchase = 0;
	protected RangeInt m_lastPurchasePrice = null;
	protected string[] m_lastPurchaseItemType = new string[0];
	protected string[] m_lastPurchaseItemContent = new string[0];

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
	
	protected string[] m_playerSources = new string[0];

	protected string m_platform;

	// Purchase limit
	protected int m_purchaseLimit = 1;
	protected int m_purchaseCount = 0;
	public int purchaseCount {
		get { return m_purchaseCount; }
	}

    // Clustering
    protected string[] m_clusters;
	public string[] clusters { get => m_clusters; set => m_clusters = value; }

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

	protected int m_endRun = 0;
	public int endRun
	{
		get { return m_endRun; }
	}

	public TimeSpan remainingTime {
		get { 
			if(isTimed && isActive) {
				return m_endTimestamp - GameServerManager.GetEstimatedServerTime(); 
			} else {
				return DateTime.MaxValue - GameServerManager.GetEstimatedServerTime();
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
		//OffersManager.LogPack(this, "UpdateState {0} ({1})", Colors.pink, m_def.sku, m_state);
		State oldState = m_state;

		// If a state change is forced, do it
		if(m_forcedStateChangePending) {
			m_forcedStateChangePending = false;
			ChangeState(m_forcedStateChange);
		} else {
			// Based on pack's state
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
		}

		// Has state changed?
		return (oldState != m_state);
	}

	/// <summary>
	/// Change the state of this pack.
	/// No checks will be performed.
	/// </summary>
	/// <param name="_newState">The state to change to.</param>
	/// <param name="_immediate">Whether to do it right now or in the next UpdateState() call.</param>
	public void ForceStateChange(State _newState, bool _immediate) {
		// Immediate?
		if(_immediate) {
			ChangeState(State.PENDING_ACTIVATION);
		} else {
			m_forcedStateChangePending = true;
			m_forcedStateChange = _newState;
		}
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
		m_groupId = string.Empty;
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
		m_durationRuns = 0;
		m_startDate = DateTime.MinValue;
		m_endDate = DateTime.MinValue;
		m_isTimed = false;

		// Optional params
		m_countriesAllowed = new string[0];
		m_countriesExcluded = new string[0];
		m_gamesPlayed = 0;

		m_payerType = PayerType.ANYONE;
		m_minSpent = 0;
		m_maxSpent = int.MaxValue;
		m_maxPurchasePrice = null;
		m_minNumberOfPurchases = 0;

		m_secondsSinceLastPurchase = 0;
		m_lastPurchasePrice = null;
		m_lastPurchaseItemType = new string[0];
		m_lastPurchaseItemContent = new string[0];

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

		m_playerSources = new string[0];

		m_platform = "";

		// Purchase limit
		m_purchaseLimit = 1;
		m_purchaseCount = 0;

		// Clustering
		m_clusters = new string[0];

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
		switch(m_type) {
			case Type.FREE: {
				ecoGroup = HDTrackingManager.EEconomyGroup.SHOP_AD_OFFER_PACK;
            } break;

			case Type.REMOVE_ADS: {
				ecoGroup = HDTrackingManager.EEconomyGroup.SHOP_REMOVE_ADS_PACK;
            } break;

			case Type.DRAGON_DISCOUNT: {
				ecoGroup = HDTrackingManager.EEconomyGroup.DRAGON_DISCOUNT;
			} break;
        }

        // Items - limited to 3 for now
        for (int i = 1; i <= MAX_ITEMS; ++i) {  // [1..N]
												// Create and initialize new item
			OfferPackItem item;
			if (m_type == Type.REFERRAL)
            {
                // In referral offers, the items are defined in the referralRewards table
				item = new OfferPackReferralReward();
			} else
            {
				item = new OfferPackItem();
			}
			
			item.InitFromDefinition(_def, i, ecoGroup);

			// If a reward wasn't generated, the item is either not properly defined or the pack doesn't have this item, don't store it
			if(item.reward == null) continue;
			m_items.Add(item);
		}


        // Unique ID
        m_groupId = m_def.GetAsString("groupId");
        if (String.IsNullOrEmpty(m_groupId)) { // 
            //[MSF] at this point, the ID is always empty.
            //So we have to use the sku and the experiment
            //number as an ID.
            m_groupId = m_def.customizationCode.ToString() + "_" + m_def.sku;
        }

        // Shop category
        m_shopCategory = m_def.GetAsString("shopCategory", m_shopCategory);

		// Params
		// We have just done a Reset(), so variables have the desired default values

		// General
		m_order = _def.GetAsInt("order", m_order);

		// Featuring
		m_featured = _def.GetAsBool("featured", m_featured);
		m_frequency = _def.GetAsInt("frequency", m_frequency);
		m_maxViews = _def.GetAsInt("maxViews", m_maxViews);
		switch(_def.GetAsString("zone", "").ToLowerInvariant()) {
			case "dragonselection":			m_whereToShow = WhereToShow.DRAGON_SELECTION;			break;
			case "dragonselectionafterrun":	m_whereToShow = WhereToShow.DRAGON_SELECTION_AFTER_RUN;	break;
			case "playscreen":				m_whereToShow = WhereToShow.PLAY_SCREEN;				break;
			default:						break;	// Already has the default value
		}

		// Timing
		m_duration = _def.GetAsFloat("durationMinutes", 0f);
		m_durationRuns = def.GetAsInt("durationRuns", 0);
		m_startDate = TimeUtils.TimestampToDate(_def.GetAsLong("startDate", 0), false);
		m_endDate = TimeUtils.TimestampToDate(_def.GetAsLong("endDate", 0), false);
		m_isTimed = (m_endDate > m_startDate) || (m_duration > 0f);


		// Segmentation
		m_minAppVersion = Version.Parse(_def.GetAsString("minAppVersion", "1.0.0"));
		m_countriesAllowed = ParseArray(_def.GetAsString("countriesAllowed"));
		m_countriesExcluded = ParseArray(_def.GetAsString("countriesExcluded"));
		m_gamesPlayed = _def.GetAsInt("gamesPlayed", m_gamesPlayed);


		// Caculate the end run
		if (m_durationRuns > 0)
		{
			m_endRun = m_gamesPlayed + m_durationRuns;
		}

		switch (_def.GetAsString("payerType", "").ToLowerInvariant()) {
			case "payer":		m_payerType = PayerType.PAYER;			break;
			case "nonpayer":	m_payerType = PayerType.NON_PAYER;		break;
			default:			break;	// Already has the default value
		}
		m_minSpent = USDToCents(_def.GetAsFloat("minSpent", m_minSpent / 100f));	// Content in USD (float), we work in cents of USD (int)
		m_maxSpent = USDToCents(_def.GetAsFloat("maxSpent", m_maxSpent / 100f));    // Content in USD (float), we work in cents of USD (int)
		Range parsedRange = ParseRange(_def, "maxPurchasePrice", null);
		if(parsedRange != null) {
			// Content in USD (float), we work in cents of USD (int)
			m_maxPurchasePrice = new RangeInt(
				USDToCents(parsedRange.min),
				USDToCents(parsedRange.max)
			);
		} else {
			m_maxPurchasePrice = null;
		}
		m_minNumberOfPurchases = _def.GetAsInt("minNumberOfPurchases", m_minNumberOfPurchases);

		m_secondsSinceLastPurchase = _def.GetAsLong("minutesSinceLastPurchase", m_secondsSinceLastPurchase / 60L) * 60L;        // Content in minutes, we work in seconds
		parsedRange = ParseRange(_def, "lastPurchasePrice", null);
		if(parsedRange != null) {
			// Content in USD (float), we work in cents of USD (int)
			m_lastPurchasePrice = new RangeInt(
				USDToCents(parsedRange.min),
				USDToCents(parsedRange.max)
			);
		} else {
			m_lastPurchasePrice = null;
		}
		m_lastPurchaseItemType = ParseArray(_def.GetAsString("lastPurchaseItemType"));
		m_lastPurchaseItemContent = ParseArray(_def.GetAsString("lastPurchaseItemContent"));

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

		m_platform = _def.GetAsString("platform").ToLower();

		if (m_platform != "" && m_platform != OffersManager.settings.emptyValue)
		{
			if (m_platform != PLATFORM_ANDROID && m_platform != PLATFORM_IOS)
			{
				Debug.LogError("The platform specified '" + m_platform + "' is not valid. Use 'android' or 'ios'");
			}
		}
			

		m_playerSources = ParseArray(_def.GetAsString("playerSources"));

        m_clusters = ParseArray(_def.GetAsString("clusterId"));

		// Purchase limit
		m_purchaseLimit = _def.GetAsInt("purchaseLimit", m_purchaseLimit);
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
		SetValueIfMissing(ref _def, "groupId", m_groupId.ToString(CultureInfo.InvariantCulture));
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
		SetValueIfMissing(ref _def, "durationRuns", m_durationRuns.ToString(CultureInfo.InvariantCulture));
		SetValueIfMissing(ref _def, "startDate", (TimeUtils.DateToTimestamp(m_startDate, false)).ToString(CultureInfo.InvariantCulture));
		SetValueIfMissing(ref _def, "endDate", (TimeUtils.DateToTimestamp(m_endDate, false)).ToString(CultureInfo.InvariantCulture));

		// Segmentation
		SetValueIfMissing(ref _def, "minAppVersion", m_minAppVersion.ToString(Version.Format.FULL));
		SetValueIfMissing(ref _def, "countriesAllowed", string.Join(";", m_countriesAllowed));
		SetValueIfMissing(ref _def, "countriesExcluded", string.Join(";", m_countriesExcluded));
		SetValueIfMissing(ref _def, "gamesPlayed", m_gamesPlayed.ToString(CultureInfo.InvariantCulture));

		SetValueIfMissing(ref _def, "payerType", "");
		SetValueIfMissing(ref _def, "minSpent", (m_minSpent / 100f).ToString(CultureInfo.InvariantCulture));    // Content in USD (float), we work in cents of USD (int)
		SetValueIfMissing(ref _def, "maxSpent", (m_maxSpent / 100f).ToString(CultureInfo.InvariantCulture));    // Content in USD (float), we work in cents of USD (int)
		if(m_maxPurchasePrice == null) {
			SetValueIfMissing(ref _def, "maxPurchasePrice", string.Empty);
		} else {
			SetValueIfMissing(ref _def, "maxPurchasePrice", string.Join(":", new string[] {
				// Content in USD (float), we work in cents of USD (int)
				(m_maxPurchasePrice.min / 100f).ToString(CultureInfo.InvariantCulture),
				(m_maxPurchasePrice.max / 100f).ToString(CultureInfo.InvariantCulture),
			}));
		}
		SetValueIfMissing(ref _def, "minNumberOfPurchases", m_minNumberOfPurchases.ToString(CultureInfo.InvariantCulture));

		SetValueIfMissing(ref _def, "minutesSinceLastPurchase", (m_secondsSinceLastPurchase / 60L).ToString(CultureInfo.InvariantCulture));
		if(m_lastPurchasePrice == null) {
			SetValueIfMissing(ref _def, "lastPurchasePrice", string.Empty);
		} else {
			SetValueIfMissing(ref _def, "lastPurchasePrice", string.Join(":", new string[] {
				// Content in USD (float), we work in cents of USD (int)
				(m_lastPurchasePrice.min / 100f).ToString(CultureInfo.InvariantCulture),
				(m_lastPurchasePrice.max / 100f).ToString(CultureInfo.InvariantCulture),
			}));
		}
		SetValueIfMissing(ref _def, "lastPurchaseItemType", string.Join(";", m_lastPurchaseItemType));
		SetValueIfMissing(ref _def, "lastPurchaseItemContent", string.Join(";", m_lastPurchaseItemContent));

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

		SetValueIfMissing(ref _def, "playerSources", string.Join(";", m_playerSources));

		SetValueIfMissing(ref _def, "clusterId", string.Join(";", m_clusters));

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
		DateTime serverTime = GameServerManager.GetEstimatedServerTime();
		//OffersManager.LogPack(this, "      CheckActivation {0}", Colors.yellow, m_def.sku);

		// Start date
		if(serverTime < m_startDate) {
			OffersManager.LogPack(this, "      CheckActivation {0}: FAIL! Start Date {1} vs {2}", Color.red, m_def.sku, m_startDate, serverTime);
			return false;
		}

		// Progression
		if(profile.gamesPlayed < m_gamesPlayed) {
			OffersManager.LogPack(this, "      CheckActivation {0}: FAIL! Games Played {1} vs {2}", Color.red, m_def.sku, m_gamesPlayed, profile.gamesPlayed);
			return false;
		}

		int playerProgress = profile.GetPlayerProgress();
		if(playerProgress < m_progressionRange.min) {
			OffersManager.LogPack(this, "      CheckActivation {0}: FAIL! Min Player Progress {1} vs {2}", Color.red, m_def.sku, m_progressionRange.min, playerProgress);
			return false;
		}

		if(profile.eggsCollected < m_openedEggs) {
			OffersManager.LogPack(this, "      CheckActivation {0}: FAIL! Eggs Collected {1} vs {2}", Color.red, m_def.sku, m_openedEggs, profile.eggsCollected);
			return false;
		}

		// Payer profile
		int totalPurchases = (trackingPersistence == null) ? 0 : trackingPersistence.TotalPurchases;
		switch(m_payerType) {
			case PayerType.PAYER: {
				if(totalPurchases == 0) {
					OffersManager.LogPack(this, "      CheckActivation {0}: FAIL! Payer Type {1} (totalPurchases {2})", Color.red, m_def.sku, m_payerType, totalPurchases);
					return false;
				}
			} break;
		}

		// Min/max spent
		float totalSpent = (trackingPersistence == null) ? 0f : trackingPersistence.TotalSpent;
		if(m_minSpent > totalSpent) {
			OffersManager.LogPack(this, "      CheckActivation {0}: FAIL! Min Spent {1} vs {2}", Color.red, m_def.sku, m_minSpent, totalSpent);
			return false;
		}

		if(totalSpent > m_maxSpent) {
			OffersManager.LogPack(this, "      CheckActivation {0}: FAIL! Max Spent {1} vs {2}", Color.red, m_def.sku, m_maxSpent, totalSpent);
			return false;
		}

		if(m_maxPurchasePrice != null) {    // Only check if needed
			int maxPurchasePrice = (trackingPersistence == null) ? -1 : trackingPersistence.MaxPurchasePrice;
			if(!m_maxPurchasePrice.Contains(maxPurchasePrice)) {
				OffersManager.LogPack(this, "      CheckActivation {0}: FAIL! Max Purchase Price {1} vs {2}", Color.red, m_def.sku, m_maxPurchasePrice.ToString(), maxPurchasePrice);
				return false;
			}
		}

		// Min number of purchases
		if(m_minNumberOfPurchases > totalPurchases) {
			OffersManager.LogPack(this, "      CheckActivation {0}: FAIL! Min Number Of Purchases {1} vs {2}", Color.red, m_def.sku, m_minNumberOfPurchases, totalPurchases);
			return false;
		}

		// Last purchase info
		if(m_lastPurchasePrice != null) {	// Only check if needed
			int lastPurchasePrice = (trackingPersistence == null) ? 0 : trackingPersistence.LastPurchasePrice;
			if(!m_lastPurchasePrice.Contains(lastPurchasePrice)) {
				OffersManager.LogPack(this, "      CheckActivation {0}: FAIL! Last Purchase Price {1} vs {2}", Color.red, m_def.sku, m_lastPurchasePrice.ToString(), lastPurchasePrice);
				return false;
			}
		}

		if(m_lastPurchaseItemType.Length > 0) { // Only check if needed
			// Comparison will fail if tracking persistence is not valid
			string lastPurchaseItemType = (trackingPersistence == null) ? string.Empty : trackingPersistence.LastPurchaseItemType;
			bool matchFound = false;
			for(int i = 0; i < m_lastPurchaseItemType.Length; ++i) {
				// Minimize typos by comparing in lowercase
				if(m_lastPurchaseItemType[i].ToLowerInvariant() == lastPurchaseItemType.ToLowerInvariant()) {
					matchFound = true;
					break;	// No need to keep looping
				}
			}
			if(!matchFound) {
				OffersManager.LogPack(this, "      CheckActivation {0}: FAIL! Last Purchase Item Type {1} vs {2}", Color.red, m_def.sku, m_lastPurchaseItemType.ToStringValues(), lastPurchaseItemType);
				return false;
			}

			// Content is only checked if type matches
			// Don't check content if more than one type was defined
			if(m_lastPurchaseItemContent.Length > 0 && m_lastPurchaseItemType.Length == 1) {  // Only check if needed
				// No need to check tracking persistence validity, it will already have failed with the type comparison and we will not reach this point
				// Check all possible values for an exact match - if not found, offer can't be activated
				string lastPurchaseItemContent = trackingPersistence.LastPurchaseItemContent;
				matchFound = false;
				for(int i = 0; i < m_lastPurchaseItemContent.Length; ++i) {
					// Minimize typos by comparing in lowercase
					if(m_lastPurchaseItemContent[i].ToLowerInvariant() == lastPurchaseItemContent.ToLowerInvariant()) {
						matchFound = true;
						break;	// No need to keep looping
					}
				}
				if(!matchFound) {
					OffersManager.LogPack(this, "      CheckActivation {0}: FAIL! Last Purchase Item Content {1} vs {2}", Color.red, m_def.sku, m_lastPurchaseItemContent.ToStringValues(), trackingPersistence.LastPurchaseItemContent);
					return false;
				}
			}
		}

		// Dragons
		for(int i = 0; i < m_dragonUnlocked.Length; ++i) {
            
            // replace "dragon_last_progression" with the actual dragon sku
            string sku = m_dragonUnlocked[i].Replace(DRAGON_LAST_PROGRESSION, DragonManager.lastClassicDragon.sku);

            if(DragonManager.GetDragonData(sku).lockState <= IDragonData.LockState.LOCKED) {
				OffersManager.LogPack(this, "      CheckActivation {0}: FAIL! Unlocked Dragons {1}", Color.red, m_def.sku, sku);
				return false;
			}
		}

		for(int i = 0; i < m_dragonOwned.Length; ++i) {
            
            // replace "dragon_last_progression" with the actual dragon sku
            string sku = m_dragonOwned[i].Replace(DRAGON_LAST_PROGRESSION, DragonManager.lastClassicDragon.sku);

            
			if(!DragonManager.IsDragonOwned(sku))
            {
                OffersManager.LogPack(this, "      CheckActivation {0}: FAIL! Owned Dragons {1}", Color.red, m_def.sku, sku);
				return false;
			}
		}

		// Pets
		if(profile.petCollection.unlockedPetsCount < m_petsOwnedCount) {
			OffersManager.LogPack(this, "      CheckActivation {0}: FAIL! Unlocked Pets Count {1} vs {2}", Color.red, m_def.sku, m_petsOwnedCount, profile.petCollection.unlockedPetsCount);
			return false;
		}

		for(int i = 0; i < m_petsOwned.Length; ++i) {
			if(!profile.petCollection.IsPetUnlocked(m_petsOwned[i])) {
				OffersManager.LogPack(this, "      CheckActivation {0}: FAIL! Owned Pets {1}", Color.red, m_def.sku, m_petsOwned[i]);
				return false;
			}
		}

		// Skins
		for(int i = 0; i < m_skinsUnlocked.Length; ++i) {
			if(profile.wardrobe.GetSkinState(m_skinsUnlocked[i]) == Wardrobe.SkinState.LOCKED) {
				OffersManager.LogPack(this, "      CheckActivation {0}: FAIL! Unlocked Skins {1}", Color.red, m_def.sku, m_skinsUnlocked[i]);
				return false;
			}
		}

		for(int i = 0; i < m_skinsOwned.Length; ++i) {
			if(profile.wardrobe.GetSkinState(m_skinsOwned[i]) != Wardrobe.SkinState.OWNED) {
				OffersManager.LogPack(this, "      CheckActivation {0}: FAIL! Owned Skins {1}", Color.red, m_def.sku, m_skinsOwned[i]);
				return false;
			}
		}

		// Player source - only if needed
		if(m_playerSources.Length > 0) {
			// Only activate if the player source is included in the sources for this offer pack
			string playerSource = trackingPersistence.PlayerSource;
			if(!m_playerSources.Contains(playerSource)) {
				OffersManager.LogPack(this, "      CheckActivation {0}: FAIL! Player Source {1}", Color.red, m_def.sku, playerSource);
				return false;
			}
		}

		// Platform
		if (!String.IsNullOrEmpty(m_platform) && m_platform != OffersManager.settings.emptyValue)
		{
            switch (m_platform)
            {
				case PLATFORM_IOS:
                    if (Application.platform != RuntimePlatform.IPhonePlayer)
                    {
						OffersManager.LogPack(this, "      CheckActivation {0}: FAIL! Platform required: {0}", Color.red, m_def.sku, m_platform);
						return false;
                    }
					break;

				case PLATFORM_ANDROID:
                    if (Application.platform != RuntimePlatform.Android)
                    {
						OffersManager.LogPack(this, "      CheckActivation {0}: FAIL! Platform required: {0}", Color.red, m_def.sku, m_platform);
						return false;
                    }
					break;

				default:
					return false;
            }
		}

		// All checks passed!
		OffersManager.LogPack(this, "      CheckActivation {0}: PASSED!", Colors.lime, m_def.sku);
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
		TrackingPersistenceSystem trackingPersistence = HDTrackingManager.Instance.TrackingPersistenceSystem;
		//OffersManager.LogPack(this, "      CheckExpiration {0} ({1})", Colors.yellow, m_def.sku, _checkTime);

		// Multiple packs may have the same unique ID, with the intention to make 
		// them mutually exclusive.
		// If another pack with the same unique ID is active, mark this one as expired!
		// Resolves issue https://mdc-tomcat-jira100.ubisoft.org/jira/browse/HDK-2026
		for(int i = 0; i < OffersManager.activeOffers.Count; ++i) {
			// Skip if it's ourselves
			if(OffersManager.activeOffers[i] == this) continue;

			// Is there a pack with the same unique ID already active?
			if(OffersManager.activeOffers[i].groupId == this.groupId) {
				// Yes! Mark offer as expired ^^
				OffersManager.LogPack(this, "      CheckExpiration {0}: EXPIRED! Duplicated unique ID {1}", Color.red, m_def.sku, this.groupId);
				return true;
			}
		}

		// Timers - only if requested
		if(_checkTime) {
			if(CheckExpirationByTime()) return true;
		}

		// Check if the offer ends after an amount of runs
		if (endRun != 0 &&  UsersManager.currentUser.gamesPlayed >= endRun)
        {
			return true;
        }

		// Purchase limit (ignore if 0 or negative, unlimited pack)
		if(m_purchaseLimit > 0) {
			if(m_purchaseCount >= m_purchaseLimit) {
				OffersManager.LogPack(this, "      CheckExpiration {0}: EXPIRED! Purchase Limit {1} vs {2}", Color.red, m_def.sku, m_purchaseLimit, m_purchaseCount);
				return true;
			}
		}

		// Main conditions
		if(m_minAppVersion > GameSettings.internalVersion) {
			OffersManager.LogPack(this, "      CheckExpiration {0}: EXPIRED! Min App Version {1} vs {2}", Color.red, m_def.sku, m_minAppVersion, GameSettings.internalVersion);
			return true;
		}

		// Payer profile
		// [AOC] Only for non-payers and if the offer hasn't yet been activated
		//       We don't want an already active offer to disappear because the player has become a payer - bad UX
		//       However, we want to expire non-payer targeted offers if the player is already a payer before they 
		//       are activated, since they will never be active and doesn't make sense to keep them in the pending_activation state (performance-wise)
		//       See User Story: https://confluence.ubisoft.com/pages/viewpage.action?pageId=872683518 
		if(m_state != State.ACTIVE) {
			// Expire if the player is a payer and the offer is targeted to non-payers, since the player can't become a non-payer anymore and thus this offer will never be activated
			int totalPurchases = (trackingPersistence == null) ? 0 : trackingPersistence.TotalPurchases;
			switch(m_payerType) {
				case PayerType.NON_PAYER: {
					if(totalPurchases > 0) {
						OffersManager.LogPack(this, "      CheckExpiration {0}: EXPIRED!  Payer Type {1} (totalPurchases {2})", Color.red, m_def.sku, m_payerType, totalPurchases);
						return true;
					}
				} break;
			}
		}

		// Max spent
		float totalSpent = (trackingPersistence == null) ? 0f : trackingPersistence.TotalSpent;
		if(totalSpent > m_maxSpent) {
			OffersManager.LogPack(this, "      CheckExpiration {0}: EXPIRED! Max Spent {1} vs {2}", Color.red, m_def.sku, m_maxSpent, totalSpent);
			return true;
		}

		// Max purchase price
		// [AOC] Same remark as payer profile (only when not active)
		if(m_state != State.ACTIVE) {
			if(m_maxPurchasePrice != null) {    // Only check if needed
				// Expire if the player's maxPurchasePrice is bigger than the upper limit of the offer's setup, since maxPurchasePrice will never go down and thus this offer will never be activated
				int maxPurchasePrice = (trackingPersistence == null) ? -1 : trackingPersistence.MaxPurchasePrice;
				if(maxPurchasePrice > m_maxPurchasePrice.max) {
					OffersManager.LogPack(this, "      CheckExpiration {0}: EXPIRED! Max Purchase Price {1} vs {2}", Color.red, m_def.sku, m_maxPurchasePrice.ToString(), maxPurchasePrice);
					return true;
				}
			}
		}

		// Progression
		int playerProgress = profile.GetPlayerProgress();
		if(playerProgress > m_progressionRange.max) {
			OffersManager.LogPack(this, "      CheckExpiration {0}: EXPIRED! Max Player Progress {1} vs {2}", Color.red, m_def.sku, m_progressionRange.max, playerProgress);
			return true;
		}

		// Dragons
		for(int i = 0; i < m_dragonNotOwned.Length; ++i) {
            
            // replace "dragon_last_progression" with the actual dragon sku
            string sku = m_dragonNotOwned[i].Replace(DRAGON_LAST_PROGRESSION, DragonManager.lastClassicDragon.sku);

            
			if(DragonManager.IsDragonOwned(sku)) {
				OffersManager.LogPack(this, "      CheckExpiration {0}: EXPIRED! Dragons Not Owned {1}", Color.red, m_def.sku, sku);
				return true;
			}
		}

		// Pets
		for(int i = 0; i < m_petsNotOwned.Length; ++i) {
			if(profile.petCollection.IsPetUnlocked(m_petsNotOwned[i])) {
				OffersManager.LogPack(this, "      CheckExpiration {0}: EXPIRED! Pets Not Owned {1}", Color.red, m_def.sku, m_petsNotOwned[i]);
				return true;
			}
		}

		// Skins
		for(int i = 0; i < m_skinsNotOwned.Length; ++i) {
			if(profile.wardrobe.GetSkinState(m_skinsNotOwned[i]) == Wardrobe.SkinState.OWNED) {
				OffersManager.LogPack(this, "      CheckExpiration {0}: EXPIRED! Skins Not Owned {1}", Color.red, m_def.sku, m_skinsNotOwned[i]);
				return true;
			}
		}

		// Countries
		string countryCode = DeviceUtilsManager.SharedInstance.GetDeviceCountryCode();

		if(m_countriesAllowed.Length > 0 && m_countriesAllowed.IndexOf(countryCode) < 0) {
			OffersManager.LogPack(this, "      CheckExpiration {0}: EXPIRED! Countries Allowed {1}", Color.red, m_def.sku, countryCode);
			return true;
		}

		if(m_countriesExcluded.IndexOf(countryCode) >= 0) {
			OffersManager.LogPack(this, "      CheckExpiration {0}: EXPIRED! Countries Excluded {1}", Color.red, m_def.sku, countryCode);
			return true;
		}

		// Player source - only if needed
		if(m_playerSources.Length > 0) {
			// Don't expire if player source hasn't yet been determined - probably first session
			string playerSource = trackingPersistence.PlayerSource;
			if(playerSource != TrackingPersistenceSystem.PLAYER_SOURCE_UNDEFINED) {
				// If the player source is not in the list of accepted ones for this offer, expire it since player source won't change anymore
				if(!m_playerSources.Contains(playerSource)) {
					OffersManager.LogPack(this, "      CheckExpiration {0}: EXPIRED! Player Source {1}", Color.red, m_def.sku, playerSource);
					return true;
				}
			}
		}

		// All checks passed!
		//OffersManager.LogPack(this, "      CheckExpiration {0}: PASSED!", Colors.lime, m_def.sku);
		return false;
	}

	/// <summary>
	/// Checks the expiration by time exclusively.
	/// </summary>
	/// <returns>Whether this pack has expired by time or not.</returns>
	public virtual bool CheckExpirationByTime() {
		// Never if the offer is not timed
		if(!m_isTimed) {
			OffersManager.LogPack(this, "      CheckExpirationByTime {0}: Offer is not timed!", Color.yellow, m_def.sku);
			return false;
		}

		// Get server time
		DateTime serverTime = GameServerManager.GetEstimatedServerTime();

		// Global end date
		if(m_endDate > DateTime.MinValue && serverTime > m_endDate) {
			OffersManager.LogPack(this, "      CheckExpirationByTime {0}: EXPIRED! End Date {1} vs {2}", Color.red, m_def.sku, m_endDate, serverTime);
			return true;
		}

		// If active, check end timestamp (duration)
		if(isActive && serverTime > m_endTimestamp) {
			OffersManager.LogPack(this, "      CheckExpirationByTime {0}: EXPIRED! End Timestamp {1} && {2} vs {3}", Color.red, m_def.sku, isActive, m_endTimestamp, serverTime);
			return true;
		}

		// All checks passed!
		//OffersManager.LogPack(this, "      CheckExpirationByTime {0}: PASSED!", Colors.lime, m_def.sku);
		return false;
	}

	/// <summary>
	/// Checks parameters that could cause a state change multiple times.
	/// These are parameters that can be triggered multiple times and activate/deactivate the pack:
	/// Currency range, etc.
	/// </summary>
	/// <returns>Whether this pack passes defined segmentation with current user progression.</returns>
	public virtual bool CheckSegmentation() {
		//OffersManager.LogPack(this, "      CheckSegmentation {0}", Colors.yellow, m_def.sku);

		// Progression
		UserProfile profile = UsersManager.currentUser;

		if(!m_scBalanceRange.Contains((float)profile.coins)) {
			OffersManager.LogPack(this, "      CheckSegmentation {0}: FAIL! SC Balance {1} vs {2}", Color.red, m_def.sku, m_scBalanceRange, profile.coins);
			return false;
		}

		if(!m_hcBalanceRange.Contains((float)profile.pc)) {
			OffersManager.LogPack(this, "      CheckSegmentation {0}: FAIL! PC Balance {1} vs {2}", Color.red, m_def.sku, m_hcBalanceRange, profile.pc);
			return false;
		}

		// Time since last purchase
		if(m_secondsSinceLastPurchase > 0) {	// Nothing to check if default
			TrackingPersistenceSystem trackingPersistence = HDTrackingManager.Instance.TrackingPersistenceSystem;
			int totalPurchases = (trackingPersistence == null) ? 0 : trackingPersistence.TotalPurchases;
			if(totalPurchases > 0) {	// Ignore if player hasn't yet purchased
				long serverTime = GameServerManager.GetEstimatedServerTimeAsLong() / 1000L;
				long timeSinceLastPurchase = serverTime - trackingPersistence.LastPurchaseTimestamp;
				if(m_secondsSinceLastPurchase > timeSinceLastPurchase) {    // Not enough time has passed
					OffersManager.LogPack(this, "      CheckSegmentation {0}: FAIL! Time Since Last Purchase {1} vs {2}", Color.red, m_def.sku, m_secondsSinceLastPurchase, timeSinceLastPurchase);
					return false;
				}
			} else {
				OffersManager.LogPack(this, "      CheckSegmentation {0}: FAIL! Time Since Last Purchase {1} vs No Purchases]", Color.red, m_def.sku, m_secondsSinceLastPurchase);
				return false;
			}
		}

		// All checks passed!
		//OffersManager.LogPack(this, "      CheckSegmentation {0}: PASSED!", Colors.lime, m_def.sku);
		return true;
	}

	/// <summary>
	/// Check if all conditions required to activate this pack (state, segmentation,
	/// activation, expiration, etc) are met.
	/// </summary>
	/// <returns>Whether the pack can be activated or not.</returns>
	public virtual bool CanBeActivated() {
		// Log
		OffersManager.Log("    CanBeActivated {0}", m_def.sku);

		// Skip active and expired packs
		if(this.state != State.PENDING_ACTIVATION) {
			OffersManager.Log("    CanBeActivated {0}: FAIL! Not from current state ({1})", Color.red, m_def.sku, this.state);
			return false;
		}

		// Skip if segmentation conditions are not met for this pack
		if(!this.CheckSegmentation()) {
			OffersManager.Log("    CanBeActivated {0}: FAIL! Segmentation check failed", Color.red, m_def.sku);
			return false;
		}

		// Skip if activation conditions are not met for this pack
		if(!this.CheckActivation()) {
			OffersManager.Log("    CanBeActivated {0}: FAIL! Activation check failed", Color.red, m_def.sku);
			return false;
		}

		// Also skip if for some reason the pack has expired!
		// [AOC] TODO!! Should it be removed?
		if(this.CheckExpiration(false)) {
			OffersManager.Log("    CanBeActivated {0}: FAIL! Expiration check failed (pack is expired)", Color.red, m_def.sku);
			return false;
		}

		// All checks passed!
		OffersManager.Log("    CanBeActivated {0}: Yes!", Colors.lime, m_def.sku);
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
					m_activationTimestamp = GameServerManager.GetEstimatedServerTime();

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

		OffersManager.LogPack(this, "ChangeState {0}: State Changed from {1} to {2}", Colors.silver, def.sku, oldState, _newState);
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
		DateTime serverTime = GameServerManager.GetEstimatedServerTime();
		TimeSpan timeSinceLastView = serverTime - m_lastViewTimestamp;
		if(timeSinceLastView.TotalMinutes < m_frequency) return null;

		// All checks passed!
		// Put popup to the queue and return
		PopupController popup = PopupShopOfferPack.LoadPopupForOfferPack(this);
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
		m_lastViewTimestamp = GameServerManager.GetEstimatedServerTime();

        // Make sure we are saving this timestamp, so the offer is not shown twice
		UsersManager.currentUser.SaveOfferPack(this);
	}

    /// <summary>
    /// Count how many items are in this pack that are a dragon or a skin
    /// We need to know this number in order to open the proper info popup in the shop
    /// </summary>
    /// <returns>The amount of dragons/skins in the pack</returns>
    public int GetDragonsSkinsCount ()
    {
		int amount = 0;

        foreach (OfferPackItem it in items)
        {
            if (it.type == RewardDragon.TYPE_CODE || it.type == RewardSkin.TYPE_CODE)
            {
				amount++;
            }
        }

		return amount;
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
            case Type.REFERRAL:	{
			    newPack = new OfferPackReferral();
			} break;
			case Type.REMOVE_ADS: {
                newPack = new OfferPackRemoveAds();
            }  break;
            case Type.SC: {
                newPack = new OfferPackCurrency();
            } break;
            case Type.HC: {
                newPack = new OfferPackCurrency();
            } break;
			case Type.DRAGON_DISCOUNT: {
				newPack = new OfferPackDragonDiscount();
            } break;
            case Type.WELCOME_BACK: {
                newPack = new OfferPackWelcomeBack();
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

		float val;
		if(tokens.Length > 0) {
			if(PersistenceUtils.TryParse<float>(tokens[0], CultureInfo.InvariantCulture, out val)) {
				r.min = val;
			}
		}

		if(tokens.Length > 1) {
			if(PersistenceUtils.TryParse<float>(tokens[1], CultureInfo.InvariantCulture, out val)) {
				r.max = val;
			}
		}

		return r;
	}

	/// <summary>
	/// Parse a range from a DefinitionNode property.
	/// </summary>
	/// <param name="_def">Definition to be parsed.</param>
	/// <param name="_propertyKey">ID of the property containing the range data.</param>
	/// <param name="_defaultValue">Default range data if property not found or not valid.</param>
	/// <returns>New range initialized with data parsed from the input definition-property. <paramref name="_defaultValue"/> if property not found or not valid.</returns>
	public static Range ParseRange(DefinitionNode _def, string _propertyKey, Range _defaultValue = null) {
		// If invalid definition, return default value
		if(_def == null) return _defaultValue;

		// If the definition doesn't have this property, default
		if(!_def.Has(_propertyKey)) return _defaultValue;

		// If property value is empty or default, return default
		string rangeStr = _def.GetAsString(_propertyKey);
		if(string.IsNullOrEmpty(rangeStr) || rangeStr == OffersManager.settings.emptyValue) {
			return _defaultValue;
		}

		// All checks passed! We can proceed with the parsing
		return ParseRange(rangeStr);
	}

	/// <summary>
	/// Custom range parser. Do it here to avoid changing Calety -_-
	/// </summary>
	/// <returns>New range initialized with data parsed from the input string.</returns>
	/// <param name="_str">String to be parsed. Must be in the format "min:max".</param>
	public static RangeInt ParseRangeInt(string _str) {
		string[] tokens = _str.Split(':');
		RangeInt r = new RangeInt(0, int.MaxValue);

		int val;
		if(tokens.Length > 0) {
			if(PersistenceUtils.TryParse<int>(tokens[0], CultureInfo.InvariantCulture, out val)) {
				r.min = val;
			}
		}

		if(tokens.Length > 1) {
			if(PersistenceUtils.TryParse<int>(tokens[1], CultureInfo.InvariantCulture, out val)) {
				r.max = val;
			}
		}

		return r;
	}

	/// <summary>
	/// Parse a range from a DefinitionNode property.
	/// </summary>
	/// <param name="_def">Definition to be parsed.</param>
	/// <param name="_propertyKey">ID of the property containing the range data.</param>
	/// <param name="_defaultValue">Default range data if property not found or not valid.</param>
	/// <returns>New range initialized with data parsed from the input definition-property. <paramref name="_defaultValue"/> if property not found or not valid.</returns>
	public static RangeInt ParseRangeInt(DefinitionNode _def, string _propertyKey, RangeInt _defaultValue = null) {
		// If invalid definition, return default value
		if(_def == null) return _defaultValue;

		// If the definition doesn't have this property, default
		if(!_def.Has(_propertyKey)) return _defaultValue;

		// If property value is empty or default, return default
		string rangeStr = _def.GetAsString(_propertyKey);
		if(string.IsNullOrEmpty(rangeStr) || rangeStr == OffersManager.settings.emptyValue) {
			return _defaultValue;
		}

		// All checks passed! We can proceed with the parsing
		return ParseRangeInt(rangeStr);
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

			case Type.PROGRESSION: 	return PROGRESSION;
			case Type.PUSHED: 		return PUSH;
			case Type.ROTATIONAL: 	return ROTATIONAL;
			case Type.FREE:			return FREE;
			case Type.REFERRAL:     return REFERRAL;
            case Type.REMOVE_ADS:   return REMOVE_ADS;
            case Type.SC:           return SC;
            case Type.HC:           return HC;
			case Type.DRAGON_DISCOUNT:	return DRAGON_DISCOUNT;
            case Type.WELCOME_BACK: return WELCOME_BACK;
		}

		return TypeToString(DEFAULT_TYPE);
	}

	/// <summary>
	/// Parse a string into a Type.
	/// </summary>
	/// <returns>The type corresponding to the given string.</returns>
	/// <param name="_typeStr">String representation of a type to be parsed.</param>
	public static Type StringToType(string _typeStr) {
		switch(_typeStr.ToLowerInvariant()) {

			case PROGRESSION:   return Type.PROGRESSION;
			case PUSH:		return Type.PUSHED;
			case ROTATIONAL:	return Type.ROTATIONAL;
			case FREE:		    return Type.FREE;
			case REFERRAL:      return Type.REFERRAL;
            case REMOVE_ADS:    return Type.REMOVE_ADS;
            case HC:            return Type.HC;
            case SC:            return Type.SC;
			case DRAGON_DISCOUNT:	return Type.DRAGON_DISCOUNT;
            case WELCOME_BACK: return Type.WELCOME_BACK;

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
        switch (_currencyStr.ToLowerInvariant())
        {
            case "real": return UserProfile.Currency.REAL;
			case "hc":
            case "pc": return UserProfile.Currency.HARD;
            case "sc": return UserProfile.Currency.SOFT;
			case "free":
			case "none": return UserProfile.Currency.NONE;
        }
        return DEFAULT_CURRENCY;
    }

	/// <summary>
	/// Convert from USD (used in content) to cents of USD (used in internal logic).
	/// Will be capped to int.MaxValue if needed.
	/// </summary>
	/// <param name="_usd">Amount in USD to be converted.</param>
	/// <returns>The converted amount in cents of USD.</returns>
	private static int USDToCents(float _usd) {
		// Using double cause we can get off float limits
		// [AOC] Have to use Round because of how the float-double conversion works: https://social.msdn.microsoft.com/Forums/vstudio/en-US/1fcf6486-807d-4dce-8aef-7fe5268b568d/convert-float-to-double?forum=csharpgeneral
		double cents = System.Math.Round(_usd * 100d);

		// Check int limits
		// [AOC] WTF Unity doesn't have System.Math.Clamp(double, double, double) - do a manual clamp
		int clampedCents = 0;
		if(cents <= (double)int.MinValue) {
			clampedCents = int.MinValue;
		} else if(cents >= (double)int.MaxValue) {
			clampedCents = int.MaxValue;
		} else {
			clampedCents = (int)cents;
		}
		return clampedCents;
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
				// Yes, so we don't activate it again
				return true;
			} break;
		}

		// No for the rest of cases
		return false;
	}

	/// <summary>
	/// Load state from a persistence object.
	/// </summary>
	/// <param name="_data">The data object loaded from persistence.</param>
	public virtual void Load(SimpleJSON.JSONClass _data) {
		string key = "";
		OffersManager.Log("<color=magenta>LOADING PACK</color> {0} with data {1}", m_def.sku, _data.ToString());

		// State
		key = "state";
		if(_data.ContainsKey(key)) {
			m_state = (State)PersistenceUtils.SafeParse<int>(_data[key]);
		}

		// Purchase count
		key = "purchaseCount";
		if(_data.ContainsKey(key)) {
			m_purchaseCount = PersistenceUtils.SafeParse<int>(_data[key]);
		}

		// View count
		key = "viewCount";
		if(_data.ContainsKey(key)) {
			m_viewsCount = PersistenceUtils.SafeParse<int>(_data[key]);
		}

		// Last view timestamp
		key = "lastViewTimestamp";
		if(_data.ContainsKey(key)) {
			m_lastViewTimestamp = PersistenceUtils.SafeParse<DateTime>(_data[key]);
		}

		// Timestamps
		key = "activationTimestamp";
		if(_data.ContainsKey(key)) {
			m_activationTimestamp = PersistenceUtils.SafeParse<DateTime>(_data[key]);
		}
		key = "endTimestamp";
		if(_data.ContainsKey(key)) {
			m_endTimestamp = PersistenceUtils.SafeParse<DateTime>(_data[key]);
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
		data.Add("state", PersistenceUtils.SafeToString((int)m_state));

		// Optimize by storing less info for expired packs
		bool expired = m_state == State.EXPIRED;

		// Timestamps
		// Only store for timed offers, the rest will be activated upon loading depending on activation triggers
		if(m_isTimed && !expired) {
			data.Add("activationTimestamp", PersistenceUtils.SafeToString(m_activationTimestamp));
			data.Add("endTimestamp", PersistenceUtils.SafeToString(m_endTimestamp));
		}

		// Purchase count - only if needed
		if(m_purchaseCount > 0) {
			data.Add("purchaseCount", PersistenceUtils.SafeToString(m_purchaseCount));
		}

		// View count - only if needed
		// Last view timestamp
		if(m_viewsCount > 0 && !expired) {
			data.Add("viewCount", PersistenceUtils.SafeToString(m_viewsCount));
			data.Add("lastViewTimestamp", PersistenceUtils.SafeToString(m_lastViewTimestamp));
		}

		// Add customization ID for pushed offers
        if ( m_type == Type.PUSHED || m_type == Type.DRAGON_DISCOUNT){
            data.Add("customId",  OffersManager.GenerateTrackingOfferName(m_def));
        }

		OffersManager.Log("<color=magenta>SAVING PACK</color> {0} with data {1}", m_def.sku, data);

		// Done!
		return data;
	}
	#endregion
}