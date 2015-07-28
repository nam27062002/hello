// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 13/05/2015.
// Copyright (c) 2015 Ubisoft. All rights reserved.

#region INCLUDES AND PREPROCESSOR --------------------------------------------------------------------------------------
using UnityEngine;
using UnityEditor;
using System.Collections;
#endregion

#region CLASSES --------------------------------------------------------------------------------------------------------
/// <summary>
/// Custom editor for the feedback spawner.
/// </summary>
[CustomEditor(typeof(UIFeedbackSpawner))]
public class UIFeedbackSpawnerEditor : Editor {
	#region INTERNAL MEMBERS -------------------------------------------------------------------------------------------
	#endregion

	#region METHODS ----------------------------------------------------------------------------------------------------
	/// <summary>
	/// Updates stuff on the inspector.
	/// </summary>
	public override void OnInspectorGUI() {
		// Make sure that all the data is up to date
		serializedObject.Update();

		// Start drawing:
		// Properties using default drawing
		EditorGUILayout.PrefixLabel(new GUIContent("Starving Message:"));
		EditorGUILayout.PropertyField(serializedObject.FindProperty("starvingMessage"));

		// Types setup vector container
		DrawList(serializedObject.FindProperty("typesSetup"));

		// Commit any changed done
		serializedObject.ApplyModifiedProperties();
	}

	private void DrawList(SerializedProperty _list) {
		// Make sure the list has exactly the amount of types we have defined
		_list.arraySize = System.Enum.GetValues(typeof(EUIFeedbackType)).Length;	// [AOC] Way to know the size of our enum

		// 1. Show the foldout
		EditorGUILayout.PropertyField(_list, false);	// [AOC] Don't display children yet, we will do it manually

		// 2. If unfolded, show the elements, skipping the size
		if(_list.isExpanded) {
			EditorGUI.indentLevel++;	// Indented :-P
			for(int i = 0; i < _list.arraySize; i++) {
				DrawListElement(_list.GetArrayElementAtIndex(i), i);
			}
			EditorGUI.indentLevel--;
		}
	}

	private void DrawListElement(SerializedProperty _element, int _iIndex) {
		// Make sure this element has the type corresponding to its position in the list
		SerializedProperty elementType = _element.FindPropertyRelative("type");
		elementType.enumValueIndex = _iIndex;

		// Draw element
		string sLabel = elementType.enumDisplayNames[_iIndex];
		EditorGUILayout.PropertyField(_element, new GUIContent(sLabel), true);
	}
	#endregion
}
#endregion