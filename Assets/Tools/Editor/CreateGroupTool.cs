// CreateGroupTool.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 09/02/2016.
// Copyright (c) 2016 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using UnityEditor;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Simple tool to group several objects into a common parent.
/// </summary>
public class CreateGroupTool {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	private const string DEFAULT_GROUP_NAME = "NewGroup";

	//------------------------------------------------------------------------//
	// STATIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Context menu addition to create a new group on the selected objects.
	/// See https://github.com/tenpn/unity3d-ui/blob/1f5289211cc97e3feb63d5f847141258127386ff/UnityEditor.UI/UI/MenuOptions.cs
	/// </summary>
	/// <param name="_command">The command that triggered the callback.</param>
	[MenuItem("GameObject/Create Group", false, 20)]	// http://docs.unity3d.com/ScriptReference/MenuItem.html
	private static GameObject CreateGroup(MenuCommand _command) {
		// Make sure we have at least one game object selected
		if(Selection.gameObjects.Length == 0) {
			EditorUtility.DisplayDialog("ERROR!", "You must have at least one object selected.", "Ok");
			return null;
		}

		// This is called once for every selected object at the moment of clicking the context menu.
		// Since we do everything on the first call, ignore the rest of calls
		if(Selection.gameObjects.Length == 1 && Selection.activeGameObject.name == DEFAULT_GROUP_NAME) {
			return null;
		}

		// TODO!! Undo management

		// Create new group object
		GameObject newGroupObj = EditorUtils.CreateGameObject(DEFAULT_GROUP_NAME, null, false);

		// Compute position of the new parent (mid point of target objects)
		Vector3 groupPos = Vector3.zero;
		foreach(GameObject go in Selection.gameObjects) {
			groupPos += go.transform.position;
		}
		groupPos = groupPos/(float)Selection.gameObjects.Length;
		newGroupObj.transform.position = groupPos;

		// Reparent all selected objects
		foreach(GameObject go in Selection.gameObjects) {
			Undo.SetTransformParent(go.transform, newGroupObj.transform, "CreateGroupTool." + go.name);
		}

		// Done! Ping new object and return
		EditorUtils.FocusObject(newGroupObj);
		return newGroupObj;
	}
}