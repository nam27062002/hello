// SectionDragonSpawn.cs
// 
// Created by Alger Ortín Castellví on 23/08/2016.
// Copyright (c) 2016 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

#pragma warning disable 0414

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
namespace LevelEditor {
	/// <summary>
	/// Simulate the progress of a single game run.
	/// TODO:
	/// 	- Use XP instead of time (local variables have already been refactored for XP)
	/// </summary>
	public class SectionDragonSpawn : ILevelEditorSection {
		//--------------------------------------------------------------------//
		// CONSTANTS														  //
		//--------------------------------------------------------------------//

		//--------------------------------------------------------------------//
		// MEMBERS AND PROPERTIES											  //
		//--------------------------------------------------------------------//
	
		//--------------------------------------------------------------------//
		// INTERFACE IMPLEMENTATION											  //
		//--------------------------------------------------------------------//
		/// <summary>
		/// Initialize this section.
		/// </summary>
		public void Init() {
			
		}
		
		/// <summary>
		/// Draw the section.
		/// </summary>
		public void OnGUI() {
			// Title - encapsulate in a nice button to make it foldable
			GUI.backgroundColor = Colors.gray;
			bool folded = Prefs.GetBoolEditor("LevelEditor.SectionDragonSpawn.folded", false);
			if(GUILayout.Button((folded ? "►" : "▼") + " Dragon Spawn Points", LevelEditorWindow.styles.sectionHeaderStyle, GUILayout.ExpandWidth(true))) {
				folded = !folded;
				Prefs.SetBoolEditor("LevelEditor.SectionDragonSpawn.folded", folded);
			}
			GUI.backgroundColor = Colors.white;

			// -Only show if unfolded
			if(!folded) {
				// Group in a box
				EditorGUILayout.BeginVertical(LevelEditorWindow.styles.sectionContentStyle, GUILayout.Height(1)); {	// [AOC] Requesting a very small size fits the group to its content's actual size
					// Aux vars
					LevelTypeSpawners spawnersLevel = null;
					// TODO MALH: select proper spawner scene
					List<Level> spawnersLevelList = LevelEditorWindow.sectionLevels.GetLevel(LevelEditorSettings.Mode.SPAWNERS);
					if ( spawnersLevelList != null && spawnersLevelList.Count > 0 ) 
						spawnersLevel = spawnersLevelList[0] as LevelTypeSpawners;
					bool levelLoaded = (spawnersLevel != null);
					bool playing = EditorApplication.isPlaying;
					string oldDragon = LevelEditor.settings.testDragon;
					string newDragon = oldDragon;

					// Only show if a Spawners Level is loaded
					if(!levelLoaded) {
						EditorGUILayout.HelpBox("A Spawners scene is required", MessageType.Error);
					} else {
						// Dragon selector
						GUI.enabled = levelLoaded && !playing;
						EditorGUILayout.BeginHorizontal(); {
							// Label
							GUILayout.Label("Test Dragon:");

							// Dragon selector
							string[] options = DefinitionsManager.SharedInstance.GetSkuList(DefinitionsCategory.DRAGONS).ToArray();
							int oldIdx = ArrayUtility.IndexOf<string>(options, oldDragon);
							int newIdx = EditorGUILayout.Popup(Mathf.Max(oldIdx, 0), options);	// Select first dragon if saved dragon was not found (i.e. sku changes)
							if(oldIdx != newIdx) {
								newDragon = options[newIdx];
								LevelEditor.settings.testDragon = newDragon;
								EditorUtility.SetDirty(LevelEditor.settings);
								EditorApplication.SaveAssets();
							}
						} EditorGUILayoutExt.EndHorizontalSafe();
						GUI.enabled = true;

						// Show/Create spawn point
						GameObject spawnPointObj = null;
						if(levelLoaded) spawnPointObj = spawnersLevel.GetDragonSpawnPoint(newDragon, false);
						if(spawnPointObj == null) {
							GUI.enabled = levelLoaded && !playing;
							if(GUILayout.Button("Create Spawn for " + newDragon)) {
								spawnPointObj = spawnersLevel.GetDragonSpawnPoint(newDragon, true);
								EditorUtils.SetObjectIcon(spawnPointObj, EditorUtils.ObjectIcon.LABEL_ORANGE);
								EditorUtils.FocusObject(spawnPointObj);
								Tools.current = Tool.Move;
							}
						} else {
							GUI.enabled = levelLoaded;
							if(GUILayout.Button("Show Spawn for " + newDragon)) {
								EditorUtils.FocusObject(spawnPointObj);
								Tools.current = Tool.Move;
							}
						}

						// Focus default spawn point
						GUI.enabled = levelLoaded;
						if(GUILayout.Button("Show Default Spawn")) {
							spawnPointObj = spawnersLevel.GetDragonSpawnPoint("", true);
							EditorUtils.FocusObject(spawnPointObj);
							EditorUtils.SetObjectIcon(spawnPointObj, EditorUtils.ObjectIcon.LABEL_ORANGE);	// Make sure we can see something :P
							Tools.current = Tool.Move;
						}

						// Focus Level Editor spawn point
						GUI.enabled = levelLoaded;
						if(GUILayout.Button("Show Level Editor Spawn")) {
							spawnPointObj = spawnersLevel.GetDragonSpawnPoint(LevelTypeSpawners.LEVEL_EDITOR_SPAWN_POINT_NAME, true);
							EditorUtils.FocusObject(spawnPointObj);
							EditorUtils.SetObjectIcon(spawnPointObj, EditorUtils.ObjectIcon.LABEL_ORANGE);	// Make sure we can see something :P
							Tools.current = Tool.Move;
						}

						GUI.enabled = true;
					}
				} EditorGUILayout.EndVertical();
			}
		}

		//--------------------------------------------------------------------//
		// INTERNAL METHODS													  //
		//--------------------------------------------------------------------//

		//--------------------------------------------------------------------//
		// CALLBACKS														  //
		//--------------------------------------------------------------------//

	}
}