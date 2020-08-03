// RangeEditor.cs
// 
// Created by Alger Ortín Castellví on 26/05/2015.
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
/// Custom editor for the RangeLong class.
/// </summary>
[CustomPropertyDrawer(typeof(RangeLong))]
public class RangeLongEditor : PropertyDrawer {
	//------------------------------------------------------------------//
	// CONSTANTS														//
	//------------------------------------------------------------------//

	//------------------------------------------------------------------//
	// MEMBERS															//
	//------------------------------------------------------------------//

	//------------------------------------------------------------------//
	// METHODS															//
	//------------------------------------------------------------------//
	/// <summary>
	/// A change has occurred in the inspector, draw the property and store new values.
	/// </summary>
	/// <param name="_position">The area in the inspector assigned to draw this property.</param>
	/// <param name="_property">The property we're drawing.</param>
	/// <param name="_label">The label of the property.</param>
	public override void OnGUI(Rect _position, SerializedProperty _property, GUIContent _label) {
		// Aux vars
		float padding = 5f;
		GUIContent content = new GUIContent();
		GUIStyle style = GUI.skin.label;	// Default label style, useful to compute text widths
		GUIStyle styleTF = new GUIStyle(EditorStyles.textField);

		// Group all
		_label = EditorGUI.BeginProperty(_position, _label, _property);
		EditorGUI.BeginChangeCheck ();

		// Retrieve min and max properties and create a temp range to work with
		SerializedProperty min = _property.FindPropertyRelative("min");
		SerializedProperty max = _property.FindPropertyRelative("max");
		long minValue = min.longValue;
		long maxValue = max.longValue;

		// Draw label and initialize position helpers
		Rect contentRect = EditorGUI.PrefixLabel(_position, _label);	// Remaining space for the content
		Rect cursor = contentRect;

		////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Draw min value
		EditorGUI.BeginChangeCheck();

		cursor.width = contentRect.width/2 - padding/2;
		content.text = "min";

		EditorGUIUtility.labelWidth = style.CalcSize(content).x + padding;	// Adjust label to "min" word
		if(minValue > max.longValue) {
			styleTF.normal.textColor = Color.red;
			styleTF.focused.textColor = Color.red;
		}
		minValue = EditorGUI.LongField(cursor, content, minValue, styleTF);

		// If changed, save new value
		if(EditorGUI.EndChangeCheck()) {
			min.longValue = minValue;
		}

		////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Draw max value
		EditorGUI.BeginChangeCheck();

		cursor.x += cursor.width + padding;
		cursor.width = contentRect.width/2 - padding/2;
		content.text = "max";
		EditorGUIUtility.labelWidth = style.CalcSize(content).x + padding;	// Adjust label to "max" word
		maxValue = EditorGUI.LongField(cursor, content, maxValue, styleTF);

		// If changed, save new value
		if(EditorGUI.EndChangeCheck()) {
			max.longValue = maxValue;
		}

		// Done!
		EditorGUI.EndProperty ();
	}
}