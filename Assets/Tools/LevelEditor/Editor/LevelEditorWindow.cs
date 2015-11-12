// NewLevelEditorWindow.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 30/09/2015.
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
		private ILevelEditorSection[] tabbedSections {
			get {
				return new ILevelEditorSection[] { sectionGround, sectionSpawners, sectionDecos, sectionDummies };
			}
		}

		// Styles shortcut
		public Styles styles { get { return m_styles; }}

		//------------------------------------------------------------------//
		// GENERIC METHODS													//
		//------------------------------------------------------------------//
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
		/// Load all the stuff specific from the level editor. If already loaded, it will be reloaded.
		/// </summary>
		public void LoadLevelEditorStuff() {
			// Make sure we don't have it twice
			UnloadLevelEditorStuff();
			
			// Load it as an additive scene into the current one
			EditorApplication.OpenSceneAdditive(EDITOR_SCENE_PATH);
		}
		
		/// <summary>
		/// Unloads the level editor specific stuff.
		/// </summary>
		public void UnloadLevelEditorStuff() {
			// Just destroy all objects with the level editor tag
			GameObject[] editorStuff = GameObject.FindGameObjectsWithTag(LevelEditor.TAG);
			for(int i = 0; i < editorStuff.Length; i++) {
				GameObject.DestroyImmediate(editorStuff[i]);
				editorStuff[i] = null;
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
						// Tab selector
						int newTab = GUILayout.Toolbar(LevelEditor.settings.selectedTab, new string[] {"Ground", "Spawners", "Decorations", "Dummies" });

						// If tab has changed, Init new tab
						if(newTab != LevelEditor.settings.selectedTab) {
							tabbedSections[newTab].Init();
						}
						LevelEditor.settings.selectedTab = newTab;

						// Tab content
						EditorGUILayout.BeginVertical(styles.boxStyle, GUILayout.ExpandHeight(true)); {
							tabbedSections[newTab].OnGUI();
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
			// This could be quite hardcore on performance, maybe do it less frequently
			GameObject[] editorStuff = GameObject.FindGameObjectsWithTag(LevelEditor.TAG);
			if(editorStuff.Length == 0) {
				LoadLevelEditorStuff();
			}

			// Force a repaint
			Repaint();
		}
	}
}