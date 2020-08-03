using UnityEngine;
using System.Collections;

namespace AI {
	namespace Behaviour {		
		[CreateAssetMenu(menuName = "Behaviour/Actions/Activate Button A")]
		public class ActivateButtonA : StateComponent {			
			protected override void OnEnter(State oldState, object[] param) {
				m_pilot.PressAction(Pilot.Action.Button_A);
			}

			protected override void OnExit(State newState) {
				m_pilot.ReleaseAction(Pilot.Action.Button_A);
			}
		}
	}
}