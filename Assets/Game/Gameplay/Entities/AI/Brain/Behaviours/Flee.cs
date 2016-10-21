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

			private FleeData m_data;

			private Vector3 m_target;

			public override StateComponentData CreateData() {
				return new FleeData();
			}

			public override System.Type GetDataType() {
				return typeof(FleeData);
			}

			protected override void OnInitialise() {
				m_data = m_pilot.GetComponentData<FleeData>();

				m_machine.SetSignal(Signals.Type.Alert, true);
			}

			protected override void OnEnter(State oldState, object[] param) {
				m_pilot.SetMoveSpeed(m_data.speed);
				m_pilot.SetBoostSpeed(m_data.speed);
				m_target = m_machine.position;
				m_pilot.PressAction(Pilot.Action.Avoid);
			}

			protected override void OnExit(State newState) {
				m_pilot.ReleaseAction(Pilot.Action.Boost);
				m_pilot.ReleaseAction(Pilot.Action.Avoid);
			}

			protected override void OnUpdate() {
				bool boost = m_machine.GetSignal(Signals.Type.Danger);

				if (boost) {
					m_pilot.PressAction(Pilot.Action.Boost);
				} else {
					m_pilot.ReleaseAction(Pilot.Action.Boost);
				}

				Transform enemy = m_machine.enemy;
				if (enemy != null) {
					
					Vector3 moveAway = m_machine.position - enemy.position;
					moveAway.z = 0f; // lets keep the current Z
					moveAway.Normalize();
					moveAway *= m_data.boostSpeed;

					m_pilot.AddImpulse(moveAway);
				}
			}
		}
	}
}