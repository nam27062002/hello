#define FIRETEST


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

    public GameObject m_dragonFlame = null;


//    public const bool FIRETEST = true;


#if FIRETEST
    private FireBreathDynamic dragonFlameInstance = null;
#else
    private DragonBreath2 dragonFlameInstance = null;
#endif

    override protected void ExtendedStart() {

        DragonMotion dragonMotion = GetComponent<DragonMotion>();

        GameObject tempFire = Instantiate<GameObject>(m_dragonFlame);

        Transform mouth = transform.FindTransformRecursive("mouth");
        m_mouthTransform = mouth; // dragonMotion.tongue;

        tempFire.transform.SetParent(mouth, true);

        tempFire.transform.localPosition = Vector3.zero;

#if FIRETEST
        tempFire.transform.localRotation = Quaternion.Euler(new Vector3(0.0f, 0.0f, 180.0f));
        dragonFlameInstance = tempFire.GetComponent<FireBreathDynamic>();
#else
        tempFire.transform.localRotation = Quaternion.Euler(new Vector3(0.0f, 0.0f, -90.0f));
        dragonFlameInstance = tempFire.GetComponent<DragonBreath2>();
#endif

        dragonFlameInstance.EnableFlame(false);

        m_groundMask = LayerMask.GetMask("Ground", "Water", "GroundVisible");
		m_noPlayerMask = ~LayerMask.GetMask("Player");

        float furyBaseLength = m_dragon.data.def.GetAsFloat("furyBaseLength");
        m_length = furyBaseLength;
        m_length *= transform.localScale.x;
      	
        dragonFlameInstance.setEffectScale(furyBaseLength, transform.localScale.x);
        m_length *= 2.0f;
        m_actualLength = m_length;

        m_sphCenter = m_mouthTransform.position;
		m_sphRadius = 0;

		m_direction = Vector2.zero;
		m_directionP = Vector2.zero;

		m_frame = 0;

//		m_light = null;
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
        dragonFlameInstance.EnableFlame(true);
    }

    override protected void EndFury() 
	{
		base.EndFury();
        dragonFlameInstance.EnableFlame(false);
    }

    override protected void Breath(){
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
                PauseFury();
			}

		}
	}

	void OnTriggerExit(Collider _other)
	{
		if ( _other.tag == "Water" )
		{
            if (m_isFuryPaused)
            {
                ResumeFury();
            }
            m_isFuryPaused = false;
		}
	}

    public override void PauseFury()
    {
        base.PauseFury();
        dragonFlameInstance.EnableFlame(false);
    }

    public override void ResumeFury()
    {
        base.ResumeFury();
        dragonFlameInstance.EnableFlame(true);
    }

}
