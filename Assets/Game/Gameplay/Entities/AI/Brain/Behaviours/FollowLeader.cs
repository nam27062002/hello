using UnityEngine;
using System.Collections;

namespace AI {
	namespace Behaviour {
		[System.Serializable]
		public class FollowLeaderData : StateComponentData {
			public float speed = 1f;
			public float catchUpSpeed = 10f;
		}

		[CreateAssetMenu(menuName = "Behaviour/FollowLeader")]
		public class FollowLeader : StateComponent {

			private enum FollowState {
				Follow = 0,
				CatchUp
			}

			private FollowLeaderData m_data;

			private Vector3 m_offset;

			private FollowState m_followState;


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
				m_pilot.SlowDown(false);
				m_followState = FollowState.CatchUp;
			}

			protected override void OnUpdate() {
				if (m_machine.GetSignal(Signals.Type.Leader)) {
					Transition(SignalTriggers.OnLeaderPromoted);
				}

				IMachine leader = m_machine.GetGroup().leader;
				switch (m_followState) {
					case FollowState.Follow:
						m_pilot.SetMoveSpeed(m_data.speed);
						m_pilot.GoTo(leader.target);
						if (ShouldCatchUp() > 1f) {
							m_pilot.SlowDown(true);
							m_followState = FollowState.CatchUp;
						}
						break;

					case FollowState.CatchUp:
						float speedFactor = ShouldCatchUp();

						m_pilot.SetMoveSpeed(Mathf.Min(m_data.catchUpSpeed, m_data.speed * speedFactor));
						m_pilot.GoTo(leader.position);

						if (speedFactor <= 1f) {
							m_pilot.SlowDown(true);
							m_followState = FollowState.Follow;
						}
						break;
				}     
			}

			private float ShouldCatchUp() {
				if (m_machine.GetSignal(Signals.Type.Warning)) {
					return 1f;
				} else {
					Group g = m_machine.GetGroup();
					IMachine leader = g.leader;
					Vector3 offset = g.GetOffset(m_pilot.m_machine, 1f);

					float dSqr = (leader.position - m_machine.position).sqrMagnitude;
					if (offset.sqrMagnitude > 0f)
						return dSqr / offset.sqrMagnitude;
					else 
						return 1f;
				}
			}
		}
	}
}