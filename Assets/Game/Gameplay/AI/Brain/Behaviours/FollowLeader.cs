using UnityEngine;
using System.Collections;
using AISM;

namespace AI {
	namespace Behaviour {
		[CreateAssetMenu(menuName = "Behaviour/FollowLeader")]
		public class FollowLeader : StateComponent {

			private Pilot m_pilot;
			private Machine m_machine;

			protected override void OnInitialise(GameObject _go) {
				m_pilot 	= _go.GetComponent<Pilot>();
				m_machine	= _go.GetComponent<Machine>();
			}

			protected override void OnUpdate() {
				IMachine leader = m_machine.GetGroup().leader;
				m_pilot.GoTo(leader.position - leader.direction * 0.125f); //TODO: serialize this!
			}
		}
	}
}