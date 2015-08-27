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
	override protected void Awake() {
		// Call parent
		base.Awake();

		// Create and initialize canvas
		if(canvas == null) {
			// Create container object at the root of the scene
			GameObject canvasObj = new GameObject("CanvasPopups");
			canvasObj.layer = LayerMask.NameToLayer("UI");
			GameObject.DontDestroyOnLoad(canvasObj);	// The popup manager is a singleton, persisting through scene changes, so should be the canvas

			// Create and setup canvas
			// Assume we want the popups to show on top of the rest of the UI, so we will setup the canvas accordingly
			canvas = canvasObj.AddComponent<Canvas>();
			canvas.renderMode = RenderMode.ScreenSpaceOverlay;
			canvas.sortingOrder = 100;	// Should be enough, default value is 0 and we don't usually have more than one canvas in the UI layer

			// Create and setup canvas scaler
			// Copied from default canvas usage, feel free to modify any of these parameters
			CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
			scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
			scaler.referenceResolution = new Vector2(800, 600);
			scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
			scaler.matchWidthOrHeight = 0;
			scaler.referencePixelsPerUnit = 100;

			// Create and setup raycaster (required for the canvas to work properly)
			// Copied from default canvas usage, feel free to modify any of these parameters
			GraphicRaycaster raycaster = canvasObj.AddComponent<GraphicRaycaster>();
			raycaster.ignoreReversedGraphics = true;
		}
	}

	/// <summary>
	/// First update call.
	/// </summary>
	override protected void Start() {
		// Call parent
		base.Start();
	}
	
	/// <summary>
	/// Called every frame.
	/// </summary>
	override protected void Update() {
		// Call parent
		base.Update();
	}

	/// <summary>
	/// Destructor
	/// </summary>
	override protected void OnDestroy() {
		// Destroy created canvas as well
		if(canvas != null) {
			Destroy(canvas.gameObject);
		}

		// Clear references
		canvas = null;

		// Call parent
		base.OnDestroy();
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
		DebugUtils.Assert(popupObj != null, "Popup " + _sResourcesPath + " could not be loaded");

		// Instantiate it to the canvas
		popupObj.transform.SetParent(instance.canvas.transform, false);

		// Open the popup
		PopupController controller = popupObj.GetComponent<PopupController>();
		if(controller != null) controller.Open();

		// Return the newly created object
		return popupObj;
	}
}
