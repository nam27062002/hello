// ScenePicker.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 01/10/2019 //*//
// Copyright (c) 2019 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Text;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Tool to select objects by right-clicking on the scene view.
/// </summary>
[InitializeOnLoad]
public class ScenePicker : Editor {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Auxiliar class.
	/// </summary>
	private class ObjectData {
		public GameObject obj = null;
		public int hierarchyLevel = 0;
		public string hierarchyPath = string.Empty;	// Up to parent object
		public int childIdx = 0;
		public int pickOrder = 0;
	}

	private const string MENU_PATH = "Tools/Scene Picker";

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Editor flags
	private static bool ENABLED {
		get { return EditorPrefs.GetBool("ScenePicker.ENABLED", true); }
		set { EditorPrefs.SetBool("ScenePicker.ENABLED", value); }
	}

	//------------------------------------------------------------------------//
	// METHODS																  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Default constructor.
	/// </summary>
	static ScenePicker() {
		// Initialize
		// Need to delay until first editor tick so that the menu will be populated before setting the check state
		EditorApplication.delayCall += () => { ToggleEnabled(ENABLED); };
	}

	/// <summary>
	/// Draw GUI on the scene view window.
	/// </summary>
	/// <param name="_sceneView">The scene view that triggered the event.</param>
	private static void OnSceneGUI(SceneView _sceneView) {
		// Check for right click
		if(Event.current.button == 1) {
			// CLick?
			if(Event.current.type == EventType.MouseDown) {
				// Create a generic contextual menu
				OpenContextualMenu();
			}
		}
	}

	/// <summary>
	/// Find all the objects in the scene under the cursor position and list them
	/// in a contextual menu.
	/// </summary>
	private static void OpenContextualMenu() {
		// Aux vars
		StringBuilder sb = new StringBuilder();

		// Find the list of objects under the cursor
		// Extracted from Unity source code: https://github.com/Unity-Technologies/UnityCsReference/blob/f50ab75c509cab05254e0ff2f06eb74f5ecd30da/Editor/Mono/SceneView/SceneViewPicking.cs
		// Pick the topmost object in loop and add it to the ignore list for next iteration, until there are no more objects to pick
		Vector2 pos = Event.current.mousePosition;
		List<ObjectData> pickedObjects = new List<ObjectData>();
		List<GameObject> processedObjects = new List<GameObject>();
		while(true) {
			// Pick topmost object, ignoring all previously picked
			GameObject go = HandleUtility.PickGameObject(pos, false, processedObjects.ToArray());

			// If nothing else to pick, break the loop
			if(go == null) {
				break;
			}

			// Add to processed list
			processedObjects.Add(go);

			// Ignore some objects
			if(go.name.Contains("TMP SubMeshUI")) {
				continue;
			}

			// Find out some extra info about this object
			ObjectData data = new ObjectData();
			data.obj = go;

			// Indent level and hierarchy
			sb.Length = 0;
			data.hierarchyLevel = 0;
			Transform t = data.obj.transform;
			while(t.parent != null && data.hierarchyLevel < 50) {   // Put a limit just in case
				// Get parent
				t = t.parent;

				// Update path
				if(data.hierarchyLevel > 0) {
					sb.Insert(0, '/');
				}
				sb.Insert(0, t.name);

				// Hierarchy level
				++data.hierarchyLevel;
			}
			data.hierarchyPath = sb.ToString();

			// Child index
			data.childIdx = data.obj.transform.GetSiblingIndex();

			// Pick order
			data.pickOrder = pickedObjects.Count;

			// Add to list
			pickedObjects.Add(data);
		}

		// Sort the list at our convenience
		pickedObjects.Sort(CompareObjectData);

		// Create contextual menu, populate and open it!
		GenericMenu menu = new GenericMenu();
		for(int i = 0; i < pickedObjects.Count; ++i) {
			// Get data
			ObjectData data = pickedObjects[i];

			// Custom name formatting
			sb.Length = 0;

			// Indentation
			sb.Append('_', data.hierarchyLevel * 2);
			sb.Append(' ');

			// Object path
			/*
			string hierarchyPath = pickedObjects[i].hierarchyPath;
			hierarchyPath = hierarchyPath.Replace('/', '\\');
			sb.Append(hierarchyPath).Append('\\');
			*/

			// Object name
			sb.Append(data.obj.name);

			// Custom name formatting if object is disabled
			if(!data.obj.activeInHierarchy) {
				sb.Append(" *");
			}

			// Add item to the list
			menu.AddItem(new GUIContent(sb.ToString()), false, OnMenuItemClick, data.obj);
		}
		menu.ShowAsContext();
	}

