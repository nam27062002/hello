// CustomEventSystem.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 16/03/2017.
// Copyright (c) 2017 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Specialization of Unity's EventSystem to do some custom stuff.
/// </summary>
public class CustomEventSystem : EventSystem {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	
	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Cache raycast results every time a RaycastAll is triggered
	private List<RaycastResult> m_lastRaycastResults = new List<RaycastResult>();
	public List<RaycastResult> lastRaycastResults {
		get { return m_lastRaycastResults; }
	}

	//------------------------------------------------------------------------//
	// METHODS																  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Override RayCastAll method to cache last raycast results.
	/// </summary>
	/// <param name="_eventData">Current pointer event data.</param>
	/// <param name="_raycastResults">List of "hits" to populate.</param>
	new public void RaycastAll(PointerEventData _eventData, List<RaycastResult> _raycastResults) {
		Debug.Log("Raycasting all from " + _eventData.currentInputModule.name);

		// Do default Unity's stuff
		base.RaycastAll(_eventData, _raycastResults);

		// Store results for future access
		m_lastRaycastResults.Clear();
		m_lastRaycastResults.AddRange(_raycastResults);
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
}