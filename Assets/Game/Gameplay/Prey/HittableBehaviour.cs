using UnityEngine;
using System.Collections;

public class HittableBehaviour : MonoBehaviour {

	[SerializeField] private float m_maxHealth = 25f;
	[SerializeField] private float m_healthRegen = 0.05f;
	private float m_health;

	// Use this for initialization
	void Start () {
		m_health = m_maxHealth;
	}

	void FixedUpdate() {
		m_health = Mathf.Min(m_health + m_healthRegen, m_maxHealth);
	}

	public void OnHit(float _damage) {
		m_health -= _damage;

		if (m_health <= 0) {
			gameObject.SetActive(false);
		}
	}

}
