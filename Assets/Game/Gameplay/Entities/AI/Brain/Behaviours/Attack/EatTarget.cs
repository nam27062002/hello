using UnityEngine;
using System.Collections;

namespace AI {
	namespace Behaviour {
		[CreateAssetMenu(menuName = "Behaviour/Attack/Eat Target")]
		public class EatTaret : StateComponent {

			[StateTransitionTrigger]
			private static string OnBiteFail = "onBiteFail";
			[StateTransitionTrigger]
			private static string OnEndEating = "onEndEating";

			private EatBehaviour m_eatBehaviour;

			public override StateComponentData CreateData() {
				return new AttackMeleeData();
			}

			public override System.Type GetDataType() {
				return typeof(AttackMeleeData);
			}

			protected override void OnInitialise() {
				m_eatBehaviour = m_pilot.GetComponent<EatBehaviour>();
				m_eatBehaviour.enabled = false;
				m_eatBehaviour.onBiteKill += OnBiteKillEvent;
				m_eatBehaviour.onEndEating += OnEndEatingEvent;
				base.OnInitialise();
			}

			protected override void OnEnter(State _oldState, object[] _param) {
				base.OnEnter(_oldState, _param);
				// Get Target!
				m_eatBehaviour.StartAttackTarget( m_machine.enemy );
				m_eatBehaviour.enabled = true;
			}

			protected override void OnExit(State _newState) {
				base.OnExit(_newState);
				m_eatBehaviour.enabled = false;
			}

			protected void OnBiteKillEvent(){
				if (!m_eatBehaviour.IsEating()){
					// It failed on eating a prey -> Return
					Transition( OnBiteFail);
				}
				// else -> wait to finish eating
			}

			protected void OnEndEatingEvent(){
				Transition( OnEndEating);
			}

			// Update -> Check if target still valid
			protected override void OnUpdate() {
				
			}
		}
	}
}