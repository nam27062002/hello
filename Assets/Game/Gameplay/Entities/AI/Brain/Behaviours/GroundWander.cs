using UnityEngine;
using System.Collections;

namespace AI {
	namespace Behaviour {		
		[System.Serializable]
		public class GroundWanderData : StateComponentData {
			public float speed = 1.5f;
			public bool isWallWalking = false;
			public Range timeToIdle = new Range(15f, 20f);
			public Range timeToChangeDirection = new Range(5f, 10f);
		}

		[CreateAssetMenu(menuName = "Behaviour/Ground Wander")]
		public class GroundWander : StateComponent {

			[StateTransitionTrigger]
			private static readonly int onRest = UnityEngine.Animator.StringToHash("onRest");


            private GroundWanderData m_data;

			private Vector2 m_limitMin;
			private Vector2 m_limitMax;

			private float m_side;

			private bool m_hasIdleState;
			private float m_idleTimer;
			private float m_sideTimer;

			private float m_distanceToTarget;


			public override StateComponentData CreateData() {
				return new GroundWanderData();
			}

			public override System.Type GetDataType() {
				return typeof(GroundWanderData);
			}

			protected override void OnInitialise() {
				m_data = m_pilot.GetComponentData<GroundWanderData>();
			}

			protected override void OnEnter(State _oldState, object[] _param) {
				m_limitMin.x = m_pilot.area.bounds.min.x;
				m_limitMax.x = m_pilot.area.bounds.max.x;

				m_limitMin.y = m_pilot.area.bounds.min.y;
				m_limitMax.y = m_pilot.area.bounds.max.y;

				m_pilot.SetMoveSpeed(m_data.speed);
				m_pilot.SlowDown(false);

				m_side = (Random.Range(0f, 1f) < 0.4f)? -1 : 1;

				m_hasIdleState = m_data.timeToIdle.min > 0f;
				m_idleTimer = m_data.timeToIdle.GetRandom();
				m_sideTimer = m_data.timeToChangeDirection.GetRandom();
			}

			protected override void OnUpdate() {
				m_pilot.SetMoveSpeed(m_data.speed);

				if (m_hasIdleState) {
					m_idleTimer -= Time.deltaTime;
					if (m_idleTimer > 0f) {
						DoWander();
					} else {
						m_idleTimer = 0f;
						m_pilot.SlowDown(true);
						float distanceToTarget = (m_pilot.target - m_machine.position).sqrMagnitude;
						if (distanceToTarget <= 2f || distanceToTarget > m_distanceToTarget) {
							Transition(onRest);
						}
						m_distanceToTarget = distanceToTarget;
					}
				} else {
					DoWander();
				}
			}

			private void DoWander() {
				Vector3 direction = m_machine.groundDirection;
				direction.z = 0f;

				if (m_data.isWallWalking || direction.y < 0.6f) {					
					Vector3 target = m_machine.position + direction * m_side * 1.5f;

					m_sideTimer -= Time.deltaTime;
					if (m_sideTimer <= 0 || ShouldChangeDirection(target)) {
						m_side *= -1;
						m_sideTimer = m_data.timeToChangeDirection.GetRandom();
					}

					m_distanceToTarget =  (target - m_machine.position).sqrMagnitude;

					m_pilot.GoTo(target);
				} else {
					Transition(onRest);
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