// NewLevelEditorWindow.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 30/09/2015.
// Copyright (c) 2015 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.Collections;
using System.Collections.Generic;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
namespace LevelEditor {
	/// <summary>
	/// Custom editor windows to simplify Hungry Dragon's level design.
	/// Composed by several subsections.
	/// </summary>
	public class LevelEditorWindow : EditorWindow {
		//------------------------------------------------------------------//
		// CONSTANTS														//
		//------------------------------------------------------------------//
		private static readonly string EDITOR_SCENE_PATH = "Assets/Tools/LevelEditor/SC_LevelEditor.unity";

		public class Styles {
			public GUIStyle groupListStyle = null;	// Option style for a SelectionGrid element
			public GUIStyle whiteScrollListStyle = null;	// Scroll list with white background
			public GUIStyle boxStyle = null;	// Background boxes
			public GUIStyle sectionHeaderStyle = null;	// Section title, background + title + button
			public GUIStyle sectionContentStyle = null;	// Section content background
		}

		//------------------------------------------------------------------//
		// MEMBERS															//
		//------------------------------------------------------------------//
		// Sections
		private ILevelEditorSection[] m_sections = new ILevelEditorSection[] {
			new SectionLevels(),
			new SectionSimulation(),
			new SectionDragonSpawn(),
			new SectionLevelEditorConfig(),
            new SectionVertexDensity()
		};

		// Styles
		private Styles m_styles = null;	// Can't be initialized here

		// Level control
		private string m_sceneName = "";


        //------------------------------------------------------------------//
        // PROPERTIES														//
        //------------------------------------------------------------------//
        // Windows instance
        private static LevelEditorWindow m_instance = null;
		public static LevelEditorWindow instance {
			get {
				if(m_instance == null) {
					m_instance = (LevelEditorWindow)EditorWindow.GetWindow(typeof(LevelEditorWindow));
				}
				return m_instance;
			}
		}
		 
		// Section shortcuts - don't change order in the array!!
		public static SectionLevels sectionLevels { get { return instance.m_sections[0] as SectionLevels; }}
		public static SectionSimulation sectionSimulation { get { return instance.m_sections[1] as SectionSimulation; }}
		public static SectionDragonSpawn sectionDragonSpawn { get { return instance.m_sections[2] as SectionDragonSpawn; }}
		public static SectionLevelEditorConfig sectionParticleManager { get { return instance.m_sections[3] as SectionLevelEditorConfig; }}
        public static SectionVertexDensity sectionVertexDensity {  get { return instance.m_sections[4] as SectionVertexDensity; } }

		// Styles shortcut
		public static Styles styles { get { return instance.m_styles; }}

        //is throwing lightmap?
        public bool m_isLightmapping = false;
        public bool m_entireLightmap = false;


        //------------------------------------------------------------------//
        // GENERIC METHODS													//
        //------------------------------------------------------------------//
        public LevelEditorWindow() {
			
		}

		/// <summary>
		/// The window has been enabled - similar to the constructor.
		/// </summary>
		public void OnEnable() {
			// Make sure we have the latest definitions loaded
			ContentManager.InitContent(true, false);

			// We must detect when application goes to play mode and back, so subscribe to the event
			EditorSceneManager.sceneLoaded += OnSceneLoaded;
			EditorSceneManager.sceneUnloaded += OnSceneUnloadedClosed;
			EditorSceneManager.sceneOpened += OnSceneOpened;
			EditorSceneManager.sceneClosed += OnSceneUnloadedClosed;
			EditorApplication.playmodeStateChanged += OnPlayModeChanged;

			// Make sure we parse the scene properly the first time
			Init();

            m_isLightmapping = false;

        }

        /// <summary>
        /// The window has been disabled - similar to the destructor.
        /// </summary>
        public void OnDisable() {
			// Remove editor scene
			CloseLevelEditorScene();

			// Unsubscribe from the event
			EditorSceneManager.sceneLoaded -= OnSceneLoaded;
			EditorSceneManager.sceneUnloaded -= OnSceneUnloadedClosed;
			EditorSceneManager.sceneOpened -= OnSceneOpened;
			EditorSceneManager.sceneClosed -= OnSceneUnloadedClosed;
			EditorApplication.playmodeStateChanged -= OnPlayModeChanged;

			// Clear instance reference
			m_instance = null;
		}


		float delayedInitTimer = 0f;

		/// <summary>
		/// Called 100 times per second on all visible windows.
		/// </summary>
		public void Update() {
			// If loaded scene has changed, check it.
			if(EditorSceneManager.GetActiveScene().name != m_sceneName) {
				m_sceneName = EditorSceneManager.GetActiveScene().name;
				Init();
			}

			if (delayedInitTimer > 0f) {
				delayedInitTimer -= Time.deltaTime;
				if (delayedInitTimer <= 0f) {
					Init();
				}
			}

            if (m_entireLightmap)
            {
                if (!(m_sections[0] as SectionLevels).updateLightmap())
                {
                    m_entireLightmap = false;
                }
            }
        }

