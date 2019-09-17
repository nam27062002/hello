// CurvedTextEditor.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 31/10/2017.
// Copyright (c) 2017 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using UnityEditor;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Custom editor for the CurvedText class.
/// </summary>
[CustomEditor(typeof(CurvedText), true)]	// True to be used by heir classes as well
[CanEditMultipleObjects]
public class CurvedTextEditor : Editor {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Casted target object
	private CurvedText m_targetCurvedText = null;
	private SerializedObject m_serializedObject = null;		// To be able to save serialized object from OnSceneGUI

	// Important properties
	private SerializedProperty m_curveProp = null;
	private SerializedProperty m_curveScaleProp = null;
	private SerializedProperty m_useCustomReferenceSizeProp = null;
	private SerializedProperty m_customReferenceSizeProp = null;

	// Internal
	private RectTransform m_rectTransform = null;
	private Rect m_curveBounds = new Rect();


	//------------------------------------------------------------------------//
	// METHODS																  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// The editor has been enabled - target object selected.
	/// </summary>
	private void OnEnable() {
		// Get target object
		m_targetCurvedText = target as CurvedText;
		m_rectTransform = m_targetCurvedText.text.rectTransform;

		// Get important properties
		m_curveProp = serializedObject.FindProperty("m_curve");
		m_curveScaleProp = serializedObject.FindProperty("m_curveScale");
		m_useCustomReferenceSizeProp = serializedObject.FindProperty("m_useCustomReferenceSize");
		m_customReferenceSizeProp = serializedObject.FindProperty("m_customReferenceSize");
	}

	/// <summary>
	/// The editor has been disabled - target object unselected.
	/// </summary>
	private void OnDisable() {
		// Clear target object
		m_targetCurvedText = null;
	}

	/// <summary>
	/// Draw the inspector.
	/// </summary>
	public override void OnInspectorGUI() {
		// Instead of modifying script variables directly, it's advantageous to use the SerializedObject and 
		// SerializedProperty system to edit them, since this automatically handles private fields, multi-object 
		// editing, undo, and prefab overrides.

		// Update the serialized object - always do this in the beginning of OnInspectorGUI.
		m_serializedObject = serializedObject;
		serializedObject.Update();

		// Loop through all serialized properties and work with special ones
		SerializedProperty p = serializedObject.GetIterator();
		p.Next(true);	// To get first element
		do {
			// Properties requiring special treatment
			// Unity's "script" property
			if(p.name == "m_Script") {
				// Draw the property, disabled
				bool wasEnabled = GUI.enabled;
				GUI.enabled = false;
				EditorGUILayout.PropertyField(p, true);
				GUI.enabled = wasEnabled;
			}

			else if(p.name == m_useCustomReferenceSizeProp.name) {
				// Comment
				EditorGUILayout.Space();
				GUILayout.Label("Use custom reference size if you want the curvature to be fixed, otherwise the curvature will depend on the text width.", CustomEditorStyles.commentLabelLeft);

				// Toggle and put custom reference size in the same line
				EditorGUILayout.BeginHorizontal(); {
					p.boolValue = GUILayout.Toggle(p.boolValue, GUIContent.none, GUILayout.Width(10f));
					GUI.enabled = p.boolValue;
					m_customReferenceSizeProp.floatValue = EditorGUILayout.FloatField("Custom Reference Size", m_customReferenceSizeProp.floatValue);
					GUI.enabled = true;
				} EditorGUILayoutExt.EndHorizontalSafe();
			}

			// Properties we don't want to show
			else if(p.name == "m_ObjectHideFlags"
				|| p.name == m_customReferenceSizeProp.name) {
				// Do nothing
			}

			// Default property display
			else {
				EditorGUILayout.PropertyField(p, true);
			}
		} while(p.NextVisible(false));		// Only direct children, not grand-children (will be drawn by default if using the default EditorGUI.PropertyField)

		// Apply changes to the serialized object - always do this in the end of OnInspectorGUI.
		serializedObject.ApplyModifiedProperties();

		// For some reason Curved Text aren't refreshing so well in edit mode :(
		// Put a button to force a refresh
		EditorGUILayout.Space();
		if(GUILayout.Button("Refresh", GUILayout.Height(30f))) {
			EditorUtility.SetDirty(m_targetCurvedText.text);
		}
	}

