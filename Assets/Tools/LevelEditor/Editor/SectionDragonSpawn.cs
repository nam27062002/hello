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
			if(GUILayout.Button((folded ? "►" : "▼") + " Spawn Points", LevelEditorWindow.styles.sectionHeaderStyle, GUILayout.ExpandWidth(true))) {
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

					List<string> availableSpawnPoints = new List<string>();

					// Dragon selector
					GUI.enabled = !playing;
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
							AssetDatabase.SaveAssets();
						}
					} EditorGUILayoutExt.EndHorizontalSafe();

					GUI.enabled = true;
					EditorGUILayout.BeginHorizontal(); {
						// Label
						bool intro = GUILayout.Toggle(LevelEditor.settings.useIntro, "Intro");
						if ( intro != LevelEditor.settings.useIntro )
						{
							LevelEditor.settings.useIntro = intro;
							EditorUtility.SetDirty(LevelEditor.settings);
							AssetDatabase.SaveAssets();
						}
					} EditorGUILayoutExt.EndHorizontalSafe();

					EditorGUILayout.BeginHorizontal(); {
						// Label
						bool cameraSpawn = GUILayout.Toggle(LevelEditor.settings.spawnAtCameraPos, "Spawn At Camera");
						if ( cameraSpawn != LevelEditor.settings.spawnAtCameraPos )
						{
							LevelEditor.settings.spawnAtCameraPos = cameraSpawn;
							EditorUtility.SetDirty(LevelEditor.settings);
							AssetDatabase.SaveAssets();
						}
					} EditorGUILayoutExt.EndHorizontalSafe();

					// Only show if a Spawners Level is loaded
					if(!levelLoaded) {
						EditorGUILayout.HelpBox("A Spawners scene is required", MessageType.Error);
					} else {
                        // Show/Create spawn point
                        GameObject spawnPointObj = null;

						GUILayout.Space(20);
						GUILayout.Label("Dragon Spawn Points:");							
						GUILayout.Space(5);

                        // Focus default spawn point
                        GUI.enabled = levelLoaded;
                        spawnPointObj = spawnersLevel.GetDragonSpawnPoint("", false, false);
                        if (spawnPointObj == null && !playing) {
							GUI.backgroundColor = Colors.orange;
                            if (GUILayout.Button("Create Default Spawn")) {
                                ConfigureSpawnObj(spawnersLevel.GetDragonSpawnPoint("", true));
                            }
                        } else if (spawnPointObj != null) {
							GUI.backgroundColor = Colors.paleGreen;
                            if (GUILayout.Button("Show Default Spawn")) {
                                ConfigureSpawnObj(spawnersLevel.GetDragonSpawnPoint("", false));
                            }
							availableSpawnPoints.Add(spawnPointObj.name);
                        }

                        GUI.enabled = levelLoaded;
                        spawnPointObj = spawnersLevel.GetDragonSpawnPoint(newDragon, false, false);
                        if (spawnPointObj == null && !playing) {
							GUI.backgroundColor = Colors.orange;
                            if (GUILayout.Button("Create Spawn for " + newDragon)) {
                                ConfigureSpawnObj(spawnersLevel.GetDragonSpawnPoint(newDragon, true));
                            }
                        } else if (spawnPointObj != null) {
							GUI.backgroundColor = Colors.paleGreen;
                            if (GUILayout.Button("Show Spawn for " + newDragon)) {
                                ConfigureSpawnObj(spawnersLevel.GetDragonSpawnPoint(newDragon, false));
                            }
							availableSpawnPoints.Add(spawnPointObj.name);
                        }

						// Focus Level Editor spawn point
						GUI.enabled = levelLoaded;
                        spawnPointObj = spawnersLevel.GetDragonSpawnPoint(LevelTypeSpawners.LEVEL_EDITOR_SPAWN_POINT_NAME, false, false);
                        if (spawnPointObj == null && !playing) {
							GUI.backgroundColor = Colors.orange;
                            if (GUILayout.Button("Create Level Editor Spawn")) {
                                ConfigureSpawnObj(spawnersLevel.GetDragonSpawnPoint(LevelTypeSpawners.LEVEL_EDITOR_SPAWN_POINT_NAME, true));
                            }
                        } else if (spawnPointObj != null) {
							GUI.backgroundColor = Colors.paleGreen;
                            if (GUILayout.Button("Show Level Editor Spawn")) {
                                ConfigureSpawnObj(spawnersLevel.GetDragonSpawnPoint(LevelTypeSpawners.LEVEL_EDITOR_SPAWN_POINT_NAME, false));
                            }
							availableSpawnPoints.Add(spawnPointObj.name);
                        }

						if (levelLoaded && spawnersLevelList != null && spawnersLevelList.Count > 0) {
							GUILayout.Space(20);
							GUILayout.Label("Level Spawn Points:");							
							GUILayout.Space(5);
							List<DefinitionNode> spawnPoints = DefinitionsManager.SharedInstance.GetDefinitionsList(DefinitionsCategory.LEVEL_SPAWN_POINTS);
							foreach (DefinitionNode spawnPoint in spawnPoints) {								
								spawnersLevel = null;

								foreach(LevelTypeSpawners level in spawnersLevelList) {
									if (level.m_sceneTags.Contains(spawnPoint.Get("sceneTags"))) {
										spawnersLevel = level;
										break;
									}									
								}

								if (spawnersLevel != null) {
									EditorGUILayout.BeginHorizontal(); {										
										// Label
										GUILayout.Label(spawnPoint.Get("tidName"), GUILayout.Width(200));
										spawnPointObj = spawnersLevel.GetDragonSpawnPoint(spawnPoint.Get("sku"), false, false);
										if (spawnPointObj == null && !playing) {
											GUI.backgroundColor = Colors.orange;
											if (GUILayout.Button("Create " + spawnPoint.Get("sku") + " Spawn")) {
												ConfigureSpawnObj(spawnersLevel.GetDragonSpawnPoint(spawnPoint.Get("sku"), true));
											}
										} else if (spawnPointObj != null) {
											GUI.backgroundColor = Colors.paleGreen;
											if (GUILayout.Button("Show " + spawnPoint.Get("sku") + " Spawn")) {
												ConfigureSpawnObj(spawnersLevel.GetDragonSpawnPoint(spawnPoint.Get("sku"), false));
											}
											availableSpawnPoints.Add(spawnPointObj.name);
										}
									} EditorGUILayoutExt.EndHorizontalSafe();							
								}								
							}
						}
						GUI.enabled = true;
					}

					GUI.enabled = !playing;
					GUILayout.Space(20);
					EditorGUILayout.BeginHorizontal(); {						
						GUILayout.Label("Selected Spawn Point:");
						
						string[] options = availableSpawnPoints.ToArray();
						int oldIdx = ArrayUtility.IndexOf<string>(options, LevelEditor.settings.spawnPoint);
						
						GUI.backgroundColor = Colors.silver;
						int newIdx = EditorGUILayout.Popup(Mathf.Max(oldIdx, 0), options);
						if(oldIdx != newIdx) {							
							LevelEditor.settings.spawnPoint = options[newIdx];
							EditorUtility.SetDirty(LevelEditor.settings);
							AssetDatabase.SaveAssets();
						}
					} EditorGUILayoutExt.EndHorizontalSafe();
				} EditorGUILayout.EndVertical();
			}
		}

		//--------------------------------------------------------------------//
		// INTERNAL METHODS													  //
		//--------------------------------------------------------------------//

        private void ConfigureSpawnObj(GameObject _go) {
            if (_go != null) {
                EditorUtils.FocusObject(_go);
                EditorUtils.SetObjectIcon(_go, EditorUtils.ObjectIcon.LABEL_ORANGE);  // Make sure we can see something :P
                Tools.current = Tool.Move;
            }
        }

		//--------------------------------------------------------------------//
		// CALLBACKS														  //
		//--------------------------------------------------------------------//

	}
}			