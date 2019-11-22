// BackButtonManager.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 21/08/2017.
// Copyright (c) 2017 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using InControl;
using System.Collections.Generic;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Global manager to control back button handlers.
/// </summary>
public class BackButtonManager : Singleton<BackButtonManager> {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	
	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Registered handlers
	// [AOC] Test if a Stack works fine or is too restrictive
	private List<BackButtonHandler> m_handlers = new List<BackButtonHandler>();

	
	//------------------------------------------------------------------------//
	// STATIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Register to the manager, on top of the stack.
	/// </summary>
	public static void Register(BackButtonHandler _handler) {
		instance.__Register(_handler);
	}

	/// <summary>
	/// Unregister from the manager, regardless of the position it's stacked.
	/// </summary>
	public static void Unregister(BackButtonHandler _handler) {
		instance.__Unregister(_handler);
	}

	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Register to the manager, on top of the stack.
	/// </summary>
	private void __Register(BackButtonHandler _handler) {
		m_handlers.Add(_handler);
	}

	/// <summary>
	/// Unregister from the manager, regardless of the position it's stacked.
	/// </summary>
	private void __Unregister(BackButtonHandler _handler) {
		m_handlers.Remove(_handler);
	}

    /// <summary>
    /// Called every frame.
    /// </summary>
    public void Update() {
        // Back button pressed?
        InputDevice device = InputManager.ActiveDevice;

        if (((device != null && device.Action2.WasReleased) || Input.GetKeyDown(KeyCode.Escape)) 
        && FeatureSettingsManager.IsBackButtonEnabled()) {	// On Android Escape is the same as Back Button
			if (!InputLocker.locked) {
				if (m_handlers.Count > 0) {
					m_handlers.Last().Trigger();
					//TODO: Should we add a delay before next back event?
				}
			}
		}
    }

    //------------------------------------------------------------------------//
    // CALLBACKS															  //
    //------------------------------------------------------------------------//
}