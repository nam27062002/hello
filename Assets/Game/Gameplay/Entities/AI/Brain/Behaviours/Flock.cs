using UnityEngine;
using System.Collections;

namespace AI {
	namespace Behaviour {		
		[System.Serializable]
		public class FlockData : StateComponentData {
			public float changeLeaderTime = 5f;
			public float separation;
		}

		[CreateAssetMenu(menuName = "Behaviour/Flock")]
		public class Flock : StateComponent {

			private FlockData m_data;

			private float m_timer;
			private float m_updateOffsetTimer;
			private Vector3 m_offset;

			private bool m_changeFormationOrientation;

			public override StateComponentData CreateData() {
				return new FlockData();
			}

			public override System.Type GetDataType() {
				return typeof(FlockData);
			}

			protected override void OnInitialise() {
				m_data = m_pilot.GetComponentData<FlockData>();
				m_changeFormationOrientation = false;
			}

			protected override void OnEnter(State oldState, object[] param) {
				m_timer = m_data.changeLeaderTime;		
				Group group = m_machine.GetGroup();

				if (group != null && group.HasOffsets()) {
					m_offset = group.GetOffset(m_pilot.m_machine, m_data.separation);
				} else {
					m_offset = UnityEngine.Random.insideUnitSphere * m_data.separation;
				}

				m_updateOffsetTimer = 0f;

				m_changeFormationOrientation = group != null && (group.formation == Group.Formation.Triangle);
			}

			protected override void OnUpdate() {
				Group group = m_machine.GetGroup();

				// Every few seconds we change the leader of this flock
				if (group != null) {
					/*if (group.count > 1) {
						if (m_data.changeLeaderTime > 0f && m_machine.GetSignal(Signals.Type.Leader)) {
							m_timer -= Time.deltaTime;
							if (m_timer <= 0) {
								m_timer = m_data.changeLeaderTime;

								group.ChangeLeader();

								m_updateOffsetTimer = 0f;
							}
						}
					}*/

					m_updateOffsetTimer -= Time.deltaTime;
					if (m_updateOffsetTimer <= 0f) {
						m_offset = group.GetOffset(m_pilot.m_machine, m_data.separation);
						m_updateOffsetTimer = 2.5f;
					}
				}

				if (m_changeFormationOrientation) {
					if (m_machine.GetSignal(Signals.Type.Leader)) {
						group.UpdateRotation(m_pilot.direction);
					}
					m_offset = group.GetOffset(m_pilot.m_machine, m_data.separation);
				}

				m_pilot.GoTo(m_pilot.target + m_offset);
			}
		}
	}
}