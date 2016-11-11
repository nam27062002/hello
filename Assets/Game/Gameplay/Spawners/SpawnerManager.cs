// SpawnerManager.cs
// Hungry Dragon
// 
// Created by Marc Saña Forrellach, Alger Ortín Castellví
// Copyright (c) 2016 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using System.Collections.Generic;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Singleton to manage all the spawners in a level in an efficient way.
/// </summary>
public class SpawnerManager : UbiBCN.SingletonMonoBehaviour<SpawnerManager> {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	private const float UPDATE_INTERVAL = 0.2f;	// Seconds, avoid updating all the spawners all the time for better performance
	public const float BACKGROUND_LAYER_Z = 45f;
    public const float SPAWNING_MAX_TIME = 4f; // Max time (in milliseconds) allowed to spend on spawning entities

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Spawners collection
	private List<ISpawner> m_spawners = null;
	private QuadTree<ISpawner> m_spawnersTree = null;

	// Internal logic
	private bool m_enabled = false;
	private float m_updateTimer = 0f;

	// External references
	private GameCamera m_newCamera = null;

	// Detection area
	private FastBounds2D m_minRect = null;	// From the game camera
	private FastBounds2D m_maxRect = null;
	private Rect[] m_subRect = new Rect[4];
	private HashSet<ISpawner> m_selectedSpawners = new HashSet<ISpawner>();

	private List<ISpawner> m_spawning;

    private float m_lastX, m_lastY;

    private System.Diagnostics.Stopwatch m_watch = new System.Diagnostics.Stopwatch();

    //------------------------------------------------------------------------//
    // GENERIC METHODS														  //
    //------------------------------------------------------------------------//
    /// <summary>
    /// Inititalization.
    /// </summary>
    private void Awake() {
		m_spawners = new List<ISpawner>();
		m_spawning = new List<ISpawner>();

#if !PRODUCTION
        Debug_Awake();
#endif
    }

    protected override void OnDestroy() {
        base.OnDestroy();

#if !PRODUCTION
        Debug_OnDestroy();
#endif      
    }

    /// <summary>
    /// Component enabled.
    /// </summary>
    private void OnEnable() {
		// Subscribe to external events
		Messenger.AddListener(GameEvents.GAME_LEVEL_LOADED, OnLevelLoaded);
		Messenger.AddListener(GameEvents.GAME_ENDED, OnGameEnded);
	}

	/// <summary>
	/// Component disabled.
	/// </summary>
	private void OnDisable() {
		// Unsubscribe from external events
		Messenger.RemoveListener(GameEvents.GAME_LEVEL_LOADED, OnLevelLoaded);
		Messenger.RemoveListener(GameEvents.GAME_ENDED, OnGameEnded);
	}

    

