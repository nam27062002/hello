// UserData.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 24/08/2015.
// Copyright (c) 2015 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using System;
using SimpleJSON;
using System.Collections.Generic;
using CodeStage.AntiCheat.ObscuredTypes;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Main user profile. Store here all the user-related data: currencies, stats, 
/// progress, purchases...
/// Singleton class, work with it via its static methods only.
/// IT extends <c>UserSaveSystem</c>, which takes care of technical parameters such as last time it's been saved and so on
/// <see cref="https://youtu.be/64uOVmQ5R1k?t=20m16s"/>
/// </summary>
public class UserProfile : UserPersistenceSystem
{
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	#region currency_constants
	/////// Currency Enum ///////
	public enum Currency {
		NONE = -1,

		SOFT,
		HARD,
		REAL,
		GOLDEN_FRAGMENTS,
		KEYS,

		COUNT
	};

   	// COMPARER. Use this on all your Dictionaries
    public struct CurrencyComparer : IEqualityComparer<Currency>
    {
        public bool Equals(Currency b1, Currency b2)
        {
            return b1 == b2;
        }
        public int GetHashCode(Currency bx)
        {
            return (int)bx;
        }
    }

    public static CurrencyComparer s_currencyComparerComparer = new CurrencyComparer();
	////////////////////////////

	public class CurrencyData {

		public ObscuredLong freeAmount = 0;		// Free amount is restricted to the limit
		public ObscuredLong paidAmount = 0;		// Paid amount can overflow the limits

		public long amount { 
			get { return freeAmount + paidAmount; }
		}

		public long min = 0;
		public long max = -1;	// -1 for unlimited

		/// <summary>
		/// Serialize into a string to store in the persistence json.
		/// </summary>
		public string Serialize() {
			// freeAmount:paidAmount -> "50:20"
			return freeAmount.ToString(PersistenceFacade.JSON_FORMATTING_CULTURE) + ":" + paidAmount.ToString(PersistenceFacade.JSON_FORMATTING_CULTURE);
		}

		/// <summary>
		/// Read values from a string as stored in the persistence json.
		/// </summary>
		/// <param name="_str">Data string, must match the format of the string produced by the Serialize() method.</param>
		/// <param name="_defaultFree">Default value if the free amount is not found.</param>
		/// <param name="_defaultPaid">Default value if the paid amount is not found.</param>
		public void Deserialize(string _str, long _defaultFree = 0, long _defaultPaid = 0) {
			// "50:20" -> freeAmount:paidAmount
			string[] values = _str.Split(':');

			// Parse free amount
			if(values.Length > 0) {
				long tmp;
				long.TryParse(values[0], System.Globalization.NumberStyles.Any, PersistenceFacade.JSON_FORMATTING_CULTURE, out tmp);
				freeAmount = tmp;
			} else {
				freeAmount = _defaultFree;
			}

			// Parse paid amount
			if(values.Length > 1) {
				long tmp;
				long.TryParse(values[1], System.Globalization.NumberStyles.Any, PersistenceFacade.JSON_FORMATTING_CULTURE, out tmp);
				paidAmount = tmp;
			} else {
				paidAmount = _defaultPaid;
			}
		}
	};
	#endregion

	//------------------------------------------------------------------------//
	// PROPERTIES															  //
	//------------------------------------------------------------------------//

	//
	// !!!! Remember to add new properties to the Reset() method!!!
	//

	#region properties
	// Last save timestamp
	private DateTime m_saveTimestamp;
    public DateTime saveTimestamp {
        get { return m_saveTimestamp; }
    }
    public int lastModified { get; set; }

	// User ID shortcut
	public string userId {
		get {
			if(!DebugSettings.useDebugServer && GameSessionManager.SharedInstance.IsLogged()) {
				return GameSessionManager.SharedInstance.GetUID(); 
			} else {
				return "local_user";
			}
		}
	}

    // Economy
    private List<CurrencyData> m_currencies;

	public long coins {
		get { return GetCurrency(Currency.SOFT); }
	}

	public long pc {
		get { return GetCurrency(Currency.HARD); }
	}

	public long goldenEggFragments {
		get { return GetCurrency(Currency.GOLDEN_FRAGMENTS); }
	}

	public long keys {
		get { return GetCurrency(Currency.KEYS); }
	}

	private int m_gemsSpent = 0;
	public int gemsSpent
	{
		get => m_gemsSpent;
		set => m_gemsSpent = value;
	}

	// Game Settings
	private string m_currentDragon;
    public string CurrentDragon
    {
        get { return m_currentDragon; }
        set
        {
            m_currentDragon = value;
        }
    }

	private string m_currentLevel;
	public string currentLevel {
		get { return m_currentLevel; }
		set { m_currentLevel = value; }
	}

	private TutorialStep m_tutorialStep;
	public TutorialStep tutorialStep { 
		get { return m_tutorialStep; }
		set { m_tutorialStep = value; }
	}

	private bool m_furyUsed = false;
	public bool furyUsed {
		get { return m_furyUsed; }
		set { m_furyUsed = value; }
	}




	// Game Stats
	private int m_gamesPlayed;
	public int gamesPlayed {
		get { return m_gamesPlayed; }
		set {
			// Mark tutorial as completed if > 0
			m_gamesPlayed = value;
			SetTutorialStepCompleted(TutorialStep.FIRST_RUN, m_gamesPlayed > 0);
			SetTutorialStepCompleted(TutorialStep.SECOND_RUN, m_gamesPlayed > 1);
		}
	}

	private long m_highScore;
	public long highScore {
		get { return m_highScore; }
		set { m_highScore = value; }
	}

	private long m_totalScore;
	public long totalScore
	{
		get { return m_totalScore; }
		set { m_totalScore = value; }
	}

	private int m_superFuryProgression;
	public int superFuryProgression {
		get { return m_superFuryProgression; }
		set { m_superFuryProgression = value; }
	}

	private int m_firerushesCount;
	public int firerushesCount
	{
		get => m_firerushesCount;
		set => m_firerushesCount = value;
	}

    // Time in seconds (no decimals)
	private int m_boostTime;
	public int boostTime
	{
		get => m_boostTime;
		set => m_boostTime = value;
	}

	// Dragon Data
	private Dictionary<string,IDragonData> m_dragonsBySku;	// Owned Dragons by Sku
	public Dictionary<string,IDragonData> dragonsBySku
	{
		get{ return m_dragonsBySku; }
	}

	// Disguises
	Wardrobe m_wardrobe;
	public Wardrobe wardrobe
	{
		get{ return m_wardrobe; }
	}

	// Pets
	PetCollection m_petCollection;
	public PetCollection petCollection {
		get { return m_petCollection; }
	}

	// Missions
	UserMissions m_userMissions;
	public UserMissions userMissions
	{
		get{ return m_userMissions; }
	}

    UserSpecialMissions m_userSpecialMissions;
    public UserSpecialMissions userSpecialMissions
    {
        get { return m_userSpecialMissions; }
    }

	AchievementsTracker m_achievements;
	public AchievementsTracker achievements
	{
		get{ return m_achievements; }
	}

	// Eggs
	private Egg[] m_eggsInventory;
	public Egg[] eggsInventory {
		get {return m_eggsInventory;}
	}

	private Egg m_incubatingEgg;
	public Egg incubatingEgg
	{
		get{ return m_incubatingEgg;}
		set{ m_incubatingEgg = value;}
	}

	private long m_incubationTimeReference;
	public long incubationTimeReference
	{
		get{ return m_incubationTimeReference; }
		set{ m_incubationTimeReference = value;}
	}

	private DateTime m_incubationEndTimestamp;
	public DateTime incubationEndTimestamp
	{
		get{ return m_incubationEndTimestamp; }
		set{ m_incubationEndTimestamp = value; }
	}

	public int eggsCollected { // Amount of eggs collected (already rewarded) by the user so far
		get; 
		set; 
	}

	private int m_openEggTriesWithoutRares;
	public int openEggTriesWithoutRares {
		get { return m_openEggTriesWithoutRares; }
		set { m_openEggTriesWithoutRares = value; }
	}

    // Chests
    private Chest[] m_dailyChests;
	public Chest[] dailyChests {
		get { return m_dailyChests; }
	}

	private DateTime m_dailyChestsResetTimestamp;
	public DateTime dailyChestsResetTimestamp {
		get{ return m_dailyChestsResetTimestamp; }
		set{ m_dailyChestsResetTimestamp = value; }
	}

	// Remove Mission Ads
	private DateTime m_dailyRemoveMissionAdTimestamp;
	public DateTime dailyRemoveMissionAdTimestamp {
		get{ return m_dailyRemoveMissionAdTimestamp; }
		set{ m_dailyRemoveMissionAdTimestamp = value; }
	}

	private int m_dailyRemoveMissionAdUses;
	public int dailyRemoveMissionAdUses {
		get{ return m_dailyRemoveMissionAdUses; }
		set{ m_dailyRemoveMissionAdUses = value; }
	}

	// Skip Mission Ads
	private DateTime m_skipMissionAdTimestamp;
	public DateTime skipMissionAdTimestamp {
		get{ return m_skipMissionAdTimestamp; }
		set{ m_skipMissionAdTimestamp = value; }
	}

	private int m_skipMissionAdUses;
	public int skipMissionAdUses {
		get{ return m_skipMissionAdUses; }
		set{ m_skipMissionAdUses = value; }
	}

	// Map upgrades
	private DateTime m_mapResetTimestamp;
	public DateTime mapResetTimestamp {
		get{ return m_mapResetTimestamp; }
		set{ m_mapResetTimestamp = value; }
	}

	public bool mapUnlocked {
		// Map is unlocked as long as the timestamp hasn't expired
		get { return m_mapResetTimestamp > GameServerManager.GetEstimatedServerTime(); }
	}

    // True if the player already pressed the map button in the game for the first time
	private bool m_mapDiscovered;
    public bool mapDiscovered
    {
        get { return m_mapDiscovered; }
        set { m_mapDiscovered = value; }
    }

	// Global events
	private Dictionary<int, GlobalEventUserData> m_globalEvents = new Dictionary<int, GlobalEventUserData>();
	public Dictionary<int, GlobalEventUserData> globalEvents {
		get { return m_globalEvents; }
	}

	// Rewards
	private Stack<Metagame.Reward> m_rewards = new Stack<Metagame.Reward>();
	public Stack<Metagame.Reward> rewardStack { get { return m_rewards; } }

	private DailyRewardsSequence m_dailyRewards;
	public DailyRewardsSequence dailyRewards {
		get { return m_dailyRewards; }
	}

	// Offer Packs
    private Dictionary<OfferPack.Type, List<JSONClass>> m_newOfferPersistanceData = new Dictionary<OfferPack.Type, List<JSONClass>>();
    public Dictionary<OfferPack.Type, List<JSONClass>> newOfferPersistanceData {
        get { return m_newOfferPersistanceData; }
    }

	public Dictionary<OfferPack.Type, Queue<string>> m_offersHistory = new Dictionary<OfferPack.Type, Queue<string>>();
	public Dictionary<OfferPack.Type, Queue<string>> offersHistory {
		get { return m_offersHistory; }
	}

	public DateTime freeOfferCooldownEndTime {
		get;
		set;
	}

	// Shopping data
	private bool m_enteredShop = false;
	public bool hasEnteredShop
	{
		get => m_enteredShop;
		set => m_enteredShop = value;
	}

	private bool m_progressionPacksDiscovered = false;
	public bool progressionPacksDiscovered
	{
		get => m_progressionPacksDiscovered;
		set => m_progressionPacksDiscovered = value;
	}

	private int m_maxTransactionPrice = 0; // in USD cents
	public int maxTransactionPrice
	{
		get => m_maxTransactionPrice;
		set => m_maxTransactionPrice = value;
	}


	// Happy hour
	private HappyHourManager.SaveData m_happyHourData = new HappyHourManager.SaveData();
	public HappyHourManager.SaveData happyHourData {
		get { return m_happyHourData; }
	}

    // Remove ads feature
    private RemoveAdsFeature m_removeAds;
    public RemoveAdsFeature removeAds
    {   get { return m_removeAds; }
        set { m_removeAds = value; }
    }

