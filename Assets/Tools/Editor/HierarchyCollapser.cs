// HierarchyCollapser.cs
// 
// Created by Alger Ortín Castellví on 15/05/2017.
// Copyright (c) 2017 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;
using System.Reflection;
using System.Collections.Generic;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Auxiliar class to collapse/expand hierarchy elements.
/// </summary>
public class HierarchyCollapser {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	
	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	
	//------------------------------------------------------------------------//
	// STATIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Grabs de current Hierarchy window and collapses/expands all its root elements.
	/// </summary>
	/// <param name="_collapse">Whether to collapse or expand.</param>
	public static void CollapseHierarchy(bool _collapse) {
		// See:
		// https://forum.unity3d.com/threads/rel-collapse-all-gameobjects-recursively-in-hierarchy-view.298914/
		// https://github.com/MattRix/UnityDecompiled/blob/82e03c823811032fd970ffc9a75246e95c626502/UnityEngine/UnityEngine.SceneManagement/Scene.cs
		// https://github.com/MattRix/UnityDecompiled/blob/master/UnityEditor/UnityEditor/SceneHierarchyWindow.cs

		// Focus and grab current hierarchy window
		EditorApplication.ExecuteMenuItem("Window/Hierarchy");
		EditorWindow hierarchyWindow = EditorWindow.focusedWindow;

		// Reflection! Find the expand/collapse method as well as Scene's "handle" property
		// private void ExpandTreeViewItem(int id, bool expand)
		MethodInfo expandTreeViewItemMethodInfo = hierarchyWindow.GetType().GetMethod("ExpandTreeViewItem", BindingFlags.NonPublic | BindingFlags.Instance);
		FieldInfo handleFieldInfo = typeof(Scene).GetField("m_Handle", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);

		// Iterate through all loaded scenes
		for(int i = 0; i < SceneManager.sceneCount; i++) {
			// Reflection!
			int sceneHandle = (int)handleFieldInfo.GetValue(SceneManager.GetSceneAt(i));
			expandTreeViewItemMethodInfo.Invoke(hierarchyWindow, new object[] { sceneHandle, !_collapse });
		}
	}

	/// <summary>
	/// Collapse/expand a specific object in the hierarchy window.
	/// </summary>
	/// <param name="_obj">The target object. Ignored if object not valid or not in the hierarchy.</param>
	/// <param name="_collapse">Whether to collapse or expand.</param>
	public static void CollapseObjectInHierarchy(GameObject _obj, bool _collapse) {
		// Skip if object not valid
		if(_obj == null) return;

		// From https://forum.unity3d.com/threads/rel-collapse-all-gameobjects-recursively-in-hierarchy-view.298914/
		// Focus and grab current hierarchy window
		EditorApplication.ExecuteMenuItem("Window/Hierarchy");
		EditorWindow hierarchyWindow = EditorWindow.focusedWindow;

		// Find the expand/collapse method via reflection
		MethodInfo expandMethodInfo = hierarchyWindow.GetType().GetMethod("SetExpandedRecursive");

		// Execute the expand/collapse method in the target object
		expandMethodInfo.Invoke(hierarchyWindow, new object[] { _obj.GetInstanceID(), !_collapse });
	}

	//------------------------------------------------------------------------//
	// MENU ENTRIES															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Collapse all root objects in the hierarchy window.
	/// </summary>
	[MenuItem("Window/Collapse Hierarchy %&LEFT", false, 150)]
	public static void CollapseHierarchy() {
		CollapseHierarchy(true);
	}

	/// <summary>
	/// Expand all root objects in the hierarchy window.
	/// </summary>
	[MenuItem("Window/Expand Hierarchy %&RIGHT", false, 151)]
	public static void ExpandHierarchy() {
		CollapseHierarchy(false);
	}
}