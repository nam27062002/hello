using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleDataPlace : MonoBehaviour {

	public ParticleData m_particle;
	private GameObject m_particleInstance = null;

	public void Init()
	{
		m_particle.CreatePool();
	}

	public void ShowParticle()
	{
		if ( m_particleInstance == null )
		{
			m_particleInstance = m_particle.Spawn();
			if (m_particleInstance != null) {
				// As children of ourselves
				// Particle system should already be created to match the zero position
				m_particleInstance.transform.SetParentAndReset(this.transform);
				m_particleInstance.transform.position += m_particle.offset;
				m_particleInstance.transform.rotation = this.transform.rotation;
			}
		}
	}


	public void HideParticle()
	{
		if ( m_particleInstance != null )
		{
			m_particle.ReturnInstance(m_particleInstance.gameObject);
			m_particleInstance = null;
		}
	}

}
