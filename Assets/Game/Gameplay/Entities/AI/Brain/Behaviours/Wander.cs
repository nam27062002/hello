using UnityEngine;
using System.Collections;

namespace AI {
	namespace Behaviour {
		[System.Serializable]
		public class WanderData : StateComponentData {
			public float speed;
		}

		[CreateAssetMenu(menuName = "Behaviour/Wander")]
		public class Wander : StateComponent {

			private Vector3 m_target;

			public override StateComponentData CreateData() {
				return new WanderData();
			}

			protected override void OnInitialise() {
				m_target = m_machine.position;
			}

			protected override void OnEnter(State _oldState, object[] _param) {
				m_target = m_machine.position;
			}

			protected override void OnUpdate() {
				m_pilot.SetSpeed(1f); //TODO

				float m = (m_machine.position - m_target).sqrMagnitude;

				if (m < 1f * 1f) { // speed sqr
					if (m_pilot.guideFunction != null) {
						m_target = m_pilot.guideFunction.NextPositionAtSpeed(1f);
					} else {						
						m_target = Random.insideUnitSphere * 10f;
						m_target.z = 0;
					} 
					m_pilot.GoTo(m_target);
				}
			}
		}
	}
}