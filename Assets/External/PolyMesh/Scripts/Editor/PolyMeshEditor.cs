// PolyMeshEditor.cs
// 
// Created by Alger Ortín Castellví on 20/05/2016, imported from HSX project.
// Copyright (c) 2016 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Custom editor for the PolyMesh class.
/// </summary>
[CustomEditor(typeof(PolyMesh))]
[CanEditMultipleObjects]
public class PolyMeshEditor : Editor {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	private enum State {
		HOVER,
		DRAG,
		BOX_SELECT,
		DRAG_SELECTED,
		ROTATE_SELECTED,
		SCALE_SELECTED,
		EXTRUDE
	}

	private const float CLICK_RADIUS = 0.12f;
	private const string DEFAULT_MATERIAL_PATH = "Tools/LevelEditor/Materials/MT_LevelEditor_0";

	//------------------------------------------------------------------------//
	// MEMBERS																  //
	//------------------------------------------------------------------------//
	private FieldInfo m_undoCallback;
	private bool m_editing;
	private bool m_tabDown;
	private State m_state;

	private List<Vector3> m_keyPoints;
	private List<Vector3> m_curvePoints;
	private List<bool> m_isCurve;

	private Matrix4x4 m_worldToLocal;
	private Quaternion m_inverseRotation;
	
	private Vector3 m_mousePosition;
	private Vector3 m_clickPosition;
	private Vector3 m_screenMousePosition;
	private MouseCursor m_mouseCursor = MouseCursor.Arrow;
	private float m_snap;

	private int m_dragIndex;
	private List<int> m_selectedIndices = new List<int>();
	private int m_nearestLine;
	private Vector3 m_splitPosition;
	private bool m_extrudeKeyDown;
	private bool m_doExtrudeUpdate;
	private bool m_draggingCurve;
	private float m_resizeScale;

	private bool m_liveMode = true;
	private bool m_multiEditing = false;

	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Inspector opened.
	/// </summary>
	private void OnEnable() {
		// Multi editing?
		m_multiEditing = targets.Length > 1;

		// Show wireframe if autoEdit is true
		if(autoEdit) {
			HideWireframe(hideWireframe);
		}

		// Subscribe to the Undo event
		#if UNITY_4_3 || UNITY_4_4 || UNITY_4_5 || UNITY_4_6 || UNITY_5
			Undo.undoRedoPerformed += OnUndoRedo;
		#else
			//Crazy hack to register undo
			if(m_undoCallback == null) {
				m_undoCallback = typeof(EditorApplication).GetField("undoRedoPerformed", BindingFlags.NonPublic | BindingFlags.Static);
				if(m_undoCallback != null) {
					m_undoCallback.SetValue(null, new EditorApplication.CallbackFunction(OnUndoRedo));
				}
			}
		#endif
	}

	/// <summary>
	/// Inspector closed.
	/// </summary>
	private void OnDisable() {
		// Unsubscribe from the Undo event
		#if UNITY_4_3 || UNITY_4_4 || UNITY_4_5 || UNITY_4_6 || UNITY_5
			Undo.undoRedoPerformed -= OnUndoRedo;
		#else
			// Crazy hack to register undo
			if(m_undoCallback != null) m_undoCallback = null;
		#endif
	}

