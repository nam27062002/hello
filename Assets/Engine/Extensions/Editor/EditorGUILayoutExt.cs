// EditorGUILayoutExt.cs
// 
// Created by Alger Ortín Castellví on 06/11/2015.
// Copyright (c) 2015 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;
using UnityEditor;
using System;
using System.Reflection;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// 
/// </summary>
public static class EditorGUILayoutExt {
	//------------------------------------------------------------------//
	// CUSTOM PROPERTY DRAWERS											//
	//------------------------------------------------------------------//
	/// <summary>
	/// Draw a separator in an editor layout GUI.
	/// </summary>
	/// <param name="_separator">The separator to be drawn.</param>
	/// <param name="_orientation">Orientation of the separator. Use horizontal separators for vertical layouts and viceversa.</param>
	/// <param name="_thickness">Size of the separator line, in pixels. Will override separator's size property if bigger.</param>
	public static void Separator(SeparatorAttribute _separator, SeparatorAttribute.Orientation _orientation = SeparatorAttribute.Orientation.HORIZONTAL, float _thickness = 1f) {
		SeparatorAttributeEditor.DrawSeparator(_separator, _orientation, _thickness);
	}

	/// <summary>
	/// Draws a separator and automatically updates position pointer.
	/// </summary>
	/// <returns>The actual height required by the field</returns>
	/// <param name="_pos">The position rectangle where the separator should be drawn.</param>
	/// <param name="_separator">The separator to be drawn.</param>
	public static float Separator(Rect _pos, SeparatorAttribute _separator) {
		return SeparatorAttributeEditor.DrawSeparator(_pos, _separator);
	}

	/// <summary>
	/// Custom drawer for an array property, excluding its size field so it can't be modified.
	/// Optionally force a fixed size on the array, removing/adding elements as needed.
	/// </summary>
	/// <returns>The actual height required by the field</returns>
	/// <param name="_pos">The position rectangle where the property should be drawn.</param>
	/// <param name="_property">The array property to be drawn. Must be of type array.</param>
	/// <param name="_customElementDrawer">Optional method to be invoked instead of the default property drawer for each element of the array. Must return the height taken by the property and take as parameters the positining Rect, the property to be displayed and the index of that property within the array we're rendering.</param>
	/// <param name="_forcedSize">If bigger than 0, elements will be added/removed to the array to match it. Otherwise the array's current length will be used.</param>
	public static float FixedLengthArray(Rect _pos, SerializedProperty _property, Func<Rect, SerializedProperty, int, float> _customElementDrawer = null, int _forcedSize = -1) {
		// Check type
		if(!DebugUtils.Assert(_property.isArray, "Must be an array property type")) return 0f;
		
		// Aux vars
		float totalHeight = 0f;
		
		// If required, adjust size by adding/removing elements to the array
		if(_forcedSize > 0) {
			if(_property.arraySize != _forcedSize) {
				_property.arraySize = _forcedSize;	// This will do it
			}
		}
		
		// Draw property label + foldout widget
		_pos.height = EditorStyles.largeLabel.lineHeight;	// Advance pointer just the size of the label
		_property.isExpanded = EditorGUI.Foldout(_pos, _property.isExpanded, _property.displayName);
		_pos.y += _pos.height;
		totalHeight += _pos.height;
		
		// If unfolded, draw array entries
		if(_property.isExpanded) {
			// Indentation in
			EditorGUI.indentLevel++;
			
			// Aux vars
			SerializedProperty elementProp = null;
			
			// Iterate through all elements
			for(int i = 0; i < _property.arraySize; i++) {
				// Get property for this element
				elementProp = _property.GetArrayElementAtIndex(i);
				
				// Draw the property - shall we use a custom drawer?
				if(_customElementDrawer != null) {
					// Yes! Invoke it and store size taken
					_pos.height = _customElementDrawer(_pos, elementProp, i);
				} else {
					// No, use default property drawer
					_pos.height = EditorGUI.GetPropertyHeight(elementProp);
					EditorGUI.PropertyField(_pos, elementProp, true);
				}
				
				// Advance position cursor
				_pos.y += _pos.height;
				totalHeight += _pos.height;
			}

			// Indent back out
			EditorGUI.indentLevel--;
		}
		
		return totalHeight;
	}
	
	/// <summary>
	/// Custom version of the EditorGUILayout.Vector3Field, putting the label at the same row as the XYZ textfields.
	/// </summary>
	/// <returns>The value entered by the user.</returns>
	/// <param name="_label">Label to display before the field. Leave empty for no label.</param>
	/// <param name="_value">The value to edit.</param>
	/// <param name="_options">An optional list of layout options that specify extra layouting properties. Any values passed in here will override settings defined by the style. See Also: GUILayout.Width, GUILayout.Height, GUILayout.MinWidth, GUILayout.MaxWidth, GUILayout.MinHeight, GUILayout.MaxHeight, GUILayout.ExpandWidth, GUILayout.ExpandHeight.</param>
	public static Vector3 Vector3Field(string _label, Vector3 _value, params GUILayoutOption[] _options) {
		// Group everything in an horizontal layout
		// Apply custom options here - may not work with all options
		EditorGUILayout.BeginHorizontal(_options); {
			// Label (if not empty)
			if(_label != "") {
				GUILayout.Label(_label);
			}
			
			// XYZ values
			EditorGUIUtility.labelWidth = 15;
			_value.x = EditorGUILayout.FloatField("X", _value.x);
			_value.y = EditorGUILayout.FloatField("Y", _value.y);
			_value.z = EditorGUILayout.FloatField("Z", _value.z);
			EditorGUIUtility.labelWidth = 0;
		} EditorGUILayoutExt.EndHorizontalSafe();
		
		return _value;
	}

	//------------------------------------------------------------------//
	// LAYOUT UTILS														//
	//------------------------------------------------------------------//
	/// <summary>
	/// Same as EditorGUILayout.EndHorizontal(), but catching exceptions.
	/// For some reason an exception is thrown when opening doing complex operations within a layout scope, for example 
	/// opening another editor window from a button in the layout. Since it's editor code, we don't care about the exception
	/// at all, but we don't want it adding noise to our console, so we will catch it.
	/// </summary>
	public static void EndHorizontalSafe() {
		try {
			EditorGUILayout.EndHorizontal(); 
		} catch { 
			
		}
	}
	
	/// <summary>
	/// Same as EditorGUILayout.EndVertical(), but catching exceptions.
	/// For some reason an exception is thrown when opening doing complex operations within a layout scope, for example 
	/// opening another editor window from a button in the layout. Since it's editor code, we don't care about the exception
	/// at all, but we don't want it adding noise to our console, so we will catch it.
	/// </summary>
	public static void EndVerticalSafe() {
		try {
			EditorGUILayout.EndVertical(); 
		} catch { 
			
		}
	}
	
	/// <summary>
	/// Same as EditorGUILayout.EndScrollView(), but catching exceptions.
	/// For some reason an exception is thrown when opening doing complex operations within a layout scope, for example 
	/// opening another editor window from a button in the layout. Since it's editor code, we don't care about the exception
	/// at all, but we don't want it adding noise to our console, so we will catch it.
	/// </summary>
	public static void EndScrollViewSafe() {
		try {
			EditorGUILayout.EndScrollView(); 
		} catch { 
			
		}
	}
}