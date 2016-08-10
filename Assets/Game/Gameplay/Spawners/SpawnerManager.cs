using UnityEngine;
using System.Collections.Generic;

public class SpawnerManager : SingletonMonoBehaviour<SpawnerManager> {
	private const float UPDATE_INTERVAL = 0.2f;	// Seconds, avoid updating all the spawners all the time for better performance
	public const float BACKGROUND_LAYER_Z = 45f;

	private List<ISpawner> m_spawners = null;
	private QuadTree<ISpawner> m_spawnersTree = null;
	private bool m_enabled = false;
	private float m_updateTimer = 0f;

	// External references
	private GameCameraController m_camera = null;
	private GameCamera m_newCamera = null;

	// Internal vars
	private FastBounds2D m_minRect = null;	// From the game camera
	private FastBounds2D m_maxRect = null;
	private Rect m_subRect = new Rect();
	private List<ISpawner> m_selectedSpawners = new List<ISpawner>();

	void Awake() {
		m_spawners = new List<ISpawner>();
		m_spawnersTree = new QuadTree<ISpawner>(-600f, -100f, 1000f, 400f);		// [AOC] TODO!! Hardcoded values
	}

	public void Register(ISpawner _spawner) {
		m_spawners.Add(_spawner);
		m_spawnersTree.Insert(_spawner);
		_spawner.Initialize();
	}

	public void Unregister(ISpawner _spawner) {
		m_spawners.Remove(_spawner);
		m_spawnersTree.Remove(_spawner);
	}

	public void EnableSpawners() {
		m_enabled = true;

		// Make sure camera reference is valid
		// Spawners are only used in the game and level editor scenes, so we can be sure that both game camera and game scene controller will be present
		Camera gameCamera = InstanceManager.GetSceneController<GameSceneControllerBase>().gameCamera;
		m_camera = gameCamera.GetComponent<GameCameraController>();
		m_newCamera = gameCamera.GetComponent<GameCamera>();
	}

	public void DisableSpawners() {
		m_enabled = false;
		for (int i = 0; i < m_spawners.Count; i++) {
			m_spawners[i].ForceRemoveEntities();
		}

		// Drop camera reference
		m_camera = null;
		m_newCamera = null;
	}

	void Update() {
		if (m_enabled) {
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
				if(DebugSettings.newCameraSystem && m_newCamera != null) {
					m_minRect = m_newCamera.activationMinRect;
					m_maxRect = m_newCamera.activationMaxRect;
				} else if(m_camera != null) {
					m_minRect = m_camera.activationMinRect;
					m_maxRect = m_camera.activationMaxRect;
				}

				// Split it in 4 rectangles that the quadtree can process
				// 1: top sub-rect
				m_subRect.Set(
					m_maxRect.x0, 
					m_maxRect.y0,
					m_maxRect.w,
					m_minRect.y0 - m_maxRect.y0
				);
				m_selectedSpawners.AddRange(m_spawnersTree.GetItemsInRange(m_subRect));

				// 2: right sub-rect
				m_subRect.Set(
					m_minRect.x1, 
					m_minRect.y0,
					m_maxRect.x1 - m_minRect.x1,
					m_minRect.h
				);
				m_selectedSpawners.AddRange(m_spawnersTree.GetItemsInRange(m_subRect));

				// 3: bottom sub-rect
				m_subRect.Set(
					m_maxRect.x0,
					m_minRect.y1,
					m_maxRect.w,
					m_maxRect.y1 - m_minRect.y1
				);
				m_selectedSpawners.AddRange(m_spawnersTree.GetItemsInRange(m_subRect));

				// 4: left sub-rect
				m_subRect.Set(
					m_maxRect.x0,
					m_minRect.y0,
					m_minRect.x0 - m_maxRect.x0,
					m_minRect.h
				);
				m_selectedSpawners.AddRange(m_spawnersTree.GetItemsInRange(m_subRect));

				// Process all selected spawners!
				for(int i = 0; i < m_selectedSpawners.Count; i++) {
					m_selectedSpawners[i].CheckRespawn();
				}

				// Perform the update on all the spawners
				/*for (int i = 0; i < m_spawners.Count; i++) {
					m_spawners[i].CheckRespawn();
				}*/
			}
		}
	}

	public void OnDrawGizmosSelected() {
		m_spawnersTree.DrawGizmos(Colors.yellow);

		Gizmos.color = Colors.WithAlpha(Colors.yellow, 1.0f);
		for(int i = 0; i < m_selectedSpawners.Count; i++) {
			Gizmos.DrawSphere(m_selectedSpawners[i].transform.position, 0.5f);
		}
	}
}
