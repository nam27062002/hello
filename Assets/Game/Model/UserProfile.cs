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

    //------------------------------------------------------------------------//
    // MEMBERS																  //
    //------------------------------------------------------------------------//

    //------------------------------------------------------------------------//
    // PROPERTIES															  //
    //------------------------------------------------------------------------//		

    // Last save timestamp
    private DateTime m_saveTimestamp = DateTime.UtcNow;
    public DateTime saveTimestamp
    {
        get { return m_saveTimestamp; }
    }

    public int lastModified { get; set; }

    // Set default values in the inspector, use static methods to set them from code
    // [AOC] We want these to be consulted but never set from outside, so don't add a setter
    [Separator("Economy")]
	[SerializeField] private long m_coins;
	public long coins {
		get { return m_coins; }
	}
	
	[SerializeField] private long m_pc;
	public  long pc { 
		get { return m_pc; }
	}

	[Separator("Game Settings")]
	[SerializeField] private string m_currentDragon = "";
	public string currentDragon {
		get { return m_currentDragon; }
		set {
            m_currentDragon = value;
        }
	}

	[SerializeField] /*[SkuList(Definitions.Category.LEVELS)]*/ private string m_currentLevel = "";
	public string currentLevel {
		get { return m_currentLevel; }
		set { m_currentLevel = value; }
	}

	[SerializeField] private TutorialStep m_tutorialStep;
	public TutorialStep tutorialStep { 
		get { return m_tutorialStep; }
		set { m_tutorialStep = value; }
	}

	[SerializeField] private bool m_furyUsed = false;
	public bool furyUsed {
		get { return m_furyUsed; }
		set { m_furyUsed = value; }
	}

	[Separator("Game Stats")]
	[SerializeField] private int m_gamesPlayed = 0;
	public int gamesPlayed {
		get { return m_gamesPlayed; }
		set { m_gamesPlayed = value; }
	}

	[SerializeField] private long m_highScore = 0;
	public long highScore {
		get { return m_highScore; }
		set { m_highScore = value; }
	}
	
	[SerializeField] private int m_superFuryProgression = 0;
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

	// Egg Data
	private Egg[] m_eggsInventory;
	public Egg[] eggsInventory
	{
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

	// DISGUISES
	Wardrobe m_wardrobe;
	public Wardrobe wardrobe
	{
		get{ return m_wardrobe; }
	}

	UserMissions m_userMissions;
	public UserMissions userMissions
	{
		get{ return m_userMissions; }
	}

    //------------------------------------------------------------------------//
    // GENERIC METHODS														  //
    //------------------------------------------------------------------------//

    //------------------------------------------------------------------------//
    // PUBLIC STATIC METHODS												  //
    //------------------------------------------------------------------------//    
	public UserProfile()
	{
		m_dragonsBySku = new Dictionary<string, DragonData>();
		DragonData newDragonData = null;
		List<DefinitionNode> defs = new List<DefinitionNode>();
		DefinitionsManager.SharedInstance.GetDefinitions(DefinitionsCategory.DRAGONS, ref defs);
		for(int i = 0; i < defs.Count; i++) {
			newDragonData = new DragonData();
			newDragonData.Init(defs[i]);
			m_dragonsBySku[defs[i].sku] = newDragonData;
		}


		m_eggsInventory = new Egg[EggManager.INVENTORY_SIZE];
		m_incubatingEgg = null;

		m_wardrobe = new Wardrobe();
		m_userMissions = new UserMissions();      
    }


	/// <summary>
	/// Add coins.
	/// </summary>
	/// <param name="_amount">Amount to add. Negative to subtract.</param>
	public void AddCoins(long _amount) {
		// Skip checks for now
		// Compute new value and dispatch event
		m_coins += _amount;
		Messenger.Broadcast<long, long>(GameEvents.PROFILE_COINS_CHANGED, coins - _amount, coins);
	}
	
	/// <summary>
	/// Add PC.
	/// </summary>
	/// <param name="_iAmount">Amount to add. Negative to subtract.</param>
	public void AddPC(long _iAmount) {
		// Skip checks for now
		// Compute new value and dispatch event
		m_pc += _iAmount;
		Messenger.Broadcast<long, long>(GameEvents.PROFILE_PC_CHANGED, pc - _iAmount, pc);
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

		if(_completed) {
			m_tutorialStep |= _step;
		} else {
			m_tutorialStep &= ~_step;
		}
	}

    //------------------------------------------------------------------------//
    // PERSISTENCE															  //
    //------------------------------------------------------------------------//   

    public override void Load()
    {
        base.Load();

        string jsonAsString = m_saveData.ToString();
        if (jsonAsString != null)
        {            
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

    /// <summary>
    /// Load state from a json object.
    /// </summary>
    /// <param name="_data">The data object loaded from persistence.</param>
    private void Load(SimpleJSON.JSONNode _data) {
		// Just read values from persistence object
		Debug.Log("Loading UserProfile\n" + _data.ToString());
		SimpleJSON.JSONNode profile = _data["userProfile"];

        if (profile.ContainsKey("timestamp"))
        {
            m_saveTimestamp = DateTime.Parse(profile["timestamp"], System.Globalization.CultureInfo.InvariantCulture);
        }
        else
        {
            m_saveTimestamp = DateTime.Now;
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
			Prefs.SetBoolPlayer("skipTutorialCheat", false);
		}

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

		if ( _data.ContainsKey("eggs") )
		{
			LoadEggData(_data["eggs"] as SimpleJSON.JSONClass);
		}
		else
		{
			// Clean Eggs Data
			for( int i = 0; i<EggManager.INVENTORY_SIZE; i++ )
				eggsInventory[i] = null;
			m_incubatingEgg = null;
		}
		
		m_wardrobe.InitFromDefinitions();
		if ( _data.ContainsKey("disguises") )
			m_wardrobe.Load( _data["disguises"] );

		if ( _data.ContainsKey("missions") )
		{
			m_userMissions.Load( _data["missions"] );
			m_userMissions.ownedDragons = GetNumOwnedDragons();
		}
		else
		{
			// Clean missions
			m_userMissions.ClearAllMissions();
		}
	}

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
		m_incubationEndTimestamp = DateTime.Parse(_data["incubationEndTimestamp"], System.Globalization.CultureInfo.InvariantCulture);
	}
	
    /// <summary>
    /// Create a json with the current data in the profile.
    /// Similar to Save(), but doesn't update timestamp nor save count.
    /// </summary>
    /// <returns>A json representing this profile.</returns>
    public SimpleJSON.JSONClass ToJson() {
		// Create new object
		SimpleJSON.JSONClass data = new SimpleJSON.JSONClass();
		SimpleJSON.JSONClass profile = new SimpleJSON.JSONClass();

        profile.Add("timestamp", m_saveTimestamp.ToString(System.Globalization.CultureInfo.InvariantCulture));

        // Economy
        profile.Add( "sc", m_coins.ToString());
		profile.Add( "pc", m_pc.ToString());

		// Game settings
		profile.Add("currentDragon",m_currentDragon);
		profile.Add("currentLevel",m_currentLevel);
		profile.Add("tutorialStep",((int)m_tutorialStep).ToString());
		profile.Add("furyUsed", m_furyUsed.ToString());

		// Game stats
		profile.Add("gamesPlayed",m_gamesPlayed.ToString());
		profile.Add("highScore",m_highScore.ToString());
		profile.Add("superFuryProgression",m_superFuryProgression.ToString());
		data.Add("userProfile", profile);

		// DRAGONS
		SimpleJSON.JSONArray dragons = new SimpleJSON.JSONArray();
		foreach( KeyValuePair<string,DragonData> pair in m_dragonsBySku)
		{
			DragonData dragonData = pair.Value;
			if ( dragonData.isOwned || !dragonData.isLocked )
				dragons.Add( dragonData.Save() );
		}
		data.Add( "dragons", dragons );

		data.Add("eggs", SaveEggData());
		data.Add("disguises", m_wardrobe.Save());
		data.Add("missions", m_userMissions.Save());

		// Return it
		return data;
	}

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
		data.Add("incubationEndTimestamp", m_incubationEndTimestamp.ToString(System.Globalization.CultureInfo.InvariantCulture));

		return data;
	}

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

	/// <summary>
	/// Return a string representation of this class.
	/// </summary>
	/// <returns>A formatted json string representing this class.</returns>
	public override string ToString() {
		return ToJson().ToString();
	}
}

