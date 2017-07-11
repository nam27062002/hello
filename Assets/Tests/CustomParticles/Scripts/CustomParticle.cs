using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomParticle : MonoBehaviour {

#if (!CUSTOMPARTICLES_DRAWMESH)
    public CustomParticleSystem m_pSystem;

    private MeshRenderer m_renderer;
    private MeshFilter m_filter;

    private Camera m_currentCamera;


    public float m_duration;
    public Vector3 m_velocity;

    public float m_initscale;
    public float m_initRot;

    private float m_currentTime;
    public void Init()
    {
        m_currentTime = Time.time;
    }

    void Awake()
    {
        m_renderer = gameObject.AddComponent<MeshRenderer>();
        m_filter = gameObject.AddComponent<MeshFilter>();
    }
	// Use this for initialization
	void Start () {
        m_renderer.material = m_pSystem.m_particleMaterial;
        m_filter.sharedMesh = m_pSystem.m_particleMesh;
        m_currentCamera = Camera.main;
    }

    // Update is called once per frame
    void Update () {
        float pTime = Time.time - m_currentTime;

        m_velocity += m_pSystem.m_gravity * Time.deltaTime;
        transform.position += m_velocity * Time.deltaTime;
//        transform.LookAt(m_currentCamera.transform.position, Vector3.up);


        float sv = m_pSystem.m_scaleAnimation.Evaluate(pTime);

        transform.localScale = Vector3.one * (m_initscale + sv);

        Color col = m_pSystem.m_colorAnimation.Evaluate(pTime);
        m_renderer.material.SetColor("_Color", col);

//        transform.rotation = m_currentCamera.transform.rotation * Quaternion.Euler(0.0f, 0.0f, m_initRot);
        transform.rotation = m_currentCamera.transform.rotation * Quaternion.Euler(0.0f, 0.0f, (m_initRot + m_pSystem.m_rotationAnimation.Evaluate(pTime)) * 360.0f);

        if (pTime > m_duration)
        {
            m_pSystem.Stack = this;
//            gameObject.SetActive(false);
        }
	}
#endif
}
