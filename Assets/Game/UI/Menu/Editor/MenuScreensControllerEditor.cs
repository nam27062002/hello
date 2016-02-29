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
	/// <summary>
	/// Static constructor.
	/// </summary>
	static MenuScreensControllerToolbar() {
		// Subscribe to external events
		SceneView.onSceneGUIDelegate += OnSceneGUI;
	}

	/// <summary>
	/// Draw gui on the scene.
	/// </summary>
	/// <param name="_sceneview">The target scene.</param>
	private static void OnSceneGUI(SceneView _sceneview) {
		// Ignore if there is no Screen controller in the scene
		MenuScreensController target = GameObject.FindObjectOfType<MenuScreensController>();
		if(target == null) return;

		/// http://answers.unity3d.com/questions/19321/onscenegui-called-without-any-object-selected.html
		int screenToEdit = -1;
		Handles.BeginGUI(); {
			// In this particular case it's easier to just go with old school GUI calls
			Rect rect = new Rect(5f, 5f, 130f, 20f);

			// Do a button for each scene
			MenuScreensController.Screens scr = MenuScreensController.Screens.NONE;
			for(int i = 0; i < (int)MenuScreensController.Screens.COUNT; i++) {
				scr = (MenuScreensController.Screens)i;
				GUI.enabled = true;
				if(GUI.Button(rect, scr.ToString())) {
					// Save it as target screen!
					screenToEdit = i;
				}

				// Advance position
				rect.x += rect.width;
			}
		} Handles.EndGUI();

		// If the user has selected a screen to edit, react to it!
		if(screenToEdit >= 0) {
			// Disable all screens except the target one and ping the target screen
			NavigationScreen scr = null;
			for(int i = 0; i < (int)MenuScreensController.Screens.COUNT; i++) {
				scr = target.screens[i];
				if(scr != null) {
					if(i == screenToEdit) {
						scr.gameObject.SetActive(true);
						EditorUtils.FocusObject(scr.gameObject);
					} else {
						scr.gameObject.SetActive(false);
					}
				}
			}

			// Move main camera to screen's snap point (if any)
			CameraSnapPoint targetSnapPoint = target.screensCameraSnapPoints[screenToEdit];
			if(targetSnapPoint != null) {
				targetSnapPoint.Apply(target.camera);
			}
		}
	}
}