	// Referral install
	private string m_referrerUserId;
	public string referrerUserId
	{
		get { return m_referrerUserId; }
		set { m_referrerUserId = value; }
	}

	private ReferralManager.State m_referralState;
	public ReferralManager.State referralState {
		get { return m_referralState; }
		set { m_referralState = value; }
	}

	private int m_totalReferrals;
	public int totalReferrals
	{
		get { return m_totalReferrals; }
        set { m_totalReferrals = value;  }
	}

	private int m_invitesSent;
	public int invitesSent
	{
		get { return m_invitesSent; }
		set { m_invitesSent = value; }
	}


	// List of unlocked referral rewards (milestones) that havent been claimed yet
	private List<OfferPackReferralReward> m_unlockedReferralRewards = new List<OfferPackReferralReward>();
	public List<OfferPackReferralReward> unlockedReferralRewards
	{
		get { return m_unlockedReferralRewards; }
		set { m_unlockedReferralRewards = value; }
	}


	// Baby dragons - Extra gem granted
	private bool m_babyDragonExtraGemGranted;
    public bool babyDragonExtraGemGranted
    {
        get { return m_babyDragonExtraGemGranted; }
        set { m_babyDragonExtraGemGranted = value; }
    }

    // Baby dragons - Extra gem failed counter
	private int m_babyDragonExtraGemFailedCounter;
    public int babyDragonExtraGemFailedCounter
    {
        get { return m_babyDragonExtraGemFailedCounter; }
        set { m_babyDragonExtraGemFailedCounter = value; }
    }

	// Clustering
	private string m_clusterId;	// Use Getter and Setter
	private bool m_clusterSynced = true;	// Is the cluster ID in sync with the server?
	public bool clusterSynced {
		get { return m_clusterSynced; }
		set { m_clusterSynced = value; }
	}

	// Remove Ads
	private bool m_removeAdsOfferActive;

	// Missions and map timers
    private int m_easyMissionCooldownsLeft;
    private int m_mediumMissionCooldownsLeft;
    private int m_hardMissionCooldownsLeft;
    private DateTime m_easyMissionCooldownTimestamp;    // Timestamp when the mission skips will be restored
    private DateTime m_mediumMissionCooldownTimestamp;
    private DateTime m_hardMissionCooldownTimestamp;
    private DateTime m_mapRevealTimestamp;


    // public List<string> m_visitedZones = new List<string>();
    public HashSet<string> m_visitedZones = new HashSet<string>();
	//--------------------------------------------------------------------------

    // Treat this enum values as if they were skus since they're persisted in user's profile
    public enum ESocialState
    {
        NeverLoggedIn,
        LoggedIn,
        LoggedInAndIncentivised
    };

    private static List<string> smSocialStatesAsString;
    private static List<string> SocialStatesAsString
    {
        get
        {
            if (smSocialStatesAsString == null)
            {
                smSocialStatesAsString = new List<string>();
                int count = Enum.GetValues(typeof(ESocialState)).Length;
                for (int i = 0; i < count; i++)
                {
                    smSocialStatesAsString.Add(((ESocialState)i).ToString());
                }
            }

            return smSocialStatesAsString;
        }
    }    

    public ESocialState SocialState { get; set; }

    public string GivenTransactions { get; set; }

	#endregion

	//
	// New variables here: Remember to initialize them in Reset()
	//

	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	#region generic_methods
	/// <summary>
	/// Default constructor.
	/// </summary>
	public UserProfile()
	{        
    }

	/// <summary>
	/// 
	/// </summary>
	~UserProfile()
	{
        Destroy();
	}

	/// <summary>
	/// 
	/// </summary>
    public override void Reset()
    {        
        Destroy();

        base.Reset();

        m_saveTimestamp = DateTime.UtcNow;
        lastModified = 0;

        if (m_currencies == null)
        {
            m_currencies = new List<CurrencyData>();

            // Initialize currencies to 0
            for (int i = 0; i < (int)Currency.COUNT; ++i)
            {
                m_currencies.Add(new CurrencyData());
            }
        }

        // Define some custom values
        m_currencies[(int)Currency.KEYS].max = 10;  // [AOC] TODO!! Get from content
        
        m_currentDragon = "";
        m_currentLevel = "";

        m_tutorialStep = TutorialStep.ALL;   
         
        m_furyUsed = false;
        m_gamesPlayed = 0;
        m_highScore = 0;
        m_superFuryProgression = 0;

        // Dragons: The Dictionay and IDragonData objects that contains are created only the first time because there are references to these objects somewhere else, so we just reset them
        List<DefinitionNode> defs = DefinitionsManager.SharedInstance.GetDefinitionsList(DefinitionsCategory.DRAGONS);
        if (m_dragonsBySku == null)
        {
            m_dragonsBySku = new Dictionary<string, IDragonData>();
        }
                        
        IDragonData newDragonData = null;
        string dragonSku;
        for (int i = 0; i < defs.Count; i++)
        {
            dragonSku = defs[i].sku;
            if (m_dragonsBySku.ContainsKey(dragonSku))
            {
                m_dragonsBySku[dragonSku].ResetLoadedData();
            }
            else
            {
				newDragonData = IDragonData.CreateFromDef(defs[i]);
                m_dragonsBySku[defs[i].sku] = newDragonData;
            }
        }        

        // Disguises
        m_wardrobe = new Wardrobe();
        m_petCollection = new PetCollection();

		// Missions and achievements
		if(m_userMissions != null) m_userMissions.ClearAllMissions();
        m_userMissions = new UserMissions();

        if (m_userSpecialMissions != null) m_userSpecialMissions.ClearAllMissions();
        m_userSpecialMissions = new UserSpecialMissions();

        m_achievements = new AchievementsTracker();

        m_eggsInventory = new Egg[EggManager.INVENTORY_SIZE];
        m_incubatingEgg = null;
        m_incubationTimeReference = 0;
        m_incubationEndTimestamp = DateTime.MinValue;
        eggsCollected = 0;
		m_openEggTriesWithoutRares = 0;

        m_dailyChests = new Chest[ChestManager.NUM_DAILY_CHESTS];   // Should always have the same length
        m_dailyChestsResetTimestamp = DateTime.MinValue;
        m_dailyRemoveMissionAdTimestamp = DateTime.MinValue;
        m_dailyRemoveMissionAdUses = 0;

        m_skipMissionAdTimestamp = DateTime.MinValue;
        m_skipMissionAdUses = 0;

        m_mapResetTimestamp = DateTime.MinValue;        
        
        m_globalEvents = new Dictionary<int, GlobalEventUserData>();    
    
        m_rewards = new Stack<Metagame.Reward>();
		Debug.Log(Colors.cyan.Tag("CREATING NEW DAILY REWARDS SEQUENCE (Reset)"));
		m_dailyRewards = new DailyRewardsSequence();

        m_newOfferPersistanceData = new Dictionary<OfferPack.Type, List<JSONClass>>();
		m_offersHistory = new Dictionary<OfferPack.Type, Queue<string>>();
        for (int i = 0; i < (int)OfferPack.Type.COUNT; i++) {
            m_newOfferPersistanceData.Add((OfferPack.Type)i, new List<JSONClass>());
			m_offersHistory.Add((OfferPack.Type)i, new Queue<string>());
        }

        m_visitedZones = new HashSet<string>();

        SocialState = ESocialState.NeverLoggedIn;

        GivenTransactions = null;

        // Remove Ads Offer
        m_removeAds = new RemoveAdsFeature();

		// Referrals
		m_totalReferrals = 0;
		m_unlockedReferralRewards.Clear();
		m_invitesSent = 0;

		// Baby Dragons
		m_babyDragonExtraGemFailedCounter = 0;
		m_babyDragonExtraGemGranted = false;

		// Clustering
		m_clusterId = "";
		m_clusterSynced = true;
	}

	/// <summary>
	/// 
	/// </summary>
    private void Destroy()
    {
        if (m_achievements != null)
        {
            m_achievements.Dispose();
            m_achievements = null;
        }

        if ( m_userMissions != null )
        {
        	m_userMissions.ClearAllMissions();
        	m_userMissions = null;
        }

        if (m_userSpecialMissions != null)
        {
            m_userSpecialMissions.ClearAllMissions();
            m_userSpecialMissions = null;
        }
    }

	/// <summary>
	/// Return a string representation of this class.
	/// </summary>
	/// <returns>A formatted json string representing this class.</returns>
	public override string ToString() {
		return ToJson().ToString();
	}
	#endregion

	//------------------------------------------------------------------------//
	// CURRENCIES MANAGEMENT METHODS										  //
	//------------------------------------------------------------------------//
	#region currencies
	/// <summary>
	/// Get current amount of any currency.
	/// </summary>
	/// <returns>The current amount of the required currency.</returns>
	/// <param name="_currency">Currency type.</param>
	public long GetCurrency(Currency _currency) {
		return m_currencies[(int)_currency].amount;
	}

    /// <summary>
    /// Add any type of currency. If free, max limit will be checked.
    /// </summary>
    /// <param name="_amount">Amount to be added.</param>
    /// <param name="_currency">Currency type.</param>
    /// <param name="_paid">Store the amount in the "paid" count. Limits will be ignored.</param>
    /// <param name="_paid">Economy group where the currency comes from. Use <c>HDTrackingManager.EEConomyGroup.CHEAT</c> if that currency comes from a cheat so it
    /// won't be tracked.</param>
    public void EarnCurrency(Currency _currency, ulong _amount, bool _paid, HDTrackingManager.EEconomyGroup _economyGroup) {
		// Gather currency data
		CurrencyData data = m_currencies[(int)_currency];
		long toAdd = (long)_amount;

		// Clamp to limit (if not paid)
		if(!_paid && data.max > 0) {	// Ignore if there is no max
			long finalTotalAmount = data.amount + toAdd;	// Predicted amount
			if(finalTotalAmount > data.max) toAdd = finalTotalAmount - data.max;	// Add as much as we can
		}

		// [AOC] DISCLAIMER: At this point we could break if the amount to be added is 0. 
		//		 However, we're finishing the flow anyway since it's harmless and might 
		//		 be expected in some cases to broadcast the event even when earnt amount is 0.

		// Set the new value!
		long oldAmount = data.amount;	// Tracking purposes
		if(_paid) {
			data.paidAmount += toAdd;
		} else {
			data.freeAmount += toAdd;
		}

		// Notify game!
		// [AOC] For now we don't need to differientate earnings from paid and free sources, neither do we need to differientate earnings and expenses, so use the same event for everything
		Messenger.Broadcast<UserProfile.Currency, long, long>(MessengerEvents.PROFILE_CURRENCY_CHANGED, _currency, oldAmount, data.amount);

        if (_economyGroup != HDTrackingManager.EEconomyGroup.CHEAT && toAdd > 0) {
            HDTrackingManager.Instance.Notify_EarnResources(_economyGroup, _currency, (int)toAdd, (int)data.amount, _paid);
        }
    }

	/// <summary>
	/// Subtract from any type of currency. Min limit will be checked.
	/// Arbitrarily, free currency will be used before paid currency, but can't be manually choosen which source to use.
	/// </summary>
	/// <param name="_amount">Amount to be subtracted.</param>
	/// <param name="_currency">Currency type.</param>
	public void SpendCurrency(Currency _currency, ulong _amount) {
		// Gather currency data
		CurrencyData data = m_currencies[(int)_currency];
		long toSpend = (long)_amount;

		// Clamp amount to subtract to min limit
		long predictedAmount = data.amount - toSpend;
		if(predictedAmount < data.min) toSpend = data.amount - data.min;	// Maximum amount that we can spend

		// [AOC] DISCLAIMER: At this point we could break if the amount to be subtracted is 0. 
		//		 However, we're finishing the flow anyway since it's harmless and might 
		//		 be expected in some cases to broadcast the event even when spent amount is 0.

		// Set the new value!
		long oldAmount = data.amount;	// Tracking purposes

        // Clustering purposes
        if (_currency == Currency.HARD) {
			m_gemsSpent += (int) _amount;
        }

		// Consume free currency first, as much as possible
		long partialAmount = (long)Mathf.Min(data.freeAmount, toSpend);
		data.freeAmount -= partialAmount;
		toSpend -= partialAmount;

		// If there is still amount to spend, take it from the paid balance
		if(toSpend > 0) {
			partialAmount = (long)Mathf.Min(data.paidAmount, toSpend);	// That should always be the toSpend, since we checked the limits earlier
			data.paidAmount -= partialAmount;
		}

		// Notify game!
		// [AOC] For now we don't need to differientate earnings from paid and free sources, neither do we need to differientate earnings and expenses, so use the same event for everything
		Messenger.Broadcast<UserProfile.Currency, long, long>(MessengerEvents.PROFILE_CURRENCY_CHANGED, _currency, oldAmount, data.amount);
	}

