// LevelEditorSceneController.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 29/09/2015.
// Copyright (c) 2015 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;
using System.Collections;

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

        private bool m_started = false;

        //------------------------------------------------------------------//
        // GENERIC METHODS													//
        //------------------------------------------------------------------//
        /// <summary>
        /// Initialization.
        /// </summary>
        override protected void Awake() {
        	m_started = false;
            ApplicationManager.CreateInstance();            

            // Initialize some required managers
            ContentManager.InitContent(true);
			SpawnerManager.CreateInstance();

            UsersManager.CreateInstance();

            DragonManager.SetupUser(UsersManager.currentUser);
            MissionManager.SetupUser(UsersManager.currentUser);
            EggManager.SetupUser(UsersManager.currentUser);
            ChestManager.SetupUser(UsersManager.currentUser);
            GameStoreManager.SharedInstance.Initialize();

            PersistenceFacade.instance.Reset();            
            PersistenceFacade.instance.Sync_FromLaunchApplication(null);
            
			if (LevelEditor.settings.poolLimit == "unlimited") {
				ParticleManager.instance.poolLimits = ParticleManager.PoolLimits.Unlimited;
			} else {
				ParticleManager.instance.poolLimits = ParticleManager.PoolLimits.LevelEditor;
			}

			// Load the dragon
            DragonManager.LoadDragon(LevelEditor.settings.testDragon);
            if (InstanceManager.player != null)
            {
                InstanceManager.player.playable = false;
				InstanceManager.player.gameObject.SetActive(false);
			}

            // Call parent
            base.Awake();

			ControlPanel.CreateInstance();
		}

		/// <summary>
		/// First update.
		/// </summary>
		private IEnumerator Start() {

			while( !PersistenceFacade.instance.LocalDriver.IsLoadedInGame)
			{
				yield return null;
			}            

            InstanceManager.player.gameObject.SetActive(true);

			ChestManager.CreateInstance();
			EggManager.CreateInstance();
			CollectiblesManager.CreateInstance();
			CollectiblesManager.OnLevelLoaded();

			HungryLettersManager lettersManager = FindObjectOfType<HungryLettersManager>();
			if ( lettersManager )
			{
				lettersManager.Respawn();
			}
		}

		/// <summary>
		/// Component enabled.
		/// </summary>
		private void OnEnable() {
			// Subscribe to external events
			Messenger.AddListener<PopupController>(EngineEvents.POPUP_CLOSED, OnPopupClosed);
			Messenger.AddListener<DamageType, Transform>(GameEvents.PLAYER_KO, OnPlayerKo);
		}
		
		/// <summary>
		/// Component disabled.
		/// </summary>
		private void OnDisable() {
			// Simulate end game
			Messenger.Broadcast(GameEvents.GAME_ENDED);

			// Unsubscribe from external events
			Messenger.RemoveListener<PopupController>(EngineEvents.POPUP_CLOSED, OnPopupClosed);
			Messenger.RemoveListener<DamageType, Transform>(GameEvents.PLAYER_KO, OnPlayerKo);
		}

		/// <summary>
		/// Called every frame.
		/// </summary>
		private void Update() {
			if (!m_started) {
				if ( InstanceManager.player != null )
					StartGame();
			} else {
				// Update running time
				m_elapsedSeconds += Time.deltaTime;

				if (Input.GetKeyDown(KeyCode.I))
				{
					bool usingEditor = true;
					InstanceManager.player.StartIntroMovement( usingEditor );
					InstanceManager.gameCamera.StartIntro( usingEditor );
					LevelTypeSpawners sp = FindObjectOfType<LevelTypeSpawners>();
					if ( sp != null )
						sp.IntroSpawn(InstanceManager.player.data.def.sku);
				}

				// Notify listeners
				Messenger.Broadcast(GameEvents.GAME_UPDATED);
			}
		}

		/// <summary>
		/// Destructor.
		/// </summary>
		override protected void OnDestroy() {
			// Clear pools
			FirePropagationManager.DestroyInstance();
			ParticleManager.Clear();
			PoolManager.Clear(true);
			UIPoolManager.Clear(true);

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
			LevelManager.SetCurrentLevel(LevelEditor.settings.levelSku);

			ParticleManager.PreBuild();
			PoolManager.Build();

			// Reset dragon stats
			InstanceManager.player.ResetStats(false);

			// Put player in position and make it playable
			InstanceManager.player.MoveToSpawnPoint(true);
			if ( LevelEditor.settings.useIntro )
			{
				InstanceManager.player.StartIntroMovement( true );	
			}
			InstanceManager.player.playable = true;

			// Init game camera
			InstanceManager.gameCamera.Init();

			// Instantiate map prefab
			InitLevelMap();

			// Simulate level loaded
			Messenger.Broadcast(GameEvents.GAME_LEVEL_LOADED);

			// Run spawner manager
			SpawnerManager.instance.EnableSpawners();

			// Enable reward manager to see coins/score feedback
			RewardManager.Reset();

			// Spawn collectibles
			// [AOC] By designers request, let's keep all collectibles visible in the level editor
			//ChestManager.SelectChest();
			//EggManager.SelectCollectibleEgg();

			// Reset timer
			m_elapsedSeconds = 0;

			m_started = true;

			// Notify the game
			Messenger.Broadcast(GameEvents.GAME_STARTED);
		}

		//------------------------------------------------------------------//
		// CALLBACKS														//
		//------------------------------------------------------------------//
		/// <summary>
		/// The player is ko.
		/// </summary>
		private void OnPlayerKo(DamageType _type, Transform _source) {
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

		public override bool IsLevelLoaded()
		{
			return m_started;
		}
	}
}
