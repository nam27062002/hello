// CloudControllerEditor.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 16/11/2016.
// Copyright (c) 2016 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Custom editor for the CloudController class.
/// </summary>
[CustomEditor(typeof(CloudController), true)]	// True to be used by heir classes as well
public class CloudControllerEditor : Editor {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Casted target object
	CloudController m_target = null;

	// Internal
	private bool m_dirty = true;
	private float m_lastUpdateTime = 0f;
	private HashSet<string> m_processedProperties = new HashSet<string>();

	//------------------------------------------------------------------------//
	// METHODS																  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// The editor has been enabled - target object selected.
	/// </summary>
	private void OnEnable() {
		// Get target object
		m_target = target as CloudController;

		// Init internal vars
		m_dirty = false;
		m_lastUpdateTime = Time.realtimeSinceStartup;

		// For the live preview, we need an update call
		// Since Unity doesn't provide editors with that functionality, subscribe ourselves to the Unity Editor's update call
		EditorApplication.update += Update;
	}

	/// <summary>
	/// The editor has been disabled - target object unselected.
	/// </summary>
	private void OnDisable() {
		// Clear target object
		m_target = null;

		// Stop live preview when not selected
		EditorApplication.update -= Update;
	}

	/// <summary>
	/// Draw the inspector.
	/// </summary>
	public override void OnInspectorGUI() {
		// Instead of modifying script variables directly, it's advantageous to use the SerializedObject and 
		// SerializedProperty system to edit them, since this automatically handles private fields, multi-object 
		// editing, undo, and prefab overrides.

		// Update the serialized object - always do this in the beginning of OnInspectorGUI.
		serializedObject.Update();

		// Manually do most of the properties
		m_processedProperties.Clear();
		SerializedProperty p;

		// Default start
		GUI.enabled = false;
		DoProperty("m_Script");
		GUI.enabled = true;
		m_processedProperties.Add("m_Script");
		m_processedProperties.Add("m_ObjectHideFlags");	// Don't show this -_-

		// References
		EditorGUILayout.Space();
		p = DoFoldout("m_controlPoint1", "References");
		if(p.isExpanded) {
			EditorGUI.indentLevel++;

			p = DoProperty("m_controlPoint1");
			if(p.objectReferenceValue == null) {
				EditorGUILayout.HelpBox("Required Field!", MessageType.Error);
			}

			p = DoProperty("m_controlPoint2");
			if(p.objectReferenceValue == null) {
				EditorGUILayout.HelpBox("Required Field!", MessageType.Error);
			}

			p = DoProperty("m_cloudContainer");
			if(p.objectReferenceValue == null) {
				EditorGUILayout.HelpBox("Required Field!", MessageType.Error);
			}

			EditorGUILayout.Space();

			p = DoProperty("m_prefabs");
			if(p.arraySize == 0) {
				EditorGUILayout.HelpBox("At least one prefab is required!", MessageType.Error);
			} else {
				// Check that all prefabs are valid
				for(int i = 0; i < p.arraySize; i++) {
					if(p.GetArrayElementAtIndex(i).objectReferenceValue == null) {
						EditorGUILayout.HelpBox("Some invalid prefabs!", MessageType.Error);
						break;	// No need to keep checking
					}
				}
			}

			EditorGUI.indentLevel--;
		}
		m_processedProperties.Add("m_controlPoint1");
		m_processedProperties.Add("m_controlPoint2");
		m_processedProperties.Add("m_cloudContainer");
		m_processedProperties.Add("m_prefabs");

		// Main Setup
		EditorGUILayout.Space();
		p = DoFoldout("m_amount", "Main Setup");
		if(p.isExpanded) {
			EditorGUI.indentLevel++;
			DoProperty("m_amount");
			DoProperty("m_zNearSpeedRange");
			DoProperty("m_zFarSpeedRange");
			EditorGUI.indentLevel--;
		}
		m_processedProperties.Add("m_amount");
		m_processedProperties.Add("m_zNearSpeedRange");
		m_processedProperties.Add("m_zFarSpeedRange");

		// Transformations
		EditorGUILayout.Space();
		p = DoFoldout("m_xScaleRange", "Transformations");
		if(p.isExpanded) {
			EditorGUI.indentLevel++;
			DoProperty("m_xScaleRange");
			DoProperty("m_yScaleRange");
			DoProperty("m_aspectRatioRange");
			EditorGUILayout.Space();
			DoProperty("m_xFlipChance");
			DoProperty("m_yFlipChance");
			EditorGUI.indentLevel--;
		}
		m_processedProperties.Add("m_xScaleRange");
		m_processedProperties.Add("m_yScaleRange");
		m_processedProperties.Add("m_aspectRatioRange");
		m_processedProperties.Add("m_xFlipChance");
		m_processedProperties.Add("m_yFlipChance");

		// Distribution
		EditorGUILayout.Space();
		DoDistributionCurves();

		// Editor Options
		EditorGUILayout.Space();
		p = DoFoldout("m_livePreview", "Editor Options");
		if(p.isExpanded) {
			EditorGUI.indentLevel++;
			DoProperty("m_livePreview");
			DoProperty("m_liveEdit");
			EditorGUI.indentLevel--;
		}
		m_processedProperties.Add("m_livePreview");
		m_processedProperties.Add("m_liveEdit");

		// Debug Options
		EditorGUILayout.Space();
		p = DoFoldout("m_debugType", "Debug Options");
		if(p.isExpanded) {
			EditorGUI.indentLevel++;

			DoProperty("m_debugType");
			DoProperty("m_boxColor");
			DoProperty("m_controlPointsColor");

			if(GUILayout.Button("Re-create clouds", GUILayout.Height(30f))) {
				m_dirty = true;
			}

			EditorGUI.indentLevel--;
		}
		m_processedProperties.Add("m_debugType");
		m_processedProperties.Add("m_boxColor");
		m_processedProperties.Add("m_controlPointsColor");

		// Internal
		EditorGUILayout.Space();
		p = DoFoldout("m_xRange", "Internal");
		if(p.isExpanded) {
			EditorGUI.indentLevel++;
			GUI.enabled = false;
			DoProperty("m_xRange");
			DoProperty("m_yRange");
			DoProperty("m_zRange");
			DoProperty("m_clouds");
			DoProperty("m_speeds");
			GUI.enabled = true;
			EditorGUI.indentLevel--;
		}
		m_processedProperties.Add("m_xRange");
		m_processedProperties.Add("m_yRange");
		m_processedProperties.Add("m_zRange");
		m_processedProperties.Add("m_clouds");
		m_processedProperties.Add("m_speeds");
		m_processedProperties.Add("m_renderers");

		// Loop through all serialized properties and process those remaining
		p = serializedObject.GetIterator();
		p.Next(true);	// To get first element
		do {
			// Skip if already processed
			if(m_processedProperties.Contains(p.name)) continue;

			// Detect changes
			EditorGUI.BeginChangeCheck();

			// Properties requiring special treatment
			if(p.name == "m_propertyRequiringSpecialTreatment") {
				// Do whatever needed
			}

			// Default property display
			else {
				EditorGUILayout.PropertyField(p, true);
			}

			// Where there any changes?
			m_dirty |= EditorGUI.EndChangeCheck();
		} while(p.NextVisible(false));		// Only direct children, not grand-children (will be drawn by default if using the default EditorGUI.PropertyField)

		// Apply changes to the serialized object - always do this in the end of OnInspectorGUI.
		serializedObject.ApplyModifiedProperties();
	}

