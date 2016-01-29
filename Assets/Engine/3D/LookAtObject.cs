// LookAtObject.cs
// 
// Created by Alger Ortín Castellví on 29/01/2016.
// Copyright (c) 2016 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// Use a GameObject as a lookAt point for another GameObject (e.g. a camera).
/// The target object will always look towards the defined object.
/// </summary>
[ExecuteInEditMode]  
public class LookAtObject : MonoBehaviour {
	//------------------------------------------------------------------//
	// CONSTANTS														//
	//------------------------------------------------------------------//

	//------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES											//
	//------------------------------------------------------------------//
	public GameObject lookAtObject = null;
	
	//------------------------------------------------------------------//
	// METHODS															//
	//------------------------------------------------------------------//
	/// <summary>
	/// Logic update call.
	/// </summary>
	void Update () {
		if(isActiveAndEnabled && lookAtObject != null) {
			transform.LookAt(lookAtObject.transform.position);
		}
	}
}
