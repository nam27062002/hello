using UnityEngine;
using System.Collections.Generic;

public class SpawnerAreaManager : SingletonMonoBehaviour<SpawnerAreaManager> {

	private const float CELL_SIZE = 5f;

	private List<ISpawner> m_spawners;
	private Rect m_mapBounds;

	private ISpawner[,] m_grid;
	private int m_rows;
	private int m_cols;

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


	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// A new level was loaded.
	/// </summary>
	private void OnLevelLoaded() {
		// Create and populate QuadTree
		// Get map bounds!
		m_mapBounds = new Rect(-440, -100, 1120, 305);	// Default hardcoded values
		LevelMapData data = GameObjectExt.FindComponent<LevelMapData>(true);
		if(data != null) {
			m_mapBounds = data.mapCameraBounds;
		}

		m_rows = (int)Mathf.Ceil(m_mapBounds.height / CELL_SIZE) + 1;
		m_cols = (int)Mathf.Ceil(m_mapBounds.width / CELL_SIZE) + 1;

		m_grid = new ISpawner[m_rows, m_cols];

		for (int i = 0; i < m_spawners.Count; i++) {
			Bounds bounds = m_spawners[i].area.bounds;

			int minX = (int)Mathf.Min(m_cols, Mathf.Max(0, ((bounds.min.x - m_mapBounds.xMin) / CELL_SIZE)));
			int minY = (int)Mathf.Min(m_rows, Mathf.Max(0, ((bounds.min.y - m_mapBounds.yMin) / CELL_SIZE)));

			int maxX = (int)Mathf.Min(m_cols, Mathf.Max(0, ((bounds.max.x - m_mapBounds.xMin) / CELL_SIZE)));
			int maxY = (int)Mathf.Min(m_rows, Mathf.Max(0, ((bounds.max.y - m_mapBounds.yMin) / CELL_SIZE)));

			for (int r = minY; r < maxY; r++) {
				for (int c = minX; c < maxX; c++) {
					m_grid[r, c] = m_spawners[i];
				}
			}
		}

		m_spawners.Clear();
	}

	/// <summary>
	/// The game has ended.
	/// </summary>
	private void OnGameEnded() {
		m_grid = null;
		m_spawners.Clear();
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
	}

	public void UpdateAreaAt(Vector3 _pos, ref AI.AIPilot _pilot) {
		int c = (int)Mathf.Min(m_cols, Mathf.Max(0, ((_pos.x - m_mapBounds.xMin) / CELL_SIZE)));
		int r = (int)Mathf.Min(m_rows, Mathf.Max(0, ((_pos.y - m_mapBounds.yMin) / CELL_SIZE)));

		_pilot.SetArea(m_grid[r, c]);
	}

	//------------------------------------------------------------------------//
	// DEBUG METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Draw helper debug stuff.
	/// </summary>
	public void OnDrawGizmosSelected() {
		if (m_grid != null) {
			Gizmos.color = Colors.WithAlpha(Colors.purple, 0.5f);

			Vector2 start = m_mapBounds.min + Vector2.one * CELL_SIZE;
			for (int r = 0; r < m_rows; r++) {
				for (int c = 0; c < m_cols; c++) {
					if (m_grid[r, c] == null) {
						Gizmos.DrawWireCube(start + (Vector2.right * c * CELL_SIZE) + (Vector2.up * r * CELL_SIZE), Vector3.one * CELL_SIZE);
					} else {
						Gizmos.DrawCube(start + (Vector2.right * c * CELL_SIZE) + (Vector2.up * r * CELL_SIZE), Vector3.one * CELL_SIZE);
					}
				}
			}
		}
	}
}
