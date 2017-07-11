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
using System.Collections;
using System.Collections.Generic;
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
public class UserProfile : UserSaveSystem
{
    //------------------------------------------------------------------------//
    // CONSTANTS															  //
    //------------------------------------------------------------------------//
	public enum Currency {
		NONE,

		SOFT,
		HARD,
		REAL,
		GOLDEN_FRAGMENTS,

		COUNT
	};

    //------------------------------------------------------------------------//
    // MEMBERS																  //
    //------------------------------------------------------------------------//

    //------------------------------------------------------------------------//
    // PROPERTIES															  //
    //------------------------------------------------------------------------//
    // Last save timestamp
    private DateTime m_saveTimestamp = DateTime.UtcNow;
    public DateTime saveTimestamp {
        get { return m_saveTimestamp; }
    }
    public int lastModified { get; set; }

	// User ID shortcut
	public string userId {
		get {
			if(GameSessionManager.SharedInstance.IsLogged()) {
				return GameSessionManager.SharedInstance.GetUID(); 
			} else {
				return "local_user";
			}
		}
	}

    // Economy
	private long m_coins;
	public long coins {
		get { return m_coins; }
	}

	private long m_pc;
	public  long pc { 
		get { return m_pc; }
	}

	// Game Settings
	private string m_currentDragon = "";
	public string currentDragon {
		get { return m_currentDragon; }
		set {
            m_currentDragon = value;
        }
	}

	private string m_currentLevel = "";
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
	private int m_gamesPlayed = 0;
	public int gamesPlayed {
		get { return m_gamesPlayed; }
		set {
			// Mark tutorial as completed if > 0
			m_gamesPlayed = value;
			SetTutorialStepCompleted(TutorialStep.FIRST_RUN, m_gamesPlayed > 0);
			SetTutorialStepCompleted(TutorialStep.SECOND_RUN, m_gamesPlayed > 1);
		}
	}

	private long m_highScore = 0;
	public long highScore {
		get { return m_highScore; }
		set { m_highScore = value; }
	}
	
	private int m_superFuryProgression = 0;
	public int superFuryProgression {
		get { return m_superFuryProgression; }
		set { m_superFuryProgression = value; }
	}

	// Dragon Data
	private Dictionary<string,DragonData> m_dragonsBySku;	// Owned Dragons by Sku
	public Dictionary<string,DragonData> dragonsBySku
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

	private int m_goldenEggFragments = 0;
	public int goldenEggFragments {
		get { return m_goldenEggFragments; }
		set { 
			int oldValue = m_goldenEggFragments;
			m_goldenEggFragments = value;
			Messenger.Broadcast<UserProfile.Currency, long, long>(GameEvents.PROFILE_CURRENCY_CHANGED, UserProfile.Currency.GOLDEN_FRAGMENTS, oldValue, m_goldenEggFragments);
		}
	}

	private int m_goldenEggsCollected = 0;
	public int goldenEggsCollected {
		get { return m_goldenEggsCollected; }
		set { m_goldenEggsCollected = value; }
	}

    // Chests
    private Chest[] m_dailyChests = new Chest[ChestManager.NUM_DAILY_CHESTS];	// Should always have the same length
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

	private int m_dailyRemoveMissionAdUses = 0;
	public int dailyRemoveMissionAdUses {
		get{ return m_dailyRemoveMissionAdUses; }
		set{ m_dailyRemoveMissionAdUses = value; }
	}

	// Map upgrades
	private DateTime m_mapResetTimestamp;
	public DateTime mapResetTimestamp {
		get{ return m_mapResetTimestamp; }
		set{ m_mapResetTimestamp = value; }
	}

	public bool mapUnlocked {
		// Map is unlocked as long as the timestamp hasn't expired
		get { return m_mapResetTimestamp > DateTime.UtcNow; }
	}

	// Global events
	// Events dictionary
	private Dictionary<int, GlobalEventUserData> m_globalEvents = new Dictionary<int, GlobalEventUserData>();
	public Dictionary<int, GlobalEventUserData> globalEvents {
		get { return m_globalEvents; }
	}

