// IHUDWidget.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 13/08/2019.
// Copyright (c) 2019 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Interface for in-game HUD widgets requiring an update.
/// </summary>
public abstract class IHUDWidget : MonoBehaviour {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Seconds, how often the widget is updated.
	/// </summary>
	public abstract float UPDATE_INTERVAL {
		get;
	}

	//------------------------------------------------------------------------//
	// ABSTRACT METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Called at regular intervals.
	/// </summary>
	public abstract void PeriodicUpdate();

	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Component has been enabled.
	/// </summary>
	protected virtual void OnEnable() {
		// Self-register to the manager
		InstanceManager.hudManager.AddWidget(this);
	}

	/// <summary>
	/// Component has been disabled.
	/// </summary>
	protected virtual void OnDisable() {
		// Self-unregister to the manager
		InstanceManager.hudManager.RemoveWidget(this);
	}
}