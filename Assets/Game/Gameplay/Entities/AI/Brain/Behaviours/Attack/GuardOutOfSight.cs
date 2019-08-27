using UnityEngine;
using System.Collections;

namespace AI {
	namespace Behaviour {
		[System.Serializable]
		public class GuardOutOfSightData : StateComponentData {
			public Range guardTime = new Range(2f, 4f);
		}

		[CreateAssetMenu(menuName = "Behaviour/Attack/Guard Out Of Sight")]
		public class GuardOutOfSight : StateComponent {

			[StateTransitionTrigger]
			private static int onEnemyInRange = UnityEngine.Animator.StringToHash("onEnemyInRange");

			[StateTransitionTrigger]
			private static readonly int onCalmDown = UnityEngine.Animator.StringToHash("onCalmDown");


			//-----------------------------------------------------------------------
			private GuardOutOfSightData m_data;
			private float m_timer;


			//-----------------------------------------------------------------------
			public override StateComponentData CreateData() {
				return new GuardOutOfSightData();
			}

			public override System.Type GetDataType() {
				return typeof(GuardOutOfSightData);
			}

			protected override void OnInitialise() {
				m_data = m_pilot.GetComponentData<GuardOutOfSightData>();
				m_machine.SetSignal(Signals.Type.Alert, true);
			}

			protected override void OnEnter(State oldState, object[] param) {
				base.OnEnter(oldState, param);

				m_pilot.Stop();
				m_pilot.PressAction(Pilot.Action.Button_A);

				m_timer = m_data.guardTime.GetRandom();
			}

			protected override void OnExit(State _newState) {
				m_pilot.ReleaseAction(Pilot.Action.Button_A);
			}

			protected override void OnUpdate() {
				if (m_machine.GetSignal(Signals.Type.Danger)) {
					Transition(onEnemyInRange);
				} else {
					m_timer -= Time.deltaTime;
					if (m_timer <= 0f) {
						Transition(onCalmDown);
					}
				}
			}
		}
	}
}