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
/// Custom inspector window to define different persistence profiles.
/// </summary>
public class PersistenceProfilesEditorWindow : EditorWindow {
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
	private static PersistenceProfilesEditorWindow m_instance = null;
	public static PersistenceProfilesEditorWindow instance {
		get {
			if(m_instance == null) {
				m_instance = (PersistenceProfilesEditorWindow)EditorWindow.GetWindow(typeof(PersistenceProfilesEditorWindow));
			}
			return m_instance;
		}
	}

	// Profiles management
	public static string m_selectedProfile = "";	// Static to keep it between window openings
	private Dictionary<string, SimpleJSON.JSONClass> m_profilePrefabs = new Dictionary<string, SimpleJSON.JSONClass>();
	public string m_newProfileName = "";
	private Vector2 m_profileListScrollPos = Vector2.zero;

	// Savegames management
	public static string m_selectedSavegame = "";	// Static to keep it between window openings
	private Dictionary<string, SimpleJSON.JSONClass> m_saveGames = new Dictionary<string,SimpleJSON.JSONClass>();
	private Vector2 m_savegameListScrollPos = Vector2.zero;

	// View management
	private Vector2 m_viewScrollPos = Vector2.zero;

	// Custom styles
	private GUIStyle m_selectionGridStyle = null;
	private GUIStyle m_titleLabelStyle = null;
	private GUIStyle m_centeredLabelStyle = null;
    private GUIStyle m_warningLabelStyle = null;

    //------------------------------------------------------------------//
    // GENERIC METHODS													//
    //------------------------------------------------------------------//
    /// <summary>
    /// Update the inspector window.
    /// </summary>
    public void OnGUI() {
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
					GUILayout.Label("Profiles", m_centeredLabelStyle);

					// Add button + name textfield
					EditorGUILayout.BeginHorizontal(); {
						// Name textfield
						m_newProfileName = EditorGUILayout.TextField(m_newProfileName);
						
						// Button
						if(GUILayout.Button("Add Profile")) 
						{
							// Create a new profile with the name at the textfield
							SimpleJSON.JSONClass newProfilePrefab = CreateNewProfile(m_newProfileName);
							
							// Was it successfully created?
							if(newProfilePrefab != null) 
							{
								m_profilePrefabs[ m_newProfileName ] = newProfilePrefab;

								PersistenceManager.SaveFromObject( m_newProfileName, newProfilePrefab);
								m_saveGames[ m_newProfileName ] = newProfilePrefab;	

								// Yes!!
								ResetSelection();
								m_selectedProfile = m_newProfileName;	// Make it the selected one
								m_newProfileName = "";	// Clear textfield
							}
						}
					} EditorGUILayout.EndHorizontal();
					
					// Spacing
					GUILayout.Space(SPACING);
						
					// Scroll - draw a box around it
					EditorGUILayout.BeginVertical(EditorStyles.helpBox); {
						m_profileListScrollPos = EditorGUILayout.BeginScrollView(m_profileListScrollPos); {
							// Profiles list
							// Generate labels
							string[] labels = new string[m_profilePrefabs.Count];
							int currentSelectionIdx = -1;
							int i = 0;
							foreach(string key in m_profilePrefabs.Keys) {
								labels[i] = key;
								if(key == m_selectedProfile) {
									currentSelectionIdx = i;
								}
								i++;
							}
							
							// Detect selection change
							GUI.changed = false;
							int newSelectionIdx = GUILayout.SelectionGrid(currentSelectionIdx, labels, 1, m_selectionGridStyle);
							if(GUI.changed) {
								// Save new selection
								ResetSelection();
								m_selectedProfile = labels[newSelectionIdx];
							}
						} EditorGUILayout.EndScrollView();
					} EditorGUILayout.EndVertical();
				} EditorGUILayout.EndVertical();

				// Some spacing
				GUILayout.Space(SPACING);

