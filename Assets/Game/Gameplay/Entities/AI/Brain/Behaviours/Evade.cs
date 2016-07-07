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


			public override StateComponentData CreateData() {
				return new EvadeData();
			}

			protected override void OnEnter(State oldState, object[] param) {
				m_machine.SetSignal(Signals.Alert.name, true);
				m_pilot.SetBoostSpeed(20f);
			}

			protected override void OnExit(State newState) {
				m_machine.SetSignal(Signals.Alert.name, false);
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