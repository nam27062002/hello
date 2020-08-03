// UI3DLoaderEditor.cs
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
/// Custom editor for the UI3DLoader class.
/// </summary>
[CustomEditor(typeof(UI3DAddressablesLoader), true)]	// True to be used by heir classes as well
[CanEditMultipleObjects]
public class UI3DAddressablesLoaderEditor : Editor {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	private const float BUTTON_HEIGHT = 25f;

    //------------------------------------------------------------------------//
    // MEMBERS AND PROPERTIES												  //
    //------------------------------------------------------------------------//
    // Casted target object
    UI3DAddressablesLoader m_targetUI3DLoader = null;

	//------------------------------------------------------------------------//
	// METHODS																  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// The editor has been enabled - target object selected.
	/// </summary>
	private void OnEnable() {
		// Get target object
		m_targetUI3DLoader = target as UI3DAddressablesLoader;

    }

	/// <summary>
	/// The editor has been disabled - target object unselected.
	/// </summary>
	private void OnDisable() {
		// Clear target object
		m_targetUI3DLoader = null;
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
            else if (p.name == "m_assetId") {
                
                // The resource path could be empty in case we want to select the assetId in runtime
                if (m_targetUI3DLoader.resourcePath != "")
                {
                    string id = Path.GetFileNameWithoutExtension(m_targetUI3DLoader.resourcePath);
                    string path = Path.GetDirectoryName(m_targetUI3DLoader.resourcePath);
                    int i = m_targetUI3DLoader.useFolderLevelInID;
                    while (i > 0)
                    {
                        // The incoming path will be always use the separator '/' regardless of the OS
                        int index = path.LastIndexOf("/");
                        if (index > 0)
                        {
                            // In the catalog the separator is always "/"
                            id = path.Substring(index + 1) + "/" + id;
                            path = path.Substring(0, index);
                            i--;
                        }
                        else
                        {
                            i = 0;
                        }
                    }

                    p.stringValue = id;
                    
                }

                EditorGUILayout.LabelField("    Addressable ID: " + p.stringValue);
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
	public static void DoGizmos(UI3DAddressablesLoader _target, GizmoType _gizmo) {
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
