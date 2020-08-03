// SpawnerIconGenerator.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 28/10/2015.
// Copyright (c) 2015 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;
using System.Collections;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// Auxiliar component for spawners generating a custom icon for that spawner.
/// </summary>
[System.Obsolete("To be deleted soon")]
public class SpawnerIconGenerator : MonoBehaviour {
	//------------------------------------------------------------------//
	// MEMBERS															//
	//------------------------------------------------------------------//
	public Color m_backgroundColor = new Color(0f, 0f, 0f, 0.25f);	// Make it quite transparent - full transparent is confusing, looks like a 3D object
	public Texture2D m_tex = null;	// The actual icon texture

	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	protected void Awake() {
		// Not much to do
	}
}
