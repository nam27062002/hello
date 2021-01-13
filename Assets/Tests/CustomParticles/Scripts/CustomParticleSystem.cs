using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//[ExecuteInEditMode]
public class CustomParticleSystem : MonoBehaviour
{

    public class Stack<T>
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
            {
                return default(T);
            }
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

        public void Clear()
        {
            idx = 0;
        }

        public int Size
        {
            get { return size; }
        }


        private T[] stack;
        private int size, idx;

    }

    [Header("Emitter")]
    public int m_MaxParticles;
    public float m_RateOverTime;
    public float m_radius;
    public float m_particleDuration;
    public float m_systemDuration;
    public bool m_local = true;
    public bool m_loop = true;
    public bool m_preWarm = false;

    [Header("Scale")]
    public Range m_scaleRange;
    public AnimationCurve m_scaleAnimation;

    [Header("Velocity")]
    public Range m_VelX;
    public Range m_VelY;
    public Range m_VelZ;

    [Header("Rot Z")]
    public Range m_rotationRange;
    public Range m_vRotationRange;
//    public AnimationCurve m_rotationAnimation;

    [Header("Color")]
    public Gradient m_colorAnimation;

    [Header("Render")]
    public Material m_particleMaterial;
    public Mesh m_particleMesh;

    [Header("Forces")]
    public Vector3 m_gravity;


    [HideInInspector]
    public Material m_particleMaterialInstance;

#if (CUSTOMPARTICLES_DRAWMESH)

    private MaterialPropertyBlock m_matProp;

    private class CustomParticleData
    {
        public Matrix4x4 mat = new Matrix4x4();

        public Vector3 m_position;
        public Vector3 m_velocity;
        public float m_initScale;
        public float m_initRotZ;
        public float m_vRotZ;
        public Color m_color;
        public float m_particleDuration;
        public float m_currentTime;
        public bool m_active;
    }

    private CustomParticleData[] m_particles;
    private Stack<CustomParticleData> m_particlesStack;
    private Stack<CustomParticleData> m_spawnedParticles;

#else
    private CustomParticle[] m_particles;
    public Stack<CustomParticle> m_particlesStack;
    private Stack<CustomParticle> m_spawnedParticles;

#endif

    private float m_invRateOverTime;
    private float m_lastParticleTime;
    private float m_startParticleTime;
    private int m_totalParticlesEmited;
    private Vector3 m_oldPosition;

    private Camera m_currentCamera;

    private bool m_playing;

    public float CurrentTime
    {
        get
        {
            return Time.time;
        }
    }

    public bool IsPlaying
    {
        get
        {
            return m_playing;
        }
    }

    void Awake()
    {
#if (CUSTOMPARTICLES_DRAWMESH)
        m_particles = new CustomParticleData[m_MaxParticles];
        m_particlesStack = new Stack<CustomParticleData>(m_MaxParticles);
        m_spawnedParticles = new Stack<CustomParticleData>(m_MaxParticles);
        m_matProp = new MaterialPropertyBlock();
#else
        m_particles = new CustomParticle[m_MaxParticles];
        m_particlesStack = new Stack<CustomParticle>(m_MaxParticles);
        m_spawnedParticles = new Stack<CustomParticle>(m_MaxParticles);
#endif

        m_particleMaterialInstance = new Material(m_particleMaterial);
        m_particleMaterialInstance.EnableKeyword(GameConstants.Materials.Keyword.CUSTOM_PARTICLE_SYSTEM);

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
    }

    // Use this for initialization
    void Start()
    {
        m_lastParticleTime = CurrentTime;
        m_totalParticlesEmited = 0;
        m_currentCamera = Camera.main;
        Play();
    }


    public void Play()
    {
        m_startParticleTime = m_lastParticleTime = CurrentTime;
        m_totalParticlesEmited = 0;
        m_oldPosition = transform.position;

        Debug.Log("CustomParticleSystem: CurrentTime " + CurrentTime);
        if (m_preWarm)
        {
            Simulate(1.0f);
        }


        m_playing = true;
    }
    public void Stop(bool stopAndClear = false)
    {
        m_playing = false;
        if (stopAndClear)
        {
            Clear();
        }
    }

    public void Clear()
    {
        m_particlesStack.Clear();
        for (int c = 0; c < m_MaxParticles; c++)
        {
#if (CUSTOMPARTICLES_DRAWMESH)
            CustomParticleData cp = m_particles[c];
            cp.m_active = false;
#else
            CustomParticle cp = m_particles[c];
			cp.gameObject.SetActive(false);
#endif
            m_particlesStack.Push(cp);
        }
    }

    public void Simulate(float dTime)
    {
        InitParticles(dTime);
        if (m_preWarm)
        {
#if (CUSTOMPARTICLES_DRAWMESH)
            CustomParticleData cp;
#else
            CustomParticle cp;
#endif
            while ((cp = m_spawnedParticles.Pop()) != null)
            {
                Debug.Log("CustomParticle: CurrentTime " + cp.m_currentTime);
                cp.m_currentTime -= cp.m_particleDuration * Random.value;
            }
            m_lastParticleTime = CurrentTime;
        }
    }

