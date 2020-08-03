using UnityEngine;

namespace AI {
	namespace Behaviour {		
		[CreateAssetMenu(menuName = "Behaviour/Stop")]
		public class Stop : StateComponent {			
			protected override void OnEnter(State oldState, object[] param) {
				m_pilot.Stop();
			}
		}
	}
}