using UnityEngine;
using System.Collections;

public class ParticleTester : MonoBehaviour {	
	public ParticleData particle = new ParticleData();

	public void SpawnParticle() {
		particle.Spawn(transform.position);
	}
}