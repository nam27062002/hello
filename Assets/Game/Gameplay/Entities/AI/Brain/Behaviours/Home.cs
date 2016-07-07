using UnityEngine;
using System.Collections;

namespace AI {
	namespace Behaviour {		
		[CreateAssetMenu(menuName = "Behaviour/Home")]
		public class Home : StateComponent {		
			private bool m_alertRestoreValue;



			protected override void OnEnter(State _oldState, object[] _param) {
				m_alertRestoreValue = m_machine.GetSignal(Signals.Alert.name);
				m_machine.SetSignal(Signals.Alert.name, false);
				m_pilot.GoTo(m_pilot.homePosition);
			}

			protected override void OnExit(State _newState) {
				m_machine.SetSignal(Signals.Alert.name, m_alertRestoreValue);
				m_machine.SetSignal(Signals.BackToHome.name, false);
			}

			protected override void OnUpdate() {
				float dSqr = (m_machine.position - m_pilot.homePosition).sqrMagnitude;
				if (dSqr < 0.1f) {
					m_machine.SetSignal(Signals.BackToHome.name, false);
				}
			}
		}
	}
}