using UnityEngine;
using System.Collections;

[DisallowMultipleComponent]
[AddComponentMenu("Behaviour/Prey/Flock")]
public class Flock : Steering {
	
	[SerializeField] private Range m_avoidRadiusRange;


	
	private FlockController m_flock; // turn into flock controller
	private float m_avoidRadius;



	void OnEnable() {

		m_avoidRadius = m_avoidRadiusRange.GetRandom();
	}

	void OnDisable() {
		AttachFlock(null);
	}

	public void AttachFlock(FlockController _flock) {
		
		if (m_flock != null) {
			m_flock.Remove(gameObject);
		}
		
		m_flock = _flock;
		
		if (m_flock != null) {
			m_flock.Add(gameObject);
		}
	}

	public bool HasController() {
		return m_flock != null;
	}

	public Vector2 GetTarget() {
	
		return m_flock.target;
	}

	public Vector2 GetForce() {
		
		Vector2 avoid = Vector2.zero;
		Vector2 direction = Vector2.zero;
		for (int i = 0; i < m_flock.entities.Length; i++) {
			
			GameObject entity = m_flock.entities[i];
			
			if (entity != null && entity != gameObject) {
				direction = m_prey.position - (Vector2)entity.transform.position;
				float distance = direction.magnitude;
				
				if (distance < m_avoidRadius) {
					avoid += direction.normalized * (m_avoidRadius - distance);
				}
			}
		}
		
		Debug.DrawLine(m_prey.position, m_prey.position + avoid);
		
		return avoid;
	}
}
