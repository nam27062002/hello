// PopupManager.cs
// Monster
// 
// Created by Alger Ortín Castellví on 17/06/2015.
// Copyright (c) 2015 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;
using System.Collections.Generic;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// Simple manager to load and open popups.
/// TODO:
/// - Keep an updated list of open popups
/// - Allow popup queues
/// </summary>
public class PopupManager : MonoBehaviour {
	//------------------------------------------------------------------//
	// CONSTANTS														//
	//------------------------------------------------------------------//

	//------------------------------------------------------------------//
	// MEMBERS															//
	//------------------------------------------------------------------//
	private Canvas canvas = null;

	//------------------------------------------------------------------//
	// SINGLETON INSTANCE												//
	//------------------------------------------------------------------//
	// [AOC] Unsafe version assuming the instance is already on the scene
	public static PopupManager instance = null;

	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	void Awake () {
		if(instance == null) {
			// Initialize singleton instance
			instance = this;

			// Make sure our object is not destroyed between scenes
			GameObject.DontDestroyOnLoad(gameObject);

			// Get canvas reference
			canvas = GetComponent<Canvas>();
		} else if(instance != this) {
			// Only one object of this type!!
			Destroy(gameObject);
		}
	}

	/// <summary>
	/// Initialization.
	/// </summary>
	void Start () {

	}
	
	/// <summary>
	/// Called every frame.
	/// </summary>
	void Update () {
	
	}

	/// <summary>
	/// Destructor
	/// </summary>
	void OnDestroy() {
	
	}

	//------------------------------------------------------------------//
	// PUBLIC UTILS														//
	//------------------------------------------------------------------//
	/// <summary>
	/// Loads a popup from the resources folder, instantiates it and opens it.
	/// </summary>
	/// <param name="_sResourcesPath">The path of the popup in the resources folder.</param>
	public GameObject OpenPopup(string _sResourcesPath) {
		// Load it from resources
		GameObject popupObj = Instantiate(Resources.Load<GameObject>(_sResourcesPath));

		// Instantiate it to the canvas
		popupObj.transform.SetParent(canvas.transform, false);

		// Open the popup
		popupObj.GetComponent<PopupController>().Open();

		// Return the newly created object
		return popupObj;
	}
}