	/// <summary>
	/// Called every frame.
	/// </summary>
	private void Update() {

		// Only if enabled!
		if(!m_enabled) return;
		if(m_spawnersTree == null) return;

		// Get activation bounds
		// Update every frame in case camera bounds change (i.e. zoom in/out)
		float currentX = 0, currentY = 0;
		if (m_newCamera != null) {
			m_minRect = m_newCamera.activationMinRect;
			m_maxRect = m_newCamera.activationMaxRect;
			currentX = m_newCamera.position.x;
			currentY = m_newCamera.position.y;
		}

		const float delta = 0.5f;
		bool checkBottom = (currentY - m_lastY) < -delta;
		bool checkRight = (currentX - m_lastX) > delta;
		bool checkTop = (currentY - m_lastY) > delta;
		bool checkLeft = (currentX - m_lastX) < -delta;

		// Update timer (optimization to avoid performing the update every frame)
		// Pre-optimization: ~10% CPU usage, 36-39 FPS
		// 0.2s: 37-40 FPS
		m_updateTimer += Time.deltaTime;
		if(m_updateTimer > UPDATE_INTERVAL) {
			// Reset timer
			m_updateTimer = 0f;

			// Only update those spawners closer to the dragon
			// Use the camera detection area for that
			// Since the detection area has a hole in it, divide it in 4 rectangle sub-areas

			//            min.x0                  min.x1
			//              |                       |
			//           _______________________________  _ max.y1
			//          |               2               |
			// min.y1 _ |_______________________________|
			//          |   |                       |   |
			//          |   |                       |   |
			//          | 3 |                       | 1 |
			//          |   |                       |   |
			// min.y0 _ |___|_______________________|___|
			//          |               0               |
			//          |_______________________________| _ max.y0
			//
			//          |                               |
			//        max.x0                         max.x1
			//
			m_selectedSpawners.Clear();
			           

			if (checkBottom)
            {
                // Split it in 4 rectangles that the quadtree can process
                // 1: bottom sub-rect
                m_subRect[0].Set(
                    m_maxRect.x0,
                    m_maxRect.y0,
                    m_maxRect.w,
                    m_minRect.y0 - m_maxRect.y0
                );
                m_spawnersTree.GetHashSetInRange(m_subRect[0], ref m_selectedSpawners);
                m_lastY = currentY;
                //Debug.LogError("BOTTOM");
            }
			if (checkRight)
            {
                // 2: right sub-rect
                m_subRect[1].Set(
                    m_minRect.x1,
                    m_minRect.y0,
                    m_maxRect.x1 - m_minRect.x1,
                    m_minRect.h
                );
                m_spawnersTree.GetHashSetInRange(m_subRect[1], ref m_selectedSpawners);
                m_lastX = currentX;
                //Debug.LogError("RIGHT");
            }
			if (checkTop)
            {
                    // 3: top sub-rect
                    m_subRect[2].Set(
                    m_maxRect.x0,
                    m_minRect.y1,
                    m_maxRect.w,
                    m_maxRect.y1 - m_minRect.y1
                );
                m_spawnersTree.GetHashSetInRange(m_subRect[2], ref m_selectedSpawners);
                m_lastY = currentY;
                //Debug.LogError("TOP");
            }
			if (checkLeft)
            {
                // 4: left sub-rect
                m_subRect[3].Set(
                    m_maxRect.x0,
                    m_minRect.y0,
                    m_minRect.x0 - m_maxRect.x0,
                    m_minRect.h
                );
                m_spawnersTree.GetHashSetInRange(m_subRect[3], ref m_selectedSpawners);
                m_lastX = currentX;
                //Debug.LogError("LEFT");
            }
			// Process all selected spawners!
			foreach(ISpawner item in m_selectedSpawners) {
				if (item.CanRespawn()) {
					//add item into respawn list and begin the respawn process
					m_spawning.Add(item);
					item.Respawn();
				}
			}
		}
		
		m_watch.Start();
        long start = m_watch.ElapsedMilliseconds;
        for (int i = 0; i < m_spawning.Count; i++) {
			ISpawner sp = m_spawning[i];
			if (sp.Respawn()) {
				m_spawning.Remove(sp);
				i = Mathf.Max(0, i - 1);
			}
			if (m_watch.ElapsedMilliseconds - start >= SPAWNING_MAX_TIME) {
				break;
			}
		}
	}

	//------------------------------------------------------------------------//
	// PUBLIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Add a spawner to the manager.
	/// </summary>
	/// <param name="_spawner">The spawner to be added.</param>
	public void Register(ISpawner _spawner) {
		m_spawners.Add(_spawner);
		if(m_spawnersTree != null) m_spawnersTree.Insert(_spawner);
		_spawner.Initialize();
	}

	/// <summary>
	/// Remove a spawner from the manager.
	/// </summary>
	/// <param name="_spawner">The spawner to be removed.</param>
	public void Unregister(ISpawner _spawner) {
		m_spawners.Remove(_spawner);
		if(m_spawnersTree != null) m_spawnersTree.Remove(_spawner);
	}

	/// <summary>
	/// Enable all spawners in the manager.
	/// </summary>
	public void EnableSpawners() {
		// Set flag
		m_enabled = true;
	}

	/// <summary>
	/// Disable all spawners in the manager.
	/// </summary>
	public void DisableSpawners() {
		// Clear spawners
		m_enabled = false;
		for (int i = 0; i < m_spawners.Count; i++) {
			m_spawners[i].ForceRemoveEntities();
		}
		m_selectedSpawners.Clear();
	}

