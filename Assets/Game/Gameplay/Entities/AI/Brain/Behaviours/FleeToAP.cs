using UnityEngine;
using System.Collections;

namespace AI {
	namespace Behaviour {		
		[System.Serializable]
		public class FleeToAPData : StateComponentData {
			public float speed = 1f;
			public float checkDragonPositionTime = 2f;
			public float checkForActionPointTime = 1f;
			public Actions actions;
		}

		[CreateAssetMenu(menuName = "Behaviour/Flee to Action Point")]
		public class FleeToAP : StateComponent {

			[StateTransitionTrigger]
			private static readonly int onActionPoint = UnityEngine.Animator.StringToHash("onActionPoint");

            [StateTransitionTrigger]
			private static readonly int onGoBackHome = UnityEngine.Animator.StringToHash("onGoBackHome");

            [StateTransitionTrigger]
			private static readonly int onIdleAlert = UnityEngine.Animator.StringToHash("onIdleAlert");


            private enum FleeState {
				Flee = 0,
				Flee_Panic,
				Panic,
				Slow_Down,
				Slow_Down_Panic
			}


			private FleeToAPData m_data;

			private FleeState m_fleeState;

			private Vector3 m_target;
			private Vector3 m_lastPos;
			private int m_runDirection;
			private float m_timeStuck;

			private float m_dragonVisibleTimer;
			private float m_dragonPositionTimer;
			private float m_actionPointTimer;
			private float m_panicTimer;

			private object[] m_params;
					

			public override StateComponentData CreateData() {
				return new FleeToAPData();
			}

			public override System.Type GetDataType() {
				return typeof(FleeToAPData);
			}

			protected override void OnInitialise() {
				m_data = m_pilot.GetComponentData<FleeToAPData>();

				m_machine.SetSignal(Signals.Type.Alert, true);

				m_target = m_machine.position;

				m_params = new object[1];
			}

			protected override void OnEnter(State oldState, object[] param) {
				m_pilot.SetMoveSpeed(m_data.speed);
				m_dragonVisibleTimer = 0f;
				m_dragonPositionTimer = 0f;
             	m_actionPointTimer = 0f;
				m_panicTimer = 0f;

				m_timeStuck = 0f;

				m_lastPos = m_machine.transform.position;

				m_params[0] = null;

				Transform enemy = m_machine.enemy;
				if (enemy != null) {
					if (enemy.position.x < m_machine.position.x) {
						m_runDirection = 1;
					} else if (enemy.position.x > m_machine.position.x) {
						m_runDirection = -1;
					}
				} else {
					m_runDirection = Random.Range(-1, 1);
				}

				m_fleeState = FleeState.Flee;
			}

			protected override void OnExit(State newState) {				
				m_pilot.SlowDown(false);
				m_pilot.ReleaseAction(Pilot.Action.Scared);
				m_pilot.SetDirection(m_machine.direction, false);
			}

