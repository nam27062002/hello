using UnityEngine;
using System.Collections;

public class MeleeWeapon : MonoBehaviour {

	[SerializeField] private float m_knockback = 0;
	[SerializeField] private float m_timeBetweenHits = 0.5f;
	[SerializeField] private DamageType m_damageType = DamageType.NORMAL;

	[SeparatorAttribute("Trail effect")]
	[SerializeField] private Xft.XWeaponTrail m_trail;
	[SerializeField] private ParticleSystem[] m_trailParticles = new ParticleSystem[0];


	private Collider m_weapon;

	private float m_damage;
	public float damage { set { m_damage = value; } }

	private float m_timer;
	private float m_timerPosition;
	private Vector3 m_lastPosition;

	void Awake() {
		m_weapon = GetComponent<Collider>();
	}

	void OnEnable() {
		m_weapon.enabled = true;
		m_lastPosition = transform.position;

		m_timer = 0;
		m_timerPosition = 0.25f;

		if (m_trail) m_trail.Activate();
		for (int i = 0; i < m_trailParticles.Length; i++) {
			if (m_trailParticles[i] != null) {
				m_trailParticles[i].Clear();
				ParticleSystem.EmissionModule em = m_trailParticles[i].emission;
				em.enabled = true;
				m_trailParticles[i].Play();
			}
		}
	}

	void OnDisable() {
		m_weapon.enabled = false;

		if (m_trail) m_trail.Deactivate();
		for (int i = 0; i < m_trailParticles.Length; i++) {
			if (m_trailParticles[i] != null) {
				ParticleSystem.EmissionModule em = m_trailParticles[i].emission;
				if (em.enabled && m_trailParticles[i].loop) {
					em.enabled = false;
					m_trailParticles[i].Stop();
				}
			}
		}
	}

	void Update() {
		if (m_timer > 0f) {
			m_timer -= Time.deltaTime;
		}

		m_timerPosition -= Time.deltaTime;
		if (m_timerPosition <= 0f) {
			m_lastPosition = transform.position;
			m_timerPosition = 0.25f;
		}
	}

	void OnTriggerEnter(Collider _other) {
		if (m_timer <= 0f && _other.CompareTag("Player")) {			
			if (m_knockback > 0) {
				DragonMotion dragonMotion = InstanceManager.player.dragonMotion;

				Vector3 knockBack = transform.position - m_lastPosition;
				knockBack.z = 0f;
				knockBack.Normalize();

				knockBack *= m_knockback;

				dragonMotion.AddForce(knockBack);
			}
			InstanceManager.player.dragonHealthBehaviour.ReceiveDamage(m_damage, m_damageType);
			m_timer = m_timeBetweenHits;
		}
	}
}
