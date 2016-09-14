using UnityEngine;
using System.Collections;

namespace AI {
	namespace Behaviour {
		[System.Serializable]
		public class GuardData : StateComponentData {
		}

		[CreateAssetMenu(menuName = "Behaviour/Attack/Guard")]
		public class Guard : StateComponent {

			[StateTransitionTrigger]
			private static string OnEnemyInRange = "onEnemyInRange";

			[StateTransitionTrigger]
			private static string OnPursuitEnemy = "onPursuitEnemy";



			protected GuardData m_data;
			protected Transform m_target;
			private object[] m_transitionParam;


			public override StateComponentData CreateData() {
				return new ChargeData();
			}

			public override System.Type GetDataType() {
				return typeof(GuardData);
			}

			protected override void OnInitialise() {
				m_data = m_pilot.GetComponentData<GuardData>();
				m_machine.SetSignal(Signals.Type.Alert, true);

				m_transitionParam = new object[1];
			}

			protected override void OnEnter(State oldState, object[] param) {
				base.OnEnter(oldState, param);

				m_pilot.Stop();
				m_pilot.PressAction(Pilot.Action.Button_A);

				m_target = param[0] as Transform;
				m_transitionParam[0] = m_target;
			}

			protected override void OnExit(State _newState) {
				m_pilot.ReleaseAction(Pilot.Action.Button_A);
			}

			protected override void OnUpdate() {
				if (m_machine.GetSignal(Signals.Type.Danger)) {
					Transition(OnEnemyInRange);
				} else {
					float m = Mathf.Abs(m_machine.position.x - m_target.position.x);
					if (m > 2f) {
						Transition(OnPursuitEnemy, m_transitionParam);
					}
				}
			}
		}
	}
}