using UnityEngine;
using System;

namespace AI {
	[Serializable]
	public sealed class MC_MotionWagon : MC_Motion {
		private BSpline.BezierSpline m_rails;
		public BSpline.BezierSpline rails { 
			set { 
				m_rails = value; 
				m_machine.position = m_rails.GetPointAtDistance(0, ref m_direction, ref m_upVector, ref m_right, true);
			} 
		}

		private Vector3 m_right;
		private float m_accelerationScalar;

		private float m_distanceMoved;

		//--------------------------------------------------
		protected override void ExtendedInit() {			
			m_distanceMoved = 0f;
			m_accelerationScalar = 0f;
		}

		protected override void ExtendedUpdate() {
			
		}

		protected override void ExtendedFixedUpdate() {
			// move along our path
			float dt = Time.fixedDeltaTime;
			float speed = m_pilot.speed;// - (m_direction.y * m_pilot.speed * 0.9f);

			if (m_direction.y < 0) { // going down
				m_accelerationScalar += Math.Abs(m_direction.y) * 0.5f;
				if (m_accelerationScalar > m_pilot.speed * 2f) 
					m_accelerationScalar = m_pilot.speed * 2f;
			} else { // going up
				m_accelerationScalar -= 0.05f;
				if (m_accelerationScalar < -m_pilot.speed * 0.1f) 
					m_accelerationScalar = -m_pilot.speed * 0.1f;
			}

			speed += m_accelerationScalar;

			m_velocity = m_direction * speed;

			m_distanceMoved += speed * dt;
			if (m_distanceMoved < m_rails.length) {				
				m_machine.position = m_rails.GetPointAtDistance(m_distanceMoved, ref m_direction, ref m_upVector, ref m_right, true);
			} else {
				FreeFall();
			}
		}

		protected override void OnFreeFall() { 
			//check free fall conditions
			m_externalVelocity = m_velocity * 2f;
			//m_velocity = Vector3.zero;
		}

		protected override void ExtendedUpdateFreeFall() {			
			m_direction = m_velocity.normalized;
			m_upVector = Vector3.Cross(m_direction, m_right);
			UpdateOrientation();
		}

		protected override void UpdateOrientation() {
			m_targetRotation = Quaternion.LookRotation(m_direction + GameConstants.Vector3.back * 0.1f, m_upVector);
		}


		//--------------------------------------------------
		//--------------------------------------------------
		protected override void ExtendedAttach() {}

		protected override void OnSetVelocity() {}

        protected override void FaceDragon() {}

		public override void OnCollisionGroundEnter(Collision _collision) {}
		public override void OnCollisionGroundStay(Collision _collision) {}
		public override void OnCollisionGroundExit(Collision _collision) {}
	}
}