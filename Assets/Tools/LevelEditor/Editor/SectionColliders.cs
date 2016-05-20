// SectionSpawners.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 06/11/2015.
// Copyright (c) 2015 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;
using UnityEditor;
using System.IO;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
namespace LevelEditor {
	/// <summary>
	/// 
	/// </summary>
	public class SectionColliders : ILevelEditorSection {
		//------------------------------------------------------------------//
		// CONSTANTS														//
		//------------------------------------------------------------------//
		private static readonly string RESOURCES_DIR = "Game/Colliders";
		private static readonly string ROOT_DIR = "Resources/Game/";

		//------------------------------------------------------------------//
		// MEMBERS															//
		//------------------------------------------------------------------//
		private SelectionGrid m_grid = new SelectionGrid();

		
		//------------------------------------------------------------------//
		// INTERFACE IMPLEMENTATION											//
		//------------------------------------------------------------------//
		/// <summary>
		/// Initialize this section.
		/// </summary>
		public void Init() {
			// Refresh grid data
			// Get folder structure
			string dirPath = Application.dataPath + "/Resources/" + RESOURCES_DIR;
			DirectoryInfo rootDirInfo = new DirectoryInfo(dirPath);
			
			// Get/Create a group for this directory!
			CreateGroup(rootDirInfo);
		}

		/// <summary>
		/// Draw the section.
		/// </summary>
		public void OnGUI() {
			// Show all options in a list
			EditorGUILayout.BeginVertical(); {
				// Spacing
				GUILayout.Space(5);
				
				// Grid list!
				m_grid.OnGUI();
				
				// Spacing
				GUILayout.Space(5);
				
				// Do it button
				EditorGUILayout.BeginHorizontal(); {
					// Center button
					GUILayout.Space(30);	// To compensate for the 'refresh' button
					GUILayout.FlexibleSpace();
					
					// Button
					GUI.enabled = (m_grid.selectedContent != null);
					if(GUILayout.Button("ADD COLLIDER", GUILayout.Width(200), GUILayout.Height(30))) {
						// Just do it!
						AddNewCollider();
					}
					GUI.enabled = true;
					
					// Center button
					GUILayout.FlexibleSpace();

					// Refresh button
					if(GUILayout.Button("↻", GUILayout.Width(30), GUILayout.Height(30))) {
						m_grid.m_groups.Clear();
						Init();
					}
				}EditorGUILayoutExt.EndHorizontalSafe();
				
				// Spacing
				GUILayout.Space(5);
			} EditorGUILayoutExt.EndVerticalSafe();
		}
		
		//------------------------------------------------------------------//
		// INTERNAL METHODS													//
		//------------------------------------------------------------------//
		/// <summary>
		/// Create a group in the selection grid for the given directory.
		/// </summary>
		/// <param name="_dirInfo">The directory to be loaded.</param>
		private void CreateGroup(DirectoryInfo _dirInfo) {
			// Create ID - dir path starting at root dir
			string id = _dirInfo.FullName.Replace('\\', '/');	// [AOC] Windows uses backward slashes, which Unity doesn't recognize;
			id = id.Substring(id.IndexOf(ROOT_DIR) + ROOT_DIR.Length);
			
			// Format dir path to something that Unity Resources API understands
			string resourcePath = _dirInfo.FullName.Replace('\\', '/');	// [AOC] Windows uses backward slashes, which Unity doesn't recognize
			resourcePath = resourcePath.Substring(resourcePath.IndexOf(RESOURCES_DIR));

			// Get all prefabs in the target directory, but don't include subdirectories
			FileInfo[] files = _dirInfo.GetFiles("*.prefab");
			
			// Get/Create group linked to this directory
			// Ignore if the directory has no valid prefabs
			if(files.Length > 0) {
				SelectionGrid.Group dirGroup = m_grid.GetGroup(id, true);
				if(dirGroup != null) {
					// Basic properties
					dirGroup.m_name = id;
					
					// Load prefabs!
					dirGroup.m_data = new Object[files.Length];
					for(int i = 0; i < files.Length; i++) {
						dirGroup.m_data[i] = Resources.Load<GameObject>(resourcePath + "/" + Path.GetFileNameWithoutExtension(files[i].Name));
					}
					
					// Init contents
					dirGroup.m_contents = new GUIContent[dirGroup.m_data.Length];
					for(int i = 0; i < dirGroup.m_data.Length; i++) {
						dirGroup.m_contents[i] = new GUIContent(dirGroup.m_data[i].name, AssetPreview.GetAssetPreview(dirGroup.m_data[i]));
					}
				}
			}
			
			// Iterate subdirectories and create group for each of them as well!
			DirectoryInfo[] subdirs = _dirInfo.GetDirectories();
			for(int i = 0; i < subdirs.Length; i++) {
				// Ignore "Assets" directories
				if(subdirs[i].Name != "Assets") {
					// Recursive call
					CreateGroup(subdirs[i]);
				}
			}
		}

		/// <summary>
		/// Creates and adds a new collider to the current group, using the selected parameters.
		/// </summary>
		private void AddNewCollider() {
			// First of all check that we have a selected group to add the preview to
			Group targetGroup = null;/*LevelEditorWindow.instance.sectionGroups.selectedGroup*/;
			if(targetGroup == null) {
				LevelEditorWindow.instance.ShowNotification(new GUIContent("A group must be selected first!"));
				return;
			}
			
			// Make sure selected prefab is valid
			GameObject colliderPrefab = m_grid.selectedObject as GameObject;
			if(colliderPrefab == null) { 
				LevelEditorWindow.instance.ShowNotification(new GUIContent("Please select a prefab from the list")); 
				return; 
			}
			
			// Create a new object and add it to the scene
			GameObject newColliderObj = PrefabUtility.InstantiatePrefab(colliderPrefab) as GameObject;
			newColliderObj.transform.SetParent(targetGroup.colliderObj.transform, true);
			
			// Add a prefix of our own and generate unique name
			newColliderObj.SetUniqueName(newColliderObj.name);
			
			// Make operation undoable
			Undo.RegisterCreatedObjectUndo(newColliderObj, "LevelEditor AddCollider");
			
			// Set position more or less to where the camera is pointing, forcing Z-0
			// Select new object in the hierarchy and center camera to it
			LevelEditor.PlaceInFrontOfCameraAtZPlane(newColliderObj, true);
			
			// Focus new object
			EditorUtils.FocusObject(newColliderObj);
		}
	}
}