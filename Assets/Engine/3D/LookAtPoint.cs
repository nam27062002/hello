// LookAtPoint.cs
// 
// Created by Alger Ortín Castellví on 29/05/2015.
// Copyright (c) 2015 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// Define a lookAt point for a game object (e.g. a camera).
/// The target object will always look towards the defined point.
/// </summary>
[ExecuteInEditMode]  
public class LookAtPoint : MonoBehaviour {
	//------------------------------------------------------------------//
	// CONSTANTS														//
	//------------------------------------------------------------------//

	//------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES											//
	//------------------------------------------------------------------//
	public Vector3 lookAtPoint = Vector3.zero;
	
	//------------------------------------------------------------------//
	// METHODS															//
	//------------------------------------------------------------------//
	/// <summary>
	/// Logic update call.
	/// </summary>
	void Update () {
		if(isActiveAndEnabled) {
			transform.LookAt(lookAtPoint);
		}
	}
}