        /// <summary>
        /// OnInspectorUpdate is called at 10 frames per second to give the inspector a chance to update.
        /// Called less times as if it was OnGUI/Update
        /// </summary>
        public void OnInspectorUpdate() {
			// Make sure level editor scene is open
			OpenLevelEditorScene();

			// Force repainting while loading asset previews
			if(AssetPreview.IsLoadingAssetPreviews()) {
				Repaint();
			}
		}

		//------------------------------------------------------------------//
		// CUSTOM METHODS													//
		//------------------------------------------------------------------//
		/// <summary>
		/// Editor initialization.
		/// </summary>
		public void Init() {
			// Make sure we have enough room for asset previews
			AssetPreview.SetPreviewTextureCacheSize(500);	// Increase if problems happen

			// Store scene name
			m_sceneName = EditorSceneManager.GetActiveScene().name;

			// Init sections
			for(int i = 0; i < m_sections.Length; i++) {
				m_sections[i].Init();
			}

        }

		/// <summary>
		/// Load the level editor scene additively, provided it's not already loaded.
		/// </summary>
		public void OpenLevelEditorScene() {
			// If editor scene was not loaded, do it
			Scene levelEditorScene = EditorSceneManager.GetSceneByPath(EDITOR_SCENE_PATH);
			if(!levelEditorScene.isLoaded && !m_isLightmapping && !m_entireLightmap) {
				// Close any non-editable scenes
				CloseNonEditableScenes();

				// Open the scene
				EditorSceneManager.OpenScene(EDITOR_SCENE_PATH, OpenSceneMode.Additive);
			}
		}

        public Scene GetLevelEditorScene()
        {
            Scene levelEditorScene = EditorSceneManager.GetSceneByPath(EDITOR_SCENE_PATH);
            return levelEditorScene;
        }

        /// <summary>
        /// Unloads the level editor specific stuff.
        /// </summary>
        public void CloseLevelEditorScene() {
			// Just do it
			Scene levelEditorScene = EditorSceneManager.GetSceneByPath(EDITOR_SCENE_PATH);
			EditorSceneManager.CloseScene(levelEditorScene, true);
		}

		/// <summary>
		/// Close any open scene that doesn't have the "Level" component.
		/// </summary>
		public void CloseNonEditableScenes() {
			// [AOC] Alternative, bug-free version
			//		 Make sure at least that neither the SC_Loading, SC_Menu, SC_Game nor SC_Popups scenes are open to avoid conflicts with the LevelEditor
			// 		 Ask to save current scenes first
			EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
			string[] toCheck = new string[] { "SC_Loading", "SC_Menu", "SC_Game", "SC_Popups" };
			List<Scene> scenesToRemove = new List<Scene>();
			for(int i = 0; i < toCheck.Length; i++) {
				Scene sc = EditorSceneManager.GetSceneByName(toCheck[i]);
				if(sc.IsValid()) {
					scenesToRemove.Add(sc);
				}
			}



			// [AOC] DISABLE FOR NOW, LOOKS BUGGY
			/*
			// Close any "non-editable" open scene (aka no Level object)
			List<Scene> scenesToRemove = new List<Scene>();
			for(int i = 0; i < EditorSceneManager.sceneCount; i++) {
				Scene sc = EditorSceneManager.GetSceneAt(i);

				// Skip if it's the level editor scene!
				if(sc.path == EDITOR_SCENE_PATH) continue;

				// Look for the Level component at any point in the scene hierarchy
				Level level = null;
				foreach(GameObject go in sc.GetRootGameObjects()) {
					// Does this root object contain a Level component?
					level = go.FindComponentRecursive<Level>();
					if(level != null) {
						break;	// Check next scene
					}
				}

				// If the scene didn't contain any Level object, close it
				if(level == null) {
					scenesToRemove.Add(sc);
				}
			}*/

			// We always need at least one scene, so if none of the current scenes are valid, open the level editor scene before removing them
			if(scenesToRemove.Count == EditorSceneManager.sceneCount) {
				// Open the scene
				EditorSceneManager.OpenScene(EDITOR_SCENE_PATH, OpenSceneMode.Additive);
			}

			// Remove non-editable scenes
			for(int i = 0; i < scenesToRemove.Count; i++) {
				EditorSceneManager.CloseScene(scenesToRemove[i], true);
			}
		}

