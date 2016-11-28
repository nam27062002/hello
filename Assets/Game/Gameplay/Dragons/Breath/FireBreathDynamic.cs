
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FireBreathDynamic : MonoBehaviour 
{
    [System.Serializable]
    public struct CollisionPrefab
    {
        [SerializeField]
        public string[] m_CollisionLayers;
        [SerializeField]
        public string m_CollisionPrefab;
        public float m_CollisionDelay;
        public int m_CollisionEmiters;

        [HideInInspector, SerializeField]
        public int m_iCollisionLayerMask;
        [HideInInspector, SerializeField]
        public string m_CollisionPrefabPath;
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
//    bool[] m_whipCollision;

    private int m_collisionSplit = 0;

    public Color m_initialColor;
    public Color m_flameColor;
    public Color m_collisionColor;

    public AnimationCurve m_shapeCurve;
    public AnimationCurve m_FlameAnimation;
    public AnimationCurve m_FlexCurve;

    public CollisionPrefab[] m_collisionPrefabs;

    private float m_collisionMaxDistance = 0.0f;
    private float m_collisionDistance = 0.0f;
    // private float m_particleDistance = 0.0f;

    private float flameAnimationTime = 0.0f;

    private int m_AllLayerMask;


    // private Vector3 lastInitialPosition;
    private GameObject m_whipEnd;
    private GameObject m_collisionPlane;

    private float m_effectScale = 1.0f;
    public float m_debugScale = -1.0f;

    private float m_lastTime;

    private float enableTime = 0.0f;
    private bool enableState = false;

    public List<ParticleSystem> m_fireParticles;
	public List<ParticleSystem> m_underWaterParticles;

	private bool m_insideWater = false;
	private float m_waterHeigth = 0;

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
    }

    // Use this for initialization
    void Start () 
	{
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
                                       m_collisionPrefabs[i].m_CollisionPrefabPath,
                                       m_collisionPrefabs[i].m_CollisionEmiters);

        }

        // Cache
        m_meshFilter = GetComponent<MeshFilter>();
        m_numPos = (int)(4 + m_splits * 2);

        m_whipEnd = transform.FindChild("WhipEnd").gameObject;
        m_collisionPlane = transform.FindChild("WhipEnd/collisionPlane").gameObject;

        for( int i = 0; i<m_fireParticles.Count; i++ )
			m_fireParticles[i].transform.SetLocalScale(m_effectScale);

		for( int i = 0; i<m_underWaterParticles.Count; i++ )
			m_underWaterParticles[i].transform.SetLocalScale(m_effectScale);

        InitWhip();
		InitArrays();
