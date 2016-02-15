// AOCBezierCurveEditor.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 19/12/2016.
// Copyright (c) 2016 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// Custom editor for the AOCBezierCurve class.
/// </summary>
[CustomEditor(typeof(BezierCurve))]
[CanEditMultipleObjects]
public class BezierCurveEditor : Editor {
	//------------------------------------------------------------------//
	// CONSTANTS														//
	//------------------------------------------------------------------//
	private enum State {
		HOVER,
		DRAG,
		BOX_SELECT,
		MOVE_SELECTED
	}

	//------------------------------------------------------------------//
	// CUSTOM STYLES													//
	//------------------------------------------------------------------//
	private static GUIStyle s_sceneLabelStyle = null;

	//------------------------------------------------------------------//
	// PROPERTIES														//
	//------------------------------------------------------------------//
	private BezierCurve targetCurve { get { return target as BezierCurve; }}

	//------------------------------------------------------------------//
	// MEMBERS															//
	//------------------------------------------------------------------//
	// Store a reference of interesting properties for faster access
	private SerializedProperty m_pointsProp = null;

	// Points
	private List<int> m_selectedIndices = new List<int>();
	private int m_dragIdx = -1;
	private Vector3 m_splitPos = Vector3.zero;

	// Mouse logic
	private State m_state = State.HOVER;
	MouseCursor m_mouseCursor = MouseCursor.Arrow;
	private Vector3 m_mouseScreenPos = Vector3.zero;
	private Vector3 m_mouseWorldPos = Vector3.zero;
	private Vector3 m_mouseClickPos = Vector3.zero;
	private int m_nearestControlPointIdx = -1;

	// Editor settings
	private bool m_editorSettingsExpanded = false;
	private float m_clickRadius = 1f;

	//------------------------------------------------------------------//
	// METHODS															//
	//------------------------------------------------------------------//
	/// <summary>
	/// The editor has been enabled - target object selected.
	/// </summary>
	private void OnEnable() {
		// Initialize custom styles if not done
		if(s_sceneLabelStyle == null) {
			// Is style initialized?
			s_sceneLabelStyle = new GUIStyle(EditorStyles.boldLabel);
			s_sceneLabelStyle.normal.textColor = Colors.WithAlpha(Colors.white, 0.5f);
			s_sceneLabelStyle.fontSize = 20;
		}

		// Store a reference of interesting properties for faster access
		m_pointsProp = serializedObject.FindProperty("m_points");

		// Subscribe to external events
		Undo.undoRedoPerformed += OnUndoRedo;
	}

	/// <summary>
	/// The editor has been disabled - target object unselected.
	/// </summary>
	private void OnDisable() {
		// Unsubscribe to external events
		Undo.undoRedoPerformed -= OnUndoRedo;
	}

