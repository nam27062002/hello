// SectionLevels.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 05/11/2015.
// Copyright (c) 2015 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.IO;
using System.Text;
using System.Collections.Generic;

#pragma warning disable 0414

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
namespace LevelEditor {
	/// 
	/// </summary>
	public class SectionLevels : ILevelEditorSection {
		//------------------------------------------------------------------//
		// CONSTANTS														//
		//------------------------------------------------------------------//
		private static readonly string ASSETS_DIR = "Assets/Game/Scenes/Levels";
		private static readonly float AUTOSAVE_FREQUENCY = 60f;	// Seconds

		

		//------------------------------------------------------------------//
		// MEMBERS AND PROPERTIES											//
		//------------------------------------------------------------------//
		// Current level - one level for each tab
		private List<Level>[] m_activeLevels = new List<Level>[(int)LevelEditorSettings.Mode.COUNT];
		public List<Level> activeLevels { 
			get { 
					if ( LevelEditor.settings.selectedMode < LevelEditorSettings.Mode.COUNT )
						return m_activeLevels[(int)LevelEditor.settings.selectedMode]; 
					else 
						return null;
				}
			set { m_activeLevels[(int)LevelEditor.settings.selectedMode] = value; }
		}	

		// Internal
		private FileInfo[] m_fileList = null;
		private float m_autoSaveTimer = 0f;

		private List<string> m_levelsSkuList = new List<string>();

        // Only load ART levels
        private bool m_onlyArt = false;

		// Every type of scene goes in a different sub-folder
		private string assetDirForCurrentMode {
			get { 
				switch(LevelEditor.settings.selectedMode) {
					case LevelEditorSettings.Mode.SPAWNERS:		return ASSETS_DIR + "/" + "Spawners";
					case LevelEditorSettings.Mode.COLLISION:	return ASSETS_DIR + "/" + "Collision";
					case LevelEditorSettings.Mode.ART:			return ASSETS_DIR + "/" + "Art";
					case LevelEditorSettings.Mode.SOUND:		return ASSETS_DIR + "/" + "Sound";
				}
				return ASSETS_DIR;
			}
		}

		//------------------------------------------------------------------//
		// GETTERS															//
		//------------------------------------------------------------------//
		/// <summary>
		/// Get the level corresponding to the given editor mode.
		/// </summary>
		/// <returns>The level. Null if no level of the target type is open.</returns>
		/// <param name="_mode">Type of level to be retrieved.</param>
		public List<Level> GetLevel(LevelEditorSettings.Mode _mode) {
			return m_activeLevels[(int)_mode];
		}
		
		//------------------------------------------------------------------//
		// INTERFACE IMPLEMENTATION											//
		//------------------------------------------------------------------//
		/// <summary>
		/// Initialize this section.
		/// </summary>
		public void Init() {
			// Figure out active level for each mode
			// This is called every time the editor is loaded or when switching to/from play mode
			for(int mode = 0; mode < (int)LevelEditorSettings.Mode.COUNT; mode++) {
				// Find all loaded levels of the target type in the current scene (should only be one)
				List<Level> levelsInScene = new List<Level>();
				switch((LevelEditorSettings.Mode)mode) {
					case LevelEditorSettings.Mode.SPAWNERS:		levelsInScene.AddRange(GameObject.FindObjectsOfType<LevelTypeSpawners>());	break;
					case LevelEditorSettings.Mode.COLLISION:	levelsInScene.AddRange(GameObject.FindObjectsOfType<LevelTypeCollision>());	break;
					case LevelEditorSettings.Mode.ART:			levelsInScene.AddRange(GameObject.FindObjectsOfType<LevelTypeArt>());		break;
					case LevelEditorSettings.Mode.SOUND:		levelsInScene.AddRange(GameObject.FindObjectsOfType<LevelTypeSound>());		break;
				}

				// Several cases:
				// a) There are no levels, start without level
				if(levelsInScene.Count == 0) {
					m_activeLevels[mode] = null;
				}
				else
				{
					m_activeLevels[mode] = levelsInScene;
				}
				/*
				// b) There is only one level, use it as current level
				else if(levelsInScene.Count == 1) {
					m_activeLevels[mode] = levelsInScene[0];
				}
				// c) There are multiple levels in the scene
				else {
					// Iterate them and prompt user what to do with each of them
					string activeLevelName = activeLevels[0].gameObject.scene.name;
					int newLevelIdx = -1;
					for(int i = 0; i < levelsInScene.Count; i++) {
						// Unity makes it easy for us ^_^
						m_activeLevels[mode] = levelsInScene[i];
						int whatToDo = EditorUtility.DisplayDialogComplex(
							activeLevelName,
							"There are multiple levels loaded into the current scene, but only one should be loaded at a time.\n" +
							"What would you like to do with level " + activeLevelName + "?",
							"Make Active", "Save and Unload", "Unload"
						);

						// a) Make it active
						if(whatToDo == 0) {
							// If there was already a selected level, unload it
							if(newLevelIdx >= 0) {
								m_activeLevels[mode] = levelsInScene[newLevelIdx];

								// Prompt user to save
								if(EditorUtility.DisplayDialog(
									"Changing Active Level",
									"Level " + activeLevels.gameObject.scene.name + " was already chosen as active one.\n" +
									"What you want to do with it?",
									"Save and Unload", "Unload"
								)) {
									// Save level
									SaveLevel();
								}

								// Unload in any case
								UnloadLevel(false);
							}

							// Store new level index
							newLevelIdx = i;
						} 

						// b) Save and unload
						else if(whatToDo == 1) {
							SaveLevel();
							UnloadLevel(false);
						} 

						// c) Just unload
						else {
							UnloadLevel(false);
						}
					}

					// Store new active level
					if(newLevelIdx >= 0) {
						m_activeLevels[mode] = levelsInScene[newLevelIdx];
					} else {
						m_activeLevels[mode] = null;
					}
				}
				*/
			}
			
			// Reset autosave timer
			m_autoSaveTimer = 0f;
		}


