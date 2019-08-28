using UnityEngine;
using System.Collections;

namespace AI {
	public class GroundPilot : AIPilot {

		public override void Spawn(ISpawner _spawner) {
			base.Spawn(_spawner);

			// set home position at ground
			RaycastHit groundHit;
            if (Physics.Linecast(m_homePosition, m_homePosition + Vector3.down * 15f, out groundHit, GameConstants.Layers.GROUND_PREYCOL)) {
				m_homePosition.y = groundHit.point.y;
				m_machine.position = m_homePosition;
			}
		}

		public override void CustomUpdate() {            
            base.CustomUpdate();

            if (speed > 0.01f) {
				Vector3 v = m_target - m_machine.position;	
				v = v.normalized * speed;
				if (m_slowDown) {
					Util.MoveTowardsVector3WithDamping(ref m_impulse, ref v, 32f * Time.deltaTime, 8.0f);
				} else {
					m_impulse = v;
				}

				#if UNITY_EDITOR
				Debug.DrawLine(m_machine.position, m_machine.position + m_impulse, Color.white);
				Debug.DrawLine(m_machine.position, m_target, Colors.coral);
				#endif

				if (!m_directionForced) {// behaviours are overriding the actual direction of this machine
					if (m_impulse.x >= 0) {
						m_direction = GameConstants.Vector3.right;
					} else {
						m_direction = GameConstants.Vector3.left;
					}
				}
			}

			m_impulse += m_externalImpulse;
			m_externalImpulse = GameConstants.Vector3.zero;            
        }

		void OnDrawGizmosSelected() {
			Gizmos.color = Color.white;
			Gizmos.DrawWireSphere(m_homePosition, 0.5f);
		}
	}
}