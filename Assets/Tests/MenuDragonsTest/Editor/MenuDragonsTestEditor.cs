// MenuDragonsTestEditor.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 17/07/2019.
// Copyright (c) 2019 Ubisoft. All rights reserved.

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
/// Custom editor for the MenuDragonsTest class.
/// </summary>
[CustomEditor(typeof(MenuDragonsTest), true)]	// True to be used by heir classes as well
[CanEditMultipleObjects]
public class MenuDragonsTestEditor : Editor {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	private const string EXPORT_KEY = "MenuDragonsTestEditor.";

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Casted target object
	private MenuDragonsTest m_target = null;

	//------------------------------------------------------------------------//
	// METHODS																  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// The editor has been enabled - target object selected.
	/// </summary>
	private void OnEnable() {
		// Get target object
		m_target = target as MenuDragonsTest;
	}

	/// <summary>
	/// The editor has been disabled - target object unselected.
	/// </summary>
	private void OnDisable() {
		// Clear target object
		m_target = null;
	}

	/// <summary>
	/// Draw the inspector.
	/// </summary>
	public override void OnInspectorGUI() {
		// Debug GUI
		DoDebugGUI();

		// Separator
		EditorGUILayoutExt.Separator();

		// Default inspector
		DrawDefaultInspector();
	}

	/// <summary>
	/// The scene is being refreshed.
	/// </summary>
	public void OnSceneGUI() {
		// Scene-related stuff
	}

	/// <summary>
	/// Draw the debug GUI in the inspector.
	/// To be called from the OnGUI() method.
	/// </summary>
	private void DoDebugGUI() {
		// Navigation Buttons
		EditorGUILayout.BeginHorizontal();
		{
			if(GUILayout.Button("←", GUILayout.Height(30f))) {
				m_target.FocusPreviousDragon();
			}

			if(GUILayout.Button("→", GUILayout.Height(30f))) {
				m_target.FocusNextDragon();
			}
		}
		EditorGUILayout.EndHorizontal();

		// Edition buttons
		EditorGUILayout.BeginHorizontal();
		{
			if(GUILayout.Button("Reset Scales To 1", GUILayout.Height(30f))) {
				RecordUndo("Reset Scales To 1");
				m_target.ResetScales(false);
			}

			if(GUILayout.Button("Reset Scales To 1\nApplying Scale Modfier", GUILayout.Height(30f))) {
				RecordUndo("Reset Scales To 1 Applying Scale Modifier");
				m_target.ResetScales(true);
			}

			GUI.color = Colors.skyBlue;
			if(GUILayout.Button("Apply Curve", GUILayout.Height(30f))) {
				RecordUndo("Apply Curve");
				m_target.ApplyCurve();
			}
			GUI.color = Color.white;
		}
		EditorGUILayout.EndHorizontal();

		// Import/Export buttons
		EditorGUILayout.BeginHorizontal();
		{
			if(GUILayout.Button("Export Data ↗", GUILayout.Height(30f))) {
				Export();
			}

			if(GUILayout.Button("Import Data ↘", GUILayout.Height(30f))) {
				Undo.RecordObject(m_target, "Import Data");
				Import();
			}
		}
		EditorGUILayout.EndHorizontal();

		// Space
		EditorGUILayout.Space();

		// Save button
		GUI.color = Colors.paleGreen;
		EditorGUI.BeginDisabledGroup(Application.isPlaying);
		if(GUILayout.Button("SAVE PREFABS" + (Application.isPlaying ? "\n(EDIT MODE ONLY)" : ""), GUILayout.Height(30f))) {
			int processedCount = 0;
			int totalDragons = m_target.m_dragons.Length;
			foreach(MenuDragonsTest.DragonData dragon in m_target.m_dragons) {
				// Show progress bar
				processedCount++;
				if(EditorUtility.DisplayCancelableProgressBar(
					"Saving Dragon Prefabs...",
					"Processing dragon " + processedCount + "/" + totalDragons + ": " + dragon.preview.sku,
					(float)processedCount / (float)totalDragons
				)) {
					// Canceled!
					break;	// Just break the loop
				}

				// Do it!
				PrefabUtility.ReplacePrefab(
					dragon.preview.gameObject,
					PrefabUtility.GetPrefabParent(dragon.preview.gameObject),
					ReplacePrefabOptions.ConnectToPrefab
				);
			}
			EditorUtility.ClearProgressBar();
		}
		EditorGUI.EndDisabledGroup();
		GUI.color = Color.white;
	}

	/// <summary>
	/// Record an Undo action an all dragons (and grids) targeted by the MenuDragonsTest.
	/// </summary>
	/// <param name="_name">Name of the undo action.</param>
	private void RecordUndo(string _name) {
		// Iterate all dragons
		List<UnityEngine.Object> toRecord = new List<UnityEngine.Object>();
		foreach(MenuDragonsTest.DragonData dragon in m_target.m_dragons) {
			// Record all objects that might be modified
			toRecord.Add(dragon.preview.transform);
			toRecord.Add(dragon.grid);
			toRecord.Add(dragon.grid.GetComponent<UIColorFX>());
		}

		// Record undo
		Undo.RecordObjects(toRecord.ToArray(), _name);
	}

	/// <summary>
	/// Export to prefs.
	/// </summary>
	private void Export() {
		// Mark that we have a valid export
		Prefs.SetBoolEditor(EXPORT_KEY + "ValidData", true);

		// Scale range
		Prefs.SetRangeEditor(EXPORT_KEY + "ScaleRange", m_target.m_scaleRange);

		// Scale by tier
		Prefs.SetBoolEditor(EXPORT_KEY + "ScaleByTier", m_target.m_scaleByTier);

		// Dragons data
		foreach(MenuDragonsTest.DragonData dragon in m_target.m_dragons) {
			string prefix = EXPORT_KEY + dragon.preview.sku + ".";
			Prefs.SetFloatEditor(prefix + "ScaleModifier", dragon.scaleModifier);
		}
	}

	/// <summary>
	/// Import from prefs.
	/// </summary>
	private void Import() {
		// Do we have any exported data?
		if(!Prefs.GetBoolEditor(EXPORT_KEY + "ValidData")) {
			Debug.LogError("No data exported! Nothing to import.");
			return;
		}

		// Scale range
		m_target.m_scaleRange = Prefs.GetRangeEditor(EXPORT_KEY + "ScaleRange", m_target.m_scaleRange);

		// Scale by tier
		m_target.m_scaleByTier = Prefs.GetBoolEditor(EXPORT_KEY + "ScaleByTier", m_target.m_scaleByTier);

		// Dragons data
		foreach(MenuDragonsTest.DragonData dragon in m_target.m_dragons) {
			string prefix = EXPORT_KEY + dragon.preview.sku + ".";
			dragon.scaleModifier = Prefs.GetFloatEditor(prefix + "ScaleModifier", dragon.scaleModifier);
		}
	}
}