using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cage : IEntity {
	
	private ISpawner m_spawner;


	void Awake() {
		m_maxHealth = 1f;
	}

	public override void Spawn(ISpawner _spawner) {		
		m_spawner = _spawner;
		base.Spawn(_spawner);
	}

	public override void Disable(bool _destroyed) {		
		base.Disable(_destroyed);

		m_spawner.RemoveEntity(gameObject, _destroyed);
	}
}
