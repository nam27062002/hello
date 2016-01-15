using UnityEngine;
using System.Collections;

public class RainController : MonoBehaviour 
{
	ParticleSystem m_rainParticle;



	// Use this for initialization
	void Start () 
	{
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	void OnParticleCollision(GameObject other) 
	{
		Debug.Log("COLLISION!!");
		/*
		ParticleSystem particleSystem = other.GetComponent<ParticleSystem>();
 ParticleSystem.CollisionEvent[] collisions = new ParticleSystem.CollisionEvent[particleSystem.safeCollisionEventSize];
 int numberOfCollisions = particleSystem.GetCollisionEvents(this.gameObject, collisions);
 */
	}
}
