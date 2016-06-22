using UnityEngine;
using System.Collections;
using AISM;

namespace AI {
	namespace Behaviour {		
		[CreateAssetMenu(menuName = "Behaviour/Flock")]
		public class Flock : StateComponent {
			
			//TODO: serialize
			public float m_changeLeaderTime = 10f;

			private Pilot m_pilot;
			private Machine m_machine;

			private float m_timer;

			protected override void OnInitialise(GameObject _go) {
				m_pilot 	= _go.GetComponent<Pilot>();
				m_machine	= _go.GetComponent<Machine>();
			}

			protected override void OnEnter(State oldState, object[] param) {
				m_timer = m_changeLeaderTime;
			}

			protected override void OnUpdate() {
				// Every few seconds we change the leader of this flock
				if (m_machine.GetSignal(Signals.Leader.name)) {
					m_timer -= Time.deltaTime;
					if (m_timer <= 0) {
						m_timer = m_changeLeaderTime;
						m_machine.GetGroup().ChangeLeader();
					}
				}
			
				// Separation
				Vector3 separation = Vector3.zero;
			
				Group group = m_machine.GetGroup();
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