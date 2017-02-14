using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleTrigger : MonoBehaviour {

	public ParticleData m_particle;
	public enum TriggerEvent
	{
		Enter,
		Exit,
		Both
	};
	public TriggerEvent m_event;

	void Start()
	{
		ParticleManager.CreatePool( m_particle, 2 );
	}

	void OnTriggerEnter( Collider other)
	{
		if ( (m_event == TriggerEvent.Enter || m_event == TriggerEvent.Both) && other.CompareTag("Player") )	
		{
			SpawnParticle( other );
		}
	}

	void OnTriggerExit( Collider other)
	{
		if ( (m_event == TriggerEvent.Exit || m_event == TriggerEvent.Both) && other.CompareTag("Player") )	
		{
			SpawnParticle(other);
		}
	}

	private void SpawnParticle(Collider other)
	{
		GameObject go = ParticleManager.Spawn(m_particle, other.transform.position);
		if ( go != null)
		{
			go.transform.rotation = other.transform.rotation;
		}
	}
}
