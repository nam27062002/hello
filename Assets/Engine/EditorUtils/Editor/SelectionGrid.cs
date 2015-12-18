// SelectionGridEditor.cs
// 
// Created by Alger Ortín Castellví on 06/11/2015.
// Copyright (c) 2015 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// 
/// </summary>
public class SelectionGrid {
	//------------------------------------------------------------------//
	// CONSTANTS														//
	//------------------------------------------------------------------//
	/// <summary>
	/// Single group within the selection grid.
	/// </summary>
	public class Group {
		// Data
		public string m_name = "";	// Name of the group (name to be displayed)
		public GUIContent[] m_contents = new GUIContent[0];	// Contents to be displayed in the grid under this group
		public Object[] m_data = new Object[0];	// Optional field linking each content to a custom object

		// Aux
		public bool m_unfolded = true;	// Whether the group is folded or not
	};
	
	//------------------------------------------------------------------//
	// MEMBERS															//
	//------------------------------------------------------------------//
	// Data
	public Dictionary<string, Group> m_groups = new Dictionary<string, Group>();
	public Group[] groups {
		get {
			Group[] groupArray = new Group[m_groups.Count];
			int i = 0;
			foreach(KeyValuePair<string, Group> g in m_groups) {
				groupArray[i] = g.Value;
				i++;
			}
			return groupArray;
		}
	}

	// Selection control
	public string m_selectedGroupId = "";
	public int m_selectedIdx = 0;	// Local index within the group

	// Setup
	private Color m_selectionColor = Colors.skyBlue;
	public float m_thumbSize = 100f;	// Desired thumb size, will be auto-adjusted to optimally fit the display area.

	// Internal
	private GUIStyle m_buttonStyle = null;	// GUIStyles can only be created during OnGUI calls
	private Vector2 m_scrollPos = Vector2.zero;
	private Rect m_gridArea = new Rect();	// The size of the area available for the grid can only be computed during the repaint event, so store it for the other events

	//------------------------------------------------------------------//
	// PROPERTIES														//
	//------------------------------------------------------------------//
	public Group selectedGroup {
		get {
			return GetGroup(m_selectedGroupId, false);
		}
	}

	public GUIContent selectedContent {
		get {
			Group g = selectedGroup;
			if(g != null) {
				if(g.m_contents != null && m_selectedIdx >= 0 && m_selectedIdx < g.m_contents.Length) {
					return g.m_contents[m_selectedIdx];
				}
			}
			return null;
		}
	}

	public Object selectedObject {
		get {
			Group g = selectedGroup;
			if(g != null) {
				if(g.m_data != null && m_selectedIdx >= 0 && m_selectedIdx < g.m_data.Length) {
					return g.m_data[m_selectedIdx];
				}
			}
			return null;
		}
	}

	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Look for the group with the given id and optionally create a new one if not found and add it to the list.
	/// </summary>
	/// <returns>If found, the group with the given key, or the newly created one if <paramref name="_createNewIfNotFound"/> was true. Null otherwise.</returns>
	/// <param name="_id">The id of the group.</param>
	/// <param name="_createNewIfNotFound">If set to <c>true</c> create a new group and add it to the list if a group with the given id was not found.</param>
	public Group GetGroup(string _id, bool _createNewIfNotFound) {
		Group gr = null;
		if(m_groups.TryGetValue(_id, out gr)) {
			// Group found!
			return gr;
		} else if(_createNewIfNotFound) {
			// Not found! Creating new one
			gr = new SelectionGrid.Group();
			m_groups.Add(_id, gr);
			return gr;
		}
		return null;
	}

