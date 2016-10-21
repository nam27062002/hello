using UnityEngine;
using System.Collections;

namespace AI {
	namespace Behaviour {
		
		[CreateAssetMenu(menuName = "Behaviour/Timing")]
		public class Timing : StateComponent {
			
			[StateTransitionTrigger]
			protected static string OnTimeFinished = "onTimeFinished";

			protected float m_timer;

			protected override void OnEnter(State oldState, object[] param) {
				m_timer = (float)param[0];
			}

			protected override void OnUpdate() {
				m_timer -= Time.deltaTime;
				if (m_timer <= 0f) {
					Transition(OnTimeFinished);
				}
			}
		}
	}
}