using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InstantiateLeveledParticle : MonoBehaviour {

    public string particle;
    public bool playOnStart = true;
	// Use this for initialization
	void Start () {
        if ( !string.IsNullOrEmpty(particle) ) 
        {
            ParticleSystem particleSystem = ParticleManager.InitLeveledParticle(particle, transform);
            if ( particleSystem != null && playOnStart)
            {
                particleSystem.gameObject.SetActive(true);
                particleSystem.Play();
            }
        }
	}
}
