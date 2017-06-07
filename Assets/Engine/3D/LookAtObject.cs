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
[System.Obsolete("This component is obsolete, use LookAt component instead.")]
public class LookAtObject : MonoBehaviour {
	//------------------------------------------------------------------//
	// CONSTANTS														//
	//------------------------------------------------------------------//

	//------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES											//
	//------------------------------------------------------------------//
	public Transform lookAtObject = null;

    private Transform m_transform;

	//------------------------------------------------------------------//
	// METHODS															//
	//------------------------------------------------------------------//
	void Awake() {
        m_transform = transform;
    }

    /// <summary>
	/// Logic update call.
	/// </summary>
	void Update () {
		if(isActiveAndEnabled && lookAtObject != null) {
			m_transform.LookAt(lookAtObject.position);
		}
	}
}
