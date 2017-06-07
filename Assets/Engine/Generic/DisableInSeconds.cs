using UnityEngine;
using System.Collections.Generic;

public class DisableInSeconds : MonoBehaviour {

	public enum PoolType {
		PoolManager = 0,
		ParticleManager,
		UIPoolManager
	};

	[SerializeField] private float m_activeTime = 1f;
	public float activeTime { set { m_activeTime = value; m_activeTimer = m_activeTime; } }
	[SerializeField] private PoolType m_returnTo = PoolType.PoolManager;
	[SerializeField] private bool m_disableOnInvisible = true;

	private float m_activeTimer;
//	private bool m_coroutineRunning;
	private List<ParticleSystem> m_particleSystems;

	private PoolHandler m_poolHandler;
	private ParticleHandler m_particleHandler;


    void Start() {
		// lets grab the particle system if it exists. 
		m_particleSystems = transform.FindComponentsRecursive<ParticleSystem>();

		if (m_returnTo == PoolType.PoolManager) {
			m_poolHandler = PoolManager.GetHandler(this.gameObject.name);
		} else if (m_returnTo == PoolType.ParticleManager) {
			m_particleHandler = ParticleManager.GetHandler(this.gameObject.name);
		} else if (m_returnTo == PoolType.UIPoolManager) {
			m_poolHandler = UIPoolManager.GetHandler(this.gameObject.name);
		}
	}

	void OnEnable() {
		m_activeTimer = m_activeTime;
//		m_coroutineRunning = false;
	}

	void OnDisable() {
		if (m_activeTime > 0f) {
			Disable();
		}
	}

	void Update() {

		m_activeTimer -= Time.deltaTime;
		if (m_activeTimer < 0f) {
			if (m_particleSystems.Count > 0)
            {
                // we are disabling a particle system
                bool alive = false;
				for (int i = 0; i < m_particleSystems.Count; i++)
                {
                    ParticleSystem ps = m_particleSystems[i];
                    ParticleSystem.EmissionModule em = ps.emission;
					if (em.enabled && m_particleSystems[i].main.loop)
                    {
                        em.enabled = false;
                        ps.Stop();
                    }

                    if (ps.IsAlive())
                    {
                        alive = true;
                    }
                }

                if (!alive)
                {
                    Disable();
                }
            }
            else
            {
                // it's a simple game object
                Disable();
            }
        }
    }

	private void Disable() {
		//gameObject.SetActive(false);
		switch(m_returnTo) {
			case PoolType.PoolManager: 	
			case PoolType.UIPoolManager:	
				if (m_poolHandler != null) m_poolHandler.ReturnInstance(gameObject); 		
				break;
			case PoolType.ParticleManager: 	
				if (m_particleHandler != null) m_particleHandler.ReturnInstance(gameObject);
				break;
		}
	}
/*
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
*/
    void OnBecameInvisible()
    {
		if ( ApplicationManager.IsAlive && m_disableOnInvisible)
    	{
	        // we are disabling a particle system
			for (int i = 0; i < m_particleSystems.Count; i++)
	        {
	            if (m_particleSystems[i].main.loop)
	            {
	                ParticleSystem.EmissionModule em = m_particleSystems[i].emission;
	                em.enabled = false;
	                m_particleSystems[i].Stop();
	            }
	        }

	        Disable();
        }
    }


}