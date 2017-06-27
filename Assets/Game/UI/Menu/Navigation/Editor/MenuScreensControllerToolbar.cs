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

	// Prefs keys
	private const string MAIN_SCREENS_EXPANDED_KEY = "MenuScreensControllerToolbar.MainScreensExpanded";
	private const string GOALS_SCREEN_EXPANDED_KEY = "MenuScreensControllerToolbar.GoalsScreenExpanded";

	// Other consts
	private const float INDENT_SIZE = 10f;

	//----------------------------------------------------------------------//
	// MEMBERS																//
	//----------------------------------------------------------------------//
	// Internal vars
	private static MenuScreensController s_screensController = null;
	private static Queue<SelectionAction> s_pendingSelections = new Queue<SelectionAction>();
	private static int s_frameCount = 0;

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
		EditorSceneManager.sceneLoaded += ((Scene _scene, OpenSceneMode _mode) => {
			FindMenuScreensController();
		});
		#endif

		// Do a first processing of the scene
		FindMenuScreensController();
	}

	/// <summary>
	/// Find the menu screens controller in the current hierarchy.
	/// </summary>
	private static void FindMenuScreensController() {
		// Look for the menu screens controller in the current hierarchy
		s_screensController = GameObject.FindObjectOfType<MenuScreensController>();

		// If there is no valid screens controller, clear pending selections
		if(s_screensController == null) s_pendingSelections.Clear();
	}

	/// <summary>
	/// Draw gui on the scene.
	/// </summary>
	/// <param name="_sceneview">The target scene.</param>
	private static void OnSceneGUI(SceneView _sceneview) {
		// Extra processing for older Unity versions: always check for the menu screens controller
		#if !UNITY_5_6_OR_NEWER
		if(s_screensController == null) {
			FindMenuScreensController();
		}
		#endif

		// Ignore if there is no Screen controller in the scene
		if(s_screensController == null) return;

		// http://answers.unity3d.com/questions/19321/onscenegui-called-without-any-object-selected.html
		int screenToEdit = -1;
		NavigationScreen tabToSelect = null;
		Handles.BeginGUI(); {
			// In this particular case it's easier to just go with old school GUI calls
			Rect rect = new Rect(5f, 5f, 130f, 20f);
			GUI.enabled = true;

			// Main Screens Foldable Group
			bool expanded = Prefs.GetBoolEditor(MAIN_SCREENS_EXPANDED_KEY, true);
			expanded = EditorGUI.Foldout(rect, expanded, "Main Screens");
			Prefs.SetBoolEditor(MAIN_SCREENS_EXPANDED_KEY, expanded);
			AdvancePos(ref rect);
			if(expanded) {
				// Indent in
				rect.x += INDENT_SIZE;

				// Do a button for each scene
				MenuScreens scr = MenuScreens.NONE;
				for(int i = 0; i < (int)MenuScreens.COUNT; i++) {
					scr = (MenuScreens)i;
					if(GUI.Button(rect, scr.ToString())) {
						// Save it as target screen!
						screenToEdit = i;
					}
					AdvancePos(ref rect);
				}

				// Indent out
				rect.x -= INDENT_SIZE;
			}

			// Goals Screen Tabs
			TabSystem goalTabs = s_screensController.GetScreen((int)MenuScreens.GOALS).GetComponentInChildren<TabSystem>();
			if(goalTabs != null) {
				// Foldable group
				expanded = Prefs.GetBoolEditor(GOALS_SCREEN_EXPANDED_KEY, true);
				expanded = EditorGUI.Foldout(rect, expanded, "Goals Screen Tabs");
				Prefs.SetBoolEditor(GOALS_SCREEN_EXPANDED_KEY, expanded);
				AdvancePos(ref rect);
				if(expanded) {
					// Indent in
					rect.x += INDENT_SIZE;

					// A button for each tab
					foreach(NavigationScreen tab in goalTabs.screens) {
						if(tab == null) continue;
						if(GUI.Button(rect, tab.name)) {
							// Store target screen and tab
							screenToEdit = (int)MenuScreens.GOALS;
							tabToSelect = tab;

							// Enable/Disable each tab
							foreach(NavigationScreen tab2 in goalTabs.screens) {
								// Only show target tab
								tab2.gameObject.SetActive(tab == tab2);
							}
						}
						AdvancePos(ref rect);
					}

					// Indent out
					rect.x -= INDENT_SIZE;
				}
			}
		} Handles.EndGUI();

		// If the user has selected a screen to edit, react to it!
		if(screenToEdit >= 0) {
			// Disable all screens except the target one and ping the target screen
			NavigationScreen scr = null;
			for(int i = 0; i < (int)MenuScreens.COUNT; i++) {
				scr = s_screensController.screens[i];
				if(scr != null) {
					if(i == screenToEdit) {
						// Select and ping 3D scene in hierarchy
						s_pendingSelections.Enqueue(new SelectionAction(s_screensController.scenes[i].gameObject, false, true));

						// Select and ping screen in hierarchy, and focus scene view on it
						scr.gameObject.SetActive(true);
						s_pendingSelections.Enqueue(new SelectionAction(scr.gameObject, true, true));
					} else {
						scr.gameObject.SetActive(false);
					}
				}
			}

			// Move main camera to screen's snap point (if any)
			CameraSnapPoint targetSnapPoint = s_screensController.GetCameraSnapPoint(screenToEdit);
			if(targetSnapPoint != null) {
				targetSnapPoint.Apply(s_screensController.GetComponent<MenuSceneController>().mainCamera);
			}
		}

		// Similarly, if a sub-tab has been chosen, select it (after the scene and screen!)
		if(tabToSelect != null) {
			s_pendingSelections.Enqueue(new SelectionAction(tabToSelect.gameObject, true, true));
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
				_rect.x += _distance > 0 ? _distance : _rect.width;

				// New row if we go off-viewport
				if(_rect.x + _rect.width > viewport.xMax) {
					_rect.x = viewport.xMin + 5f;
					_rect.y += _rect.height + 5f;
				}
			} break;

			case Layout.VERTICAL: {
				// Advance pos
				_rect.y += _distance > 0 ? _distance : _rect.height;

				// New column if we go off-viewport
				if(_rect.y + _rect.height > viewport.yMax) {
					_rect.y = viewport.yMin + 5f;
					_rect.x += _rect.width + 5f;
				}
			} break;
		}
	}
}