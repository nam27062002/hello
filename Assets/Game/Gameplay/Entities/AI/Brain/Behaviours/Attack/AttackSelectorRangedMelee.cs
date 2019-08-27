using UnityEngine;

namespace AI {
	namespace Behaviour {
		[CreateAssetMenu(menuName = "Behaviour/Attack/Attack Selector Ranged Melee")]
		public class AttackSelectorRangedMelee : StateComponent {

			[StateTransitionTrigger]
			private static readonly int onRanged = UnityEngine.Animator.StringToHash("onRanged");

            [StateTransitionTrigger]
			private static readonly int onMelee = UnityEngine.Animator.StringToHash("onMelee");

            [StateTransitionTrigger]
			private static readonly int onEnemyOutOfSight = UnityEngine.Animator.StringToHash("onEnemyOutOfSight");



            protected override void OnUpdate() {
				if (m_machine.GetSignal(Signals.Type.Critical)) {
					Transition(onMelee);
				} else if (m_machine.GetSignal(Signals.Type.Danger)) {
					Transition(onRanged);
				} else {
					Transition(onEnemyOutOfSight);
				}
			}
		}
	}
}