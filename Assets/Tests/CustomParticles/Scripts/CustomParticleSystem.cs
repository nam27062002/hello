using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class CustomParticleSystem : MonoBehaviour {

//    public class Stack<T> where T : Object
    private class Stack<T>// where T : UnityEngine.Object
    {
        public Stack(int _size)
        {
            size = _size;
            stack = new T[size];
            idx = 0;
        }

        public T Pop()
        {
            if (idx > 0)
            {
                return stack[--idx];
            }
            else
                return default(T);// null;
        }

        public void Push(T elem)
        {
            if (idx < size)
            {
                stack[idx++] = elem;
            }
        }

        public T[] ToArray()
        {
            return stack;
        }


        private T[] stack;
        private int size, idx;

    }



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

#if (CUSTOMPARTICLES_DRAWMESH)

    private MaterialPropertyBlock m_matProp;

    private class CustomParticleData
    {
        public Matrix4x4 mat = new Matrix4x4();

        public Vector3 m_position;
        public Vector3 m_velocity;
        public float m_initScale;
        public float m_rotZ;
        public Color m_color;
        public float m_duration;
        public float m_currentTime;
        public bool m_active;
    }

    private CustomParticleData[] m_particles;
    private Stack<CustomParticleData> m_particlesStack;
    /*
        private CustomParticleData Stack
        {
            get
            {
                if (m_stackIndex > 0)
                {
                    CustomParticleData cp = m_particlesStack[--m_stackIndex];
                    cp.m_active = true;
                    return cp;
                }
                else
                    return null;
            }

            set
            {
                if (m_stackIndex < m_MaxParticles)
                {
                    value.m_active = false;
                    m_particlesStack[m_stackIndex++] = value;
                }
            }
        }
    */

#else
    private CustomParticle[] m_particles;
    private Stack<CustomParticle> m_particlesStack;

/*
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
*/

#endif

    private int m_stackIndex;

    private float m_invRateOverTime;
    private float m_lastParticleTime;

    private Camera m_currentCamera;


    void Awake()
    {
#if (CUSTOMPARTICLES_DRAWMESH)
        m_particles = new CustomParticleData[m_MaxParticles];
        m_particlesStack = new Stack<CustomParticleData>(m_MaxParticles);
        m_matProp = new MaterialPropertyBlock();
#else
        m_particles = new CustomParticle[m_MaxParticles];
        m_particlesStack = new Stack<CustomParticle>(m_MaxParticles);
#endif

        for (int c = 0; c < m_MaxParticles; c++)
        {
#if (CUSTOMPARTICLES_DRAWMESH)
            CustomParticleData cp = new CustomParticleData();
            cp.m_active = false;
#else
            GameObject go = new GameObject("custom_particle");
            CustomParticle cp = go.AddComponent<CustomParticle>();
            cp.m_pSystem = this;
            go.SetActive(false);
#endif
            m_particles[c] = cp;
            m_particlesStack.Push(cp);
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
            int initialized = 0;
            for (int c = 0; c < np; c++)
            {
#if (CUSTOMPARTICLES_DRAWMESH)
                CustomParticleData cp = m_particlesStack.Pop();
#else
                CustomParticle cp = m_particlesStack.Pop();
#endif
                if (cp != null)
                {

#if (CUSTOMPARTICLES_DRAWMESH)
                    cp.m_position = transform.position + Random.insideUnitSphere * m_radius;
                    cp.m_initScale = Random.RandomRange(m_scaleRange.min, m_scaleRange.max);
                    cp.m_duration = m_duration;

                    cp.m_velocity.Set(Random.Range(m_VelX.min, m_VelX.max), Random.Range(m_VelY.min, m_VelY.max), Random.Range(m_VelZ.min, m_VelZ.max));
                    cp.m_rotZ = Random.Range(m_rotationRange.min, m_rotationRange.max);
                    cp.m_currentTime = Time.time;
                    cp.m_active = true;

#else
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
#endif
                }
                m_lastParticleTime += m_invRateOverTime;
            }
        }

#if (CUSTOMPARTICLES_DRAWMESH)
        m_matProp.Clear();
        List<Matrix4x4> matList = new List<Matrix4x4>();
        Stack<Vector4> stCol = new Stack<Vector4>(m_MaxParticles);

        for (int c = 0; c < m_MaxParticles; c++)
        {
            CustomParticleData cp = m_particles[c];
            if (cp.m_active)
            {

                float pTime = Time.time - cp.m_currentTime;
                cp.m_velocity += m_gravity * Time.deltaTime;
                cp.m_position += cp.m_velocity * Time.deltaTime;
                float sv = m_scaleAnimation.Evaluate(pTime);
                Color col = m_colorAnimation.Evaluate(pTime);
                Quaternion rot = m_currentCamera.transform.rotation * Quaternion.Euler(0.0f, 0.0f, (cp.m_rotZ + m_rotationAnimation.Evaluate(pTime)) * 360.0f);


                stCol.Push(col);
//                m_matProp.SetColor("_Color", col);

                cp.mat.SetTRS(cp.m_position, rot, Vector3.one * (sv + cp.m_initScale));
                matList.Add(cp.mat);

                if (pTime > cp.m_duration)
                {
                    cp.m_active = false;
                    m_particlesStack.Push(cp);
                }
            }
        }

        if (matList.Count > 0)
        {
            //            Graphics.DrawMeshInstanced(m_particleMesh, 0, m_particleMaterial, matList, matList.Count);
            m_matProp.SetVectorArray("_Color", stCol.ToArray());
            Graphics.DrawMeshInstanced(m_particleMesh, 0, m_particleMaterial, matList, m_matProp);
        }


#endif


    }
}
