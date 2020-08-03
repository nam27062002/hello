using UnityEngine;
using System.Collections;

namespace AI {
	namespace Behaviour {

		[CreateAssetMenu(menuName = "Behaviour/Jump")]
		public class Jump : StateComponent {

			[StateTransitionTrigger]
			private static readonly int onJumpTop = UnityEngine.Animator.StringToHash("onJumpTop");

            [StateTransitionTrigger]
			private static readonly int onJumpEnd = UnityEngine.Animator.StringToHash("onJumpEnd");

            private enum JumpState {
				GoingUp = 0,
				GoingDown,
				OnGround
			}

			private float m_lastY;
			private JumpState m_jumpState;

			protected override void OnEnter(State oldState, object[] param) {
				if (!m_pilot.IsActionPressed(Pilot.Action.Jump)) {
					m_pilot.PressAction(Pilot.Action.Jump);
					m_machine.SetVelocity(Vector3.up * 15f);
					m_jumpState = JumpState.GoingUp;
					m_lastY = m_machine.position.y;
				} else {
					m_jumpState = JumpState.GoingDown;
				}
			}

			protected override void OnUpdate() {
				if (m_pilot.IsActionPressed(Pilot.Action.Jump)) {
					if (m_jumpState == JumpState.GoingUp) {
						float y = m_machine.position.y;
						if (y < m_lastY) {
							m_jumpState = JumpState.GoingDown;
							Transition(onJumpTop);
						}
						m_lastY = y;
					}
				} else { // jump finished
					Transition(onJumpEnd);
				}
			}
		}
	}
}