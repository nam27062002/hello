// CustomEditorStyles.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 05/04/2016.
// Copyright (c) 2016 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Custom editor styles!
/// Add them all here to be able to reuse them!
/// </summary>
public static partial class CustomEditorStyles {
	//------------------------------------------------------------------------//
	// STATICS																  //
	//------------------------------------------------------------------------//
	private static GUIStyle s_wrappedTextAreaStyle = null;
	public static GUIStyle wrappedTextAreaStyle {
		get {
			if(s_wrappedTextAreaStyle == null) {
				s_wrappedTextAreaStyle = new GUIStyle(GUI.skin.textArea);
				s_wrappedTextAreaStyle.wordWrap = true;
			}
			return s_wrappedTextAreaStyle;
		}
	}
}