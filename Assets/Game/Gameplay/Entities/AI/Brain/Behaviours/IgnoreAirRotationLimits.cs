using UnityEngine;
using System.Collections;

namespace AI {
	namespace Behaviour {
		[CreateAssetMenu(menuName = "Behaviour/Ignore Air Rotation Limits")]
		public class IgnoreAirRotationLimits : StateComponent {

			AI.MachineAir m_airMachine;
			bool m_horizontalValue;
			bool m_verticalValue;

			protected override void OnInitialise() {
				m_airMachine = m_machine as MachineAir;
			}

			protected override void OnEnter(State oldState, object[] param) {
				m_horizontalValue = m_airMachine.limitHorizontalRotation;
				m_airMachine.limitHorizontalRotation = false;

				m_verticalValue = m_airMachine.limitVerticalRotation;
				m_airMachine.limitVerticalRotation = false;
			}

			protected override void OnExit(State _newState){
				m_airMachine.limitHorizontalRotation = m_horizontalValue;
				m_airMachine.limitVerticalRotation = m_verticalValue;
			}

		}
	}
}