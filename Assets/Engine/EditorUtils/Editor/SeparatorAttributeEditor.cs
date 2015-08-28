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
		// Aux helper to draw lines
		Rect lineBounds = _area;
		lineBounds.height = 1f;
		lineBounds.y = _area.y + _area.height/2f - lineBounds.height/2f;	// Vertically centered

		// Do we have title?
		if(separator.m_title == "") {
			// No! Draw a single line from left to right
			lineBounds.x = _area.x;
			lineBounds.width = _area.width;
			GUI.Box(lineBounds, "");
		} else {
			// Yes!
			// Compute title's width
			GUIContent titleContent = new GUIContent(separator.m_title);
			GUIStyle titleStyle = GUI.skin.label;	// Default label style
			float titleWidth = titleStyle.CalcSize(titleContent).x;
			titleWidth += 10f;	// Add some spacing around the title

			// Draw line at the left of the title
			lineBounds.x = _area.x;
			lineBounds.width = _area.width/2f - titleWidth/2f;
			GUI.Box(lineBounds, "");

			// Draw title
			Rect titleBounds = _area;	// Using whole area's height
			titleBounds.x = lineBounds.xMax;	// Concatenate to the line we just draw
			titleBounds.width = titleWidth;
			titleStyle.alignment = TextAnchor.MiddleCenter;	// Alignment!
			GUI.Label(titleBounds, separator.m_title, titleStyle);

			// Draw line at the right of the title
			lineBounds.x = titleBounds.xMax;	// Concatenate to the title label
			lineBounds.width = _area.width/2f - titleWidth/2f;
			GUI.Box(lineBounds, "");
		}
	}

	/// <summary>
	/// Gets the height of the decorator drawer.
	/// </summary>
	/// <returns>The height required by this decorator drawer.</returns>
	public override float GetHeight() {
		// Very short
		return 30f;
	}
}

