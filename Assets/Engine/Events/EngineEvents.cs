// GameEvents.cs
// 
// Created by Alger Ortín Castellví on 24/08/2015.
// Copyright (c) 2015 Ubisoft. All rights reserved.

/// <summary>
/// Collection of events related to the engine.
/// Start your custom game events with the last value of this enum.
/// </summary>
public enum EngineEvents {
	// Game Scene Manager
	SCENE_STATE_CHANGED,	// params: SceneManager.ESceneState _oldState, SceneManager.ESceneState _newState
	SCENE_UNLOADED,			// params: string _sceneName
	SCENE_LOADED,			// params: string _sceneName

	// Popups Management
	POPUP_CREATED,			// params: PopupController _popup
	POPUP_OPENED,			// params: PopupController _popup
	POPUP_CLOSED,			// params: PopupController _popup
	POPUP_DESTROYED,		// params: PopupController _popup

	// Screen Navigation System
	// [AOC] Triggered at the start of the animation, parameters englobed in a custom class:
	NAVIGATION_SCREEN_CHANGED,		// params: NavigationScreenSystem.ScreenChangedEvent _eventData

	// Rules and localization
	LANGUAGE_CHANGED,		// no params
	DEFINITIONS_LOADED,		// no params

	// Custom Game Events:
	// This should always be the last element of the EngineEvents enum
	// Start your custom GameEvents enum with this value (e.g. MY_FIRST_GAME_EVENT = EngineEvents.END)
	END
}