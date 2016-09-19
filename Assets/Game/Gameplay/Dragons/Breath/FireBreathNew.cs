using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class FireBreathNew : DragonBreathBehaviour {

	[Header("Emitter")]
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

	[SerializeField] private AnimationCurve m_sizeCurve = AnimationCurve.Linear(0, 0, 1, 3f);	// Will be used by the inspector to easily setup the values for each level
	public AnimationCurve curve
	{
		get { return m_sizeCurve; }
		set	{ m_sizeCurve = value; }
	}

	[Header("Particle")]

	private int m_groundMask;
	private int m_noPlayerMask;

	private Transform m_mouthTransform;
	private Transform m_headTransform;

	private Vector2 m_directionP;

	private Vector2 m_triP0;
	private Vector2 m_triP1;
	private Vector2 m_triP2;

	private Vector2 m_sphCenter;
	private float m_sphRadius;

	private float m_area;

	private int m_frame;

	private GameObject m_light;

	public string m_flameLight = "PF_FireLight";

	float m_timeToNextLoopAudio = 0;
	AudioSource m_lastAudioSource;

	private Entity[] m_checkEntities = new Entity[50];
	private int m_numCheckEntities = 0;

    [Header("BreathMesh")]

    // Mesh cache
    private int[] m_triangles = null;
    private Vector3[] m_pos = null;
    private Vector2[] m_UV = null;
    private Color[] m_color = null;

    // Meshes
    private Mesh m_mesh = null;


    // Cached components
    private MeshFilter m_meshFilter = null;

    public float m_distance = 10;
    public float m_aplitude = 6;
    private float m_splits = 10;

    private int m_numPos = 0;

    Vector3[] m_whip;
    Vector3[] m_whipTangent;
    //    bool[] m_whipCollision;

    private int m_collisionSplit = 0;


    public Color m_initialColor;
    public Color m_flameColor;
    public Color m_collisionColor;

    public AnimationCurve m_shapeCurve;
    public AnimationCurve m_FlameAnimation;
    public AnimationCurve m_FlexCurve;

    public float fireDelay = 1.0f;

    public string m_groundLayer;
    public string[] m_enemyLayers;

    private int m_groundLayerMask;

    private Vector3 lastInitialPosition;
    private GameObject whipEnd;

    //    public FireOfBreathScript breathFire = null;
    public GameObject breathFire = null;
    public float timeDelay = 0.25f;

    private float lastTime;

    override protected void ExtendedStart() {

        // Cache
        m_meshFilter = GetComponent<MeshFilter>();
        m_numPos = (int)(4 + m_splits * 2);

        m_groundLayerMask = LayerMask.GetMask(m_groundLayer);

        whipEnd = transform.FindChild("WhipEnd").gameObject;
        whipEnd.transform.SetLocalPosX(m_distance);

        InitWhip();
        InitArrays();
        InitUVs();
        InitTriangles();

        ReshapeFromWhip();

        CreateMesh();

        lastInitialPosition = whipEnd.transform.position;

        lastTime = Time.time;


        PoolManager.CreatePool((GameObject)Resources.Load("Particles/" + m_flameLight), 1, false);

		m_groundMask = LayerMask.GetMask("Ground", "Water", "GroundVisible");
		m_noPlayerMask = ~LayerMask.GetMask("Player");

		m_mouthTransform = GetComponent<DragonMotion>().tongue;
		m_headTransform = GetComponent<DragonMotion>().head;

		m_length = m_dragon.data.def.GetAsFloat("furyBaseLenght");
		m_length *= transform.localScale.x;
		float lengthIncrease = m_length * m_dragon.data.fireSkill.value;
		m_length += lengthIncrease;

		m_actualLength = m_length;

		m_sphCenter = m_mouthTransform.position;
		m_sphRadius = 0;

		m_direction = Vector2.zero;
		m_directionP = Vector2.zero;

		m_frame = 0;

		m_light = null;
	}


    void InitWhip()
    {
        m_whip = new Vector3[(int)m_splits + 1];
        m_whipTangent = new Vector3[(int)m_splits + 1];
        //        m_whipCollision = new bool[(int)m_splits + 1];

        float xStep = m_distance / (m_splits + 1);
        Vector3 move = transform.right;
        Vector3 pos = transform.position;
        for (int i = 0; i < (m_splits + 1); i++)
        {
            m_whip[i] = pos + (move * xStep * i);
            m_whipTangent[i] = transform.up;
            //            m_whipCollision[i] = false;

        }
    }
    void InitArrays()
    {
        m_pos = new Vector3[m_numPos];
        m_UV = new Vector2[m_numPos];
        m_color = new Color[m_numPos];
        for (int i = 0; i < m_numPos; i++)
        {
            m_pos[i] = Vector3.zero;
            m_UV[i] = Vector2.zero;
            m_color[i] = (i < 4) ? m_initialColor : m_flameColor;
        }
    }

    void InitUVs()
    {
        m_UV[0] = Vector2.right * 0.5f;
        m_UV[1] = Vector2.right * 0.5f;
        float vStep = 1.0f / (m_splits + 1);
        float hStep = 1.0f / (m_splits + 1);

        int step = 1;
        for (int i = 2; i < m_numPos; i += 2)
        {
            float xDisplacement = m_shapeCurve.Evaluate(step / (float)(m_splits + 2)) * 0.5f;

            //m_UV[i].x = 0.5f + hStep/2.0f * step;
            m_UV[i].x = 0.5f + xDisplacement;
            m_UV[i].y = vStep * step;

            // m_UV[i+1].x = 0.5f - hStep/2.0f * step;
            m_UV[i + 1].x = 0.5f - xDisplacement;
            m_UV[i + 1].y = vStep * step;

            step++;
        }
    }

    void InitTriangles()
    {
        int numTrianglesIndex = (m_numPos - 2) * 3;
        m_triangles = new int[numTrianglesIndex];

        int pos = 0;
        for (int i = 0; i < numTrianglesIndex; i += 3)
        {
            m_triangles[i] = pos;
            if (pos % 2 == 0)
            {
                m_triangles[i + 1] = pos + 2;
                m_triangles[i + 2] = pos + 1;

            }
            else
            {
                m_triangles[i + 1] = pos + 1;
                m_triangles[i + 2] = pos + 2;
            }
            pos++;
        }
    }

    void ReshapeFromWhip( /* float angle? */)
    {
        m_pos[0] = m_pos[1] = Vector3.zero;

        int step = 1;
        int whipIndex = 0;
        Vector3 newPos1, newPos2;
        //        Vector3 WhipExtreme = 

        for (int i = 2; i < m_numPos; i += 2)
        {
            float yDisplacement = m_shapeCurve.Evaluate(step / (float)(m_splits + 2)) * m_aplitude;
            Vector3 whipTangent = transform.InverseTransformDirection(m_whipTangent[whipIndex]);

            newPos1 = newPos2 = transform.InverseTransformPoint(m_whip[whipIndex]);

            float kd = m_FlexCurve.Evaluate(whipIndex / m_splits);

            float md = (whipEnd.transform.position.y - lastInitialPosition.y) * kd * fireDelay;


            if (transform.right.x < 0.0f)
            {
                newPos1 += (whipTangent) * (yDisplacement + md);
                newPos2 -= (whipTangent) * (yDisplacement - md);
            }
            else
            {
                newPos1 += (whipTangent) * (yDisplacement - md);
                newPos2 -= (whipTangent) * (yDisplacement + md);
            }


            m_pos[i] = newPos1;
            m_pos[i + 1] = newPos2;

            if (i > 2)
            {
                m_color[i] = m_color[i + 1] = (whipIndex > m_collisionSplit) ? m_collisionColor : m_flameColor;
            }

            step++;
            whipIndex++;
        }

        //        Debug.Log("lastInitialPositionVariation: " + (lastInitialPosition - transform.position).ToString());

        lastInitialPosition = whipEnd.transform.position;


    }


    // Recreates the mesh
    void CreateMesh()
    {
        m_mesh = new Mesh();
        m_mesh.MarkDynamic();

        m_mesh.vertices = m_pos;
        m_mesh.uv = m_UV;
        m_mesh.colors = m_color;


        m_mesh.SetTriangles(m_triangles, 0);
        // m_mesh.SetIndices(m_triangles, MeshTopology.Triangles, 0);
        m_meshFilter.sharedMesh = m_mesh;

    }

    override public bool IsInsideArea(Vector2 _point) { 
	
		if (m_isFuryOn) {
			if (m_bounds2D.Contains(_point)) {
				return IsInsideTriangle( _point );
			}
		}

		return false; 
	}

	override public bool Overlaps(CircleAreaBounds _circle)
	{
		if (m_isFuryOn) 
		{
			if (_circle.Overlaps(m_bounds2D)) 
			{
				if (IsInsideTriangle( _circle.center ))
				{
					return true;
				}
				return ( _circle.OverlapsSegment( m_triP0, m_triP1 ) || _circle.OverlapsSegment( m_triP1, m_triP2 ) || _circle.OverlapsSegment( m_triP2, m_triP0 ) );
			}
		}
		return false;
	}

	private bool IsInsideTriangle( Vector2 _point )
	{
		float sign = m_area < 0 ? -1 : 1;
		float s = (m_triP0.y * m_triP2.x - m_triP0.x * m_triP2.y + (m_triP2.y - m_triP0.y) * _point.x + (m_triP0.x - m_triP2.x) * _point.y) * sign;
		float t = (m_triP0.x * m_triP1.y - m_triP0.y * m_triP1.x + (m_triP0.y - m_triP1.y) * _point.x + (m_triP1.x - m_triP0.x) * _point.y) * sign;
		
		return s > 0 && t > 0 && (s + t) < 2 * m_area * sign;
	}

	override protected void BeginFury(Type _type) 
	{
		base.BeginFury( _type);
		m_lastAudioSource  = AudioManager.instance.PlayClip("audio/sfx/Burning/Flamethrower first");
		m_timeToNextLoopAudio = m_lastAudioSource.clip.length;
		m_light = PoolManager.GetInstance(m_flameLight);
		m_light.transform.position = m_mouthTransform.position;
		m_light.transform.localScale = new Vector3(m_actualLength * 1.25f, m_sizeCurve.Evaluate(1) * transform.localScale.x * 1.75f, 1f);
	}

	override protected void EndFury() 
	{
		base.EndFury();
		// Stop loop clip!
		m_lastAudioSource.Stop();
		m_lastAudioSource = null;
		AudioManager.instance.PlayClip("audio/sfx/Burning/Flamethrower End");
		m_light.SetActive(false);
		PoolManager.ReturnInstance( m_light );
		m_light = null;
	}

	override protected void Breath(){
		m_direction = m_mouthTransform.position - m_headTransform.position;
		m_direction.Normalize();
		m_directionP.Set(m_direction.y, -m_direction.x);

		m_timeToNextLoopAudio -= Time.deltaTime;
		if ( m_timeToNextLoopAudio <= 0f )
		{
			switch( Random.Range(0,2))
			{
				case 0:
				{
					m_lastAudioSource  = AudioManager.instance.PlayClip("audio/sfx/Burning/loop 1");
				}break;
				case 1:
				{
					m_lastAudioSource  = AudioManager.instance.PlayClip("audio/sfx/Burning/loop 2");
				}break;
			}
			m_timeToNextLoopAudio = m_lastAudioSource.clip.length;
		}

		float length = m_length;
		if ( m_type == Type.Super )
			length = m_length * m_superFuryLengthMultiplier;

		Vector3 flamesUpDir = Vector3.up;
		bool hitingWater = false;
		if (m_frame == 0) {
			// Raycast to ground
			RaycastHit ground;				
			if (Physics.Linecast(m_mouthTransform.position, m_mouthTransform.position + (Vector3)m_direction * length, out ground, m_groundMask)) 
			{
				m_actualLength = ground.distance;
				if ( ground.collider.tag == "Water" )
					hitingWater = true;
			} 
			else 
			{				
				m_actualLength = length;
			}

			if (Physics.Linecast(m_mouthTransform.position, m_mouthTransform.position + (Vector3)m_direction * length, out ground, m_noPlayerMask)) 
			{
				flamesUpDir = Vector3.Reflect( m_direction, ground.normal);
				flamesUpDir.Normalize();

			}


		}
		{
			// Pre-Calculate Triangle: wider bounding triangle to make burning easier
			m_triP0 = m_mouthTransform.position;
			m_triP1 = m_triP0 + m_direction * m_actualLength - m_directionP * m_sizeCurve.Evaluate(1) * transform.localScale.x * 0.5f;
			m_triP2 = m_triP0 + m_direction * m_actualLength + m_directionP * m_sizeCurve.Evaluate(1) * transform.localScale.x * 0.5f;
			m_area = (-m_triP1.y * m_triP2.x + m_triP0.y * (-m_triP1.x + m_triP2.x) + m_triP0.x * (m_triP1.y - m_triP2.y) + m_triP1.x * m_triP2.y) * 0.5f;

			// Circumcenter
			float c = ((m_triP0.x + m_triP1.x) * 0.5f) + ((m_triP0.y + m_triP1.y) * 0.5f) * ((m_triP0.y - m_triP1.y) / (m_triP0.x - m_triP1.x));
			float d = ((m_triP2.x + m_triP1.x) * 0.5f) + ((m_triP2.y + m_triP1.y) * 0.5f) * ((m_triP2.y - m_triP1.y) / (m_triP2.x - m_triP1.x)); 

			m_sphCenter.y = (d - c) / (((m_triP2.y - m_triP1.y) / (m_triP2.x - m_triP1.x)) - ((m_triP0.y - m_triP1.y) / (m_triP0.x - m_triP1.x)));
			m_sphCenter.x = c - m_sphCenter.y * ((m_triP0.y - m_triP1.y) / (m_triP0.x - m_triP1.x));

			m_sphRadius = (m_sphCenter - m_triP1).magnitude;

			m_bounds2D.Set(m_sphCenter.x - m_sphRadius, m_sphCenter.y - m_sphRadius, m_sphRadius * 2f, m_sphRadius * 2f);
		}

		// Spawn particles
		if ( hitingWater )	// if hitting water => show steam
		{
			
		}

		float lerpT = 0.15f;

		//Vector3 pos = new Vector3(m_triP0.x, m_triP0.y, -8f);
		m_light.transform.position = m_triP0; //Vector3.Lerp(m_light.transform.position, pos, 1f);

		float angle = Vector3.Angle(Vector3.right, m_direction);
		if (m_direction.y > 0) angle *= -1;
		m_light.transform.localRotation = Quaternion.Lerp(m_light.transform.localRotation, Quaternion.AngleAxis(angle, Vector3.back), lerpT);

		m_frame = (m_frame + 1) % 4;


		//--------------------------------------------------------------------------------------------
		// try to burn things!!!
		// search for preys!
		// Entity[] preys = EntityManager.instance.GetEntitiesInRange2D(m_sphCenter, m_sphRadius);
		m_numCheckEntities =  EntityManager.instance.GetOverlapingEntities(m_sphCenter, m_sphRadius, m_checkEntities);
		for (int i = 0; i < m_numCheckEntities; i++) 
		{
			Entity prey = m_checkEntities[i];
			if ((prey.circleArea != null && Overlaps((CircleAreaBounds)prey.circleArea.bounds)) || IsInsideArea(prey.transform.position)) 
			{
				if (CanBurn(prey) || m_type == Type.Super) {
					AI.Machine machine =  m_checkEntities[i].GetComponent<AI.Machine>();
					if (machine != null) {
						machine.Burn(damage * Time.deltaTime, transform);
					}
				} else {
					// Show message saying I cannot burn it
					Messenger.Broadcast<DragonTier, string>(GameEvents.BIGGER_DRAGON_NEEDED, DragonTier.COUNT, prey.sku);
				}
			}
		}
	}

	void OnDrawGizmos() {
		if (m_isFuryOn) {
			Gizmos.color = Color.magenta;

			Gizmos.DrawLine(m_triP0, m_triP1);
			Gizmos.DrawLine(m_triP1, m_triP2);
			Gizmos.DrawLine(m_triP2, m_triP0);

			Gizmos.DrawWireSphere(m_sphCenter, m_sphRadius);

			Gizmos.color = Color.green;
			switch(m_type)
			{
				case Type.Standard:
				{
					Gizmos.DrawLine(m_mouthTransform.position, m_mouthTransform.position + (Vector3)m_direction * m_length * 2);
				}break;
				case Type.Super:
				{
					Gizmos.DrawLine(m_mouthTransform.position, m_mouthTransform.position + (Vector3)m_direction * m_length);
				}break;
			}
		}
	}

	void OnTriggerEnter(Collider _other)
	{
		if ( _other.tag == "Water" )
		{
			if ( m_isFuryOn )
			{
				m_isFuryPaused = true;
				m_animator.SetBool("breath", false);
			}

		}
	}

	void OnTriggerExit(Collider _other)
	{
		if ( _other.tag == "Water" )
		{
			m_isFuryPaused = false;
		}
	}
}
