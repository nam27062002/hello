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
	[SerializeField]private float m_dyingTime = 0.25f;
	[SerializeField]private float m_dyingSpeed = 3f;
	[SerializeField]private Range m_finalScale = new Range(0.75f, 1.25f);

	private int m_groundMask;

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
	private float m_sphRadiusSqr;

	private float m_area;

	private int m_frame;



	override protected void ExtendedStart() {

		PoolManager.CreatePool((GameObject)Resources.Load("Particles/Flame"), m_maxParticles, false);


		m_groundMask = 1 << LayerMask.NameToLayer("Ground");

		m_mouthTransform = transform.FindSubObjectTransform("fire");
		m_headTransform = transform.FindSubObjectTransform("head");

		m_actualLength = m_length;

		m_sphCenter = m_mouthTransform.position;
		m_sphRadius = 0;
		m_sphRadiusSqr = 0;

		m_frame = 0;
	}

	override public bool IsInsideArea(Vector2 _point) { 
	
		if (m_isFuryOn) {
			if (m_bounds2D.Contains(_point)) {
				float sign = m_area < 0 ? -1 : 1;
				float s = (m_triP0.y * m_triP2.x - m_triP0.x * m_triP2.y + (m_triP2.y - m_triP0.y) * _point.x + (m_triP0.x - m_triP2.x) * _point.y) * sign;
				float t = (m_triP0.x * m_triP1.y - m_triP0.y * m_triP1.x + (m_triP0.y - m_triP1.y) * _point.x + (m_triP1.x - m_triP0.x) * _point.y) * sign;
				
				return s > 0 && t > 0 && (s + t) < 2 * m_area * sign;
			}
		}

		return false; 
	}

	override protected void Fire(){

		if (m_frame == 0) {
			m_direction = m_mouthTransform.position - m_headTransform.position;
			m_direction.Normalize();
			m_directionP = new Vector3(m_direction.y, -m_direction.x, 0);

			// Raycast to ground
			RaycastHit ground;				
			if (Physics.Linecast(m_mouthTransform.position, m_mouthTransform.position + (Vector3)m_direction * m_length, out ground, m_groundMask)) {
				m_actualLength = ground.distance;
			} else {
				m_actualLength = m_length;
			}
					
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
			m_sphRadiusSqr = m_sphRadius * m_sphRadius;

			m_bounds2D.Set(m_sphCenter.x - m_sphRadius, m_sphCenter.y - m_sphRadius, m_sphRadius * 2f, m_sphRadius * 2f);
		}

		// Spawn particles
		int count = 0;
		for (int i = 0; i < m_particleSpawn; i++) {
			
			GameObject obj = PoolManager.GetInstance("Flame");
			
			if (obj != null) {
				FlameParticle particle = obj.GetComponent<FlameParticle>();
				particle.lifeTime = m_lifeTime;
				particle.dyingTime = m_dyingTime;
				particle.dyingSpeed = m_dyingSpeed;
				particle.finalScale = m_finalScale;
				particle.Activate(m_mouthTransform, m_direction * m_actualLength, Random.Range(0.75f, 1.25f), m_sizeCurve);
			}
		}

		m_frame = (m_frame + 1) % 4;
	}

	void OnDrawGizmos() {

		Gizmos.color = Color.magenta;

		Gizmos.DrawLine(m_triP0, m_triP1);
		Gizmos.DrawLine(m_triP1, m_triP2);
		Gizmos.DrawLine(m_triP2, m_triP0);

		Gizmos.DrawWireSphere(m_sphCenter, m_sphRadius);

		Gizmos.color = Color.green;
		Gizmos.DrawLine(m_mouthTransform.position, m_mouthTransform.position + (Vector3)m_direction * m_length);
	}
}
