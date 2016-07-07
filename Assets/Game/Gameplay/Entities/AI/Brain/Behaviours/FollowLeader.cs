using UnityEngine;
using System.Collections;
using AISM;

namespace AI {
	namespace Behaviour {
		[System.Serializable]
		public class FollowLeaderData : StateComponentData {
			public float speed = 1f;
		}

		[CreateAssetMenu(menuName = "Behaviour/FollowLeader")]
		public class FollowLeader : StateComponent {

			private AIPilot m_pilot;
			private Machine m_machine;


			public override StateComponentData CreateData() {
				return new FollowLeaderData();
			}

			protected override void OnInitialise(GameObject _go) {
				m_pilot 	= _go.GetComponent<AIPilot>();
				m_machine	= _go.GetComponent<Machine>();
			}

			protected override void OnUpdate() {
				m_pilot.SetSpeed(1f); //TODO

				IMachine leader = m_machine.GetGroup().leader;
				m_pilot.GoTo(leader.position - leader.direction * 0f); //TODO: serialize this!
			}
		}
	}
}