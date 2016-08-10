// SectionSpawners.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 05/11/2015.
// Copyright (c) 2015 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
namespace LevelEditor {
	/// <summary>
	/// 
	/// </summary>
	public class SectionSpawners : ILevelEditorSection {
		//------------------------------------------------------------------//
		// CONSTANTS														//
		//------------------------------------------------------------------//
		private static readonly string RESOURCES_DIR = "Game/Entities";
		private static readonly string PREFIX = "SP_";
		
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
				
				// Type
				EditorGUIUtility.labelWidth = 50f;
				SpawnerType newType = (SpawnerType)EditorGUILayout.EnumPopup("Type:", LevelEditor.settings.spawnerType, GUILayout.Height(20));
				if ( newType != LevelEditor.settings.spawnerType )
				{
					// Refresh grid content
					LevelEditor.settings.spawnerType = newType;
					string dirPath = Application.dataPath + "/Resources/" + RESOURCES_DIR;
					DirectoryInfo rootDirInfo = new DirectoryInfo(dirPath);
					CreateGroup(rootDirInfo);
				}
				EditorGUIUtility.labelWidth = 0f;

				// Shape
				if ( LevelEditor.settings.spawnerType == SpawnerType.PATH ) 
				{// Path spawners don't need a shape
					LevelEditor.settings.spawnerShape = SpawnerShape.POINT;
				}
				else
				{
					EditorGUIUtility.labelWidth = 50f;
					LevelEditor.settings.spawnerShape = (SpawnerShape)EditorGUILayout.EnumPopup("Shape:", LevelEditor.settings.spawnerShape, GUILayout.Height(20));
					EditorGUIUtility.labelWidth = 0f;
				}

				// Label
				GUIStyle labelStyle = new GUIStyle(EditorStyles.label);
				labelStyle.alignment = TextAnchor.MiddleCenter;
				GUILayout.Label("Entity:", labelStyle);

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
					if(GUILayout.Button("ADD SPAWNER", GUILayout.Width(200), GUILayout.Height(30))) {
						// Just do it!
						AddNewSpawner();
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
			id = id.Substring(id.IndexOf(RESOURCES_DIR) + RESOURCES_DIR.Length);
			
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

					List<Object> datas = new List<Object>();
					// Load prefabs!
					for(int i = 0; i < files.Length; i++) 
					{
						GameObject prefab = Resources.Load<GameObject>(resourcePath + "/" + Path.GetFileNameWithoutExtension(files[i].Name));
						switch( LevelEditor.settings.spawnerType )
						{
							case SpawnerType.FLOCK:
							{
								if ( IsFlockBehaviour( prefab ) )
									datas.Add( prefab );
							}break;
							case SpawnerType.PATH:
							{
								if ( IsPathBehaviour( prefab ) )
									datas.Add( prefab );
							}break;
							case SpawnerType.STANDARD:
							{
								if ( !IsPathBehaviour( prefab ) && !IsFlockBehaviour( prefab ) )
									datas.Add( prefab );
							}break;
						}
					}
					dirGroup.m_data = datas.ToArray();
					
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
		/// Creates and adds a new spawner to the current group, using the selected parameters.
		/// </summary>
		private void AddNewSpawner() {
			// First of all check that we have a selected group to add the preview to
			Group targetGroup = null;/*LevelEditorWindow.instance.sectionGroups.selectedGroup;*/
			if(targetGroup == null) {
				LevelEditorWindow.instance.ShowNotification(new GUIContent("A group must be selected first!"));
				return;
			}
			
			// Make sure selected prefab is valid
			GameObject entityPrefab = m_grid.selectedObject as GameObject;
			if(entityPrefab == null) { 
				LevelEditorWindow.instance.ShowNotification(new GUIContent("Please select a prefab from the list")); 
				return; 
			}
			
			// Create a new object and add it to the scene
			GameObject newSpawnerObj = new GameObject();
			newSpawnerObj.transform.SetParent(targetGroup.spawnersObj.transform, true);
			
			// Add a name based on the entity prefab
			string spawnerName = entityPrefab.name.Replace("PF_", "");	// Entity name without the preffix (if any)
			
			// Add and initialize the transform lock component
			// Arbitrary default values fitted to the most common usage when level editing
			TransformLock newLock = newSpawnerObj.AddComponent<TransformLock>();
			newLock.SetPositionLock(false, false, true);
			newLock.SetRotationLock(true, true, true);
			newLock.SetScaleLock(true, true, true);
			
			// Add the spawner component - and optionally a suffix
			Spawner sp = null;
			if ( IsPathBehaviour( entityPrefab ) )
			{
				sp = newSpawnerObj.AddComponent<PathSpawner>();
				spawnerName += "Path";
			}
			else
			{
				sp = newSpawnerObj.AddComponent<Spawner>();
			}

			if ( entityPrefab.GetComponent<EntityGroupBehaviour>() != null )
				newSpawnerObj.AddComponent<EntityGroupController>();
				

			/*
			// Old version
			switch(LevelEditor.settings.spawnerType) {
				case SpawnerType.STANDARD: {
					sp = newSpawnerObj.AddComponent<Spawner>();
				} break;
					
				case SpawnerType.FLOCK: {
					sp = newSpawnerObj.AddComponent<FlockSpawner>();
					spawnerName += "Flock";
				} break;

				case SpawnerType.PATH: {
					sp = newSpawnerObj.AddComponent<PathSpawner>();
					spawnerName += "Path";
				} break;
			}
			*/
			
			// Add a prefix of our own and generate unique name
			newSpawnerObj.SetUniqueName(PREFIX + spawnerName);
			
			// Initialize spawner with the target prefab
			sp.m_entityPrefab = entityPrefab;
			
			// Add the shape component
			float size = 25f;	// Will be used to focus the camera. Start with the default size for a point.
			switch(LevelEditor.settings.spawnerShape) {
				case SpawnerShape.POINT: {
					// Nothing to do :)
				} break;
					
				case SpawnerShape.RECTANGLE: {
					RectArea2D area = newSpawnerObj.AddComponent<RectArea2D>();
					size = area.size.magnitude;
				} break;
					
				case SpawnerShape.CIRCLE: {
					CircleArea2D area = newSpawnerObj.AddComponent<CircleArea2D>();
					size = area.radius * 2f;
				} break;
			}
			
			// Add a spawner icon generator component as well
			newSpawnerObj.AddComponent<SpawnerIconGenerator>();
			
			// Make operation undoable
			Undo.RegisterCreatedObjectUndo(newSpawnerObj, "LevelEditor AddSpawner");
			
			// Set position more or less to where the camera is pointing, forcing Z-0
			// Select new object in the hierarchy and center camera to it
			LevelEditor.PlaceInFrontOfCameraAtZPlane(newSpawnerObj, true);
			
			// [AOC] Unfortunately FrameLastActiveSceneView fails to figure out the actual bounds of the spawner, so we 
			//       move the camera manually (based on the SceneView source code found in https://github.com/MattRix/UnityDecompiled/blob/cc432a3de42b53920d5d5dae85968ff993f4ec0e/UnityEditor/UnityEditor/SceneView.cs)
			SceneView scene = SceneView.lastActiveSceneView;
			scene.LookAt(newSpawnerObj.transform.position, scene.rotation, size, scene.orthographic, EditorApplication.isPlaying);
		}

		bool IsPathBehaviour( GameObject _entity)
		{
			return _entity.GetComponent<FollowPathBehaviour>() != null;	
		}

		bool IsFlockBehaviour( GameObject _entity )
		{
			return _entity.GetComponent<FlockBehaviour>() != null;	
		}


	}


}