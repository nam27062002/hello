// SeparatorAttributeEditor.cs
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
/// Drawer for the Separator custom attribute.
/// </summary>
[CustomPropertyDrawer(typeof(SeparatorAttribute))]
public class SeparatorAttributeEditor : DecoratorDrawer {
	//------------------------------------------------------------------//
	// CONSTANTS														//
	//------------------------------------------------------------------//

	//------------------------------------------------------------------//
	// PROPERTIES														//
	//------------------------------------------------------------------//
	private SeparatorAttribute separator {
		get { return attribute as SeparatorAttribute; }
	}

	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Draw the decorator in the inspector.
	/// </summary>
	/// <param name="_area">The area designated by the inspector to draw this decoration.</param>
	public override void OnGUI(Rect _area) {
		// Use static method
		EditorUtils.Separator(_area, separator.m_title, separator.m_size);
	}

	/// <summary>
	/// Gets the height of the decorator drawer.
	/// </summary>
	/// <returns>The height required by this decorator drawer.</returns>
	public override float GetHeight() {
		// Very short
		return separator.m_size;
	}
}

