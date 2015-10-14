// SelectionPopupWindow.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 31/09/2015.
// Copyright (c) 2015 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;
using UnityEditor;

/// <summary>
/// Simple popup window to select from a list of options.
/// </summary>
public class SelectionPopupWindow : EditorWindow {
	//------------------------------------------------------------------//
	// CONSTANTS														//
	//------------------------------------------------------------------//
	// Selection delegate, will be invoked when selection has changed
	public delegate void SelectionChangedHandler(int _selectedIdx);
	
	//------------------------------------------------------------------//
	// MEMBERS															//
	//------------------------------------------------------------------//
	private string[] m_options;
	private SelectionChangedHandler m_selectionChangedHandler = null;
	private Vector2 m_scrollPos = Vector2.zero;
	private GUIStyle m_optionStyle = null;
	
	//------------------------------------------------------------------//
	// STATIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Show the window at cursor position with automatic sizing.
	/// </summary>
	/// <param name="_options">The list of options to be chosen.</param>
	/// <param name="_handler">The callback function to be invoked when a item has been selected.</param>
	public static void Show(string[] _options, SelectionChangedHandler _handler) {
		// Open at cursor's Y, centered to current window in X
		// The window expects the position in screen coords
		Rect pos = new Rect();
		pos.width = Screen.width * 0.8f;
		pos.height = pos.width * 1.5f;	// Nice proportion
		pos.x = Screen.width/2f - pos.width/2f;
		pos.y = Event.current.mousePosition.y + 7f;	// A little bit lower

		// Use the positioned opener
		Show(pos, _options, _handler);
	}

	/// <summary>
	/// Show the window at a specific position.
	/// </summary>
	/// <param name="_pos">The position and size where to display the popup. In GUI local coords. If the content is smaller than the given height, the popup will adjust to it.</param>
	/// <param name="_options">The list of options to be chosen.</param>
	/// <param name="_handler">The callback function to be invoked when a item has been selected.</param> 
	public static void Show(Rect _pos, string[] _options, SelectionChangedHandler _handler) {
		// Create a new window instance
		SelectionPopupWindow window = new SelectionPopupWindow();
		
		// Setup window
		window.m_options = _options;
		window.m_selectionChangedHandler = _handler;
		
		// The window expects the position in screen coords, while it is given to us in local editor coords
		_pos.position = EditorGUIUtility.GUIToScreenPoint(_pos.position);
		Vector2 size = _pos.size;
		_pos.size = Vector2.zero;	// This would add an offset from position to actually start drawing the popup
		
		// If content is smaller than given size, adjust to it
		float contentHeight = (_options.Length + 1) * window.m_optionStyle.lineHeight;
		size.y = Mathf.Min(size.y, contentHeight);
		
		// Show it as a dropdown list so window is automatically closed upon losing focus
		// http://docs.unity3d.com/ScriptReference/EditorWindow.ShowAsDropDown.html
		window.ShowAsDropDown(_pos, size);
	}
	
	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Default constructor.
	/// </summary>
	public SelectionPopupWindow() {
		// Initialize custom style - based on labels
		m_optionStyle = new GUIStyle(EditorStyles.largeLabel);
		
		// No margins
		m_optionStyle.margin = new RectOffset();
		m_optionStyle.border = new RectOffset();
		
		// Hover color
		Texture2D hoverTex = Texture2DExt.Create(2, 2, Colors.gray);
		m_optionStyle.onHover.background = hoverTex;
		m_optionStyle.hover.background = hoverTex;

		// Active color
		Texture2D activeTex = Texture2DExt.Create(2, 2, Colors.darkGray);
		m_optionStyle.onActive.background = activeTex;
		m_optionStyle.active.background = activeTex;
		m_optionStyle.onActive.textColor = Colors.white;
		m_optionStyle.active.textColor = Colors.white;
	}
	
	/// <summary>
	/// Called every frame.
	/// </summary>
	private void Update() {
		// [AOC] For some reason, Unity have some delay before detecting that the mouse is hover a button and updating its visuals
		//		 This is an ugly workaround to avoid that, basically forcing a repaint every frame while the mouse is over this window
		//		 Based on http://forum.unity3d.com/threads/gui-button-hover-change-text-color-solved.262440/
		// Is the mouse over this window?
		if(mouseOverWindow == this) {
			Repaint();
		}
	}

	//------------------------------------------------------------------//
	// WINDOW METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Update the inspector window.
	/// </summary>
	public void OnGUI() {
		// Reset indentation
		int indentLevelBackup = EditorGUI.indentLevel;
		EditorGUI.indentLevel = 0;
		
		// Show all options in a list
		m_scrollPos = EditorGUILayout.BeginScrollView(m_scrollPos, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true)); {
			EditorGUILayout.BeginVertical(); {
				// Draw a selectable label for each given option
				for(int i = 0; i < m_options.Length; i++) {
					// Trick it by showing buttons with a label style
					if(GUILayout.Button(m_options[i], m_optionStyle)) {
						// Item selected! Invoke callback, break loop and close popup
						if(m_selectionChangedHandler != null) m_selectionChangedHandler(i);
						Close();
						break;
					}
				}
			} EditorGUILayout.EndVertical();
		} EditorGUILayout.EndScrollView();
		
		// Restore indentation
		EditorGUI.indentLevel = indentLevelBackup;
	}
}
