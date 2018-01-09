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
public class TrackerMapUsage : TrackerBase {
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
		Messenger.AddListener<PopupController>(MessengerEvents.POPUP_OPENED, OnPopupOpened);
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
		Messenger.RemoveListener<PopupController>(MessengerEvents.POPUP_OPENED, OnPopupOpened);

		// Call parent
		base.Clear();
	}

	private void OnPopupOpened(PopupController _popup) {
		if ( _popup.GetComponent<PopupInGameMap>() != null ){
			currentValue++;
		}
	}

}