	//------------------------------------------------------------------------//
	// INSPECTOR GUI														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Raises the inspector GU event.
	/// </summary>
	public override void OnInspectorGUI() {
		// Special subset of features when multi-editing
		if(targets.Length > 1) {
			OnInspectorGUIMulti();
			return;
		}

		// Check required params
		if(target == null) {
			EditorGUILayout.HelpBox("Invalid target, an error has occurred.", MessageType.Error);
			return;
		}

		// Aux vars
		bool wasEnabled = GUI.enabled;

		//serializedObject.Update ();

		// If no polymesh was created, do it now
		if(polyMesh.keyPoints.Count == 0) {
			CreateSquare(polyMesh, 4f);
		}

		// Live mode
		EditorGUILayout.BeginHorizontal(); {
			m_liveMode = EditorGUILayout.Toggle("Live Mode", m_liveMode);
			GUILayout.Label("Disable live mode if performance is slow.", CustomEditorStyles.commentLabelLeft);
		} EditorGUILayout.EndHorizontal();

		// Edit mode: if autoEdit is enabled, don't show/enable the start/stop editing (do it automatically)
		GUI.enabled = !autoEdit;
		if(m_editing) {
			if(GUILayout.Button("Stop Editing", GUILayout.Height(40f))) {
				m_editing = false;
				HideWireframe(false);
			}
		} else if(GUILayout.Button("Edit PolyMesh " + (autoEdit ? "\n(not available with AutoEdit)" : ""), GUILayout.Height(40f))) {
			m_editing = true;
			HideWireframe(hideWireframe);
		}
		GUI.enabled = wasEnabled;

		// Add renderer?
		MeshRenderer renderer = polyMesh.GetComponent<MeshRenderer>();
		if(renderer == null) {
			if(GUILayout.Button("Add Renderer", GUILayout.Height(40f))) {
				renderer = polyMesh.gameObject.AddComponent<MeshRenderer>();
				renderer.material = AssetDatabase.LoadAssetAtPath<Material>("Assets/" + DEFAULT_MATERIAL_PATH + ".mat");
			}
		} else {
			if(GUILayout.Button("Remove Renderer", GUILayout.Height(40f))) {
				DestroyImmediate(renderer);
				renderer = null;
			}
		}

		// Duplicate
		if(GUILayout.Button("Duplicate", GUILayout.Height(40f))) {
			PolyMesh newPolyMesh = DuplicateMesh(polyMesh);
			EditorUtils.FocusObject(newPolyMesh.gameObject, true, false, true);
		}

		// Scale fixer (single polymesh)
		/*if(GUILayout.Button("Fix Scale", GUILayout.Height(40f))) {
			// Scale mesh keypoints so they keep the position when the mesh has scale 1
			Vector3 fixScaleFactor = polyMesh.transform.localScale;
			for(int i = 0; i < m_keyPoints.Count; i++) {
				// For some reason scaling directly the list entry doesn't work :/
				Vector3 v = m_keyPoints[i];
				v.Scale(fixScaleFactor);
				m_keyPoints[i] = v;
			}
			UpdatePoly(false, true);

			// Set the mesh scale to 1
			RecordUndo();
			polyMesh.transform.localScale = Vector3.one;
		}

		// Scale fixer (all scene polymeshes)
		if(GUILayout.Button("Fix scales on all scene!", GUILayout.Height(40f))) {
			// Aux vars
			Vector3 v;

			// Find all polymeshes in the scene
			PolyMesh[] polymeshes = GameObject.FindObjectsOfType<PolyMesh>();
			foreach(PolyMesh p in polymeshes) {
				// Skip if mesh scale is already 1
				if(p.transform.localScale == Vector3.one) continue;
				Debug.Log("Fixing " + p.name + "...");

				// Undo
				RecordUndo(p);

				// Scale mesh keypoints so they keep the position when the mesh has scale 1
				Vector3 fixScaleFactor = p.transform.localScale;
				for(int i = 0; i < p.keyPoints.Count; i++) {
					// Fix keypoint
					// For some reason scaling directly the list entry doesn't work :/
					v = p.keyPoints[i];
					v.Scale(fixScaleFactor);
					p.keyPoints[i] = v;
				}

				// Special case for curve meshes
				for(int i = 0; i < p.keyPoints.Count; i++) {
					if(!p.isCurve[i]) {
						p.curvePoints[i] = Vector3.Lerp(p.keyPoints[i], p.keyPoints[(i + 1) % p.keyPoints.Count], 0.5f);
					}
				}

				// Rebuild mesh!
				p.BuildMesh();

				// Reset mesh scale to 1
				RecordUndo(p);
				p.transform.localScale = Vector3.one;

				Debug.Log("Done!");
			}
		}*/

		EditorGUILayoutExt.Separator();

		// Mesh settings
		if(meshSettings = EditorGUILayout.Foldout(meshSettings, "Mesh")) {
			// Indent in
			EditorGUI.indentLevel++;

			var pinkOffset = EditorGUILayout.FloatField("PinkMesh z Offset", polyMesh.pinkMeshOffset);
			var showNormals = EditorGUILayout.Toggle("Display Normals", polyMesh.showNormals);
			var showOutline = EditorGUILayout.Toggle("Display Outline", polyMesh.showOutline);
			var curveDetail = EditorGUILayout.Slider("Curve Detail", polyMesh.curveDetail, 0.01f, 1f);
			curveDetail = Mathf.Clamp(curveDetail, 0.01f, 1f);

			if(GUI.changed) {
				RecordUndo();
				polyMesh.pinkMeshOffset = pinkOffset;
				polyMesh.showNormals = showNormals;
				polyMesh.showOutline = showOutline;
				polyMesh.curveDetail = curveDetail;
			}

			// Buttons
			EditorGUILayout.BeginHorizontal(); {
				m_resizeScale = EditorGUILayout.FloatField("Scale", m_resizeScale);
				if(GUILayout.Button("Re-Scale")) {
					RescalePoints(m_resizeScale);
				}
			} EditorGUILayout.EndHorizontal();

			EditorGUILayout.BeginHorizontal(); {
				// [AOC] Hardcode Hack to make buttons respect indentation and have the same size
				int numItems = 2;
				Rect rect = EditorGUI.IndentedRect(new Rect(0, 0, EditorGUIUtility.currentViewWidth, 10f));
				rect.width -= rect.x + numItems * 4f;	// Remove indentation and (hardcoded) spacing between items

				GUILayout.Space(rect.x);

				if(GUILayout.Button("Rebuild Mesh", GUILayout.Width(rect.width/numItems))) {
					polyMesh.RebuildMesh();
				}

				if(GUILayout.Button("Save Mesh to Library", GUILayout.Width(rect.width/numItems))) {
					PolyMesh pM = target as PolyMesh;
					GameObject root = PrefabUtility.FindPrefabRoot(pM.gameObject);
					Object parentObject = PrefabUtility.GetPrefabParent(root);
					string path = AssetDatabase.GetAssetPath(parentObject);
					path = path.Replace(".prefab", "_ColliderMesh.asset");
					UnityEngine.Debug.Log("path: " + path);
					AssetDatabase.CreateAsset(polyMesh.meshCollider.sharedMesh, path);
					AssetDatabase.SaveAssets();              
				}
			} EditorGUILayout.EndHorizontal();

			EditorGUILayout.BeginHorizontal(); {
				// [AOC] Hardcode Hack to make buttons respect indentation and have the same size
				int numItems = 2;
				Rect rect = EditorGUI.IndentedRect(new Rect(0, 0, EditorGUIUtility.currentViewWidth, 10f));
				rect.width -= rect.x + numItems * 4f;	// Remove indentation and (hardcoded) spacing between items

				GUILayout.Space(rect.x);

				if(GUILayout.Button("Update Mesh", GUILayout.Width(rect.width/numItems))) {
					RecordUndo();
					polyMesh.BuildMesh();
				}

				if(GUILayout.Button("Invert Triangles", GUILayout.Width(rect.width/numItems))) {
					RecordUndo();
					polyMesh.InvertPoints();
					polyMesh.BuildMesh();
				}
			} EditorGUILayout.EndHorizontal();

			// Indent back out
			EditorGUI.indentLevel--;
			EditorGUILayoutExt.Separator();
		}

		// Collider section
		if(colliderSettings = EditorGUILayout.Foldout(colliderSettings, "Collider")) {
			// Indent in
			EditorGUI.indentLevel++;

			//Collider depth
			EditorGUILayout.PropertyField(serializedObject.FindProperty("colliderDepth"));
			//var colliderDepth = EditorGUILayout.FloatField("Depth", polyMesh.colliderDepth);
			//colliderDepth = Mathf.Max(colliderDepth, 0.01f);

			var buildColliderEdges = EditorGUILayout.Toggle("Build Edges", polyMesh.buildColliderEdges);
			var buildColliderFront = EditorGUILayout.Toggle("Build Font", polyMesh.buildColliderFront);
			if(GUI.changed) {
				RecordUndo();
				//polyMesh.colliderDepth = colliderDepth;
				polyMesh.buildColliderEdges = buildColliderEdges;
				polyMesh.buildColliderFront = buildColliderFront;
			}

			// [AOC] We will have the collider to the same object containing the polymesh, for clarity and keeping the hierarchy clean
			//		 This is automatically handled by Unity's [RequireComponent] attribute that has been added to the PolyMesh class definition
			/*
			EditorGUILayout.PropertyField(serializedObject.FindProperty("colliderParent"));
			EditorGUILayout.ObjectField("Collider", polyMesh.meshCollider, typeof(MeshCollider), true);

			//Destroy collider
			if(polyMesh.meshCollider == null) {
				if(GUILayout.Button("Create Collider")) {
					RecordDeepUndo();
					var obj = new GameObject("Collider", typeof(MeshCollider));
					polyMesh.meshCollider = obj.GetComponent<MeshCollider>();
					
					if(polyMesh.colliderParent == null) {
						obj.transform.parent = polyMesh.transform;
					}
					else {
						obj.transform.parent = polyMesh.colliderParent;
					}
					obj.transform.position = polyMesh.transform.position;
					obj.transform.localScale = polyMesh.transform.localScale;
				}
			} else {
				if(GUILayout.Button("Destroy Collider")) {
					RecordDeepUndo();
					DestroyImmediate(polyMesh.meshCollider.gameObject);
					polyMesh.meshCollider = null;
				}
			}
			*/

			// [AOC] Add this button instead :P
			if(GUILayout.Button("Re-create Collider")) {
				RecordUndo();
				polyMesh.BuildMesh();	// This should do the trick
			}

			// Indent back out
			EditorGUI.indentLevel--;
			EditorGUILayoutExt.Separator();
		}

		// UV settings
		if(uvSettings = EditorGUILayout.Foldout(uvSettings, "UVs")) {
			// Indent in
			EditorGUI.indentLevel++;

			var uvPosition = EditorGUILayout.Vector2Field("Position", polyMesh.uvPosition);
			var uvScale = EditorGUILayout.FloatField("Scale", polyMesh.uvScale);
			var uvRotation = EditorGUILayout.Slider("Rotation", polyMesh.uvRotation, -180, 180) % 360;
			if(uvRotation < -180) uvRotation += 360;

			if(GUI.changed) {
				RecordUndo();
				polyMesh.uvPosition = uvPosition;
				polyMesh.uvScale = uvScale;
				polyMesh.uvRotation = uvRotation;
			}

			if(GUILayout.Button("Reset UVs")) {
				polyMesh.uvPosition = Vector3.zero;
				polyMesh.uvScale = 1;
				polyMesh.uvRotation = 0;
			}

			// Indent back out
			EditorGUI.indentLevel--;
			EditorGUILayoutExt.Separator();
		}

		// Update mesh
		if(GUI.changed) {
			polyMesh.BuildMesh();
			serializedObject.ApplyModifiedProperties();
		}

		// Merge section
		if(mergeObjects = EditorGUILayout.Foldout(mergeObjects, "Merge")) {
			// Indent in
			EditorGUI.indentLevel++;
			polyMesh.mergeObject = (GameObject)EditorGUILayout.ObjectField(polyMesh.mergeObject, typeof(GameObject), true);
			polyMesh.mergeStartPoint = EditorGUILayout.IntField("myStartPoint", polyMesh.mergeStartPoint);
			polyMesh.mergeEndPoint = EditorGUILayout.IntField("myEndPoint", polyMesh.mergeEndPoint);
			if(GUILayout.Button("Merge")) {
				//MergeMeshes(polyMesh.mergeObject.GetComponent<PolyMesh>().keyPoints);
				polyMesh.MergeMeshes();
			}

			// Indent back out
			EditorGUI.indentLevel--;
			EditorGUILayoutExt.Separator();
		}

		// Split section
		if(splitSection = EditorGUILayout.Foldout(splitSection, "Split")) {
			// Indent in
			EditorGUI.indentLevel++;

			// Point indexes
			// First point
			EditorGUI.BeginChangeCheck();
			int splitPoint0 = EditorGUILayout.IntField("Point 0", m_selectedIndices.Count > 0 ? m_selectedIndices[0] : -1);
			if(EditorGUI.EndChangeCheck()) {
				// Make sure it's a valid point
				if(splitPoint0 >= 0 && splitPoint0 < m_keyPoints.Count) {
					// Make it the first in the selection
					if(m_selectedIndices.Count > 0) {
						m_selectedIndices[0] = splitPoint0;
					} else {
						m_selectedIndices.Add(splitPoint0);
					}

					// Clear the rest of selected points
					if(m_selectedIndices.Count > 2) m_selectedIndices.RemoveRange(2, m_selectedIndices.Count - 2);
				}
			}

			// Second point
			EditorGUI.BeginChangeCheck();
			int splitPoint1 = EditorGUILayout.IntField("Point 1", m_selectedIndices.Count > 1 ? m_selectedIndices[1] : -1);
			if(EditorGUI.EndChangeCheck()) {
				// Make sure it's a valid point
				if(splitPoint1 >= 0 && splitPoint1 < m_keyPoints.Count) {
					// Make it the second in the selection
					if(m_selectedIndices.Count > 1) {
						m_selectedIndices[1] = splitPoint1;
					} else if(m_selectedIndices.Count == 1) {
						m_selectedIndices.Add(splitPoint1);
					} else if(m_selectedIndices.Count == 0) {
						m_selectedIndices.Add(0);	// Add a point first
						m_selectedIndices.Add(splitPoint1);
					}

					// Clear the rest of selected points
					if(m_selectedIndices.Count > 2) m_selectedIndices.RemoveRange(2, m_selectedIndices.Count - 2);
				}
			}

			// Do it!
			if(GUILayout.Button("Split Mesh")) {
				// Don't check anything, the split method will show an error if some of the parameters are not good
				TrySplitMesh(false);
			}

			// Indent back out
			EditorGUI.indentLevel--;
			EditorGUILayoutExt.Separator();
		}

		// Editor settings
		if(editorSettings = EditorGUILayout.Foldout(editorSettings, "Editor Settings")) {
			// Indent in
			EditorGUI.indentLevel++;

			// General settings
			EditorGUILayout.BeginVertical(); {
				EditorGUILayout.LabelField("General Settings", EditorStyles.boldLabel);
				EditorGUI.indentLevel++;

				EditorGUILayout.BeginHorizontal(); {
					autoEdit = EditorGUILayout.Toggle("Auto Edit", autoEdit);
					GUILayout.Label("Automatically enter edit mode upon selecting this object?", CustomEditorStyles.commentLabelLeft);
				} EditorGUILayout.EndHorizontal();

				EditorGUI.BeginChangeCheck();
				hideWireframe = EditorGUILayout.Toggle("Hide Wireframe", hideWireframe);
				if(EditorGUI.EndChangeCheck()) {
					HideWireframe(hideWireframe);
				}

				EditorGUI.indentLevel--;
			} EditorGUILayout.EndVertical();

			// Snapping
			EditorGUILayout.BeginVertical(); {
				EditorGUILayout.Space();
				EditorGUILayout.LabelField("Snapping", EditorStyles.boldLabel);
				EditorGUI.indentLevel++;

				gridSnap = EditorGUILayout.FloatField("Grid Snap Size", gridSnap, GUILayout.ExpandWidth(false));

				EditorGUILayout.BeginHorizontal(); {
					autoSnap = EditorGUILayout.Toggle("Auto Snap", autoSnap);
					GUILayout.Label("Always snap? If set to false, points can be snapped by holding the " + (Application.platform == RuntimePlatform.OSXEditor ? "Command" : "Control") + " key while dragging it.", CustomEditorStyles.commentLabelLeft);
				} EditorGUILayout.EndHorizontal();

				EditorGUILayout.BeginHorizontal(); {
					globalSnap = EditorGUILayout.Toggle("Global Snap", globalSnap);
					GUILayout.Label("Whether to use local or global coordinates for the snapping.", CustomEditorStyles.commentLabelLeft);
				} EditorGUILayout.EndHorizontal();

				EditorGUI.indentLevel--;
			} EditorGUILayout.EndVertical();

			// Key shortcuts
			EditorGUILayout.BeginVertical(); {
				EditorGUILayout.Space();
				EditorGUILayout.LabelField("Hot Keys", EditorStyles.boldLabel);
				EditorGUI.indentLevel++;

				editKey = (KeyCode)EditorGUILayout.EnumPopup("Toggle Edit", editKey);

				selectAllKey = (KeyCode)EditorGUILayout.EnumPopup("Select All", selectAllKey);

				EditorGUILayout.Space();
				createPointKey = (KeyCode)EditorGUILayout.EnumPopup("Create Point", createPointKey);

				deletePointKey = (KeyCode)EditorGUILayout.EnumPopup("Delete Point", deletePointKey);

				EditorGUILayout.Space();
				extrudeKey = (KeyCode)EditorGUILayout.EnumPopup("Extrude", extrudeKey);
				splitMeshKey = (KeyCode)EditorGUILayout.EnumPopup("Split Mesh", splitMeshKey);

				EditorGUILayout.Space();
				string controlKeyName = (Application.platform == RuntimePlatform.OSXEditor ? "Command" : "Control");
				EditorGUILayout.Popup("Add to selection", 0, new string[] { controlKeyName });
				EditorGUILayout.Popup("Snap to grid", 0, new string[] { "Alt" });

				EditorGUI.indentLevel--;
			} EditorGUILayout.EndVertical();

			// Indent back out
			EditorGUI.indentLevel--;
			EditorGUILayoutExt.Separator();
		}

		//serializedObject.ApplyModifiedProperties ();
	}

