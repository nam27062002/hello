using UnityEngine;
using System.Collections;

namespace AI {
	namespace Behaviour {
		[CreateAssetMenu(menuName = "Behaviour/Attack/Eat Target")]
		public class EatTarget : StateComponent {

			[StateTransitionTrigger]
			private static readonly int onBiteFail = UnityEngine.Animator.StringToHash("onBiteFail");
			[StateTransitionTrigger]
			private static readonly int onEndEating = UnityEngine.Animator.StringToHash("onEndEating");

			private EatBehaviour m_eatBehaviour;

			protected override void OnInitialise() {
				m_eatBehaviour = m_pilot.GetComponent<EatBehaviour>();
				m_eatBehaviour.enabled = false;
				m_eatBehaviour.onJawsClosed += OnBiteKillEvent;
				m_eatBehaviour.onEndEating += OnEndEatingEvent;
				base.OnInitialise();
			}

			protected override void OnEnter(State _oldState, object[] _param) {
				base.OnEnter(_oldState, _param);
				// Get Target!
				if ( _param.Length > 0 ){
					m_eatBehaviour.StartAttackTarget( _param[0] as Transform);	
				}else{
					m_eatBehaviour.StartAttackTarget( m_machine.enemy );
				}
				m_eatBehaviour.enabled = true;
			}

			protected override void OnExit(State _newState) {
				base.OnExit(_newState);
				if ( m_eatBehaviour )
					m_eatBehaviour.enabled = false;
			}

			protected void OnBiteKillEvent(){
				if (!m_eatBehaviour.IsEating()){
					// It failed on eating a prey -> Return
					Transition(onBiteFail);
				}
				// else -> wait to finish eating, stop pursuing
			}

			protected void OnEndEatingEvent(){
				Transition(onEndEating);
			}

			// Update -> Check if target still valid
			protected override void OnUpdate() {
				
			}
		}
	}
}