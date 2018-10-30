// UISafeAreaEditor.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 05/12/2017.
// Copyright (c) 2017 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;
using UnityEditor;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// Custom editor for the UISafeArea class.
/// </summary>
[CustomPropertyDrawer(typeof(UISafeArea))]
public class UISafeAreaPropertyDrawer : PropertyDrawer {
	//------------------------------------------------------------------//
	// MEMBERS															//
	//------------------------------------------------------------------//

	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Default constructor.
	/// </summary>
	public UISafeAreaPropertyDrawer() {
		// Nothing to do
	}

	//------------------------------------------------------------------//
	// PARENT IMPLEMENTATION											//
	//------------------------------------------------------------------//
	/// <summary>
	/// A change has occurred in the inspector, draw the property and store new values.
	/// </summary>
	/// <param name="_position">The area in the inspector assigned to draw this property.</param>
	/// <param name="_property">The property we're drawing.</param>
	/// <param name="_label">The label of the property.</param>
	public override void OnGUI(Rect _position, SerializedProperty _property, GUIContent _label) {
		// Group all
		_label = EditorGUI.BeginProperty(_position, _label, _property);
		EditorGUI.BeginChangeCheck(); {
			// Draw label and initialize position helpers
			Rect contentRect = EditorGUI.PrefixLabel(_position, _label);	// Remaining space for the content
			Rect cursor = contentRect;
			float fieldWidth = 40f;

			// Reset indentation level since we're already drawing within the content rect
			int indentLevelBackup = EditorGUI.indentLevel;
			EditorGUI.indentLevel = 0;

			// Get relevant properties (could be moved to OnEnable)
			SerializedProperty leftProp = _property.FindPropertyRelative("left");
			SerializedProperty rightProp = _property.FindPropertyRelative("right");
			SerializedProperty bottomProp = _property.FindPropertyRelative("bottom");
			SerializedProperty topProp = _property.FindPropertyRelative("top");

			// Draw them!
			//        TOP
			// LEFT          RIGHT
			//       BOTTOM
			cursor.width = fieldWidth;
			cursor.height = contentRect.height / 3f;	// 3 rows

			// Background square
			float bgMargin = EditorGUIUtility.singleLineHeight/2f;
			Rect bgRect = contentRect;
			bgRect.y += bgMargin;
			bgRect.height -= bgMargin * 2f;
			bgRect.x += cursor.width/2f;;
			bgRect.width -= cursor.width;
			EditorGUI.DrawRect(bgRect, Color.gray);

			float lineThickness = 2f;
			bgRect.y += lineThickness;
			bgRect.height -= lineThickness * 2f;
			bgRect.x += lineThickness;
			bgRect.width -= lineThickness * 2f;
			EditorGUI.DrawRect(bgRect, Color.black);

			// Left
			cursor.y += cursor.height;
			cursor.x = contentRect.x;
			EditorGUI.PropertyField(cursor, leftProp, GUIContent.none);

			// Top
			cursor.y -= cursor.height;
			cursor.x = contentRect.center.x - cursor.width/2f;
			EditorGUI.PropertyField(cursor, topProp, GUIContent.none);
			
			// Right
			cursor.y += cursor.height;
			cursor.x = contentRect.xMax - cursor.width;
			EditorGUI.PropertyField(cursor, rightProp, GUIContent.none);

			// Bottom
			cursor.y += cursor.height;
			cursor.x = contentRect.center.x - cursor.width/2f;
			EditorGUI.PropertyField(cursor, bottomProp, GUIContent.none);

			// Restore indentation and other stuff
			EditorGUI.indentLevel = indentLevelBackup;
		}

		// DONE!! End the property group
		EditorGUI.EndProperty();
	}

	/// <summary>
	/// Gets the height of the property drawer.
	/// </summary>
	/// <returns>The height required by this property drawer.</returns>
	/// <param name="_property">The property we're drawing.</param>
	/// <param name="_label">The label of the property.</param>
	override public sealed float GetPropertyHeight(SerializedProperty _property, GUIContent _label) {
		// 3 rows height:
		//        TOP
		// LEFT          RIGHT
		//       BOTTOM
		//return EditorGUI.GetPropertyHeight(_property);
		return EditorGUIUtility.singleLineHeight * 3f;
	}
}