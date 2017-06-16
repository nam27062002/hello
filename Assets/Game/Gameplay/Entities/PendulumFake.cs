using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PendulumFake : MonoBehaviour {

	[SerializeField] private float m_ropeDistance;
	[SerializeField] private float m_speed = 2f;
	[SerializeField] private float m_mass = 10f;
	[SerializeField] private float m_angleEpsilon = 1f;

	private Vector3 m_basePosition;
	private Transform m_transform;
	private Vector3 m_impulse;

	// Use this for initialization
	void Start () {
		m_transform = transform;
	}

	void OnEnable() {
		m_transform = transform;
		m_basePosition = m_transform.position + Vector3.down * m_ropeDistance;
		m_transform.rotation = Quaternion.LookRotation(Vector3.down, Vector3.up);
		m_impulse = Vector3.zero;
	}

	// Update is called once per frame
	void FixedUpdate () {
		Vector3 newBasePosition = m_transform.position + Vector3.down * m_ropeDistance;

		Vector3 dir = (newBasePosition - m_basePosition);
		float dist = dir.magnitude;
		float mass = m_mass;
		if (dist > 1f) {
			mass /= dist * 2f;
		}
		dir.Normalize();

		dir *= m_speed;

		Vector3 impulse = (dir - m_impulse);
		impulse /= m_mass;
		m_impulse += impulse;

		m_impulse = Vector3.ClampMagnitude(m_impulse + impulse, m_speed);

		// rot
		Vector3 forward = m_basePosition - m_transform.position;
		forward.Normalize();
		float angle = Vector3.Angle(Vector3.down, forward);
		m_transform.rotation = Quaternion.LookRotation(forward, Vector3.up);

		// pos
		m_basePosition += m_impulse * Time.deltaTime;

	}

	void OnDrawGizmos() {
		Gizmos.DrawLine(transform.position + Vector3.down * m_ropeDistance + Vector3.left * 1f, transform.position + Vector3.down * m_ropeDistance + Vector3.right * 1f);
		Gizmos.DrawLine(transform.position, m_basePosition);
	}
}
