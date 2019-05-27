// ColorRampNewCollectionWindow.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 24/05/2019.
// Copyright (c) 2019 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System;
using System.IO;
using System.Collections.Generic;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Custom editor window for the color ramp editor tool.
/// </summary>
public class ColorRampNewCollectionWindow : PopupWindowContent {
	//-------------------------------------------------------------------------//
	// CONSTANTS															   //
	//-------------------------------------------------------------------------//


	//-------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												   //
	//-------------------------------------------------------------------------//
	// Internal data
	private string m_newCollectionName = "";
	private Rect m_wantedSize;
	private string m_error = null;

	// Callbacks
	private Action<string> m_onCollectionCreated;	// Params: string _collectionName

	//-------------------------------------------------------------------------//
	// METHODS																   //
	//-------------------------------------------------------------------------//
	/// <summary>
	/// Parametrized constructor.
	/// </summary>
	/// <param name="_onCollectionCreated">The function to be invoked when a new collection is successfully created.</param>
	public ColorRampNewCollectionWindow(Action<string> _onCollectionCreated) {
		// Store callback
		m_onCollectionCreated = _onCollectionCreated;
	}

	/// <summary>
	/// Update the inspector window.
	/// </summary>
	public override void OnGUI(Rect _rect) {
		// Process keyboard
		KeyboardHandling(editorWindow);

		// Aux vars
		float labelWidth = 80f;

		// Start the layout!
		Rect size = EditorGUILayout.BeginVertical();

		// If not in the layout state, store target size 
		if(Event.current.type != EventType.Layout) {
			m_wantedSize = size;
		}

		// Header
		GUILayout.BeginHorizontal(); {
			GUILayout.Label("New Color Ramp Collection", EditorStyles.boldLabel);
		} GUILayout.EndHorizontal();

		EditorGUI.BeginChangeCheck();
		{
			// Name
			GUILayout.BeginHorizontal();
			{
				GUILayout.Label("Name", GUILayout.Width(labelWidth));

				EditorGUI.FocusTextInControl("NewCollectionName");
				GUI.SetNextControlName("NewCollectionName");
				m_newCollectionName = GUILayout.TextField(m_newCollectionName);
			}
			GUILayout.EndHorizontal();
		}
		if(EditorGUI.EndChangeCheck()) {
			// Clear error
			m_error = null;
		}

		// Create
		GUILayout.BeginHorizontal();
		{
			// If there is an error, show it
			if(!string.IsNullOrEmpty(m_error)) {
				Color orgColor = GUI.color;
				GUI.color = new Color(1, 0.8f, 0.8f);
				GUILayout.Label(m_error, EditorStyles.helpBox);
				GUI.color = orgColor;
			}

			// Push button to the bottom
			GUILayout.FlexibleSpace();

			// Button
			if(GUILayout.Button("Create")) {
				CreateCollectionAndClose(editorWindow);
			}
		}
		GUILayout.EndHorizontal();

		// Final space
		GUILayout.Space(15);

		EditorGUILayout.EndVertical();
	}

	/// <summary>
	/// Return the expected size of the window.
	/// </summary>
	/// <returns>The expected window size.</returns>
	public override Vector2 GetWindowSize() {
		return new Vector2(
			350,
			m_wantedSize.height > 0 ? m_wantedSize.height : 90
		);
	}

	/// <summary>
	/// Process keyboard events.
	/// </summary>
	/// <param name="_parentWindow">Window holding this content.</param>
	private void KeyboardHandling(EditorWindow _parentWindow) {
		// Parse pressed keys
		Event evt = Event.current;
		if(evt.type == EventType.KeyDown) {
			switch(evt.keyCode) {
				// Accept
				case KeyCode.KeypadEnter:
				case KeyCode.Return: {
					CreateCollectionAndClose(_parentWindow);
				} break;
				
				// Cancel
				case KeyCode.Escape: {
					_parentWindow.Close();
				} break;
			}
		}
	}

	/// <summary>
	/// Create a new collection to the target location and closes the window.
	/// </summary>
	/// <param name="_parentWindow">Window holding this content.</param>
	private void CreateCollectionAndClose(EditorWindow _parentWindow) {
		// Compose path
		string path = ColorRampEditor.DATA_PATH + m_newCollectionName + ".asset";

		// Check for errors
		if(!InternalEditorUtility.IsValidFileName(m_newCollectionName)) {
			string invalid = InternalEditorUtility.GetDisplayStringOfInvalidCharsOfFileName(path);
			if(invalid.Length > 0) {
				m_error = string.Format("A collection filename cannot contain the following character{0}:  {1}", invalid.Length > 1 ? "s" : "", invalid);
			} else {
				m_error = "Invalid filename";
			}
		} else if(File.Exists(path)) {
			m_error = "Collection '" + m_newCollectionName + "' already exists! Ensure a unique name.";
		}

		// If there was no error, create the collection, invoke callback and close ourselves
		if(string.IsNullOrEmpty(m_error)) {
			// Create the collection!
			ColorRampCollection newCollection = ScriptableObject.CreateInstance<ColorRampCollection>();
			newCollection.name = m_newCollectionName;
			AssetDatabase.CreateAsset(newCollection, path);
			AssetDatabase.SaveAssets();

			// Focus new collection
			EditorUtility.FocusProjectWindow();
			Selection.activeObject = newCollection;

			// Invoke callback
			if(m_onCollectionCreated != null) {
				m_onCollectionCreated.Invoke(m_newCollectionName);
			}

			// Close ourselves
			_parentWindow.Close();
		}
	}
}