using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomParticleSystem : MonoBehaviour {


    [Header("Emitter")]
    public int m_MaxParticles;
    public float m_RateOverTime;
    public float m_radius;
    public float m_duration;
    public bool m_local;

    [Header("Scale")]
    public Range m_scaleRange;
    public AnimationCurve m_scaleAnimation;

    [Header("Velocity")]
    public Range m_VelX;
    public Range m_VelY;
    public Range m_VelZ;

    [Header("Rot Z")]
    public Range m_rotationRange;
    public AnimationCurve m_rotationAnimation;

    [Header("Color")]
    public Gradient m_colorAnimation;

    [Header("Render")]
    public Material m_particleMaterial;
    public Mesh m_particleMesh;

    [Header("Gravity")]
    public Vector3 m_gravity;

    public CustomParticle Stack
    {
        get
        {
            if (m_stackIndex > 0)
            {
                CustomParticle cp = m_particlesStack[--m_stackIndex];
                cp.gameObject.SetActive(true);
                return cp;
            }
            else
                return null;
        }

        set
        {
            if (m_stackIndex < m_MaxParticles)
            {
                value.gameObject.SetActive(false);
                m_particlesStack[m_stackIndex++] = value;
            }
        }
    }


    //    public 
    private CustomParticle[] m_particles;
    private CustomParticle[] m_particlesStack;
    private int m_stackIndex;

    private float m_invRateOverTime;
    private float m_lastParticleTime;

    private Camera m_currentCamera;


    void Awake()
    {
        m_particles = new CustomParticle[m_MaxParticles];
        m_particlesStack = new CustomParticle[m_MaxParticles];

        for (int c = 0; c < m_MaxParticles; c++)
        {
            GameObject go = new GameObject("custom_particle");
            CustomParticle cp = go.AddComponent<CustomParticle>();
            cp.m_pSystem = this;
            m_particlesStack[c] = m_particles[c] = cp;
            go.SetActive(false);
        }
        m_stackIndex = m_MaxParticles;
    }

	// Use this for initialization
	void Start () {
        m_lastParticleTime = Time.time;
        m_currentCamera = Camera.main;
    }
	
	// Update is called once per frame
	void Update () {
        m_invRateOverTime = 1.0f / m_RateOverTime;
        int np = (int)((Time.time - m_lastParticleTime) / m_invRateOverTime);
        if (np > 0)
        {
            for (int c = 0; c < np; c++)
            {
                CustomParticle cp = Stack;
                if (cp != null)
                {
                    cp.transform.position = transform.position + Random.insideUnitSphere * m_radius;
                    float sc = Random.Range(m_scaleRange.min, m_scaleRange.max);
                    cp.m_initscale = sc;
                    if (m_local)
                    {
                        cp.transform.parent = transform;
                    }
                    else
                    {
                        cp.transform.parent = null;
                    }
                    cp.m_duration = m_duration;

                    cp.m_velocity.Set(Random.Range(m_VelX.min, m_VelX.max), Random.Range(m_VelY.min, m_VelY.max), Random.Range(m_VelZ.min, m_VelZ.max));
                    cp.m_initRot = Random.Range(m_rotationRange.min, m_rotationRange.max);
                    cp.Init();
//                    cp.gameObject.SetActive(true);
                }
            }
            m_lastParticleTime += (float)np * m_invRateOverTime;
        }
    }
}
