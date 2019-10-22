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

			protected FollowLeaderData m_data;
			private Group m_group;

			private float m_offsetSQRMagnitude;
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
                m_group = m_machine.GetGroup();

				// m_group may be null, for example for a prey that belongs to a flock that has just been spawned in a pet's mouth
				// and it hasn't had the chance to join a group. 
				// This is just a workaround. A better approach would be to assign a specific state that doesn't have FollowLeader
				// as a StateComponent to a prey that only needs to stay in a pet's mouth
				if (m_group != null) {
					m_offsetSQRMagnitude = m_group.GetOffset (m_machine, 1f).sqrMagnitude;
				}

				m_pilot.SlowDown(false);							
				m_followState = FollowState.CatchUp;
			}

			protected override void OnUpdate() {
                CheckPromotion();

				if (m_group != null) {
					IMachine leader = m_group.leader;
					if (leader != null) {
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
								m_pilot.GoTo(leader.target);

								if (speedFactor <= 1f) {
									m_pilot.SlowDown(true);
									m_followState = FollowState.Follow;
								}
								break;
						}     
					}
                }
            }

			protected virtual void CheckPromotion() {
				if (m_machine.GetSignal(Signals.Type.Leader)) {
					Transition(SignalTriggers.onLeaderPromoted);
				}
			}

			private float ShouldCatchUp() {
				if (m_machine.GetSignal(Signals.Type.Warning)) {
					return 1f;
				} else {
					Group g = m_machine.GetGroup();
					IMachine leader = g.leader;
                    					
					float dSqr = (leader.target - m_machine.position).sqrMagnitude;
					if (m_offsetSQRMagnitude > 0f)
						return dSqr / m_offsetSQRMagnitude;
					else 
						return 1f;
				}
			}
		}
	}
}