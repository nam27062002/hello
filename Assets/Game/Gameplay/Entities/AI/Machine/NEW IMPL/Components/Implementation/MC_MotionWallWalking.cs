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
		private Vector3 m_groundNormal;

		private bool m_onGround;
		private float m_heightFromGround;

		private SubState m_subState;
		private SubState m_nextSubState;


		//--------------------------------------------------
		protected override void ExtendedInit() {
			m_onGround = false;

			//find the nearest wall and use that up vector
			RaycastHit[] hit = new RaycastHit[4];
			bool[] hasHit = new bool[4];

			hasHit[0] = Physics.Raycast(position, Vector3.down * 5f, out hit[0], GROUND_MASK);
			hasHit[1] = Physics.Raycast(position, Vector3.up * 5f,	 out hit[1], GROUND_MASK);
			hasHit[2] = Physics.Raycast(position, Vector3.right * 5f,out hit[2], GROUND_MASK);
			hasHit[3] = Physics.Raycast(position, Vector3.left * 5f, out hit[3], GROUND_MASK);

			float d = 99999f;
			for (int i = 0; i < 4; i++) {
				if (hasHit[i]) {
					if (hit[i].distance < d) {
						d = hit[i].distance;

						m_upVector = hit[i].normal;
						position = hit[i].point;

						m_heightFromGround = 0f;
						m_onGround = true;
					}
				}
			}

			m_subState = SubState.Idle;
			m_nextSubState = SubState.Idle;
		}

		public override void OnCollisionGroundEnter() {
			m_onGround = true;
		}

		public override void OnCollisionGroundExit() {
			m_onGround = false;
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
					if (m_pilot.speed > 0.01f) {
						m_nextSubState = SubState.Move;
					}

					GetGroundNormal();
					break;

				case SubState.Move:
					if (m_pilot.speed <= 0.01f) {
						m_nextSubState = SubState.Idle;
					}

					GetGroundNormal();
					break;
			}
		}

		protected override void ExtendedFixedUpdate() {
			if (m_subState > SubState.Idle) {
				if (m_mass != 1f) {
					Vector3 impulse = (m_pilot.impulse - m_velocity);
					impulse /= m_mass;
					m_velocity = Vector3.ClampMagnitude(m_velocity + impulse, m_pilot.speed);
				} else {
					m_velocity = m_pilot.impulse;
				}
			}

			// ----------------------------- gravity :3
			m_rbody.velocity = m_velocity + (-m_groundNormal * 9.8f * 3f * Time.fixedDeltaTime) + m_externalVelocity;
		}

		protected override void UpdateOrientation() {
			m_targetRotation = Quaternion.LookRotation(m_direction + Vector3.back * 0.1f, m_upVector);

			if (m_limitHorizontalRotation) {
				if (m_direction.x < 0f) 	 m_targetRotation = Quaternion.AngleAxis(m_faceLeftAngle, m_upVector) * m_targetRotation; 
				else if (m_direction.x > 0f) m_targetRotation = Quaternion.AngleAxis(m_faceRightAngle, m_upVector) * m_targetRotation; 
			}
		}

		private void GetGroundNormal() {
			Vector3 normal = m_upVector;
			Vector3 pos = position + (m_upVector * 3f);

			if (m_subState == SubState.Move) {
				// we'll check forward
				Vector3 dir = m_direction;
				dir.z = 0f;
				pos += dir * 0.5f;
			}

			RaycastHit hit;
			if (Physics.SphereCast(pos, 1f, -m_upVector, out hit, 6f, GROUND_MASK)) {				
				normal = (hit.normal * 0.75f) + (m_groundNormal * 0.25f);
				normal.Normalize();
				m_heightFromGround = hit.distance - 3f;
			} else {
				m_heightFromGround = 100f;
			}

			m_onGround = m_heightFromGround < 0.3f;
			m_groundNormal = normal;
			m_upVector = normal;

			m_viewControl.Height(m_heightFromGround);
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

		protected override void ExtendedUpdateFreeFall() { }	
	}
}