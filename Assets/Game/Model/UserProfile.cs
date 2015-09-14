﻿// UserData.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 24/08/2015.
// Copyright (c) 2015 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;
using System;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// Main user profile. Store here all the user-related data: currencies, stats, 
/// progress, purchases...
/// Singleton class, work with it via its static methods only.
/// <see cref="https://youtu.be/64uOVmQ5R1k?t=20m16s"/>
/// </summary>
public class UserProfile : Singleton<UserProfile> {
	//------------------------------------------------------------------//
	// CONSTANTS														//
	//------------------------------------------------------------------//
	/// <summary>
	/// Auxiliar serializable class to save/load to persistence.
	/// </summary>
	[Serializable]
	public class SaveData {
		// Add here any required data
		public long coins;
		public long pc;
		public DragonID currentDragon;
	}

	//------------------------------------------------------------------//
	// MEMBERS															//
	//------------------------------------------------------------------//

	//------------------------------------------------------------------//
	// PROPERTIES														//
	//------------------------------------------------------------------//
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

	[Separator("Progression")]
	[SerializeField] private DragonID m_currentDragon = DragonID.SMALL;	// [AOC] TODO!! Proper default initialization
	public static DragonID currentDragon {
		get { return instance.m_currentDragon; }
		set { instance.m_currentDragon = value; }
	}

	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	override protected void Awake() {
		// Call parent
		base.Awake();
	}

	/// <summary>
	/// Destructor.
	/// </summary>
	override protected void OnDestroy() {
		// Call parent
		base.OnDestroy();
	}

	//------------------------------------------------------------------//
	// PUBLIC METHODS													//
	//------------------------------------------------------------------//
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

	//------------------------------------------------------------------//
	// PERSISTENCE														//
	//------------------------------------------------------------------//
	/// <summary>
	/// Load state from a persistence object.
	/// </summary>
	/// <param name="_data">The data object loaded from persistence.</param>
	public static void Load(SaveData _data) {
		// Just read values from persistence object
		instance.m_coins = _data.coins;
		instance.m_pc = _data.pc;
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

		// Return it
		return data;
	}
}

