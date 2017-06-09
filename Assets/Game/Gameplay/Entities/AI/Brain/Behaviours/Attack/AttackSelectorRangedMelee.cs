using UnityEngine;

namespace AI {
	namespace Behaviour {
		[CreateAssetMenu(menuName = "Behaviour/Attack/Attack Selector Ranged Melee")]
		public class AttackSelectorRangedMelee : StateComponent {

			[StateTransitionTrigger]
			private static string OnRanged = "onRanged";

			[StateTransitionTrigger]
			private static string OnMelee = "onMelee";

			[StateTransitionTrigger]
			private static string OnEnemyOutOfSight = "onEnemyOutOfSight";



			protected override void OnUpdate() {
				if (m_machine.GetSignal(Signals.Type.Critical)) {
					Transition(OnMelee);
				} else if (m_machine.GetSignal(Signals.Type.Danger)) {
					Transition(OnRanged);
				} else {
					Transition(OnEnemyOutOfSight);
				}
			}
		}
	}
}