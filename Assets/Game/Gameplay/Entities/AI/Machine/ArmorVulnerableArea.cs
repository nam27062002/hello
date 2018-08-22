using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArmorVulnerableArea : MonoBehaviour {
	[SerializeField] private GameObject m_machineRoot;
	[SerializeField] private float m_timeBetweenAttaks = 1f;
	[SerializeField] private ParticleData m_hitParticle;

	private IArmored m_machine;
	private float m_timer;

	void Start() {
		m_machine = m_machineRoot.GetComponent<IArmored>();
	}

	// Use this for initialization
	void OnEnable () {
		m_timer = 0f;
		m_hitParticle.CreatePool();
	}
	
	// Update is called once per frame
	void Update () {
		if (m_timer > 0f) {
			m_timer -= Time.deltaTime;
		}
	}

	void OnTriggerEnter(Collider _other) {
		if (_other.CompareTag("Player") && m_timer <= 0f) {
			DragonPlayer player = InstanceManager.player;

			bool isHitValid = m_machine.ReduceDurability(player.IsBreakingMovement(), IEntity.Type.PLAYER);

			if (isHitValid)	{
				m_hitParticle.Spawn(transform.position + m_hitParticle.offset);
				m_timer = m_timeBetweenAttaks;
			}
		}
	}
}
