using UnityEngine;
using System.Collections;

public class AttackPassiveBehaviour : MonoBehaviour {
	
	[SerializeField] private float m_damage;
	[SerializeField] private float m_attackDelay;

	private float m_timer;

	// Use this for initialization
	void Start () {
		m_timer = 0;
	}

	void Update() {
		if (m_timer > 0) {
			m_timer -= Time.deltaTime;
			if (m_timer <= 0) {
				m_timer = 0;
			}
		}
	}

	void OnTriggerStay(Collider _other) {
		if (m_timer <= 0 && _other.tag == "Player") {
			DragonHealthBehaviour dragon = InstanceManager.player.GetComponent<DragonHealthBehaviour>();
			if (dragon != null) {
				dragon.ReceiveDamage(m_damage, DamageType.NORMAL);
				m_timer = m_attackDelay;
			}
		}
	}
}
