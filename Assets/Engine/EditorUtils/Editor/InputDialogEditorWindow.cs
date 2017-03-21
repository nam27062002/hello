// InputDialogEditorWindow.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 21/03/2017.
// Copyright (c) 2017 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;
using UnityEditor;
using System;

/// <summary>
/// Auxiliar dialog to input a text
/// </summary>
class InputDialog : EditorWindow {
	//------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES											//
	//------------------------------------------------------------------//
	/// Vars
	private string m_message = "";
	private string m_text = "";

	public Action<string> OnAccept = null;
	public Action OnCancel = null;
	public Func<string, string> OnValidate = null;

	//------------------------------------------------------------------//
	// METHODS															//
	//------------------------------------------------------------------//
	/// <summary>
	/// Opens the window.
	/// </summary>
	public static InputDialog ShowWindow(string _message, string _initialText) {
		// Get window!
		InputDialog window = EditorWindow.GetWindow<InputDialog>();

		// Init parameters
		window.m_message = _message;
		window.m_text = _initialText;

		// Set window size
		window.minSize = new Vector2(300f, 175f);
		window.maxSize = window.minSize;	// Not resizeable

		// Show!
		window.Show();

		return window;
	}

	/// <summary>
	/// 
	/// </summary>
	private void OnGUI() {
		// Vertically centered
		GUILayout.FlexibleSpace();

		// Message
		EditorGUILayout.LabelField(m_message);

		// Input text
		EditorGUI.BeginChangeCheck();
		m_text = EditorGUILayout.TextField(m_text);
		if(EditorGUI.EndChangeCheck()) {
			// If a validation function is defined, apply it now
			if(OnValidate != null) {
				m_text = OnValidate(m_text);
			}
		}

		// Buttons
		EditorGUILayout.BeginHorizontal(); {
			// Right aligned
			GUILayout.FlexibleSpace();

			// Cancel
			if(GUILayout.Button("Cancel")) {
				if(OnCancel != null) OnCancel();

				Close();
				GUIUtility.ExitGUI();	// ??
			}

			// Ok
			if(GUILayout.Button("Ok")) {
				if(OnAccept != null) OnAccept(m_text);

				Close();
				GUIUtility.ExitGUI();	// ??
			}
		} EditorGUILayout.EndHorizontal();

		// Vertically centered
		GUILayout.FlexibleSpace();
	}
}
