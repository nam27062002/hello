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
			private Vector3 m_oldTarget;

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
                m_pilot.SlowDown(true);
				m_oldTarget = m_machine.position;

				m_followState = FollowState.CatchUp;
			}

			protected override void OnUpdate() {
				if (m_machine.GetSignal(Signals.Type.Leader)) {
					Transition(SignalTriggers.OnLeaderPromoted);
				}

				switch (m_followState) {
					case FollowState.Follow:
						m_pilot.SetMoveSpeed(m_data.speed);
						if (ShouldCatchUp()) {
							m_followState = FollowState.CatchUp;
						}
						break;

					case FollowState.CatchUp:
						m_pilot.SetMoveSpeed(m_data.catchUpSpeed);
						if (!ShouldCatchUp()) {
							m_followState = FollowState.Follow;
						}
						break;
				}				               

				IMachine leader = m_machine.GetGroup().leader;
				Vector3 target = leader.target;

				m_pilot.GoTo(Vector3.Lerp(m_oldTarget, target, Time.smoothDeltaTime * 0.25f));
				m_oldTarget = target;
			}

			private bool ShouldCatchUp() {
				if (m_machine.GetSignal(Signals.Type.Warning)) {
					return false;
				} else {
					float dSqr = (m_pilot.target - m_machine.position).sqrMagnitude;
					float dDistInc = m_pilot.speed * Time.deltaTime * 2f;

					return dSqr > dDistInc * dDistInc;
				}
			}
		}
	}
}