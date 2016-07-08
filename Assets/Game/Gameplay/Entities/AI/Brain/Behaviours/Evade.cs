using UnityEngine;
using System.Collections;

namespace AI {
	namespace Behaviour {
		[System.Serializable]
		public class EvadeData : StateComponentData {
			public float boostSpeed = 5f;
		}

		[CreateAssetMenu(menuName = "Behaviour/Evade")]
		public class Evade : StateComponent {

			private EvadeData m_data;
			private bool m_alertRestoreValue;

			public override StateComponentData CreateData() {
				return new EvadeData();
			}

			protected override void OnInitialise() {
				m_data = (EvadeData)m_pilot.GetComponentData<Evade>();
			}

			protected override void OnEnter(State oldState, object[] param) {
				m_alertRestoreValue = m_machine.GetSignal(Signals.Alert.name);
				m_machine.SetSignal(Signals.Alert.name, true);
				m_pilot.SetBoostSpeed(m_data.boostSpeed);
			}

			protected override void OnExit(State newState) {
				m_machine.SetSignal(Signals.Alert.name, m_alertRestoreValue);
				m_pilot.Avoid(false);
				m_pilot.ReleaseAction(Pilot.Action.Boost);
			}

			protected override void OnUpdate() {
				bool avoid = m_machine.GetSignal(Signals.Warning.name);
				m_pilot.Avoid(avoid);

				if (avoid) {
					m_pilot.PressAction(Pilot.Action.Boost);
				} else {
					m_pilot.ReleaseAction(Pilot.Action.Boost);
				}
			}
		}
	}
}