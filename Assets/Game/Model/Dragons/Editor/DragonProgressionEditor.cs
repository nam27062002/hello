﻿// DragonLevelProgressionEditor.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 07/09/2015.
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
/// Custom editor for the DragonLevelProgression class.
/// </summary>
//[CustomPropertyDrawer(typeof(DragonProgression))]
public class DragonProgressionEditor : PropertyDrawer {
	//------------------------------------------------------------------//
	// MEMBERS															//
	//------------------------------------------------------------------//
	// Curve
	private AnimationCurve m_curve = null;

	// Limits of the area to edit the curve. Width will be adjusted to the actual number of levels, height will be adjusted to selected scale value
	private Rect m_curveEditorLimits = new Rect(0, 0f, 9, float.PositiveInfinity);

	// Element heights
	private float[] m_heights = new float[3];	// curve, levels header, levels

	// Styles and formatting
	// Useful to compute text widths and line heights
	private GUIStyle m_labelStyle = new GUIStyle(EditorStyles.label);
	private GUIStyle m_textAreaStyle = new GUIStyle(EditorStyles.textArea);
	private float m_padding = 5f;	// Spacing between elements

	//------------------------------------------------------------------//
	// METHODS															//
	//------------------------------------------------------------------//
	public DragonProgressionEditor() {
		// Nothing to do
	}

