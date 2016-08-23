// SectionGroups.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 05/11/2015.
// Copyright (c) 2015 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;
using UnityEditor;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
namespace LevelEditor {
	/// <summary>
	/// 
	/// </summary>
	public class SectionGroups : ILevelEditorSection {
		//------------------------------------------------------------------//
		// CONSTANTS														//
		//------------------------------------------------------------------//
		private static readonly float LIST_HEIGHT = 130f;

		//------------------------------------------------------------------//
		// MEMBERS															//
		//------------------------------------------------------------------//
		private Group m_selectedGroup = null;
		public Group selectedGroup { get { return m_selectedGroup; }}
		
		private Vector2 m_scrollPos = Vector2.zero;

		//------------------------------------------------------------------//
		// INTERFACE IMPLEMENTATION											//
		//------------------------------------------------------------------//
		/// <summary>
		/// Initialize this section.
		/// </summary>
		public void Init() {

		}
		
		/// <summary>
		/// Draw the section.
		/// </summary>
		public void OnGUI() {
			// Group everything within a box
			EditorGUILayout.BeginVertical(LevelEditorWindow.styles.boxStyle); {
				// Title
				GUI.skin.label.alignment = TextAnchor.UpperCenter;
				GUILayout.Label("Groups");
				GUI.skin.label.alignment = TextAnchor.UpperLeft;
				
				// Aux vars
				Group[] groups = LevelEditorWindow.sectionLevels.activeLevel.GetComponentsInChildren<Group>();
					
				// Spacing
				GUILayout.Space(2);
						
				// Buttons Toolbar
				EditorGUILayout.BeginHorizontal(); {
					if(GUILayout.Button("New")) {
						// An external window will manage it
						AddGroupWindow.Show(LevelEditorWindow.sectionLevels.activeLevel, OnGroupCreated);
					}
					
					GUI.enabled = (m_selectedGroup != null);
					if(GUILayout.Button("Delete")) {
						// Remove from the local groups list!
						ArrayUtility.Remove<Group>(ref groups, m_selectedGroup);
						
						// Now we can remove it from the scene and delete it
						Undo.DestroyObjectImmediate(m_selectedGroup.gameObject);	// Make it undoable!
						m_selectedGroup = null;
					}
					
					if(GUILayout.Button("Duplicate")) {
						// Create the copy
						GameObject sourceObj = m_selectedGroup.gameObject;
						GameObject copyObj = GameObject.Instantiate<GameObject>(sourceObj);
						copyObj.name = sourceObj.name;
						copyObj.transform.SetParent(sourceObj.transform.parent, false);
						EditorUtils.SetObjectIcon(copyObj, EditorUtils.ObjectIcon.LABEL_GRAY);
						
						// Move a bit aside so we can see that it was actually duplicated
						copyObj.transform.SetPosX(copyObj.transform.position.x + 10f);
						copyObj.transform.SetPosY(copyObj.transform.position.y + 10f);
						
						// Make operation undoable
						Undo.RegisterCreatedObjectUndo(copyObj, "LevelEditor DuplicateGroup");
						
						// Focus the duplicate
						m_selectedGroup = copyObj.GetComponent<Group>();
						EditorUtils.FocusObject(copyObj);
					}
					GUI.enabled = true;
				} EditorGUILayoutExt.EndHorizontalSafe();
						
				// Separator
				EditorGUILayoutExt.Separator(new SeparatorAttribute(2f));
						
				// Scroll list
				m_scrollPos = EditorGUILayout.BeginScrollView(m_scrollPos, LevelEditorWindow.styles.whiteScrollListStyle, GUILayout.Height(LIST_HEIGHT)); {
					// Generate labels and found selected index
					int selectedIdx = -1;
					string[] labels = new string[groups.Length];
					for(int i = 0; i < groups.Length; i++) {
						labels[i] = groups[i].gameObject.name;
						if(groups[i] == m_selectedGroup) {
							selectedIdx = i;
						}
					}

					// Compute how many rows we can fit and distribute all items in columns
					float height = LIST_HEIGHT - 30f;	// Approx size of margins and deco stuff
					float rows = Mathf.Floor(height/LevelEditorWindow.styles.groupListStyle.CalcSize(new GUIContent("sample label")).y);
					int cols = Mathf.CeilToInt((float)(labels.Length)/rows);
					
					// Detect selection change
					GUI.changed = false;
					selectedIdx = GUILayout.SelectionGrid(selectedIdx, labels, cols, LevelEditorWindow.styles.groupListStyle);
					if(GUI.changed) {
						// Focus new selected group
						m_selectedGroup = groups[selectedIdx];
						EditorUtils.FocusObject(m_selectedGroup.gameObject);
					}
				} EditorGUILayoutExt.EndScrollViewSafe();
			} EditorGUILayoutExt.EndVerticalSafe();
		}

		//------------------------------------------------------------------//
		// CALLBACKS														//
		//------------------------------------------------------------------//
		/// <summary>
		/// Delegate for when a group was created from the AddGroupWindow window.
		/// </summary>
		/// <param name="_newGroup">The group that was just created.</param>
		private void OnGroupCreated(Group _newGroup) {
			// Make it the selected one
			m_selectedGroup = _newGroup;
		}
	}
}