	/// <summary>
	/// Special subset of features when multi-editing
	/// </summary>
	private void OnInspectorGUIMulti() {
		// Info
		EditorGUILayout.HelpBox("Only a small subset of features are supported when editing multiple objects.\nAsk the developers if you would like a feature to be supported in multi-editing mode.", MessageType.Info);

		// Add/Remove renderer component
		EditorGUILayout.BeginHorizontal(GUILayout.Height(40f)); {
			// Add renderer
			if(GUILayout.Button("Add Renderer", GUILayout.ExpandHeight(true))) {
				PolyMesh p = null;
				MeshRenderer renderer = null;
				for(int i = 0; i < targets.Length; i++) {
					// Add renderer if not already added
					p = targets[i] as PolyMesh;
					renderer = p.GetComponent<MeshRenderer>();
					if(renderer == null) {
						renderer = p.gameObject.AddComponent<MeshRenderer>();
						renderer.material = AssetDatabase.LoadAssetAtPath<Material>("Assets/" + DEFAULT_MATERIAL_PATH + ".mat");
					}
				}
			}

			// Remove renderer
			if(GUILayout.Button("Remove Renderer", GUILayout.ExpandHeight(true))) {
				PolyMesh p = null;
				MeshRenderer renderer = null;
				for(int i = 0; i < targets.Length; i++) {
					// Remove renderer (if any)
					p = targets[i] as PolyMesh;
					renderer = p.GetComponent<MeshRenderer>();
					if(renderer != null) {
						DestroyImmediate(renderer);
						renderer = null;
					}
				}
			}
		} EditorGUILayout.EndHorizontal();
	}

	//------------------------------------------------------------------------//
	// SCENE GUI															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Raises the scene GU event.
	/// </summary>
	private void OnSceneGUI() {
		// Skip if no valid target
		if(target == null) return;

		// Skip for multiedit
		if(m_multiEditing) return;

		// Toggle editing - ignore if autoEdit
		if(KeyPressed(editKey) && !autoEdit) {
			m_editing = !m_editing;
		}

		if(m_editing || autoEdit) {
			if(e.type == EventType.keyDown && e.keyCode == KeyCode.Keypad0) {
				if(polyMesh.transform.GetComponent<MeshRenderer>().enabled) {
					polyMesh.transform.GetComponent<MeshRenderer>().enabled = false;
				} else {
					polyMesh.transform.GetComponent<MeshRenderer>().enabled = true;
				}
			}

			//Update lists
			if(m_keyPoints == null) {
				m_keyPoints = new List<Vector3>(polyMesh.keyPoints);
				m_curvePoints = new List<Vector3>(polyMesh.curvePoints);
				m_isCurve = new List<bool>(polyMesh.isCurve);
			}

			//Load handle matrix
			Handles.matrix = polyMesh.transform.localToWorldMatrix;

			//Draw points and lines
			DrawAxis();
			Handles.color = Color.white;
			for(int i = 0; i < m_keyPoints.Count; i++) {
				Handles.color = m_nearestLine == i ? Color.green : Color.white;
				DrawSegment(i);

				if(m_selectedIndices.Contains(i)) {
					Handles.color = Color.green;
					DrawCircle(m_keyPoints[i], 0.08f);
				} else {
					Handles.color = Color.white;
				}

				DrawKeyPoint(i);
				if(m_isCurve[i]) {
					Handles.color = (m_draggingCurve && m_dragIndex == i) ? Color.white : Color.blue;
					DrawCurvePoint(i);
				}
				Handles.Label(m_keyPoints[i], i.ToString());
			}

			//Quit on tool change
			if(e.type == EventType.KeyDown) {
				switch(e.keyCode) {
					case KeyCode.Q:
					case KeyCode.W:
					case KeyCode.E:
					case KeyCode.R: {
						return;
					} break;

					// [AOC] Clear selection upon pressing escape to allow selection another object
					case KeyCode.Escape: {
						Selection.activeGameObject = null;
						return;
					} break;
				}
			}

			//Quit if panning or no camera exists
			if(Tools.current == Tool.View || (e.isMouse && e.button > 0) || Camera.current == null || e.type == EventType.ScrollWheel) {
				return;
			}

			//Quit if laying out
			if(e.type == EventType.Layout) {
				HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
				return;
			}

			//Cursor rectangle
			EditorGUIUtility.AddCursorRect(new Rect(0, 0, Camera.current.pixelWidth, Camera.current.pixelHeight), m_mouseCursor);
			m_mouseCursor = MouseCursor.Arrow;

			//Extrude key state
			if(e.keyCode == extrudeKey) {
				if(m_extrudeKeyDown) {
					if(e.type == EventType.KeyUp) {
						m_extrudeKeyDown = false;
					}
				} else if(e.type == EventType.KeyDown) {
					m_extrudeKeyDown = true;
				}
			}

			//Update matrices and snap
			m_worldToLocal = polyMesh.transform.worldToLocalMatrix;
			m_inverseRotation = Quaternion.Inverse(polyMesh.transform.rotation) * Camera.current.transform.rotation;
			m_snap = gridSnap;
			
			//Update mouse position
			m_screenMousePosition = new Vector3(e.mousePosition.x, Camera.current.pixelHeight - e.mousePosition.y);
			var plane = new Plane(-polyMesh.transform.forward, polyMesh.transform.position);
			var ray = Camera.current.ScreenPointToRay(m_screenMousePosition);
			float hit;
			if(plane.Raycast(ray, out hit)) {
				m_mousePosition = m_worldToLocal.MultiplyPoint(ray.GetPoint(hit));
			} else {
				return;
			}

			//Update nearest line and split position
			m_nearestLine = NearestLine(out m_splitPosition);
			
			//Update the state
			var newState = UpdateState();
			if(m_state != newState) {
				SetState(newState);
			}

			// Repaint scene
			HandleUtility.Repaint();

			// Mark event as used
			//e.Use();
		}
	}

	/// <summary>
	/// Hides the wireframe.
	/// </summary>
	/// <param name="hide">If set to <c>true</c> hide.</param>
	private void HideWireframe(bool hide) {
		if(polyMesh.GetComponent<Renderer>() != null) {
			EditorUtility.SetSelectedWireframeHidden(polyMesh.GetComponent<Renderer>(), hide);
		}
	}

	/// <summary>
	/// Records the undo.
	/// </summary>
	private void RecordUndo(Object _target = null) {
		// [AOC] Use default target if none specified
		if(_target == null) _target = target;

#if UNITY_4_3 || UNITY_4_4 || UNITY_4_5 || UNITY_4_6 || UNITY_5
		Undo.RecordObject(_target, "PolyMesh Changed");
#else
		Undo.RegisterUndo(_target, "PolyMesh Changed");
#endif
	}

