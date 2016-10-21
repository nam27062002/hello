using UnityEngine;
using System.Collections;

namespace AI {
	namespace Behaviour {
		[System.Serializable]
		public class FollowLeaderData : StateComponentData {
			public float speed = 1f;
			public float followAheadFactor = 0f;
			public Range frequencyOffset = new Range(0f, 1f);
		}

		[CreateAssetMenu(menuName = "Behaviour/FollowLeader")]
		public class FollowLeader : StateComponent {

			private FollowLeaderData m_data;

			private float m_time;
			private float m_frequencyOffset;
			private Vector3 m_offset;
			private Vector3 m_oldTarget;

			public override StateComponentData CreateData() {
				return new FollowLeaderData();
			}

			public override System.Type GetDataType() {
				return typeof(FollowLeaderData);
			}

			protected override void OnInitialise() {
				m_data = m_pilot.GetComponentData<FollowLeaderData>();
			}

			protected override void OnEnter(State _oldState, object[] _param) {
				m_time = 0;
				m_frequencyOffset = m_data.frequencyOffset.GetRandom();
				m_offset = Random.onUnitSphere * m_data.followAheadFactor;
				m_pilot.SlowDown(true);
				m_oldTarget = m_machine.position;
			}

			protected override void OnUpdate() {
				m_pilot.SetMoveSpeed(m_data.speed);

				IMachine leader = m_machine.GetGroup().leader;
				Vector3 target = leader.target + m_offset;

				m_time += Time.smoothDeltaTime;
				target.y += Mathf.Cos(m_frequencyOffset + m_time);
				target.z += Mathf.Sin(m_frequencyOffset + m_time) * 0.5f;

				m_pilot.GoTo(Vector3.Lerp(m_oldTarget, target, Time.smoothDeltaTime));
				m_oldTarget = target;
			}
		}
	}
}