	//------------------------------------------------------------------------//
	// INTERNAL METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Comparer for ObjectData type.
	/// </summary>
	/// <param name="_data1">First item to be compared.</param>
	/// <param name="_data2">Second item to be compared.</param>
	/// <returns>-1 if <paramref name="_data1"/> goes before <paramref name="_data2"/>; 1 if <paramref name="_data2"/> goes before <paramref name="_data1"/>; 0 otherwise.</returns>
	private static int CompareObjectData(ObjectData _data1, ObjectData _data2) {
		// Null check
		if(_data1 == null) {
			if(_data2 == null) {
				return 0;	// Both null, same order
			} else {
				return 1;	// 2 goes first
			}
		} else if(_data2 == null) {
			return -1;	// 1 goes first
		}

		// Sort!
		int order = 0;
		int reverse = 1;    // -1 to reverse, 1 to normal order
		if(order == 0) order = CompareByHierarchyPath(_data1, _data2) * reverse;
		if(order == 0) order = CompareByChildIdx(_data1, _data2) * reverse;
		return order;
	}

	/// <summary>
	/// Comparer for ObjectData type using their Hierarchy Path field.
	/// Doesn't check for null parameters.
	/// </summary>
	/// <param name="_data1">First item to be compared.</param>
	/// <param name="_data2">Second item to be compared.</param>
	/// <returns>-1 if <paramref name="_data1"/> goes before <paramref name="_data2"/>; 1 if <paramref name="_data2"/> goes before <paramref name="_data1"/>; 0 otherwise.</returns>
	private static int CompareByHierarchyPath(ObjectData _data1, ObjectData _data2) {
		return string.Compare(_data1.hierarchyPath, _data2.hierarchyPath, false, System.Globalization.CultureInfo.InvariantCulture);
	}

	/// <summary>
	/// Comparer for ObjectData type using their Child Index field.
	/// Doesn't check for null parameters.
	/// </summary>
	/// <param name="_data1">First item to be compared.</param>
	/// <param name="_data2">Second item to be compared.</param>
	/// <returns>-1 if <paramref name="_data1"/> goes before <paramref name="_data2"/>; 1 if <paramref name="_data2"/> goes before <paramref name="_data1"/>; 0 otherwise.</returns>
	private static int CompareByChildIdx(ObjectData _data1, ObjectData _data2) {
		return _data1.childIdx.CompareTo(_data2.childIdx);
	}

	/// <summary>
	/// Comparer for ObjectData type using their Pick Order field.
	/// Doesn't check for null parameters.
	/// </summary>
	/// <param name="_data1">First item to be compared.</param>
	/// <param name="_data2">Second item to be compared.</param>
	/// <returns>-1 if <paramref name="_data1"/> goes before <paramref name="_data2"/>; 1 if <paramref name="_data2"/> goes before <paramref name="_data1"/>; 0 otherwise.</returns>
	private static int CompareByPickOrder(ObjectData _data1, ObjectData _data2) {
		return _data1.pickOrder.CompareTo(_data2.pickOrder);
	}

	/// <summary>
	/// Comparer for ObjectData type using their Hierarchy Level field.
	/// Doesn't check for null parameters.
	/// </summary>
	/// <param name="_data1">First item to be compared.</param>
	/// <param name="_data2">Second item to be compared.</param>
	/// <returns>-1 if <paramref name="_data1"/> goes before <paramref name="_data2"/>; 1 if <paramref name="_data2"/> goes before <paramref name="_data1"/>; 0 otherwise.</returns>
	private static int CompareByHierarchyLevel(ObjectData _data1, ObjectData _data2) {
		return _data1.hierarchyLevel.CompareTo(_data2.hierarchyLevel);
	}

	//------------------------------------------------------------------------//
	// MENU METHODS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Enable/Disable menu option.
	/// </summary>
	[MenuItem(MENU_PATH)]
	private static void ToggleEnabled() {
		// Reverse value
		ToggleEnabled(!ENABLED);
	}

	/// <summary>
	/// Enable/Disable menu option with parameter.
	/// </summary>
	/// <param name="_enable">Whether to enable or disable the picker.</param>
	private static void ToggleEnabled(bool _enable) {
		// Unsubscribe from scene GUI delegate
		SceneView.onSceneGUIDelegate -= OnSceneGUI;

		// If enabling, subscribe to scene GUI delegate
		if(_enable) {
			SceneView.onSceneGUIDelegate += OnSceneGUI;
		}

		// Store new value
		ENABLED = _enable;

		// Add/Remove checkmark to the menu option
		Menu.SetChecked(MENU_PATH, _enable);
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// An item in the contextual menu has been clicked.
	/// </summary>
	/// <param name="_obj">The selected item.</param>
	private static void OnMenuItemClick(object _obj) {
		// Cast
		GameObject go = _obj as GameObject;

		// Make it the selected item
		Selection.activeObject = go;

		// Ping it on hierarchy
		EditorGUIUtility.PingObject(go);
	}
}