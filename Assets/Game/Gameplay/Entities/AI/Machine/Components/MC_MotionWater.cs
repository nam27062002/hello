using UnityEngine;
using System;

namespace AI {
	[Serializable]
	public sealed class MC_MotionWater : MC_Motion {
		//--------------------------------------------------
		[SerializeField] private bool m_spawnsInsideWater = true;


		//--------------------------------------------------
		private float m_diveTimer;


		//--------------------------------------------------
		protected override void ExtendedInit() {
			if (m_spawnsInsideWater) {
				m_machine.SetSignal(Signals.Type.InWater, true);
			}

			m_diveTimer = 0f;
		}

		protected override void ExtendedUpdate() {
			m_direction = m_pilot.direction;

			if (m_pilot.IsActionPressed(Pilot.Action.Stop)) {
				m_direction = (m_direction.x >= 0)? Vector3.right : Vector3.left;
			}

			if (!m_machine.GetSignal(Signals.Type.InWater) 
			&&  !m_machine.GetSignal(Signals.Type.Latching)) {
				FreeFall();
				m_diveTimer = 1f;
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

		protected override void ExtendedUpdateFreeFall() {
			m_pilot.SetDirection(Vector3.down, true);
			if (m_machine.GetSignal(Signals.Type.InWater)) {				
				m_diveTimer -= Time.deltaTime;
				if (m_diveTimer <= 0f) {
					m_pilot.SetDirection(Vector3.down, false);
					m_machine.SetSignal(Signals.Type.FallDown, false);
				}
			}
		}

		protected override void UpdateOrientation() {
			m_targetRotation = Quaternion.LookRotation(m_direction + Vector3.back * 0.1f, m_upVector);
		}

		//--------------------------------------------------
		//--------------------------------------------------
		protected override void ExtendedAttach() {}

		protected override void OnSetVelocity() {}

		public override void OnCollisionGroundEnter(Collision _collision) {}
		public override void OnCollisionGroundStay(Collision _collision) {}
		public override void OnCollisionGroundExit(Collision _collision) {}
	}
}