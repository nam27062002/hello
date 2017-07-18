// PersistenceProfilesEditorWindow.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 01/09/2015.
// Copyright (c) 2015 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// Custom inspector window to define profiles settings
/// </summary>
public class ProfilerEditorWindow : EditorWindow {
	//------------------------------------------------------------------//
	// CONSTANTS														//
	//------------------------------------------------------------------//
	public static readonly float PROFILE_LIST_COLUMN_WIDTH = 200f;
	public static readonly float PROFILE_VIEW_COLUMN_WIDTH = 400f;
	public static readonly float MIN_WINDOW_HEIGHT = 400f;
	public static readonly float SPACING = 5f;

	//------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES											//
	//------------------------------------------------------------------//
	// Windows instance
	private static ProfilerEditorWindow m_instance = null;
	public static ProfilerEditorWindow instance {
		get {
			if(m_instance == null) {
				m_instance = (ProfilerEditorWindow)EditorWindow.GetWindow(typeof(ProfilerEditorWindow));
			}
			return m_instance;
		}
	}			

	// View management
	private Vector2 m_viewScrollPos = Vector2.zero;

	// Custom styles
	private GUIStyle m_selectionGridStyle = null;
	private GUIStyle m_titleLabelStyle = null;
	private GUIStyle m_centeredLabelStyle = null;
    private GUIStyle m_warningLabelStyle = null;

    private string[] m_settingsLabels = new string[] { "settings" };

    private enum ESettingsToShow
    {
        None,
        Resources,
        Cache
    };

    private ESettingsToShow SettingsToShow { get; set; }

    //------------------------------------------------------------------//
    // GENERIC METHODS													//
    //------------------------------------------------------------------//
    /// <summary>
    /// Update the inspector window.
    /// </summary>
    public void OnGUI() {
        Entities_LoadCatalog();

        // Initialize custom styles
        InitStyles();

		// Reset indentation
		EditorGUI.indentLevel = 0;

		// 2 columns: Profile list and Profile editor
		EditorGUILayout.BeginHorizontal(); {
			// Some spacing at the start
			GUILayout.Space(SPACING);

			// Profile list column: composed by 3 elements - profile list, savegame list and current savegame
			EditorGUILayout.BeginVertical(GUILayout.Width(PROFILE_LIST_COLUMN_WIDTH), GUILayout.ExpandHeight(true)); {
				// Some spacing
				GUILayout.Space(SPACING);

				// 1) Profile List
				EditorGUILayout.BeginVertical(EditorStyles.helpBox); {
					// Label
					GUILayout.Label("In Resources", m_centeredLabelStyle);
					
					// Spacing
					GUILayout.Space(SPACING);
						
					// Scroll - draw a box around it
					EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                    {
						EditorGUILayout.BeginScrollView(Vector2.zero);
                        {							
							// Detect selection change
							GUI.changed = false;
                            int index = (SettingsToShow == ESettingsToShow.Resources) ? 0 : -1;
							GUILayout.SelectionGrid(index, m_settingsLabels, 1, m_selectionGridStyle);
							if(GUI.changed)
                            {                                
                                SettingsToShow = ESettingsToShow.Resources;                                
							}
						}
                        EditorGUILayout.EndScrollView();
					}
                    EditorGUILayout.EndVertical();
				}
                EditorGUILayout.EndVertical();

				// Some spacing				
				GUILayout.Space(SPACING);

				// 3) Savegame List
				EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                {
					// Label
					GUILayout.Label("In cache", m_centeredLabelStyle);

					// Info
					EditorGUILayout.HelpBox("Settings are also stored in cache so they can be changed when running on device", MessageType.Info);
					
					// Scroll - Create a box around it
					GUILayout.BeginVertical(EditorStyles.helpBox);
                    {
						EditorGUILayout.BeginScrollView(Vector2.zero);
                        {														
							// Detect selection change
							GUI.changed = false;
                            int index = (SettingsToShow == ESettingsToShow.Cache) ? 0 : -1;
                            GUILayout.SelectionGrid(index, m_settingsLabels, 1, m_selectionGridStyle);
							if(GUI.changed)
                            {                                                                								
                                SettingsToShow = ESettingsToShow.Cache;
                            }
						} EditorGUILayout.EndScrollView();
					} EditorGUILayout.EndVertical();
				} EditorGUILayout.EndVertical();

				// Some spacing
				GUILayout.Space(SPACING);
			} EditorGUILayout.EndVertical();

			////////////////////////////////////////////////////////////////////////////////////////////////////////////

			// Profile view column
			EditorGUILayout.BeginVertical(GUILayout.Width(PROFILE_VIEW_COLUMN_WIDTH));
            {
				// Some spacing
				GUILayout.Space(SPACING);

				// Add scroll - within a box
				EditorGUILayout.BeginVertical(EditorStyles.helpBox, GUILayout.ExpandHeight(true), GUILayout.ExpandWidth(true));
                {
					m_viewScrollPos = EditorGUILayout.BeginScrollView(m_viewScrollPos, GUILayout.ExpandHeight(true), GUILayout.ExpandWidth(true));
                    {
						switch (SettingsToShow)
                        {
                            case ESettingsToShow.Resources:
                            {
                                ShowSettingsInResources();
                                break;
                            }

                            case ESettingsToShow.Cache:
                            {
                                ShowSettingsInCache();
                                break;
                            }
                        }						
					} EditorGUILayout.EndScrollView();
				}	EditorGUILayout.EndVertical();                

                // Some spacing
                GUILayout.Space(SPACING);
			} EditorGUILayout.EndVertical();

			// Some spacing at the end
			GUILayout.Space(SPACING);
		} EditorGUILayout.EndHorizontal();
	}

