using UnityEngine;
using System.Collections;

namespace AI {
	namespace Behaviour {	
		[CreateAssetMenu(menuName = "Behaviour/Attack/Tactics")]
		public class AttackTactics : StateComponent {

			[StateTransitionTrigger]
			private static int onEnemyInSight = UnityEngine.Animator.StringToHash("onEnemyInSight");

			[StateTransitionTrigger]
			private static int onEnemyInRange = UnityEngine.Animator.StringToHash("onEnemyInRange");


			private float m_shutdownSensorTime;
			private float m_timer;


			protected override void OnInitialise() {
				m_timer = 0f;
				m_shutdownSensorTime = 0f;
			}

			// The first element in _param must contain the amount of time without detecting an enemy
			protected override void OnEnter(State _oldState, object[] _param) {
				/*if (_param != null && _param.Length > 0) {
					m_shutdownSensorTime = (float)_param[0];
				} else */
				{
					m_shutdownSensorTime = 0f;
				}

				if (m_shutdownSensorTime > 0f) {
					m_timer = m_shutdownSensorTime;
				} else {
					m_timer = 0f;
				}

				m_machine.SetSignal(Signals.Type.Alert, true);
			}

			protected override void OnUpdate() {
				if (m_timer > 0f) {
					m_timer -= Time.deltaTime;
				} else {
					if (m_machine.GetSignal(Signals.Type.Danger)) {
						Transition(onEnemyInRange);
					} else if (m_machine.GetSignal(Signals.Type.Warning)) {
						Transition(onEnemyInSight);
					}
				}
			}
		}
	}
}