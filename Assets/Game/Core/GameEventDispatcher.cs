﻿// EventDispatcher.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 27/07/2016.
// Copyright (c) 2016 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Simple behaviour to be able to setup events broadcasting from inspector.
/// TODO!! Figure out a way to send parameters.
/// </summary>
public class GameEventDispatcher : MonoBehaviour {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	
	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed
	[Tooltip("Optional, to differientate between multiple dispatchers in the same GameObject")]
	[NumericRange(0)]
	[SerializeField] private int m_id = 0;

	// We must store the event as a string, since serializing the enum value would result in a wrong value if new values are added in the middle of the enum
	[Tooltip("For now, only events with no parameters are supported.")]
	[EnumList(typeof(MessengerEvents))]
	[SerializeField] private string m_eventTypeName = "";

	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Broadcast the event of the given type, no parameters.
	/// </summary>
	public void Dispatch() {
		// Figure out the enum value equivalent to the stored name
		MessengerEvents eventType = (MessengerEvents)System.Enum.Parse(typeof(MessengerEvents), m_eventTypeName, true);
		Messenger.Broadcast(eventType);
	}

	/// <summary>
	/// Broadcast the event of the given type, no parameters.
	/// Version with dispatcher's id check.
	/// </summary>
	/// <param name="_dispatcherId">The id of the dispatcher to perform the broadcast.</param>
	public void Dispatch(int _dispatcherId) {
		// If ID matches, broadcast event
		if(_dispatcherId == m_id) {
			Dispatch();
		} else {
			// Otherwise find a dispatcher in the same game object whose id matches
			GameEventDispatcher[] dispatchers = GetComponents<GameEventDispatcher>();
			for(int i = 0; i < dispatchers.Length; i++) {
				if(dispatchers[i].m_id == _dispatcherId) {
					dispatchers[i].Dispatch();
					return;
				}
			}
		}
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
}