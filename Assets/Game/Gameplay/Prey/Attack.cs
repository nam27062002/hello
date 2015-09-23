using UnityEngine;
using System.Collections;

[DisallowMultipleComponent]
[AddComponentMenu("Behaviour/Prey/Attack")]
public class Attack : Steering {

	[SerializeField] private float m_range;
	[SerializeField] private float m_delay;
	[SerializeField] private float m_damage;


	private float m_timer;
	private float m_rangeSqr;

	private int m_hitCount;
	public int hitCount { get { return m_hitCount; } }

	private DragonHealthBehaviour m_dragon;


	void Awake() {

		m_dragon = null;
		enabled = false;
	}

	void OnEnable() {

		m_timer = 0;
		m_hitCount = 0;
		m_rangeSqr = m_range * m_range;
		m_dragon = InstanceManager.player.GetComponent<DragonHealthBehaviour>();
	}

	void OnDisable() {

		m_dragon = null;
	}

	void Update() {

		Vector2 v = transform.position - m_dragon.transform.position;
		if (v.sqrMagnitude <= m_rangeSqr) {
			m_timer -= Time.deltaTime;
			if (m_timer <= 0) {
				m_hitCount++;
				m_timer = m_delay;
				m_dragon.ReceiveDamage(m_damage);
				if (!m_dragon.IsAlive()) {
					enabled = false;
				}
			}
		}
	}
}
