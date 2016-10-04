using UnityEngine;
using System.Collections;

public class DisableInSeconds : MonoBehaviour {

	public enum PoolType {
		PoolManager = 0,
		ParticleManager
	};

	[SerializeField] private float m_activeTime = 1f;
	public float activeTime { set { m_activeTime = value; m_activeTimer = m_activeTime; } }
	[SerializeField] private PoolType m_returnTo = PoolType.PoolManager;

	private float m_activeTimer;
	private bool m_coroutineRunning;
	private ParticleSystem[] m_particleSystems;

	void Start() {
		// lets grab the particle system if it exists. 
		m_particleSystems = GetComponentsInChildren<ParticleSystem>();
	}

	void OnEnable() {
		m_activeTimer = m_activeTime;
		m_coroutineRunning = false;
	}

	void Update() {	
		if (!m_coroutineRunning) {	
			m_activeTimer -= Time.deltaTime;
			if (m_activeTimer < 0f) {
				if (m_particleSystems.Length > 0) {
					// we are disabling a particle system
					for (int i = 0; i < m_particleSystems.Length; i++) {
						if (m_particleSystems[i].loop) {
							ParticleSystem.EmissionModule em = m_particleSystems[i].emission;
							em.enabled = false;
							m_particleSystems[i].Stop();
						}
					}
					StartCoroutine(WaitEndEmissionToDeactivate());
					m_coroutineRunning = true;
				} else {
					// it's a simple game object
					Disable();
				}
			}
		}
	}

	private void Disable() {
		gameObject.SetActive(false);
		switch(m_returnTo) {
			case PoolType.PoolManager: 		PoolManager.ReturnInstance(gameObject); 	break;
			case PoolType.ParticleManager: 	ParticleManager.ReturnInstance(gameObject); break;
		}
	}

	IEnumerator WaitEndEmissionToDeactivate() {
		bool alive = false;

		do {
			alive = false;
			for (int i = 0; i < m_particleSystems.Length; i++) {
				alive = alive || m_particleSystems[i].IsAlive();
			}

			if (alive) {
				yield return null;
			}
		} while (alive);

		Disable();
	}
}