	/// <summary>
	/// Window has gotten focus.
	/// </summary>
	private void OnFocus() 
	{
		// Load all profiles in the resources folder and the list of savegames
		// [AOC] Probably it's not optimal to do this every time, but we want to detect any change performed directly on the inspector
		ReloadData();
	}

	/// <summary>
	/// Window has lost focus.
	/// </summary>
	private void OnLostFocus() 
	{        
	}

	//------------------------------------------------------------------//
	// INTERNAL UTILS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Initialize custom styles.
	/// </summary>
	private void InitStyles() {
		// Selection grid
		if(m_selectionGridStyle == null) {
			Texture2D selectedTexture = Texture2DExt.Create(Colors.gray);
			Texture2D idleTexture = Texture2DExt.Create(Colors.transparentBlack);

			m_selectionGridStyle = new GUIStyle(EditorStyles.miniButton);
			m_selectionGridStyle.onActive.background = idleTexture;
			m_selectionGridStyle.active.background = idleTexture;
			
			m_selectionGridStyle.onHover.textColor = Color.white;
			m_selectionGridStyle.onHover.background = selectedTexture;
			m_selectionGridStyle.hover.background = idleTexture;
			
			m_selectionGridStyle.onNormal.textColor = Color.white;
			m_selectionGridStyle.onNormal.background = selectedTexture;
			m_selectionGridStyle.normal.background = idleTexture;
		}
		
		// Title label
		if(m_titleLabelStyle == null) {
			m_titleLabelStyle = new GUIStyle(EditorStyles.largeLabel);
			m_titleLabelStyle.fontStyle = FontStyle.Bold;
			m_titleLabelStyle.fontSize = 14;
			m_titleLabelStyle.fixedHeight = m_titleLabelStyle.lineHeight * 1.25f;
			m_titleLabelStyle.alignment = TextAnchor.MiddleCenter;
		}
		
		// Centered Label
		if(m_centeredLabelStyle == null) {
			m_centeredLabelStyle = new GUIStyle(EditorStyles.label);
			m_centeredLabelStyle.alignment = TextAnchor.MiddleCenter;
			m_centeredLabelStyle.fontStyle = FontStyle.Bold;            
        }

        // Warning Label
        if (m_warningLabelStyle == null)
        {
            m_warningLabelStyle = new GUIStyle(EditorStyles.label);
            m_warningLabelStyle.alignment = TextAnchor.MiddleCenter;
            m_warningLabelStyle.fontStyle = FontStyle.Bold;
            m_warningLabelStyle.normal.textColor = Color.red;
        }
    }

