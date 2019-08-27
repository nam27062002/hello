using UnityEngine;
using System.Collections;

namespace AI {
	namespace Behaviour {	
		[CreateAssetMenu(menuName = "Behaviour/Attack/Is Enemy Out Of Sight")]
		public class IsEnemyOutOfSight : StateComponent {
			
			[StateTransitionTrigger]
			private static readonly int onEnemyOutSight = UnityEngine.Animator.StringToHash("onEnemyOutSight");


			// The first element in _param must contain the amount of time without detecting an enemy
			protected override void OnEnter(State _oldState, object[] _param) {				
				m_machine.SetSignal(Signals.Type.Alert, true);
			}

			protected override void OnUpdate() {				
				if (!m_machine.GetSignal(Signals.Type.Warning)) {
					Transition(onEnemyOutSight);
				}
			}
		}
	}
}