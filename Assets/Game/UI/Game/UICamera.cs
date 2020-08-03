// UICamera.cs
// Hungry Dragon
// 
// Created by  on 30/06/2020.
// Copyright (c) 2020 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// The only purpose of this class is to store the UI camera in the instance manager
/// </summary>
[RequireComponent (typeof (Camera)) ]
public class UICamera : MonoBehaviour {

	//------------------------------------------------------------------------//
	// MEMBERS      														  //
	//------------------------------------------------------------------------//
	private Camera m_unityCamera;
    public Camera unityCamera
    {
		get { return m_unityCamera; } 
    }

	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {

		m_unityCamera = GetComponent<Camera>();

		DebugUtils.Assert(m_unityCamera != null, "No Camera");
        
		InstanceManager.uiCamera = this;
	}

	

	/// <summary>
	/// Destructor.
	/// </summary>
	private void OnDestroy() {

		InstanceManager.uiCamera = null;
	}


}