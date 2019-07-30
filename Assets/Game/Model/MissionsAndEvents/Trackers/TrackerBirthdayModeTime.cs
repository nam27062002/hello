// TrackerSurviveTime.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 21/03/2017.
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
/// Tracker for survival time.
/// </summary>
public class TrackerBirthdayModeTime : TrackerBaseTime {
    //------------------------------------------------------------------------//
    // GENERIC METHODS														  //
    //------------------------------------------------------------------------//
    public TrackerBirthdayModeTime() {
        Messenger.AddListener<bool, DragonSuperSize.Source>(MessengerEvents.SUPER_SIZE_TOGGLE, OnSuperSizeToggle);
    }


    //------------------------------------------------------------------------//
    // PARENT OVERRIDES														  //
    //------------------------------------------------------------------------//
    /// <summary>
    /// Finalizer method. Leave the tracker ready for garbage collection.
    /// </summary>
    override public void Clear() {
        // Unsubscribe from external events
        Messenger.RemoveListener<bool, DragonSuperSize.Source>(MessengerEvents.SUPER_SIZE_TOGGLE, OnSuperSizeToggle);

        // Call parent
        base.Clear();
    }

    //------------------------------------------------------------------------//
    // CALLBACKS															  //
    //------------------------------------------------------------------------//
    private void OnSuperSizeToggle(bool _activated, DragonSuperSize.Source _source) {
        if (_source == DragonSuperSize.Source.CAKE) {
            m_updateTime = _activated;
        }
    }
}