using UnityEngine;
using System.Collections;

namespace AI {
	namespace Behaviour {
		[System.Serializable]
		public class FollowLeaderData : StateComponentData {
			public float speed = 1f;
		}

		[CreateAssetMenu(menuName = "Behaviour/FollowLeader")]
		public class FollowLeader : StateComponent {
			

			public override StateComponentData CreateData() {
				return new FollowLeaderData();
			}

			protected override void OnUpdate() {
				m_pilot.SetSpeed(1f); //TODO

				IMachine leader = m_machine.GetGroup().leader;
				m_pilot.GoTo(leader.position - leader.direction * 0f); //TODO: serialize this!
			}
		}
	}
}