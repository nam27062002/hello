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

	GameObject[] fire;


	Transform mouthPosition;
	Transform headPosition;


	Vector2 direction;
	Vector2 directionP;


	Vector2 m_triP0;
	Vector2 m_triP1;
	Vector2 m_triP2;

	Vector2 m_sphCenter;
	float m_sphRadius;
	float m_sphRadiusSqr;

	float m_area;




	override protected void ExtendedStart() {
		GameObject instancesObj = GameObject.Find ("Instances");
		if(instancesObj == null) {
			instancesObj = new GameObject("Instances");
		}
		Transform instances = GameObject.Find ("Instances").transform;

		Object firePrefab;
		firePrefab = Resources.Load("PROTO/Flame");

		fire = new GameObject[m_maxParticles];
		for(int i=0;i<m_maxParticles;i++){
			GameObject fireObj = (GameObject)Object.Instantiate(firePrefab);
			fireObj.transform.parent = instances;
			fireObj.transform.localPosition = Vector3.zero;
			fireObj.SetActive(false);
			fire[i] = fireObj;
		}

		//timer = 0f;

		mouthPosition = transform.FindSubObjectTransform("fire");
		headPosition = transform.FindSubObjectTransform("head");
	}

	override public bool IsInsideArea(Vector2 _point) { 
	
		float d = (m_sphCenter - _point).sqrMagnitude;

		if (d < m_sphRadiusSqr) {
			if (m_isFuryOn) {
				float sign = m_area < 0 ? -1 : 1;
				float s = (m_triP0.y * m_triP2.x - m_triP0.x * m_triP2.y + (m_triP2.y - m_triP0.y) * _point.x + (m_triP0.x - m_triP2.x) * _point.y) * sign;
				float t = (m_triP0.x * m_triP1.y - m_triP0.y * m_triP1.x + (m_triP0.y - m_triP1.y) * _point.x + (m_triP1.x - m_triP0.x) * _point.y) * sign;
				
				return s > 0 && t > 0 && (s + t) < 2 * m_area * sign;
			}
		}

		return false; 
	}

	override protected void Fire(){


		direction = mouthPosition.position - headPosition.position;
		direction.Normalize();

		directionP = new Vector3(direction.y, -direction.x, 0);
	
		int count = 0;
		foreach(GameObject fireObj in fire){
			if (!fireObj.activeInHierarchy){
				FlameParticle particle = fireObj.GetComponent<FlameParticle>();

				particle.lifeTime = m_lifeTime;
				particle.dyingTime = m_dyingTime;
				particle.dyingSpeed = m_dyingSpeed;
				particle.finalScale = m_finalScale;
				particle.Activate(mouthPosition, direction * m_length, Random.Range(0.75f, 1.25f), m_sizeCurve);
			
				count++;

				if (count > m_particleSpawn)
					break;
			}
		}
	
		// Pre-Calculate Triangle: wider bounding triangle to make burning easier
		m_triP0 = mouthPosition.position;
		m_triP1 = m_triP0 + direction * (m_length + m_finalScale.max) - directionP * m_sizeCurve.Evaluate(1) * m_finalScale.max * 0.5f;
		m_triP2 = m_triP0 + direction * (m_length + m_finalScale.max) + directionP * m_sizeCurve.Evaluate(1) * m_finalScale.max * 0.5f;
		m_area = (-m_triP1.y * m_triP2.x + m_triP0.y * (-m_triP1.x + m_triP2.x) + m_triP0.x * (m_triP1.y - m_triP2.y) + m_triP1.x * m_triP2.y) * 0.5f;

		m_sphCenter = m_triP0 + direction * (m_length + m_finalScale.max) * 0.5f;
		m_sphRadius = (m_sphCenter - m_triP1).magnitude;
		m_sphRadiusSqr = m_sphRadius * m_sphRadius;
	}

	void OnDrawGizmos() {
		Gizmos.color = Color.white;

		Gizmos.DrawLine(m_triP0, m_triP1);
		Gizmos.DrawLine(m_triP1, m_triP2);
		Gizmos.DrawLine(m_triP2, m_triP0);

		Gizmos.DrawWireSphere(m_sphCenter, m_sphRadius);
	}
}
