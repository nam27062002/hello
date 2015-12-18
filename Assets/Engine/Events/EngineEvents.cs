// GameEvents.cs
// 
// Created by Alger Ortín Castellví on 24/08/2015.
// Copyright (c) 2015 Ubisoft. All rights reserved.

/// <summary>
/// Collection of events related to the engine.
/// Start your custom game events with the last value of this enum.
/// </summary>
public enum EngineEvents {
	// Scene Controller
	SCENE_STATE_CHANGED,	// params: SceneManager.ESceneState _oldState, SceneManager.ESceneState _newState

	// Popups Management
	POPUP_CREATED,			// params: PopupController _popup
	POPUP_OPENED,			// params: PopupController _popup
	POPUP_CLOSED,			// params: PopupController _popup
	POPUP_DESTROYED,		// params: PopupController _popup

	// Localization
	EVENT_LANGUAGE_CHANGED,	// no params

	// Custom Game Events:
	// This should always be the last element of the EngineEvents enum
	// Start your custom GameEvents enum with this value (e.g. MY_FIRST_GAME_EVENT = EngineEvents.END)
	END
}