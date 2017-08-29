using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireReflection : MonoBehaviour {


    private bool m_reflectionEnabled;

    private Vector3 m_velocity;

    private ParticleScaler m_particleScaler;

    public float m_scaleProgression = 1.1f;

    void Awake()
    {
        m_particleScaler = GetComponent<ParticleScaler>();
    }

    public void addVelocity(Vector3 vel)
    {
        m_velocity = vel;
        m_reflectionEnabled = true;
        m_particleScaler.m_scale = 1.0f;
    }


	// Use this for initialization
	void Start () {
        m_reflectionEnabled = false;
	}
	
	// Update is called once per frame
	void Update () {
		if (m_reflectionEnabled)
        {
            Vector3 position = transform.position;
            position += m_velocity * Time.deltaTime;
            transform.position = position;

            m_particleScaler.m_scaleOrigin = ParticleScaler.ScaleOrigin.ATTRIBUTE_SCALE;
            m_particleScaler.m_scale *= m_scaleProgression;
        }
	}
}
