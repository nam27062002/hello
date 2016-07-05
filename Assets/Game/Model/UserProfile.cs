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

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Main user profile. Store here all the user-related data: currencies, stats, 
/// progress, purchases...
/// Singleton class, work with it via its static methods only.
/// <see cref="https://youtu.be/64uOVmQ5R1k?t=20m16s"/>
/// </summary>
public class UserProfile
{
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Auxiliar serializable class to save/load to persistence.
	/// </summary>
	[Serializable]
	public class SaveData {
		// Add here any required data
		public long coins = 0;
		public long pc = 0;
		public string currentDragon = "";	// sku
		/*[SkuList(Definitions.Category.LEVELS)]*/ public string currentLevel = "";	// sku	// [AOC] Attribute causes problems on the PersistenceProfile custom editor
		[EnumMask] public TutorialStep tutorialStep = TutorialStep.INIT;

		public bool furyUsed = false;

		// Game stats
		public int gamesPlayed = 0;
		public long highScore = 0;
		public int superFuryProgression = 0;
	}

	//------------------------------------------------------------------------//
	// MEMBERS																  //
	//------------------------------------------------------------------------//

	//------------------------------------------------------------------------//
	// PROPERTIES															  //
	//------------------------------------------------------------------------//
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
		set { m_currentDragon = value; }
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

	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//

	//------------------------------------------------------------------------//
	// PUBLIC STATIC METHODS												  //
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
	/// <summary>
	/// Load state from a json object.
	/// </summary>
	/// <param name="_data">The data object loaded from persistence.</param>
	public void Load(SimpleJSON.JSONNode _data) {
		// Just read values from persistence object
		// Economy
		m_coins = _data["sc"].AsInt;
		m_pc = _data["pc"].AsInt;

		// Game settings
		m_currentDragon = _data["currentDragon"];
		m_currentLevel = _data["currentLevel"];
		m_tutorialStep = ( TutorialStep )_data["tutorialStep"].AsInt;
		m_furyUsed = _data["furyUsed"].AsBool;

		// Game stats
		m_gamesPlayed = _data["gamesPlayed"].AsInt;
		m_highScore = _data["highScore"].AsInt;
		m_superFuryProgression = _data["superFuryProgression"].AsInt;

		// Some cheats override profile settings - will be saved with the next Save()
		if(Prefs.GetBool("skipTutorialCheat")) {
			m_tutorialStep = TutorialStep.ALL;
			Prefs.SetBool("skipTutorialCheat", false);
		}
	}

	/// <summary>
	/// Create and return a persistence save data object initialized with the data.
	/// </summary>
	/// <returns>A new data object to be stored to persistence by the PersistenceManager.</returns>
	public SimpleJSON.JSONClass Save() {
		// Create new object
		SimpleJSON.JSONClass data = new SimpleJSON.JSONClass();

		// Initialize it
		// Economy
		data.Add( "sc", m_coins.ToString());
		data.Add( "pc", m_pc.ToString());

		// Game settings
		data.Add("currentDragon",m_currentDragon);
		data.Add("currentLevel",m_currentLevel);
		data.Add("tutorialStep",((int)m_tutorialStep).ToString());
		data.Add("furyUsed", m_furyUsed.ToString());

		// Game stats
		data.Add("gamesPlayed",m_gamesPlayed.ToString());
		data.Add("highScore",m_highScore.ToString());
		data.Add("superFuryProgression",m_superFuryProgression.ToString());

		// Return it
		return data;
	}
}

