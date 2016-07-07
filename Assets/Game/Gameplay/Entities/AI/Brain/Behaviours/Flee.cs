using UnityEngine;
using System.Collections;

namespace AI {
	namespace Behaviour {
		[System.Serializable]
		public class FleeData : StateComponentData {
			public float speed = 1f;
			public float boostSpeed = 5f;
		}

		[CreateAssetMenu(menuName = "Behaviour/Flee")]
		public class Flee : StateComponent {

			private Vector3 m_target;

			public override StateComponentData CreateData() {
				return new FleeData();
			}

			protected override void OnInitialise() {
				m_machine.SetSignal(Signals.Alert.name, true);
			}

			protected override void OnEnter(State oldState, object[] param) {
				m_pilot.SetSpeed(1f);
				m_pilot.SetBoostSpeed(10f);
				m_target = m_machine.position;
			}

			protected override void OnExit(State newState) {
				m_pilot.ReleaseAction(Pilot.Action.Boost);
			}

			protected override void OnUpdate() {
				bool boost = m_machine.GetSignal(Signals.Danger.name);

				if (boost) {
					m_pilot.PressAction(Pilot.Action.Boost);
				} else {
					m_pilot.ReleaseAction(Pilot.Action.Boost);
				}

				float m = (m_machine.position - m_target).sqrMagnitude;
				if (m < 1f * 1f) {
					Transform enemy = m_machine.enemy;
					if (enemy != null) {
						Vector3 moveAway = m_machine.position - enemy.position;
						moveAway.z = 0f; // lets keep the current Z
						moveAway.Normalize();
						moveAway *= 10f;

						m_target = m_machine.position + moveAway;

						m_pilot.GoTo(m_target);
					}
				}
			}
		}
	}
}