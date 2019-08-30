using UnityEngine;
using System.Collections;

namespace AI {
	namespace Behaviour {
		[System.Serializable]
		public class SpiderIdleData : IdleData {
			public Range hangDownChance = new Range(0.5f, 0.75f);
			public Range hangDownDistance = new Range(2f, 6f);
			public float hangDownSpeed = 2.5f;
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
			private float m_elapsedTime;
			private float m_timeInIdle;
			private float m_idleRandom;


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
				m_machine.UseGravity(true);

				m_pilot.SetDirection(Vector3.zero, false);
			}

			protected override void OnUpdate() {		
				float m = 0;
				float d = 1f;//((SpiderIdleData)m_data).hangDownSpeed * Time.deltaTime;

				if (m_idleState != IdleState.Normal) {
					m_elapsedTime += 5f * Time.deltaTime;
					m_pilot.SetMoveSpeed(((SpiderIdleData)m_data).hangDownSpeed);
					m_pilot.GoTo(m_target + Vector3.right * Mathf.Sin(m_elapsedTime) * 1f * m_idleRandom);
					m_pilot.SetDirection(Vector3.down, true);
				}

				switch (m_idleState) {
					case IdleState.Hang_down:
						m = (m_machine.position - m_target).sqrMagnitude;
						if (m <= d * d) {
							ChangeState(IdleState.Hang_idle);
						}
						break;

					case IdleState.Hang_idle:
						m_timer -= Time.deltaTime;
						if (m_timer <= 0f || m_machine.GetSignal(Signals.Type.Danger)) {
							ChangeState(IdleState.Hang_up);
						}
						break;

					case IdleState.Hang_up: 
						m = (m_machine.position - m_target).sqrMagnitude;
						if (m <= d * d) {
							m_machine.upVector = Vector3.down;
							Transition(onMove);
						}
						break;

					case IdleState.Normal:
						m_timer -= Time.deltaTime;
						if (m_timer <= 0f || m_machine.GetSignal(Signals.Type.Danger)) {
							Transition(onMove);
						}
						break;

				}
			}


			private void ChangeState(IdleState _state) {
				switch (_state) {
					case IdleState.Hang_down:
						m_pilot.PressAction(Pilot.Action.Button_A);
						m_machine.UseGravity(false);
						m_machine.upVector = Vector3.up;

						m_target = m_machine.position + Vector3.down * ((SpiderIdleData)m_data).hangDownDistance.GetRandom();
						m_pilot.SetMoveSpeed(((SpiderIdleData)m_data).hangDownSpeed);
						m_pilot.GoTo(m_target);
						m_pilot.SetDirection(Vector3.down, true);
						break;

					case IdleState.Hang_idle:
						m_machine.UseGravity(false);
						m_pilot.SetDirection(Vector3.down, true);
						m_pilot.SetMoveSpeed(0f, false);
						m_timeInIdle = m_data.restTime.GetRandom();
						m_timer = m_timeInIdle;
						m_idleRandom = (Random.Range(0f, 1f) < 0.5f)? -1 : 1;
						m_elapsedTime = 0f;
						break;

					case IdleState.Hang_up:
						m_machine.UseGravity(false);
						m_pilot.SetDirection(Vector3.down, true);
						m_target = m_startPosition;
						m_pilot.SetMoveSpeed(((SpiderIdleData)m_data).hangDownSpeed);
						m_pilot.GoTo(m_target);
						break;

					case IdleState.Normal:
						m_target = m_startPosition;
						m_pilot.Stop();

						float side = 1f;
						if (Random.Range(0f, 1f) > 0.5f) {
							side = -1f;
						}

						Quaternion rotation = Quaternion.AngleAxis(side * Random.Range(90f, 180f), m_machine.upVector);
						Vector3 forward = Vector3.Cross(m_machine.upVector, Vector3.right);
						m_pilot.SetDirection(rotation * forward, true);
						break;
				}

				m_idleState = _state;
			}
		}
	}
}