        private static readonly string LIGHT_CONTAINER_SCENE_PATH = "Assets/Game/Scenes/Levels/Art/ART_Medieval_Lighting_Container.unity"; //"Assets/Tools/LevelEditor/SC_LevelEditor.unity";


		bool removeSceneFromActiveLevels(Scene scene)
		{
            bool ret = false;

            for (int c = 0; c < m_activeLevels.Length; c++)
            {
                List<Level> levelList = m_activeLevels[c];
                if (levelList == null) continue;

                List<Level> newList = new List<Level>();

                int levelCount = levelList.Count;
                for (int a = 0; a < levelCount; a++)
                {
                    if (levelList[a].gameObject.scene != scene)
                    {
                        newList.Add(levelList[a]);
                        ret = true;
                    }
                    else
                    {
                        Debug.Log("Scene: " + scene.name + " stripped from active levels.");
                    }
                }
                m_activeLevels[c] = newList;
            }
			return ret;
		}

		void stripNonArtScenes()
		{
            bool finish;

            do {
                finish = true;

                for (int c = 0; c < SceneManager.sceneCount; c++)
                {
                    Scene scene = SceneManager.GetSceneAt(c);

                    if (scene.name.IndexOf("ART_", System.StringComparison.OrdinalIgnoreCase) < 0)
                    {
                        removeSceneFromActiveLevels(scene);
                        EditorSceneManager.CloseScene(scene, true);
                        finish = false;
                        break;
                    }
                }
            } while (!finish);
		}

