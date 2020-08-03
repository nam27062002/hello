// MenuScreensControllerEditor.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 25/02/2016.
// Copyright (c) 2016 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;
using System;
using System.Collections.Generic;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// Auxiliar class to show a toolbar to quickly select editing screen on the menu scene.
/// From http://docs.unity3d.com/Manual/RunningEditorCodeOnLaunch.html
/// and http://answers.unity3d.com/questions/19321/onscenegui-called-without-any-object-selected.html
/// </summary>
[InitializeOnLoad]
public class MenuScreensControllerToolbar {
	//----------------------------------------------------------------------//
	// CONSTANTS															//
	//----------------------------------------------------------------------//
	private class SelectionAction {
		public UnityEngine.Object obj = null;
		public bool focus = true;
		public bool ping = true;

		public SelectionAction(UnityEngine.Object _obj, bool _focus, bool _ping) {
			obj = _obj;
			focus = _focus;
			ping = _ping;
		}

		public void Select() {
			EditorUtils.FocusObject(obj, true, focus, ping);
		}
	}

	private class ScreensGroup {
		public string key = "";
		public List<MenuScreen> screens = new List<MenuScreen>();
		public string displayName = "";

		public ScreensGroup(string _key, string _displayName) {
			key = _key;
			displayName = _displayName;
		}
	}

	// Screen groups
	private enum EScreensGroup {
		MAIN_SCREENS,
		GOALS_SCREENS,
		REWARD_SCREENS,
		TOURNAMENT_SCREENS,

		COUNT
	}

	// Other consts
	private static readonly Color BACKGROUND_COLOR = new Color(0.25f, 0.25f, 0.25f, 0.75f);
	private static readonly Vector2 FOLD_BUTTON_SIZE = new Vector2(30f, 100f);

	//----------------------------------------------------------------------//
	// MEMBERS																//
	//----------------------------------------------------------------------//
	// Internal vars
	private static MenuTransitionManager s_transitionManager = null;
	private static Queue<SelectionAction> s_pendingSelections = new Queue<SelectionAction>();
	private static int s_frameCount = 0;
	private static ScreensGroup[] s_screenGroups = new ScreensGroup[(int)EScreensGroup.COUNT];

	// Layouting
	private static float s_maxWidth = 0f;

	// Persisting properties
	private const string EXPANDED_KEY = "MenuScreensControllerToolbar.expanded";
	private static bool expanded {
		get { return Prefs.GetBoolEditor(EXPANDED_KEY); }
		set { Prefs.SetBoolEditor(EXPANDED_KEY, value); }
	}

	private const string SCROLL_POS_KEY = "MenuScreensControllerToolbar.scrollPos";
	private static Vector2 scrollPos {
		get { return Prefs.GetVector2Editor(SCROLL_POS_KEY); }
		set { Prefs.SetVector2Editor(SCROLL_POS_KEY, value); }
	}

