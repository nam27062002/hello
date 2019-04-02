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
    new public const string TYPE_CODE = "dragonDiscount";
    new public const int NUMERIC_TYPE_CODE = 3;

    public HDDiscountEventManager() : base() {
        m_type = TYPE_CODE;
        m_numericType = NUMERIC_TYPE_CODE;
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
    }

    public override void Activate() {
        base.Activate();
        CheckEvent(null);
    }

    /// <summary>
    /// Mark the event as finished.
    /// </summary>
    public override void FinishEvent() {
        if (m_active) {
            if (HDLiveDataManager.TEST_CALLS) {
                ApplicationManager.instance.StartCoroutine(DelayedCall(m_type + "_finish.json", FinishEventResponse));
            } else {
                GameServerManager.SharedInstance.HDEvents_FinishMyEvent(data.m_eventId, FinishEventResponse);
            }
            base.FinishEvent();
        }
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
            if (discount != null) {
                IDragonData dragonData = DragonManager.GetDragonData(discount.dragonSku);
                if (dragonData.isOwned) {
                    FinishEvent(); // finish event and deactivate mods.
                }
            }
        }
    }
}