        Level getLightmapScene()
        {
            for (int c = 0; c < m_activeLevels.Length; c++)
            {
                List<Level> levelList = m_activeLevels[c];
                if (levelList == null) continue;

                int levelCount = levelList.Count;
                for (int a = 0; a < levelCount; a++)
                {
                    if (levelList[a].gameObject.scene.name.IndexOf("Medieval_Lighting", System.StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        return levelList[a];
                    }
                }
            }
            return null;
        }


        void lightMapCompleted()
        {
            LevelEditorWindow.instance.m_isLightmapping = false;
            Scene lightScene = EditorSceneManager.GetSceneByPath(LIGHT_CONTAINER_SCENE_PATH);

			removeSceneFromActiveLevels (lightScene);
            EditorSceneManager.CloseScene(lightScene, true);
        }

        void launchLightmap()
        {
			Level lightmapLevel = getLightmapScene();
			if (lightmapLevel == null) {
				Debug.Log ("Unable to find lightmap scene: ART_Medieval_LightingXXX");
				return;
			}

            Scene lightScene = EditorSceneManager.OpenScene(LIGHT_CONTAINER_SCENE_PATH, OpenSceneMode.Additive);
            if ( lightScene == null)
            {
                Debug.Log("Unable to find lightmap container scene: " + LIGHT_CONTAINER_SCENE_PATH);
                return;

            }

            LevelEditorWindow.instance.m_isLightmapping = true;

            LevelEditorWindow.instance.CloseLevelEditorScene();
            stripNonArtScenes();

            SceneManager.SetActiveScene (lightmapLevel.gameObject.scene);

            Lightmapping.completed = lightMapCompleted;

            Lightmapping.BakeAsync();
        }




        /// <summary>
        /// Draw the section.
        /// </summary>
        public void OnGUI() {
			// Aux vars
			bool levelLoaded = (activeLevels != null && activeLevels.Count > 0);
			bool playing = EditorApplication.isPlaying;

			// Some spacing
			GUILayout.Space(5f);

            EditorGUILayout.BeginHorizontal();
            {
                // Big juicy text showing the current level being edited
                GUIStyle titleStyle = new GUIStyle(EditorStyles.largeLabel);
    			titleStyle.fontSize = 20;
			    titleStyle.alignment = TextAnchor.MiddleCenter;

			    if(activeLevels != null && activeLevels.Count > 0) {
    				GUILayout.Label(activeLevels[0].gameObject.scene.name, titleStyle);
			    } else {
					GUILayout.FlexibleSpace();
					GUILayout.Label("No level loaded", titleStyle);
					if(GUILayout.Button("Detect", GUILayout.Height(30f))) {
						Init();
					}
					GUILayout.FlexibleSpace();
			    }
                if (LevelEditor.settings.selectedMode == LevelEditorSettings.Mode.ART)
                {
                    m_onlyArt = GUILayout.Toggle(m_onlyArt, "Only Art Levels");

                    if (activeLevels != null)
                    {
                        if (Lightmapping.isRunning)
                        {
                            if (GUILayout.Button("Cancel Lightmap"))
                            {
                                Lightmapping.Cancel();
                                lightMapCompleted();
                            }
                        }
                        else
                        {
                            if (GUILayout.Button("Launch Lightmap"))
                            {
                                launchLightmap();
                            }
                        }
                    }
                }

            }
            EditorGUILayoutExt.EndHorizontalSafe();

            // Some more spacing
            GUILayout.Space(5f);

			if(GUILayout.Button("Load Scenes From Definition")) {
				OnLoadScenesFromDefinition();
			}

			if(GUILayout.Button("Close All Scenes")) {
				OnCloseAllScenes();
			}

			// Toolbar
			EditorGUILayout.BeginVertical(LevelEditorWindow.styles.boxStyle, GUILayout.Height(1)); {	// [AOC] Requesting a very small size fits the group to its content's actual size
				EditorGUILayout.BeginHorizontal(); {
					// Create
					GUI.enabled = !playing;
					if(GUILayout.Button("New")) {
						OnNewLevelButton();
					}
					
					// Load
					if(GUILayout.Button("Open")) {
						OnOpenLevelButton();
					}
					// Add
					GUI.enabled = levelLoaded;
					if(GUILayout.Button("Add")) {
						OnAddLevelButton();
					}

					
					// Save - only if there is a level loaded and changes have been made
					GUI.enabled = levelLoaded && !playing && CheckChanges();
					if(GUILayout.Button("Save")) { 
						OnSaveLevelButton(); 
					}
					
					// Separator
					EditorGUILayoutExt.Separator(new SeparatorAttribute(5f), SeparatorAttribute.Orientation.VERTICAL);
					
					// Unload - only if there is a level loaded
					GUI.enabled = levelLoaded && !playing;
					if(GUILayout.Button("Close")) { 
						OnCloseLevelButton(); 
					}
					
					// Delete - only if there is a level loaded
					GUI.enabled = levelLoaded && !playing;
					if(GUILayout.Button("Delete")) { 
						OnDeleteLevelButton();
					}
					
					GUI.enabled = true;
				} EditorGUILayoutExt.EndHorizontalSafe();
				
				// If level was deleted or closed, don't continue (avoid null references)
				if(levelLoaded && activeLevels == null) return;
				
				// Separator
				EditorGUILayoutExt.Separator(new SeparatorAttribute(5f));

				// Other settings
				EditorGUILayout.BeginHorizontal(); {
					// Snap size
					EditorGUIUtility.labelWidth = EditorStyles.label.CalcSize(new GUIContent("Snap Size:")).x;
					float newSnapSize = EditorGUILayout.FloatField("Snap Size:", LevelEditor.settings.snapSize);
					newSnapSize = MathUtils.Snap(newSnapSize, 0.01f);	// Round up to 2 decimals
					newSnapSize = Mathf.Max(newSnapSize, 0f);	// Not negative
					LevelEditor.settings.snapSize = newSnapSize;

					// Handler size
					EditorGUIUtility.labelWidth = EditorStyles.label.CalcSize(new GUIContent("Handlers Size:")).x;
					float newHandlersSize = EditorGUILayout.FloatField("Handlers Size:", LevelEditor.settings.handlersSize);
					newHandlersSize = MathUtils.Snap(newHandlersSize, 0.1f);	// Round up to 1 decimal
					newHandlersSize = Mathf.Max(newHandlersSize, 0f);	// Not negative
					LevelEditor.settings.handlersSize = newHandlersSize;
					EditorGUIUtility.labelWidth = 0;
				} EditorGUILayoutExt.EndHorizontalSafe();
			} EditorGUILayoutExt.EndVerticalSafe();
		}

		//------------------------------------------------------------------//
		// INTERNAL METHODS													//
		//------------------------------------------------------------------//
		/// <summary>
		/// Prompts the save dialog and saves/discards the current level based on user's answer.
		/// </summary>
		private void PromptSaveDialog() {
			// Nothing to do if there is no loaded level
			if(activeLevels == null) return;
			
			// Only prompt if there are changes to actually be saved
			if(CheckChanges()) {
				// Prompt dialog
				if(EditorUtility.DisplayDialog("Save Level", "Save changes in current level?", "Yes", "No")) {
					// Just do it
					SaveLevel();
				}
			}
		}
		
		/// <summary>
		/// Applies changes to current loaded level's scene.
		/// </summary>
		/// <param name="_name">Optional name to be given to the file. If empty, the scene's current name will be used.</param>
		private void SaveLevel(string _name = "") {
			// Nothing to do if there is no loaded level
			if(activeLevels == null || activeLevels.Count == 0) return;

			// Figure out file name
			if(!string.IsNullOrEmpty(_name)) {
				// _name = activeLevels[0].gameObject.scene.name;
				// Save scene to disk - will automatically overwrite any existing scene with the same name
				EditorSceneManager.SaveScene(activeLevels[0].gameObject.scene, assetDirForCurrentMode + "/" + _name + ".unity");
			}
			else
			{
				for( int i = 0; i<activeLevels.Count; i++ )
				{
					_name = activeLevels[i].gameObject.scene.name;
					// Save scene to disk - will automatically overwrite any existing scene with the same name
					EditorSceneManager.SaveScene(activeLevels[i].gameObject.scene, assetDirForCurrentMode + "/" + _name + ".unity");		
				}
			}

			// Save assets to disk!!
			AssetDatabase.SaveAssets();
		}
		
		/// <summary>
		/// Unloads the current level, asking the user for saving changes first.
		/// </summary>
		/// <param name="_promptSave">Optionally ask whether to save or not before unloading (and do it).</param>
		private void UnloadLevel(bool _promptSave) {
			// Nothing to do if there is no loaded level
			if(activeLevels == null) return;
			
			// Ask for save
			if(_promptSave) PromptSaveDialog();

			// Close the scene containing the active level
			for( int i = 0; i<activeLevels.Count; i++ )
				EditorSceneManager.CloseScene(activeLevels[i].gameObject.scene, true);
			
			// Clear some references
			activeLevels = null;
		}

		private void UnloadAllLevels()
		{
			PromptSaveDialog();

			for( int j = 0; j<(int)LevelEditorSettings.Mode.COUNT; j++ )
			{
				if ( m_activeLevels[j] != null )
				{
					// Close the scene containing the active level
					for( int i = 0; i<m_activeLevels[j].Count; i++ ) {
						if(m_activeLevels[j][i] != null) {
							EditorSceneManager.CloseScene(m_activeLevels[j][i].gameObject.scene, true);
						}
					}
					m_activeLevels[j] = null;
				}
			}
		}
		
		/// <summary>
		/// Check changes performed on the current level vs its prefab.
		/// </summary>
		/// <returns><c>true</c>, if the instance was modified, <c>false</c> otherwise.</returns>
		private bool CheckChanges() {
			// Nothing to do if there is no loaded level
			if(activeLevels == null) return false;

			// Unity makes it easy for us
			bool isDirty = false;
			for( int i = 0; i<activeLevels.Count && !isDirty; i++ )
				if (activeLevels[i] != null)
					isDirty = isDirty || activeLevels[i].gameObject.scene.isDirty;
			return isDirty;
		}

		//------------------------------------------------------------------//
		// CALLBACKS														//
		//------------------------------------------------------------------//
		/// <summary>
		/// The "New" button has been pressed.
		/// </summary>
		private void OnNewLevelButton() {
			// Unload current level - will ask to save first
			UnloadLevel(true);

			// Find out suffix for the level based on current mode
			string preSuffix = "";
			switch(LevelEditor.settings.selectedMode) {
				case LevelEditorSettings.Mode.SPAWNERS:		preSuffix = "SP";	break;
				case LevelEditorSettings.Mode.COLLISION:	preSuffix = "CO";	break;
				case LevelEditorSettings.Mode.ART:			preSuffix = "ART";	break;
				case LevelEditorSettings.Mode.SOUND:		preSuffix = "SO";	break;
			}
			
			// Find out a suitable name for the new level
			int i = 0;
			string name = "";
			string path = Application.dataPath + assetDirForCurrentMode.Replace("Assets", "") + "/";	// dataPath already includes the "Assets" directory
			do {
				name = preSuffix + "_Level_" + i;
				i++;
			} while(File.Exists(path + name + ".unity"));

			// Create a new scene with the target name and make it the main one
			Scene newScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
			EditorSceneManager.SetActiveScene(newScene);

			// Create a new game object and add to it the Level component corresponding to the current edition mode
			// It will automatically be initialized with the required hierarchy
			// Since the new scene is the active one, it will be added to the root of it
			GameObject newLevelObj = null;
			switch(LevelEditor.settings.selectedMode) {
				case LevelEditorSettings.Mode.SPAWNERS:		newLevelObj = new GameObject("Level", typeof(LevelTypeSpawners));	break;
				case LevelEditorSettings.Mode.COLLISION:	newLevelObj = new GameObject("Level", typeof(LevelTypeCollision));	break;
				case LevelEditorSettings.Mode.ART:			newLevelObj = new GameObject("Level", typeof(LevelTypeArt));		break;
				case LevelEditorSettings.Mode.SOUND:		newLevelObj = new GameObject("Level", typeof(LevelTypeSound));		break;
			}
			activeLevels = new List<Level>();
			activeLevels.Add( newLevelObj.GetComponent<Level>() );

			// Save the new scene to the default dir with the name we figured out before
			SaveLevel(name);

			// Select the level and ping the scene file
			Selection.activeObject = activeLevels[0].gameObject;
			EditorGUIUtility.PingObject(AssetDatabase.LoadMainAssetAtPath(assetDirForCurrentMode + "/" + name + ".unity"));
		}


        private List<List<string>> m_sceneAreas = new List<List<string>>();
        private int firstArea = 0;


        void stripData(ref List<string> d0, List<string> d1)
        {
            for (int c = 0; c < d1.Count; c++)
            {
                d0.Remove(d1[c]);
            }
        }

        string putSeparators(List<string> list, char separator)
        {
            StringBuilder sb = new StringBuilder();

            for (int c = 0; c < list.Count; c++)
            {
                sb.Append(list[c]);
                if (c < list.Count - 1)
                    sb.Append(separator);
            }

            return sb.ToString();
        }

		private void OnLoadScenesFromDefinition(){

			Dictionary<string,DefinitionNode> defs = DefinitionsManager.SharedInstance.GetDefinitions(DefinitionsCategory.LEVELS);
			m_levelsSkuList.Clear();
			List<string> options = new List<string>(defs.Count);
            List<string> sceneareas = new List<string>();

            foreach (KeyValuePair<string, DefinitionNode> kvp in defs) {
				m_levelsSkuList.Add(kvp.Key);
				string id = kvp.Key + " (" + kvp.Value.Get("common");

				int areaIndex = 1;

                string areaString = kvp.Value.Get("area"+areaIndex);
                sceneareas.Add(areaString);
				while(!string.IsNullOrEmpty(areaString))
				{
					id += ";" + areaString;
					areaIndex++;
					areaString = kvp.Value.Get("area"+areaIndex);
				}
				id += ")";
				options.Add(id);

            }

            //Hungry Dragon specific
            DefinitionNode dfn = defs["level_0"];
            firstArea = options.Count;

            options.Add("common (" + dfn.Get("common") + ")");

            m_sceneAreas.Clear();

            List<string> area12 = dfn.GetAsList<string>("area12");
            List<string> area13 = dfn.GetAsList<string>("area13");

            List<string> area = dfn.GetAsList<string>("area1");
            stripData(ref area, area12);
            stripData(ref area, area13);
			List<string> qualityLevelScenesArea1 = GetAllQualityScenesFor( dfn, 1 );
			area.AddRange( qualityLevelScenesArea1 );
            m_sceneAreas.Add(area);
            options.Add("area1 (" + putSeparators(area, ';') + ")");

            area = dfn.GetAsList<string>("area2");
            stripData(ref area, area12);
			List<string> qualityLevelScenesArea2 = GetAllQualityScenesFor( dfn, 2 );
			area.AddRange( qualityLevelScenesArea2 );
            m_sceneAreas.Add(area);
            options.Add("area2 (" + putSeparators(area, ';') + ")");

            area = dfn.GetAsList<string>("area3");
            stripData(ref area, area13);
			List<string> qualityLevelScenesArea3 = GetAllQualityScenesFor( dfn, 3 );
			area.AddRange( qualityLevelScenesArea3 );
            m_sceneAreas.Add(area);
            options.Add("area3 (" + putSeparators(area, ';') + ")");

            m_sceneAreas.Add(area12);
            options.Add("area12 (" + putSeparators(area12, ';') + ")");

            m_sceneAreas.Add(area13);
            options.Add("area13 (" + putSeparators(area13, ';') + ")");

            // Show selection popup
            SelectionPopupWindow.Show(options.ToArray(), OnLoadScenesFromDefinitions);

		}

		public List<string> GetAllQualityScenesFor( DefinitionNode def, int areaIndex )
		{
			List<string> ret = new List<string>();
			string[] qualityLevels = {"_low", "_mid", "_high"};
			for( int i = 0; i<qualityLevels.Length; i++ )
			{
				List<string> qualityScenes = def.GetAsList<string>("area" + areaIndex + qualityLevels[i]);
				ret.AddRange( qualityScenes );
			}
			return ret;
		}

		private void OnLoadScenesFromDefinitions( int id )
		{
            bool common = true, leveleditor = true, area = true;

            string sku;
            if (id >= firstArea)
            {
                sku = m_levelsSkuList[0];
                if (id == firstArea)
                {
                    leveleditor = area = false;
                }
                else
                {
                    leveleditor = common = false;
                }

            }
            else
            {
                sku = m_levelsSkuList[id >= firstArea ? 0 : id];
            }

            // Store level data of the new level
            LevelEditor.settings.levelSku = sku;
			EditorUtility.SetDirty(LevelEditor.settings);
			AssetDatabase.SaveAssets();

			UnloadAllLevels();

			DefinitionNode def = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.LEVELS, sku);

			LevelEditorSettings.Mode oldMode = LevelEditor.settings.selectedMode;

            bool onlyArt = m_onlyArt && (oldMode == LevelEditorSettings.Mode.ART);

            if (common)
            {
                List<string> commonScene = def.GetAsList<string>("common");
                for (int i = 0; i < commonScene.Count; i++)
                {
                    EditorUtility.DisplayProgressBar("Loading Scenes for " + sku + "...", "Loading common scenes: " + commonScene[i] + "...", (float)i / (float)commonScene.Count);
                    LevelEditor.settings.selectedMode = GetModeByName(commonScene[i]);
                    if (!onlyArt || commonScene[i].StartsWith("ART"))
                    {
                        OnLoadLevel(commonScene[i] + ".unity");

                    }
                }
            }

            if (leveleditor)
            {
                List<string> editorOnlyScenes = def.GetAsList<string>("levelEditor");
                for (int i = 0; i < editorOnlyScenes.Count; i++)
                {
                    if (!string.IsNullOrEmpty(editorOnlyScenes[i]))
                    {
                        EditorUtility.DisplayProgressBar("Loading Scenes for " + sku + "...", "Loading level editor scenes: " + editorOnlyScenes[i] + "...", (float)i / (float)editorOnlyScenes.Count);
                        LevelEditor.settings.selectedMode = GetModeByName(editorOnlyScenes[i]);
                        if (!onlyArt || editorOnlyScenes[i].StartsWith("ART"))
                        {
                            OnLoadLevel(editorOnlyScenes[i] + ".unity");
                        }
                    }
                }

                List<string> gameplayWip = def.GetAsList<string>("gameplayWip");
                for (int i = 0; i < gameplayWip.Count; i++)
                {
                    if (!string.IsNullOrEmpty(gameplayWip[i]))
                    {
                        EditorUtility.DisplayProgressBar("Loading Scenes for " + sku + "...", "Loading WIP scenes: " + gameplayWip[i] + "...", (float)i / (float)gameplayWip.Count);
                        LevelEditor.settings.selectedMode = GetModeByName(gameplayWip[i]);
                        if (!onlyArt || gameplayWip[i].StartsWith("ART"))
                        {
                            OnLoadLevel(gameplayWip[i] + ".unity");
                        }
                    }
                }
            }

            if (area)
            {
                List<string> areaScenes = new List<string>();

                if (id < firstArea)
                {
                    int areaIndex = 1;
                    bool _continue = false;
                    do
                    {
                        areaScenes.Clear();
                        areaScenes = def.GetAsList<string>("area" + areaIndex);
							// Seasonal
						List<string> seasonal = def.GetAsList<string>("area" + areaIndex + "_seasonal");
						areaScenes.AddRange( seasonal );

							// Scenes by quality level
						string[] qualityLevels = {"_low", "_mid", "_high"};
						for( int i = 0; i<qualityLevels.Length; i++ )
						{
							List<string> qualityScenes = def.GetAsList<string>("area" + areaIndex + qualityLevels[i]);
							areaScenes.AddRange( qualityScenes );
						}

                        _continue = false;
                        for (int i = 0; i < areaScenes.Count; i++)
                        {
                            EditorUtility.DisplayProgressBar("Loading Scenes for " + sku + "...", "Loading scenes for Area " + areaIndex + ": " + areaScenes[i] + "...", (float)i / (float)areaScenes.Count);
                            if (!string.IsNullOrEmpty(areaScenes[i]))
                            {
                                _continue = true;
                                LevelEditor.settings.selectedMode = GetModeByName(areaScenes[i]);
                                if (!onlyArt || areaScenes[i].StartsWith("ART"))
                                {
                                    OnLoadLevel(areaScenes[i] + ".unity");
                                }
                            }
                        }
                        areaIndex++;
                    } while (_continue && id < 3);
                }
                else
                {
                    id = id - firstArea - 1;
                    areaScenes = m_sceneAreas[id];
                    for (int i = 0; i < areaScenes.Count; i++)
                    {
                        EditorUtility.DisplayProgressBar("Loading Scenes for " + sku + "...", "Loading scenes for Area " + (id + 1) + ": " + areaScenes[i] + "...", (float)i / (float)areaScenes.Count);
                        if (!string.IsNullOrEmpty(areaScenes[i]))
                        {
                            LevelEditor.settings.selectedMode = GetModeByName(areaScenes[i]);
                            if (!onlyArt || areaScenes[i].StartsWith("ART"))
                            {
                                OnLoadLevel(areaScenes[i] + ".unity");
                            }
                        }
                    }
                }
            }

			// Hide progress bar!
			EditorUtility.ClearProgressBar();

			// Start with collapsed hierarchy
			HierarchyCollapser.CollapseHierarchy();

			LevelEditor.settings.selectedMode = oldMode;
		}

