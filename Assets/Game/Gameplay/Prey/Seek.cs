using UnityEngine;
using System.Collections;

[DisallowMultipleComponent]
[RequireComponent(typeof(PreyBehaviour))]
public class Seek : Steering {

	[SerializeField] private float m_slowingRadius;


	public Vector2 GetForce(Vector2 _target) {
		
		Vector2 desiredVelocity = _target - m_prey.position;
		float distanceSqr = desiredVelocity.sqrMagnitude;
		float slowingRadiusSqr = m_slowingRadius * m_slowingRadius;
		
		desiredVelocity.Normalize();
		
		if (distanceSqr < slowingRadiusSqr) {
			desiredVelocity *= m_prey.maxSpeed * (distanceSqr / slowingRadiusSqr);
		} else { 
			desiredVelocity *= m_prey.maxSpeed;
		}
		
		desiredVelocity -= m_prey.velocity;
		
		Debug.DrawLine(m_prey.position, m_prey.position + desiredVelocity);
		
		return desiredVelocity;
	}
}
