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
public class SpawnerManager : SingletonMonoBehaviour<SpawnerManager> {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	private const float UPDATE_INTERVAL = 0.2f;	// Seconds, avoid updating all the spawners all the time for better performance
	public const float BACKGROUND_LAYER_Z = 45f;

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
	private GameCameraController m_camera = null;
	private GameCamera m_newCamera = null;

	// Detection area
	private FastBounds2D m_minRect = null;	// From the game camera
	private FastBounds2D m_maxRect = null;
	private Rect[] m_subRect = new Rect[4];
	private List<ISpawner> m_selectedSpawners = new List<ISpawner>();

	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Inititalization.
	/// </summary>
	private void Awake() {
		m_spawners = new List<ISpawner>();

		// Subscribe to external events
		Messenger.AddListener(GameEvents.GAME_LEVEL_LOADED, OnLevelLoaded);
		Messenger.AddListener(GameEvents.GAME_ENDED, OnGameEnded);
	}

	/// <summary>
	/// Destructor.
	/// </summary>
	private void OnDestroy() {
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
			//           _______________________________  _ max.y0
			//          |               1               |
			// min.y0 _ |_______________________________|
			//          |   |                       |   |
			//          |   |                       |   |
			//          | 4 |                       | 2 |
			//          |   |                       |   |
			// min.y1 _ |___|_______________________|___|
			//          |               3               |
			//          |_______________________________| _ max.y1
			//
			//          |                               |
			//        max.x0                         max.x1
			//
			m_selectedSpawners.Clear();

			// Get activation bounds
			// Update every frame in case camera bounds change (i.e. zoom in/out)
			if(DebugSettings.newCameraSystem && m_newCamera != null) {
				m_minRect = m_newCamera.activationMinRect;
				m_maxRect = m_newCamera.activationMaxRect;
			} else if(m_camera != null) {
				m_minRect = m_camera.activationMinRect;
				m_maxRect = m_camera.activationMaxRect;
			}

			// Split it in 4 rectangles that the quadtree can process
			// 1: top sub-rect
			m_subRect[0].Set(
				m_maxRect.x0, 
				m_maxRect.y0,
				m_maxRect.w,
				m_minRect.y0 - m_maxRect.y0
			);
			m_spawnersTree.AddItemsInRange(m_subRect[0], ref m_selectedSpawners);

			// 2: right sub-rect
			m_subRect[1].Set(
				m_minRect.x1, 
				m_minRect.y0,
				m_maxRect.x1 - m_minRect.x1,
				m_minRect.h
			);
			m_spawnersTree.AddItemsInRange(m_subRect[1], ref m_selectedSpawners);

			// 3: bottom sub-rect
			m_subRect[2].Set(
				m_maxRect.x0,
				m_minRect.y1,
				m_maxRect.w,
				m_maxRect.y1 - m_minRect.y1
			);
			m_spawnersTree.AddItemsInRange(m_subRect[2], ref m_selectedSpawners);

			// 4: left sub-rect
			m_subRect[3].Set(
				m_maxRect.x0,
				m_minRect.y0,
				m_minRect.x0 - m_maxRect.x0,
				m_minRect.h
			);
			m_spawnersTree.AddItemsInRange(m_subRect[3], ref m_selectedSpawners);

			// Process all selected spawners!
			for(int i = 0; i < m_selectedSpawners.Count; i++) {
				m_selectedSpawners[i].CheckRespawn();
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
		for(int i = 0; i < m_selectedSpawners.Count; i++) {
			Gizmos.DrawSphere(m_selectedSpawners[i].transform.position, 0.5f);
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
		Camera gameCamera = InstanceManager.GetSceneController<GameSceneControllerBase>().gameCamera;
		m_camera = gameCamera.GetComponent<GameCameraController>();
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
	}

	/// <summary>
	/// The game has ended.
	/// </summary>
	private void OnGameEnded() {
		// Clear QuadTree
		m_spawnersTree = null;
		m_selectedSpawners.Clear();

		// Drop camera references
		m_camera = null;
		m_newCamera = null;

		// Make sure manager is disabled
		m_enabled = false;
	}
}
