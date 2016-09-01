using UnityEngine;
using System.Collections;

namespace AI {
	namespace Behaviour {
		[System.Serializable]
		public class PursuitData : StateComponentData {
			public float speed;
			public float arrivalRadius = 1f;
			public string attackPoint;
		}

		[CreateAssetMenu(menuName = "Behaviour/Attack/Pursuit")]
		public class Pursuit : StateComponent {

			[StateTransitionTrigger]
			private static string OnEnemyInRange = "onEnemyInRange";

			[StateTransitionTrigger]
			private static string OnEnemyOutOfSight = "onEnemyOutOfSight";

			private enum PursuitState {
				Move_Towards = 0,
				Move_Away
			};

			protected PursuitData m_data;
			protected Transform m_target;
			protected AI.Machine m_targetMachine;

			private PursuitState m_pursuitState;
			private object[] m_transitionParam;

			public override StateComponentData CreateData() {
				return new PursuitData();
			}

			public override System.Type GetDataType() {
				return typeof(PursuitData);
			}

			protected override void OnInitialise() {
				m_data = m_pilot.GetComponentData<PursuitData>();

				m_machine.SetSignal(Signals.Type.Alert, true);
				m_transitionParam = new object[1];
				m_target = null;
			}

			protected override void OnEnter(State oldState, object[] param) {
				m_pilot.SetMoveSpeed(m_data.speed);
				m_pilot.SlowDown(true);

				m_target = null;
				m_targetMachine = null;

				if ( param != null && param.Length > 0 )
				{
					m_target = param[0] as Transform;
					if ( m_target )
						m_targetMachine = m_target.GetComponent<Machine>();
				}

				if (m_target == null && m_machine.enemy != null) {
					m_target = m_machine.enemy.FindTransformRecursive(m_data.attackPoint);
					if (m_target == null) {
						m_target = m_machine.enemy;
					}

					m_targetMachine = m_machine.enemy.GetComponent<Machine>();
				}

				m_pursuitState = PursuitState.Move_Towards;
			}

			protected override void OnUpdate() {	
				if (m_targetMachine != null) {
					if ( m_targetMachine.IsDead() || m_targetMachine.IsDying()) {
						m_target = null;
						m_targetMachine = null;
					}
				} else {
					if (!m_machine.GetSignal(Signals.Type.Warning)) {
						m_target = null;
					}
				}
									
				if (m_target != null && m_target.gameObject.activeInHierarchy) {

					if (m_pursuitState == PursuitState.Move_Towards) {
						if (m_machine.GetSignal(Signals.Type.Critical)) {
							ChangeState(PursuitState.Move_Away);
						} else {
							float m = (m_machine.position - m_target.position).sqrMagnitude;
							if (m < m_data.arrivalRadius * m_data.arrivalRadius) {
								m_transitionParam[0] = m_target;
								Transition(OnEnemyInRange, m_transitionParam);
							} else {
								m_pilot.GoTo(m_target.position);
							}
						}
					} else if (m_pursuitState == PursuitState.Move_Away) {
						if (m_machine.GetSignal(Signals.Type.Critical)) {
							// Player is inside our Critical area and we can't attack it from here, me should move back a bit
							Vector3 direction = m_machine.direction;
							Vector3 target = m_machine.position + direction * m_data.speed;

							if (!m_pilot.area.Contains(target)) {
								//change direction
								target = m_machine.position + direction * m_data.speed * -1;
							}

							m_pilot.GoTo(target);
						} else {
							ChangeState(PursuitState.Move_Towards);
						}
					}
				} else {
					Transition(OnEnemyOutOfSight);
				}
			}

			private void ChangeState(PursuitState _newState) {
				if (_newState != m_pursuitState) {
					if (_newState == PursuitState.Move_Away) {
						Vector3 direction = (m_machine.position - m_target.position).normalized;
						Vector3 target = m_machine.position + direction * m_data.speed;
						m_pilot.GoTo(target);
					}

					m_pursuitState = _newState;
				}
			}
		}
	}
}