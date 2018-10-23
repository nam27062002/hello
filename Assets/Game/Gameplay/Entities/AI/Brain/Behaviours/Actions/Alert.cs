using UnityEngine;
using System.Collections;

namespace AI {
	namespace Behaviour {
		
		[CreateAssetMenu(menuName = "Behaviour/Actions/Alert")]
        public class Alert : StateComponent {
			
			protected override void OnEnter(State oldState, object[] param) {
                m_machine.SetSignal(Signals.Type.Alert, true);				
			}

			protected override void OnExit(State _newState) {
                m_machine.SetSignal(Signals.Type.Alert, false);
			}
		}
	}
}