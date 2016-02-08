using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class FireBreath : DragonBreathBehaviour {

	[Header("Emitter")]
	[SerializeField]private float m_length = 6f;
	[SerializeField]private AnimationCurve m_sizeCurve = AnimationCurve.Linear(0, 0, 1, 3f);	// Will be used by the inspector to easily setup the values for each level
	[SerializeField]private int m_particleSpawn = 2;
	[SerializeField]private int m_maxParticles = 75;

	[Header("Particle")]
	[SerializeField]private float m_lifeTime = 5f;
	[SerializeField]private Range m_finalScale = new Range(0.75f, 1.25f);

	private int m_groundMask;
	private int m_noPlayerMask;

	private Transform m_mouthTransform;
	private Transform m_headTransform;

	private Vector2 m_direction;
	private Vector2 m_directionP;
	private float m_actualLength;

	private Vector2 m_triP0;
	private Vector2 m_triP1;
	private Vector2 m_triP2;

	private Vector2 m_sphCenter;
	private float m_sphRadius;

	private float m_area;

	private int m_frame;

	private GameObject m_light;
	private GameObject m_fireTip;
	private ParticleSystem m_fireTipParticle;

	override protected void ExtendedStart() {

		PoolManager.CreatePool((GameObject)Resources.Load("Particles/Flame"), m_maxParticles, false);
		PoolManager.CreatePool((GameObject)Resources.Load("Particles/FlameUp"), m_maxParticles, false);
		PoolManager.CreatePool((GameObject)Resources.Load("Particles/PF_FireLight"), 1, false);
		m_fireTip = Instantiate( Resources.Load("Particles/FireTip")) as GameObject;
		m_fireTipParticle = m_fireTip.GetComponent<ParticleSystem>();
		m_fireTipParticle.Stop();

		m_groundMask = 1 << LayerMask.NameToLayer("Ground");
		m_noPlayerMask = ~(1 << LayerMask.NameToLayer("Player"));

		m_mouthTransform = GetComponent<DragonMotion>().tongue;
		m_headTransform = GetComponent<DragonMotion>().head;

		m_actualLength = m_length;

		m_sphCenter = m_mouthTransform.position;
		m_sphRadius = 0;

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

	override protected void BeginBreath() 
	{
		base.BeginBreath();
		m_light = PoolManager.GetInstance("PF_FireLight");
		m_light.transform.position = m_mouthTransform.position;
		m_light.transform.localScale = new Vector3(m_actualLength * 1.25f, m_sizeCurve.Evaluate(1) * 1.75f, 1f);
	}

	override protected void EndBreath() 
	{
		base.EndBreath();
		m_light.SetActive(false);
		m_light = null;
	}

	override protected void Breath(){
		m_direction = m_mouthTransform.position - m_headTransform.position;
		m_direction.Normalize();
		m_directionP = new Vector3(m_direction.y, -m_direction.x, 0);

		Vector3 flamesUpDir = Vector3.up;
		if (m_frame == 0) {
			// Raycast to ground
			RaycastHit ground;				
			if (Physics.Linecast(m_mouthTransform.position, m_mouthTransform.position + (Vector3)m_direction * m_length, out ground, m_groundMask)) 
			{
				m_actualLength = ground.distance;

			} 
			else 
			{
				
				m_actualLength = m_length;
			}

			if (Physics.Linecast(m_mouthTransform.position, m_mouthTransform.position + (Vector3)m_direction * m_length, out ground, m_noPlayerMask)) 
			{
				flamesUpDir = Vector3.Reflect( m_direction, ground.normal);
				flamesUpDir.Normalize();
				/*
				m_fireTipParticle.Play();
				m_fireTip.transform.position = ground.point;
				// m_fireTip.transform.rotation = Quaternion.FromToRotation(Vector3.forward, ground.normal);
				m_fireTip.transform.rotation = Quaternion.FromToRotation(Vector3.forward, Vector3.Reflect( m_direction, ground.normal));

				// Check curve size
				ParticleSystem ps = m_fireTip.GetComponent<ParticleSystem>();
				ParticleSystem.ShapeModule shape = ps.shape;
				shape.box = (Vector3.right + Vector3.up) * m_sizeCurve.Evaluate( ground.distance / m_length) * 0.5f + Vector3.forward;
				// ps.shape = shape;
				*/
			}
			else
			{
				// m_fireTipParticle.Stop();
			}


		}
		{
			// Pre-Calculate Triangle: wider bounding triangle to make burning easier
			m_triP0 = m_mouthTransform.position;
			m_triP1 = m_triP0 + m_direction * m_actualLength - m_directionP * m_sizeCurve.Evaluate(1) * 0.5f;
			m_triP2 = m_triP0 + m_direction * m_actualLength + m_directionP * m_sizeCurve.Evaluate(1) * 0.5f;
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
			
			GameObject obj = PoolManager.GetInstance("Flame");
			
			if (obj != null) {
				FlameParticle particle = obj.GetComponent<FlameParticle>();
				particle.lifeTime = m_lifeTime;
				particle.finalScale = m_finalScale;
				particle.Activate(m_mouthTransform, m_direction * m_actualLength, Random.Range(0.75f, 1.25f), m_sizeCurve);
			}
		}


		for (int i = 0; i < m_particleSpawn / 2; i++) 
		{
			GameObject obj = PoolManager.GetInstance("FlameUp");
			
			if (obj != null) {
				FlameUp particle = obj.GetComponent<FlameUp>();
				float pos = Random.Range( m_length / 5.0f, m_length);
				float delta = pos / m_length;
				float scale = m_sizeCurve.Evaluate( pos / m_length );
				float correctedPos = pos;
				float distanceMultiplier = 1.5f;
				if ( pos > m_actualLength )
				{
					correctedPos = m_actualLength;
					distanceMultiplier = 3;

				}
				particle.m_moveDir = flamesUpDir;
				particle.Activate( scale, scale * 1.25f, (delta + 0.1f), scale * distanceMultiplier, m_mouthTransform.position + (Vector3)m_direction * correctedPos);
			}
		}


		float lerpT = 0.15f;

		//Vector3 pos = new Vector3(m_triP0.x, m_triP0.y, -8f);
		m_light.transform.position = m_triP0; //Vector3.Lerp(m_light.transform.position, pos, 1f);

		float angle = Vector3.Angle(Vector3.right, m_direction);
		if (m_direction.y > 0) angle *= -1;
		m_light.transform.localRotation = Quaternion.Lerp(m_light.transform.localRotation, Quaternion.AngleAxis(angle, Vector3.back), lerpT);

		m_frame = (m_frame + 1) % 4;
	}

	void OnDrawGizmos() {
		if (m_isFuryOn) {
			Gizmos.color = Color.magenta;

			Gizmos.DrawLine(m_triP0, m_triP1);
			Gizmos.DrawLine(m_triP1, m_triP2);
			Gizmos.DrawLine(m_triP2, m_triP0);

			Gizmos.DrawWireSphere(m_sphCenter, m_sphRadius);

			Gizmos.color = Color.green;
			Gizmos.DrawLine(m_mouthTransform.position, m_mouthTransform.position + (Vector3)m_direction * m_length);
		}
	}
}
