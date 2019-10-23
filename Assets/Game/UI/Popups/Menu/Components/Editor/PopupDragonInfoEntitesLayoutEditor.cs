// PopupDragonInfoEntitesLayoutEditor.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 13/03/2017.
// Copyright (c) 2017 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using UnityEditor;
using System.Text;
using System.Collections.Generic;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Custom editor for the PopupDragonInfoEntitesLayout class.
/// </summary>
[CustomEditor(typeof(PopupDragonInfoEntitesLayout), true)]	// True to be used by heir classes as well
public class PopupDragonInfoEntitesLayoutEditor : Editor {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	// In order to be able to use unity's json utilities, we need to work through serializable objects
	// https://docs.unity3d.com/ScriptReference/SerializeField.html
	[System.Serializable]
	private class JsonLoaderData {
		public string uniqueId = "";

		public Vector2 anchorMin = Vector2.zero;
		public Vector2 anchorMax = Vector2.zero;
		public Vector2 pivot = Vector2.zero;
		public Vector2 anchoredPosition = Vector2.zero;

		public Vector3 containerScale = Vector3.one;
		public Vector3 containerRotationEuler = Vector3.zero;
	}

	[System.Serializable]
	private class JsonData {
		public List<JsonLoaderData> loadersData = new List<JsonLoaderData>();
	}

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Casted target object
	PopupDragonInfoEntitesLayout m_targetPopupDragonInfoEntitesLayout = null;

	//------------------------------------------------------------------------//
	// METHODS																  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// The editor has been enabled - target object selected.
	/// </summary>
	private void OnEnable() {
		// Get target object
		m_targetPopupDragonInfoEntitesLayout = target as PopupDragonInfoEntitesLayout;
	}

	/// <summary>
	/// The editor has been disabled - target object unselected.
	/// </summary>
	private void OnDisable() {
		// Clear target object
		m_targetPopupDragonInfoEntitesLayout = null;
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

			// Properties we don't want to show
			else if(p.name == "m_ObjectHideFlags") {
				// Do nothing
			}

			// Default property display
			else {
				EditorGUILayout.PropertyField(p, true);
			}
		} while(p.NextVisible(false));		// Only direct children, not grand-children (will be drawn by default if using the default EditorGUI.PropertyField)

		// Tool to save/load values from/to prefs
		// Useful to edit in runtime and then apply the values to the prefab
		EditorGUILayout.BeginHorizontal(); {
			if(GUILayout.Button("Save to Prefs", GUILayout.Height(35f))) {
				SaveToPrefs();
			}

			if(GUILayout.Button("Load from Prefs", GUILayout.Height(35f))) {
				LoadFromPrefs();
			}
		} EditorGUILayout.EndHorizontal();

