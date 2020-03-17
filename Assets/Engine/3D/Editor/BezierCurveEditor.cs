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
using System.Globalization;

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

	private enum PointOperation {
		NONE = -1,

		SWAP,
		INSERT,
		DELETE
	}

	private static readonly Color POINT_COLOR = Colors.WithAlpha(Color.white, 0.5f);
	private static readonly Color HANDLE_COLOR = Colors.skyBlue;
	private static readonly Color SELECTED_COLOR = Color.yellow;
	private static readonly Color DISABLED_COLOR = Color.gray;

	private static readonly Vector3 LABEL_OFFSET = new Vector3(-0.25f, -0.5f, 0f);

	private const float CONSTANT_SIZE_FACTOR = 0.25f;	// Constant size is generally super-big. Apply a constant factor (point size can always be defined by the user).
	private const float PICK_FACTOR = 1f;

	private static readonly string[] AXIS_LABELS = new string[] { "X", "Y", "Z" };

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

	// Contents
	private GUIContent m_moveUpButtonContent = new GUIContent("▲", "Move Point Up");
	private GUIContent m_moveDownButtonContent = new GUIContent("▼", "Move Point Down");
	private GUIContent m_insertButtonContent = new GUIContent("+", "Insert Point");
	private GUIContent m_deleteButtonContent = new GUIContent("X", "Delete Point");

	// Custom styles
	private static GUIStyle s_pointLabelStyle = null;
	private GUIStyle pointLabelStyle {
		get {
			if(s_pointLabelStyle == null) {
				s_pointLabelStyle = new GUIStyle(CustomEditorStyles.bigSceneLabel);
				s_pointLabelStyle.fontSize = 14;
				s_pointLabelStyle.normal.textColor = Colors.WithAlpha(POINT_COLOR, 0.5f);
			}
			return s_pointLabelStyle;
		}
	}

	private static GUIStyle s_pointLabelStyleSelected = null;
	private GUIStyle pointLabelStyleSelected {
		get {
			if(s_pointLabelStyleSelected == null) {
				s_pointLabelStyleSelected = new GUIStyle(pointLabelStyle);
				s_pointLabelStyleSelected.normal.textColor = Colors.WithAlpha(SELECTED_COLOR, 0.5f);
			}
			return s_pointLabelStyleSelected;
		}
	}

	private static GUIStyle s_pointBackgroundStyle = null;
	private GUIStyle pointBackgroundStyle {
		get {
			if(s_pointBackgroundStyle == null) {
				int margin = 5;
				s_pointBackgroundStyle = new GUIStyle(EditorStyles.miniButton);
				s_pointBackgroundStyle.padding = new RectOffset(margin, margin, margin, margin);
			}
			return s_pointBackgroundStyle;
		}
	}

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

		// Clear selection
		SetSelection(-1, -1, false, false);
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
			DoEditorSettingsInspector();
			EditorGUI.indentLevel--;
		}

		// Separator
		EditorGUILayoutExt.Separator();

		// Points list, foldable but not directly editable
		m_pointsProp.isExpanded = EditorGUILayout.Foldout(m_pointsProp.isExpanded, m_pointsProp.displayName + " [" + targetCurve.pointCount + "]");
		if(m_pointsProp.isExpanded) {
			DoPointsInspector();
		}

		// Tools
		EditorGUILayoutExt.Separator();
		bool toolsExpanded = EditorPrefs.GetBool("BezierCurveToolsExpanded");
		toolsExpanded = EditorGUILayout.Foldout(toolsExpanded, "Tools");
		EditorPrefs.SetBool("BezierCurveToolsExpanded", toolsExpanded);
		if(toolsExpanded) {
			// Draw tools menu
			EditorGUI.indentLevel++;
			DoToolsInspector();
			EditorGUI.indentLevel--;
		}

		// Process focus changes to check if a new point has been selected
		ProcessFocusChanges();

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

		// Do line
		DrawLine(targetCurve, false, true);

		// Draw points and handlers
		BezierPoint p;
		int localSelection = -1;	// Per point: -1 (none), 0 (point), 1 (handler1), 2 (handler2)
		float handlerPointSize = targetCurve.pointSize * 0.5f;
		float pointSize = 0f;
		float handlerSize = 0f;
		Vector3 handlerPos = Vector3.zero;
		Vector3 handlerDist = Vector3.zero;
		float minHandlerDist = 0f;
		bool handlersEnabled = true;
		for(int i = 0; i < targetCurve.pointCount; i++) {
			// Get point
			p = targetCurve.GetPoint(i);

			// Aux vars
			handlersEnabled = p.handleStyle != BezierPoint.HandleStyle.NONE && !targetCurve.autoSmooth && !p.locked;
			pointSize = targetCurve.pointSize * (targetCurve.constantSize ? HandleUtility.GetHandleSize(p.globalPosition) * CONSTANT_SIZE_FACTOR : 1f);

			// Label
			Handles.Label(
				p.globalPosition + LABEL_OFFSET, 
				string.IsNullOrEmpty(p.name) ? "P" + i.ToString() : p.name, 
				(i == targetCurve.selectedIdx) ? pointLabelStyleSelected : pointLabelStyle
			);

			// Is this point or any of its handlers the current selection?
			if(i == targetCurve.selectedIdx) {
				localSelection = targetCurve.selectedHandlerIdx;	// 0, 1, 2; matches same values
			} else {
				localSelection = -1;
			}

			// Handlers
			// Draw them first so the lines are not rendered on top of the points
			if(p.handleStyle != BezierPoint.HandleStyle.NONE) {
				// HANDLER 1
				{
					// Color
					if(localSelection == 1) {
						Handles.color = SELECTED_COLOR;
					} else if(!handlersEnabled) {
						Handles.color = DISABLED_COLOR;
					} else {
						Handles.color = HANDLE_COLOR;
					}

					// Connection line
					Handles.DrawLine(p.globalPosition, p.globalHandle1);

					// Dot Size and position
					// Minimum distance from the point to be picked
					handlerSize = handlerPointSize * (targetCurve.constantSize ? HandleUtility.GetHandleSize(p.globalHandle1) * CONSTANT_SIZE_FACTOR : 1f);
					handlerPos = p.globalHandle1;
					handlerDist = handlerPos - p.globalPosition;
					minHandlerDist = pointSize/2f + handlerSize/2f;
					if(handlerDist.sqrMagnitude < minHandlerDist * minHandlerDist) {
						if(handlerDist.sqrMagnitude > 0) {
							handlerPos = p.globalPosition + handlerDist.normalized * minHandlerDist;
						} else {
							handlerPos = p.globalPosition + Vector3.left * minHandlerDist;
						}
					}

					// Do the drawing!
					if(Handles.Button(handlerPos, Quaternion.identity, handlerSize, handlerSize * PICK_FACTOR, Handles.SphereHandleCap)) {
						// Select this point
						SetSelection(i, 1, true, true);
						localSelection = 1;
					}
				}

				// HANDLER 2
				{
					// Color
					if(localSelection == 2) {
						Handles.color = SELECTED_COLOR;
					} else if(!handlersEnabled) {
						Handles.color = DISABLED_COLOR;
					} else {
						Handles.color = HANDLE_COLOR;
					}

					// Connection line
					Handles.DrawLine(p.globalPosition, p.globalHandle2);

					// Dot Size and position
					// Minimum distance from the point to be picked
					handlerSize = handlerPointSize * (targetCurve.constantSize ? HandleUtility.GetHandleSize(p.globalHandle2) * CONSTANT_SIZE_FACTOR : 1f);
					handlerPos = p.globalHandle2;
					handlerDist = handlerPos - p.globalPosition;
					minHandlerDist = pointSize/2f + handlerSize/2f;
					if(handlerDist.sqrMagnitude < minHandlerDist * minHandlerDist) {
						if(handlerDist.sqrMagnitude > 0) {
							handlerPos = p.globalPosition + handlerDist.normalized * minHandlerDist;
						} else {
							handlerPos = p.globalPosition + Vector3.right * minHandlerDist;
						}
					}
				
					// Do the drawing!
					if(Handles.Button(handlerPos, Quaternion.identity, handlerSize, handlerSize * PICK_FACTOR, Handles.SphereHandleCap)) {
						// Select this point
						SetSelection(i, 2, true, true);
						localSelection = 2;
					}
				}
			}

			// THE POINT
			{
				// Color
				if(localSelection == 0) {
					Handles.color = SELECTED_COLOR;
				} else if(p.locked) {
					Handles.color = DISABLED_COLOR;
				} else {
					Handles.color = POINT_COLOR;
				}

				// Do the drawing!
				if(Handles.Button(p.globalPosition, Quaternion.identity, pointSize, pointSize * PICK_FACTOR, Handles.SphereHandleCap)) {
					// Select this point
					SetSelection(i, 0, true, true);
					localSelection = 0;
				}
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
	[DrawGizmo(GizmoType.Pickable | GizmoType.NotInSelectionHierarchy | GizmoType.NonSelected)]	// Don't draw when selected, OnSceneGUI will do it better
	static void DrawGizmo(BezierCurve _target, GizmoType _gizmoType) {
		// Don't draw if not active
		if(!_target.isActiveAndEnabled) return;

		// [AOC] Although Handles give us more visual options, Gizmos allow selecting 
		//		 the object by clicking on the spheres/line

		// Draw line
		DrawLine(_target, true, true);

		// Draw points
		Gizmos.color = POINT_COLOR;
		float sphereRadius = _target.pointSize/2f;
		BezierPoint p;
		float scaleFactor = 1f;
		for(int i = 0; i < _target.pointCount; i++) {
			p = _target.GetPoint(i);
			scaleFactor = _target.constantSize ? HandleUtility.GetHandleSize(p.globalPosition) * CONSTANT_SIZE_FACTOR : 1f;
			Gizmos.DrawSphere(p.globalPosition, sphereRadius * scaleFactor);
		}
	}

	//------------------------------------------------------------------------//
	// SCENE DRAWING METHODS												  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Draw the line representing the target curve
	/// </summary>
	/// <param name="_curve">Curve.</param>
	/// <param name="_useGizmos">Use Gizmos library? No thickness, but pickable, can only be called from the DrawGizmo function.</param>
	/// <param name="_useHandles">Use Handles library? Beautiful, but not pickable.</param>
	private static void DrawLine(BezierCurve _curve, bool _useGizmos, bool _useHandles) {
		// Gather sampled points
		Vector3[] sampledPoints = new Vector3[_curve.editorSampledSegments.Count];
		for(int i = 0; i < _curve.editorSampledSegments.Count; i++) {
			sampledPoints[i] = _curve.editorSampledSegments[i].p1;
		}

		// Draw selectable Gizmo line
		if(_useGizmos) {
			Gizmos.color = _curve.drawColor;
			for(int i = 1; i < sampledPoints.Length; ++i) {
				Gizmos.DrawLine(sampledPoints[i-1], sampledPoints[i]);
			}
		}

		// [AOC] Overkill: To make line more visible, draw a Handles line on top of the selectable Gizmos line
		// 		 Remove if performance is critical
		if(_useHandles) {
			Handles.color = _curve.drawColor;
			Handles.DrawAAPolyLine(_curve.lineThickness, sampledPoints);
		}
	}

	//------------------------------------------------------------------------//
	// INSPECTOR METHODS													  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Draw and edit the points list.
	/// </summary>
	private void DoPointsInspector() {
		// Aux vars
		BezierPoint p;
		bool wasEnabled;
		PointOperation operation = PointOperation.NONE;
		int point1 = -1;
		int point2 = -1;

		// Some initializations
		GUI.color = Color.white;
		GUI.contentColor = Color.white;
		GUI.backgroundColor = Color.white;

		// Indent in
		EditorGUI.indentLevel++;

		// Info comment
		EditorGUILayout.LabelField("Points and Handles can be edited directly on the scene", CustomEditorStyles.commentLabelLeft);

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
		// Some aux vars outside the loop
		Rect auxPos = Rect.zero;
		for(int i = 0; i < targetCurve.pointCount; i++) {
			// Get target point
			p = targetCurve.GetPoint(i);

			// Draw a background englobing the whole point data
			GUI.backgroundColor	= i == targetCurve.selectedIdx ? SELECTED_COLOR : Color.white;

			// Add some margins and englobe everything in a layout area
			EditorGUILayout.BeginVertical(pointBackgroundStyle); {
				GUI.backgroundColor = Color.white;

				// Lock toggle, name, sort buttons
				EditorGUILayout.BeginHorizontal(); {
					// Lock
					if(i == targetCurve.selectedIdx) {
						GUI.contentColor = SELECTED_COLOR;
					} else {
						GUI.contentColor = Color.white;
					}
					p.locked = !GUILayout.Toggle(!p.locked, GUIContent.none);

					// Point name: editable textfield, shpw default name if none is defined
					EditorGUI.BeginChangeCheck();
					string pointName = string.IsNullOrEmpty(p.name) ? ("P" + i.ToString()) : p.name;
					int indentBackup = EditorGUI.indentLevel;
					EditorGUI.indentLevel = 0;
					pointName = EditorGUILayout.DelayedTextField(
						pointName, 
						GUILayout.Width(Mathf.Max(EditorStyles.textField.CalcSize(new GUIContent(pointName)).x, 50f))
					);
					if(EditorGUI.EndChangeCheck()) {
						p.name = pointName;
					}
					EditorGUI.indentLevel = indentBackup;

					// If selected or locked (or both), show it!
					if(p.locked) {
						GUI.contentColor = DISABLED_COLOR;
						GUILayout.Label("LOCKED");
					}

					if(i == targetCurve.selectedIdx) {
						GUI.contentColor = SELECTED_COLOR;
						GUILayout.Label("SELECTED");
					}
					GUI.contentColor = Color.white;

					GUILayout.FlexibleSpace();
					wasEnabled = GUI.enabled;

					// Move up
					GUI.enabled = (i > 0);
					if(GUILayout.Button(m_moveUpButtonContent, GUILayout.Width(20f))) {
						operation = PointOperation.SWAP;
						point1 = i;
						point2 = i-1;
					}

					// Move down
					GUI.enabled = (i < targetCurve.pointCount - 1);
					if(GUILayout.Button(m_moveDownButtonContent, GUILayout.Width(20f))) {
						operation = PointOperation.SWAP;
						point1 = i;
						point2 = i+1;
					}

					// Insert button
					GUI.enabled = true;
					GUI.backgroundColor = Color.green;
					if(GUILayout.Button(m_insertButtonContent, GUILayout.Width(20f))) {
						operation = PointOperation.INSERT;
						point1 = i;
					}

					// Delete button
					GUI.enabled = targetCurve.pointCount > 2;	// Don't allow if there are only 2 points!
					GUI.backgroundColor = Color.red;
					if(GUILayout.Button(m_deleteButtonContent, GUILayout.Width(20f))) {
						operation = PointOperation.DELETE;
						point1 = i;
					}
					GUI.backgroundColor = Color.white;
				} EditorGUILayoutExt.EndHorizontalSafe();

				EditorGUI.indentLevel++;

				// Handle Type - disabled if auto-smoothing
				GUI.enabled = !targetCurve.autoSmooth;
				p.handleStyle = (BezierPoint.HandleStyle)EditorGUILayout.EnumPopup("Handle Style", p.handleStyle);

				// Position
				GUI.enabled = !p.locked;
				if(i == targetCurve.selectedIdx && targetCurve.selectedHandlerIdx == 0) {
					GUI.contentColor = SELECTED_COLOR;
				} else {
					GUI.contentColor = Color.white;
				}
				p.position = Vector3FieldWithLock("Position", p.position, targetCurve.lockAxis, i + "|0");

				// Handles - disabled if auto-smoothing
				GUI.enabled = !p.locked && !targetCurve.autoSmooth;

				// Handle 1
				if(i == targetCurve.selectedIdx && targetCurve.selectedHandlerIdx == 1) {
					GUI.contentColor = SELECTED_COLOR;
				} else {
					GUI.contentColor = Color.white;
				}
				p.handle1 = Vector3FieldWithLock("Handle 1", p.handle1, targetCurve.lockHandlersAxis, i + "|1");

				// Handle 2
				if(i == targetCurve.selectedIdx && targetCurve.selectedHandlerIdx == 2) {
					GUI.contentColor = SELECTED_COLOR;
				} else {
					GUI.contentColor = Color.white;
				}
				p.handle2 = Vector3FieldWithLock("Handle 2", p.handle2, targetCurve.lockHandlersAxis, i + "|2");

				// Restore stuff
				GUI.color = Color.white;
				GUI.contentColor = Color.white;
				GUI.backgroundColor = Color.white;
				GUI.enabled = wasEnabled;

				// End
				EditorGUI.indentLevel--;
			} EditorGUILayout.EndVertical();

			// Draw a transparent button on top of everything (raycast order is reversed) to select/deselect the point
			GUI.backgroundColor = Colors.transparentWhite;
			if(GUI.Button(GUILayoutUtility.GetLastRect(), GUIContent.none)) {
				// If not selected, do it
				if(targetCurve.selectedIdx != i) {
					SetSelection(i, 0, false, false);
				}

				// Focus target point
				EditorUtils.FocusWorldPosition(p.globalPosition, false);
			}
			GUI.backgroundColor = Color.white;
		}

		// End
		EditorGUI.indentLevel--;

		// Perform any required point operation - after having drawn them all to avoid reordering issues
		switch(operation) {
			case PointOperation.SWAP: {
				m_pointsProp.MoveArrayElement(point1, point2);
				SetSelection(point2, 0, false, false);
			} break;

			case PointOperation.INSERT: {
				Vector3 newPos = targetCurve.points[point1].globalPosition;
				newPos.x += 10f;
				targetCurve.AddPoint(newPos, point1 + 1);
				SetSelection(point1 + 1, 0, false, false);
			} break;

			case PointOperation.DELETE: {
				m_pointsProp.DeleteArrayElementAtIndex(point1);
				if(targetCurve.selectedIdx == point1) {
					SetSelection(-1, 0, false, false);
				}
			} break;
		}
	}

	/// <summary>
	/// Do the editor settings section.
	/// </summary>
	private void DoEditorSettingsInspector() {
		// Resolution
		targetCurve.resolution = EditorGUILayout.IntField("Resolution", targetCurve.resolution);

		// Color
		EditorGUILayout.Space();
		EditorGUILayout.PropertyField(serializedObject.FindProperty("drawColor"), new GUIContent("Line Color"));
		EditorGUILayout.PropertyField(serializedObject.FindProperty("lineThickness"));

		// Point and pick sizes
		EditorGUILayout.Space();
		EditorGUILayout.PropertyField(serializedObject.FindProperty("pointSize"));
		EditorGUILayout.PropertyField(serializedObject.FindProperty("constantSize"));

		// Axis Lock
		EditorGUILayout.Space();
		EditorGUILayout.BeginHorizontal(); {
			EditorGUILayout.PrefixLabel("Lock Axis");

			int indentBackup = EditorGUI.indentLevel;
			EditorGUI.indentLevel = 0;
			EditorGUIUtility.labelWidth = 10f;

			GUILayout.Space(-7f);	// Alignment correction (Unity WTF)
			for(int i = 0; i < 3; ++i) {
				targetCurve.lockAxis[i] = EditorGUILayout.ToggleLeft(AXIS_LABELS[i], targetCurve.lockAxis[i], GUILayout.Width(25f));
			}

			EditorGUIUtility.labelWidth = 0f;
			EditorGUI.indentLevel = indentBackup;
		} EditorGUILayout.EndHorizontal();

		EditorGUILayout.BeginHorizontal(); {
			EditorGUILayout.PrefixLabel("Lock Handlers Axis");

			int indentBackup = EditorGUI.indentLevel;
			EditorGUI.indentLevel = 0;
			EditorGUIUtility.labelWidth = 10f;

			GUILayout.Space(-7f);	// Alignment correction (Unity WTF)
			for(int i = 0; i < 3; ++i) {
				targetCurve.lockHandlersAxis[i] = EditorGUILayout.ToggleLeft(AXIS_LABELS[i], targetCurve.lockHandlersAxis[i], GUILayout.Width(25f));
			}

			EditorGUIUtility.labelWidth = 0f;
			EditorGUI.indentLevel = indentBackup;
		} EditorGUILayout.EndHorizontal();
	}

	/// <summary>
	/// Do the tools section.
	/// </summary>
	private void DoToolsInspector() {
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
			// Make sure curve is not closed
			targetCurve.closed = false;

			// Lock the right axis for this shape
			for(int i = 0; i < 3; ++i) {
				targetCurve.lockAxis[i] = m_lineToolOffset[i] == 0f;
				targetCurve.lockHandlersAxis[i] = targetCurve.lockAxis[i];
			}

			// Apply to all points in the curve
			Vector3 pos = m_lineToolStartPoint;
			Vector3 handle1 = m_lineToolOffset / -3f;
			Vector3 handle2 = m_lineToolOffset / 3f;
			for(int i = 0; i < targetCurve.points.Count; i++) {
				BezierPoint p = targetCurve.GetPoint(i);

				bool wasLocked = p.locked;
				p.locked = false;

				p.position = pos;
				p.handle1 = handle1;
				p.handle2 = handle2;
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
			// Make sure curve is closed
			targetCurve.closed = true;

			// Lock the right axis for this shape
			targetCurve.lockAxis = new bool[] { false, true, false };
			targetCurve.lockHandlersAxis = new bool[] { false, true, false };

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

	/// <summary>
	/// Detect focus changes to see if a new point must be selected.
	/// </summary>
	private void ProcessFocusChanges() {
		// Only when something has actually changed
		if(Event.current.type != EventType.Used) return;

		// Get focused control name
		string focusedName = GUI.GetNameOfFocusedControl();

		// Nothing to do if none has the focus or it doesn't have a targeted name
		if(string.IsNullOrEmpty(focusedName)) return;

		// Parse targeted name
		string[] tokens = focusedName.Split('|');
		if(tokens.Length < 2) return;

		// Field 0: point idx [0..N-1]
		int pointIdx = -1;
		if(!int.TryParse(tokens[0], NumberStyles.Any, CultureInfo.InvariantCulture, out pointIdx)) return;

		// Field 1: handle idx [0, 1, 2]
		int handleIdx = -1;
		if(!int.TryParse(tokens[1], NumberStyles.Any, CultureInfo.InvariantCulture, out handleIdx)) return;

		// Field 3: focused control (we don't care)

		// We have a new selection candidate!
		SetSelection(pointIdx, handleIdx, false, false);
	}

	//------------------------------------------------------------------------//
	// INTERNAL UTILS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Set the current selection.
	/// </summary>
	/// <param name="_pointIdx">Point index. -1 for none.</param>
	/// <param name="_handleIdx">Handle index. -1 for none.</param>
	/// <param name="_clearFocus">Whether to focused control matching the new selection.</param>
	/// <param name="_repaint">Force a repaint of the inspector. Use it when selection is done from outside the OnGUI call.</param>
	private void SetSelection(int _pointIdx, int _handleIdx, bool _focusControl, bool _repaint) {
		// Store new values
		targetCurve.selectedIdx = _pointIdx;
		targetCurve.selectedHandlerIdx = _handleIdx;

		// Focus target control?
		if(_focusControl) GUI.FocusControl(_pointIdx + "|" + _handleIdx + "|LABEL");

		// Force a repaint?
		if(_repaint) Repaint();
	}

	/// <summary>
	/// Custom function to draw Vector3 fields.
	/// </summary>
	/// <returns>The new value for the Vector3.</returns>
	/// <param name="_label">Label.</param>
	/// <param name="_value">Value.</param>
	/// <param name="_lock">Axis Lock.</param>
	/// <param name="_focusName">Custom focus name to be able to track when this control has the focus. Suffix will be attached for every sub-control.</param>
	/// <param name="_options">Options.</param>
	private Vector3 Vector3FieldWithLock(string _label, Vector3 _value, bool[] _lock, string _focusName, params GUILayoutOption[] _options) {
		EditorGUILayout.BeginHorizontal(); {
			// Prefix label
			EditorGUILayout.PrefixLabel(_label);

			// [AOC] SUPER-DIRTY: Prefix label gets attached to the first control 
			// after, which means that if the X axis is locked, the label will be displayed disabled.
			// To prevent this, add a useless control in between with width 0 so it's not noticeable.
			// Make it an interactable control to see when the focus changes to the label
			GUI.SetNextControlName(_focusName + "|LABEL");
			EditorGUILayout.SelectableLabel("-", GUILayout.Width(0f), GUILayout.Height(0f));

			// Setup and backup
			bool wasEnabled = GUI.enabled;
			int indentBackup = EditorGUI.indentLevel;
			EditorGUI.indentLevel = 0;
			EditorGUIUtility.labelWidth = 10f;

			// 3 float fields
			GUILayout.Space(-7f);	// Alignment correction (Unity WTF)
			for(int i = 0; i < 3; ++i) {
				GUI.enabled = wasEnabled && !_lock[i];
				GUI.SetNextControlName(_focusName + "|" + AXIS_LABELS[i]);
				_value[i] = EditorGUILayout.FloatField(AXIS_LABELS[i], _value[i]);
			}

			// Restore backuped stuff
			GUI.enabled = wasEnabled;
			EditorGUIUtility.labelWidth = 0f;
			EditorGUI.indentLevel = indentBackup;
		} EditorGUILayout.EndHorizontal();

		return _value;
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