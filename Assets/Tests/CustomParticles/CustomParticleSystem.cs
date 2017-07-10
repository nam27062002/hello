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
                return m_particlesStack[--m_stackIndex];
            else
                return null;
        }

        set
        {
            if (m_stackIndex < m_MaxParticles)
            {
                m_particlesStack[m_stackIndex++] = value;
            }
        }
    }


    //    public 
    private CustomParticle[] m_particles;
    private CustomParticle[] m_particlesStack;
    private int m_stackIndex;

    private float m_invRateOverTime;
    private float m_nextParticleTime;


    void Awake()
    {
        m_particles = new CustomParticle[m_MaxParticles];
        m_particlesStack = new CustomParticle[m_MaxParticles];

        for (int c = 0; c < m_MaxParticles; c++)
        {
            GameObject go = new GameObject("custom_particle");
            CustomParticle cp = go.AddComponent<CustomParticle>();
            m_particlesStack[c] = m_particles[c] = cp;
            go.SetActive(false);
        }

        m_stackIndex = 0;
    }

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        if (Time.time > m_nextParticleTime)
        {
            CustomParticle cp = Stack;
            cp.transform.position = transform.position + Random.insideUnitSphere * m_radius;
            if (m_local)
            {
                cp.transform.parent = transform;
            }
            else
            {
                cp.transform.parent = null;
            }
        }
	}
}
