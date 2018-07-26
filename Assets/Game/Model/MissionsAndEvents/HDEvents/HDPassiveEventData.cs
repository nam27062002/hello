// HDPassiveEventData.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 03/07/2018.
// Copyright (c) 2018 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using System;
using SimpleJSON;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// 
/// </summary>
[Serializable]
public class HDPassiveEventData : HDLiveEventData {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	
	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//

	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Default constructor.
	/// </summary>
	public HDPassiveEventData() {

	}

	/// <summary>
	/// Destructor
	/// </summary>
	~HDPassiveEventData() {

	}

	/// <summary>
	/// Create the definition object for this live event data.
	/// </summary>
	protected override void BuildDefinition() {
		m_definition = new HDPassiveEventDefinition();
	}
}