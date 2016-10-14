﻿using UnityEngine;
using System.Collections;

public class FireBreathDynamic : MonoBehaviour 
{
	// Mesh cache
	private int[] m_triangles = null;
	private Vector3[] m_pos = null;
	private Vector2[] m_UV = null;
    private Color[] m_color = null;

    // Meshes
    private Mesh m_mesh = null;

	// Cached components
	private MeshFilter m_meshFilter = null;

    public float m_distance = 5;
    public float m_aplitude = 6;
    private float m_splits = 5;

    private int m_numPos = 0;

    public float m_fireFlexFactor = 1.0f;

    private Vector3[] m_whip;
    private Vector3[] m_realWhip;
    private Vector3[] m_whipTangent;
//    bool[] m_whipCollision;

    private int m_collisionSplit = 0;
    private float m_collisionDistance = 0.0f;
    private float m_particleDistance = 0.0f;

    public Color m_initialColor;
    public Color m_flameColor;
    public Color m_collisionColor;

    public AnimationCurve m_shapeCurve;
    public AnimationCurve m_FlameAnimation;
    public AnimationCurve m_FlexCurve;


    public string m_collisionFirePrefab;
    public float m_collisionFireDelay = 0.5f;
    public int m_collisionEmiters = 10;

    private float flameAnimationTime = 0.0f;

    public string m_groundLayer;
    public string[] m_enemyLayers;

    private int m_groundLayerMask;


    private Vector3 lastInitialPosition;
    private GameObject whipEnd;

    public float timeDelay = 0.25f;

    public float effectScale = 1.0f;

    private float m_lastTime;

    private float enableTime = 0.0f;
    private bool enableState = false;


    private ParticleSystem fireToonCopy;

    T CopyComponent<T>(T original, GameObject destination) where T : Component
    {
        System.Type type = original.GetType();
        Component copy = destination.AddComponent(type);
        System.Reflection.FieldInfo[] fields = type.GetFields();
        foreach (System.Reflection.FieldInfo field in fields)
        {
            field.SetValue(copy, field.GetValue(original));
        }
        return copy as T;
    }

    // Use this for initialization
    void Start () 
	{
        ParticleManager.CreatePool(m_collisionFirePrefab, "", m_collisionEmiters);
        // Cache
        m_meshFilter = GetComponent<MeshFilter>();
		m_numPos = (int)(4 + m_splits * 2);

        m_groundLayerMask = LayerMask.GetMask(m_groundLayer);

        whipEnd = transform.FindChild("WhipEnd").gameObject;
//        ParticleSystem fireToonInstance = whipEnd.GetComponentInChildren<ParticleSystem>();
//        fireToonCopy = CopyComponent<ParticleSystem>(fireToonInstance, fireToonInstance.gameObject);

        InitWhip();
		InitArrays();
//		InitUVs();
		InitTriangles();

		ReshapeFromWhip();

        CreateMesh();

        lastInitialPosition = whipEnd.transform.position;

        flameAnimationTime = m_FlameAnimation[m_FlameAnimation.length - 1].time;
        enableTime = m_lastTime = Time.time;

    }

    void InitWhip()
	{
		m_whip = new Vector3[(int)m_splits + 1];
        m_realWhip = new Vector3[(int)m_splits + 1];
        m_whipTangent = new Vector3[(int)m_splits + 1];
//        m_whipCollision = new bool[(int)m_splits + 1];

        float xStep = (m_distance * effectScale) / (m_splits + 1);
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

    void ReshapeFromWhip( /* float angle? */)
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
			float yDisplacement = m_shapeCurve.Evaluate(step/(float)(m_splits+2)) * m_aplitude;

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

        lastInitialPosition = whipEnd.transform.position;
    }

    void InitUVs()
    {
        //		m_UV[0] = Vector2.right * 0.5f;
        //		m_UV[1] = Vector2.right * 0.5f;
        float vStep = 1.0f / (m_splits + 1);
        float hStep = 1.0f / (m_splits + 1);

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

        Vector3 particlePos = whipEnd.transform.localPosition;
        float particleDistance = m_distance * Mathf.Pow(effectScale, 1.5f);
        particlePos.x = m_collisionDistance < particleDistance ? m_collisionDistance : particleDistance;
        //        whipEnd.transform.SetLocalPosX(m_distance * effectScale);
        whipEnd.transform.localPosition = particlePos;

//        Debug.Log("ScaleQuick:" + whipEnd.transform.GetGlobalScaleQuick());
//        Debug.Log("ScaleLossy:" + whipEnd.transform.lossyScale);

        //        whipEnd.transform.SetLocalScale(effectScale);


    }

