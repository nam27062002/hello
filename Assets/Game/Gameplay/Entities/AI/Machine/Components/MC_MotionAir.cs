﻿using UnityEngine;
using System;

namespace AI {
	[Serializable]
	public sealed class MC_MotionAir : MC_Motion {
		
		//--------------------------------------------------
		[SeparatorAttribute("Orientation")]
		[SerializeField] private bool m_dragonStyleRotation = false;
		[SerializeField] private bool m_faceDirection = true;
		[SerializeField] private bool m_rollRotation = false;
		[SerializeField] private float m_rollAngle = 35f;

		[SeparatorAttribute]
		[SerializeField] private bool m_limitHorizontalRotation = false;
		[SerializeField] private float m_faceLeftAngle = -90f;
		[SerializeField] private float m_faceRightAngle = 90f;

		[SeparatorAttribute]
		[SerializeField] private bool m_limitVerticalRotation = false;
		[SerializeField] private float m_faceUpAngle = 320f;
		[SerializeField] private float m_faceDownAngle = 40f;


		//--------------------------------------------------
		public bool faceDirection { get { return m_faceDirection; } set { m_faceDirection = value; } }


		//--------------------------------------------------
		protected override void ExtendedUpdate() {
			if (!m_faceDirection || m_pilot.IsActionPressed(Pilot.Action.Stop)) {
				m_direction = (m_pilot.direction.x >= 0)? Vector3.right : Vector3.left;
			} else {
				m_direction = m_velocity.normalized;//m_pilot.direction;
			}
		}

		protected override void ExtendedFixedUpdate() {
			if (m_mass != 1f) {
				Vector3 impulse = (m_pilot.impulse - m_velocity);
				impulse /= m_mass;
				m_velocity = Vector3.ClampMagnitude(m_velocity + impulse, m_pilot.speed);
			} else {
				m_velocity = m_pilot.impulse;
			}

			m_rbody.velocity = m_velocity + m_externalVelocity;
		}

		protected override void UpdateOrientation() {
			m_targetRotation = Quaternion.LookRotation(m_direction + Vector3.back * 0.1f, m_upVector);

			if (!m_pilot.IsActionPressed(Pilot.Action.Stop)) {
				if (m_rollRotation) {
					float angle = Vector3.Angle(Vector3.right, m_direction);

					if (angle < 10f || angle > 170f) {
						angle = 0f;
					} else {
						if (angle >= 90f) 
							angle = 180f - angle;

						angle = ((angle - 10f) / (90f - 10f)) * (m_rollAngle - 0f) + 0f;
					}

					m_targetRotation = Quaternion.AngleAxis(angle, m_direction) * m_targetRotation;
				}

				if (m_dragonStyleRotation) {
					float angle = m_direction.ToAngleDegrees();
					float roll = angle;
					float pitch = angle;
					float yaw = 0;

					Quaternion qRoll = Quaternion.Euler(0f, 0f, roll);
					Quaternion qYaw = Quaternion.Euler(0f, yaw, 0f);
					Quaternion qPitch = Quaternion.Euler(pitch, 0f, 0f);

					m_targetRotation = qYaw * qRoll * qPitch;
					Vector3 eulerRot = m_targetRotation.eulerAngles;

					if (m_limitVerticalRotation) {						
						if (eulerRot.z > m_faceUpAngle && eulerRot.z < 180f - m_faceUpAngle) { // top cap
							eulerRot.z = m_faceUpAngle;
						} else if (eulerRot.z > 180f + m_faceDownAngle && eulerRot.z < 360f - m_faceDownAngle) { // bottom cap
							eulerRot.z = -m_faceDownAngle;
						}
					}
					m_targetRotation = Quaternion.Euler(eulerRot) * Quaternion.Euler(0f, 90f, 0f);
				}
			}

			if (m_limitHorizontalRotation) {
				if (m_direction.x < 0f) 	 m_targetRotation = Quaternion.AngleAxis(m_faceLeftAngle, m_upVector) * m_targetRotation; 
				else if (m_direction.x > 0f) m_targetRotation = Quaternion.AngleAxis(m_faceRightAngle, m_upVector) * m_targetRotation; 
			}

			if (m_limitVerticalRotation && !m_dragonStyleRotation) {
				Vector3 euler = m_targetRotation.eulerAngles;
				if (m_direction.y > 0.25f) 			euler.x = Mathf.Max(m_faceUpAngle, euler.x);
				else if (m_direction.y < -0.25f) 	euler.x = Mathf.Min(m_faceDownAngle, euler.x);
				m_targetRotation = Quaternion.Euler(euler);
			}
		}

		//--------------------------------------------------
		//--------------------------------------------------
		protected override void ExtendedAttach() {}
		protected override void ExtendedInit() {}

		protected override void ExtendedUpdateFreeFall() {}

		protected override void OnSetVelocity() {}

		public override void OnCollisionGroundEnter(Collision _collision) {}
		public override void OnCollisionGroundStay(Collision _collision) {}
		public override void OnCollisionGroundExit(Collision _collision) {}
	}
}