using UnityEngine;
using System.Collections;

namespace AI {
	public class AirPilot : AIPilot {
		private static float DOT_END = -0.7f;
		private static float DOT_START = -0.99f;

		[SerializeField] private float m_avoidDistanceAttenuation = 2f;

		private bool m_perpendicularAvoid;


		protected virtual void Start() {
			m_perpendicularAvoid = false;
		}

		protected override void Update() {
			base.Update();

			// calculate impulse to reach our target
			m_impulse = Vector3.zero;

			if (m_speed > 0) {
				Vector3 seek = Vector3.zero;
				Vector3 flee = Vector3.zero;

				Vector3 v = m_target - transform.position;	
				Util.MoveTowardsVector3WithDamping(ref seek, ref v, m_speed, 32f * Time.deltaTime);
				Debug.DrawLine(transform.position, transform.position + seek, Color.green);

				if (m_actions[(int)Action.Avoid]) {
					Transform enemy = m_machine.enemy;
					if (enemy != null) {
						v = transform.position - enemy.position;
						float distSqr = v.sqrMagnitude;
						if (distSqr > 0) {
							v.Normalize();
							v *= (m_avoidDistanceAttenuation * m_avoidDistanceAttenuation) / distSqr;
						}
						flee = v;
						flee.z = 0;

						Debug.DrawLine(transform.position, transform.position + flee, Color.red);
					}
				}

				float dot = Vector3.Dot(seek.normalized, flee.normalized);

				if (dot <= DOT_START) {
					m_perpendicularAvoid = true;
				} else if (dot > DOT_END) {
					m_perpendicularAvoid = false;
				}

				if (m_perpendicularAvoid) {
					m_impulse.Set(-flee.y, flee.x, flee.z);
					m_impulse = m_impulse.normalized * (Mathf.Max(seek.magnitude, flee.magnitude));
				} else {
					m_impulse = seek + flee;
				}

				m_impulse += m_externalImpulse;

				m_direction = m_impulse.normalized;
				m_impulse = m_direction * seek.magnitude;

				Debug.DrawLine(transform.position, transform.position + m_impulse, Color.white);
			}

			m_externalImpulse = Vector3.zero;
		}
	}
}