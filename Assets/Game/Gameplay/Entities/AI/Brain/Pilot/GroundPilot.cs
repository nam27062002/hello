using UnityEngine;
using System.Collections;

namespace AI {
	public class GroundPilot : AIPilot {
		protected static int m_groundMask;

		protected virtual void Start() {
			m_groundMask = 1 << LayerMask.NameToLayer("Ground");

			// set home position at ground
			RaycastHit groundHit;
			if (Physics.Linecast(m_homePosition, m_homePosition + Vector3.down * 5f, out groundHit, m_groundMask)) {
				m_homePosition.y = groundHit.point.y;
			}
		}

		protected override void Update() {
			base.Update();

			m_impulse = Vector3.zero;

			if (m_speed > 0) {
				m_target.y = transform.position.y;

				//m_direction = GetGroundDirection();

				Vector3 v = m_target - transform.position;	
				Util.MoveTowardsVector3WithDamping(ref m_impulse, ref v, m_speed, 32f * Time.deltaTime);
				Debug.DrawLine(transform.position, transform.position + m_impulse, Color.white);

				if (m_impulse.x >= 0) {
					m_direction = Vector3.right;
				} else {
					m_direction = Vector3.left;
				}
			}
		}
		/*
		private Vector3 GetGroundDirection() {			
			Vector3 distance = Vector3.down * 15f;
			Vector3 leftSensor  = transform.position - Vector3.right * 0.5f;
			Vector3 rightSensor = transform.position + Vector3.right * 0.5f;

			RaycastHit leftHit;
			RaycastHit rightHit;

			bool hasLeftHit = Physics.Linecast(leftSensor, leftSensor + distance, out leftHit, m_groundMask);
			bool hasRightHit = Physics.Linecast(rightSensor, rightSensor + distance, out rightHit, m_groundMask);

			if (m_impulse.x >= 0) {
				if (hasLeftHit && hasRightHit) {
					return (rightHit.point - leftHit.point).normalized;
				}
				return Vector3.right;
			} else {
				if (hasLeftHit && hasRightHit) {
					return (leftHit.point - rightHit.point).normalized;
				}
				return Vector3.left;
			}
		}*/

		void OnDrawGizmosSelected() {
			Gizmos.color = Color.white;
			Gizmos.DrawWireSphere(m_homePosition, 0.5f);
		}
	}
}