		LevelEditorSettings.Mode GetModeByName( string name )
		{
			LevelEditorSettings.Mode mode = LevelEditorSettings.Mode.COUNT;
			string lower = name.ToLower();
			if ( lower.StartsWith("art_") )
			{
				mode = LevelEditorSettings.Mode.ART;
			}
			else if ( lower.StartsWith("sp_") )
			{
				mode = LevelEditorSettings.Mode.SPAWNERS;
			}
			else if (lower.StartsWith("so_"))
			{
				mode = LevelEditorSettings.Mode.SOUND;
			}
			else if ( lower.StartsWith("co_") )
			{
				mode = LevelEditorSettings.Mode.COLLISION;
			}
			return mode;
		}

		/// <summary>
		/// Close all currently open scenes.
		/// </summary>
		private void OnCloseAllScenes() {
			UnloadAllLevels();
		}


		/// <summary>
		/// The "Open" button has been pressed.
		/// </summary>
		private void OnOpenLevelButton() {
			// Open a dialog showing all the levels stored in resources
			// Refresh the list
			string dirPath = Application.dataPath + assetDirForCurrentMode.Replace("Assets", "");	// dataPath already contains the "Assets" directory
			DirectoryInfo dirInfo = new DirectoryInfo(dirPath);
			m_fileList = dirInfo.GetFiles("*.unity");	// Levels are scenes

			// Strip filename from full file path
			string[] fileNames = new string[m_fileList.Length];
			for(int i = 0; i < fileNames.Length; i++) {
				fileNames[i] = Path.GetFileNameWithoutExtension(m_fileList[i].Name);
			}

			// Open a popup displaying all the files
			if(fileNames.Length > 0) {
				// Show selection popup
				SelectionPopupWindow.Show(fileNames, OnLoadLevelSelected);
			}
			
			// If there are no saved levels whatsoever, show a notification instead
			else {
				LevelEditorWindow.instance.ShowNotification(new GUIContent("There are no saved levels"));
			}
		}

