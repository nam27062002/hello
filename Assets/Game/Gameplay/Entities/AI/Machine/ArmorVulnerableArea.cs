using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArmorVulnerableArea : MonoBehaviour {

	[SerializeField] private AI.MachineArmored m_machine;
	[SerializeField] private bool m_needBoost = true;
	[SerializeField] private float m_timeBetweenAttaks = 1f;
	[SerializeField] private ParticleData m_hitParticle;


	private float m_timer;

	void Start() {
		if (m_hitParticle.IsValid()) {
			ParticleManager.CreatePool(m_hitParticle, 10);
		}
	}

	// Use this for initialization
	void OnEnable () {
		m_timer = 0f;
	}
	
	// Update is called once per frame
	void Update () {
		if (m_timer > 0f) {
			m_timer -= Time.deltaTime;
		}
	}

	void OnTriggerEnter(Collider _other) {		
		if (_other.CompareTag("Player") && m_timer <= 0f) {
			DragonBoostBehaviour boost = InstanceManager.player.dragonBoostBehaviour;
			if (!m_needBoost || boost.IsBoostActive())	{
				if (m_hitParticle.IsValid()) {
					ParticleManager.Spawn(m_hitParticle, transform.position + m_hitParticle.offset);
				}

				m_machine.ReduceDurability();
				m_timer = m_timeBetweenAttaks;
			}
		}
	}
}
