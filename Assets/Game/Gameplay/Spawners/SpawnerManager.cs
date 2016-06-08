using UnityEngine;
using System.Collections.Generic;

public class SpawnerManager : SingletonMonoBehaviour<SpawnerManager> {

	private List<ISpawner> m_spawners;
	private bool m_enabled = false;

	void Awake() {
		m_spawners = new List<ISpawner>();
	}

	public void Register(ISpawner _spawner) {
		m_spawners.Add(_spawner);
		_spawner.Initialize();
	}

	public void Unregister(ISpawner _spawner) {
		m_spawners.Remove(_spawner);
	}

	public void EnableSpawners() {
		m_enabled = true;
	}

	public void DisableSpawners() {
		m_enabled = false;
		for (int i = 0; i < m_spawners.Count; i++) {
			m_spawners[i].ForceRemoveEntities();
		}
	}

	void Update() {
		if (m_enabled) {
			for (int i = 0; i < m_spawners.Count; i++) {
				m_spawners[i].UpdateTimers();
				m_spawners[i].UpdateLogic();
			}
		}
	}
}
