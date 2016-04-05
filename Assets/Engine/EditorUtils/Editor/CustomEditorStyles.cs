// CustomEditorStyles.cs
// 
// Created by Alger Ortín Castellví on 05/04/2016.
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
/// Custom editor styles!
/// Add them all here to be able to reuse them!
/// Partial class to be able to be extended by any custom styles used in project's own editors.
/// </summary>
public static partial class CustomEditorStyles {
	//------------------------------------------------------------------------//
	// STATICS																  //
	//------------------------------------------------------------------------//
	private static GUIStyle s_commentLabelLeft = null;
	public static GUIStyle commentLabelLeft {
		get {
			if(s_commentLabelLeft == null) {
				s_commentLabelLeft = new GUIStyle(EditorStyles.label);
				s_commentLabelLeft.fontStyle = FontStyle.Italic;
				s_commentLabelLeft.normal.textColor = Colors.gray;
				s_commentLabelLeft.wordWrap = true;
			}
			return s_commentLabelLeft;
		}
	}

	private static GUIStyle s_bigSceneLabel = null;
	public static GUIStyle bigSceneLabel {
		get {
			if(s_bigSceneLabel == null) {
				s_bigSceneLabel = new GUIStyle(EditorStyles.boldLabel);
				s_bigSceneLabel.normal.textColor = Colors.WithAlpha(Colors.white, 0.5f);
				s_bigSceneLabel.fontSize = 20;
			}
			return s_bigSceneLabel;
		}
	}
}