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
[CustomEditor(typeof(MachineWagon), true)]	// True to be used by heir classes as well
[CanEditMultipleObjects]
public class MachineWagonEditor : MachineEditor {

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	private SerializedProperty m_canExplodeProp = null;

	//------------------------------------------------------------------------//
	// METHODS																  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// The editor has been enabled - target object selected.
	/// </summary>
	protected override void OnEnable() {
		// Store a reference of interesting properties for faster access
		m_motionProp = serializedObject.FindProperty("m_wagonMotion");
		m_canExplodeProp = serializedObject.FindProperty("m_canExplode");

		base.OnEnable();
	}

	protected override bool HasCustomDraw(string _pName) {					
		return _pName.Equals("m_explosionDamage") || _pName.Equals("m_explosionRadius") || _pName.Equals("m_explosionCameraShake");
	}

	protected override void CustomDraw(SerializedProperty _p) {
		if (m_canExplodeProp.boolValue) {
			EditorGUI.indentLevel++; {
				EditorGUILayout.PropertyField(_p, true);
			} EditorGUI.indentLevel--;
		}
	}
}