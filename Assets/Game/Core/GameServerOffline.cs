// GameServerOffline.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 12/06/2017.
// Copyright (c) 2017 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;

using FGOL.Server;

using System;
using System.Collections.Generic;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Game server implementation for debugging server calls offline.
/// </summary>
public class GameServerOffline : GameServerManager {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	
	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Simple method to delay an action a few seconds (simulating a server response delay).
	/// </summary>
	/// <param name="_call">Action to be called.</param>
	/// <param name="_delay">Delay. If not defined, a random value from the range defined in DebugSettings will be used.</param>
	private void DelayedCall(Action _call, float _delay = -1f) {
		// If delay is not defined, get a random delay from the debug settings
		if(_delay < 0f) {
			_delay = DebugSettings.serverDelayRange.GetRandom();
		}

		// [AOC] Since this is a simple class and can't launch coroutines, use auxiliar manager to do so
		CoroutineManager.DelayedCall(_call, _delay);
	}

	//------------------------------------------------------------------------//
	// PARENT OVERRIDES														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// 
	/// </summary>
	/// <param name="_callback">Action called once the server response arrives.</param>
	override public void Ping(Action<Error> _callback) {
		// No response
		DelayedCall(() => _callback(null));
	}

	/// <summary>
	/// 
	/// </summary>
	/// <param name="_callback">Action called once the server response arrives.</param>
	override public void LogInToServerThruPlatform(string _platformId, string _platformUserId, string _platformToken, Action<Error, Dictionary<string, object>> _callback) {
		// Dummy login arguments
		Dictionary<string, object> res = new Dictionary<string, object>();
		res["fgolID"] = "debug_user_id";
		res["sessionToken"] = "debug_session_token";
		res["authState"] = Authenticator.AuthState.Authenticated.ToString();
		res["upgradeAvailable"] = false;
		res["cloudSaveAvailable"] = false;

		DelayedCall(() => _callback(null, res));
	}

	/// <summary>
	/// 
	/// </summary>
	/// <param name="_callback">Action called once the server response arrives.</param>
	override public void LogOut(Action<Error> _callback) {
		// No response
		DelayedCall(() => _callback(null));
	}

	/// <summary>
	/// 
	/// </summary>
	/// <param name="_callback">Action called once the server response arrives.</param>
	override public void GetServerTime(Action<Error, Dictionary<string, object>> _callback) {
		// Return UTC unix timestamp
		long time = (long)Globals.GetUnixTimestamp();	// Seconds

		Dictionary<string, object> res = new Dictionary<string, object>();
		res["t"] = time * 1000;	// Millis
		res["dateTime"] = time;
		res["unixTimestamp"] = time;

		DelayedCall(() => _callback(null, res));
	}

	/// <summary>
	/// 
	/// </summary>
	/// <param name="_callback">Action called once the server response arrives.</param>
	override public void GetPersistence(Action<Error, Dictionary<string, object>> _callback) {
		// Either load current local persistence of a fake json from disk
		Dictionary<string, object> res = new Dictionary<string, object>();
	#if false
		// Current local persistence
		res["response"] = PersistenceManager.LoadToObject().ToString();
	#else
		// Custom test persistence
		SimpleJSON.JSONClass persistenceData = PersistenceManager.LoadToObject("debug_offline_cloud_savegame");
		if(persistenceData == null) {
			// Test persistence not found, load default profile
			persistenceData = PersistenceManager.GetDefaultDataFromProfile();
		}
		res["response"] = persistenceData.ToString();
	#endif

		DelayedCall(() => _callback(null, res));
	}

	/// <summary>
	/// 
	/// </summary>
	/// <param name="_callback">Action called once the server response arrives.</param>
	override public void SetPersistence(string _persistence, Action<Error, Dictionary<string, object>> _callback) {
		// Doesn't need response
		DelayedCall(() => _callback(null, null));
	}

	/// <summary>
	/// 
	/// </summary>
	/// <param name="_callback">Action called once the server response arrives.</param>
	override public void UpdateSaveVersion(bool _prelimUpdate, Action<Error, Dictionary<string, object>> _callback) {
		long time = (long)Globals.GetUnixTimestamp();	// Seconds

		Dictionary<string, object> res = new Dictionary<string, object>();
		res["t"] = time * 1000;	// Millis
		res["dateTime"] = time;
		res["unixTimestamp"] = time;

		DelayedCall(() => _callback(null, res));
	}

	/// <summary>
	/// 
	/// </summary>
	/// <param name="_callback">Action called once the server response arrives.</param>
	override public void GetQualitySettings(Action<Error, Dictionary<string, object>> _callback) {
		Dictionary<string, object> res = new Dictionary<string, object>();

		res["response"] = "";
		res["upLoadRequired"] = false;

		DelayedCall(() => _callback(null, res));
	}

	/// <summary>
	/// 
	/// </summary>
	/// <param name="_callback">Action called once the server response arrives.</param>
	override public void SetQualitySettings(string _qualitySettings, Action<Error, Dictionary<string, object>> _callback) {
		// Doesn't need response
		DelayedCall(() => _callback(null, null));
	}

	//------------------------------------------------------------------------//
	// GLOBAL EVENTS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Get a list of all current global events.
	/// </summary>
	/// <param name="_callback">Callback action.</param>
	public virtual void GetGlobalEvents(Action<Error, Dictionary<string, object>> _callback) {

	}
}