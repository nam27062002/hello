﻿using UnityEngine;
using System.Collections;

namespace AI {
	namespace Behaviour {		
		[System.Serializable]
		public class GroundWanderData : StateComponentData {
			public float speed = 1.5f;
			public Range timeToIdle = new Range(15f, 20f);
			public Range timeToChangeDirection = new Range(5f, 10f);
		}

		[CreateAssetMenu(menuName = "Behaviour/Ground Wander")]
		public class GroundWander : StateComponent {

			[StateTransitionTrigger]
			private static string OnRest = "onRest";


			private GroundWanderData m_data;

			private Vector2 m_limitMin;
			private Vector2 m_limitMax;

			private float m_side;

			private float m_idleTimer;
			private float m_sideTimer;



			public override StateComponentData CreateData() {
				return new GroundWanderData();
			}

			public override System.Type GetDataType() {
				return typeof(GroundWanderData);
			}

			protected override void OnInitialise() {
				m_data = m_pilot.GetComponentData<GroundWanderData>();
			}

			protected override void OnEnter(State oldState, object[] param) {
				m_limitMin.x = m_pilot.area.bounds.min.x;
				m_limitMax.x = m_pilot.area.bounds.max.x;

				m_limitMin.y = m_pilot.area.bounds.min.y;
				m_limitMax.y = m_pilot.area.bounds.max.y;

				m_pilot.SetMoveSpeed(m_data.speed);
				m_pilot.SlowDown(false);

				m_side = (Random.Range(0f, 1f) < 0.4f)? -1 : 1;

				m_idleTimer = m_data.timeToIdle.GetRandom();
				m_sideTimer = m_data.timeToChangeDirection.GetRandom();
			}

			protected override void OnUpdate() {
				m_pilot.SetMoveSpeed(m_data.speed);
				m_idleTimer -= Time.deltaTime;

				if (m_idleTimer > 0f) {
					Vector3 direction = (m_machine.groundDirection == Vector3.zero)? Vector3.right : m_machine.groundDirection;
					Vector3 target = m_machine.position + direction * m_side * 1.5f;
					m_side = 1; // we'll keep walking in the same direction

					m_sideTimer -= Time.deltaTime;
					if (m_sideTimer <= 0 || ShouldChangeDirection(target)) {
						m_side *= -1;
						m_sideTimer = m_data.timeToChangeDirection.GetRandom();
					}

					m_pilot.GoTo(target);
				} else {
					m_idleTimer = 0f;
					m_pilot.SlowDown(true);

					float m = Mathf.Abs(m_machine.position.x - m_pilot.target.x);
					if (m < 1f) {
						Transition(OnRest);
					}
				}
			}

			private bool ShouldChangeDirection(Vector3 _pos) {
				bool goingOutside = _pos.x < m_limitMin.x || _pos.x > m_limitMax.x ||  _pos.y < m_limitMin.y || _pos.y > m_limitMax.y;
				bool changeDir = false;

				if (goingOutside) {
					Vector3 v = m_pilot.homePosition - m_machine.position;
					Vector3 d = _pos - m_machine.position;
					float dot = Vector3.Dot(d, v);
					changeDir = dot < 0;
				}

				return changeDir;
			}
		}
	}
}