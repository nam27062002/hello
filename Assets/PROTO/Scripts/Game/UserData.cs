// UserData.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 02/04/2015.
// Copyright (c) 2015 Ubisoft. All rights reserved.

#region INCLUDES AND PREPROCESSOR --------------------------------------------------------------------------------------
using UnityEngine;
using UnityEngine.UI;
using System;
#endregion

#region CLASSES --------------------------------------------------------------------------------------------------------
/// <summary>
/// User progress and profile.
/// </summary>
public class UserData : MonoBehaviour {
	#region PROPERTIES -------------------------------------------------------------------------------------------------
	// Set default values in the inspector, use events to set them from code
	// [AOC] We want these to be consulted but never set from outside, so don't add a setter
	[SerializeField] private long mCoins;
	public long coins {
		get { return mCoins; }
	}

	[SerializeField] private long mPC;
	public long pc { 
		get { return mPC; }
	}
	#endregion
	
	#region GENERIC METHODS --------------------------------------------------------------------------------------------
	/// <summary>
	/// Use this for initialization.
	/// </summary>
	void Start() {
		// Nothing to do for now
	}
	
	/// <summary>
	/// Update is called once per frame.
	/// </summary>
	void Update() {
		// Nothing to do for now
	}

	/// <summary>
	/// Called right before destroying the object.
	/// </summary>
	void OnDestroy() {
		// Nothing to do for now
	}
	#endregion

	#region PUBLIC UTILS -----------------------------------------------------------------------------------------------
	/// <summary>
	/// Add coins.
	/// </summary>
	/// <param name="_iAmount">Amount to add. Negative to subtract.</param>
	public void AddCoins(long _iAmount) {
		// Skip checks for now
		// Compute new value and dispatch event
		mCoins += _iAmount;
		Messenger.Broadcast<long, long>(GameEvents_OLD.PROFILE_COINS_CHANGED, mCoins - _iAmount, mCoins);
	}

	/// <summary>
	/// Add PC.
	/// </summary>
	/// <param name="_iAmount">Amount to add. Negative to subtract.</param>
	public void AddPC(long _iAmount) {
		// Skip checks for now
		// Compute new value and dispatch event
		mPC += _iAmount;
		Messenger.Broadcast<long, long>(GameEvents_OLD.PROFILE_PC_CHANGED, mPC - _iAmount, mPC);
	}
	#endregion
}
#endregion