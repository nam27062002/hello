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
public class HDDiscountEventManager : HDPassiveEventManager {

    public HDDiscountEventManager() : base() {
        m_type = "dragonDiscount";
        m_numericType = 3;
        Messenger.AddListener<IDragonData>(MessengerEvents.DRAGON_ACQUIRED, CheckEvent);
    }

    /// <summary>
    /// Destructor
    /// </summary>
    ~HDDiscountEventManager() {
        Messenger.RemoveListener<IDragonData>(MessengerEvents.DRAGON_ACQUIRED, CheckEvent);
    }

    public override void ParseDefinition(SimpleJSON.JSONNode _data) {
         base.ParseDefinition(_data);

        CheckEvent(null);
    }

    /// <summary>
    /// Mark the event as finished.
    /// </summary>
    public override void FinishEvent() {
        // Tell server
        if (HDLiveDataManager.TEST_CALLS) {
            ApplicationManager.instance.StartCoroutine(DelayedCall(m_type + "_finish.json", FinishEventResponse));
        } else {
            GameServerManager.SharedInstance.HDEvents_FinishMyEvent(data.m_eventId, FinishEventResponse);
        }
        base.FinishEvent();
	}

    protected override void FinishEventResponse(FGOL.Server.Error _error, GameServerManager.ServerResponse _response) {
        base.FinishEventResponse(_error, _response);
		if (!HDLiveDataManager.TEST_CALLS) {
			HDLiveDataManager.instance.ForceRequestMyEventType(m_numericType);
		}
    }

    private void CheckEvent(IDragonData _data) {
        if (EventExists() && m_data.m_state <= HDLiveEventData.State.REWARD_AVAILABLE) {
            ModEconomyDragonPrice discount = m_passiveEventDefinition.mainMod as ModEconomyDragonPrice;
            IDragonData dragonData = DragonManager.GetDragonData(discount.dragonSku);
            if (dragonData.isOwned) {
                FinishEvent(); // finish event and deactivate mods.
            } else {
                Activate(); // start event and activate mods.
            }
        }
    }
}