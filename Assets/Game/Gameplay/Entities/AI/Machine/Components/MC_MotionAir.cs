using UnityEngine;
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
		public bool limitHorizontalRotation{
			get{ return m_limitHorizontalRotation; }
			set{ m_limitHorizontalRotation = value; }
		}
		[SerializeField] private float m_faceLeftAngle = -90f;
		[SerializeField] private float m_faceRightAngle = 90f;

		[SeparatorAttribute]
		[SerializeField] private bool m_limitVerticalRotation = false;
		public bool limitVerticalRotation{
			get{ return m_limitVerticalRotation; }
			set{ m_limitVerticalRotation = value; }
		}
		[SerializeField] private float m_faceUpAngle = 320f;
		[SerializeField] private float m_faceDownAngle = 40f;


		//--------------------------------------------------
		public bool faceDirection { get { return m_faceDirection; } set { m_faceDirection = value; } }


		//--------------------------------------------------
		protected override void ExtendedUpdate() {
			if (!m_faceDirection) {
				m_direction = (m_pilot.direction.x >= 0)? GameConstants.Vector3.right : GameConstants.Vector3.left;
			} else if (m_pilot.IsActionPressed(Pilot.Action.Stop | Pilot.Action.Attack)) {
				m_direction = m_pilot.direction;
			} else {
				m_direction = m_velocity.normalized;
			}
		}

		protected override void ExtendedFixedUpdate() {
			if (Math.Abs(m_mass - 1f) > Mathf.Epsilon) {
				float impulseMagnitude = m_pilot.impulse.magnitude;
				Vector3 impulse = (m_pilot.impulse - m_velocity);
				impulse /= m_mass;
				m_velocity = (m_velocity + impulse).normalized * impulseMagnitude;
			} else {
				m_velocity = m_pilot.impulse;
			}

			//m_rbody.angularVelocity = GameConstants.Vector3.zero;
			m_rbody.velocity = m_velocity + m_externalVelocity;
		}


		protected override void UpdateOrientation() {
			m_targetRotation = Quaternion.LookRotation(m_direction + GameConstants.Vector3.back * 0.1f, m_upVector);
			
			if (!m_pilot.IsActionPressed(Pilot.Action.Stop)) {
				if (m_dragonStyleRotation) {
					float rads = m_direction.ToAngleRadiansXY();
					m_targetRotation = MathUtils.DragonRotation( rads );

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

        protected override void FaceDragon() {
            m_direction = m_dragon.position - m_machine.position;
            m_direction.Normalize();
        }

        //--------------------------------------------------
        //--------------------------------------------------
        protected override void ExtendedAttach() {}
		protected override void ExtendedInit() {}

		protected override void OnFreeFall() {}
		protected override void ExtendedUpdateFreeFall() {}

		protected override void OnSetVelocity() {}

		public override void OnCollisionGroundEnter(Collision _collision) {}
		public override void OnCollisionGroundStay(Collision _collision) {}
		public override void OnCollisionGroundExit(Collision _collision) {}
	}
}