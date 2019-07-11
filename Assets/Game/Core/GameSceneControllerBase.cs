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

