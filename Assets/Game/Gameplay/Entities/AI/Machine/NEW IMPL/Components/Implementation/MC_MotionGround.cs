using UnityEngine;
using System;

namespace AI {
	[Serializable]
	public sealed class MC_MotionGround : MC_Motion {

		//--------------------------------------------------
		private enum SubState {
			Idle = 0,
			Move,
			Jump_Start,
			Jump_Up,
			Jump_Down
		};


		//--------------------------------------------------
		[SeparatorAttribute("Orientation")]
		[SerializeField] private bool m_limitHorizontalRotation = false;
		[SerializeField] private float m_faceLeftAngle = -90f;
		[SerializeField] private float m_faceRightAngle = 90f;


		//--------------------------------------------------
		private Vector3 m_groundNormal;
		private Vector3 m_groundDirection;
		public Vector3 groundDirection { get { return m_groundDirection; } }

		private Vector3 m_gravity;

		private bool m_onGround;
		private bool m_colliderOnGround;
		private float m_heightFromGround;

		private SubState m_subState;
		private SubState m_nextSubState;


		//--------------------------------------------------
		protected override void ExtendedInit() {
			m_onGround = false;

			GetGroundNormal();
			RaycastHit hit;
			bool hasHit = Physics.Raycast(position + m_upVector * 0.1f, -m_groundNormal, out hit, 5f, GROUND_MASK);
			if (hasHit) {
				position = hit.point;
				m_heightFromGround = 0f;
				m_onGround = true;
			}

			m_subState = SubState.Idle;
			m_nextSubState = SubState.Idle;
		}

		protected override void ExtendedUpdate() {
			if (m_nextSubState != m_subState) {
				ChangeState();
			}

			m_direction = m_pilot.direction;

			if (!m_pilot.IsActionPressed(Pilot.Action.Stop)) {
				m_direction = (m_direction.x >= 0)? Vector3.right : Vector3.left;
			}

			switch (m_subState) {
				case SubState.Idle:
					GetGroundNormal();
					if (!m_onGround) {
						FreeFall();
					} else if (m_pilot.IsActionPressed(Pilot.Action.Jump)) {
						m_nextSubState = SubState.Jump_Start;
					} else if (m_pilot.speed > 0.01f) {						
						m_nextSubState = SubState.Move;
					}
					break;

				case SubState.Move:
					GetGroundNormal();
					if (!m_onGround) {
						FreeFall();
					} else if (m_pilot.IsActionPressed(Pilot.Action.Jump)) {
						m_nextSubState = SubState.Jump_Start;
					} else if (m_pilot.speed <= 0.01f) {
						m_nextSubState = SubState.Idle;
					}
					break;

				case SubState.Jump_Start:
					break;

				case SubState.Jump_Up:
					if (m_velocity.y < 0f) {
						m_nextSubState = SubState.Jump_Down;
					}
					break;

				case SubState.Jump_Down:
					GetGroundNormal();
					if (m_onGround) {
						m_pilot.ReleaseAction(Pilot.Action.Jump);
						m_nextSubState = SubState.Idle;
					}
					break;
			}
		}

		protected override void ExtendedFixedUpdate() {
			if (m_subState >= SubState.Jump_Start && m_subState <= SubState.Jump_Down) {
				// ----------------------------- gravity :3
				m_velocity += Vector3.down * GRAVITY * Time.fixedDeltaTime;
				m_rbody.velocity = m_velocity;
			} else {
				m_gravity += Vector3.down * GRAVITY * 10f * Time.fixedDeltaTime;

				if (m_subState == SubState.Idle) {
					m_rbody.velocity = m_gravity;
				} else {
					if (m_mass != 1f) {
						Vector3 impulse = (m_pilot.impulse - m_velocity);
						impulse /= m_mass;
						m_velocity = Vector3.ClampMagnitude(m_velocity + impulse, m_pilot.speed);
					} else {
						m_velocity = m_pilot.impulse;
					}

					m_rbody.velocity = m_velocity + m_externalVelocity + m_gravity;
				}
			}
		}

		protected override void ExtendedUpdateFreeFall() {
			GetGroundNormal();
			if (m_onGround) {
				m_machine.SetSignal(Signals.Type.FallDown, false);
				m_nextSubState = SubState.Idle;
			}
		}

		protected override void UpdateOrientation() {
			m_targetRotation = Quaternion.LookRotation(m_direction + Vector3.back * 0.1f, m_upVector);

			if (m_limitHorizontalRotation) {
				if (m_direction.x < 0f) 	 m_targetRotation = Quaternion.AngleAxis(m_faceLeftAngle, m_upVector) * m_targetRotation; 
				else if (m_direction.x > 0f) m_targetRotation = Quaternion.AngleAxis(m_faceRightAngle, m_upVector) * m_targetRotation; 
			}
		}

		protected override void OnSetVelocity() {
			if (m_subState == SubState.Jump_Start) {
				m_nextSubState = SubState.Jump_Up;
			}
		}

		private Vector3 GetGroundNormal() {
			Vector3 normal = Vector3.up;
			Vector3 hitPos = position;
			Vector3 pos = position + (m_upVector * 3f);

			RaycastHit hit;		
			if (Physics.Raycast(pos, -m_upVector, out hit, 6f, GROUND_MASK)) {
				normal = hit.normal;
				hitPos = hit.point;
				m_heightFromGround = hit.distance - 3f;
			} else {
				m_heightFromGround = 100f;
			}

			if (m_heightFromGround < 0.1f) {
				m_gravity = Vector3.zero;
			}

			m_onGround = m_heightFromGround < 0.3f;
			m_groundNormal = normal;

			m_groundDirection = Vector3.Cross(Vector3.back, m_upVector);
			if (m_groundDirection.y > 0.5f || m_groundDirection.y < -0.5f) {
				m_groundDirection = Vector3.right;
			}

			m_viewControl.Height(m_heightFromGround);

			return hitPos;
		}

		private void ChangeState() {
			switch (m_subState) {
				case SubState.Idle:
					break;

				case SubState.Move:
					Stop();
					break;

				case SubState.Jump_Start:
					break;

				case SubState.Jump_Up:
					break;

				case SubState.Jump_Down:
					m_viewControl.Jumping(false);
					Stop();
					break;
			}

			switch (m_nextSubState) {
				case SubState.Idle:
					break;

				case SubState.Move:
					break;

				case SubState.Jump_Start:
					m_viewControl.Jumping(true);
					Stop();
					break;

				case SubState.Jump_Up:
					break;

				case SubState.Jump_Down:
					break;
			}

			m_subState = m_nextSubState;
		}

		//--------------------------------------------------
		//--------------------------------------------------
		protected override void ExtendedAttach() {}

		public override void OnCollisionGroundEnter() {}
		public override void OnCollisionGroundExit() {}
	}
}