    //------------------------------------------------------------------------//
    // GENERIC METHODS														  //
    //------------------------------------------------------------------------//
	/// <summary>
	/// Default constructor.
	/// </summary>
	public UserProfile()
	{
		m_dragonsBySku = new Dictionary<string, DragonData>();
		DragonData newDragonData = null;
		List<DefinitionNode> defs = DefinitionsManager.SharedInstance.GetDefinitionsList(DefinitionsCategory.DRAGONS);
		for(int i = 0; i < defs.Count; i++) {
			newDragonData = new DragonData();
			newDragonData.Init(defs[i]);
			m_dragonsBySku[defs[i].sku] = newDragonData;
		}

		m_eggsInventory = new Egg[EggManager.INVENTORY_SIZE];
		m_incubatingEgg = null;
		m_goldenEggFragments = 0;
		m_goldenEggsCollected = 0;

		m_wardrobe = new Wardrobe();
		m_petCollection = new PetCollection();
		m_userMissions = new UserMissions();      
    }

	/// <summary>
	/// Return a string representation of this class.
	/// </summary>
	/// <returns>A formatted json string representing this class.</returns>
	public override string ToString() {
		return ToJson().ToString();
	}
		
	//------------------------------------------------------------------------//
	// CURRENCIES MANAGEMENT METHODS										  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Add coins.
	/// </summary>
	/// <param name="_amount">Amount to add. Negative to subtract.</param>
	public void AddCoins(long _amount) {
		// Skip checks for now
		// Compute new value and dispatch event
		m_coins += _amount;
		Messenger.Broadcast<long, long>(GameEvents.PROFILE_COINS_CHANGED, coins - _amount, coins);
		Messenger.Broadcast<UserProfile.Currency, long, long>(GameEvents.PROFILE_CURRENCY_CHANGED, UserProfile.Currency.SOFT, coins - _amount, coins);
	}
	
	/// <summary>
	/// Add PC.
	/// </summary>
	/// <param name="_iAmount">Amount to add. Negative to subtract.</param>
	public void AddPC(long _amount) {
		// Skip checks for now
		// Compute new value and dispatch event
		m_pc += _amount;
		Messenger.Broadcast<long, long>(GameEvents.PROFILE_PC_CHANGED, pc - _amount, pc);
		Messenger.Broadcast<UserProfile.Currency, long, long>(GameEvents.PROFILE_CURRENCY_CHANGED, UserProfile.Currency.HARD, pc - _amount, pc);
	}

	/// <summary>
	/// Add any type of currency.
	/// </summary>
	/// <param name="_amount">Amount to be added. Negative to subtract.</param>
	/// <param name="_currency">Currency type.</param>
	public void AddCurrency(long _amount, Currency _currency) {
		switch(_currency) {
			case Currency.SOFT:				AddCoins(_amount);					break;
			case Currency.HARD:				AddPC(_amount);						break;
			case Currency.GOLDEN_FRAGMENTS:	goldenEggFragments += (int)_amount;	break;
		}
	}

	/// <summary>
	/// Get current amount of any currency.
	/// </summary>
	/// <returns>The current amount of the required currency.</returns>
	/// <param name="_currency">Currency type.</param>
	public long GetCurrency(Currency _currency) {
		switch(_currency) {
			case Currency.SOFT:				return coins;						break;
			case Currency.HARD:				return pc;							break;
			case Currency.GOLDEN_FRAGMENTS:	return (long)goldenEggFragments;	break;
		}
		return 0;
	}

	/// <summary>
	/// Convert currency from enum to string.
	/// </summary>
	public static string CurrencyToSku(Currency _currency) {
		switch(_currency) {
			case Currency.SOFT: return "sc";
			case Currency.HARD: return "hc";
			case Currency.GOLDEN_FRAGMENTS: return "goldenFragments";
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
			case "goldenFragments": return Currency.GOLDEN_FRAGMENTS;
		}
		return Currency.NONE;
	}