	/// <summary>
	/// Show the current selected profile into the view column.
	/// </summary>
	private void ShowSettingsInResources()
    {		
		// Name - Draw a big, juicy title showing the name of the profile
		GUILayout.Label("Settings in Resources", m_titleLabelStyle);
		GUILayout.Space(10f);

		// Make active button
		if(GUILayout.Button("Reset all data"))
        {
            ProfilerSettingsManager.ResetSettingsResources();	            	
		}

        ProfilerSettings settings = ProfilerSettingsManager.SettingsResources;
        if (settings != null)
        {
            SimpleJSON.JSONNode settingsJSON = settings.ToJSON();

            // Default profile is not allowed to be edited because it's generated from rules
            if (ShowSaveDataInfo(ref settingsJSON, true))
            {                
                ProfilerSettingsManager.SaveJSONToResources(settingsJSON);                                
            }
        }
    }

    private void ShowSettingsInCache()
    {       
        // Name - Draw a big, juicy title showing the name of the profile
        GUILayout.Label("Settings in Cache", m_titleLabelStyle);
        GUILayout.Space(10f);

        // Make active button
        if (GUILayout.Button("Reset to Resources"))
        {
            ProfilerSettingsManager.ResetCachedToResources();
        }

        ProfilerSettings settings = ProfilerSettingsManager.SettingsCached;
        if (settings != null)
        {
            SimpleJSON.JSONNode settingsJSON = settings.ToJSON();
            
            if (ShowSaveDataInfo(ref settingsJSON, true))
            {
                ProfilerSettingsManager.SaveJSONToCache(settingsJSON);
            }
        }
    }    

	/// <summary>
	/// Clears the current profiles dictionary and loads them from the resources folder.
	/// It also clears the current savegames list and gets it again, making sure there are exactly one savegame per 
	/// profile by creating new savegames and deleting old ones.
	/// </summary>
	private void ReloadData()
    {    
	}
	
	private bool ShowSaveDataInfo( ref SimpleJSON.JSONNode _data, bool _isEditable = true ) {
		bool ret = false;

		string value = _data.ToString();

        // SC
        if (_isEditable)
        {
            string result = GUILayout.TextArea(value);
            if (!result.Equals(value))
            {
                try
                {
                    SimpleJSON.JSONClass parsed = SimpleJSON.JSONNode.Parse(result) as SimpleJSON.JSONClass;
                    if (parsed != null)
                    {
                        _data = parsed;
                        ret = true;
                    }
                }
                catch (System.Exception)
                {
                    Debug.Log("Cannot parse json result");
                }
            }
        }
        else
        {
            GUILayout.Label(value);
        }

		return ret;
	}

    #region settings
    private ProfilerSettings settingsCached;
    private ProfilerSettings settingsResources;

    private void Settings_Load()
    {
        // Loads the settings from cache
    }
    #endregion

    #region entities
    // This section is responsible for loading all names of the prefabs of the entities

    private bool Entities_IsCatalogLoaded { get; set; }

    private void Entities_LoadCatalog()
    {
        //if (!ProfilerSettingsManager.IsLoaded)
        {            
			List<string> files = Entity.Entities_GetFileNames();
            ProfilerSettingsManager.Load(files);                     
            Entities_IsCatalogLoaded = true;
        }
    }
    #endregion
}