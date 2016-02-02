// SeparatorAttributeEditor.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 28/08/2015.
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
/// Drawer for the Separator custom attribute.
/// </summary>
[CustomPropertyDrawer(typeof(SeparatorAttribute))]
public class SeparatorAttributeEditor : DecoratorDrawer {
	//------------------------------------------------------------------//
	// CONSTANTS														//
	//------------------------------------------------------------------//

	//------------------------------------------------------------------//
	// PROPERTIES														//
	//------------------------------------------------------------------//
	private SeparatorAttribute separator {
		get { return attribute as SeparatorAttribute; }
	}

	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Draw the decorator in the inspector.
	/// </summary>
	/// <param name="_area">The area designated by the inspector to draw this decoration.</param>
	public override void OnGUI(Rect _area) {
		// Respect indentation!
		Rect indentedArea = EditorGUI.IndentedRect(_area);

		// Use static method
		DrawSeparator(indentedArea, separator);
	}

	/// <summary>a
	/// Gets the height of the decorator drawer.
	/// </summary>
	/// <returns>The height required by this decorator drawer.</returns>
	public override float GetHeight() {
		// Add extra room at the bottom for spacing with the next element
		return separator.m_size + EditorGUIUtility.standardVerticalSpacing;
	}

	//------------------------------------------------------------------//
	// STATIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Draw a separator in an editor layout GUI.
	/// </summary>
	/// <param name="_separator">The separator to be drawn.</param>
	/// <param name="_orientation">Orientation of the separator. Use horizontal separators for vertical layouts and viceversa.</param>
	/// <param name="_thickness">Size of the separator line, in pixels. Will override separator's size property if bigger.</param>
	public static void DrawSeparator(SeparatorAttribute _separator, SeparatorAttribute.Orientation _orientation = SeparatorAttribute.Orientation.HORIZONTAL, float _thickness = 1f) {
		// Initialized style for a line
		// [AOC] We will be drawing a box actually, so copy some values from the box style
		GUIStyle lineStyle = new GUIStyle();
		lineStyle.normal.background = Texture2DExt.Create(2, 2, _separator.m_color);
		lineStyle.margin = EditorStyles.helpBox.margin;
		lineStyle.padding = EditorStyles.helpBox.padding;

		// Compute space before/after the separator
		// If thickness is bigger than separator's size, override size
		float spacing = (Mathf.Max(_separator.m_size, _thickness) - _thickness)/2f;
		
		// Add spacing before
		GUILayout.Space(spacing);
		
		// Do we have a title?
		if(_separator.m_text == "") {
			// No! Single line
			// Vertical or horizontal?
			if(_orientation == SeparatorAttribute.Orientation.VERTICAL) {
				// Draw separator
				GUILayout.Box(GUIContent.none, lineStyle, GUILayout.ExpandHeight(true), GUILayout.Width(_thickness));
			} else {
				// Draw separator
				GUILayout.Box(GUIContent.none, lineStyle, GUILayout.ExpandWidth(true), GUILayout.Height(_thickness));
			}
		} else {
			// Yes! Slightly more complicated
			// Create style and content for the text
			GUIContent textContent = new GUIContent(_separator.m_text);
			GUIStyle textStyle = new GUIStyle(EditorStyles.label);	// Default label style
			textStyle.alignment = TextAnchor.MiddleCenter;	// Alignment!
			textStyle.fontStyle = FontStyle.Italic;
			textStyle.normal.textColor = Colors.gray;
			Vector2 textSize = textStyle.CalcSize(textContent);
			
			// We need to create a layout with flexible spaces to each part so the line and the title are aligned
			// Vertical or horizontal?
			if(_orientation == SeparatorAttribute.Orientation.VERTICAL) {
				// Draw separator
				EditorGUILayout.BeginVertical(GUILayout.ExpandHeight(true), GUILayout.Width(Mathf.Max(textSize.x, _thickness))); {
					// Draw line before title
					EditorGUILayout.BeginHorizontal(GUILayout.ExpandHeight(true)); {
						GUILayout.FlexibleSpace();
						GUILayout.Box(GUIContent.none, lineStyle, GUILayout.Width(_thickness));
						GUILayout.FlexibleSpace();
					} EditorGUILayout.EndHorizontal();
					
					// Draw label
					EditorGUILayout.BeginHorizontal(GUILayout.Height(textSize.y)); {
						GUILayout.FlexibleSpace();
						GUILayout.Label(_separator.m_text, textStyle);
						GUILayout.FlexibleSpace();
					} EditorGUILayout.EndHorizontal();
					
					// Draw after before title
					EditorGUILayout.BeginHorizontal(GUILayout.ExpandHeight(true)); {
						GUILayout.FlexibleSpace();
						GUILayout.Box(GUIContent.none, lineStyle, GUILayout.Width(_thickness));
						GUILayout.FlexibleSpace();
					} EditorGUILayout.EndHorizontal();
				} EditorGUILayout.EndVertical();
			} else {
				EditorGUILayout.BeginHorizontal(GUILayout.ExpandWidth(true), GUILayout.Height(Mathf.Max(textSize.y, _thickness))); {
					// Draw line before title
					EditorGUILayout.BeginVertical(GUILayout.ExpandWidth(true)); {
						GUILayout.FlexibleSpace();
						GUILayout.Box(GUIContent.none, lineStyle, GUILayout.Height(_thickness));
						GUILayout.FlexibleSpace();
					} EditorGUILayout.EndVertical();
					
					// Draw label
					EditorGUILayout.BeginVertical(GUILayout.Width(textSize.x)); {
						GUILayout.FlexibleSpace();
						GUILayout.Label(_separator.m_text, textStyle);
						GUILayout.FlexibleSpace();
					} EditorGUILayout.EndVertical();
					
					// Draw after before title
					EditorGUILayout.BeginVertical(GUILayout.ExpandWidth(true)); {
						GUILayout.FlexibleSpace();
						GUILayout.Box(GUIContent.none, lineStyle, GUILayout.Height(_thickness));
						GUILayout.FlexibleSpace();
					} EditorGUILayout.EndVertical();
				} EditorGUILayout.EndHorizontal();
			}
		}
		
		// Add spacing after
		GUILayout.Space(spacing);
	}