	//------------------------------------------------------------------------//
	// TUTORIAL																  //
	// To simplify bitmask operations										  //
	//------------------------------------------------------------------------//
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
			Messenger.Broadcast(GameEvents.TUTORIAL_STEP_TOGGLED, _step, _completed);
		}
	}

	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Increases the map level.
	/// Doesn't perform any check or currency transaction, resets timer.
	/// Broadcasts the PROFILE_MAP_UNLOCKED event.
	/// </summary>
	public void UnlockMap() {
		// Reset timer to the start of the following day, in local time zone
		// [AOC] Small trick to figure out the start of a day, from http://stackoverflow.com/questions/3362959/datetime-now-first-and-last-minutes-of-the-day
		//DateTime tomorrow = DateTime.Now.AddDays(1);	// Using local time zone to compute tomorrow's date
		//m_mapResetTimestamp = tomorrow.Date.ToUniversalTime();	// Work in UTC

		// [AOC] Testing purposes
		//m_mapResetTimestamp = DateTime.Now.AddSeconds(30).ToUniversalTime();

		// [AOC] Fuck it! Easier implementation, fixed timer from the moment you unlock the map
		DefinitionNode gameSettingsDef = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.SETTINGS, "gameSettings");
		if(gameSettingsDef != null) {
			m_mapResetTimestamp = DateTime.UtcNow.AddMinutes(gameSettingsDef.GetAsDouble("miniMapTimer"));	// Minutes
		} else {
			m_mapResetTimestamp = DateTime.UtcNow.AddHours(24);	// Default timer just in case
		}
		Messenger.Broadcast(GameEvents.PROFILE_MAP_UNLOCKED);
	}

	/// <summary>
	/// Gets the number OF owned dragons.
	/// </summary>
	/// <returns>The number owned dragons.</returns>
	public int GetNumOwnedDragons()
	{
		int ret = 0;
		foreach( KeyValuePair<string, DragonData> pair in m_dragonsBySku )
		{
			if ( pair.Value.isOwned )
				ret++;
		}
		return ret;
	}

    //------------------------------------------------------------------------//
    // PUBLIC PERSISTENCE METHODS											  //
    //------------------------------------------------------------------------//   
	public override void Load()
    {
        base.Load();

        string jsonAsString = m_saveData.ToString();
        if (jsonAsString != null)
        {   
			#if UNITY_EDITOR
			JsonFormatter fmt = new JsonFormatter();
			Debug.Log("<color=cyan>LOADING USER PROFILE:</color> " + fmt.PrettyPrint(jsonAsString));
			#endif
			JSONNode json = JSON.Parse(jsonAsString);
            Load(json);
        }       
    }

    public override void Save()
    {
        base.Save();

        // Update timestamp
        m_saveTimestamp = DateTime.UtcNow;

        JSONNode json = ToJson();
		m_saveData.Merge(json.ToString());
    }

	//------------------------------------------------------------------------//
	// PERSISTENCE LOAD METHODS												  //
	//------------------------------------------------------------------------//   
    /// <summary>
    /// Load state from a json object.
    /// </summary>
    /// <param name="_data">The data object loaded from persistence.</param>
    private void Load(SimpleJSON.JSONNode _data) {
		// Just read values from persistence object
		SimpleJSON.JSONNode profile = _data["userProfile"];

        if (profile.ContainsKey("timestamp"))
        {
			m_saveTimestamp = DateTime.Parse(profile["timestamp"], PersistenceManager.JSON_FORMATTING_CULTURE);
        }
        else
        {
            m_saveTimestamp = DateTime.UtcNow;
        }

        // Economy
        string key = "sc";
        if (profile.ContainsKey(key)) {
            m_coins = profile[key].AsInt;
        } else {
            m_coins = 0;
        }

        key = "pc";
        if (profile.ContainsKey(key)) {
            m_pc = profile[key].AsInt;
        } else {
            m_pc = 0;
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
            m_tutorialStep = (TutorialStep)profile["tutorialStep"].AsInt;
        } else {
            m_tutorialStep = (TutorialStep)0;
        }

        key = "furyUsed";
        if (profile.ContainsKey(key)) {
            m_furyUsed = profile[key].AsBool;
        } else {
            m_furyUsed = false;
        }        

        // Game stats
        key = "gamesPlayed";
        if (profile.ContainsKey(key)) {
            m_gamesPlayed = profile[key].AsInt;
        } else {
            m_gamesPlayed = 0;
        }

        key = "highScore";
        if (profile.ContainsKey(key)) {
            m_highScore = profile[key].AsLong;
        }
        else {
            m_highScore = 0;
        }

        key = "superFuryProgression";
        if (profile.ContainsKey(key)) {
            m_superFuryProgression = profile[key].AsInt;
        }
        else {
            m_superFuryProgression = 0;
        }        

		// Some cheats override profile settings - will be saved with the next Save()
		if(Prefs.GetBoolPlayer("skipTutorialCheat")) {
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
			foreach( KeyValuePair<string, DragonData> pair in m_dragonsBySku)
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
		if(_data.ContainsKey("missions")) {
			m_userMissions.Load(_data["missions"]);
		} else {
			// Clean missions
			m_userMissions.ClearAllMissions();
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
			for(int i = 0; i < ChestManager.NUM_DAILY_CHESTS; i++) {
				dailyChests[i] = new Chest();
			}
			m_dailyChestsResetTimestamp = DateTime.UtcNow;
		}

		// Daily mission Ads
		m_dailyRemoveMissionAdUses = 0;
		if ( _data.ContainsKey("dailyRemoveMissionAdTimestamp") )
		{
			m_dailyRemoveMissionAdTimestamp = DateTime.Parse(_data["dailyRemoveMissionAdTimestamp"], PersistenceManager.JSON_FORMATTING_CULTURE);;

			if ( _data.ContainsKey("dailyRemoveMissionAdUses") )
				m_dailyRemoveMissionAdUses = _data["dailyRemoveMissionAdUses"].AsInt;
		}
		else
		{
			m_dailyRemoveMissionAdTimestamp = DateTime.UtcNow;
		}

		// Map upgrades
		key = "mapResetTimestamp";
		if(_data.ContainsKey(key)) {
			m_mapResetTimestamp = DateTime.Parse(_data["mapResetTimestamp"], PersistenceManager.JSON_FORMATTING_CULTURE);
		} else {
			m_mapResetTimestamp = DateTime.UtcNow;	// Already expired
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
				m_globalEvents[newEvent.eventID] = newEvent;
			}
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

		// Incubator timer
		m_incubationEndTimestamp = DateTime.Parse(_data["incubationEndTimestamp"], PersistenceManager.JSON_FORMATTING_CULTURE);

        // Eggs collected
        eggsCollected = _data["collectedAmount"].AsInt;

		// Golden egg
		m_goldenEggFragments = _data["goldenEggFragments"].AsInt;
		m_goldenEggsCollected = _data["goldenEggsCollected"].AsInt;
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
			|| dailyChests[i].state == Chest.State.PENDING_REWARD) {
				dailyChests[i].ChangeState(Chest.State.NOT_COLLECTED);
			}
		}

		// Reset timestamp
		m_dailyChestsResetTimestamp = DateTime.Parse(_data["resetTimestamp"], PersistenceManager.JSON_FORMATTING_CULTURE);
	}

	//------------------------------------------------------------------------//
	// PERSISTENCE SAVE METHODS												  //
	//------------------------------------------------------------------------//
    /// <summary>
    /// Create a json with the current data in the profile.
    /// Similar to Save(), but doesn't update timestamp nor save count.
    /// </summary>
    /// <returns>A json representing this profile.</returns>
    public SimpleJSON.JSONClass ToJson() {
		// Create new object
		SimpleJSON.JSONClass data = new SimpleJSON.JSONClass();
		SimpleJSON.JSONClass profile = new SimpleJSON.JSONClass();

        profile.Add("timestamp", m_saveTimestamp.ToString(PersistenceManager.JSON_FORMATTING_CULTURE));

        // Economy
		profile.Add( "sc", m_coins.ToString(PersistenceManager.JSON_FORMATTING_CULTURE));
		profile.Add( "pc", m_pc.ToString(PersistenceManager.JSON_FORMATTING_CULTURE));

		// Game settings
		profile.Add("currentDragon",m_currentDragon);
		profile.Add("currentLevel",m_currentLevel);
		profile.Add("tutorialStep",((int)m_tutorialStep).ToString(PersistenceManager.JSON_FORMATTING_CULTURE));
		profile.Add("furyUsed", m_furyUsed.ToString(PersistenceManager.JSON_FORMATTING_CULTURE));

		// Game stats
		profile.Add("gamesPlayed",m_gamesPlayed.ToString(PersistenceManager.JSON_FORMATTING_CULTURE));
		profile.Add("highScore",m_highScore.ToString(PersistenceManager.JSON_FORMATTING_CULTURE));
		profile.Add("superFuryProgression",m_superFuryProgression.ToString(PersistenceManager.JSON_FORMATTING_CULTURE));

		data.Add("userProfile", profile);

		// Dragons
		SimpleJSON.JSONArray dragons = new SimpleJSON.JSONArray();
		foreach( KeyValuePair<string,DragonData> pair in m_dragonsBySku)
		{
			DragonData dragonData = pair.Value;
			if ( dragonData.isOwned || !dragonData.isLocked )
				dragons.Add( dragonData.Save() );
		}
		data.Add( "dragons", dragons );

		data.Add("disguises", m_wardrobe.Save());
		data.Add("pets", m_petCollection.Save());
		data.Add("missions", m_userMissions.Save());

		data.Add("eggs", SaveEggData());
		data.Add("chests", SaveChestsData());

		// Daily remove missions with ads
		data.Add("dailyRemoveMissionAdTimestamp", m_dailyRemoveMissionAdTimestamp.ToString(PersistenceManager.JSON_FORMATTING_CULTURE));
		data.Add("dailyRemoveMissionAdUses", m_dailyRemoveMissionAdUses.ToString(PersistenceManager.JSON_FORMATTING_CULTURE));

		// Map upgrades
		data.Add("mapResetTimestamp", m_mapResetTimestamp.ToString(PersistenceManager.JSON_FORMATTING_CULTURE));

		// Global Events
		SimpleJSON.JSONArray eventsData = new SimpleJSON.JSONArray();
		foreach(KeyValuePair<int, GlobalEventUserData> kvp in m_globalEvents) {
			eventsData.Add(kvp.Value.Save(true));
		}
		data.Add("globalEvents", eventsData);

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

		// Incubator timer
		data.Add("incubationEndTimestamp", m_incubationEndTimestamp.ToString(PersistenceManager.JSON_FORMATTING_CULTURE));

        // Eggs collected
		data.Add("collectedAmount", eggsCollected.ToString(PersistenceManager.JSON_FORMATTING_CULTURE));

		// Golden eggs
		data.Add("goldenEggFragments", m_goldenEggFragments.ToString(PersistenceManager.JSON_FORMATTING_CULTURE));
		data.Add("goldenEggsCollected", m_goldenEggsCollected.ToString(PersistenceManager.JSON_FORMATTING_CULTURE));

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
		data.Add("resetTimestamp", m_dailyChestsResetTimestamp.ToString(PersistenceManager.JSON_FORMATTING_CULTURE));

		// Done!
		return data;
	}

	//------------------------------------------------------------------------//
	// DISGUISES MANAGEMENT													  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Get the sku of the disguise equipped to a specific dragon.
	/// </summary>
	/// <returns>The sku of the equipped disguise.</returns>
	/// <param name="_dragonSku">The dragon whose disguie we want to check.</param>
	public string GetEquipedDisguise( string _dragonSku )
	{
		if ( m_dragonsBySku.ContainsKey( _dragonSku ) )
			return m_dragonsBySku[ _dragonSku ].diguise;
		return "";
	}

	/// <summary>
	/// Try to equip the given disguise into the target dragon.
	/// Doesn't check that the disguise actually belongs to the dragon.
	/// </summary>
	/// <returns><c>true</c> if the disguise was different from the one previously equiped by the dragon, <c>false</c> otherwise.</returns>
	/// <param name="_dragonSku">Dragon sku.</param>
	/// <param name="_disguiseSku">Disguise sku.</param>
	public bool EquipDisguise( string _dragonSku, string _disguiseSku)
	{
		bool ret = false;
		if ( m_dragonsBySku.ContainsKey( _dragonSku ) )
		{
			if ( m_dragonsBySku[_dragonSku].diguise != _disguiseSku )
			{
				ret = true;
				m_dragonsBySku[_dragonSku].diguise = _disguiseSku;
			}
		}
		return ret;
	}

	//------------------------------------------------------------------------//
	// PETS MANAGEMENT														  //
	//------------------------------------------------------------------------//
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
		DragonData dragon = m_dragonsBySku[_dragonSku];

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
		DragonData dragon = m_dragonsBySku[_dragonSku];

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
				Messenger.Broadcast<string, int, string>(GameEvents.MENU_DRAGON_PET_CHANGE, _dragonSku, i, _petSku);

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
		DragonData dragon = m_dragonsBySku[_dragonSku];

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
		Messenger.Broadcast<string, int, string>(GameEvents.MENU_DRAGON_PET_CHANGE, _dragonSku, _slotIdx, _petSku);

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
		DragonData dragon = m_dragonsBySku[_dragonSku];

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
		DragonData dragon = m_dragonsBySku[_dragonSku];

		// Check slot index
		if(_slotIdx < 0 || _slotIdx >= dragon.pets.Count) return -2;

		// Make sure index is equipped
		if(string.IsNullOrEmpty(dragon.pets[_slotIdx])) return -3;

		// Everything ok, unequip the pet!
		dragon.pets[_slotIdx] = string.Empty;

		// Notify game
		Messenger.Broadcast<string, int, string>(GameEvents.MENU_DRAGON_PET_CHANGE, _dragonSku, _slotIdx, string.Empty);

		return _slotIdx;
	}

	//------------------------------------------------------------------------//
	// GLOBAL EVENTS MANAGEMENT												  //
	//------------------------------------------------------------------------//
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
			data = new GlobalEventUserData();
			data.eventID = _eventID;
			m_globalEvents[_eventID] = data;
		}
		return data;
	}

    /// <summary>
    /// Returns an int that sums up the user's progress.
    /// </summary>
    /// <returns></returns>
    public int GetPlayerProgress()
    {
        int returnValue = 0;

        // Find the dragon with the highest level among all dragons acquired by the user.
        if (m_dragonsBySku != null)
        {
            DragonData highestDragon = null;
            foreach (KeyValuePair<string, DragonData> pair in m_dragonsBySku)
            {
                if (pair.Value.isOwned)
                {                                        
		            int order = pair.Value.GetOrder();
                    if (highestDragon == null || order > highestDragon.GetOrder())
                    {
                        highestDragon = pair.Value;
                    }
                }
            }

            if (highestDragon != null)
            {
                int highestOrder = highestDragon.GetOrder();

                // Add up maxLevel of all dragons with a lower level.
                foreach (KeyValuePair<string, DragonData> pair in m_dragonsBySku)
                {
                    if (pair.Value.GetOrder() < highestOrder)
                    {
                        // Since level start at 0 the amount of level is maxLevel + 1 
                        returnValue += pair.Value.progression.maxLevel + 1;                        
                    }
                }

                // Add up the current level of that highest dragon.
                returnValue += highestDragon.progression.level;

                // Dragon level starts at 0 but player progress starts at 1
                returnValue++;
            }
        }        

        return returnValue;
    }
}

