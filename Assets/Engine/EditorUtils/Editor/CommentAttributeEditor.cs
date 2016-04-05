// CommentAttributeEditor.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 28/10/2015.
// Copyright (c) 2015 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;
using UnityEditor;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// Drawer for the Comment custom attribute.
/// </summary>
[CustomPropertyDrawer(typeof(CommentAttribute))]
public class CommentAttributeEditor : DecoratorDrawer {
	//------------------------------------------------------------------//
	// CONSTANTS														//
	//------------------------------------------------------------------//

	//------------------------------------------------------------------//
	// MEMBERS															//
	//------------------------------------------------------------------//
	private GUIContent m_content = new GUIContent();

	//------------------------------------------------------------------//
	// PROPERTIES														//
	//------------------------------------------------------------------//
	private CommentAttribute comment {
		get { return attribute as CommentAttribute; }
	}

	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Draw the decorator in the inspector.
	/// </summary>
	/// <param name="_area">The area designated by the inspector to draw this decoration.</param>
	public override void OnGUI(Rect _area) {
		// Apply spacing
		_area.y += comment.m_spaceAbove;

		// Draw label
		EditorGUI.LabelField(_area, m_content.text, CustomEditorStyles.commentLabelLeft);
	}

	/// <summary>
	/// COmpute the required height to draw this attribute.
	/// </summary>
	/// <returns>The height.</returns>
	public override float GetHeight() {
		// [AOC] Compute required height to draw the text using our custom box style with the current inspector window width
		m_content.text = comment.m_text;
		float requiredHeight = CustomEditorStyles.commentLabelLeft.CalcHeight(m_content, Screen.width - 35f);	// Screen.width gives us the size of the current inspector window. Unfortunately it doesn't compute the internal margins of the window, so try to compensate with a hardcoded value :P

		// Add spacing, if positive
		if(comment.m_spaceAbove > 0f) {
			requiredHeight += comment.m_spaceAbove;
		}

		return requiredHeight;
	}
}

