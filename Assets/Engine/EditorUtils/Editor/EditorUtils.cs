// EditorUtils.cs
// 
// Created by Alger Ortín Castellví on 17/09/2015.
// Copyright (c) 2015 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;
using UnityEditor;
using System;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// Collection of static utilities related to custom editors.
/// </summary>
public static class EditorUtils {
	//------------------------------------------------------------------//
	// CONSTANTS														//
	//------------------------------------------------------------------//
	public enum Orientation {
		HORIZONTAL,
		VERTICAL
	};

	public static readonly Color DEFAULT_SEPARATOR_COLOR = new Color(0.65f, 0.65f, 0.65f);	// Silver-ish

	//------------------------------------------------------------------//
	// MEMBERS															//
	//------------------------------------------------------------------//
	
	//------------------------------------------------------------------//
	// CUSTOM DECORATIONS												//
	//------------------------------------------------------------------//
	/// <summary>
	/// Draws a separator and automatically updates position pointer.
	/// </summary>
	/// <returns>The actual height required by the field</returns>
	/// <param name="_pos">The position rectangle where the separator should be drawn.</param>
	/// <param name="_title">The title to be displayed on the separator, optional.</param>
	/// <param name="_size">The size in pixels of the separator, optional.</param>
	public static float Separator(Rect _pos, string _title = "", float _size = 40f) {
		// Initialized style for a line
		// [AOC] We will be drawing a box actually, so copy some values from the box style
		GUIStyle lineStyle = new GUIStyle();
		lineStyle.normal.background = Texture2DExtensions.Create(2, 2, DEFAULT_SEPARATOR_COLOR);
		lineStyle.margin = EditorStyles.helpBox.margin;
		lineStyle.padding = EditorStyles.helpBox.padding;

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
			GUI.Box(lineBounds, "", lineStyle);
		} else {
			// Yes!
			// Compute title's width
			GUIContent titleContent = new GUIContent(_title);
			GUIStyle titleStyle = new GUIStyle(EditorStyles.label);	// Default label style
			titleStyle.alignment = TextAnchor.MiddleCenter;	// Alignment!
			titleStyle.fontStyle = FontStyle.Italic;
			titleStyle.normal.textColor = Colors.gray;
			float titleWidth = titleStyle.CalcSize(titleContent).x;
			titleWidth += 10f;	// Add some spacing around the title
			
			// Draw line at the left of the title
			lineBounds.x = _pos.x;
			lineBounds.width = _pos.width/2f - titleWidth/2f;
			GUI.Box(lineBounds, "", lineStyle);
			
			// Draw title
			Rect titleBounds = _pos;	// Using whole area's height
			titleBounds.x = lineBounds.xMax;	// Concatenate to the line we just draw
			titleBounds.width = titleWidth;
			GUI.Label(titleBounds, _title, titleStyle);
			
			// Draw line at the right of the title
			lineBounds.x = titleBounds.xMax;	// Concatenate to the title label
			lineBounds.width = _pos.width/2f - titleWidth/2f;
			GUI.Box(lineBounds, "", lineStyle);
		}

