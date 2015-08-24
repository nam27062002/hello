// UIFeedbackSpawnerEditor.cs
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
/// Custom editor for the feedback message.
/// </summary>
[CustomPropertyDrawer(typeof(UIFeedbackMessage))]
public class UIFeedbackMessageDrawer : PropertyDrawer {
	/// <summary>
	/// Draw the property inside the given rect
	/// </summary>
	/// <param name="_pos">Rectangle on the screen to use for the property GUI.</param>
	/// <param name="_property">The SerializedProperty to make the custom GUI for.</param>
	/// <param name="_label">The label of this property.</param>
	public override void OnGUI(Rect _pos, SerializedProperty _property, GUIContent _label) {
		// Using BeginProperty / EndProperty on the parent property means that prefab override logic works on the entire property.
		EditorGUI.BeginProperty(_pos, _label, _property);
		
		// Draw label - empty
		_label.text = " ";
		_pos = EditorGUI.PrefixLabel(_pos, GUIUtility.GetControlID(FocusType.Passive), _label);
		
		// Don't indent child fields, since they're at the same line than the label
		int indentLvl = EditorGUI.indentLevel;
		EditorGUI.indentLevel = 0;
		
		// Draw textbox
		Rect textPos = new Rect(_pos.x, _pos.y, _pos.width * 0.6f, _pos.height);
		EditorGUI.PropertyField(textPos, _property.FindPropertyRelative("text"), GUIContent.none);	// GUIContent.none so it's drawn without labels
		
		// Draw type selector
		float fMargin = 2;
		Rect typePos = new Rect(_pos.x + textPos.width + fMargin, _pos.y, _pos.width - textPos.width - fMargin, _pos.height);
		EditorGUI.PropertyField(typePos, _property.FindPropertyRelative("type"), GUIContent.none);	// GUIContent.none so it's drawn without labels
		
		// Restore indentation level
		EditorGUI.indentLevel = indentLvl;
		
		// End property drawing
		EditorGUI.EndProperty();
	}
}
#endregion