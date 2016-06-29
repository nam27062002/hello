using UnityEngine;
using System.Collections;
using AISM;

namespace AI {
	namespace Behaviour {		
		[CreateAssetMenu(menuName = "Behaviour/Attack Tactics")]
		public class AttackTactics : StateComponent {

			[StateTransitionTrigger]
			private static string OnEnemyInSight = "onEnemyInSight";

			[StateTransitionTrigger]
			private static string OnEnemyInRange = "onEnemyInRange";

			private float m_shutdownSensorTime = 10f;//TODO

			private float m_timer;

			private Pilot m_pilot;
			private Machine m_machine;

			protected override void OnInitialise(GameObject _go) {
				m_pilot 	= _go.GetComponent<Pilot>();
				m_machine	= _go.GetComponent<Machine>();
				m_machine.SetSignal(Signals.Alert.name, true);

				m_timer = 0f;
			}

			protected override void OnEnter(State _oldState, object[] _param) {
				if (m_shutdownSensorTime > 0f && _oldState != null && _oldState.name.Contains("Attack")) {
					m_timer = m_shutdownSensorTime;
					m_machine.SetSignal(Signals.Alert.name, false);
				} else {
					m_machine.SetSignal(Signals.Alert.name, true);
				}
			}

			protected override void OnUpdate() {
				if (m_timer > 0f) {
					m_timer -= Time.deltaTime;
					if (m_timer <= 0f) {
						m_machine.SetSignal(Signals.Alert.name, true);
					}
				} else {
					if (m_machine.GetSignal(Signals.Danger.name)) {
						Transition(OnEnemyInRange);
					} else if (m_machine.GetSignal(Signals.Warning.name)) {
						Transition(OnEnemyInSight);
					}
				}
			}
		}
	}
}