//#if (CUSTOMPARTICLES_DRAWMESH)
    public void InitParticles(float dTime)
//#endif
    {
        m_spawnedParticles.Clear();
        m_invRateOverTime = 1.0f / m_RateOverTime;
        int np = (int)(dTime / m_invRateOverTime);
        if (np > 0)
        {
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
                    cp.m_initScale = Random.Range(m_scaleRange.min, m_scaleRange.max);
                    cp.m_particleDuration = m_particleDuration;

                    cp.m_velocity.Set(Random.Range(m_VelX.min, m_VelX.max), Random.Range(m_VelY.min, m_VelY.max), Random.Range(m_VelZ.min, m_VelZ.max));
                    cp.m_initRotZ = Random.Range(m_rotationRange.min, m_rotationRange.max);
                    cp.m_vRotZ = Random.Range(m_vRotationRange.min, m_vRotationRange.max);
                    cp.m_currentTime = CurrentTime;
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
                    cp.m_particleDuration = m_particleDuration;

                    cp.m_velocity.Set(Random.Range(m_VelX.min, m_VelX.max), Random.Range(m_VelY.min, m_VelY.max), Random.Range(m_VelZ.min, m_VelZ.max));
                    cp.m_initRotZ = Random.Range(m_rotationRange.min, m_rotationRange.max);
                    cp.m_vRotZ = Random.Range(m_vRotationRange.min, m_vRotationRange.max);
                    cp.Init();
#endif
                    m_spawnedParticles.Push(cp);
                    m_totalParticlesEmited++;
                }
                m_lastParticleTime += m_invRateOverTime;
            }
        }
    }


    // Update is called once per frame
    void Update()
    {

        float dTime = CurrentTime - m_lastParticleTime;

        if (m_playing)
        {
            if (!m_loop)
            {
//                float dTime = CurrentTime - m_startParticleTime;
                if (dTime > m_systemDuration)
                {
                    m_playing = false;
                }
            }
            InitParticles(dTime);
        }

#if (CUSTOMPARTICLES_DRAWMESH)
        m_matProp.Clear();
        List<Matrix4x4> matList = new List<Matrix4x4>();
        Stack<Vector4> stCol = new Stack<Vector4>(m_MaxParticles);

        Vector3 dPosition = m_oldPosition - transform.position;

        for (int c = 0; c < m_MaxParticles; c++) 
        {
            CustomParticleData cp = m_particles[c];
            if (cp.m_active)
            {
                float pTime = CurrentTime - cp.m_currentTime;
                cp.m_velocity += m_gravity * Time.deltaTime;
                cp.m_position += cp.m_velocity * Time.deltaTime;
                if (m_local)
                {
                    cp.m_position -= dPosition;
                }
				float sv = m_scaleAnimation.Evaluate(pTime / cp.m_particleDuration);
                Color col = m_colorAnimation.Evaluate(pTime / cp.m_particleDuration);
//                Quaternion rot = m_currentCamera.transform.rotation * Quaternion.Euler(0.0f, 0.0f, (cp.m_initRotZ + m_rotationAnimation.Evaluate(pTime)) * 360.0f);
                Quaternion rot = m_currentCamera.transform.rotation * Quaternion.Euler(0.0f, 0.0f, cp.m_initRotZ * 360.0f);
                cp.m_initRotZ += cp.m_vRotZ * Time.deltaTime;

//                stCol.Push(col);

                cp.mat.SetTRS(cp.m_position, rot, Vector3.one * (sv * cp.m_initScale));
//                m_matProp.SetColor("_VColor", col);

                Graphics.DrawMesh(m_particleMesh, cp.mat, m_particleMaterialInstance, 0, null, 0, m_matProp);

                if (pTime > cp.m_particleDuration)
                {
                    cp.m_active = false;
                    m_particlesStack.Push(cp);
                }
            }
        }
/*
        if (matList.Count > 0)
        {
            m_matProp.SetVectorArray("_VColor", stCol.ToArray());
            Graphics.DrawMeshInstanced(m_particleMesh, 0, m_particleMaterial, matList, m_matProp);
        }
*/
        m_oldPosition = transform.position;
#endif

    }


    void OnDestroy()
    {
#if (!CUSTOMPARTICLES_DRAWMESH)
        for (int c = 0; c < m_MaxParticles; c++)
        {
            Destroy(m_particles[c].gameObject);
        }
#endif
    }
}
