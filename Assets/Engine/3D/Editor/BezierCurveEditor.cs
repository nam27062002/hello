// BezierCurveEditor.cs
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
/// Custom editor for the BezierCurve class.
/// </summary>
[CustomEditor(typeof(BezierCurve))]
[CanEditMultipleObjects]
public class BezierCurveEditor : Editor {
	//------------------------------------------------------------------//
	// CONSTANTS														//
	//------------------------------------------------------------------//
	private enum Tool {
		LINE,
		CIRCLE
	}

	private static readonly Color POINT_COLOR = Color.white;
	private static readonly Color HANDLE_COLOR = Colors.skyBlue;
	private static readonly Color SELECTED_COLOR = Color.yellow;
	private static readonly Color DISABLED_COLOR = Color.gray;
	private static readonly Vector3 LABEL_OFFSET = new Vector3(-0.25f, -0.5f, 0f);
	private const float LINE_THICKNESS = 5f;

	//------------------------------------------------------------------//
	// PROPERTIES														//
	//------------------------------------------------------------------//
	private BezierCurve targetCurve { get { return target as BezierCurve; }}

	//------------------------------------------------------------------//
	// MEMBERS															//
	//------------------------------------------------------------------//
	// Store a reference of interesting properties for faster access
	private SerializedProperty m_pointsProp = null;

	// Tools
	private Tool m_toolSelected = Tool.LINE;

	// Line Tool
	private Vector3 m_lineToolStartPoint = Vector3.zero;
	private Vector3 m_lineToolOffset = new Vector3(50f, 0f, 0f);

	// Circle Tool
	private Vector3 m_circleToolCenter = Vector3.zero;
	private float m_circleToolRadius = 10f;

	//------------------------------------------------------------------//
	// METHODS															//
	//------------------------------------------------------------------//
	/// <summary>
	/// The editor has been enabled - target object selected.
	/// </summary>
	private void OnEnable() {
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
		Undo.RecordObject(targetCurve, "BezierCurve Data Change");
		EditorGUI.BeginChangeCheck();

		// Closed curve?
		targetCurve.closed = EditorGUILayout.Toggle("Closed?", targetCurve.closed);

		// Auto-smooth
		targetCurve.autoSmooth = EditorGUILayout.Toggle("Auto Smooth", targetCurve.autoSmooth);
		if(targetCurve.autoSmooth) {
			targetCurve.autoSmoothFactor = EditorGUILayout.FloatField("Auto Smooth Factor", targetCurve.autoSmoothFactor);
		}

		// Separator
		EditorGUILayoutExt.Separator();

		// Editor settings
		bool editorSettingsExpanded = EditorPrefs.GetBool("BezierCurveEditorSettingsExpanded");
		editorSettingsExpanded = EditorGUILayout.Foldout(editorSettingsExpanded, "Editor Settings");
		EditorPrefs.SetBool("BezierCurveEditorSettingsExpanded", editorSettingsExpanded);
		if(editorSettingsExpanded) {
			// Draw editor settings menu
			EditorGUI.indentLevel++;
			DoEditorSettings();
			EditorGUI.indentLevel--;
		}

		// Separator
		EditorGUILayoutExt.Separator();

		// Points list, foldable but not directly editable
		m_pointsProp.isExpanded = EditorGUILayout.Foldout(m_pointsProp.isExpanded, m_pointsProp.displayName + " [" + targetCurve.pointCount + "]");
		if(m_pointsProp.isExpanded) {
			DoPoints();
		}

		// Add/Remove points buttons
		EditorGUILayout.BeginHorizontal(); {
			// Add
			if(GUILayout.Button("Add CP"))  {
				// Clone last point (if any)
				if (targetCurve.points.Count > 0 ) {
					Vector3 pos = targetCurve.points[targetCurve.points.Count - 1].globalPosition;
					pos.x += 10f;
					targetCurve.AddPoint(pos);
				} else {
					targetCurve.AddPoint(Vector3.zero);
				}
			}

			// Remove
			if(GUILayout.Button("Remove CP")) {
				targetCurve.RemovePoint();
			}
		} EditorGUILayoutExt.EndHorizontalSafe();

		// Tools
		EditorGUILayoutExt.Separator();
		bool toolsExpanded = EditorPrefs.GetBool("BezierCurveToolsExpanded");
		toolsExpanded = EditorGUILayout.Foldout(toolsExpanded, "Tools");
		EditorPrefs.SetBool("BezierCurveToolsExpanded", toolsExpanded);
		if(toolsExpanded) {
			// Draw tools menu
			EditorGUI.indentLevel++;
			DoTools();
			EditorGUI.indentLevel--;
		}

		// Apply changes to the serialized object - always do this in the end of OnInspectorGUI.
		serializedObject.ApplyModifiedProperties();

		// If something has changed, force a repaint of the scene
		if(EditorGUI.EndChangeCheck()) {
			SceneView.RepaintAll();
		}
	}