		// Apply changes to the serialized object - always do this in the end of OnInspectorGUI.
		serializedObject.ApplyModifiedProperties();
	}

	/// <summary>
	/// The scene is being refreshed.
	/// </summary>
	public void OnSceneGUI() {
		// Scene-related stuff
	}

	//------------------------------------------------------------------------//
	// INTERNAL METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Save relevant data to prefs to be loaded afterwards.
	/// </summary>
	private void SaveToPrefs() {
		// Compose in a json for easier parsing
		JsonData jsonData = new JsonData();

		// Add an entry for each loader child of this layout
		// Loaders should have different names, as name will be used as key
		UI3DAddressablesLoader[] loaders = m_targetPopupDragonInfoEntitesLayout.GetComponentsInChildren<UI3DAddressablesLoader>();
		for(int i = 0; i < loaders.Length; i++) {
			// Skip if it doesn't have a valid rect transform
			RectTransform rt = loaders[i].GetComponent<RectTransform>();
			if(rt == null) continue;

			// Create a new data object
			// Don't use name as unique ID, since they are all called "loader"!
			// Use custom function (path + child index)
			JsonLoaderData loaderData = new JsonLoaderData();
			loaderData.uniqueId = GenerateUniqueId(loaders[i].transform);

			// Store transform properties
			loaderData.anchorMin = rt.anchorMin;
			loaderData.anchorMax = rt.anchorMax;
			loaderData.pivot = rt.pivot;
			loaderData.anchoredPosition = rt.anchoredPosition;

			// Store container's scale and rotation
			Transform container = loaders[i].container;
			if(container != null) {
				loaderData.containerScale = container.localScale;
				loaderData.containerRotationEuler = container.localEulerAngles;
			}

			// Add to dictionary
			jsonData.loadersData.Add(loaderData);
		}

		// Convert to json and store to prefs!
		string prefsID = GetPrefsID();
		string jsonString = JsonUtility.ToJson(jsonData, true);
		Debug.Log("Saving " + prefsID + " to Prefs:\n" + jsonString);
		EditorPrefs.SetString(prefsID, jsonString);
	}

	/// <summary>
	/// Load previously saved relevant data from prefs.
	/// </summary>
	private void LoadFromPrefs() {
		// Nothing to do if there is no data for this object
		string prefsID = GetPrefsID();
		if(!EditorPrefs.HasKey(prefsID)) {
			Debug.LogError("No saved data could be found for ID " + prefsID + ", doing nothing");
			return;
		}

		// Load from json!
		string jsonString = EditorPrefs.GetString(prefsID);
		JsonData jsonData = JsonUtility.FromJson<JsonData>(jsonString);
		Debug.Log("Loading " + prefsID + " from Prefs:\n" + jsonString);

		// Apply! There should be an entry for every loader in the layout, by name
		// Loaders should the same name as when the data had been stored
		UI3DAddressablesLoader[] loaders = m_targetPopupDragonInfoEntitesLayout.GetComponentsInChildren<UI3DAddressablesLoader>();
		for(int i = 0; i < loaders.Length; i++) {
			// Skip if we don't have data for this loader
			JsonLoaderData loaderData = jsonData.loadersData.Find((_data) => _data.uniqueId == GenerateUniqueId(loaders[i].transform));
			if(loaderData == null) continue;

			// Skip if it doesn't have a valid rect transform
			RectTransform rt = loaders[i].GetComponent<RectTransform>();
			if(rt == null) continue;

			// Restore transform properties
			rt.anchorMin = loaderData.anchorMin;
			rt.anchorMax = loaderData.anchorMax;
			rt.pivot = loaderData.pivot;
			rt.anchoredPosition = loaderData.anchoredPosition;

			// Restore container's scale and rotation
			Transform container = loaders[i].container;
			if(container != null) {
				container.localScale = loaderData.containerScale;
				container.localEulerAngles = loaderData.containerRotationEuler;
			}
		}

		// Mark object as dirty so it's saved
		EditorUtility.SetDirty(target);
	}

	/// <summary>
	/// Get the ID used as key to store this layout's data into prefs.
	/// </summary>
	/// <returns>The ID corresponding to this layout.</returns>
	private string GetPrefsID() {
		// Strip the (Clone) from the target's name
		return Util.RemoveCloneSuffix(m_targetPopupDragonInfoEntitesLayout.name);
	}

	/// <summary>
	/// Generate a unique ID for a transform object based on its path in the hierarchy and child index.
	/// </summary>
	/// <param name="_t">Target transform.</param>
	/// <returns>Unique Id for the given transform.</returns>
	private string GenerateUniqueId(Transform _t) {
		// Concatenate path until prefab root
		string path = _t.name;
		Transform t = _t.parent;
		while(t != null && !t.name.Contains("PF_")) {
			path = path.Insert(0, t.name + "/");
			t = t.parent;
		}
		path = path.Insert(0, Util.RemoveCloneSuffix(t.name) + "/");

		// Concatenate child index (in case some siblings have the same name)
		path += "_" + _t.GetSiblingIndex();

		// Done!
		return path;
	}
}