	/// <summary>
	/// The scene is being refreshed.
	/// </summary>
	public void OnSceneGUI() {
		// Scene-related stuff
		// Allow moving control points around
		Handles.matrix = Matrix4x4.identity;

		// CP1
		if(m_target.m_controlPoint1 != null) {
			Vector3 newPos = Handles.PositionHandle(m_target.m_controlPoint1.position, Quaternion.identity);
			if(newPos != m_target.m_controlPoint1.position) {
				Undo.RecordObject(m_target.m_controlPoint1, "CloudController.CP1");
				m_target.m_controlPoint1.position = newPos;
				m_dirty = true;
			}
		}

		// CP2
		if(m_target.m_controlPoint2 != null) {
			Vector3 newPos = Handles.PositionHandle(m_target.m_controlPoint2.position, Quaternion.identity);
			if(newPos != m_target.m_controlPoint2.position) {
				Undo.RecordObject(m_target.m_controlPoint2, "CloudController.CP2");
				m_target.m_controlPoint2.position = newPos;
				m_dirty = true;
			}
		}
	}

	/// <summary>
	/// Called every frame.
	/// </summary>
	public void Update() {
		if(m_target == null) return;

		// If dirty and live edit, generate!
		if(m_dirty && m_target.m_liveEdit) {
			Generate();
			m_dirty = false;
		}

		// If doing live preview, manually call the Update on the target
		// Only when not in play mode!
		if(m_target.m_livePreview && !Application.isPlaying) {
			// Compute delta time
			float deltaTime = Time.realtimeSinceStartup - m_lastUpdateTime;
			m_lastUpdateTime = Time.realtimeSinceStartup;

			// Update
			m_target.DoUpdate(deltaTime);
		}
	}

