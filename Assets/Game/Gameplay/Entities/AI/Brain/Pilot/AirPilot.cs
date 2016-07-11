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

			if (speed > 0.01f) {
				Vector3 seek = Vector3.zero;
				Vector3 flee = Vector3.zero;

				Vector3 v = m_target - m_machine.position;

				if (m_slowDown) { // this machine will slow down its movement when arriving to its detination
					Util.MoveTowardsVector3WithDamping(ref seek, ref v, m_speed, 32f * Time.deltaTime);
				} else {
					seek = v.normalized * m_speed;
				}
				Debug.DrawLine(m_machine.position, m_machine.position + seek, Color.green);

				if (m_actions[(int)Action.Avoid]) {
					Transform enemy = m_machine.enemy;
					if (enemy != null) {
						v = m_machine.position - enemy.position;
						v.z = 0;

						float distSqr = v.sqrMagnitude;
						if (distSqr > 0 && m_avoidDistanceAttenuation > 0) {
							v.Normalize();
							v *= (m_avoidDistanceAttenuation * m_avoidDistanceAttenuation) / distSqr;
						}
						flee = v;
						flee.z = 0;

						Debug.DrawLine(m_machine.position, m_machine.position + flee, Color.red);
					}
				}

				float dot = Vector3.Dot(seek.normalized, flee.normalized);
				float seekMagnitude = seek.magnitude;
				float fleeMagnitude = flee.magnitude;

				if (dot <= DOT_START) {
					if (Mathf.Abs(seekMagnitude - fleeMagnitude) < 0.01f) {
						m_perpendicularAvoid = true;
					}
				} else if (dot > DOT_END) {
					m_perpendicularAvoid = false;
				}

				if (m_perpendicularAvoid) {
					m_impulse.Set(-flee.y, flee.x, flee.z);
					m_impulse = m_impulse.normalized * (Mathf.Max(seekMagnitude, fleeMagnitude));
				} else {
					m_impulse = seek + flee;
				}

				m_impulse += m_externalImpulse;

				m_direction = m_impulse.normalized;
				m_impulse = Vector3.ClampMagnitude(m_impulse, speed);

				Debug.DrawLine(m_machine.position, m_machine.position + m_impulse, Color.white);
			}

			m_externalImpulse = Vector3.zero;
		}
	}
}