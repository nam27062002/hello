// DragonSelectionScene.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 12/11/2018.
// Copyright (c) 2018 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// 3D scene controller for the Dragon Selection Screen.
/// </summary>
public class DragonSelectionScene : MenuScreenScene {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	
	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	[Tooltip("Will replace the camera snap point for the photo screen when doing photos to classic dragons.")]
	[SerializeField] private CameraSnapPoint m_photoCameraSnapPoint = null;
	public CameraSnapPoint photoCameraSnapPoint {
		get { return m_photoCameraSnapPoint; }
	}
	
	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//

	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
}