	//------------------------------------------------------------------------//
	// DEBUG METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Draw helper debug stuff.
	/// </summary>
	public void OnDrawGizmosSelected() {
		// Quadtree grid
		if(m_spawnersTree != null) {
			m_spawnersTree.DrawGizmos(Colors.yellow);
		}

		// Selected spawners
		Gizmos.color = Colors.WithAlpha(Colors.yellow, 1.0f);
		foreach(ISpawner item in m_selectedSpawners) {
			Gizmos.DrawSphere(item.boundingRect.center, 0.5f);
		}

		// All spawners gizmos
		for (int i = 0; i < m_spawners.Count; i++) {
			m_spawners[i].DrawStateGizmos();
		}

		// Sub-rectangles
		Gizmos.color = Colors.WithAlpha(Colors.lime, 0.5f);
		Gizmos.DrawCube(new Vector3(m_subRect[0].center.x, m_subRect[0].center.y, 0f), new Vector3(m_subRect[0].width, m_subRect[0].height, 1f));
		Gizmos.DrawCube(new Vector3(m_subRect[2].center.x, m_subRect[2].center.y, 0f), new Vector3(m_subRect[2].width, m_subRect[2].height, 1f));

		Gizmos.color = Colors.WithAlpha(Colors.orange, 0.5f);
		Gizmos.DrawCube(new Vector3(m_subRect[1].center.x, m_subRect[1].center.y, 0f), new Vector3(m_subRect[1].width, m_subRect[1].height, 1f));
		Gizmos.DrawCube(new Vector3(m_subRect[3].center.x, m_subRect[3].center.y, 0f), new Vector3(m_subRect[3].width, m_subRect[3].height, 1f));
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// A new level was loaded.
	/// </summary>
	private void OnLevelLoaded() {
		// Make sure camera reference is valid
		// Spawners are only used in the game and level editor scenes, so we can be sure that both game camera and game scene controller will be present
		Camera gameCamera = InstanceManager.sceneController.mainCamera;
		m_newCamera = gameCamera.GetComponent<GameCamera>();

		// Create and populate QuadTree
		// Get map bounds!
		Rect bounds = new Rect(-440, -100, 1120, 305);	// Default hardcoded values
		LevelMapData data = GameObjectExt.FindComponent<LevelMapData>(true);
		if(data != null) {
			bounds = data.mapCameraBounds;
		}
		m_spawnersTree = new QuadTree<ISpawner>(bounds.x, bounds.y, bounds.width, bounds.height);
		for(int i = 0; i < m_spawners.Count; i++) {
			m_spawnersTree.Insert(m_spawners[i]);
		}

		m_selectedSpawners.Clear();
		m_minRect = m_newCamera.activationMinRect;
		m_spawnersTree.GetHashSetInRange(m_minRect.ToRect(), ref m_selectedSpawners);

		// Process all selected spawners!
		foreach(ISpawner item in m_selectedSpawners) {
			if (item.CanRespawn()) {
				int iterations = 0;
				do {
					iterations++;
				} while (iterations < 100 && !item.Respawn());
			}
		}
	}

	/// <summary>
	/// The game has ended.
	/// </summary>
	private void OnGameEnded() {
		// Clear QuadTree
		m_spawnersTree = null;
		m_selectedSpawners.Clear();
		m_spawners.Clear();

		// Drop camera references
		m_newCamera = null;

		// Make sure manager is disabled
		m_enabled = false;

        Spawner.ResetTotalLogicUnitsSpawned();
    }

    #region debug
    private void Debug_Awake() {        
        Messenger.AddListener<string, bool>(GameEvents.CP_BOOL_CHANGED, Debug_OnChanged);

        // Enable/Disable object depending on the flag
        Debug_SetActive();
    }

    private void Debug_OnDestroy() {        
		Messenger.RemoveListener<string, bool>(GameEvents.CP_BOOL_CHANGED, Debug_OnChanged);
    }

    private void Debug_OnChanged(string _id, bool _newValue) {        
        if (_id == DebugSettings.INGAME_SPAWNERS)
        {
            // Enable/Disable object
            Debug_SetActive();
        }
    }

    private void Debug_SetActive() {
		m_enabled = Prefs.GetBoolPlayer(DebugSettings.INGAME_SPAWNERS);        
    }  
    #endregion
}
