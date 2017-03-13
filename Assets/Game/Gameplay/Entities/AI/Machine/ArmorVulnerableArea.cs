using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArmorVulnerableArea : MonoBehaviour {

	[SerializeField] private AI.MachineArmored m_machine;
	[SerializeField] private float m_timeBetweenAttaks = 1f;
	[SerializeField] private ParticleData m_hitParticle;


	private float m_timer;

	void Start() {
		if (m_hitParticle.IsValid()) {
			ParticleManager.CreatePool(m_hitParticle);
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

			bool isHitValid = m_machine.ReduceDurability(boost.IsBoostActive());

			if (isHitValid)	{
				if (m_hitParticle.IsValid()) {
					ParticleManager.Spawn(m_hitParticle, transform.position + m_hitParticle.offset);
				}


				m_timer = m_timeBetweenAttaks;
			}
		}
	}
}
