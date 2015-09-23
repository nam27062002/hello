using UnityEngine;
using System.Collections;

[DisallowMultipleComponent]
[RequireComponent(typeof(Seek))]
[AddComponentMenu("Behaviour/Prey/Pursuit")]
public class Pursuit : Steering {
	
	private Seek m_seek;
	
	void Start() {
		m_seek = GetComponent<Seek>();
	}

	public Vector2 GetForce(Vector2 _target, Vector2 _velocity, float _maxSpeed) {
		
		float distance = (m_prey.position - _target).magnitude;
		float t = 2f * (distance / _maxSpeed); // amount of time in the future
		
		Vector2 futurePosition = _target + _velocity * t;
		
		return m_seek.GetForce(futurePosition);
	}
}
