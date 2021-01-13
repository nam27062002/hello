// HDAddressablesLoaderEditor.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 13/03/2017.
// Copyright (c) 2017 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using UnityEditor;
using System.IO;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Custom editor for the AddressablesLoader class.
/// </summary>
[CustomEditor(typeof(HDAddressablesLoader), true)]	// True to be used by heir classes as well
[CanEditMultipleObjects]
public class HDAddressablesLoaderEditor : Editor {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	private const float BUTTON_HEIGHT = 25f;

    //------------------------------------------------------------------------//
    // MEMBERS AND PROPERTIES												  //
    //------------------------------------------------------------------------//
    // Casted target object
    private HDAddressablesLoader m_targetUI3DLoader = null;

	// Useful properties
	private SerializedProperty m_resPathProp = null;
	private SerializedProperty m_folderLevelProp = null;
	private SerializedProperty m_assetIdProp = null;

	//------------------------------------------------------------------------//
	// METHODS																  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// The editor has been enabled - target object selected.
	/// </summary>
	private void OnEnable() {
		// Get target object
		m_targetUI3DLoader = target as HDAddressablesLoader;

		// Gather useful properties
		m_resPathProp = serializedObject.FindProperty("m_resourcePath");
		m_folderLevelProp = serializedObject.FindProperty("m_useFolderLevelInID");
		m_assetIdProp = serializedObject.FindProperty("m_assetId");
	}

	/// <summary>
	/// The editor has been disabled - target object unselected.
	/// </summary>
	private void OnDisable() {
		// Clear target object
		m_targetUI3DLoader = null;

		// Clear properties
		m_resPathProp = null;
		m_folderLevelProp = null;
		m_assetIdProp = null;
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

			// Resource Path
			else if(p.name == m_resPathProp.name) {
				// We will automatically convert the chosen resource path into an ID for the addressables system
				// Show all related properties grouped here

				// Resources path - default editor, will use the FileList attribute to display a nice selector
				EditorGUILayout.PropertyField(p, true);

				// Show indented. Disable if path is empty.
				string resPath = p.stringValue;
				bool validPath = !string.IsNullOrEmpty(p.stringValue);  // The resource path could be empty in case we want to select the assetId in runtime
				EditorGUI.BeginDisabledGroup(!validPath);
				EditorGUI.indentLevel++;

				// Folder level: show using a slider for comfort
				// Limit max to the amount of folders in the current resource path - if defined
				int max = 10;
				if(validPath) {
					// Strip project root from the path
					string cleanPath = StringUtils.FormatPath(resPath, StringUtils.PathFormat.ASSETS_ROOT);

					// Find the amount of subdirectories included in the path
					max = cleanPath.Split('/').Length - 1;    // Trick to count folders. We have guaranteed that the path uses '/' since it comes from StringUtils.FormatPath :)
				}
				EditorGUILayout.IntSlider(m_folderLevelProp, 0, max);

				// Final asset ID: Show in a label
				if(validPath) {
					// Figure out asset Id in the catalog from the resources path and folder level properties
					m_assetIdProp.stringValue = HDAddressablesLoader.GetAssetIdFromPath(resPath, m_folderLevelProp.intValue);
				} else {
					// Clear asset Id
					m_assetIdProp.stringValue = string.Empty;
				}
				EditorGUILayout.SelectableLabel("Addressable ID: " + m_assetIdProp.stringValue);

				// Indent back out
				EditorGUI.EndDisabledGroup();
				EditorGUI.indentLevel--;
			}

			else if(p.name == m_folderLevelProp.name || p.name == m_assetIdProp.name) {
				// Already displayed with resourcePath property, do nothing :)
			}

            // Properties we don't want to show
            else if(p.name == "m_ObjectHideFlags") {
				// Do nothing
			}

			// Default property display
			else {
				EditorGUILayout.PropertyField(p, true);
			}
		} while(p.NextVisible(false));      // Only direct children, not grand-children (will be drawn by default if using the default EditorGUI.PropertyField)


		// Tools
		EditorGUILayout.Space();
        EditorGUILayout.BeginHorizontal();
		{
			// Clear loaded egg instance
			GUI.color = Colors.coral;
			if(GUILayout.Button("UNLOAD", GUILayout.Height(BUTTON_HEIGHT))) {
				m_targetUI3DLoader.Unload();
			}

			// Force loading the egg
			GUI.color = Colors.paleGreen;
			if(GUILayout.Button("RELOAD", GUILayout.Height(BUTTON_HEIGHT))) {
				m_targetUI3DLoader.Load();
			}

			// Load placeholder
			GUI.color = Colors.paleYellow;
			if(GUILayout.Button("LOAD PLACEHOLDER", GUILayout.Height(BUTTON_HEIGHT))) {
				GameObject newInstance = m_targetUI3DLoader.Load();
				if(newInstance != null) {
					// Destroy as soon as awaken (this is meant to be used in edit mode)
					newInstance.AddComponent<SelfDestroy>().seconds = 0f;

					// Rename
					newInstance.gameObject.name = "PLACEHOLDER";
				}
			}

			// Reset color
			GUI.color = Color.white;
		}
		EditorGUILayout.EndHorizontal();

		// Apply changes to the serialized object - always do this in the end of OnInspectorGUI.
		serializedObject.ApplyModifiedProperties();
	}

	/// <summary>
	/// The scene is being refreshed.
	/// </summary>
	public void OnSceneGUI() {
		// Scene-related stuff
	}

	/// <summary>
	/// Draw gizmos.
	/// </summary>
	/// <param name="_target"></param>
	/// <param name="_gizmo"></param>
	[DrawGizmo(GizmoType.Active | GizmoType.InSelectionHierarchy)]
	public static void DoGizmos(HDAddressablesLoader _target, GizmoType _gizmo) {
		// Color and matrix
		Gizmos.color = Colors.WithAlpha(Color.red, 0.25f);
		Gizmos.matrix = _target.transform.localToWorldMatrix;

		// If target doesn't have a rect transform, just draw a cube
		RectTransform rt = _target.transform as RectTransform;
		if(rt != null) {
			// Correct pivot (DrawCube's 0,0 is the center whereas graphic's 0,0 is bot-left)
			Vector2 pivotCorrection = new Vector2(rt.pivot.x - 0.5f, rt.pivot.y - 0.5f);  // Pivot from [0..1] to [-0.5..0.5]
			Vector2 cubePos = new Vector2(rt.rect.width * (-pivotCorrection.x), rt.rect.height * (-pivotCorrection.y));
			Gizmos.DrawCube(new Vector3(cubePos.x, cubePos.y, 0f), new Vector3(rt.rect.width, rt.rect.height, 1f));
		} else {
			Gizmos.DrawCube(Vector3.zero, Vector3.one);
		}

		// Restore matrix and color
		Gizmos.matrix = Matrix4x4.identity;
		Gizmos.color = Colors.white;
	}
}
