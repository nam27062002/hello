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
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	public const string TYPE_CODE = "passive";
	public const int NUMERIC_TYPE_CODE = 0;

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	public HDPassiveEventData m_passiveEventData = null;
	public HDPassiveEventDefinition m_passiveEventDefinition = null;

	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Default constructor.
	/// </summary>
	public HDPassiveEventManager() {
        m_type = TYPE_CODE;
		m_numericType = NUMERIC_TYPE_CODE;

        m_data = new HDPassiveEventData();
        m_passiveEventData = m_data as HDPassiveEventData;
        m_passiveEventDefinition = m_passiveEventData.definition as HDPassiveEventDefinition;
	}

	/// <summary>
	/// Destructor
	/// </summary>
	~HDPassiveEventManager() {
        m_data = null;
        m_passiveEventData = null;
        m_passiveEventDefinition = null;
	}

    //------------------------------------------------------------------------//
    // PARENT OVERRIDES														  //
    //------------------------------------------------------------------------//

    override public void OnLiveDataResponse() {}

    /// <summary>
    /// Check for state changes based on timestamps.
    /// </summary>
    override public void UpdateStateFromTimers() {
		// Call parent
		base.UpdateStateFromTimers();

		// Passive events don't have rewards, skip the REWARD_AVAILABLE state
        if(m_data.m_state == HDLiveEventData.State.REWARD_AVAILABLE || m_data.m_state == HDLiveEventData.State.FINALIZED) {
			FinishEvent();
		}
	}

	override public void RequestRewards() {	}

	/// <summary>
	/// Mark the event as finished.
	/// </summary>
	override public void FinishEvent() {
        // In the case of passive events, we don't need to notify the server
        if (m_active) {
            Deactivate();
            m_data.m_state = HDLiveEventData.State.FINALIZED;
            Messenger.Broadcast<int, HDLiveDataManager.ComunicationErrorCodes>(MessengerEvents.LIVE_EVENT_FINISHED, data.m_eventId, HDLiveDataManager.ComunicationErrorCodes.NO_ERROR);
        }
	}
}