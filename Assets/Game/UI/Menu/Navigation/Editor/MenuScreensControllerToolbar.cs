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
	private enum Layout {
		VERTICAL,
		HORIZONTAL
	}
	private const Layout LAYOUT = Layout.VERTICAL;

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

		COUNT
	}

	// Other consts
	private const float MARGIN = 5f;
	private const float INDENT_SIZE = 10f;
	private static readonly Vector2 POSITION = new Vector2(5f, 5f);
	private static readonly Vector2 ELEMENT_SIZE = new Vector2(130f, 20f);
	private static readonly Color BACKGROUND_COLOR = new Color(0.25f, 0.25f, 0.25f, 0.75f);

	//----------------------------------------------------------------------//
	// MEMBERS																//
	//----------------------------------------------------------------------//
	// Internal vars
	private static MenuTransitionManager s_transitionManager = null;
	private static Queue<SelectionAction> s_pendingSelections = new Queue<SelectionAction>();
	private static int s_frameCount = 0;
	private static ScreensGroup[] s_screenGroups = new ScreensGroup[(int)EScreensGroup.COUNT];

	private static Rect s_rect = new Rect();
	private static Rect s_lastTotalRect = new Rect();
	private static float s_maxWidth = 0f;

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
		for(MenuScreen scr = MenuScreen.PLAY; scr < MenuScreen.COUNT; ++scr) {
			switch(scr) {
				case MenuScreen.MISSIONS:
				case MenuScreen.CHESTS:
				case MenuScreen.GLOBAL_EVENTS: {
					s_screenGroups[(int)EScreensGroup.GOALS_SCREENS].screens.Add(scr);
				} break;

				case MenuScreen.OPEN_EGG:
				case MenuScreen.EVENT_REWARD:
				case MenuScreen.PENDING_REWARD: {
					s_screenGroups[(int)EScreensGroup.REWARD_SCREENS].screens.Add(scr);
				} break;
				} break;

				default: {
					s_screenGroups[(int)EScreensGroup.MAIN_SCREENS].screens.Add(scr);
				} break;
			}
		}

		// Initialize rects
		s_rect = new Rect(POSITION.x + MARGIN, POSITION.y + MARGIN, ELEMENT_SIZE.x, ELEMENT_SIZE.y);
		s_lastTotalRect = new Rect(s_rect);
		s_lastTotalRect.x -= MARGIN;
		s_lastTotalRect.y -= MARGIN;
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
		if(Event.current.type == EventType.Layout) {
			s_maxWidth = 0f;
		} else if(Event.current.type == EventType.Repaint) {
			s_rect.width = s_maxWidth;
		}

		// Extra processing for older Unity versions: always check for the menu screens controller
		#if !UNITY_5_6_OR_NEWER
		if(s_transitionManager == null) {
			FindMenuScreensController();
		}
		#endif

		// Ignore if there is no Screen controller in the scene
		if(s_transitionManager == null) return;

		// http://answers.unity3d.com/questions/19321/onscenegui-called-without-any-object-selected.html
		MenuScreen screenToEdit = MenuScreen.NONE;
		Handles.BeginGUI(); {
			// Background - using last known total rect
			s_lastTotalRect.width += 2 * MARGIN;
			s_lastTotalRect.height += 2 * MARGIN;
			EditorGUI.DrawRect(s_lastTotalRect, BACKGROUND_COLOR);

			// In this particular case it's easier to just go with old school GUI calls
			// Reset position
			s_rect.x = POSITION.x + MARGIN;
			s_rect.y = POSITION.y + MARGIN;
			s_lastTotalRect.height = LAYOUT == Layout.HORIZONTAL ? s_rect.height : 0f;
			s_lastTotalRect.width = LAYOUT == Layout.VERTICAL ? s_rect.width : 0f;

			GUI.enabled = true;
			for(int i = 0; i < (int)EScreensGroup.COUNT; i++) {
				DoGroup(ref s_rect, s_screenGroups[i], ref screenToEdit);
			}
		} Handles.EndGUI();

		// If the user has selected a screen to edit, react to it!
		if(screenToEdit != MenuScreen.NONE) {
			// Disable all screens except the target one and ping the target screen
			ScreenData scr = null;
			for(int i = 0; i < (int)MenuScreen.COUNT; i++) {
				scr = s_transitionManager.GetScreenData((MenuScreen)i);
				if(scr != null) {
					if(i == (int)screenToEdit) {
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
			CameraSnapPoint targetSnapPoint = s_transitionManager.GetScreenData((MenuScreen)screenToEdit).cameraSetup;
			if(targetSnapPoint != null) {
				targetSnapPoint.Apply(s_transitionManager.camera);
				s_transitionManager.camera.transform.position = targetSnapPoint.transform.position;
			}
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
	/// Advance the position pointer.
	/// </summary>
	/// <param name="_rect">Rect.</param>
	/// <param name="_distance">Optionally define the distance to advance. Otherwise it will take the rect's size as reference.</param>
	private static void AdvancePos(ref Rect _rect, float _distance = -1f) {
		// Aux vars
		Rect viewport = SceneView.currentDrawingSceneView.camera.pixelRect;

		// Different layouts
		switch(LAYOUT) {
			case Layout.HORIZONTAL: {
				// Advance pos
				float toAdvance = _distance > 0 ? _distance : _rect.width;
				_rect.x += toAdvance;
				s_lastTotalRect.width = Mathf.Max(s_lastTotalRect.width, s_lastTotalRect.width + toAdvance);

				// New row if we go off-viewport
				if(_rect.x + _rect.width > viewport.xMax) {
					_rect.x = viewport.xMin + POSITION.x;
					_rect.y += _rect.height + POSITION.y;
					s_lastTotalRect.height = Mathf.Max(s_lastTotalRect.height, s_lastTotalRect.height + _rect.height + POSITION.y);
				}
			} break;

			case Layout.VERTICAL: {
				// Advance pos
				float toAdvance = _distance > 0 ? _distance : _rect.height;
				_rect.y += toAdvance;
				s_lastTotalRect.height = Mathf.Max(s_lastTotalRect.height, s_lastTotalRect.height + toAdvance);

				// New column if we go off-viewport
				if(_rect.y + _rect.height > viewport.yMax) {
					_rect.y = viewport.yMin + 5f;
					_rect.x += _rect.width + 5f;
					s_lastTotalRect.width = Mathf.Max(s_lastTotalRect.width, s_lastTotalRect.width + _rect.width + 5f);
				}
			} break;
		}
	}

	/// <summary>
	/// Displays a screen group.
	/// </summary>
	/// <param name="_pos">Cursor.</param>
	/// <param name="_group">Group to be displayed.</param>
	/// <returns>If a screen button has been pressed, target screen..</returns>
	private static void DoGroup(ref Rect _pos, ScreensGroup _group, ref MenuScreen _screenToEdit) {
		// Main Screens Foldable Group
		bool expanded = Prefs.GetBoolEditor(_group.key, true);
		expanded = EditorGUI.Foldout(_pos, expanded, _group.displayName);
		Prefs.SetBoolEditor(_group.key, expanded);
		AdvancePos(ref _pos);
		if(expanded) {
			// Indent in
			_pos.x += INDENT_SIZE;
			_pos.width -= INDENT_SIZE;

			// Do a button for each screen in the group
			for(int i = 0; i < _group.screens.Count; ++i) {
				// Figure out required element width
				GUIContent label = new GUIContent(_group.screens[i].ToString());
				s_maxWidth = Mathf.Max(GUI.skin.button.CalcSize(label).x, s_maxWidth);

				// Draw button
				if(GUI.Button(_pos, label)) {
					// Save it as target screen!
					_screenToEdit = _group.screens[i];
				}
				AdvancePos(ref _pos);
			}

			// Indent out
			_pos.x -= INDENT_SIZE;
			_pos.width += INDENT_SIZE;
		}
	}
}