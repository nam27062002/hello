using UnityEngine;
using System.Collections;

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

			private PursuitData m_data;


			public override StateComponentData CreateData() {
				return new PursuitData();
			}

			protected override void OnInitialise() {
				m_data = (PursuitData)m_pilot.GetComponentData<Pursuit>();

				m_machine.SetSignal(Signals.Alert.name, true);
			}

			protected override void OnEnter(State oldState, object[] param) {
				m_pilot.SetMoveSpeed(m_data.speed);
				m_pilot.SlowDown(true);
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