	//------------------------------------------------------------------------//
	// INTERNAL METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Clear current cloud instances and re-create with current settings.
	/// Quite expensive, should only be called from editor.
	/// </summary>
	public void Generate() {
		// Make sure we have all the data
		m_target.UpdateArea();

		// Destroy current clouds and clear list
		Clear();

		// Create new clouds using current setup
		// Except if there are no prefabs to choose from!
		// Or no container!
		if(m_target.m_prefabs.Length == 0) return;
		if(m_target.m_cloudContainer == null) return;
		for(int i = 0; i < m_target.m_amount; i++) {
			// Create a new instance
			GameObject cloud = GameObject.Instantiate(m_target.m_prefabs.GetRandomValue(), m_target.m_cloudContainer, false) as GameObject;
			if(cloud == null) continue;

			// Initialize new instance
			// Scale
			Vector3 newScale = new Vector3(
				m_target.m_xScaleRange.GetRandom(),
				m_target.m_yScaleRange.GetRandom(),
				1f
			);

			// Aspect ratio correction - BEFORE FLIPPING!
			// It may overflow scale limits, we won't care for now
			float ar = newScale.x/newScale.y;
			if(ar < m_target.m_aspectRatioRange.min) {
				newScale.y = newScale.x * m_target.m_aspectRatioRange.min;
			} else if(ar > m_target.m_aspectRatioRange.max) {
				newScale.y = newScale.x * m_target.m_aspectRatioRange.max;
			}

			// Flipping
			if(Random.value < m_target.m_xFlipChance) {
				newScale.x *= -1f;
			}
			if(Random.value < m_target.m_yFlipChance) {
				newScale.y *= -1f;
			}

			// Apply new scale
			cloud.transform.localScale = newScale;

			// Initial position
			// https://docs.unity3d.com/Manual/RandomNumbers.html
			float zDelta = m_target.m_zDistribution.Evaluate(Random.value);
			cloud.transform.position = new Vector3(
				m_target.m_xRange.Lerp(m_target.m_xDistribution.Evaluate(Random.value)),
				m_target.m_yRange.Lerp(m_target.m_yDistribution.Evaluate(Random.value)),
				m_target.m_zRange.Lerp(zDelta)
			);

			// Give a speed to this cloud based on its Z
			float speed = Mathf.Lerp(
				m_target.m_zNearSpeedRange.GetRandom(),
				m_target.m_zFarSpeedRange.GetRandom(),
				zDelta
			);

			// Store new cloud
			m_target.m_clouds.Add(cloud);
			m_target.m_speeds.Add(speed);
			m_target.m_renderers.Add(cloud.GetComponentInChildren<SpriteRenderer>());
		}

		// Update object
		serializedObject.ApplyModifiedProperties();
	}

