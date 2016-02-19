using UnityEngine;
using System.Collections.Generic;

public class SpawnerManager : SingletonMonoBehaviour<SpawnerManager> {

	private List<ISpawner> m_spawners;

	void Awake() {
		m_spawners = new List<ISpawner>();
	}

	public void Register(ISpawner _spawner) {
		m_spawners.Add(_spawner);
	}

	public void Unregister(ISpawner _spawner) {
		m_spawners.Remove(_spawner);
	}
}
