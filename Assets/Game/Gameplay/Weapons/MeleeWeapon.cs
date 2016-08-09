using UnityEngine;
using System.Collections;

public class MeleeWeapon : MonoBehaviour {

	private Collider m_weapon;

	private float m_damage;
	public float damage { set { m_damage = value; } }

	void Awake() {
		m_weapon = GetComponent<Collider>();
	}

	void OnEnable() {
		m_weapon.enabled = true;
	}

	void OnDisable() {
		m_weapon.enabled = false;
	}

	void OnTriggerEnter(Collider _other) {
		if (_other.tag == "Player") {
			InstanceManager.player.GetComponent<DragonHealthBehaviour>().ReceiveDamage(m_damage, DamageType.NORMAL);
		}
	}
}
