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
public class TrackerCollectibleHungryModeTime : TrackerBaseTime {
    //------------------------------------------------------------------------//
    // GENERIC METHODS														  //
    //------------------------------------------------------------------------//
    public TrackerCollectibleHungryModeTime() {
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

	/// <summary>
	/// Localizes and formats a value according to this tracker's type
	/// (i.e. "52", "500 meters", "10 minutes").
	/// </summary>
	/// <returns>The localized and formatted value for this tracker's type.</returns>
	/// <param name="_value">Value to be formatted.</param>
	override public string FormatValue(long _value) {
		// Format value as time
		// [AOC] Digits formatting looks cooler in a leaderboard MM:SS
		return TimeUtils.FormatTime(_value, TimeUtils.EFormat.DIGITS, 2, TimeUtils.EPrecision.MINUTES, true);
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
	private void OnSuperSizeToggle(bool _activated, DragonSuperSize.Source _source) {
        if (_source == DragonSuperSize.Source.COLLECTIBLE) {
            m_updateTime = _activated;
        }
    }
}