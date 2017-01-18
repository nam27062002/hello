using UnityEngine;
using System.Collections;

public class MeleeWeapon : MonoBehaviour {

	[SerializeField] private float m_knockback = 0;

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
			if (m_knockback > 0) {
				DragonMotion dragonMotion = InstanceManager.player.dragonMotion;

				Vector3 knockBack = dragonMotion.transform.position - transform.position;
				knockBack.z = 0f;
				knockBack.Normalize();

				knockBack *= m_knockback;

				dragonMotion.AddForce(knockBack);
			}
			InstanceManager.player.dragonHealthBehaviour.ReceiveDamage(m_damage, DamageType.NORMAL);
		}
	}
}