    void UpdateWhip()
    {
        RaycastHit hit;
        Vector3 whipDirection = transform.right;
        whipDirection.z = 0.0f;
        whipDirection.Normalize();
        //        Vector3 whipTangent = transform.up;

        Vector3 whipTangent = Vector3.Cross(Vector3.forward, whipDirection);//transform.up;

        Vector3 whipOrigin = transform.position;

        float flameAnim = m_FlameAnimation.Evaluate(enableState ? Time.time - enableTime : flameAnimationTime - (Time.time - enableTime));
        if (!enableState && Time.time - enableTime > flameAnimationTime)
        {
            gameObject.active = false;
        }

        float xStep = (flameAnim * m_distance * effectScale) / (m_splits + 1);
//        m_collisionSplit = (int)m_splits + 1;
        m_collisionSplit = (int)m_splits - 1;
        m_collisionDistance = 10000000.0f;

        if (Physics.Raycast(transform.position, transform.right, out hit, m_distance * effectScale * 2.0f, m_groundLayerMask))
        {

            if (Time.time > m_lastTime + m_collisionFireDelay)
            {
                GameObject colFire = ParticleManager.Spawn(m_collisionFirePrefab, hit.point, "");
                if (colFire != null)
                {
                    colFire.transform.rotation = Quaternion.LookRotation(-Vector3.forward, hit.normal);
                }

                m_lastTime = Time.time;
            }

            m_collisionDistance = hit.distance;

            Vector3 hitNormal = hit.normal;
            float wn = Vector3.Dot(hitNormal, whipDirection);
            Vector3 whipReflect = whipDirection - (hitNormal * wn * 2.0f);
//            Vector3 whipReflectTangent = Vector3.Cross(whipReflect, (whipDirection.x < 0.0f) ? -Vector3.forward: Vector3.forward);
            Vector3 whipReflectTangent = Vector3.Cross(Vector3.forward, whipReflect);
            //            Vector3 whipReflectTangent = Vector3.Cross(whipReflect, Vector3.forward);
/*
            if (Time.time > (m_lastTime + timeDelay))
            {
                GameObject colFire = ParticleManager.Spawn(m_collisionFirePrefab, hit.point, "Fire&Destruction/_PrefabsWIP/FireEffects/");
                if (colFire != null)
                {
                    colFire.transform.rotation = Quaternion.LookRotation(-Vector3.forward, hit.normal);
                }

                m_lastTime = Time.time;
            }
*/
            for (int i = 0; i < m_splits + 1; i++)
            {
                float currentDist = (xStep * (i + 1));

                if (currentDist < hit.distance)
                {
                    m_whip[i] = whipOrigin + (whipDirection * currentDist);
//                    m_whipTangent[i] = whipTangent;
//                    m_whipCollision[i] = false;
                }
                else if (currentDist < (hit.distance + xStep))
                {
                    m_whip[i] = hit.point;
                    m_collisionSplit = i;
                }
                else
                {
                    m_whip[i] = hit.point;
//                    m_whipTangent[i] = whipTangent;

                    // (whipTangent + whipReflectTangent).normalized;
                    //                    m_whip[i] = hit.point + ((currentDist - hit.distance) * whipReflect);
                    //                    m_whipTangent[i] = whipReflectTangent;
                    //                    m_whipCollision[i] = false;
                }

            }
        }
        else
        {
            for (int i = 0; i < m_splits + 1; i++)
            {
                float currentDist = (xStep * (i + 1));
                m_whip[i] = whipOrigin + (whipDirection * currentDist);
//                m_whipTangent[i] = whipTangent;
//                m_whipCollision[i] = false;
            }
        }


        for (int i = 0; i < m_splits + 1; i++)
        {
            Vector3 distance = m_whip[i] - m_realWhip[i];
            float fq = Mathf.Pow(1.0f - (i / (m_splits + 1)), m_fireFlexFactor);
//            float rq = Mathf.Clamp(fq + (Vector3.Dot(distance, distance) / mrq) * m_fireFlexFactor * Time.deltaTime, 0.0f, 1.0f);
//            float rq = Mathf.Clamp(fq + (m_fireFlexFactor * Time.deltaTime), 0.0f, 1.0f);
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

    public void EnableFlame(bool value)
    {
        if (value)
        {
            gameObject.active = value;
        }

        enableTime = Time.time;
        enableState = value;
    }
}