	/// <summary>
	/// Destroy all current clouds and clear lists.
	/// </summary>
	private void Clear() {
		for(int i = 0; i < m_target.m_clouds.Count; i++) {
			if(m_target.m_clouds[i] != null) {
				GameObject.DestroyImmediate(m_target.m_clouds[i]);
			}
		}
		m_target.m_clouds.Clear();
		m_target.m_speeds.Clear();
		m_target.m_renderers.Clear();

		// Make sure clouds container is empty as well
		if(m_target.m_cloudContainer != null) {
			m_target.m_cloudContainer.DestroyAllChildren(true);
		}
	}

	/// <summary>
	/// Draw the distribution curves and store their new values.
	/// </summary>
	private void DoDistributionCurves() {
		// Aux vars
		SerializedProperty xProp = serializedObject.FindProperty("m_xDistribution");
		SerializedProperty yProp = serializedObject.FindProperty("m_yDistribution");
		SerializedProperty zProp = serializedObject.FindProperty("m_zDistribution");
		Rect curveBounds = new Rect(0f, 0f, 1f, 1f);

		// Group in a foldable widget
		xProp.isExpanded = EditorGUILayout.Foldout(xProp.isExpanded, "Distribution");
		if(xProp.isExpanded) {
			// Indent in
			EditorGUI.indentLevel++;
			EditorGUI.BeginChangeCheck();

			// Info box
			EditorGUILayout.HelpBox(
				"Vertical axis is the relative position [0..1] along the respective area axis.\n" +
				"Horizontal axis is the linear random value [0..1] that will be filtered with the curve.",
				MessageType.Info
			);

			// X curve
			xProp.animationCurveValue = EditorGUILayout.CurveField(
				"X",
				xProp.animationCurveValue,
				Colors.red,
				curveBounds,
				GUILayout.Height(30f)
			);

			// Y curve
			yProp.animationCurveValue = EditorGUILayout.CurveField(
				"Y",
				yProp.animationCurveValue,
				Colors.green,
				curveBounds,
				GUILayout.Height(30f)
			);

			// Z curve
			zProp.animationCurveValue = EditorGUILayout.CurveField(
				"Z",
				zProp.animationCurveValue,
				Colors.blue,
				curveBounds,
				GUILayout.Height(30f)
			);

			// Indent out
			m_dirty |= EditorGUI.EndChangeCheck();
			EditorGUI.indentLevel--;
		}

		// Mark as processed
		m_processedProperties.Add(xProp.name);
		m_processedProperties.Add(yProp.name);
		m_processedProperties.Add(zProp.name);
	}

	/// <summary>
	/// Do a foldable widget using the isExpanded value of a target property.
	/// </summary>
	/// <returns>The property.</returns>
	/// <param name="_propertyId">Property identifier.</param>
	/// <param name="_title">Title.</param>
	private SerializedProperty DoFoldout(string _propertyId, string _title) {
		SerializedProperty p = serializedObject.FindProperty(_propertyId);
		if(p != null) p.isExpanded = EditorGUILayout.Foldout(p.isExpanded, _title);
		return p;
	}

	/// <summary>
	/// Draw a property with the default inspector.
	/// </summary>
	/// <returns>The property.</returns>
	/// <param name="_propertyId">Property identifier.</param>
	private SerializedProperty DoProperty(string _propertyId) {
		SerializedProperty p = serializedObject.FindProperty(_propertyId);
		if(p != null) {
			EditorGUI.BeginChangeCheck();
			EditorGUILayout.PropertyField(p, true);
			m_dirty |= EditorGUI.EndChangeCheck();
		}
		return p;
	}
}