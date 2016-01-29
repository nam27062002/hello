// MenuDragonScroller.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 29/01/2016.
// Copyright (c) 2016 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// Show the currently selected dragon in the menu screen.
/// </summary>
[RequireComponent(typeof(SnappingScrollRect))]
public class MenuDragonScroller : MonoBehaviour {
	//------------------------------------------------------------------//
	// CONSTANTS														//
	//------------------------------------------------------------------//

	//------------------------------------------------------------------//
	// MEMBERS															//
	//------------------------------------------------------------------//

	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {
		
	}

	/// <summary>
	/// First update
	/// </summary>
	private void Start() {
		// Find game object linked to currently selected dragon
		string currentSelectedDragon = InstanceManager.GetSceneController<MenuSceneController>().selectedDragon;
		MenuDragonPreview[] dragonPreviews = GetComponentsInChildren<MenuDragonPreview>();
		for(int i = 0; i < dragonPreviews.Length; i++) {
			if(dragonPreviews[i].sku == currentSelectedDragon) {
				// Initialize scroll list with the currently selected dragon
				GetComponent<SnappingScrollRect>().SelectPoint(dragonPreviews[i].GetComponentInParent<ScrollRectSnapPoint>());
			}
		}
	}
}

