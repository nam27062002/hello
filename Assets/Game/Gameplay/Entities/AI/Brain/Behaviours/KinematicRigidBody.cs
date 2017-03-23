using UnityEngine;

namespace AI {
	namespace Behaviour {		
		[CreateAssetMenu(menuName = "Behaviour/Kinematic Rigid Body")]
		public class KinematicRigidBody : StateComponent {			
			protected override void OnEnter(State oldState, object[] param) {
				MachineOld m = m_machine as MachineOld;
				if (m != null) {
					m.isKinematic = true;
				}
			}

			protected override void OnExit(State _newState) {
				MachineOld m = m_machine as MachineOld;
				if (m != null) {
					m.isKinematic = false;
				}
			}
		}
	}
}