	/// <summary>
	/// Draw the grid.
	/// </summary>
	public void OnGUI() {
		// Make sure styles are properly initialized
		InitStyles();

		// Content List
		EditorGUILayout.BeginVertical(EditorStyles.helpBox); {
			// Get current layout area size (before starting the scrollview!)
			// Rect can only be trusted during the repaint event
			Rect currentRect = GUILayoutUtility.GetRect(0, 0);
			bool changed = false;
			if(Event.current.type == EventType.Repaint) {
				changed = (currentRect.width != m_gridArea.width);
				m_gridArea = currentRect;
			}
			
			// Selector - we don't want horizontal scrollbar under any circumstance
			if(!changed) {
				m_scrollPos = EditorGUILayout.BeginScrollView(m_scrollPos, GUIStyle.none, GUI.skin.verticalScrollbar); {
					// Compute how many columns fit in the current layout rect to avoid horizontal scrolling
					// Skip if area is still not valid
					int cols = Mathf.FloorToInt(m_gridArea.width/m_thumbSize);
					if(cols > 0) {
						// Maximize thumb size to fill the area
						// Add some safety margin to account for the vertical scrollbar
						float thumbSize = (m_gridArea.width - 15f)/(float)cols;
						m_buttonStyle.fixedWidth = thumbSize;
						m_buttonStyle.fixedHeight = thumbSize;

						// Iterate groups and draw the grid for every group
						foreach(KeyValuePair<string, Group> g in m_groups) {
							// Foldable arrow + title
							GUIStyle labelStyle = new GUIStyle(EditorStyles.foldout);
							labelStyle.fontStyle = FontStyle.Bold;
							labelStyle.fontSize = 14;
							g.Value.m_unfolded = EditorGUILayout.Foldout(g.Value.m_unfolded, g.Value.m_name, labelStyle);

							// If unfolded, do the grid!
							if(g.Value.m_unfolded) {
								int contentIdx = 0;
								while(contentIdx < g.Value.m_contents.Length) {
									EditorGUILayout.BeginVertical(); {
										// New row
										EditorGUILayout.BeginHorizontal(); {
											// Draw row until we run out of space or contents
											for(int col = 0; col < cols && contentIdx < g.Value.m_contents.Length; col++) {
												// Draw current item as a toggle button
												// Selected one?
												bool selected = (g.Key == m_selectedGroupId) && (contentIdx == m_selectedIdx);
												if(GUILayout.Toggle(selected, g.Value.m_contents[contentIdx], m_buttonStyle)) {
													m_selectedGroupId = g.Key;
													m_selectedIdx = contentIdx;
												}
												contentIdx++;
											}
										} EditorGUILayoutExt.EndHorizontalSafe();
									} EditorGUILayoutExt.EndVerticalSafe();
								}
							}

							// Draw group separator
							EditorGUILayoutExt.Separator(new SeparatorAttribute(10f));
						}
					}
				} EditorGUILayoutExt.EndScrollViewSafe();
			}
		} EditorGUILayoutExt.EndVerticalSafe();
	}

	/// <summary>
	/// Define the background color for the selected item.
	/// </summary>
	/// <param name="_color">The selection color.</param>
	public void SetSelectionColor(Color _color) {
		// If style was already created, update background texture
		m_selectionColor = _color;
		if(m_buttonStyle != null) {
			m_buttonStyle.onActive.background = Texture2DExt.Create(2, 2, m_selectionColor);
			m_buttonStyle.onNormal.background = Texture2DExt.Create(2, 2, m_selectionColor);
		}
	}

	//------------------------------------------------------------------//
	// INTERNAL METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Initialize custom GUIStyles.
	/// </summary>
	private void InitStyles() {
		// Button style - 100% custom
		if(m_buttonStyle == null) {
			m_buttonStyle = new GUIStyle();
			m_buttonStyle.fixedWidth = m_thumbSize;
			m_buttonStyle.fixedHeight = m_thumbSize;
			m_buttonStyle.imagePosition = ImagePosition.ImageAbove;
			m_buttonStyle.alignment = TextAnchor.MiddleCenter;
			m_buttonStyle.wordWrap = true;
			m_buttonStyle.padding = new RectOffset(5, 5, 5, 5);
			SetSelectionColor(m_selectionColor);
		}
	}
}