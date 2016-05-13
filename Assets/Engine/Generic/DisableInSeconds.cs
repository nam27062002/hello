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
	private ParticleSystem m_particleSystem;

	void Start() {
		// lets grab the particle system if it exists. 
		m_particleSystem = GetComponent<ParticleSystem>();
	}

	void OnEnable() {
		m_activeTimer = m_activeTime;
	}

	void Update() {		
		m_activeTimer -= Time.deltaTime;
		if (m_activeTimer < 0f) {
			if (m_particleSystem != null) {
				// we are disabling a particle system
				m_particleSystem.Stop();
				StartCoroutine(WaitEndEmissionToDeactivate());
			} else {
				// it's a simple game object
				Disable();
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
		while (m_particleSystem.particleCount > 0) {
			yield return null;
		}

		Disable();

		yield return null;
	}
}