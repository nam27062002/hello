﻿using UnityEngine;
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

			private FleeToAPData m_data;

			private Vector3 m_target;

			private float m_dragonPositionTimer;
			private float m_actionPointTimer;


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
			}

			protected override void OnEnter(State oldState, object[] param) {
				m_pilot.SetMoveSpeed(m_data.speed);
				m_dragonPositionTimer = 0f;
             	m_actionPointTimer = 0f;
			}

			protected override void OnExit(State newState) {				
				m_pilot.SlowDown(false);
				m_pilot.ReleaseAction(Pilot.Action.Scared);
			}

			protected override void OnUpdate() {
				ActionPoint ap = null;
				Transform enemy = m_machine.enemy;

				if (m_machine.GetSignal(Signals.Type.Danger)) {
					m_pilot.PressAction(Pilot.Action.Scared); 
				} else {
					m_pilot.ReleaseAction(Pilot.Action.Scared); 
				}

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
					Transition(OnActionPoint);
				} else {
					// Maybe dragon has outrun us! let's check!
					Vector3 direction = m_machine.direction;
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

					m_target = m_machine.position + direction * 1.5f;
					m_pilot.GoTo(m_target);
				}
			}
		}
	}
}