		return _pos.height;
	}

	/// <summary>
	/// Draw a separator in an editor layout GUI.
	/// </summary>
	/// <param name="_orientation">Orientation of the separator. Use horizontal separators for vertical layouts and viceversa.</param>
	/// <param name="_spacing">Empty space before and after the separator.</param>
	/// <param name="_text">The text to be displayed, optional. If empty, a single line will be displayed.</param>
	/// <param name="_size">Size of the separator, in pixels.</param>
	/// <param name="_color">The color of the line, optional.</param>
	public static void Separator(Orientation _orientation, float _spacing, string _text = "", float _size = 1f, Color? _color = null) {	// Nullable type, the only way to pass a default value to a Color - see https://msdn.microsoft.com/en-us/library/1t3y8s4s.aspx
		// Initialized style for a line
		// [AOC] We will be drawing a box actually, so copy some values from the box style
		GUIStyle lineStyle = new GUIStyle();
		Color lineColor = _color ?? DEFAULT_SEPARATOR_COLOR;	// Nullable type check, see https://msdn.microsoft.com/en-us/library/1t3y8s4s.aspx
		lineStyle.normal.background = Texture2DExtensions.Create(2, 2, lineColor);
		lineStyle.margin = EditorStyles.helpBox.margin;
		lineStyle.padding = EditorStyles.helpBox.padding;

		// Add spacing before
		GUILayout.Space(_spacing);

		// Do we have a title?
		if(_text == "") {
			// No! Single line
			// Vertical or horizontal?
			if(_orientation == Orientation.VERTICAL) {
				// Draw separator
				GUILayout.Box(GUIContent.none, lineStyle, GUILayout.ExpandHeight(true), GUILayout.Width(_size));
			} else {
				// Draw separator
				GUILayout.Box(GUIContent.none, lineStyle, GUILayout.ExpandWidth(true), GUILayout.Height(_size));
			}
		} else {
			// Yes! Slightly more complicated
			// Create style and content for the text
			GUIContent textContent = new GUIContent(_text);
			GUIStyle textStyle = new GUIStyle(EditorStyles.label);	// Default label style
			textStyle.alignment = TextAnchor.MiddleCenter;	// Alignment!
			textStyle.fontStyle = FontStyle.Italic;
			textStyle.normal.textColor = Colors.gray;
			Vector2 textSize = textStyle.CalcSize(textContent);

			// We need to create a layout with flexible spaces to each part so the line and the title are aligned
			// Vertical or horizontal?
			if(_orientation == Orientation.VERTICAL) {
				// Draw separator
				EditorGUILayout.BeginVertical(GUILayout.ExpandHeight(true), GUILayout.Width(Mathf.Max(textSize.x, _size))); {
					// Draw line before title
					EditorGUILayout.BeginHorizontal(GUILayout.ExpandHeight(true)); {
						GUILayout.FlexibleSpace();
						GUILayout.Box(GUIContent.none, lineStyle, GUILayout.Width(_size));
						GUILayout.FlexibleSpace();
					} EditorGUILayout.EndHorizontal();
					
					// Draw label
					EditorGUILayout.BeginHorizontal(GUILayout.Height(textSize.y)); {
						GUILayout.FlexibleSpace();
						GUILayout.Label(_text, textStyle);
						GUILayout.FlexibleSpace();
					} EditorGUILayout.EndHorizontal();
					
					// Draw after before title
					EditorGUILayout.BeginHorizontal(GUILayout.ExpandHeight(true)); {
						GUILayout.FlexibleSpace();
						GUILayout.Box(GUIContent.none, lineStyle, GUILayout.Width(_size));
						GUILayout.FlexibleSpace();
					} EditorGUILayout.EndHorizontal();
				} EditorGUILayout.EndVertical();
			} else {
				EditorGUILayout.BeginHorizontal(GUILayout.ExpandWidth(true), GUILayout.Height(Mathf.Max(textSize.y, _size))); {
					// Draw line before title
					EditorGUILayout.BeginVertical(GUILayout.ExpandWidth(true)); {
						GUILayout.FlexibleSpace();
						GUILayout.Box(GUIContent.none, lineStyle, GUILayout.Height(_size));
						GUILayout.FlexibleSpace();
					} EditorGUILayout.EndVertical();

					// Draw label
					EditorGUILayout.BeginVertical(GUILayout.Width(textSize.x)); {
						GUILayout.FlexibleSpace();
						GUILayout.Label(_text, textStyle);
						GUILayout.FlexibleSpace();
					} EditorGUILayout.EndVertical();

					// Draw after before title
					EditorGUILayout.BeginVertical(GUILayout.ExpandWidth(true)); {
						GUILayout.FlexibleSpace();
						GUILayout.Box(GUIContent.none, lineStyle, GUILayout.Height(_size));
						GUILayout.FlexibleSpace();
					} EditorGUILayout.EndVertical();
				} EditorGUILayout.EndHorizontal();
			}
		}

		// Add spacing after
		GUILayout.Space(_spacing);
	}

	//------------------------------------------------------------------//
	// CUSTOM PROPERTY DRAWERS											//
	//------------------------------------------------------------------//
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
		}

		return totalHeight;
	}

	//------------------------------------------------------------------//
	// UTILS															//
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
}