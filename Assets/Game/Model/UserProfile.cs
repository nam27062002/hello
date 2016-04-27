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

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Main user profile. Store here all the user-related data: currencies, stats, 
/// progress, purchases...
/// Singleton class, work with it via its static methods only.
/// <see cref="https://youtu.be/64uOVmQ5R1k?t=20m16s"/>
/// </summary>
public class UserProfile : SingletonMonoBehaviour<UserProfile> {
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
		public int superFuryProgression = 0;
		public long highScore = 0;
		/*[SkuList(Definitions.Category.LEVELS)]*/ public string currentLevel = "";	// sku	// [AOC] Attribute causes problems on the PersistenceProfile custom editor
		[EnumMask] public TutorialStep tutorialStep = TutorialStep.INIT;
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
	public static long coins {
		get { return instance.m_coins; }
	}
	
	[SerializeField] private long m_pc;
	public static long pc { 
		get { return instance.m_pc; }
	}

	[Separator("Game Settings")]
	[SerializeField] private string m_currentDragon = "";
	public static string currentDragon {
		get { return instance.m_currentDragon; }
		set { instance.m_currentDragon = value; }
	}

	[SerializeField] /*[SkuList(Definitions.Category.LEVELS)]*/ private string m_currentLevel = "";
	public static string currentLevel {
		get { return instance.m_currentLevel; }
		set { instance.m_currentLevel = value; }
	}

	[SerializeField] private int m_superFuryProgression = 0;
	public static int superFuryProgression {
		get { return instance.m_superFuryProgression; }
		set { instance.m_superFuryProgression = value; }
	}

	[SerializeField] private long m_highScore = 0;
	public static long highScore {
		get { return instance.m_highScore; }
		set { instance.m_highScore = value; }
	}

	[SerializeField] private TutorialStep m_tutorialStep;
	public static TutorialStep tutorialStep { 
		get { return instance.m_tutorialStep; }
		set { instance.m_tutorialStep = value; }
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
	public static void AddCoins(long _amount) {
		// Skip checks for now
		// Compute new value and dispatch event
		instance.m_coins += _amount;
		Messenger.Broadcast<long, long>(GameEvents.PROFILE_COINS_CHANGED, coins - _amount, coins);
	}
	
	/// <summary>
	/// Add PC.
	/// </summary>
	/// <param name="_iAmount">Amount to add. Negative to subtract.</param>
	public static void AddPC(long _iAmount) {
		// Skip checks for now
		// Compute new value and dispatch event
		instance.m_pc += _iAmount;
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
	public static bool IsTutorialStepCompleted(TutorialStep _step) {
		// Special case for NONE: ignore
		if(_step == TutorialStep.INIT) return true;

		return (instance.m_tutorialStep & _step) != 0;
	}

	/// <summary>
	/// Mark/unmark a tutorial step as completed.
	/// </summary>
	/// <param name="_step">The tutorial step to be marked. Can also be a composition of steps (e.g. (TutorialStep.STEP_1 | TutorialStep.STEP_2), in which case all steps will be marked).</param>
	/// <param name="_completed">Whether to mark it as completed or uncompleted.</param>
	public static void SetTutorialStepCompleted(TutorialStep _step, bool _completed = true) {
		// Special case for NONE: ignore
		if(_step == TutorialStep.INIT) return;

		if(_completed) {
			instance.m_tutorialStep |= _step;
		} else {
			instance.m_tutorialStep &= ~_step;
		}
	}

	//------------------------------------------------------------------------//
	// PERSISTENCE															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Load state from a persistence object.
	/// </summary>
	/// <param name="_data">The data object loaded from persistence.</param>
	public static void Load(SaveData _data) {
		// Just read values from persistence object
		instance.m_coins = _data.coins;
		instance.m_pc = _data.pc;
		instance.m_currentDragon = _data.currentDragon;
		instance.m_currentLevel = _data.currentLevel;
		instance.m_tutorialStep = _data.tutorialStep;
		instance.m_superFuryProgression = _data.superFuryProgression;
		instance.m_highScore = _data.highScore;
	}

	/// <summary>
	/// Create and return a persistence save data object initialized with the data.
	/// </summary>
	/// <returns>A new data object to be stored to persistence by the PersistenceManager.</returns>
	public static SaveData Save() {
		// Create new object
		SaveData data = new SaveData();

		// Initialize it
		data.coins = instance.m_coins;
		data.pc = instance.m_pc;
		data.currentDragon = instance.m_currentDragon;
		data.currentLevel = instance.m_currentLevel;
		data.tutorialStep = instance.m_tutorialStep;
		data.superFuryProgression = instance.m_superFuryProgression;
		data.highScore = instance.m_highScore;

		// Return it
		return data;
	}
}