	//----------------------------------------------------------------------//
	// METHODS																//
	//----------------------------------------------------------------------//
	/// <summary>
	/// Static constructor.
	/// </summary>
	static MenuScreensControllerToolbar() {
		// Subscribe to external events
		SceneView.onSceneGUIDelegate += OnSceneGUI;

		// Inexplicably, Unity hasn't included these events until version 5.6
		#if UNITY_5_6_OR_NEWER
		EditorSceneManager.sceneOpened += ((Scene _scene, OpenSceneMode _mode) => {
			FindMenuScreensController();
		});
		#endif

		// Do a first processing of the scene
		FindMenuScreensController();

		// Initialize screen groups
		s_screenGroups = new ScreensGroup[(int)EScreensGroup.COUNT];
		s_screenGroups[(int)EScreensGroup.MAIN_SCREENS] = new ScreensGroup("MenuScreensControllerToolbar.MainScreensExpanded", "Main Screens");
		s_screenGroups[(int)EScreensGroup.GOALS_SCREENS] = new ScreensGroup("MenuScreensControllerToolbar.GoalsScreensExpanded", "Goals Screens");
		s_screenGroups[(int)EScreensGroup.REWARD_SCREENS] = new ScreensGroup("MenuScreensControllerToolbar.RewardScreensExpanded", "Reward Screens");
		s_screenGroups[(int)EScreensGroup.TOURNAMENT_SCREENS] = new ScreensGroup("MenuScreensControllerToolbar.TournamentScreensExpanded", "Tournament Screens");
		for(MenuScreen scr = MenuScreen.PLAY; scr < MenuScreen.COUNT; ++scr) {
			switch(scr) {
				case MenuScreen.MISSIONS:
				case MenuScreen.CHESTS:
				case MenuScreen.GLOBAL_EVENTS:
                case MenuScreen.LEAGUES:
                    {
					s_screenGroups[(int)EScreensGroup.GOALS_SCREENS].screens.Add(scr);
				} break;

				case MenuScreen.OPEN_EGG:
				case MenuScreen.EVENT_REWARD:
				case MenuScreen.PENDING_REWARD:
				case MenuScreen.LEAGUES_REWARD: {
					s_screenGroups[(int)EScreensGroup.REWARD_SCREENS].screens.Add(scr);
				} break;

				case MenuScreen.TOURNAMENT_INFO:
				case MenuScreen.TOURNAMENT_DRAGON_SELECTION:
				case MenuScreen.TOURNAMENT_DRAGON_SETUP:
				case MenuScreen.TOURNAMENT_REWARD: {
					s_screenGroups[(int)EScreensGroup.TOURNAMENT_SCREENS].screens.Add(scr);
				} break;

				default: {
					s_screenGroups[(int)EScreensGroup.MAIN_SCREENS].screens.Add(scr);
				} break;
			}
		}

		// Init some other internal vars
		s_maxWidth = 0f;
	}

	/// <summary>
	/// Find the menu screens controller in the current hierarchy.
	/// </summary>
	private static void FindMenuScreensController() {
		// Look for the menu screens controller in the current hierarchy
		s_transitionManager = GameObject.FindObjectOfType<MenuTransitionManager>();

		// If there is no valid screens controller, clear pending selections
		if(s_transitionManager == null) s_pendingSelections.Clear();
	}

	/// <summary>
	/// Draw gui on the scene.
	/// </summary>
	/// <param name="_sceneview">The target scene.</param>
	private static void OnSceneGUI(SceneView _sceneview) {
		// Extra processing for older Unity versions: always check for the menu screens controller
		#if !UNITY_5_6_OR_NEWER
		if(s_transitionManager == null) {
			FindMenuScreensController();
		}
		#endif

		// Ignore if there is no Screen controller in the scene
		if(s_transitionManager == null) return;

		// http://answers.unity3d.com/questions/19321/onscenegui-called-without-any-object-selected.html
		// Initialize a GUI Handle in the scene view
		MenuScreen screenToEdit = MenuScreen.NONE;
		Handles.BeginGUI(); {
			// Compute size
			Rect viewport = SceneView.currentDrawingSceneView.camera.pixelRect;
			viewport = EditorGUIUtility.PixelsToPoints(viewport);	// [AOC] For Retina displays, convert units
			Rect size = new Rect(0, 0, s_maxWidth + 30, viewport.height);	// Add extra space for the scroll bar

			// Draw folding button
			// Setup as if it was not expanded
			bool isExpanded = expanded; // Cache to avoid consulting Prefs
			Rect foldButtonRect = new Rect(size.xMin, size.center.y - FOLD_BUTTON_SIZE.y * 0.5f, FOLD_BUTTON_SIZE.x, FOLD_BUTTON_SIZE.y);
			string foldButtonText = "►";
			if(isExpanded) {
				// Tune some stuff when expanded
				foldButtonRect.x = size.xMax - 5f;   // Adjust to right-most edge of the layout (overlap a bit)
				foldButtonText = "◄";
			}

			// Do it!
			if(GUI.Button(foldButtonRect, foldButtonText)) {
				// Update control var
				isExpanded = !isExpanded;
				expanded = isExpanded;	// Persist to Prefs
			}

			// If expanded, do buttons layout
			if(isExpanded) {
				// Initialize a layouting area
				GUI.backgroundColor = BACKGROUND_COLOR;
				GUILayout.BeginArea(size, CustomEditorStyles.simpleBox); {
					GUI.backgroundColor = Color.white;

					// Join in a full-height scroll view
					scrollPos = EditorGUILayout.BeginScrollView(scrollPos, false, true, GUIStyle.none, GUI.skin.verticalScrollbar, GUIStyle.none); {   // No horizontal scrollbar, always show vertical scrollbar
						// Create a vertical layout to fine-tune margins
						EditorGUILayout.BeginVertical(GUILayout.Width(size.width - 20f), GUILayout.Height(size.height - 25f)); {
							// Do groups
							for(int i = 0; i < (int)EScreensGroup.COUNT; i++) {
								DoGroupLayout(s_screenGroups[i], ref screenToEdit);
							}
						} EditorGUILayout.EndVertical();
					} EditorGUILayout.EndScrollView();
				} GUILayout.EndArea();
			}
		} Handles.EndGUI();

		// If the user has selected a screen to edit, react to it!
		if(screenToEdit != MenuScreen.NONE) {
			GoToScreen(screenToEdit);
		}

		// We can only do one selection action every X frames, so store them in a queue
		// Process the queue
		if(s_pendingSelections.Count > 0) {
			s_frameCount--;
			if(s_frameCount < 0) {
				// Do it!
				s_pendingSelections.Dequeue().Select();

				// If there are still pending actions, reset timer
				if(s_pendingSelections.Count > 0) s_frameCount = 10;
			}
		}
	}

