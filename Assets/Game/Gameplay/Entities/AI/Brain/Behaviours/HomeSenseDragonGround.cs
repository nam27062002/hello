using UnityEngine;
using System.Collections;

namespace AI {
	namespace Behaviour {
		[CreateAssetMenu(menuName = "Behaviour/Home Sense Dragon Ground")]
		public class HomeSenseDragonGround : HomeSenseDragon {
			protected override void OnUpdate() {
				m_pilot.GoTo(m_pilot.homePosition);

				float dX = Mathf.Abs(m_machine.position.x - m_pilot.homePosition.x);
				if (dX < 2f) {
					Transition(OnBackAtHome);
				}
			}
		}
	}
}