// UIPrefabLoader.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 31/03/2015.
// Copyright (c) 2015 Ubisoft. All rights reserved.

#region INCLUDES AND PREPROCESSOR --------------------------------------------------------------------------------------
using UnityEngine;
using UnityEngine.UI;
using System;
#endregion

#region CLASSES --------------------------------------------------------------------------------------------------------
/// <summary>
/// Behaviour to allow a canvas to load prefabs in it.
/// </summary>
[RequireComponent (typeof(Canvas))]	// This behaviour requires a UI canvas
public class UIPrefabLoader : MonoBehaviour {
	#region CONSTANTS --------------------------------------------------------------------------------------------------

	#endregion

	#region EXPOSED MEMBERS --------------------------------------------------------------------------------------------
	// [AOC] Initialize all these from the inspector :-)

	#endregion

	#region INTERNAL MEMBERS -------------------------------------------------------------------------------------------
	private Canvas mCanvas;
	#endregion
	
	#region GENERIC METHODS --------------------------------------------------------------------------------------------
	/// <summary>
	/// Use this for initialization
	/// </summary>
	void Start() {
		// Get target UI canvas
		mCanvas = gameObject.GetComponent<Canvas>();
	}

	/// <summary>
	/// Create an instance of the target prefab and put it into the canvas.
	/// <para>The prefab must be in the <c>Resources<c/> folder!</para>
	/// </summary>
	/// <returns>The newly created instance, <c>null<c/> if it wasn't possible to create it.</returns>
	/// <param name="_sPath">Path to the prefab resource. It must be in the <c>Resources</c> folder.</param>
	/// <param name="_sName">Optionally add a name to the new instance. If left empty, default name will be set.</param>
	public GameObject InstantiatePrefab(String _sPath, String _sName = "") {
		// Try to load the prefab from resources
		GameObject prefab = Resources.Load(_sPath) as GameObject;
		if(prefab != null) {
			// Prefab loaded, create a new instance of it
			GameObject newInstanceObj = Instantiate(prefab) as GameObject;
			if(newInstanceObj != null) {
				// Set custom name (if defined) otherwise use prefab's default name
				if(_sName != "") {
					newInstanceObj.name = _sName;
				} else {
					newInstanceObj.name = prefab.name;
				}

				// [AOC] TODO!! Add any other required action in here (see HSE's GUIManager)

				// Add the new instance to the canvas
				newInstanceObj.transform.SetParent(mCanvas.transform, false);

				// Return the newly created instance
				return newInstanceObj;
			}
		} else {
			Debug.LogError("Couldn't load Resources/" + _sPath + "!");
		}

		// Prefab not found, return null
		return null;
	}
	#endregion
}
#endregion