	/// <summary>
	/// A change has occurred in the inspector, draw the property and store new values.
	/// </summary>
	/// <param name="_position">The area in the inspector assigned to draw this property.</param>
	/// <param name="_property">The property we're drawing.</param>
	/// <param name="_label">The label of the property.</param>
	public override void OnGUI(Rect _position, SerializedProperty _property, GUIContent _label) {
		// [AOC] Unfortunately, custom Property Drawers do not support using EditorGUILayout, so we must keep 
		//		 manual track of positions and heights.

		// Define an area for the content
		// [AOC] This is usually done when drawing the label via the EditorGUI.PrefixLabel method, but since we won't use a label for this property, do it manually
		Rect contentPos = EditorGUI.IndentedRect(_position);
		Rect pos = new Rect(contentPos);

		// Reset indentation level - we don't want our labels to be affected by it
		int indentLevelBackup = EditorGUI.indentLevel;
		EditorGUI.indentLevel = 0;

		// Using BeginProperty / EndProperty on the parent property means that prefab override logic works on the entire property.
		EditorGUI.BeginProperty(_position, _label, _property);

		////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Get references to the class members and initialize some more aux vars
		// Take the list of xp to complete each level and extract the number of levels from it
		SerializedProperty levelsXpProperty = _property.FindPropertyRelative("m_levelsXp");
		int numLevels = levelsXpProperty.arraySize;
		int lastLevel = numLevels - 1;

		////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Draw the curve
		// Create the curve with the current level values
		Keyframe[] keys = new Keyframe[numLevels];
		for(int i = 0; i < numLevels; i++) {
			// Have a key for every level
			keys[i] = new Keyframe(i, levelsXpProperty.GetArrayElementAtIndex(i).floatValue, Mathf.Tan(Mathf.Deg2Rad * (-30f)), Mathf.Tan(Mathf.Deg2Rad * 60f));
		}

		// Create curve and make sure it has the proper shape
		m_curve = new AnimationCurve(keys);
		for(int i = 0; i < numLevels; i++) {
			m_curve.SmoothTangents(i, 0);
		}

		// Initialize position
		//pos.x = contentPos.x;
		//pos.y = pos.yMax + m_padding;
		//pos.width = contentPos.width;
		pos.height = m_heights[0];

		// Draw the curve field
		// Adjust curve editor limits
		m_curveEditorLimits.width = lastLevel;
		m_curveEditorLimits.height = keys[lastLevel].value;
		EditorGUI.CurveField(pos, m_curve, Color.green, m_curveEditorLimits);

		////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Draw header row for level values
		// Setup custom styles
		m_labelStyle.alignment = TextAnchor.MiddleRight;
		m_labelStyle.fontStyle = FontStyle.Bold;

		// Initialize position
		pos.y = pos.yMax + m_padding;
		pos.width = contentPos.width/3 - m_padding;
		pos.height = m_heights[1];

		// Draw them
		pos.x = contentPos.x + pos.width + m_padding;	// Skip first column
		EditorGUI.LabelField(pos, "XP", m_labelStyle);

		pos.x = pos.xMax + m_padding;
		EditorGUI.LabelField(pos, "XP diff", m_labelStyle);

		////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Draw the actual level values
		// Setup custom styles
		m_labelStyle.alignment = TextAnchor.MiddleRight;
		m_labelStyle.fontStyle = FontStyle.Normal;
		m_textAreaStyle.alignment = TextAnchor.MiddleRight;

		// Initialize position
		pos.y = pos.yMax;
		pos.width = contentPos.width/3 - m_padding;		// 3 columns
		pos.height = m_heights[2]/numLevels;

		// Draw them
		float value = 0;
		float prevValue = 0;
		float diff = 0;
		float prevDiff = 0;
		for(int i = 0; i < numLevels; i++) {
			// Draw the level number
			pos.x = contentPos.x;
			EditorGUI.LabelField(pos, "Level " + i + ": ", m_labelStyle);

			// If the value is lower than the previous level (or negative), highlight it as an error
			value = levelsXpProperty.GetArrayElementAtIndex(i).floatValue;
			if(value < 0 || value < prevValue) {
				m_textAreaStyle.normal.textColor = Color.red;
			} else {
				m_textAreaStyle.normal.textColor = EditorStyles.label.normal.textColor;
			}

			// Draw the xp for that level
			pos.x = pos.xMax + m_padding;
			value = (float)EditorGUI.IntField(pos, (int)value, m_textAreaStyle);

			// Store new value
			levelsXpProperty.GetArrayElementAtIndex(i).floatValue = value;

			// Show a warning if the difference is lower than the previous difference
			diff = value - prevValue;
			if(diff < 0 || diff < prevDiff) {
				m_textAreaStyle.normal.textColor = new Color(1f, 0.5f, 0f);
			} else {
				m_textAreaStyle.normal.textColor = EditorStyles.label.normal.textColor;
			}

			// Draw the difference with the previous level
			pos.x = pos.xMax + m_padding;
			//EditorGUI.SelectableLabel(pos, (value - prevValue).ToString("N0"), m_textAreaStyle);	// [AOC] Little trick to have an uneditable text area: use a label with a text area style
			EditorGUI.LabelField(pos, (value - prevValue).ToString("N0"), m_textAreaStyle);	// [AOC] Little trick to have an uneditable text area: use a label with a text area style

			// Advance to following line and reset X
			pos.y = pos.yMax;

			// Store value
			prevValue = value;
			prevDiff = diff;
		}

		////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// End the property group
		EditorGUI.EndProperty();

		// Restore indentation level
		EditorGUI.indentLevel = indentLevelBackup;
	}
	
	/// <summary>
	/// Gets the height of the property drawer.
	/// </summary>
	/// <returns>The height required by this property drawer.</returns>
	/// <param name="_property">The property we're drawing.</param>
	/// <param name="_label">The label of the property.</param>
	public override float GetPropertyHeight(SerializedProperty _property, GUIContent _label) {
		// 1. Graph
		m_heights[0] = (m_labelStyle.lineHeight + m_padding) * 10;

		// 2. Levels header
		m_heights[1] = m_labelStyle.lineHeight;

		// 3. Levels display
		SerializedProperty levelsXpProperty = _property.FindPropertyRelative("m_levelsXp");
		int numLevels = levelsXpProperty.arraySize;
		m_heights[2] = Mathf.Max(m_labelStyle.lineHeight, m_textAreaStyle.lineHeight) * numLevels;	// One line per level, since each level has both a text area and a label, choose the maximum line height

		// Add height up
		float height = 0f;
		for(int i = 0; i < m_heights.Length; i++) {
			height += m_heights[i];
			height += m_padding;
		}
		
		return height;
	}
}