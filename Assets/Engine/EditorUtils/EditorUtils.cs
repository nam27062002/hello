// EditorUtils.cs
// 
// Created by Alger Ortín Castellví on 17/09/2015.
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
/// Collection of static utilities related to custom editors.
/// </summary>
public static class EditorUtils {
	//------------------------------------------------------------------//
	// MEMBERS															//
	//------------------------------------------------------------------//
	
	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Custom drawer for an enum field, with the additional option of excluding first and last elements of the enum.
	/// First element is commonly used as NONE or INIT values, while last element is often used as COUNT value.
	/// </summary>
	/// <param name="_pos">The position rectangle where the property should be drawn. Its height will be changed to the actual height required by the enum field.</param>
	/// <param name="_property">The enum property to be drawn.</param>
	/// <param name="_excludeFirstElement">If set to <c>true</c> _exclude first element of the enum.</param>
	/// <param name="_excludeLastElement">If set to <c>true</c> _exclude last element of the enum.</param>
	public static void EnumField(ref Rect _pos, SerializedProperty _property, bool _excludeFirstElement = false, bool _excludeLastElement = false) {
		// Figure out the list of options to be displayed
		int numOptions = _property.enumNames.Length;
		if(_excludeFirstElement) numOptions--;
		if(_excludeLastElement) numOptions--;
		string[] options = new string[numOptions];
		int[] values = new int[numOptions];
		int enumIdx = _excludeFirstElement ? 1 : 0;
		for(int i = 0; i < numOptions; i++, enumIdx++) {
			options[i] = _property.enumDisplayNames[enumIdx];
			values[i] = enumIdx;
		}

		// Clamp current value, in case it's either the first or the last and they're excluded
		_property.enumValueIndex = Mathf.Clamp(_property.enumValueIndex, values[0], values[numOptions - 1]);

		// Display the property and store new value
		_pos.height = EditorGUI.GetPropertyHeight(_property);	// [AOC] Default enum field height
		_property.enumValueIndex = EditorGUI.IntPopup(_pos, _property.displayName, _property.enumValueIndex, options, values);
	}

	/// <summary>
	/// Draws a separator and automatically updates position pointer.
	/// </summary>
	/// <param name="_pos">The position rectangle where the separator should be drawn. Its height will be changed to the actual height required by the separator.</param>
	/// <param name="_title">The title to be displayed on the separator, optional.</param>
	/// <param name="_size">The size in pixels of the separator, optional.</param>
	public static void DrawSeparator(ref Rect _pos, string _title = "", float _size = 40f) {
		// Store separator size
		_pos.height = _size;

		// Aux helper to draw lines
		Rect lineBounds = _pos;
		lineBounds.height = 1f;
		lineBounds.y = _pos.y + _pos.height/2f - lineBounds.height/2f;	// Vertically centered
		
		// Do we have title?
		if(_title == "") {
			// No! Draw a single line from left to right
			lineBounds.x = _pos.x;
			lineBounds.width = _pos.width;
			GUI.Box(lineBounds, "");
		} else {
			// Yes!
			// Compute title's width
			GUIContent titleContent = new GUIContent(_title);
			GUIStyle titleStyle = new GUIStyle(EditorStyles.label);	// Default label style
			titleStyle.alignment = TextAnchor.MiddleCenter;	// Alignment!
			titleStyle.fontStyle = FontStyle.Italic;
			float titleWidth = titleStyle.CalcSize(titleContent).x;
			titleWidth += 10f;	// Add some spacing around the title
			
			// Draw line at the left of the title
			lineBounds.x = _pos.x;
			lineBounds.width = _pos.width/2f - titleWidth/2f;
			GUI.Box(lineBounds, "");
			
			// Draw title
			Rect titleBounds = _pos;	// Using whole area's height
			titleBounds.x = lineBounds.xMax;	// Concatenate to the line we just draw
			titleBounds.width = titleWidth;
			GUI.Label(titleBounds, _title, titleStyle);
			
			// Draw line at the right of the title
			lineBounds.x = titleBounds.xMax;	// Concatenate to the title label
			lineBounds.width = _pos.width/2f - titleWidth/2f;
			GUI.Box(lineBounds, "");
		}
	}
}