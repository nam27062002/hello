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
		private float m_timer = 0.5f;

		//------------------------------------------------------------------//
		// GENERIC METHODS													//
		//------------------------------------------------------------------//
		/// <summary>
		/// Initialization.
		/// </summary>
		override protected void Awake() {
			// Initialize some required managers
			ContentManager.InitContent();
			PersistenceManager.Load();
			SpawnerManager.CreateInstance();

			// Load the dragon
			DragonManager.LoadDragon(LevelEditor.settings.testDragon);
			if ( InstanceManager.player != null )
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
			Messenger.AddListener(GameEvents.PLAYER_KO, OnPlayerKo);
		}
		
		/// <summary>
		/// Component disabled.
		/// </summary>
		private void OnDisable() {
			// Simulate end game
			Messenger.Broadcast(GameEvents.GAME_ENDED);

			// Unsubscribe from external events
			Messenger.RemoveListener<PopupController>(EngineEvents.POPUP_CLOSED, OnPopupClosed);
			Messenger.RemoveListener(GameEvents.PLAYER_KO, OnPlayerKo);
		}

		/// <summary>
		/// Called every frame.
		/// </summary>
		private void Update() {
			// Update running time
			m_elapsedSeconds += Time.deltaTime;

			// Quick'n'dirty timer to place the dragon at the spawn point
			if(m_timer > 0f) {
				m_timer -= Time.deltaTime;
				if(m_timer <= 0f)
					if (InstanceManager.player)
				 		InstanceManager.player.MoveToSpawnPoint(true);
			}
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
			// Simulate level loaded
			Messenger.Broadcast(GameEvents.GAME_LEVEL_LOADED);

			// Run spawner manager
			SpawnerManager.instance.EnableSpawners();

			if ( InstanceManager.player != null ){
				// Reset dragon stats
				InstanceManager.player.ResetStats(false);

				// Put player in position and make it playable
				InstanceManager.player.MoveToSpawnPoint(true);
				InstanceManager.player.playable = true;
			}else{
				// Reset dragon stats
				InstanceManager.playerEntity.ResetStats(false);

				// Put player in position and make it playable
				InstanceManager.playerEntity.MoveToSpawnPoint(true);
			}

			// Enable reward manager to see coins/score feedback
			RewardManager.Reset();

			// Spawn chest
			ChestManager.SelectChest();

			// Reset timer
			m_elapsedSeconds = 0;
		}

		//------------------------------------------------------------------//
		// CALLBACKS														//
		//------------------------------------------------------------------//
		/// <summary>
		/// The player is ko.
		/// </summary>
		private void OnPlayerKo() {
			// Just open summary popup for now
			Time.timeScale = 0.0f;	// Pause game
			PopupManager.OpenPopupInstant(PopupLevelEditorSummary.PATH);
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

					case PopupLevelEditorSummary.Result.REVIVE: {
						Time.timeScale = 1f;	// Unpause
						InstanceManager.player.ResetStats(true);
					} break;
				}
			}
		}
	}
}
