using UnityEngine;
using System.Collections;

namespace AI {
	namespace Behaviour {
		[CreateAssetMenu(menuName = "Behaviour/Home Sense Dragon Ground")]
		public class HomeSenseDragonGround : HomeSenseDragon {
			protected override void OnUpdate() {				
				Vector3 direction = m_machine.groundDirection;
				direction.z = 0f;

				float m_side = 1f;
				if (m_pilot.homePosition.x < m_machine.position.x) {
					m_side = -1f;
				}

				m_pilot.GoTo(m_machine.position + direction * m_side * 1.5f);

				//
				float dX = Mathf.Abs(m_machine.position.x - m_pilot.homePosition.x);
				if (dX < 2f) {
					Transition(onBackAtHome);
				}
			}
		}
	}
}