	/// <summary>
	/// Select and focus a specific screen.
	/// </summary>
	/// <param name="_screen">Screen to focus.</param>
	private static void GoToScreen(MenuScreen _screen) {
		// Disable all screens except the target one and ping the target screen
		ScreenData scr = null;
		for(int i = 0; i < (int)MenuScreen.COUNT; i++) {
			scr = s_transitionManager.GetScreenData((MenuScreen)i);
			if(scr != null) {
				if(i == (int)_screen) {
					// Select and ping 3D scene in hierarchy
					if(scr.scene3d != null) {
						s_pendingSelections.Enqueue(new SelectionAction(scr.scene3d.gameObject, false, true));
					}

					// Select and ping screen in hierarchy, and focus scene view on it
					if(scr.ui != null) {
						scr.ui.gameObject.SetActive(true);
						s_pendingSelections.Enqueue(new SelectionAction(scr.ui.gameObject, true, true));
					}
				} else {
					if(scr.ui != null) scr.ui.gameObject.SetActive(false);
				}
			}
		}

		// Move main camera to screen's snap point (if any)
		CameraSnapPoint targetSnapPoint = s_transitionManager.GetScreenData(_screen).cameraSetup;
		if(targetSnapPoint != null) {
			targetSnapPoint.Apply(s_transitionManager.camera);
			s_transitionManager.camera.transform.position = targetSnapPoint.transform.position;
		}
	}

	/// <summary>
	/// Displays a screen group.
	/// </summary>
	/// <param name="_group">Group to be displayed.</param>
	/// <returns>If a screen button has been pressed, target screen..</returns>
	private static void DoGroupLayout(ScreensGroup _group, ref MenuScreen _screenToEdit) {
		// Main Screens Foldable Group
		bool groupExpanded = Prefs.GetBoolEditor(_group.key, true);
		groupExpanded = EditorGUILayout.Foldout(groupExpanded, _group.displayName);
		Prefs.SetBoolEditor(_group.key, groupExpanded);
		if(groupExpanded) {
			// Indent in
			EditorGUI.indentLevel++;

			// Do a button for each screen in the group
			for(int i = 0; i < _group.screens.Count; ++i) {
				// Draw button
				// [AOC] Hardcode Hack to make buttons respect indentation
				EditorGUILayout.BeginHorizontal(); {
					// Figure out required width based on button's label
					GUIContent label = new GUIContent(_group.screens[i].ToString());
					float contentWidth = GUI.skin.button.CalcSize(label).x;

					// Figure out indentation size
					float indentationWidth = EditorGUI.IndentedRect(new Rect(0, 0, 100f, 10f)).x;

					// Update absolute max width var
					s_maxWidth = Mathf.Max(contentWidth + indentationWidth, s_maxWidth);

					// Space with indent size
					GUILayout.Space(indentationWidth);

					// Button
					EditorGUI.BeginDisabledGroup(label.text.Contains("EMPTY"));
					if(GUILayout.Button(label)) {
						// Save it as target screen!
						_screenToEdit = _group.screens[i];
					}
					EditorGUI.EndDisabledGroup();
				} EditorGUILayout.EndHorizontal();
			}

			// Indent out
			EditorGUI.indentLevel--;
		}
	}
}