	/// <summary>
	/// Records the deep undo.
	/// </summary>
	private void RecordDeepUndo(Object _target = null) {
		// [AOC] Use default target if none specified
		if(_target == null) _target = target;

#if UNITY_4_3 || UNITY_4_4 || UNITY_4_5 || UNITY_4_6 || UNITY_5
		Undo.RegisterFullObjectHierarchyUndo(_target, "PolyMesh Changed");
#else
		Undo.RegisterSceneUndo("PolyMesh Changed");
#endif
	}

	//------------------------------------------------------------------------//
	// STATE CONTROL METHODS												  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Sets the state.
	/// </summary>
	/// <param name="newState">New state.</param>
	private void SetState(State newState) {
		m_state = newState;
		switch(m_state) {
			case State.HOVER:
			break;
		}
	}

	/// <summary>
	/// Updates the state.
	/// </summary>
	/// <returns>The state.</returns>
	private State UpdateState() {
		switch(m_state) {
			//Hovering
			case State.HOVER: {
				DrawNearestLineAndSplit();

				// [AOC] Select a point by single-clicking on it
				TrySelectPoint(); // Don't return! Allow select + drag a point at the same frame ^^

				if((Tools.current == Tool.Move || Tools.current == Tool.Rect) && TryDragSelected())		return State.DRAG_SELECTED;
				if(Tools.current == Tool.Rotate && TryRotateSelected())									return State.ROTATE_SELECTED;
				if(Tools.current == Tool.Scale && TryScaleSelected())									return State.SCALE_SELECTED;
				if((Tools.current == Tool.Move || Tools.current == Tool.Rect) && TryExtrude())			return State.EXTRUDE;

				if(TrySelectAll()) 		return State.HOVER;
				if(TrySplitLine()) 		return State.HOVER;
				if(TryDeleteSelected())	return State.HOVER;

				// [AOC] Split mesh by key
				if(TrySplitMesh(true)) 	return State.HOVER;

				//if(TryHoverCurvePoint(out dragIndex) && TryDragCurvePoint(dragIndex))	return State.Drag;
				if(TryHoverKeyPoint(out m_dragIndex) && TryDragKeyPoint(m_dragIndex)) 		return State.DRAG;
				if(TryBoxSelect()) 															return State.BOX_SELECT;
			} break;

			//Dragging
			case State.DRAG: {
				m_mouseCursor = MouseCursor.MoveArrow;
				DrawCircle(m_keyPoints[m_dragIndex], CLICK_RADIUS);
				if(m_draggingCurve) {
					MoveCurvePoint(m_dragIndex, m_mousePosition - m_clickPosition);
				} else {
					MoveKeyPoint(m_dragIndex, m_mousePosition - m_clickPosition);
				}
				if(TryStopDrag()) return State.HOVER;
			} break;

			//Box Selecting
			case State.BOX_SELECT: {
				if(TryBoxSelectEnd()) return State.HOVER;
			} break;

			//Dragging selected
			case State.DRAG_SELECTED: {
				m_mouseCursor = MouseCursor.MoveArrow;
				MoveSelected(m_mousePosition - m_clickPosition);
				if(TryStopDrag()) return State.HOVER;
			} break;

			//Rotating selected
			case State.ROTATE_SELECTED: {
				m_mouseCursor = MouseCursor.RotateArrow;
				RotateSelected();
				if(TryStopDrag()) return State.HOVER;
			} break;

			//Scaling selected
			case State.SCALE_SELECTED: {
				m_mouseCursor = MouseCursor.ScaleArrow;
				ScaleSelected();
				if(TryStopDrag()) return State.HOVER;
			} break;

			//Extruding
			case State.EXTRUDE: {
				m_mouseCursor = MouseCursor.MoveArrow;
				MoveSelected(m_mousePosition - m_clickPosition);
				if(m_doExtrudeUpdate && m_mousePosition != m_clickPosition) {
					UpdatePoly(false, false);
					m_doExtrudeUpdate = false;
				}
				if(TryStopDrag()) return State.HOVER;
			} break;
		}
		return m_state;
	}

	/// <summary>
	/// Raises the undo redo event.
	/// </summary>
	private void OnUndoRedo() {
		// Update the mesh on undo/redo
		m_keyPoints = new List<Vector3>(polyMesh.keyPoints);
		m_curvePoints = new List<Vector3>(polyMesh.curvePoints);
		m_isCurve = new List<bool>(polyMesh.isCurve);
		polyMesh.BuildMesh();
	}

	/// <summary>
	/// Loads the poly.
	/// </summary>
	private void LoadPoly() {
		for(int i = 0; i < m_keyPoints.Count; i++) {
			m_keyPoints[i] = polyMesh.keyPoints[i];
			m_curvePoints[i] = polyMesh.curvePoints[i];
			m_isCurve[i] = polyMesh.isCurve[i];
		}
	}

	/// <summary>
	/// Transforms the poly.
	/// </summary>
	/// <param name="matrix">Matrix.</param>
	private void TransformPoly(Matrix4x4 matrix) {
		for(int i = 0; i < m_keyPoints.Count; i++) {
			m_keyPoints[i] = matrix.MultiplyPoint(polyMesh.keyPoints[i]);
			m_curvePoints[i] = matrix.MultiplyPoint(polyMesh.curvePoints[i]);
		}
	}

	/// <summary>
	/// Updates the poly.
	/// </summary>
	/// <param name="sizeChanged">If set to <c>true</c> size changed.</param>
	/// <param name="recordUndo">If set to <c>true</c> record undo.</param>
	private void UpdatePoly(bool sizeChanged, bool recordUndo) {
		if(recordUndo) {
			RecordUndo();
		}

		if(sizeChanged) {
			polyMesh.keyPoints = new List<Vector3>(m_keyPoints);
			polyMesh.curvePoints = new List<Vector3>(m_curvePoints);
			polyMesh.isCurve = new List<bool>(m_isCurve);
		} else {
			for(int i = 0; i < m_keyPoints.Count; i++) {
				polyMesh.keyPoints[i] = m_keyPoints[i];
				polyMesh.curvePoints[i] = m_curvePoints[i];
				polyMesh.isCurve[i] = m_isCurve[i];
			}
		}

		for(int i = 0; i < m_keyPoints.Count; i++) {
			if(!m_isCurve[i]) {
				polyMesh.curvePoints[i] = m_curvePoints[i] = Vector3.Lerp(m_keyPoints[i], m_keyPoints[(i + 1) % m_keyPoints.Count], 0.5f);
			}
		}

		if(m_liveMode) {
			polyMesh.BuildMesh();
		}
	}

	/// <summary>
	/// Moves the key point.
	/// </summary>
	/// <param name="index">Index.</param>
	/// <param name="amount">Amount.</param>
	private void MoveKeyPoint(int index, Vector3 amount) {
		var moveCurve = m_selectedIndices.Contains((index + 1) % m_keyPoints.Count);
		if(doSnap) {
			if(globalSnap) {
				m_keyPoints[index] = Snap(polyMesh.keyPoints[index] + amount);
				if(moveCurve) {
					m_curvePoints[index] = Snap(polyMesh.curvePoints[index] + amount);
				}
			} else {
				amount = Snap(amount);
				m_keyPoints[index] = polyMesh.keyPoints[index] + amount;
				if(moveCurve) {
					m_curvePoints[index] = polyMesh.curvePoints[index] + amount;
				}
			}
		} else {
			m_keyPoints[index] = polyMesh.keyPoints[index] + amount;
			if(moveCurve) {
				m_curvePoints[index] = polyMesh.curvePoints[index] + amount;
			}
		}
	}

	/// <summary>
	/// Moves the curve point.
	/// </summary>
	/// <param name="index">Index.</param>
	/// <param name="amount">Amount.</param>
	private void MoveCurvePoint(int index, Vector3 amount) {
		m_isCurve[index] = true;
		if(doSnap) {
			if(globalSnap) {
				m_curvePoints[index] = Snap(polyMesh.curvePoints[index] + amount);
			} else {
				m_curvePoints[index] = polyMesh.curvePoints[index] + amount;
			}
		} else {
			m_curvePoints[index] = polyMesh.curvePoints[index] + amount;
		}
	}

	/// <summary>
	/// Moves the selected.
	/// </summary>
	/// <param name="amount">Amount.</param>
	private void MoveSelected(Vector3 amount) {
		foreach(var i in m_selectedIndices) {
			MoveKeyPoint(i, amount);
		}
	}

	/// <summary>
	/// Rotates the selected.
	/// </summary>
	private void RotateSelected() {
		var center = GetSelectionCenter();

		Handles.color = Color.white;
		Handles.DrawLine(center, m_clickPosition);
		Handles.color = Color.green;
		Handles.DrawLine(center, m_mousePosition);

		var clickOffset = m_clickPosition - center;
		var mouseOffset = m_mousePosition - center;
		var clickAngle = Mathf.Atan2(clickOffset.y, clickOffset.x);
		var mouseAngle = Mathf.Atan2(mouseOffset.y, mouseOffset.x);
		var angleOffset = mouseAngle - clickAngle;

		foreach(var i in m_selectedIndices) {
			var point = polyMesh.keyPoints[i];
			var pointOffset = point - center;
			var a = Mathf.Atan2(pointOffset.y, pointOffset.x) + angleOffset;
			var d = pointOffset.magnitude;
			m_keyPoints[i] = center + new Vector3(Mathf.Cos(a) * d, Mathf.Sin(a) * d);
		}
	}

