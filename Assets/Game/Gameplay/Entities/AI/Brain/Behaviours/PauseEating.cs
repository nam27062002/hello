using UnityEngine;

namespace AI {
	namespace Behaviour {		
		[CreateAssetMenu(menuName = "Behaviour/Pause Eating")]
		public class PauseEating : StateComponent {		
			
			protected MachineEatBehaviour m_eatBehaviour;
			protected override void OnInitialise() {
				m_eatBehaviour = m_pilot.GetComponent<MachineEatBehaviour>();
			}
			protected override void OnEnter(State oldState, object[] param) {
				m_eatBehaviour.PauseEating();
			}
			protected override void OnExit(State _newState){
				m_eatBehaviour.ResumeEating();
			}
		}
	}
}