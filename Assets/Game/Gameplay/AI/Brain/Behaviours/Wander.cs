using UnityEngine;
using System.Collections;
using AISM;

namespace AI {
	namespace Behaviour {		
		[CreateAssetMenu(menuName = "Behaviour/Wander")]
		public class Wander : StateComponent {

			private Vector3 m_target;

			private Pilot m_pilot;
			private Machine m_machine;

			protected override void OnInitialise(GameObject _go) {
				m_pilot 	= _go.GetComponent<Pilot>();
				m_machine	= _go.GetComponent<Machine>();
			}

			protected override void OnUpdate() {
				m_pilot.SetSpeed(1f); //TODO

				float m = (m_machine.position - m_target).sqrMagnitude;

				if (m < 0.1f) {
					m_target = Random.insideUnitSphere * 10f;
					m_target.z = 0;
				} 

				m_pilot.GoTo(m_target);
			}
		}
	}
}