			protected override void OnUpdate() {
				ActionPoint ap = null;
				Transform enemy = m_machine.enemy;

				// Let's see if we have found an action point
				m_actionPointTimer -= Time.deltaTime;
				if (m_actionPointTimer <= 0f) {
					ap = ActionPointManager.instance.GetActionPointAt(m_machine.transform.position);

					if (ap != null) {
						Actions.Action action = null;

						if (ap.CanEnter()) {
							action = ap.GetAction(ref m_data.actions);
						} else {
							action = ap.GetDefaultAction();
						}

						if (action != null) {
							m_params[0] = action;
							if (action.id == Actions.Id.Home) {
								Transition(onGoBackHome);
								return;
							} else if (action.id == Actions.Id.GoOn) {
								ap = null;
							}
						} else {
							ap = null;
						}
					}

					m_actionPointTimer = m_data.checkForActionPointTime;
				}

				if (ap != null) {
					Transition(onActionPoint, m_params);
				} else {
					bool warning = m_machine.GetSignal(Signals.Type.Warning);
					bool danger = m_machine.GetSignal(Signals.Type.Danger);


					switch (m_fleeState) {
						case FleeState.Flee:
						case FleeState.Flee_Panic:
							if (!warning) {
								if (m_fleeState == FleeState.Flee) 	ChangeState(FleeState.Slow_Down);
								else 								ChangeState(FleeState.Slow_Down_Panic);
								return;
							} else {
								m_dragonVisibleTimer -= Time.deltaTime;
								if (m_dragonVisibleTimer <= 0f) {
									if (danger) ChangeState(FleeState.Flee_Panic);
									else 		ChangeState(FleeState.Flee);

									m_dragonVisibleTimer = 1f;
									return;
								}
							}

							// Maybe dragon has outrun us! let's check!
							m_dragonPositionTimer -= Time.deltaTime;
							if (m_dragonPositionTimer <= 0f) {
								if (enemy) {						
									if (enemy.position.x < m_machine.position.x) {
										m_runDirection = 1;
									} else if (enemy.position.x > m_machine.position.x) {
										m_runDirection = -1;
									}
								}
								m_dragonPositionTimer = m_data.checkDragonPositionTime;
							}

							if (m_machine.groundDirection.y > 0.6f) {
								ChangeState(FleeState.Panic);
								return;
							} else if (m_pilot.speed >= m_pilot.moveSpeed * 0.5f) {
								float dSqr = (m_machine.position - m_lastPos).sqrMagnitude;
								if (dSqr < 0.001f) 	m_timeStuck += Time.deltaTime;
								else 				m_timeStuck = 0f;
								
								m_lastPos = m_machine.position;

								if (m_timeStuck > 1f) {
									ChangeState(FleeState.Panic);
									return;
								}
							}
							break;

						case FleeState.Panic:
							if (m_machine.enemy != null) {
								if (m_machine.enemy.position.x < m_machine.position.x) {
									m_pilot.SetDirection(Vector3.left, true);
								} else {
									m_pilot.SetDirection(Vector3.right, true);
								}
							}

							m_panicTimer -= Time.deltaTime;
							if (m_panicTimer <= 0) {
								if (danger) {
									m_panicTimer = 2.5f;
								} else {
									m_pilot.SetDirection(m_machine.direction, false);
									ChangeState(FleeState.Flee_Panic);
								}
								return;
							}
							break;
													
						case FleeState.Slow_Down:
						case FleeState.Slow_Down_Panic:
							if (warning) {
								ChangeState(FleeState.Flee);
								return;
							} 

							if (m_machine.groundDirection.y > 0.6f) {
								ChangeState(FleeState.Panic);
								return;
							} else {
								m_panicTimer -= Time.deltaTime;
								if (m_panicTimer <= 0) {
									m_pilot.SetMoveSpeed(0);
									if (m_pilot.speed < 1) Transition(onIdleAlert);							
								}
							}
							break;
					}

					m_target = m_machine.position + m_runDirection * m_machine.groundDirection * 1.5f;
					m_pilot.GoTo(m_target);
				}
			}

			private void ChangeState(FleeState _nextState) {
				if (_nextState != m_fleeState) {
					switch(m_fleeState) {
						case FleeState.Flee:
							break;

						case FleeState.Flee_Panic:
							m_pilot.ReleaseAction(Pilot.Action.Scared);
							break;

						case FleeState.Panic:
							m_timeStuck = 0f;
							m_pilot.ReleaseAction(Pilot.Action.Scared);
							break;

						case FleeState.Slow_Down:
							break;
					}

					switch(_nextState) {
						case FleeState.Flee:
							m_dragonVisibleTimer = 0f;
							m_pilot.SetMoveSpeed(m_data.speed);
							break;

						case FleeState.Flee_Panic:
							m_dragonVisibleTimer = 0f;
							m_pilot.SetMoveSpeed(m_data.speed);
							m_pilot.PressAction(Pilot.Action.Scared); 
							break;

						case FleeState.Panic:
							m_panicTimer = 5f;
							m_pilot.Stop();
							m_pilot.PressAction(Pilot.Action.Scared); 
							break;
													
						case FleeState.Slow_Down:
							m_panicTimer = 1.5f;
							break;

						case FleeState.Slow_Down_Panic:
							m_panicTimer = 1.5f;
							m_pilot.PressAction(Pilot.Action.Scared);
							break;
					}

					m_fleeState = _nextState;
				}
			}
		}
	}
}