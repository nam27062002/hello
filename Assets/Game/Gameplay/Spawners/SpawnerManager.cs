using UnityEngine;
using System.Collections.Generic;

public class SpawnerManager : SingletonMonoBehaviour<SpawnerManager> {

	private List<ISpawner> m_spawners;

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

	void Update() {
		for (int i = 0; i < m_spawners.Count; i++) {
			m_spawners[i].UpdateTimers();
			m_spawners[i].Respawn();
			m_spawners[i].UpdateLogic();
		}
	}
}