	/// <summary>
	/// Draw the inspector.
	/// </summary>
	public override void OnInspectorGUI() {
		// Update the serializedProperty - always do this in the beginning of OnInspectorGUI.
		serializedObject.Update();

		// Start tracking changes
		Undo.RecordObject(targetCurve, "AOCBezierCurve Data Change");
		EditorGUI.BeginChangeCheck();

		// Closed curve?
		targetCurve.closed = EditorGUILayout.Toggle("Closed?", targetCurve.closed);

		// Resolution
		targetCurve.resolution = EditorGUILayout.IntField("Resolution", targetCurve.resolution);

		// Color
		targetCurve.drawColor = EditorGUILayout.ColorField("Color", targetCurve.drawColor);

		// Z-lock
		targetCurve.lockZ = EditorGUILayout.Toggle("Z-Lock", targetCurve.lockZ);

		// Spacing
		EditorGUILayout.Space();

		// Points list, foldable but not directly editable
		m_pointsProp.isExpanded = EditorGUILayout.Foldout(m_pointsProp.isExpanded, m_pointsProp.displayName);
		if(m_pointsProp.isExpanded) {
			DrawPoints();
		}

		// Add/Remove points buttons
		EditorGUILayout.BeginHorizontal(); {
			// Add
			if(GUILayout.Button("Add CP")) {
				targetCurve.AddPoint(Vector3.zero);
			}

			// Remove
			if(GUILayout.Button("Remove CP")) {
				targetCurve.RemovePoint();
			}
		} EditorGUILayoutExt.EndHorizontalSafe();

		// Apply changes to the serialized object - always do this in the end of OnInspectorGUI.
		serializedObject.ApplyModifiedProperties();

		// If something has changed, force a repaint of the scene
		if(EditorGUI.EndChangeCheck()) {
			SceneView.RepaintAll();
		}

		// General editing settings
		// Separator
		EditorGUILayoutExt.Separator();

		// [AOC] Grouped in a foldout, stored in EditorPrefs
		m_editorSettingsExpanded = EditorPrefs.GetBool("AOCBezierCurveSettingsExpanded");
		m_editorSettingsExpanded = EditorGUILayout.Foldout(m_editorSettingsExpanded, "Editor Settings");
		EditorPrefs.SetBool("AOCBezierCurveSettingsExpanded", m_editorSettingsExpanded);
		if(m_editorSettingsExpanded) {
			// Click radius
			m_clickRadius = EditorPrefs.GetFloat("AOCBezierCurveClickRadius");
			m_clickRadius = EditorGUILayout.Slider("Click Radius", m_clickRadius, 0f, 1f);
			EditorPrefs.SetFloat("AOCBezierCurveClickRadius", m_clickRadius);
		}
	}

	/// <summary>
	/// The scene is being refreshed.
	/// </summary>
	public void OnSceneGUI() {
		// Record changes
		Undo.RecordObject(targetCurve, "AOCBezierCurve Data Change");

		// Scene-related stuff
		// Draw position handles to move points and handlers
		BezierPoint p;
		for(int i = 0; i < targetCurve.pointCount; i++) {
			p = targetCurve.GetPoint(i);

			// The point itself
			Handles.color = Colors.white;
			p.globalPosition = Handles.PositionHandle(p.globalPosition, Quaternion.identity);

			// Handlers
			if(p.handleStyle != BezierPoint.HandleStyle.NONE) {
				Handles.color = Colors.skyBlue;
				p.globalHandle1 = Handles.PositionHandle(p.globalHandle1, Quaternion.identity);
				p.globalHandle2 = Handles.PositionHandle(p.globalHandle2, Quaternion.identity);
			}
		}

		// VERSION 0.2 - from PolyMesh
		/*
		// Load handle matrix
		Handles.matrix = targetCurve.transform.localToWorldMatrix;

		// Draw points
		AOCBezierPoint p;
		for(int i = 0; i < targetCurve.pointCount; i++) {
			// Get point and other aux vars
			p = targetCurve.GetPointAt(i);
			bool selected = m_selectedIndices.Contains(i);

			// Different color for nearest line
			// [AOC] TODO!!
			//Handles.color = nearestLine == i ? Colors.green : Colors.white;
			//DrawSegment(i);

			// The point itself
			// Draw a circle around if selected
			if(selected) {
				Handles.color = Colors.green;
				//DrawCircle(p.globalPosition, 0.08f);
				//Handles.CircleCap(0, p.globalPosition, inverseRotation, HandleUtility.GetHandleSize(p.globalPosition) * 0.08f);
				Handles.CircleCap(0, p.globalPosition, Quaternion.identity, HandleUtility.GetHandleSize(p.globalPosition) * 0.08f);
			} else if(m_dragIdx == i) {
				Handles.color = Colors.blue;
			} else {
				Handles.color = Colors.white;
			}
			Handles.DotCap(0, p.globalPosition, Quaternion.identity, HandleUtility.GetHandleSize(p.globalPosition) * 0.03f);

			// Handlers
			// [AOC] TODO!! Interactable - for now let's just draw them
			// Only on the selected nodes
			if(selected) {
				// Handler 1
				Handles.color = Colors.skyBlue;
				Handles.DrawLine(p.globalPosition, p.globalHandle1);
				Handles.CircleCap(0, p.globalHandle1, Quaternion.identity, HandleUtility.GetHandleSize(p.globalPosition) * 0.1f);

				// Handler 2
				Handles.color = Colors.skyBlue;
				Handles.DrawLine(p.globalPosition, p.globalHandle2);
				Handles.CircleCap(0, p.globalHandle2, Quaternion.identity, HandleUtility.GetHandleSize(p.globalPosition) * 0.1f);
			}

			// Label
			Handles.Label(p.globalPosition, i.ToString(), s_sceneLabelStyle);
		}

		// Check several quit conditions before proceeding
		if(Tools.current == Tool.View) return;	// Using camera
		if(Camera.current == null) return;	// No camera
		if(Event.current.type == EventType.Layout) return;	// Layouting				// HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
		if(Event.current.type == EventType.ScrollWheel) return;	// Zooming in-out
		if(Event.current.isMouse && Event.current.button > 0) return;	// ????

		// Update mouse vars - if mouse doesn't intersect the curve's Z plane, no need to continue
		EditorGUIUtility.AddCursorRect(new Rect(0, 0, Camera.current.pixelWidth, Camera.current.pixelHeight), m_mouseCursor);
		m_mouseCursor = MouseCursor.Arrow;
		m_mouseScreenPos = new Vector3(Event.current.mousePosition.x, Camera.current.pixelHeight - Event.current.mousePosition.y);
		Plane plane = new Plane(-targetCurve.transform.forward, targetCurve.transform.position);
		Ray ray = Camera.current.ScreenPointToRay(m_mouseScreenPos);
		float hit;
		if(plane.Raycast(ray, out hit)) {
			m_mouseWorldPos = targetCurve.transform.worldToLocalMatrix.MultiplyPoint(ray.GetPoint(hit));
		} else {
			return;
		}

		// Update nearest segment and split position
		ComputeNearestCPAndSplitPosition();

		// Update logic state
		UpdateState();

		// Repaint
		HandleUtility.Repaint();
		*/
	}

