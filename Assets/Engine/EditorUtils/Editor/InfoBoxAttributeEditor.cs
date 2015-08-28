// InfoBoxAttributeEditor.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 28/08/2015.
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
/// Drawer for the InfoBox custom attribute.
/// </summary>
[CustomPropertyDrawer(typeof(InfoBoxAttribute))]
public class InfoBoxAttributeEditor : DecoratorDrawer {
	//------------------------------------------------------------------//
	// CONSTANTS														//
	//------------------------------------------------------------------//
	private static readonly int MARGIN = 5;	// Top/bottom margins

	//------------------------------------------------------------------//
	// MEMBERS															//
	//------------------------------------------------------------------//
	private GUIStyle m_boxStyle = new GUIStyle(GUI.skin.GetStyle("HelpBox"));
	private GUIContent m_content = new GUIContent();

	//------------------------------------------------------------------//
	// PROPERTIES														//
	//------------------------------------------------------------------//
	private InfoBoxAttribute infoBox {
		get { return attribute as InfoBoxAttribute; }
	}

	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Default constructor.
	/// </summary>
	public InfoBoxAttributeEditor() {
		// Initialize box style
		m_boxStyle.margin = new RectOffset(50, 50, 50, 50);
		m_boxStyle.padding = new RectOffset(5, 5, 5, 5);
	}

	/// <summary>
	/// Draw the decorator in the inspector.
	/// </summary>
	/// <param name="_area">The area designated by the inspector to draw this decoration.</param>
	public override void OnGUI(Rect _area) {
		// Add margins top and bot
		_area.y += MARGIN;
		_area.height -= MARGIN * 2f;
		//GUI.Box(_area, m_content, m_boxStyle);
		EditorGUI.HelpBox(_area, m_content.text, MessageType.Info);
	}

	/// <summary>
	/// COmpute the required height to draw this attribute.
	/// </summary>
	/// <returns>The height.</returns>
	public override float GetHeight() {
		// [AOC] Compute required height to draw the text using our custom box style with the current inspector window width
		m_content.text = infoBox.m_text;
		float requiredHeight = m_boxStyle.CalcHeight(m_content, Screen.width - 35f);	// Screen.width gives us the size of the current inspector window. Unfortunately it doesn't compute the internal margins of the window, so try to compensate with a hardcoded value :P

		// Add margins and return
		requiredHeight += MARGIN * 2f;	// Top and bot
		return requiredHeight;
	}
}