//		InitUVs();
		InitTriangles();

		ReshapeFromWhip();

        CreateMesh();

        // lastInitialPosition = m_whipEnd.transform.position;

        flameAnimationTime = m_FlameAnimation[m_FlameAnimation.length - 1].time;
        enableTime = m_lastTime = Time.time;

    }

    void InitWhip()
	{
		m_whip = new Vector3[(int)m_splits + 1];
        m_realWhip = new Vector3[(int)m_splits + 1];
        m_whipTangent = new Vector3[(int)m_splits + 1];
//        m_whipCollision = new bool[(int)m_splits + 1];

        float xStep = (m_distance * m_effectScale) / (m_splits + 1);
		Vector3 move = transform.right;
		Vector3 pos = transform.position;
		for( int i = 0; i < (m_splits + 1); i++ )
		{
			m_realWhip[i] = m_whip[i] = pos + (move * xStep * i);
            m_whipTangent[i] = transform.up;
//            m_whipCollision[i] = false;

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

        for ( int i = 2; i < m_numPos; i += 2 )
		{
			float yDisplacement = m_shapeCurve.Evaluate(step / (float)(m_splits+2)) * m_aplitude * m_effectScale*0.25f;

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

//        Debug.Log("lastInitialPositionVariation: " + (lastInitialPosition - transform.position).ToString());

        // lastInitialPosition = m_whipEnd.transform.position;
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
//        MoveWhip();
        UpdateWhip();
		ReshapeFromWhip();
//		InitUVs();

		m_mesh.uv = m_UV;
		m_mesh.vertices = m_pos;
        m_mesh.colors = m_color;

        Vector3 particlePos = m_whipEnd.transform.localPosition;
        float particleDistance = m_distance * (m_effectScale*0.5f);       
        //Mathf.Pow(effectScale, 1.5f);
        particlePos.x = m_collisionDistance < particleDistance ? m_collisionDistance : particleDistance;
        //        whipEnd.transform.SetLocalPosX(m_distance * effectScale);
        m_whipEnd.transform.localPosition = particlePos;
//        whipEnd.transform.SetLocalScale(whipEnd.transform.GetGlobalScaleQuick() * m_effectScale);

    }

    void UpdateWhip()
    {
        RaycastHit hit;
        Vector3 whipDirection = transform.right;
        whipDirection.z = 0.0f;
        whipDirection.Normalize();
        //        Vector3 whipTangent = transform.up;

        // Vector3 whipTangent = Vector3.Cross(Vector3.forward, whipDirection);//transform.up;

        Vector3 whipOrigin = transform.position;

        float flameAnim = m_FlameAnimation.Evaluate(enableState ? Time.time - enableTime : flameAnimationTime - (Time.time - enableTime));
        if (!enableState && Time.time - enableTime > flameAnimationTime)
        {
        	if (!HasParticleAlive())
            	gameObject.active = false;
        }

        float xStep = (flameAnim * m_distance * m_effectScale) / (m_splits + 1);
//        m_collisionSplit = (int)m_splits + 1;
        m_collisionSplit = (int)m_splits - 1;
        m_collisionDistance = 10000000.0f;

//        Debug.DrawLine(transform.position, transform.position + transform.right * m_distance * m_effectScale * 2.0f);

        if (Physics.Raycast(transform.position, transform.right, out hit, m_collisionMaxDistance, m_AllLayerMask))
        {

            int hitLayer = 1 << hit.transform.gameObject.layer;

            foreach (CollisionPrefab cp in m_collisionPrefabs)
            {
                if ((cp.m_iCollisionLayerMask & hitLayer) != 0)
                {
                    if (Time.time > m_lastTime + cp.m_CollisionDelay)
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
            m_collisionPlane.transform.up = hit.normal;


			SetParticleCollisionsPlane( m_collisionPlane.transform );


            m_collisionDistance = hit.distance;

            Vector3 hitNormal = hit.normal;
            float wn = Vector3.Dot(hitNormal, whipDirection);
            // Vector3 whipReflect = whipDirection - (hitNormal * wn * 2.0f);
//            Vector3 whipReflectTangent = Vector3.Cross(whipReflect, (whipDirection.x < 0.0f) ? -Vector3.forward: Vector3.forward);
            // Vector3 whipReflectTangent = Vector3.Cross(Vector3.forward, whipReflect);

            for (int i = 0; i < m_splits + 1; i++)
            {
                float currentDist = (xStep * (i + 1));

                if (currentDist < hit.distance)
                {
                    m_whip[i] = whipOrigin + (whipDirection * currentDist);
                }
                else if (currentDist < (hit.distance + xStep))
                {
                    m_whip[i] = hit.point;
                    m_collisionSplit = i;
                }
                else
                {
                    m_whip[i] = hit.point;
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
                    m_whipTangent[i] = -transform.up;
                }
                else
                {
                    m_whipTangent[i] = transform.up;
                }
            }
            else
            {
                whipDirection = m_realWhip[i] - m_realWhip[i - 1];
//                whipDirection.z = 0.0f;
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
    	}

        enableTime = Time.time;
        enableState = value;
    }

	public void SwitchToWaterMode()
    {
		m_insideWater = true;
		m_waterHeigth = transform.position.y;
		for( int i = 0; i<m_fireParticles.Count; i++ )
			m_fireParticles[i].Stop();

		for( int i = 0; i<m_underWaterParticles.Count; i++ )
			m_underWaterParticles[i].Play();
    }

    public void SwitchToNormalMode()
    {
		m_insideWater = false;
		for( int i = 0; i<m_fireParticles.Count; i++ )
			m_fireParticles[i].Play();

		for( int i = 0; i<m_underWaterParticles.Count; i++ )
			m_underWaterParticles[i].Stop();
    }
}
