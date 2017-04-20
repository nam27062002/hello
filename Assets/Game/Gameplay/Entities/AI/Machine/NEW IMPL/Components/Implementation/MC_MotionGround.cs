using UnityEngine;
using System;

namespace AI {
	[Serializable]
	public sealed class MC_MotionGround : MC_Motion {
		//--------------------------------------------------
		private static float FREE_FALL_THRESHOLD = 0.35f;

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

		private float m_fallTimer;

		private SubState m_subState;
		private SubState m_nextSubState;


		//--------------------------------------------------
		protected override void ExtendedInit() {
			m_onGround = false;

			GetGroundNormal(0.3f);
			RaycastHit hit;
			bool hasHit = Physics.Raycast(position + m_upVector * 0.1f, -m_groundNormal, out hit, 5f, GROUND_MASK);
			if (hasHit) {
				position = hit.point;
				m_heightFromGround = 0f;
				m_onGround = true;
			}

			m_gravity = Vector3.zero;
			m_fallTimer = FREE_FALL_THRESHOLD;

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
					if (m_pilot.IsActionPressed(Pilot.Action.Jump)) {
						m_nextSubState = SubState.Jump_Start;
					} else if (m_pilot.speed > 0.01f) {						
						m_nextSubState = SubState.Move;
					}
					break;

				case SubState.Move:					
					if (m_pilot.IsActionPressed(Pilot.Action.Jump)) {
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
					if (m_onGround) {
						m_pilot.ReleaseAction(Pilot.Action.Jump);
						m_nextSubState = SubState.Idle;
					}
					break;
			}

			if (m_subState <= SubState.Move) {
				if (!m_onGround) {
					m_fallTimer -= Time.deltaTime;
					if (m_fallTimer <= 0f) {
						FreeFall();
						m_fallTimer = FREE_FALL_THRESHOLD;
					}
				}

				m_onGround = false;
			}
		}

		protected override void ExtendedFixedUpdate() {
			Vector3 gv = Vector3.down * GRAVITY * Time.fixedDeltaTime;

			if (m_subState >= SubState.Jump_Start && m_subState <= SubState.Jump_Down) {
				// ----------------------------- gravity :3
				m_velocity += gv;
				m_rbody.velocity = m_velocity;
			} else {
				if (m_groundDirection.y < -0.25f && m_direction.x > 0f
				||  m_groundDirection.y > 0.25f && m_direction.x < 0f) {
					gv *= 25f * Mathf.Abs(m_groundDirection.y);
				}

				m_gravity += gv;

				if (m_subState == SubState.Idle) {
					m_rbody.velocity = Vector3.ClampMagnitude(m_gravity, m_terminalVelocity);
				} else {
					if (m_mass != 1f) {
						Vector3 impulse = (m_pilot.impulse - m_velocity);
						impulse /= m_mass;
						m_velocity = Vector3.ClampMagnitude(m_velocity + impulse, m_pilot.speed);
					} else {
						m_velocity = m_pilot.impulse;
					}

					//lets clamp velocity while entity is turning around
					/*float angle = AngleBetweenRotTargetRot();
					float factor = (((angle - 180f) / (0f - 180f)) * (1f - 0.25f)) + 0.25f;
					m_velocity *= factor;*/

					m_rbody.velocity = Vector3.ClampMagnitude(m_velocity + m_externalVelocity + m_gravity, m_terminalVelocity);
				}
			}
		}

		protected override void ExtendedUpdateFreeFall() {			
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

		private Vector3 GetGroundNormal(float _onGroundHeight) {
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

			if (m_heightFromGround < 0.3f) {
				m_gravity = Vector3.zero;
			}

			m_onGround = m_heightFromGround < _onGroundHeight;
			m_groundNormal = normal;

			m_groundDirection = Vector3.Cross(Vector3.back, m_groundNormal);

			m_viewControl.Height(m_heightFromGround);

			return hitPos;
		}

		private void ChangeState() {
			// leave current state
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
					m_onGround = true;
					m_viewControl.Jumping(false);
					Stop();
					break;
			}

			// enter next state
			switch (m_nextSubState) {
				case SubState.Idle:
					break;

				case SubState.Move:
					break;

				case SubState.Jump_Start:
					m_onGround = true;
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

		public override void OnCollisionGroundEnter(Collision _collision) {
			OnCollisionGroundStay(_collision);
		}

		public override void OnCollisionGroundStay(Collision _collision) {
			for (int i = 0; i < _collision.contacts.Length; i++) {
				Vector3 hitPoint = _collision.contacts[i].point;
				float error = (hitPoint - position).sqrMagnitude;

				if (error <= 0.3f) {					
					m_groundNormal = _collision.contacts[i].normal;
					m_groundDirection = Vector3.Cross(Vector3.back, m_groundNormal);

					m_gravity = Vector3.zero;
					m_fallTimer = FREE_FALL_THRESHOLD;

					m_viewControl.Height(0f);

					m_onGround = true;
					break;
				}
			}
		}

		public override void OnCollisionGroundExit(Collision _collision) {
			m_onGround = false;
			m_viewControl.Height(100f);
		}
	}
}