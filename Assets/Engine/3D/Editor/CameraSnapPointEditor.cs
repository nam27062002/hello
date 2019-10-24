// CameraSnapPointEditor.cs
// 
// Created by Alger Ortín Castellví on 24/02/2016.
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
/// Custom editor for the CameraSnapPoint point script.
/// </summary>
[CustomEditor(typeof(CameraSnapPoint))]
public class CameraSnapPointEditor : Editor {
	//------------------------------------------------------------------//
	// CONSTANTS														//
	//------------------------------------------------------------------//
	private string TARGET_CAMERA_NAME_KEY = "CameraSnapPointEditor.TargetCameraName";

	//------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES											//
	//------------------------------------------------------------------//
	private CameraSnapPoint m_targetSnapPoint = null;
	private Camera m_editionCamera = null;
	private Camera m_targetCamera = null;

	private SerializedProperty m_livePreviewProp = null;

	//------------------------------------------------------------------//
	// METHODS															//
	//------------------------------------------------------------------//
	/// <summary>
	/// Editor enabled.
	/// </summary>
	public void OnEnable() {
		// Store target and some interesting properties
		m_targetSnapPoint = (CameraSnapPoint)target;
		m_livePreviewProp = serializedObject.FindProperty("livePreview");

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
				m_editionCamera.depth = 90;
			}
			camObj.name = "CameraSnapPointPreview";
			camObj.hideFlags = HideFlags.DontSave;

