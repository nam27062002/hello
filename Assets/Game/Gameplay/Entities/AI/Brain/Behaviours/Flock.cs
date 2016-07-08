﻿using UnityEngine;
using System.Collections;

namespace AI {
	namespace Behaviour {		
		[System.Serializable]
		public class FlockData : StateComponentData {
			public float changeLeaderTime;
			public float separation;
		}

		[CreateAssetMenu(menuName = "Behaviour/Flock")]
		public class Flock : StateComponent {

			private FlockData m_data;

			private float m_timer;


			public override StateComponentData CreateData() {
				return new FlockData();
			}

			protected override void OnInitialise() {
				m_data = (FlockData)m_pilot.GetComponentData<Flock>();
			}

			protected override void OnEnter(State oldState, object[] param) {
				m_timer = 0f;
			}

			protected override void OnUpdate() {
				Group group = m_machine.GetGroup();

				// Every few seconds we change the leader of this flock
				if (group.count > 1) {
					if (m_machine.GetSignal(Signals.Leader.name)) {
						m_timer -= Time.deltaTime;
						if (m_timer <= 0) {
							m_timer = m_data.changeLeaderTime;
							group.ChangeLeader();
						}
					}
				
					// Separation
					Vector3 separation = Vector3.zero;
					for (int i = 0; i < group.count; i++) {
						if ((IMachine)group[i] != m_machine) {
							Vector3 v = m_machine.position - group[i].position;
							float d = v.magnitude;
							if (d < m_data.separation) {
								separation += v.normalized * (m_data.separation - d);
							}
						}
					}
				
					m_pilot.AddImpulse(separation);
				}
			}
		}
	}
}