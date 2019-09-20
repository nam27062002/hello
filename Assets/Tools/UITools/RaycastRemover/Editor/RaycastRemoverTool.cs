// RaycastRemoverTool.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 17/09/2019.
// Copyright (c) 2019 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using System.Collections.Generic;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// Simple tool to remove all raycast checks from selected objects.
/// </summary>
public class RaycastRemoverTool : EditorWindow {
	//------------------------------------------------------------------//
	// CONSTANTS														//
	//------------------------------------------------------------------//
	private const float WINDOW_MARGIN = 10f;

	//------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES											//
	//------------------------------------------------------------------//
	// Window instance
	private static RaycastRemoverTool m_instance = null;
	public static RaycastRemoverTool instance {
		get {
			if(m_instance == null) {
				m_instance = (RaycastRemoverTool)EditorWindow.GetWindow(typeof(RaycastRemoverTool));
			}
			return m_instance;
		}
	}

	// Editor flags
	private static bool APPLY_PREFABS {
		get { return EditorPrefs.GetBool("RaycastRemoverTool.APPLY_PREFABS", true); }
		set { EditorPrefs.SetBool("RaycastRemoverTool.APPLY_PREFABS", value); }
	}

	// Internal
	private Vector2 m_scrollPos = Vector2.zero;

	//------------------------------------------------------------------//
	// WINDOW METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Opens the window.
	/// </summary>
	[MenuItem("Tools/Raycast Remover")]
	public static void OpenWindow() {
		// Setup window
		instance.minSize = new Vector2(220f, 135f);

		// Show!
		instance.Show();
		//instance.ShowTab();
		//instance.ShowPopup();
		//instance.ShowUtility();
		//instance.ShowAuxWindow();
	}

	/// <summary>
	/// The editor has been enabled - target object selected.
	/// </summary>
	private void OnEnable() {
		
	}

	/// <summary>
	/// The editor has been disabled - target object unselected.
	/// </summary>
	private void OnDisable() {
		
	}

	/// <summary>
	/// Called 100 times per second on all visible windows.
	/// </summary>
	public void Update() {
		
	}

	/// <summary>
	/// OnInspectorUpdate is called at 10 frames per second to give the inspector a chance to update.
	/// Called less times as if it was OnGUI/Update
	/// </summary>
	public void OnInspectorUpdate() {
		
	}

	/// <summary>
	/// Update the inspector window.
	/// </summary>
	public void OnGUI() {
		// Scroll Rect
		m_scrollPos = EditorGUILayout.BeginScrollView(m_scrollPos);
		{
			// Left Margin
			EditorGUILayout.BeginHorizontal();
			GUILayout.Space(WINDOW_MARGIN);

			// Top margin
			EditorGUILayout.BeginVertical();
			GUILayout.Space(WINDOW_MARGIN);

			// Window Content!
			// Apply Prefabs Toggle
			APPLY_PREFABS = EditorGUILayout.Toggle("Apply Prefabs after", APPLY_PREFABS);

			// Do we have a valid selection?
			bool validSelection = Selection.gameObjects.Length > 0;

			// Button - enable only if valid selection
			EditorGUI.BeginDisabledGroup(!validSelection);
			{
				GUI.color = Color.green;
				if(GUILayout.Button("DISABLE ALL RAYCASTS\nIN SELECTED HIERARCHY", GUILayout.Height(50f))) {
					// Do it!
					RemoveRaycasts(Selection.gameObjects);

					// Show feedback
					ShowNotification(new GUIContent("DONE!"));
				}
				GUI.color = Color.white;
			}
			EditorGUI.EndDisabledGroup();

			// If not valid selection, show info message
			if(!validSelection) {
				EditorGUILayout.HelpBox("No GameObject selected!", MessageType.Warning);
			}

			// Bottom margin
			GUILayout.Space(WINDOW_MARGIN);
			EditorGUILayout.EndVertical();

			// Right margin
			GUILayout.Space(WINDOW_MARGIN);
			EditorGUILayout.EndHorizontal();
		}
		EditorGUILayout.EndScrollView();
	}

