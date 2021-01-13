using UnityEngine;
using System.Collections;

namespace AI {
	namespace Behaviour {
		
		[CreateAssetMenu(menuName = "Behaviour/Attack/Is Enemy Behind")]
		public class IsEnemyBehind : StateComponent {
			
			[StateTransitionTrigger]
			private static readonly int onTurnAround = UnityEngine.Animator.StringToHash("onTurnAround");

			[StateTransitionTrigger]
			private static readonly int onKeepDirection = UnityEngine.Animator.StringToHash("onKeepDirection");


			private DragonMotion m_dragon;


			protected override void OnEnter(State _oldState, object[] _param) {
				m_dragon = InstanceManager.player.dragonMotion;
			}

			protected override void OnUpdate() {
				Vector3 machineToDragon = m_dragon.position - m_machine.position;
				machineToDragon.Normalize();

				float dot = Vector3.Dot(m_machine.direction, machineToDragon);
				if (dot < -0.5f) {
					Transition(onTurnAround);
				} else {
					Transition(onKeepDirection);
				}
			}
		}
	}
}