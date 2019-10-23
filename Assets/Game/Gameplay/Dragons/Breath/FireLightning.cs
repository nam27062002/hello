using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FireLightning : DragonBreathBehaviour {

	public float m_segmentLength = 25f; 

    public float m_maxAmplitude = 1.0f;
    public AnimationCurve m_widthCurve = new AnimationCurve();
    public float m_widthMultiplier = 1.0f;
    public float m_offsetRays = 0.5f;
	public Material m_rayMaterial;

    public int m_numRays = 1;

    public float m_maxAmplitude2 = 1.0f;
    public AnimationCurve m_widthCurve2 = new AnimationCurve();
    public float m_widthMultiplier2 = 1.0f;
    public float m_offsetRays2 = 0.5f;
    public Material m_rayMaterial2;

    public float m_logicDetectionSize = 1;

    private float relativeSizeMultiplier = 1;

    public int m_numRays2 = 1;


    private int m_numRaysOld = -1;

	// Test
	[SerializeField] private float m_length = 6f;
	public float length
	{
		get
		{
			return m_length;
		}
		set
		{
			m_length = value;
		}
	}

	private float m_currentLength;

	public Object m_particleStartPrefab;
	public Object m_particleEndPrefab;

    GameObject m_particleStart;
	GameObject m_particleEnd;
    ParticleSystem[] m_PSend;

    Transform m_mouthTransform;
	Transform m_headTransform;

	bool m_insideWater;

    Lightning[] m_rays = null;// new Lightning[3];
    Lightning[] m_rays2 = null;// new Lightning[3];

	private Entity[] m_checkEntities = new Entity[50];
	private int m_numCheckEntities = 0;


    public class Lightning
    {

		public LineRenderer m_line;

		public float m_amplitude;
		public float m_segmentLength;
        public float m_initOffset;

        //		public Lightning(float rayWidth, Color color,float numSegments, Material rayMaterial)
        public Lightning(Color color, Material rayMaterial, AnimationCurve shapeCurve)
//        public Lightning(float iniRayWidth, float endRayWidth, Color color, Material rayMaterial, AnimationCurve shapeCurve)
        {
            GameObject obj = new GameObject();
			obj.name = "RaySegment";
			obj.transform.parent = GameObject.Find ("InstanceManager").transform;
			m_line = obj.AddComponent<LineRenderer>();
			m_line.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            m_line.textureMode = LineTextureMode.Tile;
			m_line.receiveShadows = false;
//			m_line.SetWidth(iniRayWidth, endRayWidth);
            m_line.widthCurve = shapeCurve;
            m_line.SetColors(color,color);
			m_line.material = new Material(rayMaterial);
			m_line.enabled = false; 
		}


		public void Draw(Vector3 start, Vector3 end){

			Vector3 previous = start;
			Vector3 dir = (end-start).normalized;
			Vector3 normal = Vector3.Cross(dir,Vector3.forward);
			float dist = (end-start).magnitude;
			int numSegments = (int)(dist/m_segmentLength);
			m_line.SetVertexCount(numSegments + 1);
			m_line.enabled = true;

//            float amplitude = Mathf.Clamp(Mathf.Cos(Time.time * Mathf.Deg2Rad * 180.0f) * m_amplitude, 0.0f, 1.0f);
            float amplitude = m_amplitude;

            for (int i=0;i<(int)numSegments;i++)
			{
				m_line.SetPosition(i,previous);
				// dir  = (end-previous).normalized;
				previous = previous+dir*m_segmentLength+normal*Random.Range (-amplitude, amplitude);
			}
			m_line.SetPosition( numSegments, end );

//            m_line.material.SetColor("_RayColor", Random.ColorHSV(0.0f, 1.0f, 0.5f, 1.0f, 0.75f, 1.0f));
//            m_line.material.SetFloat("_RayOffset", m_initOffset);

        }

        public void Hide()
		{
				m_line.enabled = false;
		}

        public void Destroy()
        {
            DestroyObject(m_line.gameObject);
        }
	}

    void EnableEndBall(bool value)
    {
        if (m_PSend != null)
        {
            if (value)
            {
                if (!m_PSend[1].isPlaying)
                {
                    m_PSend[0].Play();
                    m_PSend[1].Play();
                    m_PSend[2].Play();
                }
            }
            else
            {
                m_PSend[0].Play();
                m_PSend[1].Stop();
                m_PSend[2].Stop();

            }
        }
    }

	// Use this for initialization
	override protected void ExtendedStart () 
	{
		if ( m_particleStartPrefab )
			m_particleStart = (GameObject)Object.Instantiate(m_particleStartPrefab);
		if ( m_particleStart )
		{
			m_particleStart.transform.localPosition = Vector3.zero;
			m_particleStart.gameObject.SetActive(true);
		}

		if ( m_particleEndPrefab )
			m_particleEnd = (GameObject)Object.Instantiate(m_particleEndPrefab);

		if ( m_particleEnd )
		{
            m_PSend = m_particleEnd.GetComponentsInChildren<ParticleSystem>();

            m_particleEnd.transform.localPosition = Vector3.zero;
			m_particleEnd.gameObject.SetActive(true);

		}

        m_mouthTransform = transform.FindTransformRecursive("Fire_Dummy");
		m_headTransform = GetComponent<DragonMotion>().head;

        /*
                m_rays[0] = new Lightning(m_segmentInitialWidth, m_segmentFinalWidth, Color.white, m_rayMaterial, m_shapeCurve);
                m_rays[0].m_segmentLength = m_segmentLength;
                m_rays[0].m_initOffset = 0.0f;

                m_rays[1] = new Lightning(m_segmentInitialWidth, m_segmentFinalWidth, Color.grey, m_rayMaterial, m_shapeCurve);
                m_rays[1].m_segmentLength = m_segmentLength;
                m_rays[1].m_initOffset = m_offsetRays;

                m_rays[2] = new Lightning(m_segmentInitialWidth, m_segmentFinalWidth, new Color(0.25f,0.25f,0.25f,1f), m_rayMaterial, m_shapeCurve);
                m_rays[2].m_segmentLength = m_segmentLength;
                m_rays[2].m_initOffset = m_offsetRays * 2.0f;
        */

        m_actualLength = m_length;
		m_currentLength = m_length;
		m_insideWater = false;
	}

    void updateRays()
    {
        if (m_numRays != m_numRaysOld)
        {
            destroyRays(m_rays);
            destroyRays(m_rays2);

            m_rays = new Lightning[m_numRays];
            m_rays2 = new Lightning[m_numRays2];
            float offStep = 1.0f / (float)m_numRays;
            for (int c = 0; c < m_rays.Length; c++)
            {
                m_rays[c] = new Lightning(Color.gray, m_rayMaterial, m_widthCurve);
                m_rays[c].m_segmentLength = m_segmentLength;
                m_rays[c].m_initOffset = offStep * (float)c;
            }

            for (int c = 0; c < m_rays2.Length; c++)
            {
                m_rays2[c] = new Lightning(Color.gray, m_rayMaterial2, m_widthCurve2);
                m_rays2[c].m_segmentLength = m_segmentLength;
                m_rays2[c].m_initOffset = offStep * (float)c;
            }

            m_numRaysOld = m_numRays;
        }
    }

    static void destroyRays(Lightning[] rays)
    {
        if (rays != null)
        {
            for (int c = 0; c < rays.Length; c++)
            {
                rays[c].Destroy();
                rays[c] = null;
            }
        }
        rays = null;
    }


    void OnDestroy()
    {
        destroyRays(m_rays);
        destroyRays(m_rays2);

        print("FireLightning destroy!!!!");
    }

    static public void SetAmplitude(Lightning[] rays, float amplitude)
	{
//		m_maxAmplitude = amplitude;

        if (rays != null)
        {
            for (int c = 0; c < rays.Length; c++)
            {
                rays[c].m_amplitude = amplitude;
            }
        }
	}

    public void SetWidthCurve(Lightning[] rays, AnimationCurve curve)
    {
        if (rays != null)
        {
            for (int c = 0; c < rays.Length; c++)
            {
                rays[c].m_line.widthCurve = curve;
            }
        }
    }

    public void SetWidthMultiplier(Lightning[] rays, float multiplier)
    {
        if (rays != null)
        {
            for (int c = 0; c < rays.Length; c++)
            {
                rays[c].m_line.widthMultiplier = multiplier;
            }
        }
    }

    public void HideRays(Lightning[] rays)
    {
        if (rays != null)
        {
            for (int c = 0; c < rays.Length; c++)
            {
                rays[c].Hide();
            }
        }
    }

    override protected void Breath()
	{
		m_direction = -m_mouthTransform.right;
		m_direction.Normalize();

		Vector3 p1 = m_mouthTransform.position;
		if ( m_particleStart )
			m_particleStart.transform.position = m_mouthTransform.position;

		Vector3 p2;

		RaycastHit ground;
		float length = m_length;


        updateRays();
        SetAmplitude(m_rays, m_maxAmplitude);
        SetAmplitude(m_rays2, m_maxAmplitude2);

        bool isGround = false;

        if ( m_insideWater )
		{
			m_currentLength = Mathf.Lerp(m_currentLength, m_length * 2f, Time.deltaTime * 2);
		}
		else
		{
            if (Physics.Linecast( m_mouthTransform.position, m_mouthTransform.position+(Vector3)m_direction*length, out ground, GameConstants.Layers.WATER))
			{
				float addition = (length - ground.distance); // distance enering water
				// length += addition;	// We double it
				m_currentLength = Mathf.Lerp( m_currentLength, m_length + addition, Time.deltaTime * 2);
                isGround = true;
            }
            else
			{
				m_currentLength = Mathf.Lerp( m_currentLength, m_length, Time.deltaTime * 2);
			}
		}

        if (Physics.Linecast( m_mouthTransform.position, m_mouthTransform.position+(Vector3)m_direction*m_currentLength, out ground, GameConstants.Layers.GROUND))
        {
			p2 = ground.point;
			m_actualLength = ground.distance;
            isGround = true;
        }
        else
        {
			p2 =  m_mouthTransform.position+(Vector3)m_direction*m_currentLength;
			m_actualLength = m_currentLength;
		}

		if ( m_particleEnd )
        {
            EnableEndBall(isGround);
//            m_particleEnd.gameObject.SetActive(isGround);
            m_particleEnd.transform.position = p2;
        }

        for (int i=0;i<m_rays.Length;i++)
        {
            m_rays[i].Draw(p1, p2);
        }

        for (int i = 0; i < m_rays2.Length; i++)
        {
            m_rays2[i].Draw(p1, p2);
        }

		SetWidthMultiplier(m_rays, m_widthMultiplier * relativeSizeMultiplier);
		SetWidthMultiplier(m_rays2, m_widthMultiplier2 * relativeSizeMultiplier);

        SetWidthCurve(m_rays, m_widthCurve);
        SetWidthCurve(m_rays2, m_widthCurve2);

        // Look entities to damage!
		m_numCheckEntities = EntityManager.instance.GetEntitiesInNonAlloc((Vector2)m_mouthTransform.position, (Vector2)m_direction, m_logicDetectionSize * 2 * relativeSizeMultiplier, m_actualLength, m_checkEntities);
		for (int i = 0; i < m_numCheckEntities; i++) 
		{
			if (m_checkEntities[i].IsBurnable(m_tier) || m_type == Type.Mega) {
				AI.IMachine machine =  m_checkEntities[i].machine;
				if (machine != null) {					
					machine.Burn(transform, IEntity.Type.PLAYER, false, m_type == Type.Mega);
				}
			}
			/*
			if (!burned){
				// Show I cannot burn this entity!
			}
			*/	
		}

        m_bounds2D.center = m_mouthTransform.position;
		m_bounds2D.width = Mathf.Max( m_actualLength, m_maxAmplitude);
		m_bounds2D.height = Mathf.Max( m_actualLength, m_maxAmplitude);

		base.Breath();
	}



	override protected void BeginFury( Type _type ) 
	{
		base.BeginFury( _type );

		if (_type == Type.Mega){
			relativeSizeMultiplier = 2;
		}else{
			relativeSizeMultiplier = 1;
		}


		if ( m_particleStart )
		{
			m_particleStart.transform.position = m_mouthTransform.position;
			m_particleStart.gameObject.SetActive(true);
		}

		m_direction = -m_mouthTransform.right;
		m_direction.Normalize();

		if ( m_particleEnd )
		{
			m_particleEnd.transform.position = m_mouthTransform.position+(Vector3)m_direction*m_length;
			m_particleEnd.gameObject.SetActive(true);
		}

        HideRays(m_rays);
        HideRays(m_rays2);
	}

	override public void RecalculateSize()
    {
    	if ( m_dragon )
    	{
			float furyBaseLength = m_dragon.data.def.GetAsFloat("furyBaseLength");
			m_length = furyBaseLength + furyBaseLength * lengthPowerUpPercentage / 100.0f;
			m_length *= transform.localScale.x;
		}
    }

	override protected void EndFury( bool increase_mega_fire = true) 
	{
		base.EndFury(increase_mega_fire);
		if ( m_particleStart )
			m_particleStart.gameObject.SetActive(false);
		if ( m_particleEnd )
			m_particleEnd.gameObject.SetActive(false);

        HideRays(m_rays);
        HideRays(m_rays2);

    }


    void OnDrawGizmos() {
		if (IsFuryOn()) 
		{
			Gizmos.color = Color.magenta;
			Gizmos.DrawLine( m_mouthTransform.position, m_mouthTransform.position + (Vector3)m_direction * m_actualLength );

			Vector2 up = m_direction.RotateDegrees(90).normalized;
			Vector3 upStart = m_mouthTransform.position + (Vector3)up * m_logicDetectionSize; 
			Gizmos.DrawLine( upStart, upStart + (Vector3)m_direction * m_actualLength );

			Vector2 down = m_direction.RotateDegrees(-90).normalized;
			Vector3 downStart = m_mouthTransform.position + (Vector3)down * m_logicDetectionSize; 
			Gizmos.DrawLine( downStart, downStart + (Vector3)m_direction * m_actualLength );
		}
	}

	override public bool IsInsideArea(Vector2 _point) { 
	
		if (IsFuryOn()) 
		{
			float halfAmplitude = m_maxAmplitude/2.0f;
			float angle = Mathf.Atan2( m_direction.y, m_direction.x);

			Vector2 inversePos = _point - (Vector2)m_mouthTransform.position;
			inversePos = inversePos.RotateRadians( -angle );
			if ( inversePos.x >= 0 && inversePos.x <= length )
			{
				if ( inversePos.y >= -halfAmplitude && inversePos.y <= halfAmplitude )
				{
					return true;
				}
			}
		}

		return false; 
	}

	void OnTriggerEnter(Collider _other)
	{
		if ( _other.CompareTag("Water") )
		{
			m_insideWater = true;
		}
	}

	void OnTriggerExit(Collider _other)
	{
		if ( _other.CompareTag("Water") )
		{
			m_insideWater = false;
		}
	}




}
