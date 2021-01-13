// TrackerBoostTime.cs
// Hungry Dragon
// 
// Created by Miguel Ángel Linares on 25/10/2017.
// Copyright (c) 2017 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Tracker for boosting time.
/// </summary>
public class TrackerBoostTime : TrackerBaseTime {
	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Default constructor.
	/// </summary>
	public TrackerBoostTime() {
		// Subscribe to external events
		Broadcaster.AddListener(BroadcastEventType.BOOST_TOGGLED, this);
	}

	/// <summary>
	/// Destructor
	/// </summary>
	~TrackerBoostTime() {
		
	}

	//------------------------------------------------------------------------//
	// PARENT OVERRIDES														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Finalizer method. Leave the tracker ready for garbage collection.
	/// </summary>
	override public void Clear() {
		// Unsubscribe from external events
		Broadcaster.RemoveListener(BroadcastEventType.BOOST_TOGGLED, this);

		// Call parent
		base.Clear();
	}


	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
    
    override public void OnBroadcastSignal(BroadcastEventType eventType, BroadcastEventInfo broadcastEventInfo)
    {
        switch(eventType)
        {
            case BroadcastEventType.BOOST_TOGGLED:
            {
                ToggleParam toggleParam = (ToggleParam)broadcastEventInfo;
                OnBoostToggled(toggleParam.value); 
            }break;
        }
    }
    
	/// <summary>
	/// The dragon has entered/exit water.
	/// </summary>
	/// <param name="_activated">Whether the dragon has entered or exited the water.</param>
	private void OnBoostToggled(bool _activated) {
		// Update internal flag
		m_updateTime = _activated;
	}
}