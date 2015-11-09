// AddGroupWindow.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 15/10/2015.
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
	/// Auxiliar window to create a new group in the current loaded level.
	/// </summary>
	public class AddGroupWindow : EditorWindow {
		//------------------------------------------------------------------//
		// CONSTANTS														//
		//------------------------------------------------------------------//
		private static readonly string PREFIX = "GRP_";

		// Group created delegate
		public delegate void GroupCreatedDelegate(Group _newGroup);

		//------------------------------------------------------------------//
		// MEMBERS															//
		//------------------------------------------------------------------//
		private Level m_targetLevel = null;
		private string m_name = "";
		private GroupCreatedDelegate m_groupCreatedDelegate = null;

		//------------------------------------------------------------------//
		// STATIC METHODS													//
		//------------------------------------------------------------------//
		/// <summary>
		/// Show the window.
		/// </summary>
		/// <param name="_targetLevel">The level where to add the new group</param>
		/// <param name="_groupCreatedDelegate">Optional delegate method to be called whenever the group is created.</param> 
		public static void Show(Level _targetLevel, GroupCreatedDelegate _groupCreatedDelegate = null) {
			// Nothing to do if given level is not valid
			if(_targetLevel == null) return;

			// Create a new window instance
			AddGroupWindow window = new AddGroupWindow();
			
			// Setup window
			window.minSize = new Vector2(200f, 70f);
			window.maxSize = window.minSize;
			window.m_targetLevel = _targetLevel;
			window.m_groupCreatedDelegate = _groupCreatedDelegate;

			// Open at cursor's position
			// The window expects the position in screen coords
			Rect pos = new Rect();
			pos.x = Event.current.mousePosition.x - window.maxSize.x/2f;
			pos.y = Event.current.mousePosition.y + 7f;	// A little bit lower
			pos.position = EditorGUIUtility.GUIToScreenPoint(pos.position);
			
			// Show it as a dropdown list so window is automatically closed upon losing focus
			// http://docs.unity3d.com/ScriptReference/EditorWindow.ShowAsDropDown.html
			window.ShowAsDropDown(pos, window.maxSize);
		}
		
		//------------------------------------------------------------------//
		// GENERIC METHODS													//
		//------------------------------------------------------------------//
		/// <summary>
		/// Default constructor.
		/// </summary>
		public AddGroupWindow() {
			// Nothing to do
		}
		
		/// <summary>
		/// Called every frame.
		/// </summary>
		private void Update() {

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
			EditorGUILayout.BeginVertical(); {
				// Name input
				EditorGUILayout.BeginHorizontal(); {
					GUILayout.Label("Name", GUILayout.ExpandWidth(false));

					GUI.enabled = false;
					GUILayout.TextField(PREFIX, GUILayout.ExpandWidth(false));
					GUI.enabled = true;

					m_name = GUILayout.TextField(m_name);
				} EditorGUILayoutExt.EndHorizontalSafe();

				// Confirm button
				if(GUILayout.Button("Add")) {
					// Do it!!
					// Create a new game object and add to it the Group component
					// It will automatically be initialized with the required hierarchy
					GameObject newGroupObj = new GameObject("", typeof(Group));
					Group newGroup = newGroupObj.GetComponent<Group>();

					// Add it to the level's hierarchy and generate unique name
					newGroupObj.transform.SetParent(m_targetLevel.gameObject.transform, true);
					newGroupObj.SetUniqueName(PREFIX + m_name);

					// Since groups are empty at the start, use one of Unity's default icons
					EditorUtils.SetObjectIcon(newGroupObj, EditorUtils.ObjectIcon.LABEL_GRAY);
					
					// Add and initialize the transform lock component
					// Arbitrary default values fitted to the most common usage when level editing
					TransformLock newLock = newGroupObj.AddComponent<TransformLock>();
					newLock.SetPositionLock(false, false, true);
					newLock.SetRotationLock(true, true, false);
					newLock.SetScaleLock(true, true, true);

					// Make operation undoable
					Undo.RegisterCreatedObjectUndo(newGroupObj, "LevelEditor AddGroup");

					// Set position more or less to where the camera is pointing, forcing Z-0
					// Select new object in the hierarchy and center camera to it
					LevelEditor.PlaceInFrontOfCameraAtZPlane(newGroupObj, true);

					// Invoke delegate (if any)
					if(m_groupCreatedDelegate != null) {
						m_groupCreatedDelegate(newGroup);
					}

					// Close window
					Close();
				}
			} EditorGUILayout.EndVertical();
			
			// Restore indentation
			EditorGUI.indentLevel = indentLevelBackup;
		}
	}
}