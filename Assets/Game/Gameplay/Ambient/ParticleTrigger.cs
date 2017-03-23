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
	private Collider m_collider;

	void Start()
	{
		ParticleManager.CreatePool(m_particle);
		m_collider = GetComponent<Collider>();
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
		Vector3 position = m_collider.ClosestPointOnBounds( other.transform.position);
		GameObject go = ParticleManager.Spawn(m_particle, position);
		if (go != null) {
			go.transform.rotation = other.transform.rotation;
		}
	}
}
