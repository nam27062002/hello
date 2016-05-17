// DirectionShaderPropertyDrawer.cs
// 
// Created by Alger Ortín Castellví on 17/05/2016.
// Copyright (c) 2016 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;
using UnityEditor;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// Custom inspector for a Lisght Direction property on a custom shader.
/// It should be equivalent to a Vector4 where the xyz represent the light position
/// and the w the intensity.
/// Use with "[LightDirection]" before a float shader property
/// </summary>
public class LightDirectionDrawer : MaterialPropertyDrawer {
	//------------------------------------------------------------------//
	// MEMBERS															//
	//------------------------------------------------------------------//

	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Draw the property inside the given rect.
	/// </summary>
	override public void OnGUI(Rect _rect, MaterialProperty _prop, string _label, MaterialEditor _editor) {
		// [AOC] Direction property is a Vector4 with the w being the light intensity
		// Setup
		Vector4 value = _prop.vectorValue;
		EditorGUI.BeginChangeCheck();
		EditorGUI.showMixedValue = _prop.hasMixedValue;
		Rect pos = new Rect(_rect.x, _rect.y, _rect.width, EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing);

		// Label
		EditorGUI.PrefixLabel(pos, new GUIContent("Light Setup"));
		pos.x += 25f;
		pos.width -= 25f;
		EditorGUIUtility.labelWidth = 0;

		// Show the custom control
		// 1. Light position
		// [AOC] Since there is no pretty way to do this, try with sliders
		pos.y += pos.height;
		value.x = EditorGUI.Slider(pos, "x", value.x, -1, 1);
		pos.y += pos.height;
		value.y = EditorGUI.Slider(pos, "y", value.y, -1, 1);
		pos.y += pos.height;
		value.z = EditorGUI.Slider(pos, "z", value.z, -1, 1);

		// 2. Light intensity
		pos.y += pos.height;
		value.w = EditorGUI.Slider(pos, "Intensity", value.w, 0, 10);

		// Apply changed values
		EditorGUI.showMixedValue = false;
		if(EditorGUI.EndChangeCheck()) {
			Vector3 dir = new Vector3(value.x, value.y, value.z);
			dir.Normalize();
			value.Set(dir.x, dir.y, dir.z, value.w);
			_prop.vectorValue = value;
		}
	}

	/// <summary>
	/// Gets the height of the property drawer.
	/// </summary>
	/// <returns>The height required by this property drawer.</returns>
	/// <param name="_property">The property we're drawing.</param>
	/// <param name="_label">The label of the property.</param>
	override public float GetPropertyHeight(MaterialProperty _prop, string _label, MaterialEditor _editor) {
		return EditorGUIUtility.singleLineHeight * 5 + EditorGUIUtility.standardVerticalSpacing * 5;	// label + x-y-z-w + spacing
	}
}