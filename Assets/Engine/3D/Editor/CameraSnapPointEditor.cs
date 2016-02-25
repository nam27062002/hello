// CameraSnapPointEditor.cs
// 
// Created by Alger Ortín Castellví on 24/02/2016.
// Copyright (c) 2016 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;
using UnityEditor;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// Custom editor for the CameraSnapPoint point script.
/// </summary>
[CustomEditor(typeof(CameraSnapPoint))]
public class CameraSnapPointEditor : Editor {
	//------------------------------------------------------------------//
	// CONSTANTS														//
	//------------------------------------------------------------------//

	//------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES											//
	//------------------------------------------------------------------//
	private CameraSnapPoint m_targetSnapPoint = null;
	private Camera m_editionCamera = null;

	private static GUIStyle m_commentLabelStyle = null;

	//------------------------------------------------------------------//
	// METHODS															//
	//------------------------------------------------------------------//
	/// <summary>
	/// Editor enabled.
	/// </summary>
	public void OnEnable() {
		m_targetSnapPoint = (CameraSnapPoint)target;

		// Create a temp camera to preview during edition
		if(m_editionCamera == null) {
			// Create a duplicate of the main camera (if any)
			GameObject camObj;
			if(Camera.main != null) {
				camObj = GameObject.Instantiate(Camera.main.gameObject);
				m_editionCamera = camObj.GetComponent<Camera>();
			} else {
				camObj = new GameObject();
				m_editionCamera = camObj.AddComponent<Camera>();
			}
			camObj.name = "CameraSnapPointPreview";
			camObj.hideFlags = HideFlags.DontSave;
			camObj.SetActive(m_targetSnapPoint.livePreview);

			m_targetSnapPoint.Apply(m_editionCamera);

			UnityEditorInternal.InternalEditorUtility.RepaintAllViews();	// HACKS! http://answers.unity3d.com/questions/449407/how-to-manually-update-gameview.html
		}
	}

	/// <summary>
	/// Editor disabled.
	/// </summary>
	public void OnDisable() {
		if(m_editionCamera != null) {
			GameObject.DestroyImmediate(m_editionCamera.gameObject);
			m_editionCamera = null;
		}

		UnityEditorInternal.InternalEditorUtility.RepaintAllViews();	// HACKS! http://answers.unity3d.com/questions/449407/how-to-manually-update-gameview.html
	}

