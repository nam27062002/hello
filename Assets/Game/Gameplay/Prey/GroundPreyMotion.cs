using UnityEngine;
using System.Collections;

/// <summary>
/// Prey motion. Movement and animation control layer.
/// </summary>
[DisallowMultipleComponent]
public class GroundPreyMotion : PreyMotion {

	private Vector2 m_velocityProject;

	protected override void UpdateVelocity() {
		m_steering = Vector2.ClampMagnitude(m_steering, m_steerForce);
		m_steering = m_steering / m_mass;

		m_velocity = Vector2.ClampMagnitude(m_velocity + m_steering, Mathf.Lerp(m_currentSpeed, m_currentMaxSpeed, 0.05f));

		RaycastHit sensorA;
		RaycastHit sensorB;
		CheckGround(out sensorA, out sensorB);
		if (m_velocity.x < 0) 	m_direction = (sensorA.point - sensorB.point).normalized;
		else 					m_direction = (sensorB.point - sensorA.point).normalized;
		m_orientation.SetDirection(m_direction);

		m_currentSpeed = m_velocity.magnitude;
		m_velocityProject = Vector3.Project(m_velocity, m_direction);
		m_velocityProject = m_velocityProject.normalized * m_currentSpeed;

		Debug.DrawLine(m_position, m_position + m_velocityProject, m_velocityColor);
	}

	protected override void UpdatePosition() {
		m_lastPosition = m_position;
		m_position = m_position + (m_velocityProject * Time.fixedDeltaTime);
	}

	private bool CheckGround(out RaycastHit _leftHit, out RaycastHit _rightHit) {
		Vector3 distance = Vector3.down * 15f;
		bool hit_L = false;
		bool hit_R = false;

		Vector3 leftSensor  = m_lastPosition + Vector2.up * 5f;
		Vector3 rightSensor = leftSensor + Vector3.right * 2f;

		hit_L = Physics.Linecast(leftSensor, leftSensor + distance, out _leftHit, m_groundMask);
		hit_R = Physics.Linecast(rightSensor, rightSensor + distance, out _rightHit, m_groundMask);

		return (hit_L && hit_R);
	}
}