	/// <summary>
	/// Logic state update.
	/// </summary>
	private void UpdateState() {
		// Different actions based on current state
		switch(m_state) {
			// Mouse hovering the curve, idle-like state
			case State.HOVER: {
				// Draw nearest line and split point
				if(m_nearestControlPointIdx >= 0) {
					Handles.color = Color.green;
					//Handles.DrawLine(targetCurve[m_nearestLine].globalPosition, targetCurve[(m_nearestLine + 1) % targetCurve.pointCount].globalPosition);
					DrawCurve(targetCurve[m_nearestControlPointIdx], targetCurve[(m_nearestControlPointIdx + 1) % targetCurve.pointCount], targetCurve.resolution);

					Handles.color = Color.red;
					Handles.DotCap(0, m_splitPos, Quaternion.identity, HandleUtility.GetHandleSize(m_splitPos) * 0.03f);
				}

				// Are we dragging selected points?
				// [AOC] TODO!!
				//if(TryDragSelected()) ChangeState(State.DRAG); return;

				// Are we selecting all points?
				// [AOC] TODO!!
				//if(TrySelectAll()) ChangeState(State.HOVER); return;
			} break;
		}
	}

	/// <summary>
	/// Perform all required actions when changing from one state to another.
	/// Nothing will happen if state is the same as we already are.
	/// </summary>
	/// <param name="_newState">New state.</param>
	private void ChangeState(State _newState) {
		// Skip if state is the same
		if(_newState == m_state) return;

		// Perform some actions based on state we're leaving

		// Perform some actions based on new state

		// Store state change
		m_state = _newState;
	}

