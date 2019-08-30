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
		[SerializeField] private GameObject m_canvas;

        private bool m_started = false;

        //------------------------------------------------------------------//
        // GENERIC METHODS													//
        //------------------------------------------------------------------//
        /// <summary>
        /// Initialization.
        /// </summary>
        override protected void Awake() {
        	m_started = false;

			if (m_canvas != null) {
				m_canvas.SetActive(true);
			}

            ApplicationManager.CreateInstance();
			ControlPanel.CreateInstance();

			HDAddressablesManager.Instance.Initialize();

            // Initialize some required managers
			ContentManager.InitContent(true, false);

            FirePropagationManager.CreateInstance();
            BubbledEntitySystem.CreateInstance();
            SpawnerManager.CreateInstance();
            DecorationSpawnerManager.CreateInstance();

            // Initialize fake profile for the level editor
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

            PoolManager.instance.poolLimits = PoolManager.PoolLimits.Unlimited;

			// Prepare pets
			System.Collections.Generic.List<string> equipedPets = UsersManager.currentUser.GetEquipedPets(LevelEditor.settings.testDragon);
			for (int i = 0; i < equipedPets.Count; i++)
			{
				UsersManager.currentUser.UnequipPet(LevelEditor.settings.testDragon, equipedPets[i]);
			}
			// Set pets
			// Unlock all pets
			System.Collections.Generic.List<DefinitionNode> petDefs = DefinitionsManager.SharedInstance.GetDefinitionsList(DefinitionsCategory.PETS);
			for(int i = 0; i < petDefs.Count; i++) {
				UsersManager.currentUser.petCollection.UnlockPet(petDefs[i].sku);
			}

			for (int i = 0; i < LevelEditor.settings.testPets.Length; i++)
			{
				string petSku = LevelEditor.settings.testPets[i];
				if ( !string.IsNullOrEmpty(petSku) && petSku != "none" )
				{
					UsersManager.currentUser.EquipPet(LevelEditor.settings.testDragon, petSku);
				}
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
		
		}

		/// <summary>
		/// First update.
		/// </summary>
		private IEnumerator Start() {

			while( !PersistenceFacade.instance.LocalDriver.IsLoadedInGame)
			{
				yield return null;
			}

			// Mark tutorial as completed
			UsersManager.currentUser.SetTutorialStepCompleted(TutorialStep.ALL, true);
			UsersManager.currentUser.gamesPlayed = 25;

			// Override some values in the user profile
			DefinitionNode testDragonDef = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.DRAGONS, LevelEditor.settings.testDragon);
			if(testDragonDef.GetAsString("type") == DragonDataClassic.TYPE_CODE) {
				UsersManager.currentUser.currentClassicDragon = testDragonDef.sku;
				SceneController.SetMode(Mode.DEFAULT);
			} else {
				UsersManager.currentUser.currentSpecialDragon = testDragonDef.sku;
				SceneController.SetMode(Mode.SPECIAL_DRAGONS);
			}

            InstanceManager.player.gameObject.SetActive(true);

			ChestManager.CreateInstance();
			ChestManager.SetupUser(UsersManager.currentUser);

			EggManager.CreateInstance();
			EggManager.SetupUser(UsersManager.currentUser);

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
			Broadcaster.AddListener(BroadcastEventType.POPUP_CLOSED, this);
			Messenger.AddListener<DamageType, Transform>(MessengerEvents.PLAYER_KO, OnPlayerKo);
		}
		
		/// <summary>
		/// Component disabled.
		/// </summary>
		private void OnDisable() {
			// Simulate end game
			Broadcaster.Broadcast(BroadcastEventType.GAME_ENDED);

			// Unsubscribe from external events
			Broadcaster.RemoveListener(BroadcastEventType.POPUP_CLOSED, this);
			Messenger.RemoveListener<DamageType, Transform>(MessengerEvents.PLAYER_KO, OnPlayerKo);
		}

        public override void OnBroadcastSignal(BroadcastEventType eventType, BroadcastEventInfo broadcastEventInfo)
        {
            base.OnBroadcastSignal(eventType, broadcastEventInfo);
            switch(eventType)
            {
                case BroadcastEventType.POPUP_CLOSED:
                {
                    PopupManagementInfo info = (PopupManagementInfo)broadcastEventInfo;
                    OnPopupClosed(info.popupController);
                }break;
            }
        }


        /// <summary>
        /// Called every frame.
        /// </summary>
        override protected void Update() {
			if (!m_started) {
				if ( InstanceManager.player != null )
					StartGame();
			} else {
                base.Update();

				// Update running time
				m_elapsedSeconds += Time.deltaTime;

				if (Input.GetKeyDown(KeyCode.I))
				{					
					SpawnPlayer(true);
					
					LevelTypeSpawners sp = FindObjectOfType<LevelTypeSpawners>();
					if ( sp != null )
						sp.IntroSpawn(InstanceManager.player.data.def.sku);
				}

				// Notify listeners
				Messenger.Broadcast(MessengerEvents.GAME_UPDATED);
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

			string bloodOverride = SeasonManager.GetBloodParticlesName();
			if (string.IsNullOrEmpty(bloodOverride)) {
				ParticleManager.DisableBloodOverride();
			} else {
				ParticleManager.EnableBloodOverride(bloodOverride);
			}
			ParticleManager.PreBuild();
			PoolManager.Build();

			// Setup progression offset based on spawn position
			progressionOffsetSeconds = float.Parse(LevelEditor.settings.progressionOffsetSeconds);
			progressionOffsetXP = int.Parse(LevelEditor.settings.progressionOffsetXP);

			// Reset dragon stats
			InstanceManager.player.ResetStats(false);
			
			Vector3 startPos = GameConstants.Vector3.zero;
			// Setup spawn position
			if (LevelEditor.settings.spawnAtCameraPos) {
				startPos = mainCamera.transform.position;
				startPos.z = 0f;
				InstanceManager.player.transform.position = startPos;
				InstanceManager.player.playable = true;
				InstanceManager.gameCamera.Init(startPos);
			} else {
				SpawnPlayer(true);
			}			
			
			// Instantiate map prefab
			InitLevelMap();

			// Simulate level loaded
			Broadcaster.Broadcast(BroadcastEventType.GAME_LEVEL_LOADED);

			// Run spawner manager
			SpawnerManager.instance.EnableSpawners();
            DecorationSpawnerManager.instance.EnableSpawners();

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
			Messenger.Broadcast(MessengerEvents.GAME_STARTED);
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