	/// <summary>
	/// Gets the currency min value.
	/// </summary>
	/// <returns>The minimum amount user can have of a specific currency.</returns>
	/// <param name="_currency">Currency type.</param>
	public long GetCurrencyMin(Currency _currency) {
		return m_currencies[(int)_currency].min;
	}

	/// <summary>
	/// Gets the currency max value.
	/// </summary>
	/// <returns>The maximum amount user can have of a specific currency. -1 if unlimited.</returns>
	/// <param name="_currency">Currency type.</param>
	public long GetCurrencyMax(Currency _currency) {
		return m_currencies[(int)_currency].max;
	}

	/// <summary>
	/// Gets the data object of a specific currency.
	/// Mostly for debug purposes. Use with caution!
	/// </summary>
	/// <returns>The currency data of the requested currency.</returns>
	/// <param name="_currency">Target currency.</param>
	public CurrencyData GetCurrencyData(Currency _currency) {
		return m_currencies[(int)_currency];
	}

	/// <summary>
	/// Directly sets the given amounts to the target currency, overriding current values.
	/// Mostly for debug purposes. Use with caution!
	/// </summary>
	/// <param name="_currency">Target currency.</param>
	/// <param name="_freeAmount">Free amount to set.</param>
	/// <param name="_paidAmount">Paid amount to set.</param>
	public void SetCurrency(Currency _currency, long _freeAmount, long _paidAmount) {
		// Reset current amount manually
		CurrencyData data = GetCurrencyData(_currency);
		data.freeAmount = 0;
		data.paidAmount = 0;

		// Use earn method so limits are checked and events broadcasted
		UsersManager.currentUser.EarnCurrency(_currency, (ulong)_freeAmount, false, HDTrackingManager.EEconomyGroup.CHEAT);
		UsersManager.currentUser.EarnCurrency(_currency, (ulong)_paidAmount, true, HDTrackingManager.EEconomyGroup.CHEAT);
	}

	/// <summary>
	/// Convert currency from enum to string.
	/// </summary>
	public static string CurrencyToSku(Currency _currency) {
		switch(_currency) {
			case Currency.SOFT: return "sc";
			case Currency.HARD: return "hc";
			case Currency.GOLDEN_FRAGMENTS: return "goldenFragments";
			case Currency.KEYS: return "keys";
			case Currency.REAL: return "money";
		}
		return string.Empty;
	}

	/// <summary>
	/// Convert currency from string to enum.
	/// </summary>
	public static Currency SkuToCurrency(string _sku) {
		switch(_sku) {
			case "sc": return Currency.SOFT;
			case "hc": return Currency.HARD;
			case "pc": return Currency.HARD;
			case "goldenFragments": return Currency.GOLDEN_FRAGMENTS;
			case "keys": return Currency.KEYS;
			case "money": return Currency.REAL;
		}
		return Currency.NONE;
	}
	#endregion

	//------------------------------------------------------------------------//
	// TUTORIAL																  //
	// To simplify bitmask operations										  //
	//------------------------------------------------------------------------//
	#region tutorial
	/// <summary>
	/// Check whether a tutorial step has been completed by this user.
	/// </summary>
	/// <returns><c>true</c> if <paramref name="_step"/> is marked as completed in this profile; otherwise, <c>false</c>.</returns>
	/// <param name="_step">The tutorial step to be checked. Can also be a composition of steps (e.g. (TutorialStep.STEP_1 | TutorialStep.STEP_2), in which case all steps will be tested).</param>
	public bool IsTutorialStepCompleted(TutorialStep _step) {
		// Special case for NONE: ignore
		if(_step == TutorialStep.INIT) return true;

		return (m_tutorialStep & _step) != 0;
	}

	/// <summary>
	/// Mark/unmark a tutorial step as completed.
	/// </summary>
	/// <param name="_step">The tutorial step to be marked. Can also be a composition of steps (e.g. (TutorialStep.STEP_1 | TutorialStep.STEP_2), in which case all steps will be marked).</param>
	/// <param name="_completed">Whether to mark it as completed or uncompleted.</param>
	public void SetTutorialStepCompleted(TutorialStep _step, bool _completed = true) {
		// Special case for NONE: ignore
		if(_step == TutorialStep.INIT) return;

		bool wasCompleted = IsTutorialStepCompleted(_step);
		if(_completed) {
			m_tutorialStep |= _step;
		} else {
			m_tutorialStep &= ~_step;
		}

		// Notify game (only if value has changed)
		if(wasCompleted != _completed) {
			Messenger.Broadcast(MessengerEvents.TUTORIAL_STEP_TOGGLED, _step, _completed);
		}
	}

	/// <summary>
	/// Check whether the player has played a specific amount of games or not.
	/// </summary>
	/// <returns><c>true</c> if the player has played AT LEAST the given amount of games, <c>false</c> otherwise.</returns>
	/// <param name="_toCheck">Number of games to check.</param>
	public bool HasPlayedGames(int _toCheck) {
		return m_gamesPlayed >= _toCheck;
	}
	#endregion

	//------------------------------------------------------------------------//
	// MAP MANAGEMENT METHODS												  //
	//------------------------------------------------------------------------//
	#region map
	/// <summary>
	/// Increases the map level.
	/// Doesn't perform any check or currency transaction, resets timer.
	/// Broadcasts the PROFILE_MAP_UNLOCKED event.
	/// </summary>
	public void UnlockMap() {
		// Reset timer to the start of the following day, in local time zone
		// [AOC] Small trick to figure out the start of a day, from http://stackoverflow.com/questions/3362959/datetime-now-first-and-last-minutes-of-the-day
		//DateTime tomorrow = DateTime.Now.Date.AddDays(1);	// Using local time zone to compute tomorrow's date
		//m_mapResetTimestamp = tomorrow.Date.ToUniversalTime();	// Work in UTC

		// [AOC] Testing purposes
		//m_mapResetTimestamp = DateTime.Now.AddSeconds(30).ToUniversalTime();

		// [AOC] Fuck it! Easier implementation, fixed timer from the moment you unlock the map
		DefinitionNode gameSettingsDef = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.SETTINGS, "gameSettings");
		if(gameSettingsDef != null) {
			m_mapResetTimestamp = GameServerManager.GetEstimatedServerTime().AddMinutes(gameSettingsDef.GetAsDouble("miniMapTimer"));	// Minutes
		} else {
			m_mapResetTimestamp = GameServerManager.GetEstimatedServerTime().AddHours(24);	// Default timer just in case
		}
		Broadcaster.Broadcast(BroadcastEventType.PROFILE_MAP_UNLOCKED);
	}

    /// <summary>
    /// Unlock the map for a specified time duration
    /// Doesn't perform any check or currency transaction, resets timer.
    /// Broadcasts the PROFILE_MAP_UNLOCKED event.
    /// </summary>
    /// <param name="seconds">Duration of the map reveal, in seconds</param>
    public void UnlockMap(int seconds)
    {
        m_mapResetTimestamp = GameServerManager.GetEstimatedServerTime().AddSeconds(seconds);   // Default timer just in case
        
        Broadcaster.Broadcast(BroadcastEventType.PROFILE_MAP_UNLOCKED);
    }
	#endregion

	//------------------------------------------------------------------------//
	// PUBLIC PERSISTENCE METHODS											  //
	//------------------------------------------------------------------------// 
	#region persistence
	public override void Load()
    {
        base.Load();

        string jsonAsString = m_persistenceData.ToString();
        if (jsonAsString != null)
        {
			#if UNITY_EDITOR
			PrintJsonString(jsonAsString, "<color=cyan>LOADING USER PROFILE:</color>\n");
			#endif

			JSONNode json = JSON.Parse(jsonAsString);
            Load(json);


        }

        // Load Remove Ads offer from player prefs
        m_removeAds.InitializeFromDefinition();
        string jsonString = PlayerPrefs.GetString("removeAds");
        if (!string.IsNullOrEmpty(jsonString))
        {
            m_removeAds.Load(SimpleJSON.JSON.Parse(jsonString));
        }
    }

    public override void Save()
    {
        base.Save();

        // Update timestamp
        m_saveTimestamp = DateTime.UtcNow;

        JSONNode json = ToJson();
		m_persistenceData.Merge(json.ToString(), false);

        // Save remove ads in player prefs
        SimpleJSON.JSONNode jsonNode = m_removeAds.Save();
        if (jsonNode != null)
        {
            string jsonString = jsonNode.ToString();
            PlayerPrefs.SetString("removeAds", jsonString);
        }

#if UNITY_EDITOR
        PrintJsonString(json.ToString(), "<color=cyan>SAVING USER PROFILE:</color>\n");

		JSONArray rotationalsArrayData = new JSONArray();
		foreach(string offerSku in m_offersHistory[OfferPack.Type.ROTATIONAL]) {
			rotationalsArrayData.Add(offerSku);
		}

		// ROTATIONALS DEBUG
		/*
			JSONClass offerHistoryData = new SimpleJSON.JSONClass();
			offerHistoryData.Add(OfferPack.TypeToString(OfferPack.Type.ROTATIONAL), rotationalsArrayData);
			JsonFormatter fmt = new JsonFormatter();
			string printStr = fmt.PrettyPrint(offerHistoryData.ToString());
			Debug.Log(Colors.yellow.Tag("--------- SAVING ROTATIONALS ---------") + "\n" + printStr);
		*/
#endif
	}

#if UNITY_EDITOR
	/// <summary>
	/// Split into different logs to make sure everything is printed.
	/// </summary>
	/// <param name="_jsonString"></param>
	/// <param name="_header"></param>
	private void PrintJsonString(string _jsonString, string _header) {
		// Pretty print the json
		JsonFormatter fmt = new JsonFormatter();
		string printStr = fmt.PrettyPrint(_jsonString);

		// Because the Unity console has a character limit per log, split it into several logs
		int CHAR_LIMIT = 10000; // [AOC] Done by manually testing - actual limit is 16297, but we need to add some extra room for the call stack
		int idx = 0;
		int substrLength = 0;
		int loopLimit = 10;
		while(idx < printStr.Length && loopLimit > 0) {
			if(idx == 0) {
				substrLength = Mathf.Min(CHAR_LIMIT - _header.Length, printStr.Length - _header.Length - idx);   // Allow some room for the header
				Debug.Log(_header + printStr.Substring(idx, substrLength));    
			} else {
				substrLength = Mathf.Min(CHAR_LIMIT, printStr.Length - idx);
				Debug.Log(printStr.Substring(idx, substrLength));
			}
			idx += substrLength;
			loopLimit--;
		}

		// Print non-pretty json as well for those who like it hardcore
		Debug.Log(_jsonString);
	}