	//------------------------------------------------------------------//
	// DRAWING METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Draw and edit the points list.
	/// </summary>
	private void DrawPoints() {
		// Show all points in a list
		BezierPoint p;
		for(int i = 0; i < targetCurve.pointCount; i++) {
			// Get target point
			p = targetCurve.GetPoint(i);

			// Handle Type
			p.handleStyle = (BezierPoint.HandleStyle)EditorGUILayout.EnumPopup("Handle Style", p.handleStyle);

			// Position
			p.position = EditorGUILayout.Vector3Field("Position", p.position);

			// Handle 1
			p.handle1 = EditorGUILayout.Vector3Field("Handle 1", p.handle1);

			// Handle 2
			p.handle2 = EditorGUILayout.Vector3Field("Handle 2", p.handle2);

			// Spacing
			EditorGUILayout.Space();
		}
	}

	/// <summary>
	/// Draws a curve between two points in the Editor Scene.
	/// Color must be set beforehand using the Handles.color property.
	/// </summary>
	/// <param name="_p1">The bezier point at the beginning of the curve to be drawn.</param>
	/// <param name="_p2">The bezier point at the end of the curve to be drawn.</param>
	/// <param name='resolution'>The number of segments along the curve to draw.</param>
	private void DrawCurve(BezierPoint _p1, BezierPoint _p2, int _resolution) {
		// Aux vars
		int limit = _resolution + 1;
		float res = _resolution;
		Vector3 lastPoint = _p1.globalPosition;
		Vector3 currentPoint = Vector3.zero;

		// Divide the curve into segments (based on resolution) and draw a straight line for each one
		for(int i = 1; i < limit; i++){
			currentPoint = BezierCurve.GetValue(_p1, _p2, i/res);
			Handles.DrawLine(lastPoint, currentPoint);
			lastPoint = currentPoint;
		}
	}	

	//------------------------------------------------------------------//
	// INTERNAL UTILS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Compute nearest control point to current mouse position and nearest split point.
	/// Store the result in the m_nearestControlPointIdx and m_splitPos members respectively.
	/// </summary>
	void ComputeNearestCPAndSplitPosition() {
		// Skip if not enough points
		if(targetCurve.pointCount < 2) {
			m_nearestControlPointIdx = -1;
			m_splitPos = Vector3.zero;
			return;
		}

		// Custom version 2 - using pre-computed segments
		m_nearestControlPointIdx = -1;
		m_splitPos = targetCurve[0].globalPosition;
		float minDist = float.MaxValue;
		Vector3 tmpPos = Vector3.zero;
		for(int i = 0; i < targetCurve.sampledSegments.Count; i++) {
			// Get target segment
			BezierCurve.SampledSegment segment = targetCurve.sampledSegments[i];

			// Only eligible if cusror is in an acute angle towards the segment and smaller than the actual segment
			// (or something like that, extracted from PolyMeshEditor)
			Vector3 line = segment.p2 - segment.p1;
			Vector3 lineToCursor = m_mouseWorldPos - segment.p1;
			float dot = Vector3.Dot(line.normalized, lineToCursor);
			if(dot >= 0 && dot <= line.magnitude) {
				// Compute where the cursor intersects the segment
				tmpPos = segment.p1 + line.normalized * dot;

				// If it's the closest line so far, save it
				var dist = Vector3.Distance(tmpPos, m_mouseWorldPos);
				if(dist < minDist) {
					minDist = dist;
					m_splitPos = tmpPos;
					m_nearestControlPointIdx = targetCurve.GetPointIdx(segment.cp1);
				}
			}
		}
	}

	//------------------------------------------------------------------//
	// CALLBACKS														//
	//------------------------------------------------------------------//
	/// <summary>
	/// An undo action has been performed.
	/// </summary>
	private void OnUndoRedo() {
		// Mark curve as dirty and repaint
		targetCurve.SetDirty();
		SceneView.RepaintAll();
	}
}