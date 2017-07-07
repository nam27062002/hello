using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomParticle : MonoBehaviour {
    public CustomParticleSystem m_pSystem;

    private MeshRenderer m_renderer;
    private MeshFilter m_filter;
    void Awake()
    {
        m_renderer = gameObject.AddComponent<MeshRenderer>();
        m_filter = gameObject.AddComponent<MeshFilter>();

        m_renderer.material = new Material(m_pSystem.m_particleMaterial);
    }
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