				// 2) Active Profile
				EditorGUILayout.BeginHorizontal(); {
					// Label
					GUILayout.Label("Active Profile:");

					// Dropdown menu
					// Generate label list and figure out current selected index
					string[] labels = new string[m_profilePrefabs.Count];
					int currentIdx = -1;
					int i = 0;
					foreach(string key in m_profilePrefabs.Keys) 
					{
						labels[i] = key;
						if(key == PersistenceManager.activeProfile) 
						{
							currentIdx = i;
						}
						i++;
					}

					// Detect selection change
					GUI.changed = false;
					int newIdx = EditorGUILayout.Popup(currentIdx, labels);
					if(GUI.changed) 
					{
						// Store new selected profile as active one
						PersistenceManager.activeProfile = labels[newIdx];
					}
				} EditorGUILayout.EndHorizontal();

				// Some spacing
				GUILayout.Space(SPACING);

				// 3) Savegame List
				EditorGUILayout.BeginVertical(EditorStyles.helpBox); {
					// Label
					GUILayout.Label("Saved Games", m_centeredLabelStyle);

					// Info
					EditorGUILayout.HelpBox("Every profile has a save game linked to it, automatically created when creating the profile. The save game can then be edited or reset to its linked profile values.", MessageType.Info);
					
					// Scroll - Create a box around it
					GUILayout.BeginVertical(EditorStyles.helpBox); {
						m_savegameListScrollPos = EditorGUILayout.BeginScrollView(m_savegameListScrollPos); {
							// Save games list
							// Generate labels and find selected index
							string[] labels = new string[m_saveGames.Count];
							int currentSelectionIdx = -1;
							int i = 0;
							foreach( string key in m_saveGames.Keys)
							{
								labels[i] = key;
								if(labels[i] == m_selectedSavegame) {
									currentSelectionIdx = i;
								}
								i++;
							}
							
							// Detect selection change
							GUI.changed = false;
							int newSelectionIdx = GUILayout.SelectionGrid(currentSelectionIdx, labels, 1, m_selectionGridStyle);
							if(GUI.changed) {
								// Save new selection
								ResetSelection();
								m_selectedSavegame = labels[newSelectionIdx];
							}
						} EditorGUILayout.EndScrollView();
					} EditorGUILayout.EndVertical();
				} EditorGUILayout.EndVertical();

