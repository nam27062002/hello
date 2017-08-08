﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//[ExecuteInEditMode]
public class CustomParticle : MonoBehaviour {

#if (!CUSTOMPARTICLES_DRAWMESH)
    public CustomParticleSystem m_pSystem;

    private MeshRenderer m_renderer;
    private MeshFilter m_filter;

    private Camera m_currentCamera;


    public float m_particleDuration;
    public Vector3 m_velocity;

    public float m_initscale;
    public float m_initRotZ;
    public float m_vRotZ;

    [HideInInspector]
    public float m_currentTime;
    public void Init()
    {
        gameObject.SetActive(true);
        m_currentTime = Time.time;
    }

    void Awake()
    {
        m_renderer = gameObject.AddComponent<MeshRenderer>();
        m_filter = gameObject.AddComponent<MeshFilter>();
    }
	// Use this for initialization
	void Start () {
        m_renderer.material = m_pSystem.m_particleMaterialInstance;
        m_filter.sharedMesh = m_pSystem.m_particleMesh;
        m_currentCamera = Camera.main;
    }

    // Update is called once per frame
    void Update () {
        float pTime = Time.time - m_currentTime;

        m_velocity += m_pSystem.m_gravity * Time.deltaTime;
        transform.position += m_velocity * Time.deltaTime;
//        transform.LookAt(m_currentCamera.transform.position, Vector3.up);


        float sv = m_pSystem.m_scaleAnimation.Evaluate(pTime / m_particleDuration);

        transform.localScale = Vector3.one * (m_initscale * sv);

        Color col = m_pSystem.m_colorAnimation.Evaluate(pTime / m_particleDuration);
//        m_renderer.material.SetColor("_VColor", col);

//        transform.rotation = m_currentCamera.transform.rotation * Quaternion.Euler(0.0f, 0.0f, m_initRotZ);
//        transform.rotation = m_currentCamera.transform.rotation * Quaternion.Euler(0.0f, 0.0f, (m_initRotZ + m_pSystem.m_rotationAnimation.Evaluate(pTime)) * 360.0f);
        transform.rotation = m_currentCamera.transform.rotation * Quaternion.Euler(0.0f, 0.0f, m_initRotZ * 360.0f);
        m_initRotZ += m_vRotZ * Time.deltaTime;

        if (pTime > m_particleDuration)
        {
            m_pSystem.m_particlesStack.Push(this);
            gameObject.SetActive(false);
        }
	}
#endif
}
