// InputLocker.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 19/04/2016.
// Copyright (c) 2016 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Auxiliar static class to lock/unlock all input events.
/// Based on http://trusteddevelopments.com/2014/10/11/how-to-disable-touches-mouse-clicks-in-ugui/
/// </summary>
public class InputLocker {
	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Keep a list of pointer input modules that we have disabled so that we can re-enable them
	private static List<PointerInputModule> s_disabledModules = new List<PointerInputModule>();

	// Is the input locked?
	private static bool s_locked = false;
	public static bool locked {
		get { return s_locked; }
		set { InternalSetLock(value); }
	}
	
	//------------------------------------------------------------------------//
	// PUBLIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Lock all input events.
	/// </summary>
	public static void Lock() {
		// Use property
		locked = true;
	}

	/// <summary>
	/// Unlock previously locked input events.
	/// </summary>
	public static void Unlock() {
		// Use property
		locked = false;
	}

	/// <summary>
	/// Toggle state.
	/// </summary>
	public static void Toggle() {
		// Use property
		locked = !locked;
	}

	//------------------------------------------------------------------------//
	// INTERNAL METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Performs all the logic to lock/unlock the Input.
	/// </summary>
	/// <param name="_lock">Whether to lock or unlock the Input events.</param>
	static void InternalSetLock(bool _lock) {
		// Store new lock state
		s_locked = _lock;

		// First re-enable all systems
		for(int i = 0; i < s_disabledModules.Count; i++) {
			if(s_disabledModules[i] != null) {
				s_disabledModules[i].enabled = true;
			}
		}
		s_disabledModules.Clear();

		// Get current event system
		EventSystem es = EventSystem.current;
		if(es == null) return;

		// Prevent navigation events (Unity makes it easy for us)
		es.sendNavigationEvents = !_lock;

		// If locking, disable all input modules in the current event system
		if(_lock) {
			// Find all PointerInputModules and disable them
			PointerInputModule[] pointerInput = es.GetComponents<PointerInputModule>();
			if(pointerInput != null) {
				for(int i = 0; i < pointerInput.Length; i++) {
					PointerInputModule pim = pointerInput[i];
					if(pim.enabled) {
						pim.enabled = false;
						s_disabledModules.Add(pim);	// Keep a list of disabled ones
					}
				}
			}

			// Cause EventSystem to update its list of modules
			es.enabled = false;
			es.enabled = true;
		}
	}
}