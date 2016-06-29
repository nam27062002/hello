using UnityEngine;
using System.Collections;
using AISM;

namespace AI {
	namespace Behaviour {		
		[CreateAssetMenu(menuName = "Behaviour/Evade")]
		public class Evade : StateComponent {

			private Pilot m_pilot;
			private Machine m_machine;

			protected override void OnInitialise(GameObject _go) {
				m_pilot 	= _go.GetComponent<Pilot>();
				m_machine	= _go.GetComponent<Machine>();
			}

			protected override void OnEnter(State oldState, object[] param) {
				m_machine.SetSignal(Signals.Alert.name, true);				
			}

			protected override void OnExit(State newState) {
				m_machine.SetSignal(Signals.Alert.name, false);
			}

			protected override void OnUpdate() {
				m_pilot.Avoid(m_machine.GetSignal(Signals.Warning.name));
			}
		}
	}
}