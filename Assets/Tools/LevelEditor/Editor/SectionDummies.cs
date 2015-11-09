// SectionDummies.cs
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
	public class SectionDummies : ILevelEditorSection {
		//------------------------------------------------------------------//
		// CONSTANTS														//
		//------------------------------------------------------------------//
		private static readonly string ASSETS_DIR = "Tools/LevelEditor/LDAssets/";
		private static readonly string PREFIX = "DM_";
		
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

			// Dragon prefabs
			SelectionGrid.Group dragonGroup = m_grid.GetGroup("DragonPreviews", true);
			if(dragonGroup != null) {
				// Basic properties
				dragonGroup.m_name = "Dragon Previews";
				dragonGroup.m_data = new Object[(int)DragonId.COUNT];
				dragonGroup.m_contents = new GUIContent[(int)DragonId.COUNT];

				// Init contents
				for(int i = 0; i < (int)DragonId.COUNT; i++) {
					// Load the prefab for the dragon with the given ID
					DragonData data = DragonManager.GetDragonData((DragonId)i);
					GameObject prefabObj = Resources.Load<GameObject>(data.prefabPath);
					dragonGroup.m_data[i] = prefabObj;
					dragonGroup.m_contents[i] = new GUIContent(prefabObj.name, AssetPreview.GetAssetPreview(prefabObj));
				}
			}

			// Dummy resources prefabs
			SelectionGrid.Group dummiesGroup = m_grid.GetGroup("DummyPrefabs", true);
			if(dummiesGroup != null) {
				// Basic properties
				dummiesGroup.m_name = "Dummy Prefabs";
				dummiesGroup.m_data = EditorUtils.LoadAllAssetsAtPath<GameObject>(ASSETS_DIR, "prefab", true);
				dummiesGroup.m_contents = new GUIContent[dummiesGroup.m_data.Length];
				
				// Init contents
				for(int i = 0; i < dummiesGroup.m_data.Length; i++) {
					dummiesGroup.m_contents[i] = new GUIContent(dummiesGroup.m_data[i].name, AssetPreview.GetAssetPreview(dummiesGroup.m_data[i]));
				}
			}
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
					GUILayout.FlexibleSpace();
					
					// Button
					GUI.enabled = (m_grid.selectedContent != null);
					if(GUILayout.Button("ADD DUMMY", GUILayout.Width(200), GUILayout.Height(30))) {
						// Is it a dragon preview or a legit dummy?
						if(m_grid.m_selectedGroupId == "DragonPreviews") {
							CreateDragonPreview();
						} else {
							AddNewDummy();
						}
					}
					GUI.enabled = true;
					
					// Center button
					GUILayout.FlexibleSpace();
				}EditorGUILayoutExt.EndHorizontalSafe();
				
				// Spacing
				GUILayout.Space(10);
			} EditorGUILayoutExt.EndVerticalSafe();
		}

		//------------------------------------------------------------------//
		// INTERNAL METHODS													//
		//------------------------------------------------------------------//
		/// <summary>
		/// Creates and adds a new dummy to the current level, using the selected parameters.
		/// </summary>
		private void AddNewDummy() {
			// Check all required parameters
			// First of all check that we have a selected group to add the preview to
			Group targetGroup = LevelEditorWindow.instance.sectionGroups.selectedGroup;
			if(targetGroup == null) {
				LevelEditorWindow.instance.ShowNotification(new GUIContent("A group must be selected first!"));
				return;
			}

			// Make sure selected prefab is valid
			GameObject dummyPrefab = m_grid.selectedObject as GameObject;
			if(dummyPrefab == null) { 
				LevelEditorWindow.instance.ShowNotification(new GUIContent("Please select a prefab from the list")); 
				return; 
			}
			
			// Create a new instance of the prefab and add it to the scene
			GameObject newDummyObj = PrefabUtility.InstantiatePrefab(dummyPrefab) as GameObject;
			newDummyObj.transform.SetParent(targetGroup.editorObj.transform, true);
			newDummyObj.SetLayerRecursively("LevelEditor");
			
			// Add a name based on the entity prefab
			string dummyName = dummyPrefab.name.Replace("PF_", "");	// Entity name without the preffix (if any)
			newDummyObj.SetUniqueName(PREFIX + dummyName);	// Add a prefix of our own and generate unique name
			
			// Add and initialize the transform lock component
			// Arbitrary default values fitted to the most common usage when level editing
			TransformLock newLock = newDummyObj.AddComponent<TransformLock>();
			newLock.SetPositionLock(false, false, true);
			newLock.SetRotationLock(true, true, false);
			newLock.SetScaleLock(false, false, true);
			
			// Make operation undoable
			Undo.RegisterCreatedObjectUndo(newDummyObj, "LevelEditor AddDummy");
			
			// Set position more or less to where the camera is pointing, forcing Z-0
			// Select new object in the hierarchy and center camera to it
			LevelEditor.PlaceInFrontOfCameraAtZPlane(newDummyObj, true);
			
			// Focus new object
			EditorUtils.FocusObject(newDummyObj);
		}

		/// <summary>
		/// Create a new dragon preview in the selected group.
		/// </summary>
		private void CreateDragonPreview() {
			// First of all check that we have a selected group to add the preview to
			Group targetGroup = LevelEditorWindow.instance.sectionGroups.selectedGroup;
			if(targetGroup == null) {
				LevelEditorWindow.instance.ShowNotification(new GUIContent("A group must be selected first!"));
				return;
			}
			
			// Make sure selected dragon prefab is valid
			GameObject dragonPrefab = m_grid.selectedObject as GameObject;
			if(dragonPrefab == null) { 
				LevelEditorWindow.instance.ShowNotification(new GUIContent("Please select a prefab from the list")); 
				return; 
			}
			
			// Create an instance of the dragon model - just the model, no logic whatsoever
			// We're only interested in the view subobject, get it and create an instance
			GameObject viewPrefabObj = dragonPrefab.FindSubObject("view");	// Naming convention
			GameObject previewObj = GameObject.Instantiate<GameObject>(viewPrefabObj);
			previewObj.SetLayerRecursively("LevelEditor");
			previewObj.name = dragonPrefab.name + "Dummy";
			previewObj.transform.SetParent(targetGroup.editorObj.transform, false);
			
			// Make operation undoable
			Undo.RegisterCreatedObjectUndo(previewObj, "LevelEditor AddDragonPreview");
			
			// Set position more or less to where the camera is pointing, forcing Z-0
			// Select new object in the hierarchy and center camera to it
			LevelEditor.PlaceInFrontOfCameraAtZPlane(previewObj, true);
			
			// Add and initialize a transform lock component
			// Arbitrary default values fitted to the most common usage when level editing
			TransformLock newLock = previewObj.AddComponent<TransformLock>();
			newLock.SetPositionLock(false, false, true);
			newLock.SetRotationLock(true, false, false);
			newLock.SetScaleLock(true, true, true);
		}
	}
}