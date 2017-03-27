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

	// Internal
	private bool m_initialFocusPending = false;

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
	/// Window has been opened.
	/// </summary>
	private void OnEnable() {
		m_initialFocusPending = true;
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
		GUI.SetNextControlName("InputTextfield");
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
				OnCancelButton();
			}

			// Ok
			if(GUILayout.Button("Ok")) {
				OnSumbitButton();
			}
		} EditorGUILayout.EndHorizontal();

		// Vertically centered
		GUILayout.FlexibleSpace();

		// If initial focus is pending, do it now
		if(m_initialFocusPending) {
			EditorGUI.FocusTextInControl("InputTextfield");
			m_initialFocusPending = false;
		}

		// Capture Enter and Escape keys for fast submit/cancel
		switch(Event.current.type) {
			case EventType.keyDown: {
				switch(Event.current.keyCode) {
					case KeyCode.Return:
					case KeyCode.KeypadEnter: {
						OnSumbitButton();	
					} break;

					case KeyCode.Escape: {
						OnCancelButton();
					} break;
				}
			} break;
		}
	}

	//------------------------------------------------------------------//
	// CALLBACKS														//
	//------------------------------------------------------------------//
	/// <summary>
	/// The cancel button has been pressed.
	/// </summary>
	private void OnCancelButton() {
		// Invoke external callbacks
		if(OnCancel != null) OnCancel();

		// Close the dialog
		Close();
		GUIUtility.ExitGUI();	// Interrupt OnGUI properly
	}

	/// <summary>
	/// The submit button has been pressed.
	/// </summary>
	private void OnSumbitButton() {
		// Invoke external callbacks
		if(OnAccept != null) OnAccept(m_text);

		// Close the dialog
		Close();
		GUIUtility.ExitGUI();	// Interrupt OnGUI properly
	}
}
