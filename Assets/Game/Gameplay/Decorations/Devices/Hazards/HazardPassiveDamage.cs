using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HazardPassiveDamage : MonoBehaviour {

	[SerializeField] private float m_damage = 10f;
	[SerializeField] private DamageType m_type = DamageType.NORMAL;
	[SerializeField] private float m_delay = 1f;

	private Transform m_transform;
	private DragonHealthBehaviour m_dragon;
	private float m_timer;


	// Use this for initialization
	void Start() {
		m_transform = transform;

		m_timer = 0f;
		m_dragon = InstanceManager.player.dragonHealthBehaviour;
	}
	
	// Update is called once per frame
	void Update() {
		if (m_timer > 0f) {
			m_timer -= Time.deltaTime;
			if (m_timer <= 0f) {
				m_timer = 0f;
			}
		}
	}

	void OnTriggerEnter(Collider _other) {
		if (m_timer <= 0f) {
			if (_other.CompareTag("Player")) {
				m_dragon.ReceiveDamage(m_damage, m_type, m_transform);
			}
			m_timer = m_delay;
		}
	}
}
