// TrackerMapUsage.cs
// Hungry Dragon
// 
// Created by Miguel Ángel Linares on 25/10/2017.
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
/// Tracker for map usage.
/// </summary>
public class TrackerMapUsage : TrackerBase, IBroadcastListener {
	//------------------------------------------------------------------------//
	// MEMBERS																  //
	//------------------------------------------------------------------------//

	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Default constructor.
	/// </summary>
	public TrackerMapUsage() {
		Broadcaster.AddListener(BroadcastEventType.POPUP_OPENED, this);
	}

	/// <summary>
	/// Destructor
	/// </summary>
	~TrackerMapUsage() {
		
	}

	/// <summary>
	/// Finalizer method. Leave the tracker ready for garbage collection.
	/// </summary>
	override public void Clear() {
		// Unsubscribe from external events
		Broadcaster.RemoveListener(BroadcastEventType.POPUP_OPENED, this);

		// Call parent
		base.Clear();
	}
    
    public void OnBroadcastSignal(BroadcastEventType eventType, BroadcastEventInfo broadcastEventInfo)
    {
        switch(eventType)
        {
            case BroadcastEventType.POPUP_OPENED:
            {
                PopupManagementInfo info = (PopupManagementInfo)broadcastEventInfo;
                OnPopupOpened(info.popupController);
            }break;
        }
    }
    

	private void OnPopupOpened(PopupController _popup) {
		if ( _popup.GetComponent<PopupInGameMap>() != null ){
			currentValue++;
		}
	}

}