// CustomInputModule.cs
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
/// Specialization of Unity's input module to do some custom stuff.
/// </summary>
public class CustomInputModule : StandaloneInputModule {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	
	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Expose last pointer event data
	// https://forum.unity3d.com/threads/eventsystem-raycastall-alternative-workaround.372234/
	public PointerEventData lastPointerEventData {
		get { return GetLastPointerEventData(-1); }
	}

	// Expose last raycast results list
	public List<RaycastResult> lastRaycastResults {
		get { return m_RaycastResultCache; }
	}

	//------------------------------------------------------------------------//
	// METHODS																  //
	//------------------------------------------------------------------------//

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
}