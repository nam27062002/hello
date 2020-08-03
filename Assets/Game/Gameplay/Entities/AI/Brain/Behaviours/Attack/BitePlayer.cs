using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace AI {
	namespace Behaviour {
		[CreateAssetMenu(menuName = "Behaviour/Attack/Bite Player")]
		public class BitePlayer : StateComponent {

			[StateTransitionTrigger]
			private static int onPlayerBitted = UnityEngine.Animator.StringToHash("onPlayerBitted");

			private EatBehaviour m_eatBehaviour;
			private bool m_bit = false;

			protected override void OnInitialise() {
				m_eatBehaviour = m_pilot.GetComponent<EatBehaviour>();
				m_eatBehaviour.enabled = false;
			}

			protected override void OnEnter(State oldState, object[] param) {
				m_eatBehaviour.enabled = true;
				m_eatBehaviour.onBitePlayer += OnBiteKillEvent;
				m_eatBehaviour.canLatchOnPlayer = false;
				m_eatBehaviour.canBitePlayer = true;
				m_bit = false;
			}

			protected override void OnExit(State _newState) {
				m_eatBehaviour.enabled = false;
				m_eatBehaviour.onBitePlayer -= OnBiteKillEvent;
			}

			void OnBiteKillEvent() {
				m_bit = true;
			}

			protected override void OnUpdate() {
				if (m_bit)	{
					Transition(onPlayerBitted);
					m_bit = false;
					return;
				}

			}
		}
	}
}