	/// <summary>
	/// Scales the selected.
	/// </summary>
	private void ScaleSelected() {
		Handles.color = Color.green;
		Handles.DrawLine(m_clickPosition, m_mousePosition);

		var center = GetSelectionCenter();
		var scale = m_mousePosition - m_clickPosition;

		//Uniform scaling if shift pressed
		if(e.shift) {
			if(Mathf.Abs(scale.x) > Mathf.Abs(scale.y))
				scale.y = scale.x;
			else
				scale.x = scale.y;
		}

		//Determine direction of scaling
		if(scale.x < 0)
			scale.x = 1 / (-scale.x + 1);
		else
			scale.x = 1 + scale.x;
		if(scale.y < 0)
			scale.y = 1 / (-scale.y + 1);
		else
			scale.y = 1 + scale.y;

		foreach(var i in m_selectedIndices) {
			var point = polyMesh.keyPoints[i];
			var offset = point - center;
			offset.x *= scale.x;
			offset.y *= scale.y;
			m_keyPoints[i] = center + offset;
		}
	}
	
	/// <summary>
	/// Merges the meshes.
	/// </summary>
	/// <param name="points">Points.</param>
	public void MergeMeshes(List<Vector3> points) {
		if(polyMesh.mergeObject) {	
			int myStartPoint = polyMesh.mergeStartPoint; //selectedIndices[0];
			int myEndPoint = polyMesh.mergeEndPoint; //selectedIndices[1];
			
			int otherStartPoint = 0;
			int otherEndPoint = 0;
			for(int i = 1; i < points.Count; i++) {
				if(Vector3.Distance(points[i], m_keyPoints[myStartPoint]) < Vector3.Distance(points[otherStartPoint], m_keyPoints[myStartPoint])) {
					otherStartPoint = i;
				}
				if(Vector3.Distance(points[i], m_keyPoints[myEndPoint]) < Vector3.Distance(points[otherEndPoint], m_keyPoints[myEndPoint])) {
					otherEndPoint = i;
				}
			}

			UnityEngine.Debug.Log("otherStartPoint: " + otherStartPoint);
			UnityEngine.Debug.Log("otherEndPoint: " + otherEndPoint);
			UnityEngine.Debug.Log("otherMaxPoints: " + points.Count);

			// remove start and end points (remove points inbetween)
			int count = myEndPoint < myStartPoint ? m_keyPoints.Count - myStartPoint : myEndPoint - myStartPoint + 1;
			m_keyPoints.RemoveRange(myStartPoint, count);
			
			if(myEndPoint < myStartPoint) {
				m_keyPoints.RemoveRange(0, myEndPoint + 1);
				myStartPoint -= myEndPoint + 1;
			}
			
			// loop through the rest of the points as the start is different
			for(int i = 0; i < points.Count; i++) {
				int ii = (i + otherStartPoint) % points.Count;
				
				// no wrap around
				bool mergePoint = ii >= otherStartPoint && ii <= (otherEndPoint < otherStartPoint ? points.Count - 1 : otherEndPoint);
				// if we have a wrap around
				if(i + otherStartPoint >= points.Count) {
					mergePoint = ii < otherEndPoint;
				}
					
				if(mergePoint) {
					Vector3 mPoint = points[ii];
					m_mousePosition = mPoint;
					//nearestLine = NearestLine(out mPoint);
					
					int line = myStartPoint + i;
					if(line == m_keyPoints.Count + 1) {
						m_keyPoints.Add(mPoint);
						m_curvePoints.Add(Vector3.zero);
						m_isCurve.Add(false);
					}
					else {
						m_keyPoints.Insert(line, mPoint);
						m_curvePoints.Insert(line, Vector3.zero);
						m_isCurve.Insert(line, false);
					}
					
					mPoint = points[ii];
					//keyPoints[line] = mPoint;
					UpdatePoly(true, true);
				} else {
					Debug.Log("Chucked Away: " + ii);
				}
			}
		}
	}

	//------------------------------------------------------------------------//
	// DRAWING METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Draws the axis.
	/// </summary>
	private void DrawAxis() {
		Handles.color = Color.red;
		var size = HandleUtility.GetHandleSize(Vector3.zero) * 0.1f;
		Handles.DrawLine(new Vector3(-size, 0), new Vector3(size, 0));
		Handles.DrawLine(new Vector3(0, -size), new Vector2(0, size));
	}

	/// <summary>
	/// Draws the key point.
	/// </summary>
	/// <param name="index">Index.</param>
	private void DrawKeyPoint(int index) {
		Handles.DotCap(0, m_keyPoints[index], Quaternion.identity, HandleUtility.GetHandleSize(m_keyPoints[index]) * 0.03f);
	}

	/// <summary>
	/// Draws the curve point.
	/// </summary>
	/// <param name="index">Index.</param>
	private void DrawCurvePoint(int index) {
		Handles.DotCap(0, m_curvePoints[index], Quaternion.identity, HandleUtility.GetHandleSize(m_keyPoints[index]) * 0.03f);
	}

	/// <summary>
	/// Draws the segment.
	/// </summary>
	/// <param name="index">Index.</param>
	private void DrawSegment(int index) {
		var from = m_keyPoints[index];
		var to = m_keyPoints[(index + 1) % m_keyPoints.Count];
		if(m_isCurve[index]) {
			var control = PolyMesh.Bezier.Control(from, to, m_curvePoints[index]);
			var count = Mathf.Ceil(1 / polyMesh.curveDetail);
			for(int i = 0; i < count; i++)
				Handles.DrawLine(PolyMesh.Bezier.Curve(from, control, to, i / count), PolyMesh.Bezier.Curve(from, control, to, (i + 1) / count));
		}
		else
			Handles.DrawLine(from, to);
	}

	/// <summary>
	/// Draws the circle.
	/// </summary>
	/// <param name="position">Position.</param>
	/// <param name="size">Size.</param>
	private void DrawCircle(Vector3 position, float size) {
		Handles.CircleCap(0, position, m_inverseRotation, HandleUtility.GetHandleSize(position) * size);
	}

	/// <summary>
	/// Draws the nearest line and split.
	/// </summary>
	private void DrawNearestLineAndSplit() {
		if(m_nearestLine >= 0) {
			Handles.color = Color.green;
			DrawSegment(m_nearestLine);
			Handles.color = Color.red;
			Handles.DotCap(0, m_splitPosition, Quaternion.identity, HandleUtility.GetHandleSize(m_splitPosition) * 0.03f);
		}
	}

	//------------------------------------------------------------------------//
	// MESH EDITING METHODS													  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Tries the hover key point.
	/// </summary>
	/// <returns><c>true</c>, if hover key point was tryed, <c>false</c> otherwise.</returns>
	/// <param name="index">Index.</param>
	private bool TryHoverKeyPoint(out int index) {
		if(TryHover(m_keyPoints, Color.white, out index)) {
			m_mouseCursor = MouseCursor.MoveArrow;
			return true;
		}
		return false;
	}

	/// <summary>
	/// Tries the hover curve point.
	/// </summary>
	/// <returns><c>true</c>, if hover curve point was tryed, <c>false</c> otherwise.</returns>
	/// <param name="index">Index.</param>
	private bool TryHoverCurvePoint(out int index) {
		if(TryHover(m_curvePoints, Color.white, out index)) {
			m_mouseCursor = MouseCursor.MoveArrow;
			return true;
		}
		return false;
	}

	/// <summary>
	/// Tries the drag key point.
	/// </summary>
	/// <returns><c>true</c>, if drag key point was tryed, <c>false</c> otherwise.</returns>
	/// <param name="index">Index.</param>
	private bool TryDragKeyPoint(int index) {
		if(TryDrag(m_keyPoints, index)) {
			m_draggingCurve = false;
			return true;
		}
		return false;
	}

	/// <summary>
	/// Tries the drag curve point.
	/// </summary>
	/// <returns><c>true</c>, if drag curve point was tryed, <c>false</c> otherwise.</returns>
	/// <param name="index">Index.</param>
	private bool TryDragCurvePoint(int index) {
		if(TryDrag(m_curvePoints, index)) {
			m_draggingCurve = true;
			return true;
		}
		return false;
	}

	/// <summary>
	/// Tries the hover.
	/// </summary>
	/// <returns><c>true</c>, if hover was tryed, <c>false</c> otherwise.</returns>
	/// <param name="points">Points.</param>
	/// <param name="color">Color.</param>
	/// <param name="index">Index.</param>
	private bool TryHover(List<Vector3> points, Color color, out int index) {
		if(Tools.current == Tool.Move || Tools.current == Tool.Rect) {
			index = NearestPoint(points);
			if(index >= 0 && IsHovering(points[index])) {
				Handles.color = color;
				DrawCircle(points[index], CLICK_RADIUS);
				return true;
			}
		}
		index = -1;
		return false;
	}

	/// <summary>
	/// Tries the drag.
	/// </summary>
	/// <returns><c>true</c>, if drag was tryed, <c>false</c> otherwise.</returns>
	/// <param name="points">Points.</param>
	/// <param name="index">Index.</param>
	private bool TryDrag(List<Vector3> points, int index) {
		if(e.type == EventType.MouseDown && IsHovering(points[index])) {
			m_clickPosition = m_mousePosition;
			return true;
		}
		return false;
	}

	/// <summary>
	/// Tries the stop drag.
	/// </summary>
	/// <returns><c>true</c>, if stop drag was tryed, <c>false</c> otherwise.</returns>
	private bool TryStopDrag() {
		if(e.type == EventType.MouseUp) {
			m_dragIndex = -1;
			UpdatePoly(false, m_state != State.EXTRUDE);
			return true;
		}
		return false;
	}

