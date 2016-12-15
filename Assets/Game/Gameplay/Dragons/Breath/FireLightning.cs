using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FireLightning : DragonBreathBehaviour {

	public float m_segmentLength = 25f; 
	public float m_segmentWidth = 5f; 
	public float m_maxAmplitude = 50f; 

	public Material m_rayMaterial;

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

	Transform m_mouthTransform;
	Transform m_headTransform;
	Vector3 m_dir;

	int m_groundMask;
	int m_waterMask;
	bool m_insideWater;

	Lightning[] m_rays = new Lightning[3];


	class Lightning{

		LineRenderer m_line;

		public float m_amplitude;
		public float m_segmentLength;
	
		public Lightning(float rayWidth, Color color,float numSegments, Material rayMaterial)
		{
			GameObject obj = new GameObject();
			obj.name = "RaySegment";
			obj.transform.parent = GameObject.Find ("InstanceManager").transform;
			m_line = obj.AddComponent<LineRenderer>();
			m_line.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
			m_line.receiveShadows = false;
			m_line.SetWidth( rayWidth, rayWidth);
			m_line.SetColors(color,color);
			m_line.material = rayMaterial;
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

			for(int i=0;i<(int)numSegments;i++)
			{
				m_line.SetPosition(i,previous);
				// dir  = (end-previous).normalized;
				previous = previous+dir*m_segmentLength+normal*Random.Range (-m_amplitude, m_amplitude);
			}
			m_line.SetPosition( numSegments, end );

		}

		public void Hide()
		{
				m_line.enabled = false;
		}
	}



	// Use this for initialization
	override protected void ExtendedStart () 
	{
		m_particleStart = (GameObject)Object.Instantiate(m_particleStartPrefab);
		m_particleStart.transform.localPosition = Vector3.zero;
		m_particleStart.gameObject.SetActive(true);
		
		m_particleEnd = (GameObject)Object.Instantiate(m_particleStartPrefab);
		m_particleEnd.transform.localPosition = Vector3.zero;
		m_particleEnd.gameObject.SetActive(true);

		m_mouthTransform = GetComponent<DragonMotion>().tongue;
		m_headTransform = GetComponent<DragonMotion>().head;

		m_groundMask = LayerMask.GetMask("Ground", "GroundVisible");
		m_waterMask = LayerMask.GetMask("Water");


		m_rays[0] = new Lightning(m_segmentWidth, Color.white, m_length/m_segmentLength,m_rayMaterial);
		m_rays[0].m_segmentLength = m_segmentLength;

		m_rays[1] = new Lightning(m_segmentWidth*0.5f, Color.grey, m_length/m_segmentLength,m_rayMaterial);
		m_rays[1].m_segmentLength = m_segmentLength;

		m_rays[2] = new Lightning(m_segmentWidth*0.25f, new Color(0.25f,0.25f,0.25f,1f), m_length/m_segmentLength,m_rayMaterial);
		m_rays[2].m_segmentLength = m_segmentLength;

		SetAmplitude( m_maxAmplitude );

		m_actualLength = m_length;
		m_currentLength = m_length;
		m_insideWater = false;
	}

	public void SetAmplitude( float amplitude )
	{
		m_maxAmplitude = amplitude;
		m_rays[0].m_amplitude = m_maxAmplitude*0.25f;
		m_rays[1].m_amplitude = m_maxAmplitude*0.5f;
		m_rays[2].m_amplitude = m_maxAmplitude*0.5f;
	}

	override protected void Breath()
	{
		m_dir = m_mouthTransform.position - m_headTransform.position;
		m_dir.z = 0f;
		m_dir.Normalize();

		Vector3 p1 = m_mouthTransform.position;
		m_particleStart.transform.position = m_mouthTransform.position;

		Vector3 p2;

		RaycastHit ground;
		float length = m_length;
		if ( m_insideWater )
		{
			m_currentLength = Mathf.Lerp(m_currentLength, m_length * 2f, Time.deltaTime * 2);
		}
		else
		{
			if (Physics.Linecast( m_mouthTransform.position, m_mouthTransform.position+m_dir*length, out ground, m_waterMask))
			{
				float addition = (length - ground.distance); // distance enering water
				// length += addition;	// We double it
				m_currentLength = Mathf.Lerp( m_currentLength, m_length + addition, Time.deltaTime * 2);
			}
			else
			{
				m_currentLength = Mathf.Lerp( m_currentLength, m_length, Time.deltaTime * 2);
			}
		}

		if (Physics.Linecast( m_mouthTransform.position, m_mouthTransform.position+m_dir*m_currentLength, out ground, m_groundMask)){
			p2 = ground.point;
			m_actualLength = ground.distance;
		}else{
			p2 =  m_mouthTransform.position+m_dir*m_currentLength;
			m_actualLength = m_currentLength;
		}

		m_particleEnd.transform.position = p2;

		for(int i=0;i<m_rays.Length;i++)
			m_rays[i].Draw(p1,p2);

		// Look entities to damage!
		Entity[] preys = EntityManager.instance.GetEntitiesIn((Vector2)m_mouthTransform.position, (Vector2)m_dir, m_maxAmplitude, m_actualLength);
		for (int i = 0; i < preys.Length; i++) 
		{
			if (preys[i].IsBurnable(m_tier) || m_type == Type.Super) {
				AI.Machine machine =  preys[i].GetComponent<AI.Machine>();
				if (machine != null) {					
					machine.Burn(transform);					
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
	}



	override protected void BeginFury( Type _type ) 
	{
		base.BeginFury( _type );
		m_particleStart.transform.position = m_mouthTransform.position;
		m_dir = m_mouthTransform.position - m_headTransform.position;
		m_dir.z = 0f;
		m_dir.Normalize();
		m_particleEnd.transform.position = m_mouthTransform.position+m_dir*m_length;
		
		m_particleStart.gameObject.SetActive(true);
		m_particleEnd.gameObject.SetActive(true);

		for(int i=0;i<m_rays.Length;i++)
			m_rays[i].Hide ();
	}

	override protected void EndFury() 
	{
		base.EndFury();
		m_particleStart.gameObject.SetActive(false);
		m_particleEnd.gameObject.SetActive(false);
		for(int i=0;i<m_rays.Length;i++)
			m_rays[i].Hide ();

	}


	void OnDrawGizmos() {
		if (m_isFuryOn) 
		{
			Gizmos.color = Color.magenta;
			Gizmos.DrawLine( m_mouthTransform.position, m_mouthTransform.position + m_dir * m_actualLength );
		}
	}

	override public bool IsInsideArea(Vector2 _point) { 
	
		if (m_isFuryOn) 
		{
			float halfAmplitude = m_maxAmplitude/2.0f;
			float angle = Mathf.Atan2( m_dir.y, m_dir.x);

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
