// ButtonExtendedEditor.cs
// 
// Created by Alger Ortín Castellví on 25/05/2016.
// Copyright (c) 2016 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Custom editor for the ButtonExtended class.
/// </summary>
[CustomEditor(typeof(ButtonExtended), true)]	// True to be used by heir classes as well
[CanEditMultipleObjects]
public class ButtonExtendedEditor : UnityEditor.UI.ButtonEditor {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//

	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// The editor has been enabled - target object selected.
	/// </summary>
	override protected void OnEnable() {
		// Call parent
		base.OnEnable();

		// Store a reference of interesting properties for faster access
		// [AOC] Nothing so far
	}

	/// <summary>
	/// The editor has been disabled - target object unselected.
	/// </summary>
	override protected void OnDisable() {
		// Call parent
		base.OnDisable();
	}

	/// <summary>
	/// Draw the inspector.
	/// </summary>
	override public void OnInspectorGUI() {
		// Call parent
		base.OnInspectorGUI();

		// Update the serialized object - always do this in the beginning of OnInspectorGUI.
		serializedObject.Update();

		// Draw any custom property
		// [AOC] Nothing for now

		// Apply changes to the serialized object - always do this in the end of OnInspectorGUI.
		serializedObject.ApplyModifiedProperties();
	}

	//------------------------------------------------------------------------//
	// STATIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Context menu addition to create a new Button Extended object.
	/// See https://github.com/tenpn/unity3d-ui/blob/1f5289211cc97e3feb63d5f847141258127386ff/UnityEditor.UI/UI/MenuOptions.cs
	/// </summary>
	/// <param name="_command">The command that triggered the callback.</param>
	[MenuItem("GameObject/UI/Button Extended", false, 10)]	// http://docs.unity3d.com/ScriptReference/MenuItem.html
	private static GameObject CreateButtonExtended(MenuCommand _command) {
		// Use our own EditorUtils!
		GameObject buttonObj = EditorUtils.CreateUIGameObject("ButtonExtended", EditorUtils.GetContextObject(_command));

		// Add components
		buttonObj.AddComponent<Image>();
		buttonObj.AddComponent<ButtonExtended>();

		// Done!
		return buttonObj;
	}
}