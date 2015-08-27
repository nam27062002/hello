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
/// Custom editor for the Range class.
/// </summary>
[CustomPropertyDrawer(typeof(Range))]
public class RangeEditor : PropertyDrawer {
	//------------------------------------------------------------------//
	// CONSTANTS														//
	//------------------------------------------------------------------//
	public enum EZoomLevel { x1, x10, x100, x1000, x10000, x100000, x1000000 };
	private static readonly float[] ZOOM_LEVELS = {1f, 10f, 100f, 1000f, 10000f, 100000f, 1000000f};

	//------------------------------------------------------------------//
	// MEMBERS															//
	//------------------------------------------------------------------//
	private Range limits = null;
	private EZoomLevel zoom = EZoomLevel.x1;

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
		GUIStyle style = new GUIStyle();	// Default style

		// Group all
		_label = EditorGUI.BeginProperty(_position, _label, _property);
		EditorGUI.BeginChangeCheck ();

		// Retrieve min and max properties and create a temp range to work with
		SerializedProperty min = _property.FindPropertyRelative("min");
		SerializedProperty max = _property.FindPropertyRelative("max");
		float minValue = min.floatValue;
		float maxValue = max.floatValue;

		// Initialize slider's limits (only the first time) and select the most adequate zoom level
		if(limits == null) {
			// Find out the zoom level that matches our range best
			Range tempRange = new Range(minValue, maxValue);
			float magnitude = MathUtils.GetMagnitude(tempRange.distance);
			for(int i = 0; i < ZOOM_LEVELS.Length; i++) {
				if(magnitude <= ZOOM_LEVELS[i]) {
					zoom = (EZoomLevel)i;
					break;
				}
			}

			// Compute slider limits
			limits = new Range();
			UpdateLimits(minValue, maxValue);
		}

		// Draw label and initialize position helpers
		Rect contentRect = EditorGUI.PrefixLabel(_position, _label);	// Remaining space for the content
		Rect cursor = contentRect;
		//cursor.height = contentRect.height/2f;	// Size of a single line

		////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Draw min value
		EditorGUI.BeginChangeCheck();

		cursor.width = contentRect.width/2 - padding/2;
		content.text = "min";
		EditorGUIUtility.labelWidth = style.CalcSize(content).x + padding;	// Adjust label to "min" word
		minValue = EditorGUI.FloatField(cursor, content, minValue);

		if(EditorGUI.EndChangeCheck()) {
			// Check new value validity, cap if invalid
			if(minValue > max.floatValue) {
				minValue = max.floatValue;
			}
			min.floatValue = minValue;

			// Update limits
			UpdateLimits(minValue, maxValue);
		}

		////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Draw max value
		EditorGUI.BeginChangeCheck();

		cursor.x += cursor.width + padding;
		cursor.width = contentRect.width/2 - padding/2;
		content.text = "max";
		EditorGUIUtility.labelWidth = style.CalcSize(content).x + padding;	// Adjust label to "max" word
		maxValue = EditorGUI.FloatField(cursor, content, maxValue);

		if(EditorGUI.EndChangeCheck()) {
			// Check new value validity, cap if invalid
			if(maxValue < min.floatValue) {
				maxValue = min.floatValue;
			}
			max.floatValue = maxValue;

			// Update limits
			UpdateLimits(minValue, maxValue);
		}
		/*
		////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Zoom Selector
		EditorGUI.BeginChangeCheck();

		cursor.x = _position.x + 40f;	// A bit indented
		cursor.y = contentRect.y + contentRect.height/2f;	// New line
		cursor.width = contentRect.x - cursor.x - padding;
		content.text = "zoom";
		EditorGUIUtility.labelWidth = style.CalcSize(content).x + padding;	// Adjust label to "zoom" word
		zoom = (EZoomLevel)EditorGUI.EnumPopup(cursor, content, zoom);

		if(EditorGUI.EndChangeCheck()) {
			UpdateLimits(minValue, maxValue);
		}

		////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Left limit label
		cursor.x = contentRect.x;
		content.text = limits.min.ToString();
		style.alignment = TextAnchor.MiddleRight;
		cursor.width = style.CalcSize(content).x;
		EditorGUI.LabelField(cursor, content, style);

		float leftLimitLabelWidth = cursor.width;	// Store for future usage

		////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Right limit label
		content.text = limits.max.ToString();
		style.alignment = TextAnchor.MiddleLeft;
		cursor.width = style.CalcSize(content).x;
		cursor.x = contentRect.xMax - cursor.width;
		EditorGUI.LabelField(cursor, content, style);

		////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Draw slider
		EditorGUI.BeginChangeCheck();

		cursor.x = contentRect.x + leftLimitLabelWidth + padding;	// After first label
		cursor.width = contentRect.width - leftLimitLabelWidth - cursor.width - padding * 2;	// Subtract limits labels
		EditorGUI.MinMaxSlider(cursor, ref minValue, ref maxValue, limits.min, limits.max);

		if(EditorGUI.EndChangeCheck()) {
			// Update values in the property
			min.floatValue = minValue;
			max.floatValue = maxValue;
		}
		*/

		////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// DONE!!
		EditorGUI.EndProperty ();
	}

	/// <summary>
	/// Gets the height of the property drawer.
	/// </summary>
	/// <returns>The height required by this property drawer.</returns>
	/// <param name="_property">The property we're drawing.</param>
	/// <param name="_label">The label of the property.</param>
	/*public override float GetPropertyHeight(SerializedProperty _property, GUIContent _label) {
		// Let's get a double line
		return base.GetPropertyHeight(_property, _label) * 2f;
	}*/

	/// <summary>
	/// Update the limits of the slider based on current zoom and range's min and max.
	/// </summary>
	/// <param name="_minValue">Current range's min value.</param>
	/// <param name="_maxValue">Current range's max value.</param>
	private void UpdateLimits(float _minValue, float _maxValue) {
		limits.min = MathUtils.PreviousMultiple(_minValue, ZOOM_LEVELS[(int)zoom]);
		limits.max = MathUtils.NextMultiple(_maxValue, ZOOM_LEVELS[(int)zoom]);
	}
}