	/// <summary>
	/// The scene is being refreshed.
	/// </summary>
	public void OnSceneGUI() {
		// Don't draw if not active
		if(!targetCurve.isActiveAndEnabled) return;

		// Record changes
		Undo.RecordObject(targetCurve, "BezierCurve Data Change");

		// Scene-related stuff
		// Draw position handles to move points and handlers
		BezierPoint p;
		int localSelection = -1;	// Per point: -1 (none), 0 (point), 1 (handler1), 2 (handler2)
		float handlerPointSize = targetCurve.pointSize * 0.5f;
		float handlerPickSize = targetCurve.pickSize * 0.5f;
		for(int i = 0; i < targetCurve.pointCount; i++) {
			// Get point
			p = targetCurve.GetPoint(i);
			bool handlersEnabled = p.handleStyle != BezierPoint.HandleStyle.NONE && !targetCurve.autoSmooth;

			// Label
			Handles.Label(p.globalPosition + LABEL_OFFSET, i.ToString(), CustomEditorStyles.bigSceneLabel);

			// Is this point or any of its handlers the current selection?
			if(i == targetCurve.selectedIdx) {
				localSelection = targetCurve.selectedHandlerIdx;	// 0, 1, 2; matches same values
			} else {
				localSelection = -1;
			}

			// Handlers
			// Draw them first so the lines are not rendered on top of the points
			if(p.handleStyle != BezierPoint.HandleStyle.NONE) {
				// Handler 1
				if(!handlersEnabled) {
					Handles.color = DISABLED_COLOR;
				} else if(localSelection == 1) {
					Handles.color = SELECTED_COLOR;
				} else {
					Handles.color = HANDLE_COLOR;
				}
				Handles.DrawLine(p.globalPosition, p.globalHandle1);
				if(Handles.Button(p.globalHandle1, Quaternion.identity, handlerPointSize, handlerPickSize, Handles.SphereHandleCap)) {
					// Select this point
					targetCurve.selectedIdx = i;
					targetCurve.selectedHandlerIdx = 1;
					localSelection = 1;
				}

				// Handler 2
				if(!handlersEnabled) {
					Handles.color = DISABLED_COLOR;
				} else if(localSelection == 2) {
					Handles.color = SELECTED_COLOR;
				} else {
					Handles.color = HANDLE_COLOR;
				}
				Handles.DrawLine(p.globalPosition, p.globalHandle2);
				if(Handles.Button(p.globalHandle2, Quaternion.identity, handlerPointSize, handlerPickSize, Handles.SphereHandleCap)) {
					// Select this point
					targetCurve.selectedIdx = i;
					targetCurve.selectedHandlerIdx = 2;
					localSelection = 2;
				}
			}

			// [AOC] We'll be drawing Handle spheres on top of the Gizmos to detect selection
			// The point itself
			Handles.color = localSelection == 0 ? SELECTED_COLOR : POINT_COLOR;
			if(Handles.Button(p.globalPosition, Quaternion.identity, targetCurve.pointSize, targetCurve.pickSize, Handles.SphereHandleCap)) {
				// Select this point
				targetCurve.selectedIdx = i;
				targetCurve.selectedHandlerIdx = 0;
				localSelection = 0;
			}

			// Skip position handlers if the point is locked!
			if(p.locked) continue;

			// Draw position handler in the selected point/handler
			switch(localSelection) {
				case 0: {
					p.globalPosition = Handles.PositionHandle(p.globalPosition, Quaternion.identity);
				} break;

				case 1: {
					// Except if HandleMode is "NONE" or autoSmooth is enabled
					if(handlersEnabled) {
						p.globalHandle1 = Handles.PositionHandle(p.globalHandle1, Quaternion.identity);
					}
				} break;

				case 2: {
					if(handlersEnabled) {
						p.globalHandle2 = Handles.PositionHandle(p.globalHandle2, Quaternion.identity);
					}
				} break;
			}
		}
	}

