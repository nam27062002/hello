﻿using UnityEngine;
using System.Collections;

public class ReturnParticleAfterFinish : MonoBehaviour {

	private ParticleSystem m_particleSystems;
	// Use this for initialization
	void Start () {
		m_particleSystems = GetComponent<ParticleSystem>();
	}
	
	// Update is called once per frame
	void Update () {
		if ( m_particleSystems != null && !m_particleSystems.IsAlive() )
			Disable();
	}

	private void Disable() {
		gameObject.SetActive(false);
		ParticleManager.ReturnInstance(gameObject);
	}
}
