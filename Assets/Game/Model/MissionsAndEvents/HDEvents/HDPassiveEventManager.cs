// HDPassiveEventManager.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 28/06/2018.
// Copyright (c) 2018 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using System;
using System.Collections.Generic;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Specialization of a Live Event for Passive Events
/// </summary>
[Serializable]
public class HDPassiveEventManager : HDLiveEventManager {
	//------------------------------------------------------------------------//
	// CASSES																  //
	//------------------------------------------------------------------------//

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//

	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Default constructor.
	/// </summary>
	public HDPassiveEventManager() {
		
	}

	/// <summary>
	/// Destructor
	/// </summary>
	~HDPassiveEventManager() {
		
	}

	//------------------------------------------------------------------------//
	// PARENT OVERRIDES														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Check for state changes based on timestamps.
	/// </summary>
	override public void UpdateStateFromTimers() {
		// Call parent
		base.UpdateStateFromTimers();

		// Passive events don't have rewards, skip the REWARD_AVAILABLE state
		if(m_data.m_state == HDLiveEventData.State.REWARD_AVAILABLE) {
			FinishEvent();
		}
	}

	/// <summary>
	/// Mark the event as finished.
	/// </summary>
	override public void FinishEvent() {
		// In the case of passive events, we don't need to notify the server
		Deactivate();
		m_data.m_state = HDLiveEventData.State.FINALIZED;
		Messenger.Broadcast<int,HDLiveEventsManager.ComunicationErrorCodes>(MessengerEvents.LIVE_EVENT_FINISHED, data.m_eventId, HDLiveEventsManager.ComunicationErrorCodes.NO_ERROR);
	}
}