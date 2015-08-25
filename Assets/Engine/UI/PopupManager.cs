// PopupManager.cs
// Monster
// 
// Created by Alger Ortín Castellví on 17/06/2015.
// Copyright (c) 2015 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// Simple manager to load and open popups.
/// TODO:
/// - Keep an updated list of open popups
/// - Allow popup queues
/// - Optional delay before opening a popup
/// - Stacked popups (popup over popup)
/// </summary>
public class PopupManager : Singleton<PopupManager> {
	//------------------------------------------------------------------//
	// CONSTANTS														//
	//------------------------------------------------------------------//

	//------------------------------------------------------------------//
	// MEMBERS															//
	//------------------------------------------------------------------//
	// Use our own canvas for practicity.
	private Canvas canvas = null;

	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	void Awake() {
		// Create and initialize canvas
		if(canvas == null) {
			canvas = gameObject.AddComponent<Canvas>();

			// Setup scaling technique
			CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();
			if(scaler != null) {
				// Copied from default canvas usage, feel free to modify any of these parameters
				scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
				scaler.referenceResolution = new Vector2(800, 600);
				scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
				scaler.matchWidthOrHeight = 0;
				scaler.referencePixelsPerUnit = 100;
			}

			// Assume we want the popups to show on top of the rest of the UI, so we will setup the canvas accordingly
			gameObject.layer = LayerMask.NameToLayer("UI");
			canvas.sortingOrder = 100;	// Should be enough, default value is 0 and we don't usually have more than one canvas in the UI layer
		}
	}

	/// <summary>
	/// First update call.
	/// </summary>
	void Start() {

	}
	
	/// <summary>
	/// Called every frame.
	/// </summary>
	void Update() {
	
	}

	/// <summary>
	/// Destructor
	/// </summary>
	void OnDestroy() {
	
	}

	//------------------------------------------------------------------//
	// SINGLETON STATIC METHODS											//
	//------------------------------------------------------------------//
	/// <summary>
	/// Loads a popup from the resources folder, instantiates it and opens it.
	/// </summary>
	/// <param name="_sResourcesPath">The path of the popup in the resources folder.</param>
	public static GameObject OpenPopup(string _sResourcesPath) {
		// Load it from resources
		GameObject popupObj = Instantiate(Resources.Load<GameObject>(_sResourcesPath));

		// Instantiate it to the canvas
		popupObj.transform.SetParent(instance.canvas.transform, false);

		// Open the popup
		popupObj.GetComponent<PopupController>().Open();

		// Return the newly created object
		return popupObj;
	}
}
