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
	private Camera m_customTargetCamera = null;

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
		// Start recording
		Undo.RecordObject(m_targetSnapPoint, "CameraSnapPoint");
		EditorGUI.BeginChangeCheck();

		// Live preview enabled?
		m_targetSnapPoint.livePreview = EditorGUILayout.Toggle("Live Preview", m_targetSnapPoint.livePreview);
		EditorGUILayout.Space();

		// Optional values - single line
		EditorGUILayout.Space();
		EditorGUILayout.LabelField("Optional Parameters", CustomEditorStyles.commentLabelLeft);

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
		EditorGUILayout.LabelField("Optional Fog Setup", CustomEditorStyles.commentLabelLeft);

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
		EditorGUILayout.LabelField("Editor Stuff", CustomEditorStyles.commentLabelLeft);

		// Color
		m_targetSnapPoint.gizmoColor = EditorGUILayout.ColorField("Gizmo Color", m_targetSnapPoint.gizmoColor);

		// Apply to target camera
		EditorGUILayout.BeginHorizontal(); {
			// Label
			EditorGUILayout.PrefixLabel("Apply to camera");

			// Camera selection
			Camera[] sceneCameras = GameObject.FindObjectsOfType<Camera>();
			m_customTargetCamera = EditorGUILayoutExt.Popup<Camera>("", m_customTargetCamera, sceneCameras);

			// Apply button
			bool wasEnabled = GUI.enabled;
			GUI.enabled = (m_customTargetCamera != null);
			if(GUILayout.Button("Apply")) {
				m_targetSnapPoint.Apply(m_customTargetCamera);
			}
			GUI.enabled = wasEnabled;
		} EditorGUILayout.EndHorizontal();

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
		
	}
}