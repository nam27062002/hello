using UnityEngine;
using System.Collections;

namespace AI {
	namespace Behaviour {
		[System.Serializable]
		public class ActivateActionPeriodicallyData : StateComponentData {
			public Pilot.Action action = Pilot.Action.Boost;
			public float time = 5f;
			public float duration = 1f;
		}
		
		[CreateAssetMenu(menuName = "Behaviour/Actions/Activate Action Periodically")]
		public class ActivateActionPeriodically : StateComponent {

			private ActivateActionPeriodicallyData m_data;

			private float m_timer;
			private bool m_active;


			public override StateComponentData CreateData() {
				return new ActivateActionPeriodicallyData();
			}

			public override System.Type GetDataType() {
				return typeof(ActivateActionPeriodicallyData);
			}

			protected override void OnInitialise() {
				m_data = m_pilot.GetComponentData<ActivateActionPeriodicallyData>();
			}

			protected override void OnEnter(State oldState, object[] param) {
				m_pilot.ReleaseAction(m_data.action);

				m_timer = 0f;//m_data.time;
				m_active = false;
			}

			protected override void OnExit(State newState) {
				m_pilot.ReleaseAction(m_data.action);
			}

			protected override void OnUpdate() {
				m_timer -= Time.deltaTime;
				if (m_timer <= 0f) {
					if (m_active) {
						m_pilot.ReleaseAction(m_data.action);

						m_timer = m_data.time;
						m_active = false;
					} else {
						m_pilot.PressAction(m_data.action);

						m_timer = m_data.duration;
						m_active = true;
					}
				}
			}
		}
	}
}