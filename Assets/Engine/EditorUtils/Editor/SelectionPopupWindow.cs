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
/// Suppports sections, use the SECTION keyword as an option to insert a spacing between two options (wont be selectable).
/// Use SECTION+text to insert a header between two options (e.g. options[i] = SelectionPopupWindow.SECTION + "MySectionName";)
/// </summary>
public class SelectionPopupWindow : EditorWindow {
	//------------------------------------------------------------------//
	// CONSTANTS														//
	//------------------------------------------------------------------//
	// Selection delegate, will be invoked when selection has changed
	public delegate void SelectionChangedHandler(int _selectedIdx);
	
	public static readonly string SECTION = "SECTION";

	//------------------------------------------------------------------//
	// MEMBERS															//
	//------------------------------------------------------------------//
	// Setup
	private string[] m_options;
	private SelectionChangedHandler m_selectionChangedHandler = null;

	// Styles
	private GUIStyle m_optionStyle = null;
	private GUIStyle m_sectionStyle = null;

	// Control
	private Vector2 m_scrollPos = Vector2.zero;

	// Internal
	private bool m_moveToCursorPending = false;
	private Rect m_initialPos = new Rect();
	private Vector2 m_initialSize = Vector2.zero;
	
	//------------------------------------------------------------------//
	// STATIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Show the window at cursor position with automatic sizing.
	/// </summary>
	/// <param name="_options">The list of options to be chosen. If an option starts with the SECTION keyword, it wont be selectable and it will be displayed with a different style.</param>
	/// <param name="_handler">The callback function to be invoked when a item has been selected.</param>
	public static SelectionPopupWindow Show(string[] _options, SelectionChangedHandler _handler) {
		// Open at cursor's Y, centered to current window in X
		// The window expects the position in screen coords
		Rect pos = new Rect();
		pos.width = Screen.width * 0.8f;
		pos.height = pos.width * 1.5f;	// Nice proportion
		pos.x = Screen.width/2f - pos.width/2f;
		// [AOC] Unfortunately, we can't obtain the mouse position outside the OnGUI call, so pospone this step
		//pos.y = Event.current.mousePosition.y + 7f;	// A little bit lower
		pos.y = Screen.height/2f - pos.height/2f;

		// Use the positioned opener
		SelectionPopupWindow window = Show(pos, _options, _handler);

		// Indicate that repositioning is pending
		window.m_moveToCursorPending = true;
		return window;
	}

	/// <summary>
	/// Show the window at a specific position.
	/// </summary>
	/// <param name="_pos">The position and size where to display the popup. In GUI local coords. If the content is smaller than the given height, the popup will adjust to it.</param>
	/// <param name="_options">The list of options to be chosen.</param>
	/// <param name="_handler">The callback function to be invoked when a item has been selected.</param> 
	public static SelectionPopupWindow Show(Rect _pos, string[] _options, SelectionChangedHandler _handler) {
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
		float contentHeight = (_options.Length + 1) * window.m_optionStyle.lineHeight + 10;	// Arbitrary margin
		size.y = Mathf.Min(size.y, contentHeight);
		
		// Show it as a dropdown list so window is automatically closed upon losing focus
		// http://docs.unity3d.com/ScriptReference/EditorWindow.ShowAsDropDown.html
		//window.ShowAsDropDown(_pos, size);
		// [AOC] If the window is opened from a custom inspector, opening it immediately will cause a layout exception. Use a delayed call instead.
		window.m_initialPos = _pos;
		window.m_initialSize = size;
		EditorApplication.delayCall += window.DelayedShow;

		return window;
	}

	/// <summary>
	/// Callback for the delayed opening of the window
	/// </summary>
	private void DelayedShow() {
		// Show it as a dropdown list so window is automatically closed upon losing focus
		// http://docs.unity3d.com/ScriptReference/EditorWindow.ShowAsDropDown.html
		ShowAsDropDown(m_initialPos, m_initialSize);
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
		// Margin to the left to show indented
		m_optionStyle.margin = new RectOffset(20, 0, 0, 0);
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

		// Section style
		m_sectionStyle = new GUIStyle(m_optionStyle);
		m_sectionStyle.fontStyle = FontStyle.Bold;
		m_sectionStyle.margin = new RectOffset(0, 0, 5, 0);
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
		// If a repositioning of the window was pending, do it now
		if(m_moveToCursorPending) {
			Vector2 mousePos = Event.current.mousePosition;
			mousePos.y += 7f;	// A little bit lower
			mousePos = EditorGUIUtility.GUIToScreenPoint(mousePos);

			Rect pos = this.position;
			pos.position = mousePos;
			this.position = pos;

			m_moveToCursorPending = false;
		}

		// Reset indentation
		int indentLevelBackup = EditorGUI.indentLevel;
		EditorGUI.indentLevel = 0;
		
		// Show all options in a list
		int selectedOption = -1;
		m_scrollPos = EditorGUILayout.BeginScrollView(m_scrollPos, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true)); {
			EditorGUILayout.BeginVertical(); {
				// Draw a selectable label for each given option
				for(int i = 0; i < m_options.Length; i++) {
					// If option is a section header, just draw a label
					if(m_options[i].StartsWith(SECTION)) {
						// Remove prefix
						GUILayout.Label(m_options[i].Replace(SECTION, ""), m_sectionStyle);
					} else {
						// Trick it by showing buttons with a label style
						if(GUILayout.Button(m_options[i], m_optionStyle)) {
							// Item selected! Store its index to invoke callback and close popup afterwards
							selectedOption = i;
						}
					}
				}
			} EditorGUILayoutExt.EndVerticalSafe();
		} EditorGUILayoutExt.EndScrollViewSafe();
		
		// Restore indentation
		EditorGUI.indentLevel = indentLevelBackup;

		// If required, invoke selection handler and close popup
		if(selectedOption >= 0) {
			if(m_selectionChangedHandler != null) m_selectionChangedHandler(selectedOption);
			Close();
		}
	}
}