	/// <summary>
	/// Draw gizmos for the target bezier curve.
	/// Do it here rather than OnDrawGizoms so we can define it as pickable as well
	/// as save time by avoiding compilation of the whole project (just the Editor code).
	/// </summary>
	/// <param name="_target">Target curve.</param>
	/// <param name="_gizmoType">Gizmo type.</param>
	[DrawGizmo(GizmoType.Pickable | GizmoType.InSelectionHierarchy | GizmoType.NotInSelectionHierarchy)]
	static void DrawGizmo(BezierCurve _target, GizmoType _gizmoType) {
		// Don't draw if not active
		if(!_target.isActiveAndEnabled) return;

		// [AOC] Although Handles give us more visual options, we will use Gizmos
		// 		 instead to allow selecting the object by clicking on the spheres/line

		// Draw line
		// Gather sampled points
		Vector3[] sampledPoints = new Vector3[_target.sampledSegments.Count];
		for(int i = 0; i < _target.sampledSegments.Count; i++) {
			sampledPoints[i] = _target.sampledSegments[i].p1;
		}

		// Draw selectable Gizmo line
		Gizmos.color = _target.drawColor;
		for(int i = 1; i < sampledPoints.Length; ++i) {
			Gizmos.DrawLine(sampledPoints[i-1], sampledPoints[i]);
		}

		// [AOC] Overkill: To make line more visible, draw a Handles line on top of the selectable Gizmos line
		// 		 Remove if performance is critical
		Handles.color = _target.drawColor;
		Handles.DrawAAPolyLine(LINE_THICKNESS, sampledPoints);

		// Draw points
		Gizmos.color = POINT_COLOR;
		float sphereRadius = _target.pointSize/2f;
		BezierPoint p;
		for(int i = 0; i < _target.pointCount; i++) {
			p = _target.GetPoint(i);
			Gizmos.DrawSphere(p.globalPosition, sphereRadius);
		}
	}

