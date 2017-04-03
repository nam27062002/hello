
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FireBreathDynamic : MonoBehaviour 
{
    [System.Serializable]
    public struct CollisionPrefab
    {
    	public enum Type
    	{
    		NORMAL,
    		INSIDE_WATER,
    		WATER_SURFACE
    			
    	}
        [SerializeField]
        public string[] m_CollisionLayers;
        [SerializeField]
        public string m_CollisionPrefab;
        public float m_CollisionDelay;
        
        [HideInInspector, SerializeField]
        public int m_iCollisionLayerMask;
        [HideInInspector, SerializeField]
        public string m_CollisionPrefabPath;

		[SerializeField]
		public Type m_type;
    }


    // consts
    public static readonly float CURRENT_DRAGONBABY_REFERENCEVALUE = 2.1985f;
    public static readonly float CURRENT_DRAGONCLASSIC_REFERENCEVALUE = 5.535f;

    // Mesh cache
    private int[] m_triangles = null;
	private Vector3[] m_pos = null;
	private Vector2[] m_UV = null;
    private Color[] m_color = null;

    // Meshes
    private Mesh m_mesh = null;

	// Cached components
	private MeshFilter m_meshFilter = null;

    public float m_distance = 1;
    public float m_aplitude = 6;
    private float m_splits = 5;

    private int m_numPos = 0;

    public float m_fireFlexFactor = 1.0f;

    private Vector3[] m_whip;
    private Vector3[] m_realWhip;
    private Vector3[] m_whipTangent;

    private int m_collisionSplit = 0;

    public Color m_initialColor;
    public Color m_flameColor;
    public Color m_collisionColor;

    public AnimationCurve m_shapeCurve;
    public AnimationCurve m_FlameAnimation;

    public CollisionPrefab[] m_collisionPrefabs;

    private float m_collisionMaxDistance = 0.0f;
    private float m_collisionDistance = 0.0f;

    private int m_AllLayerMask;
    private int m_WaterLayerMask;

    private GameObject m_whipEnd;
    private GameObject m_collisionPlane;

    private float m_effectScale = 1.0f;
    public float m_debugScale = -1.0f;

    private float m_lastTime;


    private bool enableState = false;

	private float m_showFlameTimer = 0.0f;
    private bool m_showFlame = false;

    public List<ParticleSystem> m_fireParticles;
	public List<int> m_fireParticlesMaxParticles;

	public List<ParticleSystem> m_underWaterParticles;
	public List<int> m_underWaterParticlesMaxParticles;

	private bool m_insideWater = false;
	private float m_waterHeigth = 0;

    private ParticleSystem[] m_particleList;

    void OnEnable()
    {
        Messenger.AddListener<bool>(GameEvents.BOOST_TOGGLED, OnBoostToggled);
    }
    void OnDisable()
    {
        Messenger.RemoveListener<bool>(GameEvents.BOOST_TOGGLED, OnBoostToggled);
    }

    void OnBoostToggled(bool value)
    {
        ParticleSystem.InheritVelocityModule iVelocityModule;
        foreach (ParticleSystem psystem in m_particleList)
        {
            iVelocityModule = psystem.inheritVelocity;
//            iVelocityModule.curveMultiplier = value ? 2.0f : 0.7f;
            iVelocityModule.curveMultiplier = value ? 0.975f : 0.975f;
        }
        // Messenger.AddListener<bool>(GameEvents.BOOST_TOGGLED, OnBoostToggled);
    }


    public void setEffectScale(float furyBaseLength, float dragonScale)
    {
        m_collisionMaxDistance = (furyBaseLength * dragonScale);

        if (m_debugScale > 0.0f)
        {
            m_effectScale = m_debugScale;
        }
        else
        {
            m_effectScale = 1.0f + ((m_collisionMaxDistance - CURRENT_DRAGONBABY_REFERENCEVALUE) / (CURRENT_DRAGONCLASSIC_REFERENCEVALUE - CURRENT_DRAGONBABY_REFERENCEVALUE));
        }

        m_collisionMaxDistance *= 2.0f;

        for (int i = 0; i < m_fireParticles.Count; i++)
            m_fireParticles[i].transform.SetLocalScale(m_effectScale);

        for (int i = 0; i < m_underWaterParticles.Count; i++)
            m_underWaterParticles[i].transform.SetLocalScale(m_effectScale);

    }

    // Use this for initialization
    void Start () 
	{
		m_WaterLayerMask = 1 << LayerMask.NameToLayer("Water");
        m_AllLayerMask = 0;
        for (int i = 0; i < m_collisionPrefabs.Length; i++)
        {
            int layerMask = 0;
            for (int c = 0; c < m_collisionPrefabs[i].m_CollisionLayers.Length; c++)
            {
                layerMask |= LayerMask.GetMask(m_collisionPrefabs[i].m_CollisionLayers[c]);
            }
            m_collisionPrefabs[i].m_iCollisionLayerMask = layerMask;
            m_AllLayerMask |= layerMask;

            string cp = m_collisionPrefabs[i].m_CollisionPrefab;
            int backslash = cp.LastIndexOf('/');
            if (backslash >= 0)
            {
                m_collisionPrefabs[i].m_CollisionPrefabPath = cp.Substring(0, backslash);
                m_collisionPrefabs[i].m_CollisionPrefab = cp.Substring(backslash + 1);
            }
            else
            {
                m_collisionPrefabs[i].m_CollisionPrefabPath = "";
            }

            ParticleManager.CreatePool(m_collisionPrefabs[i].m_CollisionPrefab,
                                       m_collisionPrefabs[i].m_CollisionPrefabPath);

        }

        // Cache
        m_meshFilter = GetComponent<MeshFilter>();
        m_numPos = (int)(4 + m_splits * 2);

        m_whipEnd = transform.FindChild("WhipEnd").gameObject;
        m_collisionPlane = transform.FindChild("WhipEnd/collisionPlane").gameObject;


/*
        for( int i = 0; i<m_fireParticles.Count; i++ )
			m_fireParticles[i].transform.SetLocalScale(m_effectScale);

		for( int i = 0; i<m_underWaterParticles.Count; i++ )
			m_underWaterParticles[i].transform.SetLocalScale(m_effectScale);
*/
        InitWhip();
		InitArrays();
//		InitUVs();
		InitTriangles();

		ReshapeFromWhip();

        CreateMesh();


        m_particleList = GetComponentsInChildren<ParticleSystem>();

    }

    void InitWhip()
	{
		m_whip = new Vector3[(int)m_splits + 1];
        m_realWhip = new Vector3[(int)m_splits + 1];
        m_whipTangent = new Vector3[(int)m_splits + 1];

        float xStep = (m_distance * m_effectScale) / (m_splits + 1);
		Vector3 move = transform.right;
		Vector3 pos = transform.position;
		for( int i = 0; i < (m_splits + 1); i++ )
		{
			m_realWhip[i] = m_whip[i] = pos + (move * xStep * i);
            m_whipTangent[i] = transform.up;

        }
	}

	void InitArrays()
	{	
		m_pos = new Vector3[m_numPos];
		m_UV = new Vector2[m_numPos];
        m_color = new Color[m_numPos];
        for ( int i = 0; i < m_numPos; i++ )
		{
			m_pos[i] = Vector3.zero;
			m_UV[i] = Vector2.zero;
            m_color[i] = (i < 6) ? m_initialColor : m_flameColor;
		}
	}

    void InitTriangles()
	{
		int numTrianglesIndex = (m_numPos-2) * 3;
		m_triangles = new int[ numTrianglesIndex ];

		int pos = 0;
		for( int i = 0; i<numTrianglesIndex; i += 3 )
		{
			m_triangles[i] = pos;
			if ( pos % 2 == 0 )
			{
				m_triangles[i+1] = pos+2;
				m_triangles[i+2] = pos+1;

			}
			else
			{
				m_triangles[i+1] = pos+1;
				m_triangles[i+2] = pos+2;
			}
			pos++;
		}
	}

    void ReshapeFromWhip()
	{
		m_pos[0] = m_pos[1] = Vector3.zero;

        m_UV[0] = Vector2.right * 0.5f;
        m_UV[1] = Vector2.right * 0.5f;


        float vStep = 1.0f / (m_splits + 1.0f);

        int step = 1;
		int whipIndex = 0;
        Vector3 newPos1, newPos2;

        float efs = Mathf.Clamp(m_effectScale, 0.0f, 3.75f);
        for ( int i = 2; i < m_numPos; i += 2 )
		{
			float yDisplacement = m_shapeCurve.Evaluate(step / (float)(m_splits+2)) * m_aplitude * efs *0.25f;

            Vector3 whipTangent = transform.InverseTransformDirection(m_whipTangent[whipIndex]);

            newPos1 = newPos2 = transform.InverseTransformPoint(m_realWhip[whipIndex]);

            if (transform.right.x < 0.0f)
            {
                newPos1 += whipTangent * yDisplacement;
                newPos2 -= whipTangent * yDisplacement;
            }
            else
            {
                newPos1 += whipTangent * yDisplacement;
                newPos2 -= whipTangent * yDisplacement;
            }

            m_pos[i] = newPos1;
            m_pos[i + 1] = newPos2;

            yDisplacement *= 0.5f;

            m_UV[i].Set(0.5f + yDisplacement, vStep * step);
            m_UV[i + 1].Set(0.5f - yDisplacement, vStep * step);

            if (i > 4)
            {
                m_color[i] = m_color[i + 1] = (whipIndex > m_collisionSplit) ? m_collisionColor : m_flameColor;
            }
/*            else
            {
                m_color[i] = m_color[i + 1] = m_initialColor;
            }
*/
            step++;
			whipIndex++;
		}

    }

    void InitUVs()
    {
        //		m_UV[0] = Vector2.right * 0.5f;
        //		m_UV[1] = Vector2.right * 0.5f;
        float vStep = 1.0f / (m_splits + 1);
        // float hStep = 1.0f / (m_splits + 1);

        int step = 0;
        for (int i = 0; i < m_numPos; i += 2)
        {
            float xDisplacement = m_shapeCurve.Evaluate(step / (float)(m_splits)) * 0.5f;

            //            m_UV[i].x = 0.75f;// - ((hStep/2.0f) * step);
            //			m_UV[i].x = 0.5f + xDisplacement;
            //float xDisplacement = 0.0f;// (((i >> 1) & 0) != 0) ? -0.1f : 0.1f;
            m_UV[i].x = 0.5f + xDisplacement;
            m_UV[i].y = vStep * step;

            //            m_UV[i + 1].x = 0.25f;// + ((hStep/2.0f) * step);
            //			m_UV[i+1].x = 0.5f - xDisplacement;
            m_UV[i + 1].x = 0.5f - xDisplacement;
            m_UV[i + 1].y = vStep * step;

            step++;
        }
    }

    // Recreates the mesh
    void CreateMesh()
	{
		m_mesh = new Mesh();
		m_mesh.MarkDynamic();

		m_mesh.vertices = m_pos;
        m_mesh.uv = m_UV;
        m_mesh.colors = m_color;    

        m_mesh.SetTriangles( m_triangles, 0);
		// m_mesh.SetIndices(m_triangles, MeshTopology.Triangles, 0);
        m_meshFilter.sharedMesh = m_mesh;

	}

	
	// Update is called once per frame
	void FixedUpdate () 
	{
        UpdateWhip();
		ReshapeFromWhip();

		m_mesh.uv = m_UV;
		m_mesh.vertices = m_pos;
        m_mesh.colors = m_color;

        Vector3 particlePos = m_whipEnd.transform.localPosition;
        float particleDistance = m_distance * (m_effectScale*0.38f);       
        particlePos.x = m_collisionDistance < particleDistance ? m_collisionDistance : particleDistance;
        m_whipEnd.transform.localPosition = particlePos;

    }

    void UpdateWhip()
    {
        RaycastHit hit;
        Vector3 whipDirection = transform.right;
        whipDirection.z = 0.0f;
        whipDirection.Normalize();

        Vector3 whipOrigin = transform.position;

        if ( m_showFlame )
        {
        	m_showFlameTimer += Time.deltaTime;	// Scale flame up
        }
        else
        {
        	m_showFlameTimer -= Time.deltaTime * 10;	// Scale Flame down
        }
        m_showFlameTimer = Mathf.Clamp01( m_showFlameTimer );

        float flameAnim = m_FlameAnimation.Evaluate( m_showFlameTimer );
        if (!enableState && m_showFlameTimer <= 0)
        {
        	if (!HasParticleAlive())
            	gameObject.active = false;
        }
		float xStep = (flameAnim * m_distance * m_effectScale) / (m_splits + 1);
        m_collisionSplit = (int)m_splits - 1;
		m_collisionDistance = m_collisionMaxDistance;


        bool hitsSomething = false;	// if fire colliding with something
        Vector3 hitPoint = Vector3.zero;	// Closest hit point
        Transform collisionPlaneTransform = null;

		if (Physics.Raycast(transform.position, transform.right, out hit, m_collisionDistance, m_AllLayerMask))
        {
        	hitsSomething = true;
            int hitLayer = 1 << hit.transform.gameObject.layer;

            foreach (CollisionPrefab cp in m_collisionPrefabs)
            {
                if ((cp.m_iCollisionLayerMask & hitLayer) != 0 )
                {
					bool spawn = false;
					switch( cp.m_type )
					{
						case CollisionPrefab.Type.NORMAL:
						{
							spawn = !WaterAreaManager.instance.IsInsideWater( hit.point );
						}break;

						case CollisionPrefab.Type.INSIDE_WATER:
						{
							spawn = WaterAreaManager.instance.IsInsideWater( hit.point );
						}break;

						default:
						{
						}break;
					}
                    if (spawn && Time.time > m_lastTime + cp.m_CollisionDelay)
                    {
                        GameObject colFire = ParticleManager.Spawn(cp.m_CollisionPrefab, hit.point, cp.m_CollisionPrefabPath);
                        if (colFire != null)
                        {
                            colFire.transform.rotation = Quaternion.LookRotation(-Vector3.forward, hit.normal);
                        }

                        m_lastTime = Time.time;
                    }
                }
            }

            m_collisionPlane.transform.position = hit.point;
            m_collisionPlane.transform.up = hit.normal;

			collisionPlaneTransform = m_collisionPlane.transform;
            m_collisionDistance = hit.distance;
            hitPoint = hit.point;
        }



        if ( m_insideWater )
        {
        	Vector3 endPos = transform.position + transform.right * m_collisionDistance;
			if (Physics.Raycast(endPos, -transform.right, out hit, m_collisionDistance, m_WaterLayerMask))
			{
				hitsSomething = true;
				int hitLayer = 1 << hit.transform.gameObject.layer;
				foreach (CollisionPrefab cp in m_collisionPrefabs)
	            {
					if ((cp.m_iCollisionLayerMask & hitLayer) != 0 && cp.m_type == CollisionPrefab.Type.WATER_SURFACE)
	                {
						bool spawn = false;
	                    if (spawn && Time.time > m_lastTime + cp.m_CollisionDelay)
	                    {
	                        GameObject colFire = ParticleManager.Spawn(cp.m_CollisionPrefab, hit.point, cp.m_CollisionPrefabPath);
	                        if (colFire != null)
	                        {
	                            colFire.transform.rotation = Quaternion.LookRotation(-Vector3.forward, hit.normal);
	                        }

	                        m_lastTime = Time.time;
	                    }
	                    break;
	                }
	            }

				m_collisionPlane.transform.position = hit.point;
	            m_collisionPlane.transform.up = -hit.normal;

				collisionPlaneTransform = m_collisionPlane.transform;
				m_collisionDistance = m_collisionDistance - hit.distance;
	            hitPoint = hit.point;
			}
        }


        if ( hitsSomething )
        {
			SetParticleCollisionsPlane(collisionPlaneTransform);
			for (int i = 0; i < m_splits + 1; i++)
            {
                float currentDist = (xStep * (i + 1));

				if (currentDist < m_collisionDistance)
                {
                    m_whip[i] = whipOrigin + (whipDirection * currentDist);
                }
				else if (currentDist < (m_collisionDistance + xStep))
                {
                    m_whip[i] = hitPoint;
                    m_collisionSplit = i;
                }
                else
                {
                    m_whip[i] = hitPoint;
                }
            }
        }
        else
        {
			SetParticleCollisionsPlane(null);

            for (int i = 0; i < m_splits + 1; i++)
            {
                float currentDist = (xStep * (i + 1));
                m_whip[i] = whipOrigin + (whipDirection * currentDist);
            }
        }





        for (int i = 0; i < m_splits + 1; i++)
        {
            // Vector3 distance = m_whip[i] - m_realWhip[i];
            float fq = Mathf.Pow(1.0f - (i / (m_splits + 1)), m_fireFlexFactor);
            float rq = Mathf.Clamp(fq + ((1.0f / m_fireFlexFactor) * Time.fixedDeltaTime), 0.0f, 1.0f);

            m_realWhip[i] = Vector3.Lerp(m_realWhip[i], m_whip[i], rq);

            if (i < 1)
            {
                if (transform.right.x < 0.0f)
                {
                    m_whipTangent[i] = transform.up;
                }
                else
                {
                    m_whipTangent[i] = -transform.up;
                }
            }
            else
            {
                whipDirection = m_realWhip[i] - m_realWhip[i - 1];
                m_whipTangent[i] = Vector3.Normalize(Vector3.Cross(Vector3.forward, whipDirection));//transform.up;
            }
        }
    }


    void SetParticleCollisionsPlane( Transform _tr )
    {
		for( int i = 0; i<m_fireParticles.Count; i++ )
			m_fireParticles[i].collision.SetPlane(0, _tr);

		for( int i = 0; i<m_underWaterParticles.Count; i++ )
			m_underWaterParticles[i].collision.SetPlane(0, _tr);
    }

    bool HasParticleAlive()
    {
		for( int i = 0; i<m_fireParticles.Count; i++ )
			if (m_fireParticles[i].IsAlive())
				return true;

		for( int i = 0; i<m_underWaterParticles.Count; i++ )
			if (m_underWaterParticles[i].IsAlive())
				return true;
		return false;
    }

    public void EnableFlame(bool value, bool insideWater = false)
    {
    	if ( value )
    	{
			gameObject.active = true;
			m_showFlame = !insideWater;
    		// Check if inside water!
			for( int i = 0; i<m_fireParticles.Count; i++ )
			{
				if (!insideWater)
					m_fireParticles[i].Play();
			}

			for( int i = 0; i<m_underWaterParticles.Count; i++ )
				if (insideWater)
					m_underWaterParticles[i].Play();
    	}
    	else
    	{
			for( int i = 0; i<m_fireParticles.Count; i++ )
				m_fireParticles[i].Stop();

			for( int i = 0; i<m_underWaterParticles.Count; i++ )
				m_underWaterParticles[i].Stop();

			m_showFlame = false;
    	}

        enableState = value;
    }

	public void SwitchToWaterMode()
    {
		m_insideWater = true;
		m_showFlame = false;
		m_waterHeigth = transform.position.y;
		for( int i = 0; i<m_fireParticles.Count; i++ )
			m_fireParticles[i].Stop();

		for( int i = 0; i<m_underWaterParticles.Count; i++ )
			m_underWaterParticles[i].Play();
    }

    public void SwitchToNormalMode()
    {
		m_insideWater = false;
		m_showFlame = true;
		for( int i = 0; i<m_fireParticles.Count; i++ )
			m_fireParticles[i].Play();

		for( int i = 0; i<m_underWaterParticles.Count; i++ )
			m_underWaterParticles[i].Stop();
    }
}
