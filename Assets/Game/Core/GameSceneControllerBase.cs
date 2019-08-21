// GameSceneControllerBase.cs
// Hungry Dragon
// 
// Created by Marc Saña Castellví on 26/01/2016.
// Copyright (c) 2016 Ubisoft. All rights reserved.

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
public class GameSceneControllerBase : SceneController, IBroadcastListener {
	
	//------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES											//
	//------------------------------------------------------------------//
	// Time
	protected float m_elapsedSeconds = 0;
	public float elapsedSeconds {
		get { return m_elapsedSeconds; }
        set { m_elapsedSeconds = value; }
    }

	private int m_progressionOffsetXP;
	public int progressionOffsetXP { 
		get { return m_progressionOffsetXP; } 
		set { m_progressionOffsetXP = value; } 
	}

	private float m_progressionOffsetSeconds;
	public float progressionOffsetSeconds {
		get { return m_progressionOffsetSeconds; }
		set { m_progressionOffsetSeconds = value; }
	}

	// Handled by heirs
	protected bool m_paused = false;
	public bool paused {
		get { return m_paused; }
	}

	protected int m_freezeElapsedSeconds = 0;
	public int freezeElapsedSeconds {
		get { return m_freezeElapsedSeconds; }
		set { m_freezeElapsedSeconds = value; }
	}
	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	protected override void Awake() {
		// Call parent
		base.Awake();

		// Subscribe to external events
		Messenger.AddListener(MessengerEvents.GAME_STARTED, OnGameStarted);
		Broadcaster.AddListener(BroadcastEventType.GAME_ENDED, this);
	}

	/// <summary>
	/// Destructor.
	/// </summary>
	protected override void OnDestroy() {
		// Call parent
		base.OnDestroy();

		// Unsubscribe to external events
		Messenger.RemoveListener(MessengerEvents.GAME_STARTED, OnGameStarted);
		Broadcaster.RemoveListener(BroadcastEventType.GAME_ENDED, this);
	}

    protected virtual void Update() {
        SpawnerManager.instance.Update();
        EntityManager.instance.Update();
        DecorationManager.instance.Update();
        FirePropagationManager.instance.Update();
        BubbledEntitySystem.instance.Update();
        MachineInflammableManager.instance.Update();
    }

    private void FixedUpdate() {
        EntityManager.instance.FixedUpdate();
    }

    private void LateUpdate() {
        EntityManager.instance.LateUpdate();
    }


    public virtual void OnBroadcastSignal(BroadcastEventType eventType, BroadcastEventInfo broadcastEventInfo)
    {
        switch( eventType )
        {
            case BroadcastEventType.GAME_ENDED:
            {
                OnGameEnded();
            }break;
        }
    }
    

	public virtual bool IsLevelLoaded()
	{
		return true;
	}

	//------------------------------------------------------------------------//
	// INTERNAL METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Instantiates the map object corresponding to the current level (LevelManager.currentLevelData)
	/// and initializes the map camera references in the InstanceManager.
	/// </summary>
	protected virtual void InitLevelMap() {
		// Get current level's data
		LevelData levelData = LevelManager.currentLevelData;

		// Create an instance of the map camera from the level's data prefab
		Debug.Assert(levelData.mapPrefab != null, "The loaded level doesn't have a Map prefab assigned, minimap will be disabled.");
		if(levelData.mapPrefab != null) {
			GameObject mapObj = Instantiate<GameObject>(levelData.mapPrefab);
			InstanceManager.mapCamera = mapObj.GetComponentInChildren<MapCamera>();
			Debug.Assert(InstanceManager.mapCamera != null, "The object holding the LevelMapData doesn't have a Camera component");

			// Start with camera disabled, the map scroller will enable it when needed
			if(InstanceManager.mapCamera != null) {
				InstanceManager.mapCamera.gameObject.SetActive(false);
			}
		}
	}

	protected void SpawnPlayer(bool _isLevelEditor) {
		GameObject spawnPointObj = null;
		string dragonSKU = InstanceManager.player.data.def.sku;

		if (_isLevelEditor) {
			string selectedSP = LevelEditor.LevelEditor.settings.spawnPoint;
			if (!string.IsNullOrEmpty(selectedSP)) {
				spawnPointObj = GameObject.Find(selectedSP);
			}

			if (spawnPointObj == null) {
				spawnPointObj = GameObject.Find(LevelEditor.LevelTypeSpawners.DRAGON_SPAWN_POINT_NAME + "_" + LevelEditor.LevelTypeSpawners.LEVEL_EDITOR_SPAWN_POINT_NAME);
				if (spawnPointObj == null) {
					spawnPointObj = GameObject.Find(LevelEditor.LevelTypeSpawners.DRAGON_SPAWN_POINT_NAME + "_" + dragonSKU);
				}
			}
		} else {
			// maybe we are inside a tournament
			if (HDLiveDataManager.tournament.isActive) {
				HDTournamentDefinition tournamentDef = HDLiveDataManager.tournament.tournamentData.tournamentDef;
				string selectedSP = tournamentDef.m_goal.m_spawnPoint;
				if (!string.IsNullOrEmpty(selectedSP)) {
					spawnPointObj = GameObject.Find(LevelEditor.LevelTypeSpawners.DRAGON_SPAWN_POINT_NAME + "_" + selectedSP);
				}
			}
			
			if (spawnPointObj == null) {
				spawnPointObj = GameObject.Find(LevelEditor.LevelTypeSpawners.DRAGON_SPAWN_POINT_NAME + "_" + dragonSKU);
			}
		}
		// If we couldn't find a valid spawn point, try to find a generic one
		if (spawnPointObj == null) {
			spawnPointObj = GameObject.Find(LevelEditor.LevelTypeSpawners.DRAGON_SPAWN_POINT_NAME);
		}

		if (spawnPointObj != null) {
			DragonSpawnPoint dsp = spawnPointObj.GetComponent<DragonSpawnPoint>();
			if (dsp != null) dsp.Spawn();

			Vector3 startPos = spawnPointObj.transform.position;
			
			InstanceManager.player.gameObject.SetActive(true);
			if (_isLevelEditor && !LevelEditor.LevelEditor.settings.useIntro) {
				InstanceManager.player.playable = true;
				InstanceManager.player.dragonMotion.MoveToSpawnPosition(startPos);				
			} else {
				InstanceManager.player.playable = false;
				InstanceManager.player.dragonMotion.StartIntroMovement(startPos);
			}		
			// Init game camera
			InstanceManager.gameCamera.Init(startPos);
		}	
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// The game has started.
	/// </summary>
	protected virtual void OnGameStarted() {
		
	}

	/// <summary>
	/// The game has eneded.
	/// </summary>
	protected virtual void OnGameEnded() {
		
	}
}

