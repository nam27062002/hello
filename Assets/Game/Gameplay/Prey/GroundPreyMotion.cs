using UnityEngine;
using System.Collections;

/// <summary>
/// Prey motion. Movement and animation control layer.
/// </summary>
[DisallowMultipleComponent]
public class GroundPreyMotion : PreyMotion {

	private Vector2 m_velocityProject;

	protected override void AvoidCollisions() {}

	protected override void UpdateVelocity(bool insidePowerUp) {
		if (!m_burning)
		{
			m_steering = Vector2.ClampMagnitude(m_steering, m_steerForce);
			m_steering = m_steering / m_mass;

			float targetSpeed = m_currentMaxSpeed;
			if ( insidePowerUp )
				targetSpeed = m_currentMaxSpeed * 0.5f;

			m_velocity = Vector2.ClampMagnitude(m_velocity + m_steering, Mathf.Lerp(m_currentSpeed, targetSpeed, Time.deltaTime * 2));

			RaycastHit sensorA;
			RaycastHit sensorB;
			CheckGround(out sensorA, out sensorB);
			if (m_velocity.x < 0) 	m_direction = (sensorA.point - sensorB.point).normalized;
			else 					m_direction = (sensorB.point - sensorA.point).normalized;
			m_orientation.SetDirection(m_direction);

			m_currentSpeed = m_velocity.magnitude;
			m_velocityProject = Vector3.Project(m_velocity, m_direction);
			m_velocityProject = m_velocityProject.normalized * m_currentSpeed;
		}
		else
		{
			m_velocityProject = Vector2.zero;
		}

		Debug.DrawLine(m_position, m_position + m_velocityProject, Color.white);
	}

	protected override void UpdatePosition( float delta ) {
		m_lastPosition = m_position;
		m_position = m_position + (m_velocityProject * delta);
	}

	private bool CheckGround(out RaycastHit _leftHit, out RaycastHit _rightHit) {
		Vector3 distance = Vector3.down * 15f;
		bool hit_L = false;
		bool hit_R = false;

		Vector3 leftSensor  = m_groundSensor.position;
		Vector3 rightSensor = leftSensor + Vector3.right * 2f;

		hit_L = Physics.Linecast(leftSensor, leftSensor + distance, out _leftHit, m_groundMask);
		hit_R = Physics.Linecast(rightSensor, rightSensor + distance, out _rightHit, m_groundMask);

		return (hit_L && hit_R);
	}
}
