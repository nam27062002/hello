// TrackerDistance.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 19/06/2017.
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
/// Tracker for traveling distance.
/// </summary>
public class TrackerDistance : TrackerBaseDistance {
	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Default constructor.
	/// </summary>
	public TrackerDistance() {
	}

	/// <summary>
	/// Destructor
	/// </summary>
	~TrackerDistance() {
		
	}



	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// A new game has started.
	/// </summary>
	protected override void OnGameStarted() {
		base.OnGameStarted();

		m_updateDistance = true;
	}
}