	/// <summary>
	/// Tries the box select.
	/// </summary>
	/// <returns><c>true</c>, if box select was tryed, <c>false</c> otherwise.</returns>
	private bool TryBoxSelect() {
		if(e.type == EventType.MouseDown) {
			m_clickPosition = m_mousePosition;
			return true;
		}
		return false;
	}

	/// <summary>
	/// Tries the box select end.
	/// </summary>
	/// <returns><c>true</c>, if box select end was tryed, <c>false</c> otherwise.</returns>
	private bool TryBoxSelectEnd() {
		var min = new Vector3(Mathf.Min(m_clickPosition.x, m_mousePosition.x), Mathf.Min(m_clickPosition.y, m_mousePosition.y));
		var max = new Vector3(Mathf.Max(m_clickPosition.x, m_mousePosition.x), Mathf.Max(m_clickPosition.y, m_mousePosition.y));
		Handles.color = Color.white;
		Handles.DrawLine(new Vector3(min.x, min.y), new Vector3(max.x, min.y));
		Handles.DrawLine(new Vector3(min.x, max.y), new Vector3(max.x, max.y));
		Handles.DrawLine(new Vector3(min.x, min.y), new Vector3(min.x, max.y));
		Handles.DrawLine(new Vector3(max.x, min.y), new Vector3(max.x, max.y));

		if(e.type == EventType.MouseUp) {
			var rect = new Rect(min.x, min.y, max.x - min.x, max.y - min.y);

			if(!control)
				m_selectedIndices.Clear();
			for(int i = 0; i < m_keyPoints.Count; i++)
				if(rect.Contains(m_keyPoints[i]))
					m_selectedIndices.Add(i);

			return true;
		}
		return false;
	}

	/// <summary>
	/// Tries the drag selected.
	/// </summary>
	/// <returns><c>true</c>, if drag selected was tryed, <c>false</c> otherwise.</returns>
	private bool TryDragSelected() {
		if(m_selectedIndices.Count > 0 && TryDragButton(GetSelectionCenter(), 0.2f)) {
			m_clickPosition = m_mousePosition;
			return true;
		}
		return false;
	}

	/// <summary>
	/// Tries the rotate selected.
	/// </summary>
	/// <returns><c>true</c>, if rotate selected was tryed, <c>false</c> otherwise.</returns>
	private bool TryRotateSelected() {
		if(m_selectedIndices.Count > 0 && TryRotateButton(GetSelectionCenter(), 0.3f)) {
			m_clickPosition = m_mousePosition;
			return true;
		}
		return false;
	}

	/// <summary>
	/// Tries the scale selected.
	/// </summary>
	/// <returns><c>true</c>, if scale selected was tryed, <c>false</c> otherwise.</returns>
	private bool TryScaleSelected() {
		if(m_selectedIndices.Count > 0 && TryScaleButton(GetSelectionCenter(), 0.3f)) {
			m_clickPosition = m_mousePosition;
			return true;
		}
		return false;
	}

	/// <summary>
	/// Tries the drag button.
	/// </summary>
	/// <returns><c>true</c>, if drag button was tryed, <c>false</c> otherwise.</returns>
	/// <param name="position">Position.</param>
	/// <param name="size">Size.</param>
	private bool TryDragButton(Vector3 position, float size) {
		size *= HandleUtility.GetHandleSize(position);
		if(Vector3.Distance(m_mousePosition, position) < size) {
			if(e.type == EventType.MouseDown)
				return true;
			else {
				m_mouseCursor = MouseCursor.MoveArrow;
				Handles.color = Color.green;
			}
		}
		else
			Handles.color = Color.white;
		var buffer = size / 2;
		Handles.DrawLine(new Vector3(position.x - buffer, position.y), new Vector3(position.x + buffer, position.y));
		Handles.DrawLine(new Vector3(position.x, position.y - buffer), new Vector3(position.x, position.y + buffer));
		Handles.RectangleCap(0, position, Quaternion.identity, size);
		return false;
	}

	/// <summary>
	/// Tries the rotate button.
	/// </summary>
	/// <returns><c>true</c>, if rotate button was tryed, <c>false</c> otherwise.</returns>
	/// <param name="position">Position.</param>
	/// <param name="size">Size.</param>
	private bool TryRotateButton(Vector3 position, float size) {
		size *= HandleUtility.GetHandleSize(position);
		var dist = Vector3.Distance(m_mousePosition, position);
		var buffer = size / 4;
		if(dist < size + buffer && dist > size - buffer) {
			if(e.type == EventType.MouseDown)
				return true;
			else {
				m_mouseCursor = MouseCursor.RotateArrow;
				Handles.color = Color.green;
			}
		}
		else
			Handles.color = Color.white;
		Handles.CircleCap(0, position, m_inverseRotation, size - buffer / 2);
		Handles.CircleCap(0, position, m_inverseRotation, size + buffer / 2);
		return false;
	}

	/// <summary>
	/// Tries the scale button.
	/// </summary>
	/// <returns><c>true</c>, if scale button was tryed, <c>false</c> otherwise.</returns>
	/// <param name="position">Position.</param>
	/// <param name="size">Size.</param>
	private bool TryScaleButton(Vector3 position, float size) {
		size *= HandleUtility.GetHandleSize(position);
		if(Vector3.Distance(m_mousePosition, position) < size) {
			if(e.type == EventType.MouseDown)
				return true;
			else {
				m_mouseCursor = MouseCursor.ScaleArrow;
				Handles.color = Color.green;
			}
		}
		else
			Handles.color = Color.white;
		var buffer = size / 4;
		Handles.DrawLine(new Vector3(position.x - size - buffer, position.y), new Vector3(position.x - size + buffer, position.y));
		Handles.DrawLine(new Vector3(position.x + size - buffer, position.y), new Vector3(position.x + size + buffer, position.y));
		Handles.DrawLine(new Vector3(position.x, position.y - size - buffer), new Vector3(position.x, position.y - size + buffer));
		Handles.DrawLine(new Vector3(position.x, position.y + size - buffer), new Vector3(position.x, position.y + size + buffer));
		Handles.RectangleCap(0, position, Quaternion.identity, size);
		return false;
	}

	/// <summary>
	/// Tries the select all.
	/// </summary>
	/// <returns><c>true</c>, if select all was tryed, <c>false</c> otherwise.</returns>
	private bool TrySelectAll() {
		if(KeyPressed(selectAllKey)) {
			m_selectedIndices.Clear();
			for(int i = 0; i < m_keyPoints.Count; i++)
				m_selectedIndices.Add(i);
			return true;
		}
		return false;
	}

	/// <summary>
	/// Tries the split line.
	/// </summary>
	/// <returns><c>true</c>, if split line was tryed, <c>false</c> otherwise.</returns>
	private bool TrySplitLine() {
		if(m_nearestLine >= 0 && KeyPressed(createPointKey)) {
			if(m_nearestLine == m_keyPoints.Count - 1) {
				m_keyPoints.Add(m_splitPosition);
				m_curvePoints.Add(Vector3.zero);
				m_isCurve.Add(false);
			}
			else {
				m_keyPoints.Insert(m_nearestLine + 1, m_splitPosition);
				m_curvePoints.Insert(m_nearestLine + 1, Vector3.zero);
				m_isCurve.Insert(m_nearestLine + 1, false);
			}
			m_isCurve[m_nearestLine] = false;
			UpdatePoly(true, true);
			return true;
		}
		return false;
	}

	/// <summary>
	/// Tries the extrude.
	/// </summary>
	/// <returns><c>true</c>, if extrude was tryed, <c>false</c> otherwise.</returns>
	private bool TryExtrude() {
		if(m_nearestLine >= 0 && m_extrudeKeyDown && e.type == EventType.MouseDown) {
			var a = m_nearestLine;
			var b = (m_nearestLine + 1) % m_keyPoints.Count;
			if(b == 0 && a == m_keyPoints.Count - 1) {
				//Extrude between the first and last points
				m_keyPoints.Add(polyMesh.keyPoints[a]);
				m_keyPoints.Add(polyMesh.keyPoints[b]);
				m_curvePoints.Add(Vector3.zero);
				m_curvePoints.Add(Vector3.zero);
				m_isCurve.Add(false);
				m_isCurve.Add(false);
				
				m_selectedIndices.Clear();
				m_selectedIndices.Add(m_keyPoints.Count - 2);
				m_selectedIndices.Add(m_keyPoints.Count - 1);
			}
			else {
				//Extrude between two inner points
				var pointA = m_keyPoints[a];
				var pointB = m_keyPoints[b];
				m_keyPoints.Insert(a + 1, pointA);
				m_keyPoints.Insert(a + 2, pointB);
				m_curvePoints.Insert(a + 1, Vector3.zero);
				m_curvePoints.Insert(a + 2, Vector3.zero);
				m_isCurve.Insert(a + 1, false);
				m_isCurve.Insert(a + 2, false);
				
				m_selectedIndices.Clear();
				m_selectedIndices.Add(a + 1);
				m_selectedIndices.Add(a + 2);
			}
			m_isCurve[m_nearestLine] = false;

			m_clickPosition = m_mousePosition;
			m_doExtrudeUpdate = true;
			UpdatePoly(true, true);
			return true;
		}
		return false;
	}