				// Some spacing
				GUILayout.Space(SPACING);
			} EditorGUILayout.EndVertical();

			////////////////////////////////////////////////////////////////////////////////////////////////////////////

			// Profile view column
			EditorGUILayout.BeginVertical(GUILayout.Width(PROFILE_VIEW_COLUMN_WIDTH)); {
				// Some spacing
				GUILayout.Space(SPACING);

				// Add scroll - within a box
				EditorGUILayout.BeginVertical(EditorStyles.helpBox, GUILayout.ExpandHeight(true), GUILayout.ExpandWidth(true)); {
					m_viewScrollPos = EditorGUILayout.BeginScrollView(m_viewScrollPos, GUILayout.ExpandHeight(true), GUILayout.ExpandWidth(true)); {
						// Different logic for profiles and saved games
						if(m_selectedProfile != "") {
							ShowSelectedProfile();
						} else if(m_selectedSavegame != "") {
							ShowSelectedSavegame();
						} else {
							// Nothing to show
						}
					} EditorGUILayout.EndScrollView();
				}	EditorGUILayout.EndVertical();

                if (m_selectedProfile == PersistenceProfile.DEFAULT_PROFILE) {
                    GUILayout.Label("Generated from rules. You can't delete or edit it", m_warningLabelStyle);
                }

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
	private void ShowSelectedProfile() {
		// Some checks first
		if(m_selectedProfile == "") return;
		if(!m_profilePrefabs.ContainsKey(m_selectedProfile)) return;

		// Name - Draw a big, juicy title showing the name of the profile
		GUILayout.Label(m_selectedProfile + " Profile", m_titleLabelStyle);
		GUILayout.Space(10f);

		// Make active button
		if(GUILayout.Button("Make Active")) {
			PersistenceManager.activeProfile = m_selectedProfile;	// Savegames and profiles have the same name
		}

        // Delete button - only if allowed   
        bool delete = false;
        bool _isDefaultProfile = m_selectedProfile == PersistenceProfile.DEFAULT_PROFILE;

        GUI.enabled = true;
        // Default profile is not allowed to be deleted         
        if (!_isDefaultProfile)
        {
            delete = GUILayout.Button("Delete Profile");
        }
		GUI.enabled = true;
        
		if(delete) 
		{
			string saveToRemove = m_selectedProfile;
			m_profilePrefabs.Remove( saveToRemove );
			DeleteCurrentProfile();
			PersistenceManager.Clear(saveToRemove);
			m_saveGames.Remove(saveToRemove);
		}
			
		// If not deleted, draw profile content
		else {
			// Create a serialized object for this profile
			SimpleJSON.JSONClass profile =  m_profilePrefabs[m_selectedProfile];

            // Default profile is not allowed to be edited because it's generated from rules
			if (ShowSaveDataInfo( ref profile, !_isDefaultProfile))
			{
				m_profilePrefabs[m_selectedProfile] = profile;
				SaveProfile( m_selectedProfile, profile);
			}
		}
	}

	/// <summary>
	/// Show the current selected savegame into the view column.
	/// </summary>
	private void ShowSelectedSavegame() {
		// Some checks first
		if(m_selectedSavegame == "") return;
		if(!m_profilePrefabs.ContainsKey(m_selectedSavegame)) return;

		// Name - Draw a big, juicy title showing the name of the profile
		GUILayout.Label(m_selectedSavegame + " Saved Game", m_titleLabelStyle);
		GUILayout.Space(10f);

		// Make Active button
		// If game is running, reloads the game as well
		if(Application.isPlaying) {
			if(GUILayout.Button("Make Active and Apply (reloads the game)")) {
				PersistenceManager.activeProfile = m_selectedSavegame;	// Savegames and profiles have the same name
				FlowManager.Restart();
			}
		} else {
			if(GUILayout.Button("Make Active")) {
				PersistenceManager.activeProfile = m_selectedSavegame;	// Savegames and profiles have the same name
			}
		}

		// Clear button
		if(GUILayout.Button("Reset to Profile")) 
		{
			m_saveGames[ m_selectedSavegame ] = m_profilePrefabs[ m_selectedSavegame ];
			PersistenceManager.SaveFromObject( m_selectedSavegame, m_saveGames[ m_selectedSavegame ]);
		}

		// If not cleared, draw content
		else 
		{
            if (m_saveGames.ContainsKey(m_selectedSavegame))
            {
                SimpleJSON.JSONClass data = m_saveGames[m_selectedSavegame];
                if (ShowSaveDataInfo(ref data))
                {
                    m_saveGames[m_selectedSavegame] = data;
                    PersistenceManager.SaveFromObject(m_selectedSavegame, data);
                }
            }
		}
	}

	/// <summary>
	/// Clears the current profiles dictionary and loads them from the resources folder.
	/// It also clears the current savegames list and gets it again, making sure there are exactly one savegame per 
	/// profile by creating new savegames and deleting old ones.
	/// </summary>
	private void ReloadData() {
        // If definitions are not loaded, do it now
        if (!ContentManager.ready) ContentManager.InitContent(true);

        // Load all gameobjects in the target resources folder
        TextAsset[] prefabs = Resources.LoadAll<TextAsset>(PersistenceProfile.RESOURCES_FOLDER);

        SimpleJSON.JSONClass json;

        // Use a dictionary for easier access
        m_profilePrefabs.Clear();
		for(int i = 0; i < prefabs.Length; i++) {
            if (prefabs[i].name == PersistenceProfile.DEFAULT_PROFILE) {
                json = PersistenceManager.GetDefaultDataFromProfile(PersistenceProfile.DEFAULT_PROFILE);
            }
            else {
                json = SimpleJSON.JSONNode.Parse(prefabs[i].text) as SimpleJSON.JSONClass;
            }

			m_profilePrefabs.Add(prefabs[i].name, json );
		}

		// If the default profile doesn't exist, create it immediately
		if(!m_profilePrefabs.ContainsKey(PersistenceProfile.DEFAULT_PROFILE)) {
			// Profile doesn't exist! Create it
			CreateNewProfile(PersistenceProfile.DEFAULT_PROFILE);
		}

		// Make sure current selection is still valid
		if(!m_profilePrefabs.ContainsKey(m_selectedProfile)) {
			m_selectedProfile = "";
		}

		// Get save games list
		m_saveGames.Clear();
		string[] saveGameNames = PersistenceManager.GetSavedGamesList();        
		
		// Make sure we still have exactly one savegame for each profile
		// Make sure current selection is still valid
		// a) Check for savegames to be deleted
		for(int i = 0; i < saveGameNames.Length; i++) 
		{            
			if(!m_profilePrefabs.ContainsKey(saveGameNames[i])) 
			{         
				PersistenceManager.Clear(saveGameNames[i]);
			}
			else
			{	
				m_saveGames[ saveGameNames[i] ] = PersistenceManager.LoadToObject( saveGameNames[i]);	
			}
		}

		// b) Check for profiles needing a new savegame
		foreach(string key in m_profilePrefabs.Keys) 
		{
			if(!PersistenceManager.HasSavedGame(key)) 
			{
				PersistenceManager.SaveFromObject( key, m_profilePrefabs[ key ] );
			}
		}

	}

	/// <summary>
	/// Creates a new profile prefab with the given name and adds it to the dictionary.
	/// </summary>
	/// <returns>A reference to the newly created profile prefab, null if it couldn't be created.</returns>
	/// <param name="_name">The name to give to the new profile.</param>
	private SimpleJSON.JSONClass CreateNewProfile(string _name) {
		// Check that the given name is valid
		if(_name == "") {
			EditorUtility.DisplayDialog("Empty profile name", "Please enter a valid name for the new profile", "Ok");
			return null;
		}
		
		// Check that there is no profile with this name already
		if(m_profilePrefabs.ContainsKey(_name)) {
			EditorUtility.DisplayDialog("Profile already exists", "Another profile with this name already exists.\nPlease choos a different name.", "Ok");
			return null;
		}

		// Store new profile to the resources folder
		SimpleJSON.JSONClass newSaveData = null;
		try
		{
			newSaveData = PersistenceManager.GetDefaultDataFromProfile(PersistenceProfile.DEFAULT_PROFILE);
		}
		catch( System.Exception )
		{
			
		}
		if ( newSaveData == null )
			newSaveData = new SimpleJSON.JSONClass();

		SaveProfile( _name, newSaveData );

		// Add it to the dictionary
		m_profilePrefabs.Add(_name, newSaveData);

		return newSaveData;
	}

	private void SaveProfile( string _name, SimpleJSON.JSONClass _saveData)
	{
        // The default profile doesn't have to be saved to a json file because it's generated from rules
        if (_name != PersistenceProfile.DEFAULT_PROFILE)
        {
            string resourcesFolder = Application.dataPath + "/Resources/" + PersistenceProfile.RESOURCES_FOLDER;
            File.WriteAllText(resourcesFolder + "/" + _name + ".json", _saveData.ToString());
        }
	}

	/// <summary>
	/// Deletes the current profile.
	/// </summary>
	private void DeleteCurrentProfile() {
		// Should be simple
		AssetDatabase.MoveAssetToTrash("Assets/Resources/" + PersistenceProfile.RESOURCES_FOLDER + m_selectedProfile + ".json");
		ResetSelection();
	}

	/// <summary>
	/// Creates a new savegame for the given persistence profile. If one already exists, it will be overwritten.
	/// The newly created savegame will be initialized with the profile data.
	/// </summary>
	/// <param name="_name">The profile to which we want to create a new savegame.</param>
	/// <param name="_profile">The profile to which we want to create a new savegame.</param>
	private void CreateNewSavegame(string _name, SimpleJSON.JSONClass _profile) 
	{
		PersistenceManager.SaveFromObject(_name, _profile);
	}

	/// <summary>
	/// Unselect any profile/savegame and clear current focus.
	/// </summary>
	private void ResetSelection() {
		m_selectedProfile = "";		// Unselect any profile
		m_selectedSavegame = "";	// Unselect any savegame
		m_viewScrollPos = Vector2.zero;	// Reset scroll view
		GUI.FocusControl("");	// Lose focus on the name textfield
	}




	private bool ShowSaveDataInfo( ref SimpleJSON.JSONClass _data, bool _isEditable = true ) {
		bool ret = false;

		string value = _data.ToString();

        // SC
        if (_isEditable) {
            string result = GUILayout.TextArea(value);
            if (!result.Equals(value)) {
                try {
                    SimpleJSON.JSONClass parsed = SimpleJSON.JSONNode.Parse(result) as SimpleJSON.JSONClass;
                    if (parsed != null) {
                        _data = parsed;
                        ret = true;
                    }
                }
                catch (System.Exception) {
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
}