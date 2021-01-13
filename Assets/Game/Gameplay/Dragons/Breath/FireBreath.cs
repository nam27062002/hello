using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class FireBreath : DragonBreathBehaviour {

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

	[SerializeField] private int m_particleSpawn = 2;
	[SerializeField] private int m_maxParticles = 75;

	[Header("Particle")]
	[SerializeField] private float m_lifeTime = 5f;
	[SerializeField] private Range m_finalScale = new Range(0.75f, 1.25f);

	private int m_groundMask;
	private int m_noPlayerMask;

	private Transform m_fireDummyTransform;
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

	public string m_flameParticle = "Flame";
	public string m_flameUpParticle = "FlameUp";
	public string m_superFlameParticle = "Flame";
	public string m_superFlameUpParticle = "FlameUp";
	public string m_flameLight = "PF_FireLight";

	private PoolHandler m_flamePoolHandler;
	private PoolHandler m_flameUpPoolHandler;
	private PoolHandler m_superFlamePoolHandler;
	private PoolHandler m_superFlameUpPoolHandler;
	private PoolHandler m_flameLightPoolHandler;


	private Entity[] m_checkEntities = new Entity[50];
	private int m_numCheckEntities = 0;

	private Cage[] m_checkCages = new Cage[50];
	private int m_numCheckCages = 0;



	override protected void ExtendedStart() {

		m_flamePoolHandler 			= PoolManager.RequestPool(m_flameParticle, m_maxParticles);
		m_flameUpPoolHandler 		= PoolManager.RequestPool(m_flameUpParticle, m_maxParticles);
		m_superFlamePoolHandler 	= PoolManager.RequestPool(m_superFlameParticle, m_maxParticles);
		m_superFlameUpPoolHandler 	= PoolManager.RequestPool(m_superFlameUpParticle, m_maxParticles);
		m_flameLightPoolHandler 	= PoolManager.RequestPool(m_flameLight, 1);

        m_groundMask = GameConstants.Layers.GROUND_WATER_FIREBLOCK;
        m_noPlayerMask = ~GameConstants.Layers.PLAYER;


        m_fireDummyTransform = transform.FindTransformRecursive("Fire_Dummy");
		m_headTransform = GetComponent<DragonMotion>().head;

		m_length = m_dragon.data.def.GetAsFloat("furyBaseLength");
		m_length *= transform.localScale.x;

		m_actualLength = m_length;

		m_sphCenter = m_fireDummyTransform.position;
		m_sphRadius = 0;

		m_direction = Vector2.zero;
		m_directionP = Vector2.zero;

		m_frame = 0;

		m_light = null;
	}

	override public bool IsInsideArea(Vector2 _point) { 
	
		if (IsFuryOn()) {
			if (m_bounds2D.Contains(_point)) {
				return IsInsideTriangle( _point );
			}
		}

		return false; 
	}

	override public bool Overlaps(CircleAreaBounds _circle)
	{
		if (IsFuryOn()) 
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
		m_light = m_flameLightPoolHandler.GetInstance();
		m_light.transform.position = m_fireDummyTransform.position;
		m_light.transform.localScale = new Vector3(m_actualLength * 1.25f, m_sizeCurve.Evaluate(1) * transform.localScale.x * 1.75f, 1f);
	}

	override protected void EndFury( bool increase_mega_fury = true ) 
	{
		base.EndFury( increase_mega_fury );
		m_light.SetActive(false);
		m_flameLightPoolHandler.ReturnInstance(m_light);
		m_light = null;
	}

	override protected void Breath(){
		m_direction = m_fireDummyTransform.position - m_headTransform.position;
		m_direction.Normalize();
		m_directionP.Set(m_direction.y, -m_direction.x);

		float length = m_length;
		if ( m_type == Type.Mega )
			length = m_length * m_superFuryLengthMultiplier;

		Vector3 flamesUpDir = Vector3.up;
		bool hitingWater = false;
		if (m_frame == 0) {
			// Raycast to ground
			RaycastHit ground;				
			if (Physics.Linecast(m_fireDummyTransform.position, m_fireDummyTransform.position + (Vector3)m_direction * length, out ground, m_groundMask)) 
			{
				m_actualLength = ground.distance;
				if ( ground.collider.CompareTag("Water") )
					hitingWater = true;
			} 
			else 
			{				
				m_actualLength = length;
			}

			if (Physics.Linecast(m_fireDummyTransform.position, m_fireDummyTransform.position + (Vector3)m_direction * length, out ground, m_noPlayerMask)) 
			{
				flamesUpDir = Vector3.Reflect( m_direction, ground.normal);
				flamesUpDir.Normalize();

			}
		}

		{
			// Pre-Calculate Triangle: wider bounding triangle to make burning easier
			m_triP0 = m_fireDummyTransform.position;
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

		for (int i = 0; i < m_particleSpawn; i++) {
			
			GameObject obj = null;
			switch( m_type )
			{
				case Type.Standard: {
						obj = m_flamePoolHandler.GetInstance();
				}	break;
				case Type.Mega: {
						obj = m_superFlamePoolHandler.GetInstance();
				}	break;
			}
			
			if (obj != null) {
				FlameParticle particle = obj.GetComponent<FlameParticle>();
				particle.lifeTime = m_lifeTime;
				particle.finalScale = m_finalScale;
				particle.Activate(m_fireDummyTransform, m_direction * m_actualLength, Random.Range(0.75f, 1.25f), m_sizeCurve);
			}
		}


		for (int i = 0; i < m_particleSpawn / 2; i++) 
		{
			GameObject obj = null;
			switch( m_type )
			{
				case Type.Standard: {
						obj = m_flameUpPoolHandler.GetInstance();
				}	break;
				case Type.Mega: {
						obj = m_superFlameUpPoolHandler.GetInstance();
				}	break;
			}

			if (obj != null) {
				FlameUp particle = obj.GetComponent<FlameUp>();
				float pos = Random.Range( length / 5.0f, length);
				float delta = pos / m_length;
				float scale = m_sizeCurve.Evaluate( pos / length ) * transform.localScale.x;
				float correctedPos = pos;
				float distanceMultiplier = 1.5f;
				if ( pos > m_actualLength )
				{
					correctedPos = m_actualLength;
					distanceMultiplier = 3;
				}
				particle.m_moveDir = flamesUpDir;
				particle.Activate( scale, scale * 1.25f, (delta + 0.1f), scale * distanceMultiplier, m_fireDummyTransform.position + (Vector3)m_direction * correctedPos, m_fireDummyTransform);
			}
		}

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
				if (prey.IsBurnable(m_tier) || m_type == Type.Mega) {
					AI.IMachine machine =  m_checkEntities[i].machine;
					if (machine != null) {
                        
                        KillType killType;

                        // Special dragons deal different type of damage whith their breath
                        switch (DragonManager.CurrentDragon.sku)
                        {
                            case "dragon_ice":
                                killType = KillType.FROZEN;
                                break;

                            case "dragon_electric":
                                killType = KillType.ELECTRIFIED;
                                break;

                            default:
                                killType = KillType.BURNT;
                                break;
                        }

						machine.Burn(transform, IEntity.Type.PLAYER, killType);
					}
				} else {
					// Show message saying I cannot burn it
					Messenger.Broadcast<DragonTier, string>(MessengerEvents.BIGGER_DRAGON_NEEDED, DragonTier.COUNT, prey.sku);
				}
			}
		}

		// pick cages 
		m_numCheckCages = EntityManager.instance.GetCagesInRange2DNonAlloc(m_sphCenter, m_sphRadius, m_checkCages);
		for (int i = 0; i < m_numCheckCages; i++) 
		{
			Cage cage = m_checkCages[i];
			if ((cage.circleArea != null && Overlaps((CircleAreaBounds)cage.circleArea.bounds)) || IsInsideArea(cage.transform.position)) 
			{
				cage.behaviour.Break();
			}
		}
	}

	void OnDrawGizmos() {
		if (IsFuryOn()) {
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
					Gizmos.DrawLine(m_fireDummyTransform.position, m_fireDummyTransform.position + (Vector3)m_direction * m_length * 2);
				}break;
				case Type.Mega:
				{
					Gizmos.DrawLine(m_fireDummyTransform.position, m_fireDummyTransform.position + (Vector3)m_direction * m_length);
				}break;
			}
		}
	}

	void OnTriggerEnter(Collider _other)
	{
		if ( _other.CompareTag("Water") )
		{
			if ( IsFuryOn() )
			{
				m_isFuryPaused = true;
				m_animator.SetBool( GameConstants.Animator.BREATH , false);
			}

		}
	}

	void OnTriggerExit(Collider _other)
	{
		if ( _other.CompareTag("Water") )
		{
			m_isFuryPaused = false;
		}
	}
}