	/// <summary>
	/// The scene is being refreshed.
	/// </summary>
	public void OnSceneGUI() {
		if(m_serializedObject == null) return;

		// Aux vars
		Vector3 lossyScale = m_rectTransform.lossyScale;

		// Compute bounds used as reference for the curve in X: different logic if using custom reference size or not
		Range boundsX;
		Vector3 centerPoint;
		if(m_useCustomReferenceSizeProp.boolValue) {
			// Using custom size as reference for the curve: use world position to center the custom reference rect
			boundsX = new Range(
				m_rectTransform.position.x - m_customReferenceSizeProp.floatValue/2f * lossyScale.x,
				m_rectTransform.position.x + m_customReferenceSizeProp.floatValue/2f * lossyScale.x
			);

			centerPoint = new Vector3(
				boundsX.center, 
				m_rectTransform.position.y, 
				m_rectTransform.position.z
			);
		} else {
			// Using actual text size as reference for the curve
			boundsX = new Range(
				m_rectTransform.position.x + m_targetCurvedText.text.bounds.min.x * lossyScale.x,
				m_rectTransform.position.x + m_targetCurvedText.text.bounds.max.x * lossyScale.x
			);

			centerPoint = new Vector3(
				boundsX.center, 
				m_rectTransform.position.y + m_targetCurvedText.text.bounds.center.y * lossyScale.y,
				m_rectTransform.position.z
			);
		}

		// In order to have the same curvature regardless of the number of characters, compute correction factor
		float correctedCurveScale = m_curveScaleProp.floatValue * 0.1f * boundsX.distance;	// [AOC] In order to have easier tunable numbers in the inspector, curve scale is multiplier by 10

		// Compute a decent amount of points so the line looks smooth (base it on distance)
		float unitsPerPoint = 0.1f;
		int pointCount = Mathf.Max(Mathf.CeilToInt(boundsX.distance / unitsPerPoint) + 1, 5);	// At least 5
		Vector3[] points = new Vector3[pointCount];
		Range deltaYRange = new Range(0, 0);
		for(int i = 0; i < pointCount; ++i) {
			float deltaX = Mathf.InverseLerp(0, pointCount - 1, i);
			float deltaY = m_curveProp.animationCurveValue.Evaluate(deltaX);
			points[i] = new Vector3(
				boundsX.Lerp(deltaX),
				centerPoint.y + deltaY * correctedCurveScale, 
				centerPoint.z
			);

			// Update bounds
			if(i == 0) {
				m_curveBounds.Set(points[i].x, points[i].y, 0, 0);
				deltaYRange.Set(deltaY, deltaY);
			} else {
				m_curveBounds.xMin = Mathf.Min(m_curveBounds.xMin, points[i].x);
				m_curveBounds.xMax = Mathf.Max(m_curveBounds.xMax, points[i].x);
				m_curveBounds.yMin = Mathf.Min(m_curveBounds.yMin, points[i].y);
				m_curveBounds.yMax = Mathf.Max(m_curveBounds.yMax, points[i].y);

				deltaYRange.min = Mathf.Min(deltaYRange.min, deltaY);
				deltaYRange.max = Mathf.Max(deltaYRange.max, deltaY);
			}
		}

		Handles.matrix = Matrix4x4.identity;

		// Draw curve!
		float lineWidth = 5f;
		Handles.color = Color.red;
		Handles.DrawAAPolyLine(lineWidth, pointCount, points);

		// Draw bounds
		Handles.color = Colors.orange;
		Handles.DrawAAPolyLine(
			lineWidth,
			new Vector3(m_curveBounds.xMin, m_curveBounds.yMin, centerPoint.z), 
			new Vector3(m_curveBounds.xMin, m_curveBounds.yMax, centerPoint.z),
			new Vector3(m_curveBounds.xMax, m_curveBounds.yMax, centerPoint.z),
			new Vector3(m_curveBounds.xMax, m_curveBounds.yMin, centerPoint.z), 
			new Vector3(m_curveBounds.xMin, m_curveBounds.yMin, centerPoint.z)
		);

		// Draw & process FreeMoveHandles
		float handleSize = HandleUtility.GetHandleSize(centerPoint) * 0.1f;
		Handles.color = Colors.orange;

		// LEFT HANDLE
		Vector3 old_left = new Vector3(m_curveBounds.xMin, m_curveBounds.center.y, centerPoint.z);
		Vector3 new_left = Handles.FreeMoveHandle(old_left, Quaternion.identity, handleSize, Vector3.zero, Handles.DotCap);
		bool hasChanged = false;
		if(old_left != new_left) {
			float delta = old_left.x - new_left.x;
			m_customReferenceSizeProp.floatValue += delta/2f / lossyScale.x;
			hasChanged = true;
		}

		// TOP HANDLE
		Vector3 old_top = new Vector3(m_curveBounds.center.x, m_curveBounds.yMax, centerPoint.z);
		Vector3 new_top = Handles.FreeMoveHandle(old_top, Quaternion.identity, handleSize, Vector3.zero, Handles.DotCap);
		if(old_top != new_top) {
			// Reverse conversion formula from Scale to Y to figure out new scale
			// y = center.y + deltaY * cs * 0.1f * width
			// y - center.y = deltaY * cs * 0.1f * width
			// cs = (y - center.y)/(deltaY * 0.1f * width)
			m_curveScaleProp.floatValue = (new_top.y - centerPoint.y)/(deltaYRange.max * 0.1f * boundsX.distance);
			hasChanged = true;
		}

		// RIGHT HANDLE
		Vector3 old_right = new Vector3(m_curveBounds.xMax, m_curveBounds.center.y, centerPoint.z);
		Vector3 new_right = Handles.FreeMoveHandle(old_right, Quaternion.identity, handleSize, Vector3.zero, Handles.DotCap);
		if(old_right != new_right) {
			float delta = old_right.x - new_right.x;
			m_customReferenceSizeProp.floatValue -= delta/2f / lossyScale.x;
			hasChanged = true;
		}

		// BOTTOM HANDLE
		Vector3 old_bottom = new Vector3(m_curveBounds.center.x, m_curveBounds.yMin, centerPoint.z);
		Vector3 new_bottom = Handles.FreeMoveHandle(old_bottom, Quaternion.identity, handleSize, Vector3.zero, Handles.DotCap);
		if(old_bottom != new_bottom) {
			// Reverse conversion formula from Scale to Y to figure out new scale
			// y = center.y + deltaY * cs * 0.1f * width
			// y - center.y = deltaY * cs * 0.1f * width
			// cs = (y - center.y)/(deltaY * 0.1f * width)
			m_curveScaleProp.floatValue = (new_bottom.y - centerPoint.y)/(deltaYRange.min * 0.1f * boundsX.distance);
			hasChanged = true;
		}

		// Mark as dirty!
		if(hasChanged) {
			Undo.RecordObjects(new Object[] { m_rectTransform, m_targetCurvedText }, "CurvedText");
			m_serializedObject.ApplyModifiedProperties();
			EditorUtility.SetDirty(target);
		}
	}
}