using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomParticle : MonoBehaviour {
    public CustomParticleSystem m_pSystem;

    private MeshRenderer m_renderer;
    private MeshFilter m_filter;

    public float m_duration;
    public Vector3 m_velocity;

    void Awake()
    {
        m_renderer = gameObject.AddComponent<MeshRenderer>();
        m_filter = gameObject.AddComponent<MeshFilter>();
    }
	// Use this for initialization
	void Start () {
        m_renderer.material = m_pSystem.m_particleMaterial;
        m_filter.sharedMesh = m_pSystem.m_particleMesh;
    }

    // Update is called once per frame
    void Update () {
        m_duration -= Time.deltaTime;

        if (m_duration < 0.0f)
        {
            m_pSystem.Stack = this;
            gameObject.SetActive(false);
        }

        m_velocity += m_pSystem.m_gravity * Time.deltaTime;
        transform.position += m_velocity * Time.deltaTime;

	}
}
