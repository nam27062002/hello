using UnityEngine;
using System.Collections;

[DisallowMultipleComponent]
[RequireComponent(typeof(Flee))]
[AddComponentMenu("Behaviour/Prey/Evade")]
public class Evade : Steering {

	private Flee m_flee;

	void Start() {
		m_flee = GetComponent<Flee>();
	}

	public Vector2 GetForce(Vector2 _target, Vector2 _velocity, float _maxSpeed) {
		
		float distance = (m_prey.position - _target).magnitude;
		float t = 2f * (distance / _maxSpeed); // amount of time in the future
		
		Vector2 futurePosition = _target + _velocity * t;
		
		return m_flee.GetForce(futurePosition);
	}
}