	//------------------------------------------------------------------//
	// DRAWING METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Draw and edit the points list.
	/// </summary>
	private void DoPoints() {
		// Aux vars
		BezierPoint p;
		bool wasEnabled;
		int swapPoint1 = -1;
		int swapPoint2 = -1;

		// Indent in
		EditorGUI.indentLevel++;

		// Info comment
		EditorGUILayout.LabelField("Handles can be edited directly on the scene by holding the Control key", CustomEditorStyles.commentLabelLeft);

		// Toggle All
		int allLocked = 0;	// 0 -> false, 1 -> true, 2 -> mixed
		for(int i = 0; i < targetCurve.pointCount; i++) {
			// Get point
			p = targetCurve.GetPoint(i);

			// First point straight away
			if(i == 0) {
				allLocked = p.locked ? 1 : 0;
			} else {
				// Different from previous values?
				if((allLocked == 0 && p.locked)
				|| (allLocked == 1 && !p.locked)) {
					// Yes! Set mixed value and break loop
					allLocked = 2;
					break;
				}
			}
		}

		// Little trick from http://forum.unity3d.com/threads/how-to-make-partially-selected-editorgui-toggle-solved.327805/
		// (since EditorGUI.showMixedValue doesn't work with GUILayout.Toggle)
		GUIStyle toggleStyle = (allLocked == 2) ? GUI.skin.GetStyle("ToggleMixed") : EditorStyles.toggle;

		// We want to know if the toggle was changed, but we don't want to interfere with the outer BeginChangeCheck block, so backup current "changed" status
		bool guiChangedBackup = GUI.changed;
		GUI.changed = false;
		bool lockAll = !GUILayout.Toggle((allLocked == 0), " Toggle all", toggleStyle);
		if(GUI.changed) {
			// Apply to all points
			for(int i = 0; i < targetCurve.pointCount; i++) {
				targetCurve.GetPoint(i).locked = lockAll;
			}
		}

		// Revert temp stuff to avoid interfering with following controls
		GUI.changed = guiChangedBackup || GUI.changed;	// Either it was already changed or it has changed now

		EditorGUILayout.Space();

		// Show all points in a list
		for(int i = 0; i < targetCurve.pointCount; i++) {
			// Get target point
			p = targetCurve.GetPoint(i);

			// Lock toggle, name, sort buttons
			EditorGUILayout.BeginHorizontal(); {
				// Lock & point name
				p.locked = !GUILayout.Toggle(!p.locked, " " + i.ToString());

				GUILayout.FlexibleSpace();

				// Move up
				wasEnabled = GUI.enabled;
				GUI.enabled = (i > 0);
				if(GUILayout.Button("▲", GUILayout.Width(20f))) {
					swapPoint1 = i;
					swapPoint2 = i-1;
				}

				// Move down
				GUI.enabled = (i < targetCurve.pointCount - 1);
				if(GUILayout.Button("▼", GUILayout.Width(20f))) {
					swapPoint1 = i;
					swapPoint2 = i+1;
				}
				GUI.enabled = wasEnabled;

				// Delete button
				if(GUILayout.Button("X", GUILayout.Width(20f))) {
					swapPoint1 = i;
				}
			} EditorGUILayoutExt.EndHorizontalSafe();

			EditorGUI.indentLevel++;

			// Handle Type - disabled if auto-smoothing
			wasEnabled = GUI.enabled;
			GUI.enabled = !targetCurve.autoSmooth;
			p.handleStyle = (BezierPoint.HandleStyle)EditorGUILayout.EnumPopup("Handle Style", p.handleStyle);
			GUI.enabled = wasEnabled;

			// Position
			wasEnabled = GUI.enabled;
			GUI.enabled = !p.locked;
			p.position = EditorGUILayout.Vector3Field("Position", p.position);
			GUI.enabled = wasEnabled;

			// Handles 1 and 2 - disabled if auto-smoothing
			wasEnabled = GUI.enabled;
			GUI.enabled = !p.locked && !targetCurve.autoSmooth;
			p.handle1 = EditorGUILayout.Vector3Field("Handle 1", p.handle1);
			p.handle2 = EditorGUILayout.Vector3Field("Handle 2", p.handle2);
			GUI.enabled = wasEnabled;

			// Spacing
			EditorGUILayout.Space();

			// End
			EditorGUI.indentLevel--;
		}

		// End
		EditorGUI.indentLevel--;

		// Perform any required point swapping/deleting - after having drawn them all to avoid reordering issues
		if(swapPoint1 >= 0) {
			if(swapPoint2 < 0) {
				// Delete
				m_pointsProp.DeleteArrayElementAtIndex(swapPoint1);
			} else {
				// Move
				m_pointsProp.MoveArrayElement(swapPoint1, swapPoint2);
			}
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

	/// <summary>
	/// Do the editor settings section.
	/// </summary>
	private void DoEditorSettings() {
		// Resolution
		targetCurve.resolution = EditorGUILayout.IntField("Resolution", targetCurve.resolution);

		// Color
		EditorGUILayout.PropertyField(serializedObject.FindProperty("drawColor"));

		// Z-lock
		EditorGUILayout.PropertyField(serializedObject.FindProperty("lockZ"));

		// Point and pick sizes
		EditorGUILayout.PropertyField(serializedObject.FindProperty("pointSize"));
		EditorGUILayout.PropertyField(serializedObject.FindProperty("pickSize"));
	}

	/// <summary>
	/// Do the tools section.
	/// </summary>
	private void DoTools() {
		// Tool selector
		m_toolSelected = EditorGUILayoutExt.PrefField<Tool>("Tool", "BezierCurveToolSelected", Tool.LINE);

		// Do different stuff depending on selected tool
		switch(m_toolSelected) {
			case Tool.LINE:		DoToolLine();	break;
			case Tool.CIRCLE:	DoToolCircle();	break;
		}
	}

	/// <summary>
	/// Draw the controls for the Line tool.
	/// </summary>
	private void DoToolLine() {
		// Info Label
		EditorGUILayout.LabelField("Reset all points in a straight line", CustomEditorStyles.commentLabelLeft);

		// Start point
		m_lineToolStartPoint = EditorGUILayoutExt.PrefField<Vector3>("Start Point", "BezierCurveLineToolStartPoint", m_lineToolStartPoint);

		// Offset
		m_lineToolOffset = EditorGUILayoutExt.PrefField<Vector3>("Offset", "BezierCurveLineToolOffset", m_lineToolOffset);

		// Do it!
		// Reset!
		if(GUILayout.Button("DO IT!", GUILayout.Height(30f))) {
			// Apply to all points in the curve
			Vector3 pos = m_lineToolStartPoint;
			for(int i = 0; i < targetCurve.points.Count; i++) {
				BezierPoint p = targetCurve.GetPoint(i);

				bool wasLocked = p.locked;
				p.locked = false;

				p.position = pos;
				p.handle1 = Vector3.left;
				p.handle2 = Vector3.right;
				pos += m_lineToolOffset;

				p.locked = wasLocked;
			}
		}
	}

	/// <summary>
	/// Draw the controls for the Circle tool.
	/// </summary>
	private void DoToolCircle() {
		// Info Label
		EditorGUILayout.LabelField("Reset all points in a circle\nCurve will be closed", CustomEditorStyles.commentLabelLeft);

		// Center point
		m_circleToolCenter = EditorGUILayoutExt.PrefField<Vector3>("Center", "BezierCurveCircleToolCenter", m_circleToolCenter);

		// Offset
		m_circleToolRadius = EditorGUILayoutExt.PrefField<float>("Radius", "BezierCurveCircleToolRadius", m_circleToolRadius);

		// Do it!
		// Reset!
		if(GUILayout.Button("DO IT!", GUILayout.Height(30f))) {
			// Make sure curve is closed and z is not locked!
			targetCurve.closed = true;
			targetCurve.lockZ = false;

			// Apply to all points in the curve
			int numPoints = targetCurve.points.Count;
			for(int i = 0; i < numPoints; i++) {
				// Get target point
				BezierPoint p = targetCurve.GetPoint(i);

				// Force unlock and connected style
				bool wasLocked = p.locked;
				p.locked = false;

				// Compute position in the X-Y plane
				float angle = (float)i/(float)numPoints * 360f;
				Quaternion q = Quaternion.AngleAxis(angle, Vector3.up);
				p.position = m_circleToolCenter + q * (Vector3.right * m_circleToolRadius);	// This will do the trick!

				// Restore lock state
				p.locked = wasLocked;
			}

			// Auto-smooth handlers for a nice rounded shape!
			targetCurve.AutoSmooth(0.33f);
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
		EditorUtility.SetDirty(targetCurve);
	}
}