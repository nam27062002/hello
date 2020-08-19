// UI3DLoader.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 07/03/2017.
// Copyright (c) 2017 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Simple script to load 3D prefabs into a UI canvas.
/// </summary>
public class UI3DAddressablesLoader : HDAddressablesLoader {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//

	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//

	//------------------------------------------------------------------------//
	// PARENT OVERRIDES														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// To be overwritten by heirs to do further processing on a newly loaded instance.
	/// </summary>
	protected override void ProcessNewInstance() {
		// Process billboards
		LookAtMainCamera[] lookAts = m_loadedInstance.GetComponentsInChildren<LookAtMainCamera>();
		if(lookAts.Length > 0) {
			// Apply parent canvas' render camera
			Canvas parentCanvas = GetComponentInParent<Canvas>();
			if(parentCanvas != null) {
				Camera uiCamera = parentCanvas.rootCanvas.worldCamera;
				if(uiCamera != null) {
					for(int i = 0; i < lookAts.Length; ++i) {
						lookAts[i].overrideCamera = uiCamera;
					}
				}
			}
		}
	}
}