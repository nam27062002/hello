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
public class GlobalEventManager : Singleton<GlobalEventManager> {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	
	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Current user data
	private UserProfile m_user = null;
	public static UserProfile user {
		get { return instance.m_user; }
	}

	// Current event
	private GlobalEvent m_currentEvent = null;
	public static GlobalEvent currentEvent {
		get { return instance.m_currentEvent; }
	}

	// Past events
	private List<GlobalEvent> m_pastEvents = new List<GlobalEvent>();	// Sorted from older to more recent
	public static List<GlobalEvent> pastEvents {
		get { return instance.m_pastEvents; }
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
	/// Can the user contribute to the current event?
	/// Several conditions will be checked, including internet connectivity, proper setup, current event state, etc.
	/// </summary>
	/// <returns><c>true</c> if the current user can contribute to the current event; <c>false</c> otherwise.</returns>
	public static bool CanContribute() {
		// Debugging override
		if(CPGlobalEventsTest.testEnabled) return true;

		// We must be online!
		if(Application.internetReachability == NetworkReachability.NotReachable) return false;

		// Manager must be properly setup
		if(!IsReady()) return false;

		// User must be logged in FB!
		//if(!FacebookManager.SharedInstance.IsLoggedIn()) return false;	// [AOC] CHECK!

		// We must have a valid event
		if(currentEvent == null) return false;

		// Event must be active!
		if(currentEvent.state != GlobalEvent.State.ACTIVE) return false;

		// All checks passed!
		return true;
	}

	/// <summary>
	/// Tells the manager with which user data he should work.
	/// </summary>
	/// <param name="_user">Profile to work with.</param>
	public static void SetupUser(UserProfile _user) {
		instance.m_user = _user;
	}

	/// <summary>
	/// Has a user been loaded into the manager?
	/// </summary>
	public static bool IsReady() {
		return instance.m_user != null;
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
}