			// Destroy audio listener component, if any, otherwise the editor complains
			AudioListener listener = m_editionCamera.GetComponent<AudioListener>();
			if(listener != null) GameObject.DestroyImmediate(listener);
		}

		// Initialize live preview
		UpdateLivePreview();

		// We want the live preview to be constantly updated!
		EditorApplication.update += Update;
	}

	/// <summary>
	/// Editor disabled.
	/// </summary>
	public void OnDisable() {
		// Destroy preview camera
		if(m_editionCamera != null) {
			GameObject.DestroyImmediate(m_editionCamera.gameObject);
			m_editionCamera = null;
		}

		// Clear references
		m_livePreviewProp = null;
		m_targetSnapPoint = null;

		// Make sure all views are updated
		UpdateLivePreview();

		// Unsubscribe from external events
		EditorApplication.update -= Update;
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

		// Main toggles
		m_targetSnapPoint.changeRotation = EditorGUILayout.Toggle("Rotation", m_targetSnapPoint.changeRotation);

		// Optional values - single line
		EditorGUILayout.Space();
		EditorGUILayout.LabelField("Optional Parameters", CustomEditorStyles.commentLabelLeft);

		// Fov
		EditorGUILayout.BeginHorizontal(); {
			m_targetSnapPoint.changeFov = GUILayout.Toggle(m_targetSnapPoint.changeFov, GUIContent.none, GUILayout.Width(10f));
			GUI.enabled = m_targetSnapPoint.changeFov;
			EditorGUILayout.BeginVertical(); {
				m_targetSnapPoint.fov = EditorGUILayout.Slider("FOV", m_targetSnapPoint.fov, 1f, 179f);
				m_targetSnapPoint.fov43 = EditorGUILayout.Slider("FOV 4:3", m_targetSnapPoint.fov43, 1f, 179f);
			} EditorGUILayoutExt.EndHorizontalSafe();
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

		// Dark screen
		EditorGUILayout.Space();
		EditorGUILayout.LabelField("Optional Dark Screen Setup", CustomEditorStyles.commentLabelLeft);

		// Group it all in a toggle group
		m_targetSnapPoint.darkenScreen = EditorGUILayout.BeginToggleGroup("Dark Screen", m_targetSnapPoint.darkenScreen); {
			EditorGUI.indentLevel++;
			m_targetSnapPoint.darkScreenDistance = EditorGUILayout.FloatField("Distance", m_targetSnapPoint.darkScreenDistance);
			m_targetSnapPoint.darkScreenColor = EditorGUILayout.ColorField("Color", m_targetSnapPoint.darkScreenColor);
			m_targetSnapPoint.darkScreenRenderQueue = EditorGUILayout.IntField("Render Queue", m_targetSnapPoint.darkScreenRenderQueue);
			EditorGUI.indentLevel--;
		} EditorGUILayout.EndToggleGroup();

		// Editor stuff
		EditorGUILayout.Space();
		EditorGUILayout.LabelField("Editor Stuff", CustomEditorStyles.commentLabelLeft);

		// Color
		m_targetSnapPoint.gizmoColor = EditorGUILayout.ColorField("Gizmo Color", m_targetSnapPoint.gizmoColor);

		// Camera editing tools
		EditorGUILayoutExt.Separator("Edit Tools");

		// Apply from/to target camera
		EditorGUILayout.BeginVertical(); {
			// Camera selection - exclude UI cameras
			Camera[] sceneCameras = GameObject.FindObjectsOfType<Camera>();
			List<Camera> validCameras = new List<Camera>();
			for(int i = 0; i < sceneCameras.Length; i++) {
				// Exclude UI cameras, for now it's enough by checking the name
				if(!sceneCameras[i].name.Contains("UI")) {
					validCameras.Add(sceneCameras[i]);

					// If target camera is not yet defined, try loading it from prefs
					if(m_targetCamera == null) {
						if(sceneCameras[i].name == EditorPrefs.GetString(TARGET_CAMERA_NAME_KEY)) {
							m_targetCamera = sceneCameras[i];
						}
					}
				}
			}
			Camera newTargetCamera = EditorGUILayoutExt.Popup<Camera>("Reference Camera", m_targetCamera, validCameras.ToArray());
			if(newTargetCamera != m_targetCamera) {
				// Store new camera
				m_targetCamera = newTargetCamera;
				EditorPrefs.SetString(TARGET_CAMERA_NAME_KEY, newTargetCamera.name);
			}

			// Buttons
			EditorGUI.BeginDisabledGroup(m_targetCamera == null); {
				// Apply button
				if(GUILayout.Button("Apply to Ref Camera")) {
					m_targetSnapPoint.Apply(m_targetCamera);
				}

				// Read buttons
				if(GUILayout.Button("Read transform from Ref Camera")) {
					m_targetSnapPoint.transform.position = m_targetCamera.transform.position;
					m_targetSnapPoint.transform.rotation = m_targetCamera.transform.rotation;
				}

				if(GUILayout.Button("Read FOV from Ref Camera")) {
					m_targetSnapPoint.fov = m_targetCamera.fieldOfView;
				}

				if(GUILayout.Button("Read FOV 4:3 from Ref Camera")) {
					m_targetSnapPoint.fov43 = m_targetCamera.fieldOfView;
				}

				if(GUILayout.Button("Read planes from Ref Camera")) {
					m_targetSnapPoint.near = m_targetCamera.nearClipPlane;
					m_targetSnapPoint.far = m_targetCamera.farClipPlane;
				}
			} EditorGUI.EndDisabledGroup();
		} EditorGUILayout.EndVertical();

		// Store new values
		if(EditorGUI.EndChangeCheck()) {
			serializedObject.ApplyModifiedProperties();
		}
	}

	/// <summary>
	/// Updates stuff on the scene.
	/// </summary>
	public void OnSceneGUI() {
		
	}

	/// <summary>
	/// Called every frame.
	/// </summary>
	public void Update() {
		UpdateLivePreview();
	}

	/// <summary>
	/// Updates the live preview.
	/// </summary>
	private void UpdateLivePreview() {
		// Live preview, if enabled
		if(m_editionCamera != null && m_livePreviewProp != null) {
			//m_editionCamera.gameObject.SetActive(m_targetSnapPoint.livePreview);
			m_editionCamera.gameObject.SetActive(m_livePreviewProp.boolValue);
			if(m_livePreviewProp.boolValue) {
				// Update edition camera
				m_editionCamera.depth = 99;	// Make sure camera has priority
				m_editionCamera.transform.position = m_targetSnapPoint.transform.position;
				m_targetSnapPoint.Apply(m_editionCamera);
			}
		}

		// Make sure all views are up to date
		UnityEditorInternal.InternalEditorUtility.RepaintAllViews();	// HACKS! http://answers.unity3d.com/questions/449407/how-to-manually-update-gameview.html
	}

	/// <summary>
	/// Draw gizmos for the target snap point.
	/// Do it here rather than OnDrawGizoms to save time by avoiding compilation 
	/// of the whole project (just the Editor code).
	/// </summary>
	/// <param name="_target">Target curve.</param>
	/// <param name="_gizmoType">Gizmo type.</param>
	[DrawGizmo(GizmoType.Pickable | GizmoType.InSelectionHierarchy)]
	static void DrawGizmo(CameraSnapPoint _target, GizmoType _gizmoType) {
		// Ignore if gizmos disabled
		if(!_target.drawGizmos) return;

		// Camera frustum
		// If not defined, use main camera values in a different color
		// If there is no main camera, use default values in a different color
		Gizmos.color = _target.gizmoColor;
		Camera refCamera = Camera.main;

		// Fov
		float targetFov = _target.GetFOV();
		if(!_target.changeFov) {
			targetFov = (refCamera != null) ? refCamera.fieldOfView : 60f;
		}

		// Near
		float targetNear = _target.near;
		if(!_target.changeNear) {
			targetNear = (refCamera != null) ? refCamera.nearClipPlane : 0.3f;
		}

		// Far
		float targetFar = _target.far;
		if(!_target.changeFar) {
			targetFar = (refCamera != null) ? refCamera.farClipPlane : 1000f;
		}

		// Draw camera frustum
		Gizmos.matrix = _target.transform.localToWorldMatrix;
		Gizmos.DrawFrustum(Vector3.zero, targetFov, targetFar, targetNear, (refCamera != null) ? refCamera.aspect : 4f/3f);
		Gizmos.matrix = Matrix4x4.identity;
	}
}