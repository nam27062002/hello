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
using System.IO;
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
		}

		//------------------------------------------------------------------//
		// MEMBERS															//
		//------------------------------------------------------------------//
		// Sections
		private ILevelEditorSection[] m_sections = new ILevelEditorSection[] {
			new SectionLevels(),
			new SectionGroups(),
			new SectionGround(),
			new SectionSpawners(),
			new SectionDecos(),
			new SectionDummies()
		};

		private Dictionary<string, ILevelEditorSection> m_tabs = new Dictionary<string, ILevelEditorSection>();

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
		public SectionLevels sectionLevels { get { return m_sections[0] as SectionLevels; }}
		public SectionGroups sectionGroups { get { return m_sections[1] as SectionGroups; }}
		public SectionGround sectionGround { get { return m_sections[2] as SectionGround; }}
		public SectionSpawners sectionSpawners { get { return m_sections[3] as SectionSpawners; }}
		public SectionDecos sectionDecos { get { return m_sections[4] as SectionDecos; }}
		public SectionDummies sectionDummies { get { return m_sections[5] as SectionDummies; }}

		// Styles shortcut
		public Styles styles { get { return m_styles; }}

		//------------------------------------------------------------------//
		// GENERIC METHODS													//
		//------------------------------------------------------------------//
		public LevelEditorWindow() {
			// Initialize tabs dictionary
			m_tabs.Add("Spawners", sectionSpawners);
			m_tabs.Add("Collisions", sectionGround);
			m_tabs.Add("Decorations", sectionDecos);
			m_tabs.Add("Dummies", sectionDummies);
		}

		/// <summary>
		/// The window has been enabled - similar to the constructor.
		/// </summary>
		public void OnEnable() {
			// We must detect when application goes to play mode and back, so subscribe to the event
			EditorApplication.playmodeStateChanged += OnPlayModeChanged;

			// Make sure we parse the scene properly the first time
			Init();
		}

		/// <summary>
		/// The window has been disabled - similar to the destructor.
		/// </summary>
		public void OnDisable() {
			// Remove editor scene
			CloseLevelEditorScene();

			// Unsubscribe from the event
			EditorApplication.playmodeStateChanged -= OnPlayModeChanged;

			// Clear instance reference
			m_instance = null;
		}

		/// <summary>
		/// Called 100 times per second on all visible windows.
		/// </summary>
		public void Update() {
			// If loaded scene has changed, check it.
			if(EditorApplication.currentScene != m_sceneName) {
				m_sceneName = EditorApplication.currentScene;
				Init();
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
			m_sceneName = EditorApplication.currentScene;

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
			if(levelEditorScene == null || !levelEditorScene.isLoaded) {
				EditorSceneManager.OpenScene(EDITOR_SCENE_PATH, OpenSceneMode.Additive);
			}
		}
		
		/// <summary>
		/// Unloads the level editor specific stuff.
		/// </summary>
		public void CloseLevelEditorScene() {
			// Just do it
			Scene levelEditorScene = EditorSceneManager.GetSceneByPath(EDITOR_SCENE_PATH);
			if(levelEditorScene != null) {
				bool success = EditorSceneManager.CloseScene(levelEditorScene, true);
			}
		}

		//------------------------------------------------------------------//
		// WINDOW METHODS													//
		//------------------------------------------------------------------//
		/// <summary>
		/// Update the inspector window.
		/// </summary>
		public void OnGUI() {
			// Initialize styles - must be done during the OnGUI call
			if(m_styles == null) {
				InitStyles();
			}

			// Reset indentation
			EditorGUI.indentLevel = 0;

			EditorGUILayout.BeginVertical(GUILayout.ExpandHeight(true)); {
				// Mode selector
				LevelEditorSettings.Mode newMode = (LevelEditorSettings.Mode)GUILayout.Toolbar((int)LevelEditor.settings.selectedMode, new string[] {"SPAWNERS", "COLLISION", "ART" }, GUILayout.Height(50));
				if(newMode != LevelEditor.settings.selectedMode) {
					// Mode changed! Do whatever needed
					LevelEditor.settings.selectedMode = newMode;
					LevelEditor.settings.selectedTab = 0;	// Reset selected tab
					AssetDatabase.SaveAssets();	// Record settings
				}

				// Draw sections
				// Level section - always drawn
				sectionLevels.OnGUI();

				// Rest of the sections: only if a level is loaded and we're not testing
				if(sectionLevels.activeLevel != null && !EditorApplication.isPlaying) {
					// Groups section
					GUILayout.Space(10f);
					sectionGroups.OnGUI();

					// Tabbed sections
					GUILayout.Space(10f);
					EditorGUILayout.BeginVertical(styles.boxStyle); {
						// Tab selector - different options based on mode
						List<string> tabNames = new List<string>();
						switch(LevelEditor.settings.selectedMode) {
							case LevelEditorSettings.Mode.SPAWNERS:		tabNames = new List<string>(new string[] { "Spawners", "Dummies" });	break;
							case LevelEditorSettings.Mode.COLLISION:	tabNames = new List<string>(new string[] { "Collisions" });				break;
							case LevelEditorSettings.Mode.ART:			tabNames = new List<string>(new string[] { "Decorations" });			break;
						}
						int newTab = GUILayout.Toolbar(LevelEditor.settings.selectedTab, tabNames.ToArray());

						// If tab has changed, Init new tab
						if(newTab != LevelEditor.settings.selectedTab) {
							m_tabs[tabNames[newTab]].Init();
							LevelEditor.settings.selectedTab = newTab;
							AssetDatabase.SaveAssets();	// Record settings
						}

						// Tab content
						EditorGUILayout.BeginVertical(styles.boxStyle, GUILayout.ExpandHeight(true)); {
							m_tabs[tabNames[newTab]].OnGUI();
						} EditorGUILayoutExt.EndVerticalSafe();
					} EditorGUILayoutExt.EndVerticalSafe();
				}
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
				
				Texture2D selectedTexture = Texture2DExt.Create(2, 2, Colors.gray);
				Texture2D idleTexture = Texture2DExt.Create(2, 2, Colors.transparentBlack);
				
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
				
				Texture2D whiteTexture = Texture2DExt.Create(2, 2, Colors.white);
				
				newStyle.normal.background = whiteTexture;
				newStyle.onNormal.background = whiteTexture;
				
				m_styles.whiteScrollListStyle = newStyle;
			}
			
			{ // Background boxes
				m_styles.boxStyle = new GUIStyle(EditorStyles.helpBox);
				m_styles.boxStyle.padding = new RectOffset(10, 10, 10, 10);
			}
		}

		//------------------------------------------------------------------//
		// CALLBACKS														//
		//------------------------------------------------------------------//
		/// <summary>
		/// The application is being played or stopped.
		/// </summary>
		public void OnPlayModeChanged() {
			// Window is recreated and all values are lost, so re-init everything
			Init();

			// Make sure we have the level editor stuff loaded
			OpenLevelEditorScene();

			// Force a repaint
			Repaint();
		}
	}
}