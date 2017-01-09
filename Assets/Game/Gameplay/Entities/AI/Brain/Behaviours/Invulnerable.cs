using UnityEngine;
using System.Collections;

namespace AI {
	namespace Behaviour {
		
		[CreateAssetMenu(menuName = "Behaviour/Invulnerable")]
		public class Invulnerable : StateComponent {
			
			protected override void OnEnter(State oldState, object[] param) {
				m_machine.SetSignal(Signals.Type.Invulnerable, true);				
			}

			protected override void OnExit(State _newState) {
				m_machine.SetSignal(Signals.Type.Invulnerable, false);
			}
		}
	}
}