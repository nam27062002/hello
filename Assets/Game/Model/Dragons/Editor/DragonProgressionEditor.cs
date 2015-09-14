// DragonLevelProgressionEditor.cs
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
[CustomPropertyDrawer(typeof(DragonProgression))]
public class DragonProgressionEditor : PropertyDrawer {
	//------------------------------------------------------------------//
	// MEMBERS															//
	//------------------------------------------------------------------//
	// Limits of the area to edit the curve. Width will be adjusted to the actual number of levels, height will be adjusted to selected scale value
	private Rect m_curveEditorLimits = new Rect(0, 0f, 9, float.PositiveInfinity);

	// Element heights
	private float[] m_heights = new float[4];	// scale selector, curve, levels header, levels

	// Styles and formatting
	// Useful to compute text widths and line heights
	private GUIStyle m_labelStyle = new GUIStyle(EditorStyles.label);
	private GUIStyle m_textAreaStyle = new GUIStyle(EditorStyles.textArea);
	private GUIStyle m_numberFieldStyle = new GUIStyle(EditorStyles.numberField);
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

		// Aux vars
		GUIContent content = new GUIContent();

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
		SerializedProperty maxXpProperty = _property.FindPropertyRelative("m_maxXp");

		// Take the list of xp to complete each level and extract the number of levels from it
		SerializedProperty levelsXpProperty = _property.FindPropertyRelative("m_levelsXp");
		int numLevels = levelsXpProperty.arraySize;
		int lastLevel = numLevels - 1;

		// Get the current progression curve
		SerializedProperty curveProperty = _property.FindPropertyRelative("m_curve");

		////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Max XP editor
		// Setup custom styles
		m_numberFieldStyle.alignment = TextAnchor.MiddleRight;
		m_labelStyle.alignment = TextAnchor.MiddleRight;

		// Adjust label size to text
		content.text = "Max XP:";
		EditorGUIUtility.labelWidth = m_labelStyle.CalcSize(content).x;

		// Initialize position
		pos.height = m_heights[0];

		// Draw it and get the new value
		float newMaxXp = EditorGUI.FloatField(pos, content, maxXpProperty.floatValue, m_numberFieldStyle);

		// Check value - can't be negative - and store it
		if(newMaxXp < 0) newMaxXp = 0;
		maxXpProperty.floatValue = newMaxXp;

		////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Draw the curve property
		// If the curve hasn't been initialized, do it now
		AnimationCurve curve = curveProperty.animationCurveValue;
		if(curve.length == 0) {
			// Create both keys for level 0 and N with arbitrary initial values
			curve.AddKey(0f, 1f);
			curve.AddKey(lastLevel, 10f);
			curveProperty.animationCurveValue = curve;
		}
		
		// Adjust curve editor limits
		m_curveEditorLimits.width = lastLevel;
		m_curveEditorLimits.height = maxXpProperty.floatValue;

		// Initialize position
		pos.x = contentPos.x;
		pos.y = pos.yMax + m_padding;
		pos.width = contentPos.width;
		pos.height = m_heights[1];

		// Draw it and get the new value
		AnimationCurve editedCurve = EditorGUI.CurveField(pos, curve, Color.green, m_curveEditorLimits);

		// Make sure all the changes are valid:
		// a) If there are less than 2 keys, ignore the changes
		Keyframe[] newKeys = editedCurve.keys;	// "keys" returns a copy, so we cannot edit it directly
		if(newKeys.Length < 2) {
			// [AOC] Damn bugged Unity Curve Editor deletes the key anyway in the window. To force a refresh, we must assign a new curve and then restore the original one -_-'
			curveProperty.animationCurveValue = AnimationCurve.Linear(0, 1, lastLevel, 100);
			curveProperty.animationCurveValue = curve;
		} else {
			// Iterate all keys
			for(int i = 0; i < newKeys.Length; i++) {
				// b) None of the keys should be before t0 or after tN
				if(newKeys[i].time < 0f) {
					newKeys[i].time = 0f;
				} else if(newKeys[i].time > lastLevel) {
					newKeys[i].time = lastLevel;
				}

				// c) No negative values either
				if(newKeys[i].value < 0f) {
					newKeys[i].value = 0f;
				}
			}
			
			// d) First and last keys can't be modified - they are level 0 and max level
			newKeys[0].time = 0f;
			newKeys[0].value = 0f;
			newKeys[editedCurve.length - 1].time = lastLevel;
			newKeys[editedCurve.length - 1].value = newMaxXp;
			
			// All checks passed, update keys and xp thresholds for every level
			editedCurve.keys = newKeys;
			for(int i = 0; i < numLevels; i++) {
				levelsXpProperty.GetArrayElementAtIndex(i).floatValue = editedCurve.Evaluate(i);	// Get the xp at level i (0, 1, 2,... 8, 9)
			}
			
			// Store edited curve
			curveProperty.animationCurveValue = editedCurve;
		}

		////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Draw header row for level values
		// Setup custom styles
		m_labelStyle.alignment = TextAnchor.MiddleRight;
		m_labelStyle.fontStyle = FontStyle.Bold;

		// Initialize position
		pos.y = pos.yMax + m_padding;
		pos.width = contentPos.width/3 - m_padding;
		pos.height = m_heights[2];

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
		pos.height = m_heights[3]/numLevels;

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
			EditorGUI.SelectableLabel(pos, value.ToString("N0"), m_textAreaStyle);	// [AOC] Little trick to have an uneditable text area: use a label with a text area style

			// Show a warning if the difference is lower than the previous difference
			diff = value - prevValue;
			if(diff < 0 || diff < prevDiff) {
				m_textAreaStyle.normal.textColor = new Color(1f, 0.5f, 0f);
			} else {
				m_textAreaStyle.normal.textColor = EditorStyles.label.normal.textColor;
			}

			// Draw the difference with the previous level
			pos.x = pos.xMax + m_padding;
			EditorGUI.SelectableLabel(pos, (value - prevValue).ToString("N0"), m_textAreaStyle);	// [AOC] Little trick to have an uneditable text area: use a label with a text area style

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
		// 1. Scale selector
		m_heights[0] = m_labelStyle.lineHeight + m_padding;

		// 2. Graph
		m_heights[1] = (m_labelStyle.lineHeight + m_padding) * 8;

		// 3. Levels header
		m_heights[2] = m_labelStyle.lineHeight;

		// 4. Levels display
		SerializedProperty levelsXpProperty = _property.FindPropertyRelative("m_levelsXp");
		int numLevels = levelsXpProperty.arraySize;
		m_heights[3] = Mathf.Max(m_labelStyle.lineHeight, m_textAreaStyle.lineHeight) * numLevels;	// One line per level, since each level has both a text area and a label, choose the maximum line height

		// Add height up
		float height = 0f;
		for(int i = 0; i < m_heights.Length; i++) {
			height += m_heights[i];
			height += m_padding;
		}
		
		return height;
	}
}