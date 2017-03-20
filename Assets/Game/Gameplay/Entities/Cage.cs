using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cage : IEntity {
	
	private ISpawner m_spawner;
	private float m_timer;
	private bool m_wasDestroyed;

	//
	protected override void Awake() {
		base.Awake();
		m_maxHealth = 1f;
	}

	void OnDestroy() {
		if (EntityManager.instance != null) {
			EntityManager.instance.UnregisterEntityCage(this);
		}
	}

	//
	public override void Spawn(ISpawner _spawner) {		
		m_spawner = _spawner;
		m_timer = 0f;
		base.Spawn(_spawner);
	}

	//
	public override void Disable(bool _destroyed) {	
		m_wasDestroyed = _destroyed;
		m_timer = 0.25f;
	}

	// Update is called once per frame
	void Update() {
		if (m_timer > 0f) {
			m_timer -= Time.deltaTime;
			if (m_timer <= 0f) {	
				m_spawner.RemoveEntity(gameObject, m_wasDestroyed);
				base.Disable(m_wasDestroyed);
			}
		}
	}
}
