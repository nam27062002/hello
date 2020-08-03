// BezierCurvePointModifierEditor.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 08/02/2018.
// Copyright (c) 2018 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;
using UnityEditor;
using DG.Tweening;
using System.Collections.Generic;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// Custom editor for the Camera Test BezierCurvePointModifier class.
/// </summary>
[CustomPropertyDrawer(typeof(BezierCurvePointModifier.PointData))]
public class BezierCurvePointModifierEditor : ExtendedPropertyDrawer {
	//------------------------------------------------------------------//
	// CONSTANTS														//
	//------------------------------------------------------------------//

	//------------------------------------------------------------------//
	// MEMBERS															//
	//------------------------------------------------------------------//

	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Default constructor.
	/// </summary>
	public BezierCurvePointModifierEditor() {
		// Nothing to do
	}

	//------------------------------------------------------------------//
	// PARENT IMPLEMENTATION											//
	//------------------------------------------------------------------//
	/// <summary>
	/// A change has occurred in the inspector, draw the property and store new values.
	/// Use the m_pos member as position reference and update it using AdvancePos() method.
	/// </summary>
	/// <param name="_property">The property we're drawing.</param>
	/// <param name="_label">The label of the property.</param>
	override protected void OnGUIImpl(SerializedProperty _property, GUIContent _label) {
		// Aux vars
		m_pos.height = EditorStyles.popup.lineHeight + 5;	// [AOC] Default popup field height + some margin
		SerializedProperty curveProp = m_rootProperty.FindPropertyRelative("curve");
		SerializedProperty pointIdProp = m_rootProperty.FindPropertyRelative("pointId");

		// Indent in
		Rect contentRect = new Rect(m_pos);
		int indentLevelBackup = EditorGUI.indentLevel;
		EditorGUI.indentLevel++;

		// Target Curve
		Rect curvePos = new Rect(contentRect);
		curvePos.width = contentRect.width * 0.5f;
		EditorGUI.PropertyField(curvePos, curveProp, GUIContent.none, true);

		// Point ID
		EditorGUI.indentLevel = 0;
		BezierCurve curve = (BezierCurve)curveProp.objectReferenceValue;

		// Figure out rect
		Rect pointIdPos = new Rect(contentRect);
		pointIdPos.x = curvePos.xMax;
		pointIdPos.width = contentRect.width - curvePos.width;

		// If no curve is assigned, show an info message instead
		if(curve == null) {
			EditorGUI.HelpBox(pointIdPos, "No curve selected!", MessageType.Info);
		}

		// Show a list of all the named points in the curve
		else {
			int selectedIdx = -1;
			List<string> optionsList = new List<string>();
			for(int i = 0; i < curve.points.Count; ++i) {
				// Skip points with no custom name
				if(!string.IsNullOrEmpty(curve.points[i].name)) {
					// Add option
					optionsList.Add(curve.points[i].name);

					// Is it the current selected value?
					if(string.Equals(curve.points[i].name, pointIdProp.stringValue)) {
						selectedIdx = optionsList.Count - 1;
					}
				}
			}


			// If the curve has no named points, show an error message instead
			if(optionsList.Count == 0) {
				// Error message
				EditorGUI.HelpBox(pointIdPos, "Selected curve has no NAMED control points!", MessageType.Error);
			} else {
				// Display the list and store new value
				int newSelectedIdx = EditorGUI.Popup(pointIdPos, string.Empty, selectedIdx, optionsList.ToArray());
				if(selectedIdx != newSelectedIdx) {
					pointIdProp.stringValue = optionsList[newSelectedIdx];
				}
			}
		}

		// End
		EditorGUI.indentLevel = indentLevelBackup;
		AdvancePos();
	}

	/// <summary>
	/// Optionally override to give a custom label for this property field.
	/// </summary>
	/// <returns>The new label for this property.</returns>
	/// <param name="_property">The property we're drawing.</param>
	/// <param name="_defaultLabel">The default label for this property.</param>
	override protected GUIContent GetLabel(SerializedProperty _property, GUIContent _defaultLabel) {
		return _defaultLabel;
	}
}