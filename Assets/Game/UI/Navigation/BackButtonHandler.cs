// BackButtonTrigger.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 21/08/2017.
// Copyright (c) 2017 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.Events;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Individual handler for the back button
/// </summary>
public abstract class BackButtonHandler : MonoBehaviour {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//


	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	private void OnDestroy() {
		if ( ApplicationManager.IsAlive )
			Unregister();
	}


	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Register to the manager.
	/// </summary>
	protected void Register() {
		BackButtonManager.Register(this);
	}

	/// <summary>
	/// Unregister from the manager.
	/// </summary>
	protected void Unregister() {
		if ( ApplicationManager.IsAlive )
			BackButtonManager.Unregister(this);
	}

	/// <summary>
	/// Perform the defined action on to this handler.
	/// To be called by the manager.
	/// </summary>
	public abstract void Trigger();


	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//

}