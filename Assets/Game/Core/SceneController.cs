// SceneController.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 25/08/2015.
// Copyright (c) 2015 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// Base class for all scene controllers.
/// Each scene should have an object containing one of these, usually a custom
/// implementation of this class.
/// </summary>
public class SceneController : MonoBehaviour {
	//------------------------------------------------------------------//
	// CONSTANTS														//
	//------------------------------------------------------------------//

	//------------------------------------------------------------------//
	// PROPERTIES														//
	//------------------------------------------------------------------//

	public enum Mode
	{
		DEFAULT,
		TOURNAMENT,
        SPECIAL_DRAGONS
	};
	private static Mode s_mode = Mode.DEFAULT;
	public static Mode mode {
		get { return s_mode; }
	}

	//------------------------------------------------------------------//
	// MEMBERS															//
	//------------------------------------------------------------------//
	[Comment("Mark it only if the scene doesn't have any dependency with previous scenes.")]
	[SerializeField] private bool m_standaloneScene = false;

	[Comment("Optional, main camera of the scene for faster global access", 10f)]
	[SerializeField] private Camera m_mainCamera = null;
	public Camera mainCamera {
		get { return m_mainCamera; }
	}

	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	protected virtual void Awake() {
		// If it's the first scene being loaded and it can't run standalone, restart the game flow
		if(string.IsNullOrEmpty(GameSceneManager.prevScene) && !m_standaloneScene) {
			FlowManager.Restart();
		}

		// Register ourselves to the instance manager
		InstanceManager.sceneController = this;
	}

	/// <summary>
	/// Destructor.
	/// </summary>
	protected virtual void OnDestroy() {
		// Unregister ourselves from the instance manager
		// If the instance manager was not created (or already destroyed) we would 
		// be creating a new object while destroying current scene, which is quite 
		// problematic for Unity.
		if(InstanceManager.isInstanceCreated) InstanceManager.sceneController = null;
	}

	//------------------------------------------------------------------//
	// STATIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Change current game mode.
	/// Will trigger the GAME_MODE_CHANGED messenger event.
	/// </summary>
	/// <param name="_newMode">New mode.</param>
	public static void SetMode(Mode _newMode) {
		// Log
		Debug.Log(Colors.orange.Tag("Changing Game Mode from " + s_mode + " to " + _newMode));

		// Specific actions to perform when leaving a mode
		switch(s_mode) {
			case Mode.DEFAULT: {
				
			} break;

			case Mode.SPECIAL_DRAGONS: {
				
			} break;

			case Mode.TOURNAMENT: {

			} break;
		}

		// Swap mode
		Mode oldMode = s_mode;
		s_mode = _newMode;

		// Specific actions to perform when entering a new mode
		switch(s_mode) {
			case Mode.DEFAULT: {
				// Set selected dragon to the current classic dragon
				if(InstanceManager.menuSceneController != null) {
					InstanceManager.menuSceneController.SetSelectedDragon(DragonManager.CurrentDragon.sku);
				}
			} break;

			case Mode.SPECIAL_DRAGONS: {
				// Set selected dragon to the current special dragon
				if(InstanceManager.menuSceneController != null) {
					// Current special dragon can be null if no special dragon has been unlocked yet
					string selectedDragonSku = "";
					if(DragonManager.CurrentDragon != null) {
						selectedDragonSku = DragonManager.CurrentDragon.sku;
					} else {
						selectedDragonSku = DragonManager.GetDragonsByOrder(IDragonData.Type.SPECIAL).First().sku;	// Select first dragon
					}
					InstanceManager.menuSceneController.SetSelectedDragon(selectedDragonSku);
				}
			} break;

			case Mode.TOURNAMENT: {

			} break;
		}

		// Notify game
		Messenger.Broadcast<Mode, Mode>(MessengerEvents.GAME_MODE_CHANGED, oldMode, _newMode);
	}

	/// <summary>
	/// Get the mode associated to a given dragon type.
	/// </summary>
	/// <returns>The game mode associated to <paramref name="_type"/>.</returns>
	/// <param name="_type">Dragon type to be checked.</param>
	public static Mode DragonTypeToMode(IDragonData.Type _type) {
		switch(_type) {
			case IDragonData.Type.CLASSIC: {
				return Mode.DEFAULT;
			} break;

			case IDragonData.Type.SPECIAL: {
				return Mode.SPECIAL_DRAGONS;
			} break;
		}
		return Mode.DEFAULT;
	}

	/// <summary>
	/// Get the dragon type associated to a given game mode.
	/// </summary>
	/// <returns>The dragon type associated to <paramref name="_mode"/>.</returns>
	/// <param name="_mode">Mode to be checked.</param>
	public static IDragonData.Type ModeToDragonType(Mode _mode) {
		switch(_mode) {
			case Mode.DEFAULT:
			case Mode.TOURNAMENT: {
				return IDragonData.Type.CLASSIC;
			} break;

			case Mode.SPECIAL_DRAGONS: {
				return IDragonData.Type.SPECIAL;
			} break;
		}
		return IDragonData.Type.CLASSIC;
	}

	//------------------------------------------------------------------------//
	// STATIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Returns the main camera for the current active scene.
	/// </summary>
	/// <returns>The default camera for current scene. Can be <c>null</c> when no scene is loaded.</returns>
	public static Camera GetMainCameraForCurrentScene() {
		// If there is no active scene, just return null
		if(InstanceManager.sceneController == null) return null;

		// Are we in the menu?
		if(InstanceManager.sceneController is MenuSceneController) {
			// Yes! Return menu camera
			return InstanceManager.menuSceneController.mainCamera;
		} else if(InstanceManager.sceneController is GameSceneController) {
			// No! Are we in-game or in the results?
			if(InstanceManager.gameSceneController.resultsScene == null) {
				// Ingame, return game camera
				return InstanceManager.gameSceneController.mainCamera;
			} else {
				// Results screen, return results scene camera
				return InstanceManager.gameSceneController.resultsScene.mainCamera;
			}
		}

		// Rest of cases (shouldn't get here)
		return null;
	}
}

