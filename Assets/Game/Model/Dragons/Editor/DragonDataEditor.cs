// DragonDataEditor.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 14/09/2015.
// Copyright (c) 2015 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;
using UnityEditor;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// Custom editor for the DragonData class.
/// </summary>
//[CustomPropertyDrawer(typeof(DragonData))]
public class DragonDataEditor : PropertyDrawer {
	//------------------------------------------------------------------//
	// MEMBERS															//
	//------------------------------------------------------------------//

	//------------------------------------------------------------------//
	// METHODS															//
	//------------------------------------------------------------------//
	public DragonDataEditor() {
		// Nothing to do
	}

	/// <summary>
	/// A change has occurred in the inspector, draw the property and store new values.
	/// </summary>
	/// <param name="_position">The area in the inspector assigned to draw this property.</param>
	/// <param name="_property">The property we're drawing.</param>
	/// <param name="_label">The label of the property.</param>
	public override void OnGUI(Rect _position, SerializedProperty _property, GUIContent _label) {

		// [AOC] TODO!!
	}
	
	/// <summary>
	/// Gets the height of the property drawer.
	/// </summary>
	/// <returns>The height required by this property drawer.</returns>
	/// <param name="_property">The property we're drawing.</param>
	/// <param name="_label">The label of the property.</param>
	public override float GetPropertyHeight(SerializedProperty _property, GUIContent _label) {
		return base.GetPropertyHeight(_property, _label);
	}
}