	//------------------------------------------------------------------//
	// STATIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Remove raycasts on target object and its hierarchy.
	/// </summary>
	/// <param name="_obj">Target object.</param>
	/// <param name="_disabledGraphics">Amount of graphics (Image, Text, etc.) whose raycast property has been disabled.</param>
	/// <param name="_disabledCanvasGroups">Amount of canvas groups whose raycast property has been disabled.</param>
	/// <param name="_summary">Summary string to report later.</param>
	public static void RemoveRaycasts(GameObject _obj, out int _disabledGraphics, out int _disabledCanvasGroups, ref string _summary) {
		// Init return vars
		_disabledGraphics = 0;
		_disabledCanvasGroups = 0;

		// Check params
		if(_obj == null) return;

		// Graphics
		List<Graphic> graphics = FindComponentsRecursive<Graphic>(_obj.transform);
		for(int i = 0; i < graphics.Count; ++i) {
			if(graphics[i].raycastTarget) {
				graphics[i].raycastTarget = false;
				_disabledGraphics++;
				_summary += "\t" + graphics[i].name + " (" + graphics[i].GetType().Name + ")\n";
			}
		}

		// Canvas groups
		List<CanvasGroup> groups = FindComponentsRecursive<CanvasGroup>(_obj.transform);
		for(int i = 0; i < groups.Count; ++i) {
			if(groups[i].blocksRaycasts) {
				groups[i].blocksRaycasts = false;
				_disabledCanvasGroups++;
				_summary += "\t" + groups[i].name + " (" + groups[i].GetType().Name + ")\n";
			}
		}

		// Apply prefab?
		if(APPLY_PREFABS) {
			GameObject rootObj = PrefabUtility.FindRootGameObjectWithSameParentPrefab(_obj.gameObject);
			bool wasActive = rootObj.activeSelf;
			rootObj.SetActive(true);
			PrefabUtility.ReplacePrefab(
				rootObj,
				PrefabUtility.GetPrefabParent(_obj.gameObject),
				ReplacePrefabOptions.ConnectToPrefab
			);
			rootObj.SetActive(wasActive);
		}

		// Object summary
		_summary = "\n" + _obj.name + ": " + _disabledGraphics + " Graphics and " + _disabledCanvasGroups + " CanvasGroups disabled\n" + _summary;
	}

	/// <summary>
	/// Remove raycasts on target objects and their hierarchy.
	/// </summary>
	/// <param name="_objs">Target objects.</param>
	public static void RemoveRaycasts(GameObject[] _objs) {
		// Aux vars
		int disabledGraphics = 0;
		int disabledGroups = 0;
		int totalDisabledGraphics = 0;
		int totalDisabledGroups = 0;
		string summary = "";

		// Just go 1 by 1
		int objCount = _objs.Length;
		for(int i = 0; i < objCount; ++i) {
			// Show progress bar
			if(EditorUtility.DisplayCancelableProgressBar(
				"Remove Raycasts Tool",
				"Processing " + i + "/" + objCount + " (" + _objs[i].name + ")",
				(float)i/(float)objCount
			)) {
				// If canceled, break loop
				break;
			}

			// Process object
			RemoveRaycasts(_objs[i], out disabledGraphics, out disabledGroups, ref summary);

			// Add up
			totalDisabledGraphics += disabledGraphics;
			totalDisabledGroups += disabledGroups;
		}

		// Clear progress bar
		EditorUtility.ClearProgressBar();

		// Show summary (and log)
		summary = "DONE! " + totalDisabledGraphics + " Graphics and " + totalDisabledGroups + " CanvasGroups disabled in total\n\n" + summary;
		//EditorUtility.DisplayDialog("Remove Raycasts Tool", summary, "OK!");
		RaycastRemoverToolSummaryWindow.OpenWindow("Remove Raycasts Tool", summary, "OK!");
		Debug.Log(summary);
	}

	//------------------------------------------------------------------//
	// INTERNAL METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Returns the first component of type T found in any of the child objects.
	/// </summary>
	/// <returns>The first component of type T found in any of the children objects.</returns>
	/// <param name="_t">The root transform to look wihtin.</param>
	/// <typeparam name="T">The type to look for.</typeparam>
	private static List<T> FindComponentsRecursive<T>(Transform _t) where T : Component {
		// Found!
		List<T> components = new List<T>();

		T c = _t.GetComponent<T>();
		if((c as T) != null) {
			components.Add(c);
		}

		// Not found, iterate children transforms
		foreach(Transform t in _t) {
			components.AddRange(FindComponentsRecursive<T>(t));
		}

		return components;
	}

	//------------------------------------------------------------------//
	// CALLBACKS														//
	//------------------------------------------------------------------//
	/// <summary>
	/// Selection has changed.
	/// </summary>
	private void OnSelectionChange() {
		// Force a refresh of the window
		this.Repaint();
	}
}