	/// <summary>
	/// Updates stuff on the inspector.
	/// </summary>
	public override void OnInspectorGUI() {
		// If custom style wasn't created, do it now
		if(m_commentLabelStyle == null) {
			m_commentLabelStyle = new GUIStyle(EditorStyles.label);
			m_commentLabelStyle.fontStyle = FontStyle.Italic;
			m_commentLabelStyle.normal.textColor = Colors.gray;
			m_commentLabelStyle.wordWrap = true;
		}

		// Start recording
		Undo.RecordObject(m_targetSnapPoint, "CameraSnapPoint");
		EditorGUI.BeginChangeCheck();

		// Live preview enabled?
		m_targetSnapPoint.livePreview = EditorGUILayout.Toggle("Live Preview", m_targetSnapPoint.livePreview);
		EditorGUILayout.Space();

		// LookAt is mandatory
		EditorGUILayout.LabelField("Required Parameters", m_commentLabelStyle);
		EditorGUILayout.LabelField("If lookAtObject is defined, lookAtPoint will be linked to the position of the object", m_commentLabelStyle);
		m_targetSnapPoint.lookAtPoint = EditorGUILayout.Vector3Field("Look At Point", m_targetSnapPoint.lookAtPoint);
		m_targetSnapPoint.lookAtObject = (Transform)EditorGUILayout.ObjectField("Look At Object", m_targetSnapPoint.lookAtObject, typeof(Transform), true);

		// Optional values - single line
		EditorGUILayout.Space();
		EditorGUILayout.LabelField("Optional Parameters", m_commentLabelStyle);

		// Fov
		EditorGUILayout.BeginHorizontal(); {
			m_targetSnapPoint.changeFov = GUILayout.Toggle(m_targetSnapPoint.changeFov, GUIContent.none, GUILayout.Width(10f));
			GUI.enabled = m_targetSnapPoint.changeFov;
			m_targetSnapPoint.fov = EditorGUILayout.Slider("FOV", m_targetSnapPoint.fov, 1f, 179f);
			GUI.enabled = true;
		} EditorGUILayoutExt.EndHorizontalSafe();

		// Near
		EditorGUILayout.BeginHorizontal(); {
			m_targetSnapPoint.changeNear = GUILayout.Toggle(m_targetSnapPoint.changeNear, GUIContent.none, GUILayout.Width(10f));
			GUI.enabled = m_targetSnapPoint.changeNear;
			m_targetSnapPoint.near = EditorGUILayout.FloatField("Near", m_targetSnapPoint.near);
			GUI.enabled = true;
		} EditorGUILayoutExt.EndHorizontalSafe();

		// Far
		EditorGUILayout.BeginHorizontal(); {
			m_targetSnapPoint.changeFar = GUILayout.Toggle(m_targetSnapPoint.changeFar, GUIContent.none, GUILayout.Width(10f));
			GUI.enabled = m_targetSnapPoint.changeFar;
			m_targetSnapPoint.far = EditorGUILayout.FloatField("Far", m_targetSnapPoint.far);
			GUI.enabled = true;
		} EditorGUILayoutExt.EndHorizontalSafe();

		// Fog setup
		EditorGUILayout.Space();
		EditorGUILayout.LabelField("Optional Fog Setup", m_commentLabelStyle);

		// Fog Color
		EditorGUILayout.BeginHorizontal(); {
			m_targetSnapPoint.changeFogColor = GUILayout.Toggle(m_targetSnapPoint.changeFogColor, GUIContent.none, GUILayout.Width(10f));
			GUI.enabled = m_targetSnapPoint.changeFogColor;
			m_targetSnapPoint.fogColor = EditorGUILayout.ColorField("Fog Color", m_targetSnapPoint.fogColor);
			GUI.enabled = true;
		} EditorGUILayoutExt.EndHorizontalSafe();

		// Fog Start
		EditorGUILayout.BeginHorizontal(); {
			m_targetSnapPoint.changeFogStart = GUILayout.Toggle(m_targetSnapPoint.changeFogStart, GUIContent.none, GUILayout.Width(10f));
			GUI.enabled = m_targetSnapPoint.changeFogStart;
			m_targetSnapPoint.fogStart = EditorGUILayout.FloatField("Fog Start", m_targetSnapPoint.fogStart);
			GUI.enabled = true;
		} EditorGUILayoutExt.EndHorizontalSafe();

		// Fog End
		EditorGUILayout.BeginHorizontal(); {
			m_targetSnapPoint.changeFogEnd = GUILayout.Toggle(m_targetSnapPoint.changeFogEnd, GUIContent.none, GUILayout.Width(10f));
			GUI.enabled = m_targetSnapPoint.changeFogEnd;
			m_targetSnapPoint.fogEnd = EditorGUILayout.FloatField("Fog End", m_targetSnapPoint.fogEnd);
			GUI.enabled = true;
		} EditorGUILayoutExt.EndHorizontalSafe();

		// Editor stuff
		EditorGUILayout.Space();
		EditorGUILayout.LabelField("Editor Stuff", m_commentLabelStyle);

		// Color
		m_targetSnapPoint.gizmoColor = EditorGUILayout.ColorField("Gizmo Color", m_targetSnapPoint.gizmoColor);

		// Live preview, if enabled
		if(m_editionCamera != null) {
			m_editionCamera.gameObject.SetActive(m_targetSnapPoint.livePreview);
			if(m_targetSnapPoint.livePreview) {
				m_targetSnapPoint.Apply(m_editionCamera);
			}
		}

		// Finish
		if(EditorGUI.EndChangeCheck()) {
			serializedObject.ApplyModifiedProperties();
		}
	}

	/// <summary>
	/// Updates stuff on the scene.
	/// </summary>
	public void OnSceneGUI() {
		if(m_targetSnapPoint) {
			// Draw and get positioning handles for the lookAt point
			if(Tools.pivotRotation == PivotRotation.Global) {
				m_targetSnapPoint.lookAtPoint = Handles.PositionHandle(m_targetSnapPoint.lookAtPoint, Quaternion.identity);
			} else {
				m_targetSnapPoint.lookAtPoint = Handles.PositionHandle(m_targetSnapPoint.lookAtPoint, m_targetSnapPoint.transform.rotation);	// [AOC] Use object's rotation
			}

			if(GUI.changed) {
				EditorUtility.SetDirty(m_targetSnapPoint);
			}
		}
	}
}