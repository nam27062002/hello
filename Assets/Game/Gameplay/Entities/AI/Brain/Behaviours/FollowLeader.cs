using UnityEngine;
using System.Collections;

namespace AI {
	namespace Behaviour {
		[System.Serializable]
		public class FollowLeaderData : StateComponentData {
			public float speed = 1f;
			public Range frequencyOffset = new Range(0f, 1f);
		}

		[CreateAssetMenu(menuName = "Behaviour/FollowLeader")]
		public class FollowLeader : StateComponent {

			private FollowLeaderData m_data;

			private float m_time;
			private float m_frequencyOffset;

			public override StateComponentData CreateData() {
				return new FollowLeaderData();
			}

			protected override void OnInitialise() {
				m_data = (FollowLeaderData)m_pilot.GetComponentData<FollowLeader>();
			}

			protected override void OnEnter(State _oldState, object[] _param) {
				m_time = 0;
				m_frequencyOffset = m_data.frequencyOffset.GetRandom();
			}

			protected override void OnUpdate() {
				m_pilot.SetSpeed(m_data.speed);

				IMachine leader = m_machine.GetGroup().leader;
				Vector3 target = leader.position - leader.direction * 0f;

				m_time += Time.smoothDeltaTime;
				target.y += Mathf.Cos(m_frequencyOffset + m_time);
				target.z += Mathf.Sin(m_frequencyOffset + m_time) * 0.5f;

				m_pilot.GoTo(target);
			}
		}
	}
}