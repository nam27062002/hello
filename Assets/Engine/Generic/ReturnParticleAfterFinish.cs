using UnityEngine;
using System.Collections;

public class ReturnParticleAfterFinish : MonoBehaviour {

	private ParticleSystem m_particleSystems;
	private ParticleHandler m_handler;

	// Use this for initialization
	void Start () {
		m_particleSystems = GetComponent<ParticleSystem>();
		m_handler = ParticleManager.GetHandler(gameObject.name);
	}
	
	// Update is called once per frame
	void Update () {
		if (m_particleSystems != null && !m_particleSystems.IsAlive())
			Disable();
	}

	private void Disable() {
		m_handler.ReturnInstance(gameObject);
	}
}
