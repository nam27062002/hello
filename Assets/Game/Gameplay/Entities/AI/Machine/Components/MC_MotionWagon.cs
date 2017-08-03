using UnityEngine;
using System;

namespace AI {
	[Serializable]
	public sealed class MC_MotionWagon : MC_Motion {		

		private BSpline.BezierSpline m_rails;
		public BSpline.BezierSpline rails { 
			set { 
				m_rails = value; 
				m_machine.position = m_rails.GetPointAtDistance(0, ref m_direction, ref m_upVector, ref m_right);
			} 
		}

		private Vector3 m_right;

		private float m_distanceMoved;

		//--------------------------------------------------
		protected override void ExtendedInit() {
			m_rbody.isKinematic = true;
			m_distanceMoved = 0;
		}

		protected override void ExtendedUpdate() {
			if (m_distanceMoved > m_rails.length) {
				m_velocity = m_direction * m_pilot.speed;
				m_rbody.isKinematic = false;
				//check free fall conditions
				FreeFall();
			}
		}

		protected override void ExtendedFixedUpdate() {
			// move along our path
			m_distanceMoved += m_pilot.speed * Time.fixedDeltaTime;
			if (m_distanceMoved < m_rails.length) {				
				m_machine.position = m_rails.GetPointAtDistance(m_distanceMoved, ref m_direction, ref m_upVector, ref m_right);
			}
		}

		protected override void ExtendedUpdateFreeFall() {			
			m_direction = m_velocity.normalized;
			UpdateOrientation();
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