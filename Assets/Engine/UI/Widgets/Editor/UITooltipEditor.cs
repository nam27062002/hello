// UITooltipEditor.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 29/11/2018.
// Copyright (c) 2018 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;
using System.Collections.Generic;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Custom editor for the UITooltip class.
/// </summary>
[CustomEditor(typeof(UITooltip), true)]	// True to be used by heir classes as well
[CanEditMultipleObjects]
public class UITooltipEditor : Editor {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Casted target object
	UITooltip m_targetUITooltip = null;

	// Editor flags
	private static bool applyPrefabAfterDisablingRaycasts {
		get { return Prefs.GetBoolEditor("UITooltipEditor.applyPrefabAfterDisablingRaycasts", true); }
		set { Prefs.SetBoolEditor("UITooltipEditor.applyPrefabAfterDisablingRaycasts", value); }
	}

	//------------------------------------------------------------------------//
	// METHODS																  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// The editor has been enabled - target object selected.
	/// </summary>
	private void OnEnable() {
		// Get target object
		m_targetUITooltip = target as UITooltip;
	}

	/// <summary>
	/// The editor has been disabled - target object unselected.
	/// </summary>
	private void OnDisable() {
		// Clear target object
		m_targetUITooltip = null;
	}

	/// <summary>
	/// Draw the inspector.
	/// </summary>
	public override void OnInspectorGUI() {
		// Default inspector
		DrawDefaultInspector();

		// Raycast disabling tool
		DoRaycastDisablingTool(m_targetUITooltip.gameObject);
	}

	/// <summary>
	/// The scene is being refreshed.
	/// </summary>
	public void OnSceneGUI() {
		// Scene-related stuff
	}

	//------------------------------------------------------------------------//
	// AUX METHODS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Show the raycast disabling tool in the inspector UI.
	/// </summary>
	public static void DoRaycastDisablingTool(GameObject _target) {
		// Button to disable all raycasting in nested objects
		EditorGUILayoutExt.Separator();

		// Apply prefab toggle
		bool applyPrefab = EditorGUILayout.Toggle("Apply Prefab after disabling raycasts", applyPrefabAfterDisablingRaycasts);
		applyPrefabAfterDisablingRaycasts = applyPrefab;

		// If apply prefab is toggled, disable feature and show warning message
		if(applyPrefab) {
			GUI.color = Color.red;
			EditorGUILayout.HelpBox(
				"\nApply Prefab feature doesn't support the new prefabs workflow.\n\n" +
				"Toggle it off to use the Raycast Disable tool.\n\n" +
				"TODO as soon as possible.\n"
				, MessageType.Warning, true);
			GUI.color = Color.white;
		}

		EditorGUI.BeginDisabledGroup(applyPrefab);
		GUI.color = Colors.orange;
		if(GUILayout.Button("DISABLE ALL RAYCASTS", GUILayout.Height(50f))) {
			// Graphics
			List<Graphic> graphics = _target.transform.FindComponentsRecursive<Graphic>();
			for(int i = 0; i < graphics.Count; ++i) {
				graphics[i].raycastTarget = false;
			}

			// Canvas groups
			List<CanvasGroup> groups = _target.transform.FindComponentsRecursive<CanvasGroup>();
			for(int i = 0; i < groups.Count; ++i) {
				groups[i].blocksRaycasts = false;
			}

			// Apply prefab?
			if(applyPrefabAfterDisablingRaycasts) {
				GameObject rootObj = PrefabUtility.FindRootGameObjectWithSameParentPrefab(_target);
				bool wasActive = rootObj.activeSelf;
				rootObj.SetActive(true);
				PrefabUtility.ReplacePrefab(
					rootObj,
					PrefabUtility.GetPrefabParent(_target),
					ReplacePrefabOptions.ConnectToPrefab
				);
				rootObj.SetActive(wasActive);
			}
		}
		GUI.color = Color.white;
		EditorGUI.EndDisabledGroup();
	}
}