using UnityEngine;
using System.Collections;

namespace AI {
	namespace Behaviour {
		
		[CreateAssetMenu(menuName = "Behaviour/Gold Check")]
		public class GoldCheck : StateComponent {
			
			[StateTransitionTrigger]
			protected static readonly int isGolden = UnityEngine.Animator.StringToHash("isGolden");

			[StateTransitionTrigger]
			protected static readonly int notGolden = UnityEngine.Animator.StringToHash("notGolden");


			protected override void OnUpdate() {
				Entity e = m_pilot.GetComponent<Entity>();
				if (e.isGolden) Transition(isGolden);
				else 			Transition(notGolden);				
			}
		}
	}
}