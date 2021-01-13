using UnityEngine;
using System.Collections;

namespace AI {
	namespace Behaviour {		
		[CreateAssetMenu(menuName = "Behaviour/Actions/Activate Button B")]
		public class ActivateButtonB : StateComponent {
			protected override void OnEnter(State oldState, object[] param) {
				m_pilot.PressAction(Pilot.Action.Button_B);
			}

			protected override void OnExit(State newState) {
				m_pilot.ReleaseAction(Pilot.Action.Button_B);
			}
		}
	}
}