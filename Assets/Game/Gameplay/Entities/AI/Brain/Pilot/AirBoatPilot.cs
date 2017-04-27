using UnityEngine;
using System.Collections;

namespace AI {
	public class AirBoatPilot : AIPilot {
		private static float DOT_END = -0.7f;
		private static float DOT_START = -0.99f;

		private const int CollisionCheckPools = 4;
		private static uint NextCollisionCheckID = 0;


		private Vector3 m_lastImpulse;



		protected virtual void Start() {
			m_lastImpulse = Vector3.zero;
		}

		public override void Spawn(ISpawner _spawner) {
			base.Spawn(_spawner);
		}

		public override void CustomUpdate() {
			base.CustomUpdate();

			// calculate impulse to reach our target
			m_lastImpulse = m_impulse;
			m_impulse = Vector3.zero;

			if (speed > 0.01f) {
				Vector3 v = m_target - m_machine.position;

				if (m_slowDown) { // this machine will slow down its movement when arriving to its detination
					v = v.normalized * Mathf.Min(speed, v.magnitude * 2);
					Util.MoveTowardsVector3WithDamping(ref m_impulse, ref v, 32f * Time.deltaTime, 8.0f);
				} else {
					m_impulse = v.normalized * speed;
				}

				m_direction = m_impulse.normalized;
				Debug.DrawLine(m_machine.position, m_machine.position + m_impulse, Color.white);
			}

			m_externalImpulse = Vector3.zero;
		}
	}
}