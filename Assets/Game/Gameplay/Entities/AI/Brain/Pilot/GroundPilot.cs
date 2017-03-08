using UnityEngine;
using System.Collections;

namespace AI {
	public class GroundPilot : AIPilot {

		public override void Spawn(ISpawner _spawner) {
			base.Spawn(_spawner);

			// set home position at ground
			RaycastHit groundHit;
			if (Physics.Linecast(m_homePosition, m_homePosition + Vector3.down * 15f, out groundHit, m_groundMask)) {
				m_homePosition.y = groundHit.point.y;
				m_machine.position = m_homePosition;
			}
		}

		public override void CustomUpdate() {
			base.CustomUpdate();

			// m_impulse = Vector3.zero;

			if (speed > 0.01f) {
				TransformTarget();				 

				Vector3 v = m_target - transform.position;	
				v = v.normalized * speed;
				if (m_slowDown) {
					Util.MoveTowardsVector3WithDamping(ref m_impulse, ref v, 32f * Time.deltaTime, 8.0f);
				} else {
					m_impulse = v;
				}
				Debug.DrawLine(transform.position, transform.position + m_impulse, Color.white);

				if (!m_directionForced) {// behaviours are overriding the actual direction of this machine
					if (m_impulse.x >= 0) {
						m_direction = Vector3.right;
					} else {
						m_direction = Vector3.left;
					}
				}
			}

			m_impulse += m_externalImpulse;
			m_externalImpulse = Vector3.zero;
		}

		private void TransformTarget() {
			// ground line
			Vector3 groundP1 = transform.position;
			Vector3 groundP2 = groundP1 + m_machine.groundDirection;

			// target line
			m_target.z = m_homePosition.z;

			Vector3 targetP1 = m_target;
			Vector3 targetP2 = targetP1;
			targetP2.y = 0;

			float groundA = groundP2.y - groundP1.y;
			float groundB = groundP1.x - groundP2.x;
			float groundC = groundA * groundP1.x + groundB * groundP1.y;

			float targetA = targetP2.y - targetP1.y;
			float targetB = targetP1.x - targetP2.x;
			float targetC = targetA * targetP1.x + targetB * targetP1.y;

			float det = groundA * targetB - targetA * groundB;

			if (det != 0) {
				m_target.x = (targetB * groundC - groundB * targetC) / det;
				m_target.y = (groundA * targetC - targetA * groundC) / det;
			}
		}

		void OnDrawGizmosSelected() {
			Gizmos.color = Color.white;
			Gizmos.DrawWireSphere(m_homePosition, 0.5f);
		}
	}
}