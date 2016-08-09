using UnityEngine;
using System.Collections;

namespace AI {
	namespace Behaviour {
		[CreateAssetMenu(menuName = "Behaviour/Attack/ChargeMelee")]
		public class AttackChargeMelee : AttackMelee {
			

			protected override void OnEnter(State oldState, object[] param) {
				base.OnEnter(oldState, param);
				m_pilot.PressAction(Pilot.Action.Button_A);
			}

			protected override void OnExit(State _newState) {
				base.OnExit(_newState);
				m_pilot.ReleaseAction(Pilot.Action.Button_A);
			}
		}
	}
}