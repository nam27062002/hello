using UnityEngine;
using System.Collections;

namespace AI {
	namespace Behaviour {
		[System.Serializable]
		public class GuardOutOfRangeData : StateComponentData {
		}

		[CreateAssetMenu(menuName = "Behaviour/Attack/Guard Out Of Range")]
		public class GuardOutOfRange : StateComponent {

			[StateTransitionTrigger]
			private static readonly int onEnemyInRange = UnityEngine.Animator.StringToHash("onEnemyInRange");

			[StateTransitionTrigger]
			private static readonly int onPursuitEnemy = UnityEngine.Animator.StringToHash("onPursuitEnemy");



			protected GuardOutOfRangeData m_data;
			protected Transform m_target;
			private object[] m_transitionParam;


			public override StateComponentData CreateData() {
				return new GuardOutOfRangeData();
			}

			public override System.Type GetDataType() {
				return typeof(GuardOutOfRangeData);
			}

			protected override void OnInitialise() {
				m_data = m_pilot.GetComponentData<GuardOutOfRangeData>();
				m_machine.SetSignal(Signals.Type.Alert, true);

				m_transitionParam = new object[1];
			}

			protected override void OnEnter(State oldState, object[] param) {
				base.OnEnter(oldState, param);

				m_pilot.Stop();
				m_pilot.PressAction(Pilot.Action.Button_A);

				m_target = m_machine.enemy;
				//m_target = param[0] as Transform;
				//m_transitionParam[0] = m_target;
			}

			protected override void OnExit(State _newState) {
				m_pilot.ReleaseAction(Pilot.Action.Button_A);
			}

			protected override void OnUpdate() {
				if (m_machine.GetSignal(Signals.Type.Danger)) {
					Transition(onEnemyInRange);
				} else {
					if (m_target != null) {					
						float m = Mathf.Abs(m_machine.position.x - m_target.position.x);
						if (m > 2f) {
							Transition(onPursuitEnemy/*, m_transitionParam*/);
						}
					} else {
						Transition(onPursuitEnemy);
					}
				}
			}
		}
	}
}