		/// <summary>
		/// On Add level button event
		/// </summary>
		private void OnAddLevelButton(){
			// Open a dialog showing all the levels stored in resources
			// Refresh the list
			string dirPath = Application.dataPath + assetDirForCurrentMode.Replace("Assets", "");	// dataPath already contains the "Assets" directory
			DirectoryInfo dirInfo = new DirectoryInfo(dirPath);
			m_fileList = dirInfo.GetFiles("*.unity");	// Levels are scenes

			// Strip filename from full file path
			string[] fileNames = new string[m_fileList.Length];
			for(int i = 0; i < fileNames.Length; i++) {
				fileNames[i] = Path.GetFileNameWithoutExtension(m_fileList[i].Name);
			}

			// Open a popup displaying all the files
			if(fileNames.Length > 0) {
				// Show selection popup
				SelectionPopupWindow.Show(fileNames, OnAddLevelSelected);
			}
			
			// If there are no saved levels whatsoever, show a notification instead
			else {
				LevelEditorWindow.instance.ShowNotification(new GUIContent("There are no saved levels"));
			}
		}
		
		/// <summary>
		/// The "Save" button has been pressed.
		/// </summary>
		private void OnSaveLevelButton() {
			// Just do it
			SaveLevel();
		}
		
		/// <summary>
		/// The "Close" button has been pressed.
		/// </summary>
		private void OnCloseLevelButton() {
			// Just do it - ask for saving first
			UnloadLevel(true);
		}
		