	/// <summary>
	/// Tries the delete selected.
	/// </summary>
	/// <returns><c>true</c>, if delete selected was tryed, <c>false</c> otherwise.</returns>
	private bool TryDeleteSelected() {
		if(KeyPressed(deletePointKey)) {
			if(m_selectedIndices.Count > 0) {
				if(m_keyPoints.Count - m_selectedIndices.Count >= 3) {
					for(int i = m_selectedIndices.Count - 1; i >= 0; i--) {
						var index = m_selectedIndices[i];
						m_keyPoints.RemoveAt(index);
						m_curvePoints.RemoveAt(index);
						m_isCurve.RemoveAt(index);
					}
					m_selectedIndices.Clear();
					UpdatePoly(true, true);
					return true;
				}
			} else if(IsHovering(m_curvePoints[m_nearestLine])) {
				m_isCurve[m_nearestLine] = false;
				UpdatePoly(false, true);
			}
		}
		return false;
	}

	/// <summary>
	/// Split a mesh into two, dividing through the selected non-consecutive points.
	/// </summary>
	/// <returns><c>true</c> if the mesh was successfully split, <c>false</c> otherwise.</returns>
	/// <param name="_checkKey">Whether to check the assigned key press.</param>
	private bool TrySplitMesh(bool _checkKey) {
		// Key check
		if(_checkKey && !KeyPressed(splitMeshKey)) {
			return false;
		}

		// Exactly 2 points are needed
		if(m_selectedIndices.Count < 2) {
			EditorUtility.DisplayDialog("Not enough points selected!", "In order to perform the split mesh operation, select exactly 2 non-consecutive points of the poly that will be used to split the mesh in two.", "Understood");
			return false;
		}

		if(m_selectedIndices.Count > 2) {
			EditorUtility.DisplayDialog("Too many points selected!", "In order to perform the split mesh operation, select exactly 2 non-consecutive points of the poly that will be used to split the mesh in two.", "Understood");
			return false;
		}

		// Points must be non-consecutive
		if(Mathf.Abs(m_selectedIndices[1] - m_selectedIndices[0]) == 1) {
			EditorUtility.DisplayDialog("Invalid points!", "In order to perform the split mesh operation, select exactly 2 non-consecutive points of the poly that will be used to split the mesh in two.", "Understood");
			return false;
		}

		// All checks passed, do it!
		// Create a duplicate of the whole game object
		PolyMesh newPolyMesh = DuplicateMesh(polyMesh);

		// Delete the unwanted points from both meshes
		// Make sure we get both selected points sorted
		List<int> selected = new List<int>(m_selectedIndices);
		selected.Sort();

		// Pick the first set of points and update mesh (the new polymesh)
		RecordUndo(newPolyMesh);
		newPolyMesh.keyPoints = new List<Vector3>(m_keyPoints.GetRange(selected[0], selected[1] - selected[0] + 1));	// Include both selected points!
		newPolyMesh.isCurve = new List<bool>(m_isCurve.GetRange(selected[0], selected[1] - selected[0] + 1));
		for(int i = 0; i < newPolyMesh.keyPoints.Count; i++) {
			// [AOC] Is this right?? From UpdatePoly()
			if(newPolyMesh.isCurve[i]) {
				newPolyMesh.curvePoints[i] = Vector3.zero;
			} else {
				newPolyMesh.curvePoints[i] = Vector3.Lerp(newPolyMesh.keyPoints[i], newPolyMesh.keyPoints[(i + 1) % newPolyMesh.keyPoints.Count], 0.5f);
			}
		}
		if(m_liveMode) {
			newPolyMesh.BuildMesh();
		}

		// Pick the second set of points and update mesh (this polymesh)
		m_keyPoints.RemoveRange(selected[0] + 1, selected[1] - selected[0] - 1);	// Keep both selected points!
		m_isCurve.RemoveRange(selected[0] + 1, selected[1] - selected[0] - 1);
		m_curvePoints.RemoveRange(selected[0] + 1, selected[1] - selected[0] - 1);
		UpdatePoly(true, true);

		// Clear selection
		m_selectedIndices.Clear();

		// Select the newly created mesh
		EditorUtils.FocusObject(newPolyMesh.gameObject, true, false, true);

		return true;
	}

	/// <summary>
	/// Try to select a single point by clicking on it.
	/// </summary>
	/// <returns><c>true</c> if a point was selected, <c>false</c> otherwise.</returns>
	private bool TrySelectPoint() {
		int index = NearestPoint(m_keyPoints);
		if(index >= 0 && IsHovering(m_keyPoints[index])) {
			if(e.type == EventType.MouseDown) {
				// If the point was already selected, return false so we can proceed to detect point dragging
				if(m_selectedIndices.Contains(index)) return false;

				// Otherwise select the point
				if(!control) m_selectedIndices.Clear();
				m_selectedIndices.Add(index);
				return true;
			}
		}
		return false;
	}

	/// <summary>
	/// Determines whether this instance is hovering the specified point.
	/// </summary>
	/// <returns><c>true</c> if this instance is hovering the specified point; otherwise, <c>false</c>.</returns>
	/// <param name="point">Point.</param>
	private bool IsHovering(Vector3 point) {
		return Vector3.Distance(m_mousePosition, point) < HandleUtility.GetHandleSize(point) * CLICK_RADIUS;
	}

	//------------------------------------------------------------------------//
	// INTERNAL UTILS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Nearests the point.
	/// </summary>
	/// <returns>The point.</returns>
	/// <param name="points">Points.</param>
	private int NearestPoint(List<Vector3> points) {
		var near = -1;
		var nearDist = float.MaxValue;
		for(int i = 0; i < points.Count; i++) {
			var dist = Vector3.Distance(points[i], m_mousePosition);
			if(dist < nearDist) {
				nearDist = dist;
				near = i;
			}
		}
		return near;
	}

	/// <summary>
	/// Nearests the line.
	/// </summary>
	/// <returns>The line.</returns>
	/// <param name="position">Position.</param>
	private int NearestLine(out Vector3 position) {
		if(m_keyPoints == null || m_keyPoints.Count == 0) {
			position = Vector3.zero;
			return -1;
		}

		var near = -1;
		var nearDist = float.MaxValue;
		position = m_keyPoints[0];
		var linePos = Vector3.zero;
		for(int i = 0; i < m_keyPoints.Count; i++) {
			var j = (i + 1) % m_keyPoints.Count;
			var line = m_keyPoints[j] - m_keyPoints[i];
			var offset = m_mousePosition - m_keyPoints[i];
			var dot = Vector3.Dot(line.normalized, offset);
			if(dot >= 0 && dot <= line.magnitude) {
				if(m_isCurve[i])
					linePos = PolyMesh.Bezier.Curve(m_keyPoints[i], PolyMesh.Bezier.Control(m_keyPoints[i], m_keyPoints[j], m_curvePoints[i]), m_keyPoints[j], dot / line.magnitude);
				else
					linePos = m_keyPoints[i] + line.normalized * dot;
				var dist = Vector3.Distance(linePos, m_mousePosition);
				if(dist < nearDist) {
					nearDist = dist;
					position = linePos;
					near = i;
				}
			}
		}
		return near;
	}

	/// <summary>
	/// Keies the pressed.
	/// </summary>
	/// <returns><c>true</c>, if pressed was keyed, <c>false</c> otherwise.</returns>
	/// <param name="key">Key.</param>
	private bool KeyPressed(KeyCode key) {
		return e.type == EventType.KeyDown && e.keyCode == key;
	}

	/// <summary>
	/// Keies the released.
	/// </summary>
	/// <returns><c>true</c>, if released was keyed, <c>false</c> otherwise.</returns>
	/// <param name="key">Key.</param>
	private bool KeyReleased(KeyCode key) {
		return e.type == EventType.KeyUp && e.keyCode == key;
	}

	/// <summary>
	/// Snap the specified value.
	/// </summary>
	/// <param name="value">Value.</param>
	private Vector3 Snap(Vector3 value) {
		value.x = Mathf.Round(value.x / m_snap) * m_snap;
		value.y = Mathf.Round(value.y / m_snap) * m_snap;
		return value;
	}

	/// <summary>
	/// Gets the selection center.
	/// </summary>
	/// <returns>The selection center.</returns>
	private Vector3 GetSelectionCenter() {
		var center = Vector3.zero;
		foreach(var i in m_selectedIndices)
			center += polyMesh.keyPoints[i];
		return center / m_selectedIndices.Count;
	}

	//------------------------------------------------------------------------//
	// PROPERTIES															  //
	//------------------------------------------------------------------------//
	private PolyMesh polyMesh {
		get { return (PolyMesh)target; }
	}

	private Event e {
		get { return Event.current; }
	}

	private bool control {
		get { return Application.platform == RuntimePlatform.OSXEditor ? e.command : e.control; }
	}

	private bool doSnap {
		get { return autoSnap ? !e.alt : e.alt; }
	}

	// Foldable groups
	private static bool meshSettings {
		get { return EditorPrefs.GetBool("PolyMeshEditor_meshSettings", false); }
		set { EditorPrefs.SetBool("PolyMeshEditor_meshSettings", value); }
	}

	private static bool colliderSettings {
		get { return EditorPrefs.GetBool("PolyMeshEditor_colliderSettings", false); }
		set { EditorPrefs.SetBool("PolyMeshEditor_colliderSettings", value); }
	}

	private static bool uvSettings {
		get { return EditorPrefs.GetBool("PolyMeshEditor_uvSettings", false); }
		set { EditorPrefs.SetBool("PolyMeshEditor_uvSettings", value); }
	}

	private static bool editorSettings {
		get { return EditorPrefs.GetBool("PolyMeshEditor_editorSettings", false); }
		set { EditorPrefs.SetBool("PolyMeshEditor_editorSettings", value); }
	}

	private static bool mergeObjects {
		get { return EditorPrefs.GetBool("PolyMeshEditor_mergeObjects", false); }
		set { EditorPrefs.SetBool("PolyMeshEditor_mergeObjects", value); }
	}

