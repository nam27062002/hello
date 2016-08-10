using UnityEngine;
using System.Collections;

namespace AI {
	namespace Behaviour {
		[System.Serializable]
		public class SpiderIdleData : IdleData {
			public Range hangDownChance = new Range(0.5f, 0.75f);
			public Range hangDownDistance = new Range(2f, 6f);
		}

		[CreateAssetMenu(menuName = "Behaviour/Spider/Idle")]
		public class SpiderIdle : AI.Behaviour.Idle {

			protected enum IdleState {
				Normal = 0,
				Hang_down,
				Hang_idle,
				Hang_up
			}

			private Vector3 m_startPosition;
			private Vector3 m_target;

			private IdleState m_idleState;


			public override StateComponentData CreateData() {
				return new SpiderIdleData();
			}

			public override System.Type GetDataType() {
				return typeof(SpiderIdleData);
			}

			protected override void OnInitialise() {
				m_data = m_pilot.GetComponentData<SpiderIdleData>();
			}

			protected override void OnEnter(AI.State oldState, object[] param) {
				base.OnEnter(oldState, param);

				m_startPosition = m_machine.position;

				float dot = Vector3.Dot(Vector3.down, m_machine.upVector);
				if (dot >= 0.6f && dot <= 1f && Random.Range(0f, 1f) <= ((SpiderIdleData)m_data).hangDownChance.GetRandom()) {
					ChangeState(IdleState.Hang_down);
				} else {
					ChangeState(IdleState.Normal);
				}
			}

			protected override void OnExit(AI.State _newState) {
				m_pilot.ReleaseAction(Pilot.Action.Button_A);
				m_machine.StickToCollisions(true);

				m_pilot.SetDirection(Vector3.zero, false);
			}

			protected override void OnUpdate() {		
				float m = 0;

				switch (m_idleState) {
					case IdleState.Hang_down:
						m = (m_machine.position - m_target).sqrMagnitude;
						if (m < 0.5f * 0.5f) {
							ChangeState(IdleState.Hang_idle);
						}
						break;

					case IdleState.Hang_idle:
						m_timer -= Time.deltaTime;
						if (m_timer <= 0f) {
							ChangeState(IdleState.Hang_up);
						}
						break;

					case IdleState.Hang_up:
						m = (m_machine.position - m_target).sqrMagnitude;
						if (m < 0.5f * 0.5f) {
							Transition(OnMove);
						}
						break;

					case IdleState.Normal:
						m_timer -= Time.deltaTime;
						if (m_timer <= 0f) {
							Transition(OnMove);
						}
						break;

				}
			}


			private void ChangeState(IdleState _state) {
				switch (_state) {
					case IdleState.Hang_down:
						m_pilot.PressAction(Pilot.Action.Button_A);
						m_machine.StickToCollisions(false);
						m_machine.upVector = Vector3.up;

						m_target = m_machine.position + Vector3.down * ((SpiderIdleData)m_data).hangDownDistance.GetRandom();
						m_pilot.SetMoveSpeed(2.5f);
						m_pilot.GoTo(m_target);
						m_pilot.SetDirection(Vector3.down, true);
						break;

					case IdleState.Hang_idle:
						m_pilot.SetMoveSpeed(0f, false);
						m_timer = m_data.restTime.GetRandom();
						break;

					case IdleState.Hang_up:
						m_target = m_startPosition;
						m_pilot.SetMoveSpeed(2.5f);
						m_pilot.GoTo(m_target);
						break;

					case IdleState.Normal:
						m_target = m_startPosition;
						m_pilot.SetMoveSpeed(0f, false);
						m_pilot.SetDirection(new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-0.5f, -1f)), true);
						break;
				}

				m_idleState = _state;
			}
		}
	}
}