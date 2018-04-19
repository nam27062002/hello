// PopupCustomizerFieldEditor.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 17/04/2018.
// Copyright (c) 2018 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Custom property drawer for the Field class in the PopupCustomizer.
/// </summary>
[CustomPropertyDrawer(typeof(PopupCustomizer.Field))]
public class PopupCustomizerFieldEditor : PropertyDrawer {
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
	/// A change has occurred in the inspector, draw the property and store new values.
	/// </summary>
	/// <param name="_position">The area in the inspector assigned to draw this property.</param>
	/// <param name="_property">The property we're drawing.</param>
	/// <param name="_label">The label of the property.</param>
	public override void OnGUI(Rect _position, SerializedProperty _property, GUIContent _label) {
		// Reset indent level
		int indentBackup = EditorGUI.indentLevel;
		EditorGUI.indentLevel = 0;

		// Aux vars
		Rect r = _position;

		// No prefix label (it's meant to go into an array)
		EditorGUIUtility.labelWidth = 0f;

		// Type
		r.width = 60f;	// Enough to display type labels
		SerializedProperty typeProp = _property.FindPropertyRelative("type");
		EditorGUI.PropertyField(r, typeProp, GUIContent.none, true);

		// Target
		// Filter object selector by type :)
		r.x += r.width;
		r.width = _position.width - r.width;
		SerializedProperty targetProp = _property.FindPropertyRelative("target");
		switch((PopupCustomizer.Field.Type)typeProp.enumValueIndex) {
			case PopupCustomizer.Field.Type.TEXT: {
				targetProp.objectReferenceValue = EditorGUI.ObjectField(
					r, GUIContent.none, 
					targetProp.objectReferenceValue as TextMeshProUGUI, 
					typeof(TextMeshProUGUI)
				);
			} break;

			case PopupCustomizer.Field.Type.IMAGE: {
				targetProp.objectReferenceValue = EditorGUI.ObjectField(
					r, GUIContent.none, 
					targetProp.objectReferenceValue as Image, 
					typeof(Image)
				);
			} break;

			case PopupCustomizer.Field.Type.BUTTON:	{
				targetProp.objectReferenceValue = EditorGUI.ObjectField(
					r, GUIContent.none, 
					targetProp.objectReferenceValue as Button, 
					typeof(Button)
				);
			} break;
		}

		// Restore indent level
		EditorGUI.indentLevel = indentBackup;
	}

	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
}