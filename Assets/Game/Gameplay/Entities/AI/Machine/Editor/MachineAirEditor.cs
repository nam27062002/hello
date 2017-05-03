// MonoBehaviourTemplateEditor.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on DD/MM/2016.
// Copyright (c) 2016 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using UnityEditor;
using AI;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Custom editor for the MonoBehaviourTemplate class.
/// </summary>
[CustomEditor(typeof(MachineAir), true)]	// True to be used by heir classes as well
[CanEditMultipleObjects]
public class MachineAirEditor : MachineEditor {
	//------------------------------------------------------------------------//
	// METHODS																  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// The editor has been enabled - target object selected.
	/// </summary>
	protected override void OnEnable() {
		// Store a reference of interesting properties for faster access
		m_motionProp = serializedObject.FindProperty("m_airMotion");

		base.OnEnable();
	}
}