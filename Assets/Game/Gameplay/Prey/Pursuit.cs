using UnityEngine;
using System.Collections;

[DisallowMultipleComponent]
[RequireComponent(typeof(PreyBehaviour))]
public class Pursuit : Steering {


	public Vector2 GetTarget(Vector2 _target, Vector2 _velocity, float _maxSpeed) {
		
		float distance = (m_prey.position - _target).magnitude;
		float t = 2f * (distance / _maxSpeed); // amount of time in the future
		
		Vector2 futurePosition = _target + _velocity * t;
		
		return futurePosition;
	}
}