	/// <summary>
	/// Draws a separator given a manual position rectangle.
	/// </summary>
	/// <returns>The actual height required by the field</returns>
	/// <param name="_pos">The position rectangle where the separator should be drawn.</param>
	/// <param name="_separator">The separator to be drawn.</param>
	public static float DrawSeparator(Rect _pos, SeparatorAttribute _separator) {
		// Initialized style for a line
		// [AOC] We will be drawing a box actually, so copy some values from the box style
		GUIStyle lineStyle = new GUIStyle();
		lineStyle.normal.background = Texture2DExt.Create(2, 2, _separator.m_color);
		lineStyle.margin = EditorStyles.helpBox.margin;
		lineStyle.padding = EditorStyles.helpBox.padding;
		
		// Store separator size
		_pos.height = _separator.m_size;
		
		// Aux helper to draw lines
		Rect lineBounds = _pos;
		lineBounds.height = 1f;
		lineBounds.y = _pos.y + _pos.height/2f - lineBounds.height/2f;	// Vertically centered
		
		// Do we have title?
		if(_separator.m_text == "") {
			// No! Draw a single line from left to right
			lineBounds.x = _pos.x;
			lineBounds.width = _pos.width;
			GUI.Box(lineBounds, "", lineStyle);
		} else {
			// Yes!
			// Compute title's width
			GUIContent titleContent = new GUIContent(_separator.m_text);
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
			GUI.Label(titleBounds, _separator.m_text, titleStyle);
			
			// Draw line at the right of the title
			lineBounds.x = titleBounds.xMax;	// Concatenate to the title label
			lineBounds.width = _pos.width/2f - titleWidth/2f;
			GUI.Box(lineBounds, "", lineStyle);
		}
		
		return _pos.height;
	}
}

