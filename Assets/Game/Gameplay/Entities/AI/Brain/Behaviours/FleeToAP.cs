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
			private static string OnActionPoint = "onActionPoint";

			[StateTransitionTrigger]
			private static string OnGoBackHome = "onGoBackHome";

			[StateTransitionTrigger]
			private static string OnIdleAlert = "onIdleAlert";


			private enum FleeState {
				Flee = 0,
				Flee_Panic,
				Panic,
				Slow_Down
			}


			private FleeToAPData m_data;

			private FleeState m_fleeState;

			private Vector3 m_target;
			private Vector3 m_lastPos;
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
								Transition(OnGoBackHome);
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
					Transition(OnActionPoint, m_params);
				} else {
					Vector3 direction = m_machine.direction;
					bool warning = m_machine.GetSignal(Signals.Type.Warning);
					bool danger = m_machine.GetSignal(Signals.Type.Danger);


					switch (m_fleeState) {
						case FleeState.Flee:
						case FleeState.Flee_Panic: {
							if (!warning) {
								ChangeState(FleeState.Slow_Down); 
							} else {
								m_dragonVisibleTimer -= Time.deltaTime;
								if (m_dragonVisibleTimer <= 0f) {
									if (danger) {
										ChangeState(FleeState.Flee_Panic);
									} else {
										ChangeState(FleeState.Flee);
									}
									m_dragonVisibleTimer = 5f;
								}
							}

							// Maybe dragon has outrun us! let's check!
							m_dragonPositionTimer -= Time.deltaTime;
							if (m_dragonPositionTimer <= 0f) {
								if (enemy) {						
									if (enemy.position.x < m_machine.position.x) {
										direction = Vector3.right;
									} else {
										direction = Vector3.left;
									}
								}
								m_dragonPositionTimer = m_data.checkDragonPositionTime;
							}

							if (m_pilot.speed >= m_pilot.moveSpeed * 0.5f) {
								float dSqr = (m_machine.transform.position - m_lastPos).sqrMagnitude;
								if (dSqr < 0.001f) {
									m_timeStuck += Time.deltaTime;
								} else {
									m_timeStuck = 0;	
								}
								m_lastPos = m_machine.transform.position;

								if (m_timeStuck > 1f) {
									Debug.Log("[" + m_pilot.name + "] Can't move!");
									ChangeState(FleeState.Panic);
								}
							}

						}   break;

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
								m_pilot.SetDirection(m_machine.direction, false);
								ChangeState(FleeState.Flee_Panic);
							}
							break;

						case FleeState.Slow_Down:
							if (warning) ChangeState(FleeState.Flee);			

							m_pilot.SetMoveSpeed(0);

							if (m_pilot.speed < 1) Transition(OnIdleAlert);							
							break;
					}

					m_target = m_machine.position + direction * 1.5f;
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
							m_dragonVisibleTimer = 5f;
							m_pilot.SetMoveSpeed(m_data.speed);
							break;

						case FleeState.Flee_Panic:
							m_dragonVisibleTimer = 5f;
							m_pilot.SetMoveSpeed(m_data.speed);
							m_pilot.PressAction(Pilot.Action.Scared); 
							break;

						case FleeState.Panic:
							m_panicTimer = 5f;
							m_pilot.Stop();
							m_pilot.PressAction(Pilot.Action.Scared); 
							break;

						case FleeState.Slow_Down:
							break;
					}

					m_fleeState = _nextState;
				}
			}
		}
	}
}