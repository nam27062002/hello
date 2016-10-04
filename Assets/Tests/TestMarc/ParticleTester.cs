using UnityEngine;
using System.Collections;

public class ParticleTester : MonoBehaviour {


	public string particleName = "";
	public string particlePath = "";



	public void SpawnParticle() {
		ParticleManager.Spawn(particleName, transform.position, particlePath);
	}
}