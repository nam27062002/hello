using UnityEngine;

namespace AI {
	namespace Behaviour {		
		[CreateAssetMenu(menuName = "Behaviour/Kinematic Rigid Body")]
		public class KinematicRigidBody : StateComponent {			
			protected override void OnEnter(State oldState, object[] param) {
				Machine m = m_machine as Machine;
				if (m != null) {
					m.isKinematic = true;
				}
			}

			protected override void OnExit(State _newState) {
				Machine m = m_machine as Machine;
				if (m != null) {
					m.isKinematic = false;
				}
			}
		}
	}
}