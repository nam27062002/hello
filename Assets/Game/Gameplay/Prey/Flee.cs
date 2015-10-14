using UnityEngine;

[DisallowMultipleComponent]
[AddComponentMenu("Behaviour/Prey/Flee")]
public class Flee : Steering {

	public Vector2 GetForce(Vector2 _from) {
		
		Vector2 desiredVelocity = m_prey.position - _from;
		desiredVelocity = (desiredVelocity - m_prey.velocity);
		
		Debug.DrawLine(m_prey.position, m_prey.position + desiredVelocity, Color.red);
		
		return desiredVelocity;
	}
}