		/// <summary>
		/// The "Delete" button has been pressed.
		/// </summary>
		private void OnDeleteLevelButton() {
			// Show confirmation dialog
			if(EditorUtility.DisplayDialog("Delete Level", "Are you sure?", "Yes", "No")) {
				// Just do it
				List<Level> l = activeLevels;
				for( int i = 0; i<l.Count; i++ )
					AssetDatabase.MoveAssetToTrash(assetDirForCurrentMode + "/" + l[i].gameObject.scene.name + ".unity");

				// Unload level - don't prompt for saving, of course
				UnloadLevel(false);
			}
		}
		
		/// <summary>
		/// A level has been selected to be loaded.
		/// </summary>
		/// <param name="_selectedIdx">The index of the selected option.</param>
		public void OnLoadLevelSelected(int _selectedIdx) {
			// Check index (just in case)
			if(_selectedIdx < 0 || _selectedIdx >= m_fileList.Length) return;
			
			// Do it!!
			// Unload current level - will ask to save it
			UnloadLevel(true);

			OnLoadLevel( m_fileList[_selectedIdx].Name );
		}

		public void OnLoadLevel( string levelName )
		{
			// Load the new level scene and store reference to the level object
			Debug.Log(levelName + " loaded");
			EditorSceneManager.OpenScene(assetDirForCurrentMode + "/" + levelName, OpenSceneMode.Additive);
			activeLevels = new List<Level>();
			switch(LevelEditor.settings.selectedMode) {
				case LevelEditorSettings.Mode.SPAWNERS:		activeLevels.AddRange(Object.FindObjectsOfType<LevelTypeSpawners>());		break;
				case LevelEditorSettings.Mode.COLLISION:	activeLevels.AddRange(Object.FindObjectsOfType<LevelTypeCollision>());	break;
				case LevelEditorSettings.Mode.ART:			activeLevels.AddRange(Object.FindObjectsOfType<LevelTypeArt>());			break;
				case LevelEditorSettings.Mode.SOUND:		activeLevels.AddRange(Object.FindObjectsOfType<LevelTypeSound>());			break;
			}

			// Level Type component must be on the scene!
			if(activeLevels.Count > 0) {
				// Focus the level object in the hierarchy and ping the opened scene in the project window
				Selection.activeObject = activeLevels[0].gameObject;
				for( int i = 0; i<activeLevels.Count;i++ )
					EditorGUIUtility.PingObject(AssetDatabase.LoadMainAssetAtPath(assetDirForCurrentMode + "/" + activeLevels[i].gameObject.scene.name + ".unity"));
			} else {
				Debug.Log("<color=yellow>Didn't find LevelType component in scene " + levelName + "!</color>");
			}
		}

		/// <summary>
		/// A level has been selected to be added.
		/// </summary>
		/// <param name="_selectedIdx">The index of the selected option.</param>
		public void OnAddLevelSelected(int _selectedIdx) {
			// Check index (just in case)
			if(_selectedIdx < 0 || _selectedIdx >= m_fileList.Length) return;
			
			// Do it!!
			activeLevels.Clear();

			OnLoadLevel( m_fileList[_selectedIdx].Name );

		}


	}
}