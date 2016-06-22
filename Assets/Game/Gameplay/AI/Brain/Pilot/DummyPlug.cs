using UnityEngine;
using System.Collections;

namespace AI {
	public class DummyPlug : Pilot {

		public float m_editorRadius = 1f;
		public float m_editorSpeed = 1f;
		public float m_distanceDelta = 0.1f;
		public float m_separation = 2f;
		public float m_followDistance = 1f;

		public float m_changeLeaderTime = 0.5f;

		private float m_timer = 0.5f;

		void LateUpdate() {
			SetSpeed(m_editorSpeed);

			m_machine.SetSignal(Signals.Alert.name, true);
			Avoid(m_machine.GetSignal(Signals.Warning.name));

			// flocking
			if (m_machine.GetSignal(Signals.Leader.name)) {
				float m = (transform.position - m_target).sqrMagnitude;

				if (m < m_distanceDelta) {
					m_target = Random.insideUnitSphere * m_editorRadius;
					m_target.z = 0;
				} 

				GoTo(m_target);

				m_timer -= Time.deltaTime;
				if (m_timer <= 0) {
					m_timer = m_changeLeaderTime;
					m_machine.GetGroup().ChangeLeader();
				}
			} else {
				FollowBehaviour();
				FlockBehaviour();
			}
		}

		void FollowBehaviour() {
			IMachine leader = m_machine.GetGroup().leader;
			GoTo(leader.position - leader.direction * m_followDistance);
		}

		void FlockBehaviour() {
			// separation
			Vector3 separation = Vector3.zero;

			Group group = m_machine.GetGroup();
			for (int i = 0; i < group.count; i++) {
				if ((IMachine)group[i] != m_machine) {
					Vector3 v = m_machine.position - group[i].position;
					float d = v.magnitude;
					if (d < m_separation) {
						separation += v.normalized * (m_separation - d);
					}
				}
			}

			AddImpulse(separation);

			// alignment
			Vector3 alignment = Vector3.zero;
		}
	}
}