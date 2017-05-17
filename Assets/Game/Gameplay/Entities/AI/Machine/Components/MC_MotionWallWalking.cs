using UnityEngine;
using System;

namespace AI {
	[Serializable]
	public sealed class MC_MotionWallWalking : MC_Motion {

		//--------------------------------------------------
		private enum SubState {
			Idle = 0,
			Move
		};


		//--------------------------------------------------
		[SeparatorAttribute("Orientation")]
		[SerializeField] private bool m_limitHorizontalRotation = false;
		[SerializeField] private float m_faceLeftAngle = -90f;
		[SerializeField] private float m_faceRightAngle = 90f;


		//--------------------------------------------------
		private Vector3 m_upVectorSave;
		private Vector3 m_groundNormalSave;
		private Vector3 m_groundDirectionSave;

		private Vector3 m_groundNormal;
		private Vector3 m_groundDirection;
		public Vector3 groundDirection { get { return m_groundDirection; } }

		private Vector3 m_gravity;

		private bool m_onGround;
		private float m_heightFromGround;

		private bool m_checkCollisions;
		public bool checkCollisions { set { 
				if (m_checkCollisions != value) {
					if (value) {
						m_groundNormal = m_groundNormalSave;
						m_upVector = m_upVectorSave;
						m_groundDirection = m_groundDirectionSave;
					} else {
						m_groundNormalSave = m_groundNormal;
						m_groundNormal = Vector3.up;

						m_upVectorSave = m_upVector;
						m_upVector = Vector3.up;

						m_groundDirectionSave = m_groundDirection;
						m_groundDirection = Vector3.Cross(Vector3.back, m_upVector);
					}
					m_checkCollisions = value;
				}
			}
		}

		private SubState m_subState;
		private SubState m_nextSubState;


		//--------------------------------------------------
		protected override void ExtendedInit() {
			m_onGround = false;
			m_checkCollisions = true;

			//find the nearest wall and use that up vector
			FindUpVector();

			if (!m_onGround) {
				m_upVector = Vector3.up;
				FreeFall();
			}

			m_groundDirection = Vector3.Cross(Vector3.back, m_upVector);

			m_gravity = Vector3.zero;

			m_subState = SubState.Idle;
			m_nextSubState = SubState.Idle;
		}

		protected override void ExtendedUpdate() {
			if (m_nextSubState != m_subState) {
				ChangeState();
			}

			m_direction = m_pilot.direction;

			switch (m_subState) {
				case SubState.Idle:
					if (m_pilot.speed > 0.01f) {
						m_nextSubState = SubState.Move;
					}
					break;

				case SubState.Move:
					if (m_pilot.speed <= 0.01f) {
						m_nextSubState = SubState.Idle;
					}
					break;
			}
		}

		protected override void ExtendedFixedUpdate() {
			if (m_checkCollisions) {
				m_gravity += -m_groundNormal * GRAVITY * Time.fixedDeltaTime;
			} else {
				m_gravity = Vector3.zero;
			}

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

		protected override void ExtendedUpdateFreeFall() {			
			if (m_onGround) {
				m_machine.SetSignal(Signals.Type.FallDown, false);
				FindUpVector();
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

		private Vector3 GetGroundNormal() {	
			Vector3 hitPos = position;

			if (m_checkCollisions) {
				Vector3 normal = m_upVector;
				Vector3 pos = position + (m_upVector * 2f);

				if (m_subState == SubState.Move) {
					// we'll check forward
					Vector3 dir = m_direction;
					dir.z = 0f;
					pos += dir * 0.15f;
				}

				RaycastHit hit;
				if (Physics.Raycast(pos, -m_upVector, out hit, 6f, GROUND_MASK)) {
					normal = (hit.normal * 0.75f) + (m_groundNormal * 0.25f);
					hitPos = hit.point;
					m_heightFromGround = hit.distance - 3f;
				} else {
					m_heightFromGround = 100f;
				}

				if (m_heightFromGround < 0.3f) {
					m_gravity = Vector3.zero;
				}

				m_onGround = m_heightFromGround < 0.3f;
				m_groundNormal = normal;
				m_upVector = normal;

				m_groundDirection = Vector3.Cross(Vector3.back, m_upVector);
			}

			return hitPos;
		}

		private void FindUpVector() {
			RaycastHit[] hit = new RaycastHit[4];
			bool[] hasHit = new bool[4];

			hasHit[0] = Physics.Raycast(position, Vector3.down,  out hit[0], 6f, GROUND_MASK);
			hasHit[1] = Physics.Raycast(position, Vector3.up,	 out hit[1], 6f, GROUND_MASK);
			hasHit[2] = Physics.Raycast(position, Vector3.right, out hit[2], 6f, GROUND_MASK);
			hasHit[3] = Physics.Raycast(position, Vector3.left,  out hit[3], 6f, GROUND_MASK);

			float d = 99999f;
			for (int i = 0; i < 4; i++) {
				if (hasHit[i]) {
					if (hit[i].distance < d) {
						d = hit[i].distance;

						m_upVector = hit[i].normal;
						m_groundNormal = hit[i].normal;
						position = hit[i].point;

						m_heightFromGround = 0f;
						m_onGround = true;
					}
				}
			}
		}

		private void ChangeState() {
			switch (m_subState) {
				case SubState.Idle:
					break;

				case SubState.Move:
					Stop();
					break;
			}

			switch (m_nextSubState) {
				case SubState.Idle:
					break;

				case SubState.Move:
					break;
			}

			m_subState = m_nextSubState;
		}

		//--------------------------------------------------
		//--------------------------------------------------
		protected override void ExtendedAttach() {}

		protected override void OnSetVelocity() {}

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

					m_heightFromGround = 0f;
					m_viewControl.Height(0f);

					m_onGround = true;
					break;
				}
			}
		}

		public override void OnCollisionGroundExit(Collision _collision) {
			m_onGround = false;
			m_heightFromGround = 100f;
			m_viewControl.Height(100f);
		}
	}
}