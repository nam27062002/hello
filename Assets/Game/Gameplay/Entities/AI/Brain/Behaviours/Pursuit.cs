using UnityEngine;
using System.Collections;
using AISM;

namespace AI {
	namespace Behaviour {
		[System.Serializable]
		public class PursuitData : StateComponentData {
			public float speed;
		}

		[CreateAssetMenu(menuName = "Behaviour/Pursuit")]
		public class Pursuit : StateComponent {

			[StateTransitionTrigger]
			private static string OnEnemyInRange = "onEnemyInRange";

			[StateTransitionTrigger]
			private static string OnEnemyOutOfSight = "onEnemyOutOfSight";

			private AIPilot m_pilot;
			private Machine m_machine;

			public override StateComponentData CreateData() {
				return new PursuitData();
			}

			protected override void OnInitialise(GameObject _go) {
				m_pilot 	= _go.GetComponent<AIPilot>();
				m_machine	= _go.GetComponent<Machine>();
				m_machine.SetSignal(Signals.Alert.name, true);
			}

			protected override void OnEnter(State oldState, object[] param) {
				m_pilot.SetSpeed(3f); // run speed
			}

			protected override void OnUpdate() {
				Transform enemy = m_machine.enemy;

				if (enemy) {
					if (m_machine.GetSignal(Signals.Danger.name)) {
						Transition(OnEnemyInRange);
					} else {
						m_pilot.GoTo(enemy.position);
					}
				} else {
					Transition(OnEnemyOutOfSight);
				}
			}
		}
	}
}