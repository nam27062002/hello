using UnityEngine;
using System.Collections;

namespace AI {
	namespace Behaviour {
		[System.Serializable]
		public class OnActionPointData : StateComponentData {
			
		}

		[CreateAssetMenu(menuName = "Behaviour/On Action Point")]
		public class OnActionPoint : StateComponent {
			
			//[StateTransitionTrigger]
			//protected static string OnGoBackHome = "onGoBackHome";


			protected OnActionPointData m_data;

			private ActionPoint m_ap;
			private float m_timer;

			public override StateComponentData CreateData() {
				return new OnActionPointData();
			}

			public override System.Type GetDataType() {
				return typeof(OnActionPointData);
			}

			protected override void OnInitialise() {
				m_data = m_pilot.GetComponentData<OnActionPointData>();
			}

			protected override void OnEnter(State oldState, object[] param) {
				Actions.Action action = (Actions.Action)param[0];

				m_pilot.PressAction(Pilot.Action.Scared);
				m_pilot.Stop();

				m_ap = ActionPointManager.instance.GetActionPointAt(m_machine.transform.position);
				m_ap.Enter();

				if (action != null) {
					if (action.id == Actions.Id.Hide) {
						m_machine.SetSignal(Signals.Type.Invulnerable, true);
					}
				}
			}

			protected override void OnExit(State newState) {
				m_pilot.ReleaseAction(Pilot.Action.Scared);
				m_machine.SetSignal(Signals.Type.Invulnerable, false);
				m_ap.Leave();
			}

			protected override void OnUpdate() {
				if (m_machine.enemy != null) {
					if (m_machine.enemy.position.x < m_machine.position.x) {
						m_pilot.SetDirection(Vector3.left, true);
					} else {
						m_pilot.SetDirection(Vector3.right, true);
					}
				}
			}
		}
	}
}