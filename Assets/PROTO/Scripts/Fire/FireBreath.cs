using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class FireBreath : DragonBreathBehaviour {

	[SerializeField]private AnimationCurve m_sizeCurve = AnimationCurve.Linear(0, 0, 1, 3f);	// Will be used by the inspector to easily setup the values for each level

	public int spawn = 2;

    const int maxFireParticles = 256;
	GameObject[] fire = new GameObject[maxFireParticles];



	Transform mouthPosition;
	Transform headPosition;


	Vector3 direction;
	Vector3 directionP;
	float magnitude;


	Vector3 m_p0;
	Vector3 m_p1;
	Vector3 m_p2;

	float m_area;


	override protected void ExtendedStart() {
		GameObject instancesObj = GameObject.Find ("Instances");
		if(instancesObj == null) {
			instancesObj = new GameObject("Instances");
		}
		Transform instances = GameObject.Find ("Instances").transform;

		Object firePrefab;
		firePrefab = Resources.Load("PROTO/Flame");

		for(int i=0;i<maxFireParticles;i++){
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

	override public bool IsInsideArea(Vector3 _point) { 
	
		if (m_isFuryOn) {
			float sign = m_area < 0 ? -1 : 1;
			float s = (m_p0.y * m_p2.x - m_p0.x * m_p2.y + (m_p2.y - m_p0.y) * _point.x + (m_p0.x - m_p2.x) * _point.y) * sign;
			float t = (m_p0.x * m_p1.y - m_p0.y * m_p1.x + (m_p0.y - m_p1.y) * _point.x + (m_p1.x - m_p0.x) * _point.y) * sign;
			
			return s > 0 && t > 0 && (s + t) < 2 * m_area * sign;
		}

		return false; 
	}

	override protected void Fire(){


		direction = mouthPosition.position - headPosition.position;
		direction.z = 0f;
		direction.Normalize();
		magnitude = m_length;

		directionP = new Vector3(direction.y, -direction.x, 0);
	
		int count = 0;
		foreach(GameObject fireObj in fire){
			if (!fireObj.activeInHierarchy){
				fireObj.GetComponent<FlameParticle>().Activate(mouthPosition, direction * m_length, Random.Range(0.75f, 1.25f), m_sizeCurve);
			
				count++;

				if (count > spawn)
					break;
			}
		}
	
		// Pre-Calculate Triangle
		m_p0 = mouthPosition.position;
		m_p1 = m_p0 + direction * m_length - directionP * m_sizeCurve.Evaluate(1) * 0.5f;
		m_p2 = m_p0 + direction * m_length + directionP * m_sizeCurve.Evaluate(1) * 0.5f;
		m_area = (-m_p1.y * m_p2.x + m_p0.y * (-m_p1.x + m_p2.x) + m_p0.x * (m_p1.y - m_p2.y) + m_p1.x * m_p2.y) * 0.5f;


		Debug.DrawLine(m_p0, m_p1, Color.white);
		Debug.DrawLine(m_p1, m_p2, Color.white);
		Debug.DrawLine(m_p2, m_p0, Color.white);
	}
}
