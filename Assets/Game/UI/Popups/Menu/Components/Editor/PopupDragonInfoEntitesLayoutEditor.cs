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
		public string name = "";
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

		// Small tool to print current layout setup so it can be easily applied afterwards in editing time
		if(GUILayout.Button("Print Layout", GUILayout.Height(50f))) {
			PrintLayoutSetup();
		}

		// Tool to save/load values from/to prefs
		// Useful to edit in runtime and then apply the values to the prefab
		EditorGUILayout.BeginHorizontal(); {
			if(GUILayout.Button("Save to Prefs", GUILayout.Height(50f))) {
				SaveToPrefs();
			}

			if(GUILayout.Button("Load from Prefs", GUILayout.Height(50f))) {
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
	/// Small tool to print current layout setup so it can be easily applied 
	/// afterwards in editing time.
	/// </summary>
	private void PrintLayoutSetup() {
		// Create string builder and fill it using the recursive call
		StringBuilder sb = new StringBuilder();
		PrintLayoutSetupRec(m_targetPopupDragonInfoEntitesLayout.GetComponent<RectTransform>(), ref sb, 0);

		// Done! Print it
		Debug.Log(sb.ToString());
	}

	/// <summary>
	/// Recursive call to prepare the layout setup string.
	/// </summary>
	/// <param name="_t">Transform to be attached to the string.</param>
	/// <param name="_Sb">String builder where the string will be attached.</param>
	/// <param name="_indentLevel">Indent level.</param>
	private void PrintLayoutSetupRec(RectTransform _t, ref StringBuilder _sb, int _indentLevel) {
		// Attach transform's info
		// Name
		_sb.Append('\t', _indentLevel)
			.Append("- ")
			.AppendLine(_t.name);

		// Loader's position
		UI3DAddressablesLoader loader = _t.GetComponent<UI3DAddressablesLoader>();
		if(loader != null) {
			_sb.Append('\t', _indentLevel)
				.Append("  anchoredPosition: ")
				.Append(_t.anchoredPosition.ToString())
				.AppendLine();
		}

		// Container's scale and rotation
		else if(_t.name.Contains("Container")) {
			_sb.Append('\t', _indentLevel)
				.Append("  scale: ")
				.Append(_t.localScale.ToString())
				.AppendLine();

			_sb.Append('\t', _indentLevel)
				.Append("  localRotation: ")
				.Append(_t.localRotation.eulerAngles.ToString())
				.AppendLine();
		}

		// Do the recursive call with its children
		for(int i = 0; i < _t.childCount; i++) {
			// Ignore if not rect transform (small trick to ignore placeholder 3D views)
			RectTransform t = _t.GetChild(i).GetComponent<RectTransform>();	// Will fail if not a rect transform
			if(t == null) continue;

			// Everything ok! Do the recursive call
			PrintLayoutSetupRec(t, ref _sb, _indentLevel + 1);
		}
	}

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
			JsonLoaderData loaderData = new JsonLoaderData();
			loaderData.name = loaders[i].name;

			// Store transform properties
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
			JsonLoaderData loaderData = jsonData.loadersData.Find((_data) => _data.name == loaders[i].name);
			if(loaderData == null) continue;

			// Skip if it doesn't have a valid rect transform
			RectTransform rt = loaders[i].GetComponent<RectTransform>();
			if(rt == null) continue;

			// Restore transform properties
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
}