	private static bool splitSection {
		get { return EditorPrefs.GetBool("PolyMeshEditor_splitSection", false); }
		set { EditorPrefs.SetBool("PolyMeshEditor_splitSection", value); }
	}

	// Snapping
	private static bool autoSnap {
		get { return EditorPrefs.GetBool("PolyMeshEditor_autoSnap", false); }
		set { EditorPrefs.SetBool("PolyMeshEditor_autoSnap", value); }
	}

	private static bool globalSnap {
		get { return EditorPrefs.GetBool("PolyMeshEditor_globalSnap", false); }
		set { EditorPrefs.SetBool("PolyMeshEditor_globalSnap", value); }
	}

	private static float gridSnap {
		get { return EditorPrefs.GetFloat("PolyMeshEditor_gridSnap", 1); }
		set { EditorPrefs.SetFloat("PolyMeshEditor_gridSnap", value); }
	}

	// Hotkeys
	public KeyCode editKey {
		get { return (KeyCode)EditorPrefs.GetInt("PolyMeshEditor_editKey", (int)KeyCode.Tab); }
		set { EditorPrefs.SetInt("PolyMeshEditor_editKey", (int)value); }
	}

	public KeyCode selectAllKey {
		get { return (KeyCode)EditorPrefs.GetInt("PolyMeshEditor_selectAllKey", (int)KeyCode.A); }
		set { EditorPrefs.SetInt("PolyMeshEditor_selectAllKey", (int)value); }
	}

	public KeyCode createPointKey {
		get { return (KeyCode)EditorPrefs.GetInt("PolyMeshEditor_createPointKey", (int)KeyCode.S); }
		set { EditorPrefs.SetInt("PolyMeshEditor_createPointKey", (int)value); }
	}

	public KeyCode deletePointKey {
		get { return (KeyCode)EditorPrefs.GetInt("PolyMeshEditor_deletePointKey", (int)KeyCode.Delete); }
		set { EditorPrefs.SetInt("PolyMeshEditor_deletePointKey", (int)value); }
	}

	public KeyCode extrudeKey {
		get { return (KeyCode)EditorPrefs.GetInt("PolyMeshEditor_extrudeKey", (int)KeyCode.D); }
		set { EditorPrefs.SetInt("PolyMeshEditor_extrudeKey", (int)value); }
	}

	public KeyCode splitMeshKey {
		get { return (KeyCode)EditorPrefs.GetInt("PolyMeshEditor_splitMeshKey", (int)KeyCode.P); }
		set { EditorPrefs.SetInt("PolyMeshEditor_splitMeshKey", (int)value); }
	}

	// Other settings
	private static bool hideWireframe {
		get { return EditorPrefs.GetBool("PolyMeshEditor_hideWireframe", true); }
		set { EditorPrefs.SetBool("PolyMeshEditor_hideWireframe", value); }
	}

	private static bool autoEdit {	// Whether to automatically enter edit mode when selecting the object
		get { return EditorPrefs.GetBool("PolyMeshEditor_autoEdit", true); }
		set { EditorPrefs.SetBool("PolyMeshEditor_autoEdit", value); }
	}

	//------------------------------------------------------------------------//
	// MENU ITEMS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Context menu addition to create a new polymesh object.
	/// </summary>
	/// <param name="_command">The command that triggered the callback.</param>
	[MenuItem("GameObject/Hungry Dragon/Collision PolyMesh (transparent)", false, 10)]	// http://docs.unity3d.com/ScriptReference/MenuItem.html
	private static GameObject CreatePolyMeshTransparent(MenuCommand _command) {
		// Create a new game object with all the required components
		// Place the new object as child of the currently selected object (default Unity's behaviour)
		// Use our own EditorUtils!!
		GameObject obj = EditorUtils.CreateGameObject("CollisionPolyMesh", EditorUtils.GetContextObject(_command), false);
		obj.AddComponent<MeshFilter>();
		PolyMesh polyMesh = obj.AddComponent<PolyMesh>();

		// Initialize the new polymesh with a basic shape
		CreateSquare(polyMesh, 4f);

		// [AOC] In this particular case, we want collisions to be on the "Ground" layed, so make sure it's done
		obj.SetLayerRecursively("Ground");

		// Done!
		return obj;
	}

	/// <summary>
	/// Context menu addition to create a new polymesh object.
	/// </summary>
	/// <param name="_command">The command that triggered the callback.</param>
	[MenuItem("GameObject/Hungry Dragon/Collision PolyMesh (solid)", false, 10)]	// http://docs.unity3d.com/ScriptReference/MenuItem.html
	private static GameObject CreatePolyMeshSolid(MenuCommand _command) {
		// Use the transparent constructor
		GameObject obj = CreatePolyMeshTransparent(_command);

		// Add and initialize the MeshRenderer component
		MeshRenderer renderer = obj.AddComponent<MeshRenderer>();
		renderer.material = AssetDatabase.LoadAssetAtPath<Material>("Assets/" + DEFAULT_MATERIAL_PATH + ".mat");

		// Done!
		return obj;
	}

	/// <summary>
	/// Creates the square.
	/// </summary>
	/// <param name="polyMesh">Poly mesh.</param>
	/// <param name="size">Size.</param>
	private static void CreateSquare(PolyMesh polyMesh, float size) {
		polyMesh.keyPoints.AddRange(new Vector3[] {
			new Vector3(size, size),
			new Vector3(size, -size),
			new Vector3(-size, -size),
			new Vector3(-size, size)
		});
		polyMesh.curvePoints.AddRange(new Vector3[] {
			Vector3.zero,
			Vector3.zero,
			Vector3.zero,
			Vector3.zero
		});
		polyMesh.isCurve.AddRange(new bool[] { false, false, false, false });
		polyMesh.BuildMesh();
	}

	/// <summary>
	/// Create a duplicate of the given polymesh.
	/// </summary>
	/// <returns>The new polymesh object.</returns>
	/// <param name="_polyMesh">The polymesh to be duplicated.</param>
	private static PolyMesh DuplicateMesh(PolyMesh _polyMesh) {
		// Security checks
		if(_polyMesh == null) return null;

		// Use game object creation menu static methods
		// Solid or transparent?
		bool transparent = (_polyMesh.GetComponent<MeshRenderer>() == null);
		GameObject newObj = null;
		GameObject parentObj = (_polyMesh.transform.parent != null) ? _polyMesh.transform.parent.gameObject : null;
		MenuCommand cmd = new MenuCommand(parentObj);	// Create it on the currently selected object
		if(transparent) {
			newObj = CreatePolyMeshTransparent(cmd);
		} else {
			newObj = CreatePolyMeshSolid(cmd);
		}

		// Move next to the source
		newObj.transform.SetSiblingIndex(_polyMesh.transform.GetSiblingIndex() + 1);

		// Copy the name and other game object properties
		newObj.name = _polyMesh.name;
		newObj.transform.position = _polyMesh.transform.position;
		newObj.transform.rotation = _polyMesh.transform.rotation;
		newObj.transform.localScale = _polyMesh.transform.localScale;
		newObj.SetActive(_polyMesh.gameObject.activeSelf);
		newObj.isStatic = _polyMesh.gameObject.isStatic;
		newObj.layer = _polyMesh.gameObject.layer;
		newObj.tag = _polyMesh.gameObject.tag;

		// Clone the points
		PolyMesh newPolyMesh = newObj.GetComponent<PolyMesh>();
		newPolyMesh.keyPoints.Clear();
		newPolyMesh.keyPoints.AddRange(_polyMesh.keyPoints);
		newPolyMesh.curvePoints.Clear();
		newPolyMesh.curvePoints.AddRange(_polyMesh.curvePoints);
		newPolyMesh.isCurve.Clear();
		newPolyMesh.isCurve.AddRange(_polyMesh.isCurve);

		// Clone other properties
		newPolyMesh.buildColliderEdges = _polyMesh.buildColliderEdges;
		newPolyMesh.buildColliderFront = _polyMesh.buildColliderFront;
		newPolyMesh.colliderDepth = _polyMesh.colliderDepth;
		newPolyMesh.curveDetail = _polyMesh.curveDetail;
		newPolyMesh.mergeEndPoint = _polyMesh.mergeEndPoint;
		newPolyMesh.pinkMeshOffset = _polyMesh.pinkMeshOffset;
		newPolyMesh.showNormals = _polyMesh.showNormals;
		newPolyMesh.showOutline = _polyMesh.showOutline;
		newPolyMesh.mergeStartPoint = _polyMesh.mergeStartPoint;
		newPolyMesh.uvPosition = _polyMesh.uvPosition;
		newPolyMesh.uvRotation = _polyMesh.uvRotation;
		newPolyMesh.uvScale = _polyMesh.uvScale;

		// Rebuild the mesh
		newPolyMesh.RebuildMesh();

		// Clone material
		if(!transparent) {
			newPolyMesh.GetComponent<MeshRenderer>().sharedMaterial = _polyMesh.GetComponent<MeshRenderer>().sharedMaterial;
		}

		// Done!
		return newPolyMesh;
	}

	//------------------------------------------------------------------------//
	// PUBLIC UTILS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Rescales the points.
	/// </summary>
	/// <param name="scale">Scale.</param>
	public void RescalePoints(float scale) {
		for(int i = 0; i < polyMesh.keyPoints.Count; i++) {
			polyMesh.keyPoints[i] = polyMesh.keyPoints[i] * scale;
		}
		polyMesh.BuildMesh();
	}
}
