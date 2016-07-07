using UnityEngine;
using System.Collections;

namespace AI {
	namespace Behaviour {		
		[System.Serializable]
		public class FlockData : StateComponentData {
			public float m_changeLeaderTime;
		}

		[CreateAssetMenu(menuName = "Behaviour/Flock")]
		public class Flock : StateComponent {
			
			//TODO: serialize
			public float m_changeLeaderTime = 5f;

			private float m_timer;


			public override StateComponentData CreateData() {
				return new FlockData();
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
							m_timer = m_changeLeaderTime;
							group.ChangeLeader();
						}
					}
				
					// Separation
					Vector3 separation = Vector3.zero;
					for (int i = 0; i < group.count; i++) {
						if ((IMachine)group[i] != m_machine) {
							Vector3 v = m_machine.position - group[i].position;
							float d = v.magnitude;
							if (d < 1f) { //TODO: serialize separation
								separation += v.normalized * (1f - d);
							}
						}
					}
				
					m_pilot.AddImpulse(separation);
				}
			}
		}
	}
}