﻿using UnityEngine;
using System.Collections.Generic;

public class DisableInSeconds : MonoBehaviour {

	public enum PoolType {
		PoolManager = 0,
		ParticleManager,
		UIPoolManager,
        DisabledState
	};

	[SerializeField] private float m_activeTime = 1f;
	public float activeTime { set { m_activeTime = value; m_activeTimer = m_activeTime; } }
	[SerializeField] private PoolType m_returnTo = PoolType.PoolManager;
	[SerializeField] private bool m_disableOnInvisible = true;
	[SerializeField] private bool m_alwaysActive = true;

	private bool m_active = false;

	private float m_activeTimer;
	private ParticleControl m_particleControl;

	private PoolHandler m_poolHandler;
	private ParticleHandler m_particleHandler;


    void Start() {
		m_active = false;

		// lets grab the particle system if it exists. 
		m_particleControl = GetComponent<ParticleControl>();

		if (m_returnTo == PoolType.PoolManager) {
			m_poolHandler = PoolManager.GetHandler(this.gameObject.name);
		} else if (m_returnTo == PoolType.ParticleManager) {
			m_particleHandler = ParticleManager.GetHandler(this.gameObject.name);
		} else if (m_returnTo == PoolType.UIPoolManager) {
			m_poolHandler = UIPoolManager.GetHandler(this.gameObject.name);
		}
	}

	public void Activate() {
		m_active = true;
	}

	void OnEnable() {
		m_activeTimer = m_activeTime;
	}

	void OnDisable() {
		Disable();
	}

	void Update() {
		if (m_alwaysActive || m_active) {
			m_activeTimer -= Time.deltaTime;
			if (m_activeTimer < 0f) {
				if (m_particleControl != null) {
	                // we are disabling a particle system
					bool isStopped = m_particleControl.Stop();
					if (isStopped) {
	                    Disable();
	                }
	            } else {
	                // it's a simple game object
	                Disable();
	            }
	        }
		}
    }

	private void Disable() {
		m_active = false;

		switch(m_returnTo) {
			case PoolType.PoolManager: 	
			case PoolType.UIPoolManager:	
				if (m_poolHandler != null) m_poolHandler.ReturnInstance(gameObject); 		
				break;
			case PoolType.ParticleManager: 	
				if (m_particleHandler != null) m_particleHandler.ReturnInstance(gameObject);
				break;
			case PoolType.DisabledState:
				gameObject.SetActive(false);
				break;
		}
	}

    void OnBecameInvisible()  {
		if (ApplicationManager.IsAlive && m_disableOnInvisible) {
	        // we are disabling a particle system
			if (m_particleControl != null) {
				m_particleControl.Stop();
	        }
	        Disable();
        }
	}
}