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
	public static void Separator(SeparatorAttribute _separator = null, SeparatorAttribute.Orientation _orientation = SeparatorAttribute.Orientation.HORIZONTAL, float _thickness = 1f) {
		if(_separator == null) _separator = new SeparatorAttribute();
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
	/// Custom drawer for an array property, excluding its size field so it can't be modified.
	/// Optionally force a fixed size on the array, removing/adding elements as needed.
	/// Layout version.
	/// </summary>
	/// <param name="_property">The array property to be drawn. Must be of type array.</param>
	/// <param name="_customElementDrawer">Optional method to be invoked instead of the default property drawer for each element of the array. Must take as parameters the property to be displayed and the index of that property within the array we're rendering.</param>
	/// <param name="_forcedSize">If bigger than 0, elements will be added/removed to the array to match it. Otherwise the array's current length will be used.</param>
	public static void FixedLengthArray(SerializedProperty _property, Action<SerializedProperty, int> _customElementDrawer = null, int _forcedSize = -1) {
		// Check type
		if(!DebugUtils.Assert(_property.isArray, "Must be an array property type")) return;

		// If required, adjust size by adding/removing elements to the array
		if(_forcedSize > 0) {
			if(_property.arraySize != _forcedSize) {
				_property.arraySize = _forcedSize;	// This will do it
			}
		}

		// Draw property label + foldout widget
		_property.isExpanded = EditorGUILayout.Foldout(_property.isExpanded, _property.displayName);

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
					_customElementDrawer(elementProp, i);
				} else {
					// No, use default property drawer
					EditorGUILayout.PropertyField(elementProp, true);
				}
			}

			// Indent back out
			EditorGUI.indentLevel--;
		}
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

	/// <summary>
	/// Generic object popup version.
	/// </summary>
	/// <returns>The newly selected object in the popup.</returns>
	/// <param name="_label">The label of the inspector field.</param>
	/// <param name="_selectedObject">Current selection.</param>
	/// <param name="_options">Candidate values.</param>
	/// <param name="_layoutOptions">Visual options for the inspector.</param>
	public static T Popup<T>(string _label, T _selectedObject, T[] _options, params GUILayoutOption[] _layoutOptions) where T : UnityEngine.Object {
		// Create options array and find out selected index
		int selectedIdx = 0;	// First option by default
		string[] options = new string[_options.Length];
		for(int i = 0; i < options.Length; i++) {
			options[i] = _options[i].name;
			if(_options[i] == _selectedObject) {
				selectedIdx = i;
			}
		}

		// Show string popup
		selectedIdx = EditorGUILayout.Popup(_label, selectedIdx, options, _layoutOptions);

		// Return new selection
		if(selectedIdx < _options.Length) {
			return _options[selectedIdx];
		}
		return null;
	}

	/// <summary>
	/// Draw a property with a toggle in front of it, storing the toggle value in another given property.
	/// </summary>
	/// <param name="_toggleProp">Property used to determine toggle status.</param>
	/// <param name="_p">Property to be displayed.</param>
	/// <param name="_label">Label to be displayed.</param>
	/// <param name="_customPropertyDrawer">Optional method to be invoked instead of the default property drawer for the target property. Must take as parameters the property to be displayed.</param>
	public static void TogglePropertyField(SerializedProperty _toggleProp, SerializedProperty _p, string _label, Action<SerializedProperty> _customPropertyDrawer = null) {
		// Toggle property must be boolean!
		Debug.Assert(_toggleProp.propertyType == SerializedPropertyType.Boolean);

		// Create a horizontal group
		EditorGUILayout.BeginHorizontal(); {
			// Do toggle property
			_toggleProp.boolValue = GUILayout.Toggle(_toggleProp.boolValue, GUIContent.none, GUILayout.Width(10f));
			EditorGUIUtility.labelWidth -= 10f;	// Compensate Toggle's width

			// Enable/Disable GUI based on toggle value
			bool wasEnabled = GUI.enabled;
			GUI.enabled = _toggleProp.boolValue;

			// Do the property
			// Use default drawer?
			if(_customPropertyDrawer == null) {
				EditorGUILayout.PropertyField(_p, new GUIContent(_label), true);
			} else {
				_customPropertyDrawer(_p);
			}

			// Restore GUI enabled state
			GUI.enabled = wasEnabled;
			EditorGUIUtility.labelWidth += 10f;	// Restore label width
		} EditorGUILayoutExt.EndHorizontalSafe();
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

	//------------------------------------------------------------------//
	// PREFENCES FIELDS 												//
	//------------------------------------------------------------------//
	/// <summary>
	/// Draws the default field for the given type, loading and saving the new value 
	/// to the EditorPrefs dictionary.
	/// </summary>
	/// <returns>The new value of the pref.</returns>
	/// <param name="_prefId">The pref identifier.</param>
	/// <param name="_defaultValue">Default value if the pref wasn't stored.</param>
	/// <typeparam name="T">Type of the preference value. Should be string, int, float, bool or Enum</typeparam>
	public static T PrefField<T>(string _label, string _prefId, T _defaultValue, params GUILayoutOption[] _options) {
		// [AOC] Unfortunately we can't switch a type directly, but we can compare type via an if...else collection
		// [AOC] Double cast trick to prevent compilation errors: http://stackoverflow.com/questions/4092393/value-of-type-t-cannot-be-converted-to
		Type t = typeof(T);

		// String
		if(t == typeof(string)) {
			string value = Prefs.GetStringEditor(_prefId, (string)(object)_defaultValue);
			value = EditorGUILayout.TextField(_label, value, _options);
			Prefs.SetStringEditor(_prefId, value);
			return (T)(object)value;
		}

		// Float
		else if(t == typeof(float)) {
			float value = Prefs.GetFloatEditor(_prefId, (float)(object)_defaultValue);
			value = EditorGUILayout.FloatField(_label, value, _options);
			Prefs.SetFloatEditor(_prefId, value);
			return (T)(object)value;
		}

		// Int
		else if(t == typeof(int)) {
			int value = Prefs.GetIntEditor(_prefId, (int)(object)_defaultValue);
			value = EditorGUILayout.IntField(_label, value, _options);
			Prefs.SetIntEditor(_prefId, value);
			return (T)(object)value;
		}

		// Bool
		else if(t == typeof(bool)) {
			bool value = Prefs.GetBoolEditor(_prefId, (bool)(object)_defaultValue);
			value = EditorGUILayout.Toggle(_label, value, _options);
			Prefs.SetBoolEditor(_prefId, value);
			return (T)(object)value;
		}

		// Enum
		else if(t.IsEnum) {
			Enum value = (Enum)(object)Prefs.Get<T>(_prefId, _defaultValue, Prefs.Mode.EDITOR);
			value = EditorGUILayout.EnumPopup(_label, value, _options);
			Prefs.Set<T>(_prefId, (T)(object)value, Prefs.Mode.EDITOR);
			return (T)(object)value;
		}

		// Vector2
		else if(t == typeof(Vector2)) {
			Vector2 value = Prefs.GetVector2Editor(_prefId, (Vector2)(object)_defaultValue);
			value = EditorGUILayout.Vector2Field(_label, value, _options);
			Prefs.SetVector2Editor(_prefId, value);
			return (T)(object)value;
		}

		// Vector3
		else if(t == typeof(Vector3)) {
			Vector3 value = Prefs.GetVector3Editor(_prefId, (Vector3)(object)_defaultValue);
			value = EditorGUILayout.Vector3Field(_label, value, _options);
			Prefs.SetVector3Editor(_prefId, value);
			return (T)(object)value;
		}

		// Color
		else if(t == typeof(Color)) {
			Color value = Prefs.GetColorEditor(_prefId, (Color)(object)_defaultValue);
			value = EditorGUILayout.ColorField(_label, value, _options);
			Prefs.SetColorEditor(_prefId, value);
			return (T)(object)value;
		}

		// Unsupported
		else {
			Debug.Log("Unsupported type!");
		}
		return _defaultValue;
	}
}