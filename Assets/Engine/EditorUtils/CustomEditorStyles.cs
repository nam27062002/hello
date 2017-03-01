// CustomEditorStyles.cs
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
				s_commentLabelLeft = new GUIStyle(GUI.skin.label);
				s_commentLabelLeft.fontStyle = FontStyle.Italic;
				s_commentLabelLeft.normal.textColor = Colors.gray;
				s_commentLabelLeft.wordWrap = true;
				s_commentLabelLeft.richText = true;
			}
			return s_commentLabelLeft;
		}
	}

	private static GUIStyle s_bigSceneLabel = null;
	public static GUIStyle bigSceneLabel {
		get {
			if(s_bigSceneLabel == null) {
				s_bigSceneLabel = new GUIStyle(GUI.skin.label);
				s_bigSceneLabel.normal.textColor = Colors.WithAlpha(Colors.white, 0.5f);
				s_bigSceneLabel.fontSize = 20;
				s_bigSceneLabel.fontStyle = FontStyle.Bold;
			}
			return s_bigSceneLabel;
		}
	}

	private static GUIStyle s_richLabel = null;
	public static GUIStyle richLabel {
		get {
			if(s_richLabel == null) {
				s_richLabel = new GUIStyle(GUI.skin.label);
				s_richLabel.richText = true;
			}
			return s_richLabel;
		}
	}
}