#endif
	#endregion

	//------------------------------------------------------------------------//
	// PERSISTENCE LOAD METHODS												  //
	//------------------------------------------------------------------------//
	#region load
	/// <summary>
	/// Load state from a json object.
	/// </summary>
	/// <param name="_data">The data object loaded from persistence.</param>
	private void Load(SimpleJSON.JSONNode _data) {
		// Aux vars
		string key;

		// Just read values from persistence object
		SimpleJSON.JSONNode profile = _data["userProfile"];

        if (profile.ContainsKey("timestamp"))
        {
			m_saveTimestamp = PersistenceUtils.SafeParse<DateTime>(profile["timestamp"]);
        }
        else
        {
            m_saveTimestamp = DateTime.UtcNow;
        }

        // Economy
		m_currencies[(int)Currency.SOFT].Deserialize(profile.ContainsKey("sc") ? (string)profile["sc"] : "");
		m_currencies[(int)Currency.HARD].Deserialize(profile.ContainsKey("pc") ? (string)profile["pc"] : "");
		m_currencies[(int)Currency.GOLDEN_FRAGMENTS].Deserialize(profile.ContainsKey("gf") ? (string)profile["gf"] : "");
		m_currencies[(int)Currency.KEYS].Deserialize(profile.ContainsKey("keys") ? (string)profile["keys"] : "", 0, 0);

		key = "gemsSpent";
		if (profile.ContainsKey(key))
		{
			m_gemsSpent = PersistenceUtils.SafeParse<int>(profile[key]);
		}
		else
		{
			m_gemsSpent = 0;
		}


		// Game settings
		if ( profile.ContainsKey("currentDragon") )
			m_currentDragon = profile["currentDragon"];
		else
            m_currentDragon = "";

		if ( profile.ContainsKey("currentLevel") )
			m_currentLevel = profile["currentLevel"];
		else
			m_currentLevel = "";


        key = "tutorialStep";
        if (profile.ContainsKey(key)) {
            m_tutorialStep = (TutorialStep)PersistenceUtils.SafeParse<int>(profile[key]);
        } else {
            m_tutorialStep = (TutorialStep)0;
        }

        key = "furyUsed";
        if (profile.ContainsKey(key)) {
            m_furyUsed = PersistenceUtils.SafeParse<bool>(profile[key]);
        } else {
            m_furyUsed = false;
        }

		// Game stats
		key = "gamesPlayed";
        if (profile.ContainsKey(key)) {
            m_gamesPlayed = PersistenceUtils.SafeParse<int>(profile[key]);
        } else {
            m_gamesPlayed = 0;
        }

        key = "highScore";
        if (profile.ContainsKey(key)) {
            m_highScore = PersistenceUtils.SafeParse<long>(profile[key]);
        }
        else {
            m_highScore = 0;
        }

        key = "superFuryProgression";
        if (profile.ContainsKey(key)) {
            m_superFuryProgression = PersistenceUtils.SafeParse<int>(profile[key]);
        }
        else {
            m_superFuryProgression = 0;
        }

        key = "socialState";
        SocialState = ESocialState.NeverLoggedIn;
        if (profile.ContainsKey(key)) {
            int count = Enum.GetValues(typeof(ESocialState)).Length;
            string value = profile["socialState"];

            // Workaround to prevent old clients from breaking after fixing a typo in the constant
            if (value == "LoggedInAndInventivised")
            {
                value = ESocialState.LoggedInAndIncentivised.ToString();
            }

            int index = SocialStatesAsString.IndexOf(value);
            if (index == -1) {                
                Debug.LogError("USER_PROFILE: " + value + " is not a valid ESocialState");                
            } else {
                SocialState = (ESocialState)index;
            }
        }

		key = "firerushesCount";
		if (profile.ContainsKey(key)) {
			m_firerushesCount = PersistenceUtils.SafeParse<int>(profile[key]);
		} else {
			m_firerushesCount = 0;
		}

		key = "totalScore";
		if (profile.ContainsKey(key)) {
			m_totalScore = PersistenceUtils.SafeParse<long>(profile[key]);
		} else {
			m_totalScore = 0;
		}

		key = "boostTime";
		if (profile.ContainsKey(key))		{
			m_boostTime = PersistenceUtils.SafeParse<int>(profile[key]);
		} else {
			m_boostTime = 0;
		}

		// Shopping data
		key = "enteredShop";
		if (profile.ContainsKey(key))
		{
			m_enteredShop = PersistenceUtils.SafeParse<bool>(profile[key]);
		} else
		{
			m_enteredShop = false;
		}

		key = "progressionPacksDiscovered";
		if (profile.ContainsKey(key))
		{
			m_progressionPacksDiscovered = PersistenceUtils.SafeParse<bool>(profile[key]);
		} else
		{
			m_progressionPacksDiscovered = false;
		}

		key = "maxTransactionPrice";
		if (profile.ContainsKey(key))
		{
			m_maxTransactionPrice = PersistenceUtils.SafeParse<int>(profile[key]);
		} else
		{
			m_maxTransactionPrice = 0;
		}
       
		// Clustering
		key = "clusterId";
		if (profile.ContainsKey(key))
		{
			m_clusterId = profile[key];
		} else
		{
			m_clusterId = "";
		}

		key = "clusterSynced";
		if(profile.ContainsKey(key)) {
			m_clusterSynced = profile[key].AsBool;
		} else {
			m_clusterSynced = true;
		}

		// Some cheats override profile settings - will be saved with the next Save()
		if (Prefs.GetBoolPlayer("skipTutorialCheat")) {
			m_tutorialStep = TutorialStep.ALL;
			UsersManager.currentUser.gamesPlayed = 5;	// Fake the amount of played games to skip some tutorial steps depending on it
			Prefs.SetBoolPlayer("skipTutorialCheat", false);
		}

		// Dragons
		if ( _data.ContainsKey("dragons") )
		{
			SimpleJSON.JSONArray dragons = _data["dragons"] as SimpleJSON.JSONArray;
			for( int i = 0; i<dragons.Count; i++ )
			{
				string sku = dragons[i]["sku"];
				m_dragonsBySku[sku].Load(dragons[i]);
			}
		}
		else
		{
			// Clean Dragon Data
			foreach( KeyValuePair<string, IDragonData> pair in m_dragonsBySku)
				pair.Value.ResetLoadedData();
		}

		// Disguises
		m_wardrobe.InitFromDefinitions();
		if ( _data.ContainsKey("disguises") ) {
			m_wardrobe.Load( _data["disguises"] );
		}

		// Pets
		m_petCollection.Reset();
		if(_data.ContainsKey("pets")) {
			m_petCollection.Load(_data["pets"]);
		}

        // Missions
        // Clean missions
        m_userMissions.ClearAllMissions();
		if(_data.ContainsKey("missions")) {
			m_userMissions.Load(_data["missions"]);
		}

        m_userSpecialMissions.ClearAllMissions();
        if (_data.ContainsKey("missionsSpecial")) {
            m_userSpecialMissions.Load(_data["missionsSpecial"]);
        }

        // Achievements
		m_achievements.Initialize();
		if ( _data.ContainsKey("achievements") ){
			m_achievements.Load( _data["achievements"] );
		}

		// Eggs
		if(_data.ContainsKey("eggs")) {
			LoadEggData(_data["eggs"] as SimpleJSON.JSONClass);
		} else {
			// Clean Eggs Data
			for(int i = 0; i<EggManager.INVENTORY_SIZE; i++) {
				eggsInventory[i] = null;
			}
			m_incubatingEgg = null;
		}

		// Chests
		if(_data.ContainsKey("chests")) {
			LoadChestsData(_data["chests"] as SimpleJSON.JSONClass);
		} else {
			// Defaults
			for(int i = 0; i < ChestManager.NUM_DAILY_CHESTS; i++) {
				dailyChests[i] = new Chest();
			}

			//m_dailyChestsResetTimestamp = GameServerManager.GetEstimatedServerTime();	// That will reset to 24hrs from now
			// Reset timestamp to 00:00 of local time (but using server timezone!)
			TimeSpan toMidnight = DateTime.Today.AddDays(1) - DateTime.Now;	// Local
			m_dailyChestsResetTimestamp = GameServerManager.GetEstimatedServerTime() + toMidnight;	// Local 00:00 in server timezone
		}

		// Daily mission Ads
		m_dailyRemoveMissionAdUses = 0;
		if ( _data.ContainsKey("dailyRemoveMissionAdTimestamp") )
		{
			m_dailyRemoveMissionAdTimestamp = PersistenceUtils.SafeParse<DateTime>(_data["dailyRemoveMissionAdTimestamp"]);

			if ( _data.ContainsKey("dailyRemoveMissionAdUses") )
				m_dailyRemoveMissionAdUses = PersistenceUtils.SafeParse<int>(_data["dailyRemoveMissionAdUses"]);
		}
		else
		{
			m_dailyRemoveMissionAdTimestamp = GameServerManager.GetEstimatedServerTime();	// Already expired
		}

		m_skipMissionAdUses = 0;
		if(_data.ContainsKey("skipMissionAdTimestamp")) {
			m_skipMissionAdTimestamp = PersistenceUtils.SafeParse<DateTime>(_data["skipMissionAdTimestamp"]);
			if(_data.ContainsKey("skipMissionAdUses")) {
				m_skipMissionAdUses = PersistenceUtils.SafeParse<int>(_data["skipMissionAdUses"]);
			}
		} else {
			m_skipMissionAdTimestamp = GameServerManager.GetEstimatedServerTime();	// Already expired
		}

		// Map upgrades
		key = "mapResetTimestamp";
		if(_data.ContainsKey(key)) {
			m_mapResetTimestamp = PersistenceUtils.SafeParse<DateTime>(_data["mapResetTimestamp"]);
		} else {
			m_mapResetTimestamp = GameServerManager.GetEstimatedServerTime();	// Already expired
		}

		key = "mapDiscovered";
		if (_data.ContainsKey(key))
		{
			mapDiscovered = PersistenceUtils.SafeParse<bool>(_data["mapDiscovered"]);
		}
		else
		{
			mapDiscovered = false;  
		}


		// Global events
		key = "globalEvents";
		m_globalEvents.Clear();	// Clear current events data
		if(_data.ContainsKey(key)) {
			// Parse json array
			SimpleJSON.JSONArray eventsData = _data[key].AsArray;
			for(int i = 0; i < eventsData.Count; ++i) {
				// Create a new event with the given data and store it to the events dictionary
				GlobalEventUserData newEvent = new GlobalEventUserData();
				newEvent.Load(eventsData[i]);
				newEvent.userID = userId;
				if ( newEvent.eventID > 0 ){
					m_globalEvents[newEvent.eventID] = newEvent;
				}
				#if UNITY_EDITOR
				else{
					DebugUtils.Assert(false, "Event with negative id");
				}
				#endif
			}
		}

		// Pending rewards
		key = "rewards";
		m_rewards.Clear();
		if(_data.ContainsKey(key)) {
			// Parse json array
			// Reverse iterate to respect the stack order
			SimpleJSON.JSONArray rewardsData = _data[key].AsArray;
			for(int i = rewardsData.Count - 1; i >= 0 ; --i) {
				// Create new reward with the given data
				Metagame.Reward r = Metagame.Reward.CreateFromJson(rewardsData[i]);

                // Makes sure that r can be pushed before pushing it in order to avoid a unique reward (such as remove_ads) from being given more than once
                if (CanPushReward(r))
				    m_rewards.Push(r);
			}
		}

		// Daily rewards
		key = "dailyRewards";
		m_dailyRewards.Reset();
		Debug.Log(Colors.cyan.Tag("LOADING DAILY REWARDS"));
		if(_data.ContainsKey(key)) {
			Debug.Log(Colors.lime.Tag("VALID DATA!!\n") + new JsonFormatter().PrettyPrint(_data[key].ToString()));
			m_dailyRewards.LoadData(_data[key]);
		} else {
			Debug.Log(Colors.red.Tag("INVALID DATA! Generating new sequence"));
			m_dailyRewards.Generate();	// Generate a new sequence
		}

		key = "xPromo";
		Debug.Log(Colors.cyan.Tag("LOADING XPROMO"));
		if (_data.ContainsKey(key))
		{
			Debug.Log(Colors.lime.Tag("VALID DATA!!\n") + new JsonFormatter().PrettyPrint(_data[key].ToString()));
			XPromoManager.instance.xPromoCycle.LoadData(_data[key]);
		}
		else
		{
			Debug.Log(Colors.red.Tag("INVALID DATA!"));
		}

		// Offer Packs
		// Old version. transform to offer packs v2
		if ( _data.ContainsKey( "offerPacks" ) || _data.ContainsKey("offerPacksRotationalHistory") )
        {
            UpdateOfferPacksPersistance( _data );
        }

        key = "newOffersPacks";
        if ( _data.ContainsKey(key) ) {
            int max = (int)OfferPack.Type.COUNT;
            for (int i = 0; i < max; i++) {
                OfferPack.Type offerType = (OfferPack.Type)i;
                string typeStr = OfferPack.TypeToString( offerType );
                JSONArray array = _data[key][typeStr].AsArray;
                for (int j = 0; j < array.Count; j++) {
                    m_newOfferPersistanceData[offerType].Add( array[j].AsObject );
                }
            }
        }

		// Offers history
		key = "offerPacksHistory";
		if(_data.ContainsKey(key)) {
			// Get data
			JSONNode historyData = _data[key];

			// Data is stored by offer type
			int typeCount = (int)OfferPack.Type.COUNT;
			for(int i = 0; i < typeCount; ++i) {
				// Get key for this type and check if we have data stored
				OfferPack.Type offerType = (OfferPack.Type)i;
				key = OfferPack.TypeToString(offerType);
				if(historyData.ContainsKey(key)) {
					// History for this type is stored as an array representing a queue
					JSONArray queueData = historyData[key].AsArray;
					for(int j = 0; j < queueData.Count; ++j) {
						m_offersHistory[offerType].Enqueue(queueData[j]);
					}
				}
			}
		}

		// Free offer
		key = "freeOfferCooldownEndTime";
		if(_data.ContainsKey(key)) {
			freeOfferCooldownEndTime = new DateTime(PersistenceUtils.SafeParse<long>(_data[key]));
		} else {
			freeOfferCooldownEndTime = DateTime.MinValue;
		}

		// Happy hour offer
		// [AOC] As of 2.6 format has changed. Add support for retrocompatibility. Can be removed at 2.8.
		key = "happyHourOffer";
		if(_data.ContainsKey(key)) {
			// Pre 2.6 - Delete by 2.8
			// Create a fake json object using the new keys
			JSONNode oldData = _data[key];
			JSONClass newData = new JSONClass();

			key = "happyHourExpirationTime";
			if(oldData.ContainsKey(key)) {
				newData["expirationTime"] = oldData[key];
			}

			key = "happyHourExtraGemsRate";
			if(oldData.ContainsKey(key)) {
				newData["extraGemsFactor"] = oldData[key];
			}

			key = "happyHourLastPackSku";
			if(oldData.ContainsKey(key)) {
				newData["lastPackSku"] = oldData[key];
			}

			// Inject sku
			newData["activeSku"] = "happy_hour_local";	// Legacy system only had the local one

			// Load it!
			m_happyHourData.FromJson(newData as JSONClass);
		} else {
			// Post 2.6 - Keep it
			key = "happyHour";
			if(_data.ContainsKey(key)) {
				m_happyHourData.FromJson(_data[key] as JSONClass);
			}
		}

		

		// Visited Zones
		key = "visitedZones";
        m_visitedZones.Clear();
        if(_data.ContainsKey(key)) {
            // Parse json object into the list
            JSONArray zonesArray = _data[key] as JSONArray;
            int max = zonesArray.Count;
            for (int i = 0; i < max; i++)
            {
                m_visitedZones.Add(zonesArray[i]);
            }
        }

        key = "givenTransactions";
        if (_data.ContainsKey(key))
        {
            GivenTransactions = _data[key];
        }

		// Referral
		key = "referrerUserId";
		if (_data.ContainsKey(key))
		{
			m_referrerUserId = _data[key].Value;
		}
		else
		{
			m_referrerUserId = "";
		}

		key = "referralState";	// Important: before referralConfirmed!
		if(_data.ContainsKey(key)) {
			m_referralState = (ReferralManager.State)_data[key].AsInt;
		} else {
			m_referralState = ReferralManager.State.UNKNOWN;
		}

		key = "referralConfirmed";	// Deprecated
        if (_data.ContainsKey(key))
        {
			// [AOC] Persistence format changed for 3.2
			//		 Adding retrocompatibility support
			bool referralConfirmed = _data[key].AsBool;
			if(referralConfirmed) {
				m_referralState = ReferralManager.State.REFERRAL_CONFIRMED;
			} else {
				m_referralState = ReferralManager.State.UNKNOWN;	// [AOC] If we have a valid referrerUserId, the ReferralManager will switch to PENDING_CONFIRMATION state
			}
        }

		key = "invitesSent";
		if (_data.ContainsKey(key))
		{
			m_invitesSent = PersistenceUtils.SafeParse<int> (_data[key]);
		}
		else
		{
			m_invitesSent = 0;
		}

		// Baby Dragons - Extra gem granted
		key = "babyDragonExtraGemGranted";
		if (profile.ContainsKey(key))
		{
			m_babyDragonExtraGemGranted = PersistenceUtils.SafeParse<bool>(profile[key]);
		}
		else
		{
			m_babyDragonExtraGemGranted = false;
		}

		// Baby Dragons - Extra gem failed counter
		key = "babyDragonExtraGemFailedCounter";
		if (profile.ContainsKey(key))
		{
			m_babyDragonExtraGemFailedCounter = PersistenceUtils.SafeParse<int>(profile[key]);
		}
		else
		{
			m_babyDragonExtraGemFailedCounter = 0;
		}
	}


	/// <summary>
	/// Loads the data related to eggs.
	/// </summary>
	/// <param name="_data">The persistence data.</param>
	private void LoadEggData( SimpleJSON.JSONClass _data )
	{
		// Inventory
		SimpleJSON.JSONArray inventoryArray = _data["inventory"].AsArray;
		for(int i = 0; i < EggManager.INVENTORY_SIZE; i++) 
		{
			// In case INVENTORY_SIZE changes (if persisted is bigger, just ignore remaining data, if lower fill new slots with null)
			if(i < inventoryArray.Count) 
			{
				// Either create new egg, delete egg or update existing egg
				if(m_eggsInventory[i] == null && inventoryArray[i] != null) {			// Create new egg?
					m_eggsInventory[i] = Egg.CreateFromSaveData(inventoryArray[i]);
				} else if(m_eggsInventory[i] != null && inventoryArray[i] == null) {	// Delete egg?
					m_eggsInventory[i] = null;
				} else if(m_eggsInventory[i] != null && inventoryArray[i] != null) {	// Update egg?
					m_eggsInventory[i].Load(inventoryArray[i]);
				}
			} else {
				m_eggsInventory[i] = null;
			}
		}

		// Incubator - same 3 cases
		bool dataIncubatingEgg = _data.ContainsKey("incubatingEgg");
		if(m_incubatingEgg == null && dataIncubatingEgg) {			// Create new egg?
			m_incubatingEgg = Egg.CreateFromSaveData(_data["incubatingEgg"]);
		} else if(m_incubatingEgg != null && !dataIncubatingEgg) {	// Delete egg?
			m_incubatingEgg = null;
		} else if(m_incubatingEgg != null && dataIncubatingEgg) {	// Update egg?
			m_incubatingEgg.Load(_data["incubatingEgg"]);
		}

		if ( _data.ContainsKey("incubationTimeReference") ){
			m_incubationTimeReference = PersistenceUtils.SafeParse<long>(_data["incubationTimeReference"]);
		}else{
			m_incubationTimeReference = GameServerManager.GetEstimatedServerTimeAsLong();
		}

		// Incubator timer
		m_incubationEndTimestamp = PersistenceUtils.SafeParse<DateTime>(_data["incubationEndTimestamp"]);

        // Eggs collected
        eggsCollected = PersistenceUtils.SafeParse<int>(_data["collectedAmount"]);

		// Dynamic Probability data
		m_openEggTriesWithoutRares = PersistenceUtils.SafeParse<int>(_data["openEggTriesWithoutRares"]);
    }

	/// <summary>
	/// Load the data related to the chests.
	/// </summary>
	/// <param name="_data">Persistence data.</param>
	private void LoadChestsData(SimpleJSON.JSONClass _data) {
		// Amount of chests is constant
		SimpleJSON.JSONArray chestsArray = _data["chests"].AsArray;
		for(int i = 0; i < ChestManager.NUM_DAILY_CHESTS; i++) {
			// If chest was not created, do it now
			if(dailyChests[i] == null) {
				dailyChests[i] = new Chest();
			}

			// If we have data for this chest, load it
			if(chestsArray != null && i < chestsArray.Count) {
				dailyChests[i].Load(chestsArray[i]);
			}

			// A chest should never initially be in the INIT state, nor in the REWARD_PENDING. Validate that.
			if(dailyChests[i].state == Chest.State.INIT
			//|| dailyChests[i].state == Chest.State.PENDING_REWARD) {	// [AOC] Right now persistence is being reloaded during the game and before the results screen, so this safecheck results in resetting the chests collected during that run
			) {
				dailyChests[i].ChangeState(Chest.State.NOT_COLLECTED);
			}
		}

		// Reset timestamp
		m_dailyChestsResetTimestamp = PersistenceUtils.SafeParse<DateTime>(_data["resetTimestamp"]);
	}
	#endregion

	//------------------------------------------------------------------------//
	// PERSISTENCE SAVE METHODS												  //
	//------------------------------------------------------------------------//
	#region save
	/// <summary>
	/// Create a json with the current data in the profile.
	/// Similar to Save(), but doesn't update timestamp nor save count.
	/// </summary>
	/// <returns>A json representing this profile.</returns>
	public SimpleJSON.JSONClass ToJson() {
		// Create new object
		SimpleJSON.JSONClass data = new SimpleJSON.JSONClass();
		SimpleJSON.JSONClass profile = new SimpleJSON.JSONClass();

        profile.Add("timestamp", PersistenceUtils.SafeToString(m_saveTimestamp));

        // Economy
		profile.Add( "sc", m_currencies[(int)Currency.SOFT].Serialize());
		profile.Add( "pc", m_currencies[(int)Currency.HARD].Serialize());
		profile.Add( "gf", m_currencies[(int)Currency.GOLDEN_FRAGMENTS].Serialize());
		profile.Add( "keys", m_currencies[(int)Currency.KEYS].Serialize());
		profile.Add( "gemsSpent", PersistenceUtils.SafeToString(m_gemsSpent));

		// Game settings
		profile.Add("currentDragon",m_currentDragon);
		profile.Add("currentLevel",m_currentLevel);
		profile.Add("tutorialStep", PersistenceUtils.SafeToString((int)m_tutorialStep));
		profile.Add("furyUsed", PersistenceUtils.SafeToString(m_furyUsed));

		// Game stats
		profile.Add("gamesPlayed", PersistenceUtils.SafeToString(m_gamesPlayed));
		profile.Add("highScore", PersistenceUtils.SafeToString(m_highScore));
		profile.Add("superFuryProgression", PersistenceUtils.SafeToString(m_superFuryProgression));
        profile.Add("socialState",SocialStatesAsString[(int)SocialState]);
		profile.Add("firerushesCount", PersistenceUtils.SafeToString(m_firerushesCount));
		profile.Add("boostTime", PersistenceUtils.SafeToString(m_boostTime));
		profile.Add("totalScore", PersistenceUtils.SafeToString(m_totalScore));

		// Shopping data
		profile.Add("enteredShop", PersistenceUtils.SafeToString(m_enteredShop));
		profile.Add("progressionPacksDiscovered", PersistenceUtils.SafeToString(m_progressionPacksDiscovered));
		profile.Add("maxTransactionPrice", PersistenceUtils.SafeToString(m_maxTransactionPrice));

		// Clustering
		profile.Add("clusterId", m_clusterId);
		profile.Add("clusterSynced", m_clusterSynced);

		data.Add("userProfile", profile);

		// Dragons
		SimpleJSON.JSONArray dragons = new SimpleJSON.JSONArray();
		foreach( KeyValuePair<string,IDragonData> pair in m_dragonsBySku)
		{
			IDragonData dragonData = pair.Value;
			dragons.Add( dragonData.Save() );
		}
		data.Add( "dragons", dragons );

		data.Add("disguises", m_wardrobe.Save());
		data.Add("pets", m_petCollection.Save());
		data.Add("missions", m_userMissions.Save());
        data.Add("missionsSpecial", m_userSpecialMissions.Save());

		data.Add("achievements", m_achievements.Save());

		data.Add("eggs", SaveEggData());
		data.Add("chests", SaveChestsData());

		// Daily remove missions with ads
		data.Add("dailyRemoveMissionAdTimestamp", PersistenceUtils.SafeToString(m_dailyRemoveMissionAdTimestamp));
		data.Add("dailyRemoveMissionAdUses", PersistenceUtils.SafeToString(m_dailyRemoveMissionAdUses));
		data.Add("skipMissionAdTimestamp", PersistenceUtils.SafeToString(m_skipMissionAdTimestamp));
		data.Add("skipMissionAdUses", PersistenceUtils.SafeToString(m_skipMissionAdUses));

		// Map upgrades
		data.Add("mapResetTimestamp", PersistenceUtils.SafeToString(m_mapResetTimestamp));
		data.Add("mapDiscovered", PersistenceUtils.SafeToString(m_mapDiscovered));

		// Global Events
		SimpleJSON.JSONArray eventsData = new SimpleJSON.JSONArray();
		foreach(KeyValuePair<int, GlobalEventUserData> kvp in m_globalEvents) {
			if ( kvp.Value.eventID > 0 ){
				eventsData.Add(kvp.Value.Save(true));
			}
			#if UNITY_EDITOR
			else{
				DebugUtils.Assert(false, "Event with negative id");
			}
			#endif
		}
		data.Add("globalEvents", eventsData);

		// Pending rewards
		SimpleJSON.JSONArray rewardsData = new SimpleJSON.JSONArray();
		if(m_rewards.Count > 0) {
			// The foreach loop will grab the elements at the top of the stack first
			foreach(Metagame.Reward r in m_rewards) {
				rewardsData.Add(r.ToJson());
			}
		}
		data.Add("rewards", rewardsData);

		// Daily rewards
		JSONClass dailyRewardsData = m_dailyRewards.SaveData();
		if(dailyRewardsData != null) {  // Can be null if the sequence was never generated
			//Debug.Log(Colors.lime.Tag("VALID DATA!\n") + new JsonFormatter().PrettyPrint(dailyRewardsData.ToString()));
			data.Add("dailyRewards", dailyRewardsData);
		} else {
			Debug.Log(Colors.red.Tag("DAILY REWARDS: INVALID DATA!"));
		}

		// XPromo
		JSONClass xPromoData = XPromoManager.instance.xPromoCycle.SaveData();
		if (xPromoData != null)
		{  // Can be null if the sequence was never generated
			data.Add("xPromo", xPromoData);
		}
		else
		{
			Debug.Log(Colors.red.Tag("XPROMO: INVALID DATA!"));
		}

		// Offer packs
		JSONClass newOffersData = new SimpleJSON.JSONClass();
        int count = (int)OfferPack.Type.COUNT;
        for (int i = 0; i < count; i++) {
            JSONArray array = new JSONArray();
            OfferPack.Type t = (OfferPack.Type)i;
            for (int j = 0; j < m_newOfferPersistanceData[t].Count; j++) {
                array.Add( m_newOfferPersistanceData[t][j]);
            }
            newOffersData.Add(OfferPack.TypeToString(t), array);
        }
        data.Add("newOffersPacks", newOffersData);

		// Offer history
		JSONClass offerHistoryData = new SimpleJSON.JSONClass();
		foreach(KeyValuePair<OfferPack.Type, Queue<string>> kvp in m_offersHistory) {
			// No need to store this offer type if history is empty
			if(kvp.Value.Count == 0) continue;

			// Store the history for this offer type into a JSON Array
			JSONArray arrayData = new JSONArray();
			foreach(string offerSku in kvp.Value) {
				arrayData.Add(offerSku);
			}

			// Save this kvp into the parent JSON object
			offerHistoryData.Add(OfferPack.TypeToString(kvp.Key), arrayData);
		}
		data.Add("offerPacksHistory", offerHistoryData);

		// Free offer
		data.Add("freeOfferCooldownEndTime", PersistenceUtils.SafeToString(freeOfferCooldownEndTime.Ticks));

        // Happy hour
        data.Add("happyHour", m_happyHourData.ToJson());

        // Remove Ads offer
        // For safety reasons we store the Remove Ads feature in the player preferences
        // but we store it also in the user profile just in case we need in the future
        data.Add("removeAdsFeature", m_removeAds.Save());


		// Visited Zones
		JSONArray zonesArray = new SimpleJSON.JSONArray();
        int max = m_visitedZones.Count;
        foreach( string str in m_visitedZones)
        {
            zonesArray.Add( str );
        }
        data.Add("visitedZones", zonesArray);

        if (!string.IsNullOrEmpty(GivenTransactions))
        {
            data.Add("givenTransactions", GivenTransactions);
        }

		// Referral
		data.Add("referrerUserId", referrerUserId);
		//data.Add("referralConfirmed", referralConfirmed);		// [AOC] Not anymore as of 3.2
		data.Add("referralState", (int)referralState);
		data.Add("invitesSent", invitesSent);

		// Baby dragons - Extra gem granted
		data.Add("babyDragonExtraGemGranted", PersistenceUtils.SafeToString(m_babyDragonExtraGemGranted));

		// Baby dragons - Extra gem failed counter
		data.Add("babyDragonExtraGemFailedCounter", PersistenceUtils.SafeToString(m_babyDragonExtraGemFailedCounter));

        // Return it
		return data;
	}

	/// <summary>
	/// Create the save data for the eggs.
	/// </summary>
	/// <returns>The save data for the eggs.</returns>
	private SimpleJSON.JSONClass SaveEggData()
	{
		// Create new object, initialize and return it
		SimpleJSON.JSONClass data = new SimpleJSON.JSONClass();

		// Inventory
		SimpleJSON.JSONArray inventoryArray = new SimpleJSON.JSONArray();
		for(int i = 0; i < EggManager.INVENTORY_SIZE; i++) 
		{
			if(m_eggsInventory[i] != null) 
			{
				inventoryArray.Add(m_eggsInventory[i].Save());
			}
		}
		data.Add("inventory", inventoryArray);

		// Incubator
		if(m_incubatingEgg != null) 
		{
			data.Add("incubatingEgg", m_incubatingEgg.Save());
		}

		// Incubation Time Reference
		data.Add( "incubationTimeReference", PersistenceUtils.SafeToString(m_incubationTimeReference));

		// Incubator timer
		data.Add("incubationEndTimestamp", PersistenceUtils.SafeToString(m_incubationEndTimestamp));

        // Eggs collected
		data.Add("collectedAmount", PersistenceUtils.SafeToString(eggsCollected));

		// Dynamic Probability data
		data.Add("openEggTriesWithoutRares", PersistenceUtils.SafeToString(m_openEggTriesWithoutRares));

        return data;
	}

	/// <summary>
	/// Creates the save data object for the chests.
	/// </summary>
	/// <returns>The chests save data object.</returns>
	private SimpleJSON.JSONClass SaveChestsData() {
		// Create new array
		SimpleJSON.JSONClass data = new SimpleJSON.JSONClass();

		// Chests array
		SimpleJSON.JSONArray chestsArray = new SimpleJSON.JSONArray();
		for(int i = 0; i < dailyChests.Length; i++) {
			if(dailyChests[i] != null) {
				chestsArray.Add(dailyChests[i].Save());
			}
		}
		data.Add("chests", chestsArray);

		// Reset timestamp
		data.Add("resetTimestamp", PersistenceUtils.SafeToString(m_dailyChestsResetTimestamp));

		// Done!
		return data;
	}
	#endregion

	//------------------------------------------------------------------------//
	// SKINS MANAGEMENT														  //
	//------------------------------------------------------------------------//
	#region skins
	/// <summary>
	/// Get the sku of the disguise equipped to a specific dragon.
	/// </summary>
	/// <returns>The sku of the equipped disguise.</returns>
	/// <param name="_dragonSku">The dragon whose disguie we want to check.</param>
	public string GetEquipedDisguise( string _dragonSku )
	{
		if ( m_dragonsBySku.ContainsKey( _dragonSku ) )
			return m_dragonsBySku[ _dragonSku ].disguise;
		return "";
	}

	/// <summary>
	/// Try to equip the given disguise into the target dragon.
	/// Doesn't check that the disguise actually belongs to the dragon.
	/// </summary>
	/// <returns><c>true</c> if the disguise was different from the one previously equiped by the dragon, <c>false</c> otherwise.</returns>
	/// <param name="_dragonSku">Dragon sku.</param>
	/// <param name="_disguiseSku">Disguise sku.</param>
	/// <param name="_persistent">Whether the equipped disguised is to be persisted or is just for preview.</param>
	public bool EquipDisguise(string _dragonSku, string _disguiseSku, bool _persistent)
	{
		bool ret = false;
		if ( m_dragonsBySku.ContainsKey( _dragonSku ) )
		{
			if ( m_dragonsBySku[_dragonSku].disguise != _disguiseSku )
			{
				ret = true;
				m_dragonsBySku[_dragonSku].disguise = _disguiseSku;
			}

			// Persist?
			if(_persistent) {
				m_dragonsBySku[_dragonSku].persistentDisguise = _disguiseSku;
				PersistenceFacade.instance.Save_Request();
			}
		}
		return ret;
	}
	#endregion

	//------------------------------------------------------------------------//
	// PETS MANAGEMENT														  //
	//------------------------------------------------------------------------//
	#region pets
	/// <summary>
	/// Get the current pet loadout for the target dragon.
	/// </summary>
	/// <returns>The list of all the pet slots of the target dragon with the sku of the pet equipped in each of them. Empty string if the slot is empty.</returns>
	/// <param name="_dragonSku">The dragon whose pet loadout we want to know.</param>
	public List<string> GetEquipedPets( string _dragonSku )
	{
		if ( m_dragonsBySku.ContainsKey( _dragonSku ) )
			return m_dragonsBySku[ _dragonSku ].pets;
		return new List<string>();
	}

	/// <summary>
	/// Given a dragon and a pet, check whether the pet is equipped in that dragon
	/// and figure out which slot it's in.
	/// </summary>
	/// <returns>The slot the pet is in.<c>-1</c> if the pet is not equipped or either the dragon or the pet skus were not valid.</returns>
	/// <param name="_dragonSku">The dragon whose loadout we want to check.</param>
	/// <param name="_petSku">The pet we're looking for.</param>
	public int GetPetSlot(string _dragonSku, string _petSku) {
		// Check dragon sku
		if(!m_dragonsBySku.ContainsKey(_dragonSku)) return -1;
		IDragonData dragon = m_dragonsBySku[_dragonSku];

		// Just find target pet's slot in the target dragon's loadout
		return dragon.pets.IndexOf(_petSku);
	}

	/// <summary>
	/// Try to equip the given pet to the first available slot in the target dragon.
	/// Checks that the pet is actually unlocked, and not already equipped in another slot.
	/// Also makes sure that there is slots available.
	/// </summary>
	/// <returns>
	/// The index of the slot where the pet was equipped.
	/// Negative value if pet couldn't be equipped, with the following error codes:
	/// -1: Unknown dragon sku
	/// -2: Pet already equipped
	/// -3: Pet is locked or sku not valid
	/// -4: No free slots available
	/// </returns>
	/// <param name="_dragonSku">The dragon where we want to attach the pet.</param>
	/// <param name="_petSku">The pet we want to equip.</param>
	public int EquipPet(string _dragonSku, string _petSku) {
		// Check dragon sku
		if(!m_dragonsBySku.ContainsKey(_dragonSku)) return -1;
		IDragonData dragon = m_dragonsBySku[_dragonSku];

		// Is pet already equipped?
		if(dragon.pets.Contains(_petSku)) return -2;

		// Is pet unlocked?
		if(!m_petCollection.IsPetUnlocked(_petSku)) return -3;

		// Find the first available slot
		for(int i = 0; i < dragon.pets.Count; i++) {
			if(string.IsNullOrEmpty(dragon.pets[i])) {
				// Success! Equip pet
				dragon.pets[i] = _petSku;

				// Notify game
				Messenger.Broadcast<string, int, string>(MessengerEvents.MENU_DRAGON_PET_CHANGE, _dragonSku, i, _petSku);

				return i;
			}
		}

		// No empty slots found
		return -4;
	}

	/// <summary>
	/// Try to equip the given pet to the first available slot in the target dragon.
	/// Checks that the pet is actually unlocked, and not already equipped in another slot.
	/// Also makes sure that there is slots available.
	/// </summary>
	/// <returns>
	/// The index of the slot where the pet was equipped.
	/// Negative value if pet couldn't be equipped, with the following error codes:
	/// -1: Unknown dragon sku
	/// -2: Pet already equipped
	/// -3: Pet is locked or sku not valid
	/// -4: Given slot index not valid
	/// -5: Requested slot is not available
	/// </returns>
	/// <param name="_dragonSku">The dragon where we want to attach the pet.</param>
	/// <param name="_petSku">The pet we want to equip.</param>
	/// <param name="_slotIdx">Slot where we want to equip the pet.</param>
	public int EquipPet(string _dragonSku, string _petSku, int _slotIdx) {
		// Check dragon sku
		if(!m_dragonsBySku.ContainsKey(_dragonSku)) return -1;
		IDragonData dragon = m_dragonsBySku[_dragonSku];

		// Is pet already equipped?
		if(dragon.pets.Contains(_petSku)) return -2;

		// Is pet unlocked?
		if(!m_petCollection.IsPetUnlocked(_petSku)) return -3;

		// Is slot index valid?
		if(_slotIdx < 0 || _slotIdx >= dragon.pets.Count) return -4;

		// Is the requested slot available?
		if(!string.IsNullOrEmpty(dragon.pets[_slotIdx])) return -5;

		// All checks passed! Equip pet
		dragon.pets[_slotIdx] = _petSku;

		// Notify game
		Messenger.Broadcast<string, int, string>(MessengerEvents.MENU_DRAGON_PET_CHANGE, _dragonSku, _slotIdx, _petSku);

		return _slotIdx;
	}

	/// <summary>
	/// Try to unequip the given pet from the target dragon.
	/// </summary>
	/// <returns>
	/// The index of the slot where the pet was equipped.
	/// Negative value if pet couldn't be unequipped, with the following error codes:
	/// -1: Unknown dragon sku
	/// -2: Pet not equipped or sku not valid
	/// </returns>
	/// <param name="_dragonSku">The dragon from where we want to unequip the pet.</param>
	/// <param name="_petSku">The pet we want to unequip.</param>
	public int UnequipPet(string _dragonSku, string _petSku) {
		// Check dragon sku
		if(!m_dragonsBySku.ContainsKey(_dragonSku)) return -1;
		IDragonData dragon = m_dragonsBySku[_dragonSku];

		// Check whether pet is equipped
		int idx = dragon.pets.IndexOf(_petSku);
		if(idx < 0) return -2;

		// Empty slot
		return UnequipPet(_dragonSku, idx);
	}

	/// <summary>
	/// Same as the previous method, but using slot index instead of pet sku.
	/// </summary>
	/// <returns>
	/// The index of the slot where the pet was equipped.
	/// Negative value if pet couldn't be unequipped, with the following error codes:
	/// -1: Unknown dragon sku
	/// -2: Invalid slot index
	/// -3: Slot is already empty
	/// </returns>
	/// <param name="_dragonSku">The dragon from where we want to unequip the pet.</param>
	/// <param name="_slotIdx">Slot to be unequipped.</param>
	public int UnequipPet(string _dragonSku, int _slotIdx) {
		// Check dragon sku
		if(!m_dragonsBySku.ContainsKey(_dragonSku)) return -1;
		IDragonData dragon = m_dragonsBySku[_dragonSku];

		// Check slot index
		if(_slotIdx < 0 || _slotIdx >= dragon.pets.Count) return -2;

		// Make sure index is equipped
		if(string.IsNullOrEmpty(dragon.pets[_slotIdx])) return -3;

		// Everything ok, unequip the pet!
		dragon.pets[_slotIdx] = string.Empty;

		// Notify game
		Messenger.Broadcast<string, int, string>(MessengerEvents.MENU_DRAGON_PET_CHANGE, _dragonSku, _slotIdx, string.Empty);

		return _slotIdx;
	}
	#endregion

	//------------------------------------------------------------------------//
	// GLOBAL EVENTS MANAGEMENT												  //
	//------------------------------------------------------------------------//
	#region global_events
	/// <summary>
	/// Get the data of this user for a given event.
	/// A new one will be created if the user has no data stored for this event.
	/// </summary>
	/// <returns>The event data for the requested event.</returns>
	/// <param name="_eventId">Event identifier.</param>
	public GlobalEventUserData GetGlobalEventData(int _eventID) {
		// If the user doesn't have data of this event, create a new one
		GlobalEventUserData data = null;
		if(!m_globalEvents.TryGetValue(_eventID, out data)) {
			data = new GlobalEventUserData(
				_eventID,
				userId,
				0,
				-1,
				0
			);
			m_globalEvents[_eventID] = data;
		}

		// [AOC] Sick of insane bugs, make sure the user ID is valid!
		data.userID = this.userId;
		return data;
	}
    
    public void OnRulesUpdated(){
        // Because We cache dragon's price on a variable we need to refresh the value
        foreach(KeyValuePair<string, IDragonData> pair in m_dragonsBySku) {
            pair.Value.RefreshPrice();
            pair.Value.RefreshShadowRevealUnlock();
			if ( pair.Value is DragonDataClassic )
			{
				(pair.Value as DragonDataClassic).RefreshProgression();
			}
        }

		// Propagate to daily rewards sequence
		if(m_dailyRewards != null) {
			m_dailyRewards.OnRulesUpdated();
		}
    }
	#endregion

	//------------------------------------------------------------------------//
	// DRAGONS MANAGEMENT													  //
	//------------------------------------------------------------------------//
	#region dragons
	/// <summary>
	/// Gets the number OF owned dragons.
	/// </summary>
	/// <returns>The number owned dragons.</returns>
	public int GetNumOwnedDragons() {
		int ret = 0;
		foreach(KeyValuePair<string, IDragonData> pair in m_dragonsBySku) {
			if(pair.Value.isOwned)
				ret++;
		}
		return ret;
	}
    
    /// <summary>
    /// Gets the number owned special dragons.
    /// </summary>
    /// <returns>The number owned special dragons.</returns>
    public int GetNumOwnedSpecialDragons()
    {
        int ret = 0;
        foreach(KeyValuePair<string, IDragonData> pair in m_dragonsBySku) {
            if (pair.Value.isOwned && (pair.Value is DragonDataSpecial))
            {
                ret++;
            }
        }
        return ret;
    }

	/// <summary>
	/// Only CLASSIC dragons considered.
	/// </summary>
	public DragonDataClassic GetHighestDragon() {
		DragonDataClassic returnValue = null;

        // Find the dragon with the highest level among all dragons acquired by the user.
        if (m_dragonsBySku != null) {            
            foreach (KeyValuePair<string, IDragonData> pair in m_dragonsBySku) {
				// Skip if not classic
				if(pair.Value.type != IDragonData.Type.CLASSIC) continue;

				// Is it owned?
                if (pair.Value.isOwned) {
                    int order = pair.Value.GetOrder();
                    if (returnValue == null || order > returnValue.GetOrder()) {
						returnValue = pair.Value as DragonDataClassic;
                    }
                }
            }            
        }

        return returnValue;
    }

    /// <summary>
    /// Returns an int that sums up the user's progress.
    /// </summary>
    /// <returns></returns>
    public int GetPlayerProgress() {        
        // Find the dragon with the highest level among all dragons acquired by the user.
        IDragonData highestDragon = GetHighestDragon();        
        return (highestDragon == null) ? 0 : GetDragonProgress(highestDragon);        
    }

    /// <summary>
    /// Returns the progress of the dragon sku passed as a parameter.
    /// </summary>
    /// <param name="_dragonSku">Dragon sku in <c>DefinitionsCategory.DRAGONS</c> table.</param>
    /// <returns>0 is returned if <c>_dragonSku</c> is not a valid gragon, otherwise a positive value representing the progress of the dragon sku 
    /// passed as a parameter is returned.
    /// </returns>
    public int GetDragonProgress(string _dragonSku) {
        IDragonData data = null;
        if (m_dragonsBySku != null && m_dragonsBySku.ContainsKey(_dragonSku)) {
            data = m_dragonsBySku[_dragonSku];
        }

        return GetDragonProgress(data);
    }

    /// <summary>
    /// Returns the progress of the dragon data passed as a parameter.
    /// </summary>
    /// <param name="_dragonData">Dragon data.</param>
    /// <returns>0 is returned if <c>_dragonData</c> is null, otherwise a positive value representing the progress of the dragon data
    /// passed as a parameter is returned.
    /// </returns>
    public int GetDragonProgress(IDragonData _dragonData) {
        int returnValue = 0;

		if (_dragonData != null && _dragonData is DragonDataClassic) {
            int highestOrder = _dragonData.GetOrder();

			// Add up maxLevel of all dragons with a lower level.
			foreach(KeyValuePair<string, IDragonData> pair in m_dragonsBySku) {
				// [AOC] Exclude special dragons
				if(pair.Value is DragonDataClassic) {
					DragonDataClassic classicData = pair.Value as DragonDataClassic;
					if(classicData.GetOrder() < highestOrder) {
						// Since level start at 0 the amount of level is maxLevel + 1 
						returnValue += classicData.progression.maxLevel + 1;
					}
				}
            }

            // Add up the current level of that highest dragon.
			returnValue += (_dragonData as DragonDataClassic).progression.level;

            // Dragon level starts at 0 but player progress starts at 1
            returnValue++;
        }

        return returnValue;
    }
	#endregion

	//------------------------------------------------------------------------//
	// REWARDS MANAGEMENT													  //
	//------------------------------------------------------------------------//
	#region rewards
	/// <summary>
	/// Returns whether or not a reward can be pushed to <c>m_rewards</c>, the stack of pending rewards. The requirements that a reward must meet to be allowed to be pushed to the stack are:
	/// a)For a unique reward (the type of reward that can be given only once such as remove_ads):
	///     a.1)The user must not own it already
	///     a.2)It must not be in the stack already
	/// b)For a non unique reward (the default type of reward): No requirements. This type of reward can be pushed with no concerns.
	/// </summary>
	/// <param name="_reward"></param>
	/// <returns></returns>
	private bool CanPushReward(Metagame.Reward _reward) {
        bool _returnValue = true;
        if (_reward == null)
        {
			_returnValue = false;
			Firebase.Crashlytics.Crashlytics.LogException(new Exception(">>>>UserProfile.CanPushReward(): null reward"));
        }
        else if (_reward.IsUnique()) {
            // If the user already owns this type of reward then _reward can not be pushed
            if (_reward.IsAlreadyOwned()) {
                _returnValue = false;
            } else {
                // Loops through all the rewards already in the stack. If one already gives the type of _reward then _reward shouldn't be pushed again because it should be given only once
                if (m_rewards == null)
                {
					Firebase.Crashlytics.Crashlytics.LogException(new Exception(">>>>UserProfile.CanPushReward(): null m_rewards"));
				}
				else
                {
					var enumerator = m_rewards.GetEnumerator();
					while (enumerator.MoveNext() && _returnValue)
					{
						if (enumerator.Current.type == _reward.type)
						{
							_returnValue = false;
						}
					}
				}
			}
        }

        return _returnValue;
    }   

	/// <summary>
	/// Push a reward to the stack.
	/// </summary>
	/// <returns><c>true</c> if the reward has been added. <c>false</c> if the reward hasn't been added because the user already owns it and the user is allowed to own only an instance of this 
	/// type of reward.amount, for example, removeAds is not added if the user already owns it</returns>
	/// <param name="_reward">Reward to be pushed.</param>
	public bool PushReward(Metagame.Reward _reward) {        
        if (!CanPushReward(_reward))		
			return false;

		rewardStack.Push(_reward);
		Debug.Log("<color=green>PUSH! " + _reward.GetType().Name + "</color>");
		Messenger.Broadcast<Metagame.Reward>(MessengerEvents.PROFILE_REWARD_PUSHED, _reward);

		return true;
	}

	/// <summary>
	/// Push a list of rewards to the stack.
	/// </summary>
	/// <returns>The amount of rewards that have been added to the profile effectively. For example removeAds is not added if the user already owns it</returns>
	/// <param name="_rewards">List of rewards to add.</param>
	public int PushRewards(List<Metagame.Reward> _rewards) {
		int _returnValue = 0;
		if (_rewards != null) {
			int count = _rewards.Count;
			for (int i = 0; i < count; i++) {
				if (PushReward (_rewards [i])) {
					_returnValue++;
				}
			}				
		}

		return _returnValue;
	}


	/// <summary>
	/// Pop a reward from the stack.
	/// </summary>
	/// <returns>The popped reward.</returns>
	public Metagame.Reward PopReward() {
		Metagame.Reward r = rewardStack.Pop();
		Debug.Log("<color=red>POP " + r.GetType().Name + "</color>");
		Messenger.Broadcast<Metagame.Reward>(MessengerEvents.PROFILE_REWARD_POPPED, r);
		return r;
	}
	#endregion

	//------------------------------------------------------------------------//
	// OFFERS MANAGEMENT													  //
	//------------------------------------------------------------------------//
	#region offers
	/// <summary>
	/// Updates the offer packs persistance.
	/// </summary>
	/// <param name="">.</param>
	public void UpdateOfferPacksPersistance( SimpleJSON.JSONNode _data )
    {
        string key = "";
        key = "offerPacksRotationalHistory";
        Queue<string> offerPacksRotationalHistory = new Queue<string>();
        if(_data.ContainsKey(key)) {
            // Parse json array into the queue
            JSONArray historyData = _data[key].AsArray;
            for(int i = 0; i < historyData.Count; ++i) {
                offerPacksRotationalHistory.Enqueue(historyData[i]);
            }
        }
        
        key = "offerPacks";
        if(_data.ContainsKey(key)) {
            // Parse json object into the dictionary
            JSONClass offersJson = _data[key] as JSONClass;
            foreach(KeyValuePair<string, JSONNode> kvp in offersJson.m_Dict) {
                JSONClass jSONClass = kvp.Value as JSONClass;   // Old data
                // check type
                string sku = jSONClass["sku"];
                DefinitionNode offerDef = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.OFFER_PACKS, sku);
                OfferPack.Type offerType = OfferPack.StringToType(offerDef.Get("type"));
                switch( offerType ){
                    case OfferPack.Type.PROGRESSION:{
                        //if progression then just keep it
                        m_newOfferPersistanceData[OfferPack.Type.PROGRESSION].Add(jSONClass);
                    }break;   
                    case OfferPack.Type.PUSHED:{
                            // Keep all but we will clean all pushed offers that do not belong to the customizer we are using
                            // Extract customizer from key
                            string customId = kvp.Key;
                            string offerKey = kvp.Key;
                            int substringIndex = offerKey.Length - sku.Length - 1;
                            if ( substringIndex > 0 )
                            { 
                                string customizerId = offerKey.Substring( 0, substringIndex); // remove _sku from XX_sku
                                customId = customId + "." + customizerId;
                            }
                            jSONClass.Add("customId", customId);
                        m_newOfferPersistanceData[OfferPack.Type.PUSHED].Add(jSONClass);
                    }break;
                    case OfferPack.Type.ROTATIONAL:{
                        // Kepp only if in queue
                        if ( offerPacksRotationalHistory.Contains( sku ) )
                        {
                            m_newOfferPersistanceData[OfferPack.Type.ROTATIONAL].Add( jSONClass );
                        }
                    }break;
					case OfferPack.Type.FREE: {
						// Nothing to do, didn't exist in old system
					} break;
                    case OfferPack.Type.REMOVE_ADS:
                        {
                            // Nothing to do, didn't exist in old system
                        }
                    break;
                }
            }
        }
    }
    
    /// <summary>
    /// Cleans old pushed offers. Removes all push offer packs that are not in the current customization id.
    /// </summary>
	/// <param name="_type">Type of offers to be cleaned.</param>
    /// <param name="_currentPushedIDs">Offer IDs in the current customization.</param>
    public void CleanOldPushedOffers(OfferPack.Type _type, List<string> _currentPushedIDs ) {
		// Gather persistence data for the target offer type
		List<JSONClass> offersData = m_newOfferPersistanceData[_type];

		// Reverse-iterate to delete all data that is not in the current customization
		int count = offersData.Count;
		for (int i = count - 1; i >= 0; --i) {
            string customId = "";
            if(offersData[i].ContainsKey("customId")) {
                customId = offersData[i]["customId"];
            }
        
            if(!_currentPushedIDs.Contains(customId)) {
				OffersManager.Log("PUSHED OFFER PACK {0} ({1}) IS BEING CLEANED FROM PERSISTENCE", Color.red, offersData[i]["sku"], _type.ToString());
				offersData.RemoveAt(i);
            }
        }
        m_newOfferPersistanceData[_type] = offersData;
    }

	/// <summary>
	/// Clean offers of a specific type that don't need to be persisted anymore.
	/// </summary>
	/// <param name="_type">The type of offers to be purged.</param>
	/// <param name="_skusToRemove">List of offer Skus to be removed from persistence.</param>
	public void CleanOldOffers(OfferPack.Type _type, List<string> _skusToRemove) {
		// Gather persistence data for the target offer type
		List<JSONClass> offersData = m_newOfferPersistanceData[_type];

		// Reverse-iterate to delete all data that matches the given list IDs
		int count = offersData.Count;
		for(int i = count - 1; i >= 0; --i) {
			if(offersData[i].ContainsKey("sku")) {
				if(_skusToRemove.Contains(offersData[i]["sku"])) {
					OffersManager.Log("OFFER PACK {0} ({1}) IS BEING CLEANED FROM PERSISTENCE", Color.red, offersData[i]["sku"], _type.ToString());
					offersData.RemoveAt(i);
				}
			}
		}
		m_newOfferPersistanceData[_type] = offersData;
	}

	/// <summary>
	/// Register an offer pack for persistence save.
	/// </summary>
	/// <param name="_offerPack">Pack to be saved.</param>
	public void SaveOfferPack(OfferPack _pack) {
		// Some checks
		if(_pack == null) return;

		// If pack shouldn't be persisted, clear it from persistence data
		bool shouldBePersisted = _pack.ShouldBePersisted();

        // Check whether we already have data for this offer pack
        List<JSONClass> offers = m_newOfferPersistanceData[_pack.type];
        int max = offers.Count;
        bool found = false;
        for(int i = 0; i < max && !found; i++) {
            if(offers[i].ContainsKey("sku") && offers[i]["sku"] == _pack.def.sku) {
				switch(_pack.type) {
					// If it's a pushed offer, check customization ID
					case OfferPack.Type.PUSHED:
					case OfferPack.Type.DRAGON_DISCOUNT: {
						// Only if it actually needs to be persisted!
						if(shouldBePersisted) {
							if(offers[i].ContainsKey("customId")) {
								string customId = OffersManager.GenerateTrackingOfferName(_pack.def);
								if(customId == offers[i]["customId"]) {
									m_newOfferPersistanceData[_pack.type][i] = _pack.Save();
									found = true;
								}
							}
						}
					} break;

					// Otherwise just directly override existing data
					default: {
						// If pack doesn't need to be persisted anymore, remove it from the data instead
						if(shouldBePersisted) {
							m_newOfferPersistanceData[_pack.type][i] = _pack.Save();
						} else {
							OffersManager.Log("OFFER PACK {0} ({1}) IS BEING CLEANED FROM PERSISTENCE", Color.red, _pack.def.sku, _pack.type.ToString());
							m_newOfferPersistanceData[_pack.type].RemoveAt(i);  // We will interrupt the loop by setting the found flag to true, so there is no problem in removing an element at this point
						}
						found = true;
					} break;
				}
            }
        }

		// If we couldn't find existing data for this offer pack, add it!
        if(!found && shouldBePersisted) {
            m_newOfferPersistanceData[_pack.type].Add(_pack.Save());
        }
	}

	/// <summary>
	/// Load persistence data corresponding to a specific pack into it, if there is any.
	/// </summary>
	public void LoadOfferPack(OfferPack _pack) {
		// Parameter check
		if(_pack == null) return;

		// Do we have persistence data for this pack?
        List<JSONClass> offers = m_newOfferPersistanceData[_pack.type];
        int max = offers.Count;
        bool found = false;
        for (int i = 0; i < max && !found; i++){
            if ( offers[i].ContainsKey("sku") && offers[i]["sku"] == _pack.def.sku ){
				// Break loop
                found = true;

				// Check customization code as well for pushed offers
				bool match = false;
				switch(_pack.type) {
					case OfferPack.Type.PUSHED:
					case OfferPack.Type.DRAGON_DISCOUNT: {
						match = _pack.def.customizationCode == offers[i]["customizerId"];
                    } break;

					default: {
						match = true;
                    } break;
                }

				// If we have a match, load data into the pack
				if(match) {
					_pack.Load(offers[i]);
				}
            }
        }
        
	}
	
    /// <summary>
    /// Load persistence data corresponding to a ads removal offer if there is any.
    /// </summary>
    public void LoadRemoveAdsOffer(RemoveAdsFeature _removeAds)
    {
        if (_removeAds != null)
        {
            _removeAds.IsActive = m_removeAdsOfferActive;
            _removeAds.easyExtraMissionsLeft = m_easyMissionCooldownsLeft;
            _removeAds.mediumExtraMissionsLeft = m_mediumMissionCooldownsLeft;
            _removeAds.hardExtraMissionsLeft = m_hardMissionCooldownsLeft;
            _removeAds.mapRevealTimestamp = m_mapRevealTimestamp;
        }
    }

    /// <summary>
    /// Save persistence data corresponding to an ads removal offer if there is any.
    /// </summary>
    public void SaveRemoveAdsOffer(RemoveAdsFeature _removeAds)
    {
        if (_removeAds != null)
        {
            m_removeAdsOfferActive = _removeAds.IsActive;
            m_easyMissionCooldownsLeft = _removeAds.easyExtraMissionsLeft;
            m_mediumMissionCooldownsLeft = _removeAds.mediumExtraMissionsLeft;
            m_hardMissionCooldownsLeft = _removeAds.hardExtraMissionsLeft;
            m_mapRevealTimestamp = _removeAds.mapRevealTimestamp;
        }
    }
	#endregion

	//------------------------------------------------------------------------//
	// CLUSTERING															  //
	//------------------------------------------------------------------------//
	#region clustering
	/// <summary>
	/// Get the stored cluster ID for this user.
	/// </summary>
	/// <param name="_checkGDPRCompliance">Whether to check if the cluster ID is compliant with current GDPR settings or not. If set to true and not compliant, it will be corrected.</param>
	/// <returns></returns>
	public string GetClusterId(bool _checkGDPRCompliance = true) {
		// Check GDPR compliance?
		if(_checkGDPRCompliance) {
			// If current assigned cluster ID is not compliant with GDPR settings, assign the Generic one
			if(!CheckClusterIdGDPRCompliance(m_clusterId)) {
				SetClusterId(ClusteringManager.CLUSTER_GENERIC);
				UsersManager.currentUser.clusterSynced = false;     // Pending sync with server
				ClusteringManager.Instance.SyncWithServer();
			}
		}

		// Return stored cluster ID
		return m_clusterId;
	}

	/// <summary>
	/// Set a new cluster Id for this user. No checks performed.
	/// </summary>
	/// <param name="_newClusterId">The Cluster Id assigned to this user.</param>
	public void SetClusterId(string _newClusterId) {
		// Store the new value
		m_clusterId = _newClusterId;

		// Notify game
		ClusterIdEventInfo eventData = new ClusterIdEventInfo() { clusterId = _newClusterId };
		Broadcaster.Broadcast(BroadcastEventType.CLUSTER_ID_ASSIGNED, eventData);

		// Send tracking event
		HDTrackingManager.Instance.Notify_ClusterAssigned(_newClusterId);
	}

	/// <summary>
	/// Is the given cluster ID compliant with current GDPR settings?
	/// </summary>
	/// <param name="_clusterId">The cluster ID to be checked.</param>
	/// <returns>Whether the given cluster ID is compliant with GDPR settings or not.</returns>
	private bool CheckClusterIdGDPRCompliance(string _clusterId) {
		// No problems if cluster is not assigned
		if(string.IsNullOrEmpty(_clusterId)) return true;

		// If user is underage, the cluster must be the generic one
		if(GDPRManager.SharedInstance.IsAgeRestrictionEnabled()) {
			return _clusterId == ClusteringManager.CLUSTER_GENERIC;
		}

		// If user is an adult, check the marketing consent flag. If not given, generic cluster must be used.
		else if(GDPRManager.SharedInstance.IsConsentTrackingRestrictionEnabled()) {
			return _clusterId == ClusteringManager.CLUSTER_GENERIC;
		}

		// No more checks to perform, cluster is compliant with current GDPR settings
		return true;
	}
	#endregion
}