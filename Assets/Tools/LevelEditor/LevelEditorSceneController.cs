// LevelEditorSceneController.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 29/09/2015.
// Copyright (c) 2015 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
namespace LevelEditor {
	/// <summary>
	/// Scene controller for the level editor scene.
	/// Simplified version of the game scene controller for 
	/// </summary>
	public class LevelEditorSceneController : GameSceneControllerBase {
		//------------------------------------------------------------------//
		// CONSTANTS														//
		//------------------------------------------------------------------//
		public static readonly string NAME = "SC_LevelEditor";

		//------------------------------------------------------------------//
		// MEMBERS AND PROPERTIES											//
		//------------------------------------------------------------------//

		//------------------------------------------------------------------//
		// GENERIC METHODS													//
		//------------------------------------------------------------------//
		/// <summary>
		/// Initialization.
		/// </summary>
		override protected void Awake() {
			DefinitionsManager.CreateInstance();

			// Load the dragon
			DragonManager.LoadDragon(LevelEditor.settings.testDragon);
			InstanceManager.player.playable = false;

			// Call parent
			base.Awake();
		}

		/// <summary>
		/// First update.
		/// </summary>
		private void Start() {
			StartGame();
		}

		/// <summary>
		/// Component enabled.
		/// </summary>
		private void OnEnable() {
			// Subscribe to external events
			Messenger.AddListener<PopupController>(EngineEvents.POPUP_CLOSED, OnPopupClosed);
			Messenger.AddListener(GameEvents.PLAYER_DIED, OnPlayerDied);
		}
		
		/// <summary>
		/// Component disabled.
		/// </summary>
		private void OnDisable() {
			// Unsubscribe from external events
			Messenger.RemoveListener<PopupController>(EngineEvents.POPUP_CLOSED, OnPopupClosed);
			Messenger.RemoveListener(GameEvents.PLAYER_DIED, OnPlayerDied);
		}

		/// <summary>
		/// Called every frame.
		/// </summary>
		private void Update() {
			// Update running time
			m_elapsedSeconds += Time.deltaTime;
		}

		/// <summary>
		/// Destructor.
		/// </summary>
		override protected void OnDestroy() {
			// Clear pools
			FirePropagationManager.DestroyInstance();
			PoolManager.Clear(true);

			// Call parent
			base.OnDestroy();
		}

		//------------------------------------------------------------------//
		// INTERNAL METHODS													//
		//------------------------------------------------------------------//
		/// <summary>
		/// Do all the necessary stuff to start a game.
		/// </summary>
		private void StartGame() {
			// Reset dragon stats
			InstanceManager.player.ResetStats(false);

			// Put player in position and make it playable
			InstanceManager.player.MoveToSpawnPoint();
			InstanceManager.player.playable = true;

			// Enable reward manager to see coins/score feedback
			RewardManager.Reset();

			// Spawn chest
			ChestManager.SelectChest();

			// Reset timer
			m_elapsedSeconds = 0;
		}

		/// <summary>
		/// Do all the necessary stuff when ending a game.
		/// </summary>
		private void EndGame() {
			// Just open summary popup for now
			Time.timeScale = 0.0f;	// Pause game
			PopupManager.OpenPopupInstant(PopupLevelEditorSummary.PATH);
		}

		//------------------------------------------------------------------//
		// CALLBACKS														//
		//------------------------------------------------------------------//
		/// <summary>
		/// The player has died.
		/// </summary>
		private void OnPlayerDied() {
			// End game
			EndGame();
		}

		/// <summary>
		/// A popup has been closed.
		/// </summary>
		/// <param name="_popup">The popup that has been closed</param>
		public void OnPopupClosed(PopupController _popup) {
			// Figure out which popup has been closed
			// a) Level editor summary popup
			PopupLevelEditorSummary summaryPopup = _popup.GetComponent<PopupLevelEditorSummary>();
			if(summaryPopup != null) {
				switch(summaryPopup.result) {
					case PopupLevelEditorSummary.Result.FINISH: {
						#if UNITY_EDITOR
						UnityEditor.EditorApplication.isPlaying = false;
						#else
						Application.Quit();
						#endif
					} break;
				}
			}
		}
	}
}
