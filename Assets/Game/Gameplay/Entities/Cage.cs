using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cage : IEntity {
	
	[SerializeField] private float m_disableDelay = 270f;

	private ISpawner m_spawner;
	private float m_timer;
	private bool m_wasDestroyed;
	protected CageBehaviour m_behaviour;
	public CageBehaviour behaviour{
		get{ return m_behaviour; }
	}

	private CircleArea2D m_bounds;
	public override CircleArea2D circleArea { get{ return m_bounds; } }

	private bool m_disabling;

	//
	protected override void Awake() {
		base.Awake();
		m_bounds = GetComponentInChildren<CircleArea2D>();
		m_behaviour = GetComponent<CageBehaviour>();
		m_maxHealth = 1f;
	}

	void OnDestroy() {
		if (EntityManager.instance != null) {
			EntityManager.instance.UnregisterEntityCage(this);
		}
	}

	public void SetDestroyedByDragon() {
		m_wasDestroyed = true;
	}

	//
	public override void Spawn(ISpawner _spawner) {		
		m_spawner = _spawner;
		m_wasDestroyed = false;
		m_timer = 0f;
		m_disabling = false;
		base.Spawn(_spawner);
	}

	//
	public override void Disable(bool _destroyed) {	
		if (!m_disabling) {
			if (m_wasDestroyed) {
				m_timer = m_disableDelay;
			} else {
				m_timer = 0.25f;
			}
			m_disabling = true;
		}
	}

	public bool IntersectsWith(Vector2 _center, float _radius) {
		if (m_bounds != null) {
			return m_bounds.Overlaps(_center, _radius);
		} 

		// return _r.Contains(transform.position);
		float sqrMagnitude = (_center - (Vector2)transform.position).sqrMagnitude;
		return ( sqrMagnitude <= _radius * _radius );	
	}

	// Update is called once per frame
	public override void CustomUpdate() {
		base.CustomUpdate();
		if (m_disabling) {
			m_timer -= Time.deltaTime;
			if (m_timer <= 0f) {
				m_spawner.RemoveEntity(this, m_wasDestroyed);
				base.Disable(m_wasDestroyed);
			}
		}
	}
}
