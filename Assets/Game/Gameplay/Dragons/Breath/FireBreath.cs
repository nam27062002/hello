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

	public string m_flameParticle = "Flame";
	public string m_flameUpParticle = "FlameUp";
	public string m_superFlameParticle = "Flame";
	public string m_superFlameUpParticle = "FlameUp";
	public string m_flameLight = "PF_FireLight";

	float m_timeToNextLoopAudio = 0;
	AudioSource m_lastAudioSource;

	override protected void ExtendedStart() {

		PoolManager.CreatePool((GameObject)Resources.Load("Particles/" + m_flameParticle), m_maxParticles, false);
		PoolManager.CreatePool((GameObject)Resources.Load("Particles/" + m_flameUpParticle), m_maxParticles, false);
		PoolManager.CreatePool((GameObject)Resources.Load("Particles/" + m_superFlameParticle), m_maxParticles, false);
		PoolManager.CreatePool((GameObject)Resources.Load("Particles/" + m_superFlameUpParticle), m_maxParticles, false);
		PoolManager.CreatePool((GameObject)Resources.Load("Particles/" + m_flameLight), 1, false);

		m_groundMask = 1 << LayerMask.NameToLayer("Ground") | 1 << LayerMask.NameToLayer("Water");
		m_noPlayerMask = ~(1 << LayerMask.NameToLayer("Player"));

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

	override public bool IsInsideArea(Vector2 _point) { 
	
		if (m_isFuryOn) {
			if (m_bounds2D.Contains(_point)) {
				return IsInsideTriangle( _point );
			}
		}

		return false; 
	}

	override public bool Overlaps( CircleArea2D _circle)
	{
		if (m_isFuryOn) 
		{
			if (_circle.Overlaps( m_bounds2D )) 
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

		for (int i = 0; i < m_particleSpawn; i++) {
			
			GameObject obj = null;
			switch( m_type )
			{
				case Type.Standard:
				{
					obj = PoolManager.GetInstance(m_flameParticle);
				}break;
				case Type.Super:
				{
					obj = PoolManager.GetInstance(m_superFlameParticle);
				}break;
			}
			
			if (obj != null) {
				FlameParticle particle = obj.GetComponent<FlameParticle>();
				particle.lifeTime = m_lifeTime;
				particle.finalScale = m_finalScale;
				particle.Activate(m_mouthTransform, m_direction * m_actualLength, Random.Range(0.75f, 1.25f), m_sizeCurve);
			}
		}


		for (int i = 0; i < m_particleSpawn / 2; i++) 
		{
			GameObject obj = null;
			switch( m_type )
			{
				case Type.Standard:
				{
					obj = PoolManager.GetInstance(m_flameUpParticle);
				}break;
				case Type.Super:
				{
					obj = PoolManager.GetInstance(m_superFlameUpParticle);
				}break;
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
				particle.Activate( scale, scale * 1.25f, (delta + 0.1f), scale * distanceMultiplier, m_mouthTransform.position + (Vector3)m_direction * correctedPos, m_mouthTransform);
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
		Entity[] preys = EntityManager.instance.GetEntitiesInRange2D(m_sphCenter, m_sphRadius);
		for (int i = 0; i < preys.Length; i++) 
		{
			InflammableBehaviour entity =  preys[i].GetComponent<InflammableBehaviour>();
			if (entity != null) 
			{
				Entity prey = preys[i];
				if ((prey.circleArea != null && Overlaps(prey.circleArea)) || IsInsideArea(entity.transform.position)) 
				{
					// Check if I can burn it
					if (CanBurn( entity ) || m_type == Type.Super)
					{
						entity.Burn(damage * Time.deltaTime, transform);
					}
					else
					{
						// Show message saying I cannot burn it
						Messenger.Broadcast<DragonTier>(GameEvents.BIGGER_DRAGON_NEEDED, DragonTier.COUNT);
					}
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
