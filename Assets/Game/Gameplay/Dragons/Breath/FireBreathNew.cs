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

	private Vector2 m_directionP;

	private Vector2 m_triP0;
	private Vector2 m_triP1;
	private Vector2 m_triP2;

	private Vector2 m_sphCenter;
	private float m_sphRadius;

	private float m_area;

	private int m_frame;

	private Entity[] m_checkEntities = new Entity[50];
	private int m_numCheckEntities = 0;

    public GameObject m_dragonFlameStandard = null;
    public GameObject m_dragonFlameSuper = null;

    private FireBreathDynamic dragonFlameStandardInstance = null;
    private FireBreathDynamic dragonFlameSuperInstance = null;
    //    private FireBreathDynamic dragonFlameInstance = null;

    private bool m_insideWater = false;
    private bool m_waterMode = false;
    private float m_waterY = 0;



    override protected void ExtendedStart() {

        Transform cacheTransform = transform;
        Transform mouth = cacheTransform.FindTransformRecursive("mouth");
        m_mouthTransform = mouth;

        GameObject tempFire = Instantiate<GameObject>(m_dragonFlameStandard);
        Transform t = tempFire.transform;
        t.SetParent(mouth, true);
        t.localPosition = Vector3.zero;
        t.localRotation = Quaternion.Euler(new Vector3(0.0f, 0.0f, 180.0f));
        dragonFlameStandardInstance = tempFire.GetComponent<FireBreathDynamic>();

        tempFire = Instantiate<GameObject>(m_dragonFlameSuper);
        tempFire.transform.SetParent(mouth, true);
        tempFire.transform.localPosition = Vector3.zero;
        tempFire.transform.localRotation = Quaternion.Euler(new Vector3(0.0f, 0.0f, 180.0f));
        dragonFlameSuperInstance = tempFire.GetComponent<FireBreathDynamic>();

		m_groundMask = LayerMask.GetMask("Ground", "Water", "GroundVisible", "FireBlocker");
		m_noPlayerMask = ~LayerMask.GetMask("Player");

        m_actualLength = m_length;

        m_sphCenter = m_mouthTransform.position;
		m_sphRadius = 0;

		m_direction = Vector2.zero;
		m_directionP = Vector2.zero;

		m_frame = 0;

        dragonFlameStandardInstance.EnableFlame(false);
        dragonFlameStandardInstance.gameObject.SetActive(false);
        dragonFlameSuperInstance.EnableFlame(false);
        dragonFlameSuperInstance.gameObject.SetActive(false);

        //		m_light = null;
    }

    override public void RecalculateSize()
    {
    	if ( m_dragon )
    	{
			float furyBaseLength = m_dragon.data.def.GetAsFloat("furyBaseLength");
			m_length = furyBaseLength + furyBaseLength * m_lengthPowerUpMultiplier / 100.0f;
	        m_length *= transform.localScale.x;

			dragonFlameStandardInstance.setEffectScale(furyBaseLength / 2.0f, transform.localScale.x);
			dragonFlameSuperInstance.setEffectScale(furyBaseLength * m_superFuryLengthMultiplier / 2.0f, transform.localScale.x);
		}
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
        if (_type == Type.Standard)
        {
            dragonFlameStandardInstance.EnableFlame(true, m_insideWater);
        }
        else
        {
            dragonFlameSuperInstance.EnableFlame(true, m_insideWater);
        }
    }

    override protected void EndFury() 
	{
        if (m_type == Type.Standard)
        {
            dragonFlameStandardInstance.EnableFlame(false);
        }
        else
        {
            dragonFlameSuperInstance.EnableFlame(false);
        }
        base.EndFury();

    }

    override protected void Breath()
    {
        m_direction = -m_mouthTransform.right;
		m_direction.Normalize();
		m_directionP.Set(m_direction.y, -m_direction.x);

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
				if ( ground.collider.CompareTag("Water") )
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

//		float lerpT = 0.15f;

		//Vector3 pos = new Vector3(m_triP0.x, m_triP0.y, -8f);
//		m_light.transform.position = m_triP0; //Vector3.Lerp(m_light.transform.position, pos, 1f);

		float angle = Vector3.Angle(Vector3.right, m_direction);
		if (m_direction.y > 0) angle *= -1;
//		m_light.transform.localRotation = Quaternion.Lerp(m_light.transform.localRotation, Quaternion.AngleAxis(angle, Vector3.back), lerpT);

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
				if (prey.IsBurnable()) {
					if (prey.IsBurnable(m_tier) || m_type == Type.Super) {
						AI.IMachine machine =  m_checkEntities[i].machine;
						if (machine != null) {
							machine.Burn(transform);
						}
					} else {
						// Show message saying I cannot burn it
						Messenger.Broadcast<DragonTier, string>(GameEvents.BIGGER_DRAGON_NEEDED, DragonTier.COUNT, prey.sku);
					}
				}
			}
		}

		if ( m_insideWater )
		{
			if (m_mouthTransform.position.y > m_waterY)
			{
				if ( m_waterMode )
				{	
					ShowNormalMode();
					m_waterMode = false;
				}
			}
			else
			{
				if (!m_waterMode)
				{
					ShowWaterMode();
					m_waterMode = true;
				}
			}
		}

		base.Breath();
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
					Gizmos.DrawLine(m_mouthTransform.position, m_mouthTransform.position + (Vector3)m_direction * m_length);
				}break;
				case Type.Super:
				{
					Gizmos.DrawLine(m_mouthTransform.position, m_mouthTransform.position + (Vector3)m_direction * m_length * m_superFuryLengthMultiplier);
				}break;
			}
		}
	}

	void OnTriggerEnter(Collider _other)
	{
		if ( _other.CompareTag("Water") )
		{
			m_insideWater = true;
			if ( m_mouthTransform )
				m_waterY = m_mouthTransform.position.y;
			m_waterMode = true;
			ShowWaterMode();
		}
	}

	private void ShowWaterMode()
	{
		if ( m_isFuryOn )
		{
			// Change to water modes
			switch( m_type )
			{
				case Type.Standard:
				{
					dragonFlameStandardInstance.SwitchToWaterMode();
				}break;
				case Type.Super:
				{
					dragonFlameSuperInstance.SwitchToWaterMode();
				}break;
			}
		}
	}


	void OnTriggerExit(Collider _other)
	{
		if ( _other.CompareTag("Water") )
		{
			m_insideWater = false;
			m_waterMode = false;
			ShowNormalMode();
		}
	}

	private void ShowNormalMode()
	{
		if ( m_isFuryOn )
		{
			switch( m_type )
			{
				case Type.Standard:
				{
					dragonFlameStandardInstance.SwitchToNormalMode();
				}break;
				case Type.Super:
				{
					dragonFlameSuperInstance.SwitchToNormalMode();
				}break;
			}
		}
	}

    public override void PauseFury()
    {
        base.PauseFury();
        if (m_type == Type.Standard)
        {
            dragonFlameStandardInstance.EnableFlame(false);
        }
        else
        {
            dragonFlameSuperInstance.EnableFlame(false);
        }
    }

    public override void ResumeFury()
    {
        base.ResumeFury();
        if (m_type == Type.Standard)
        {
			dragonFlameStandardInstance.EnableFlame(true, m_insideWater);
        }
        else
        {
			dragonFlameSuperInstance.EnableFlame(true, m_insideWater);
        }
    }

}
