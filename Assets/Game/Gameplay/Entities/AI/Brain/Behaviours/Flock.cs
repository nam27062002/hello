using UnityEngine;
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
			private Vector3 m_offset;


			public override StateComponentData CreateData() {
				return new FlockData();
			}

			public override System.Type GetDataType() {
				return typeof(FlockData);
			}

			protected override void OnInitialise() {
				m_data = m_pilot.GetComponentData<FlockData>();
			}

			protected override void OnEnter(State oldState, object[] param) {
				m_timer = m_data.changeLeaderTime;
				Group group = m_machine.GetGroup();

				if (group != null && group.HasOffsets()) {
					m_offset = group.GetOffset(m_pilot.m_machine, m_data.separation);
				} else {
					m_offset = UnityEngine.Random.insideUnitSphere * m_data.separation;
				}
			}

			protected override void OnUpdate() {
				Group group = m_machine.GetGroup();

				// Every few seconds we change the leader of this flock
				if (group != null && group.count > 1) {
					if (m_data.changeLeaderTime > 0f && m_machine.GetSignal(Signals.Type.Leader)) {
						m_timer -= Time.deltaTime;
						if (m_timer <= 0) {
							m_timer = m_data.changeLeaderTime;
							group.ChangeLeader();
						}
					}
				}

				m_pilot.GoTo(m_pilot.target + m_offset);
			}
		}
	}
}