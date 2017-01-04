using UnityEngine;
using System.Collections;

public class MeleeWeapon : MonoBehaviour {

	private Collider m_weapon;
	private TrailRenderer m_trail;

	private float m_damage;
	public float damage { set { m_damage = value; } }

	void Awake() {
		m_weapon = GetComponent<Collider>();
		m_trail = GetComponentInChildren<TrailRenderer>();
	}

	void OnEnable() {
		m_weapon.enabled = true;
		if (m_trail) m_trail.enabled = true;
	}

	void OnDisable() {
		m_weapon.enabled = false;
		if (m_trail) m_trail.enabled = false;
	}

	void OnTriggerEnter(Collider _other) {
		if (_other.CompareTag("Player")) {
			InstanceManager.player.GetComponent<DragonHealthBehaviour>().ReceiveDamage(m_damage, DamageType.NORMAL);
		}
	}
}
