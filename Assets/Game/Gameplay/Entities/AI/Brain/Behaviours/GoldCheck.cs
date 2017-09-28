using UnityEngine;
using System.Collections;

namespace AI {
	namespace Behaviour {
		
		[CreateAssetMenu(menuName = "Behaviour/Gold Check")]
		public class GoldCheck : StateComponent {
			
			[StateTransitionTrigger]
			protected static string IsGolden = "isGolden";

			[StateTransitionTrigger]
			protected static string NotGolden = "notGolden";


			protected override void OnUpdate() {
				Entity e = m_pilot.GetComponent<Entity>();
				if (e.isGolden) Transition(IsGolden);
				else 			Transition(NotGolden);				
			}
		}
	}
}