		//------------------------------------------------------------------//
		// WINDOW METHODS													//
		//------------------------------------------------------------------//
		/// <summary>
		/// Update the inspector window.
		/// </summary>
		public void OnGUI() {
			if (!ContentManager.m_ready)
				return;
			// Initialize styles - must be done during the OnGUI call
			if(m_styles == null) {
				InitStyles();
			}

			// Reset indentation
			EditorGUI.indentLevel = 0;

			EditorGUILayout.BeginVertical(GUILayout.ExpandHeight(true)); {
				// Mode selector
				LevelEditorSettings.Mode newMode = (LevelEditorSettings.Mode)GUILayout.Toolbar((int)LevelEditor.settings.selectedMode, new string[] {"SPAWNERS", "COLLISION", "ART", "SOUND" }, GUILayout.Height(50));
				if(newMode != LevelEditor.settings.selectedMode && newMode < LevelEditorSettings.Mode.COUNT) {
					// Mode changed! Do whatever needed
					LevelEditor.settings.selectedMode = newMode;
					LevelEditor.settings.selectedTab = 0;	// Reset selected tab
					AssetDatabase.SaveAssets();	// Record settings
				}

				// Draw sections
				// Level section
				sectionLevels.OnGUI();

				// Dragon spawn section
				sectionDragonSpawn.OnGUI();

				// Particle Manager
				sectionParticleManager.OnGUI();

				// Simulation section
				sectionSimulation.OnGUI();

                // Vertex density section
                sectionVertexDensity.OnGUI();
			} EditorGUILayoutExt.EndVerticalSafe();
		}

		//------------------------------------------------------------------//
		// INTERNAL METHODS													//
		//------------------------------------------------------------------//
		/// <summary>
		/// Initialize the custom styles used in this window.
		/// </summary>
		private void InitStyles() {
			// Create and initialize new struct instance
			m_styles = new Styles();
			
			{ // Group list elements
				GUIStyle newStyle = new GUIStyle(EditorStyles.miniButton);
				
				Texture2D selectedTexture = Texture2DExt.Create(Colors.gray);
				Texture2D idleTexture = Texture2DExt.Create(Colors.transparentBlack);
				
				newStyle.alignment = TextAnchor.MiddleLeft;
				
				newStyle.onActive.background = idleTexture;
				newStyle.active.background = idleTexture;
				
				newStyle.onHover.textColor = Color.white;
				newStyle.onHover.background = selectedTexture;
				newStyle.hover.background = idleTexture;
				
				newStyle.onNormal.textColor = Color.white;
				newStyle.onNormal.background = selectedTexture;
				newStyle.normal.background = idleTexture;
				
				m_styles.groupListStyle = newStyle;
			}
			
			{ // Scroll list white background
				GUIStyle newStyle = new GUIStyle(EditorStyles.helpBox);
				
				Texture2D whiteTexture = Texture2DExt.Create(Colors.white);
				
				newStyle.normal.background = whiteTexture;
				newStyle.onNormal.background = whiteTexture;
				
				m_styles.whiteScrollListStyle = newStyle;
			}
			
			{ // Background boxes
				m_styles.boxStyle = new GUIStyle(EditorStyles.helpBox);
				m_styles.boxStyle.padding = new RectOffset(10, 10, 10, 10);
			}

			{ // Section header style
				GUIStyle newStyle = new GUIStyle(EditorStyles.helpBox);

				newStyle.fontSize = EditorStyles.boldLabel.fontSize;
				newStyle.fontSize = 11;
				newStyle.fontStyle = FontStyle.Bold;
				newStyle.alignment = TextAnchor.MiddleLeft;
				newStyle.margin = new RectOffset(4, 4, 4, -1);	// No separation with content box

				m_styles.sectionHeaderStyle = newStyle;
			}

			{ // Section content style
				GUIStyle newStyle = new GUIStyle(m_styles.sectionHeaderStyle);

				newStyle.margin = new RectOffset(4, 4, -1, 4);	// No separation with header box
				newStyle.padding = new RectOffset(10, 10, 10, 10);

				m_styles.sectionContentStyle = newStyle;
			}
		}

		//------------------------------------------------------------------//
		// CALLBACKS														//
		//------------------------------------------------------------------//
		public void OnSceneLoaded(Scene scene, LoadSceneMode mode) {
			delayedInitTimer = 1f;
		}

		public void OnSceneOpened(Scene _scene, OpenSceneMode _mode) {
			delayedInitTimer = 1f;
		}
		
		public void OnSceneUnloadedClosed(Scene _scene) {
			delayedInitTimer = 1f;
		}

		/// <summary>
		/// The application is being played or stopped.
		/// </summary>
		public void OnPlayModeChanged() {
			// Window is recreated and all values are lost, so re-init everything
			Init();

			// Make sure we have the level editor stuff loaded
			OpenLevelEditorScene();

			// If an art scene is opened, make it the main scene (so the lightning setup is the right one)
			// Except for Mac, where some lightning settings make Unity crash -_-
			// #if !UNITY_EDITOR_OSX
			// TODO MALH: Select proper scene from art
			List<Level> artLevel = sectionLevels.GetLevel(LevelEditorSettings.Mode.ART);
			if(artLevel != null && artLevel.Count !=0) {
				EditorSceneManager.SetActiveScene(artLevel[0].gameObject.scene);
			}
			// #endif

			// Force a repaint
			Repaint();
		}
	}
}