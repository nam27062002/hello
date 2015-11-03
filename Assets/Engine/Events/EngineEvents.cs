// GameEvents.cs
// 
// Created by Alger Ortín Castellví on 24/08/2015.
// Copyright (c) 2015 Ubisoft. All rights reserved.

/// <summary>
/// Collection of events related to the engine.
/// </summary>
public static class EngineEvents {
	// Scene Controller
	public const string SCENE_STATE_CHANGED = "SCENE_STATE_CHANGED";		// params: SceneManager.ESceneState _oldState, SceneManager.ESceneState _newState

	// Popups Management
	public const string POPUP_CREATED = "POPUP_CREATED";	// params: PopupController _popup
	public const string POPUP_OPENED = "POPUP_OPENED";	// params: PopupController _popup
	public const string POPUP_CLOSED = "POPUP_CLOSED";	// params: PopupController _popup
	public const string POPUP_DESTROYED = "POPUP_DESTROYED";	// params: PopupController _popup
}