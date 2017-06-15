// GlobalEventManager.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 20/03/2017.
// Copyright (c) 2017 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using System.Collections.Generic;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Global singleton manager for global events.
/// </summary>
public class GlobalEventManager : UbiBCN.SingletonMonoBehaviour<GlobalEventManager> {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	// Error codes for event interaction
	public enum ErrorCode {
		NONE = 0,

		OFFLINE,
		NOT_INITIALIZED,
		NOT_LOGGED_IN,
		NO_VALID_EVENT,
		EVENT_NOT_ACTIVE
	}

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Current user data
	private UserProfile m_user = null;
	public static UserProfile user {
		get { return instance.m_user; }
	}

	// Current event
	// Will always contain the data to the last relevant event to the user
	// That could be an event whose rewards are pending, a future event or the active event
	// Can also be null if there is no event currently, or if event data could not be retrieved from the server
	private GlobalEvent m_currentEvent = null;
	public static GlobalEvent currentEvent {
		get { return instance.m_currentEvent; }
	}
	
	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {
		
	}

	/// <summary>
	/// First update call.
	/// </summary>
	private void Start() {

	}

	/// <summary>
	/// Component has been enabled.
	/// </summary>
	private void OnEnable() {

	}

	/// <summary>
	/// Component has been disabled.
	/// </summary>
	private void OnDisable() {

	}

	/// <summary>
	/// Called every frame
	/// </summary>
	private void Update() {

	}

	/// <summary>
	/// Destructor.
	/// </summary>
	private void OnDestroy() {

	}

	//------------------------------------------------------------------------//
	// SINGLETON METHODS													  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Contribute to current event!
	/// Contribution amount will be taken from current event tracker.
	/// </summary>
	/// <returns>Whether the contribution has been applied or not and why.</returns>
	/// <param name="_bonusDragonMultiplier">Apply bonus dragon multiplier?</param>
	/// <param name="_keysMultiplier">Apply keys multiplier? Transaction must have been performed already.</param>
	public static ErrorCode Contribute(float _bonusDragonMultiplier, float _keysMultiplier) {
		// Can the user contribute to the current event?
		ErrorCode err = CanContribute();
		if(err != ErrorCode.NONE) return err;

		// Get contribution amount and apply multipliers
		float contribution = currentEvent.objective.tracker.currentValue;
		contribution *= _bonusDragonMultiplier;
		contribution *= _keysMultiplier;

		// Add to event's total contribution!
		currentEvent.AddContribution(contribution);

		// Add to current user individual contribution in this event
		user.GetGlobalEventData(currentEvent.id).score += contribution;

		// [AOC] TODO!! Update leaderboard?

		// Everything went well!
		return ErrorCode.NONE;
	}

	/// <summary>
	/// Can the user contribute to the current event?
	/// Several conditions will be checked, including internet connectivity, proper setup, current event state, etc.
	/// </summary>
	/// <returns>Whether the user can contribute to the current event and why.</returns>
	public static ErrorCode CanContribute() {
		// Debugging override
		bool testing = CPGlobalEventsTest.testEnabled;

		// We must be online!
		if(!testing && Application.internetReachability == NetworkReachability.NotReachable) return ErrorCode.OFFLINE;

		// Manager must be properly setup
		if(!IsReady()) return ErrorCode.NOT_INITIALIZED;

		// User must be logged in FB!
		if(!testing && !SocialManager.Instance.IsLoggedIn(SocialFacade.Network.Default)) return ErrorCode.NOT_LOGGED_IN;	// [AOC] CHECK!

		// We must have a valid event
		if(currentEvent == null) return ErrorCode.NO_VALID_EVENT;

		// Event must be active!
		if(!currentEvent.isActive) return ErrorCode.EVENT_NOT_ACTIVE;

		// All checks passed!
		return ErrorCode.NONE;
	}

	/// <summary>
	/// Tells the manager with which user data he should work.
	/// </summary>
	/// <param name="_user">Profile to work with.</param>
	public static void SetupUser(UserProfile _user) {
		// If it's a new user, request current event to the server for this user
		if(instance.m_user != _user) {
			instance.m_user = _user;
			GameServerManager.SharedInstance.GlobalEvent_GetCurrent(instance.OnCurrentEventResponse);
		}
	}

	/// <summary>
	/// Has a user been loaded into the manager?
	/// </summary>
	public static bool IsReady() {
		return instance.m_user != null;
	}

	//------------------------------------------------------------------------//
	// SERVER COMMUNICATION METHODS											  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Server callback for the current event request.
	/// </summary>
	/// <param name="_error">Error.</param>
	/// <param name="_response">Response.</param>
	private void OnCurrentEventResponse(FGOL.Server.Error _error, GameServerManager.ServerResponse _response) {
		// Error?
		if(_error == null) {
			// Did the server gave us an event?
			if(_response != null && _response["response"] != null) {
				// Yes! If no event is created, create one
				if(m_currentEvent == null) {
					m_currentEvent = new GlobalEvent();
				}

				// If the ID is different from the stored event, load the new event's data!
				SimpleJSON.JSONClass responseJson = _response["response"] as SimpleJSON.JSONClass;
				if(m_currentEvent.id != responseJson["id"]) {
					m_currentEvent.FromJson(responseJson);
				}

				// Get current event's state
				// [AOC] TODO!!
			} else {
				// No! Clear current event (if any)
				if(m_currentEvent != null) m_currentEvent = null;
			}
		} else {
			// [AOC] Do something or just ignore?
			// Probably store somewhere that there was an error so retry timer is reset or smth
		}
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
}