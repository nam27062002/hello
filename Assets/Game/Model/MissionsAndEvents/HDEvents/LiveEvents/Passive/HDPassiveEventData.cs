// HDPassiveEventData.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 03/07/2018.
// Copyright (c) 2018 Ubisoft. All rights reserved.

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
/// 
/// </summary>
[Serializable]
public class HDPassiveEventData : HDLiveEventData {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
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
	public HDPassiveEventData() {
        m_definition = new HDPassiveEventDefinition();
	}

    public override void UpdateStateFromTimers() {
        if (m_eventId > 0 && definition.m_eventId == m_eventId) {
            DateTime serverTime = GameServerManager.SharedInstance.GetEstimatedServerTime();
            if (serverTime < m_definition.m_startTimestamp) {
                m_state = State.TEASING;
            } else if (m_state < State.REWARD_AVAILABLE) {
                if (serverTime > m_definition.m_endTimestamp) {
                    m_state = State.FINALIZED;
                } else {
                    if (m_state == State.NONE || m_state == State.TEASING) {
                        m_state = State.JOINED;
                    }
                }
            }
        }
    }
}