using UnityEngine;
using System.Collections;

namespace AI {
	namespace Behaviour {
		[System.Serializable]
		public class WanderPathData : StateComponentData {
			public float speed;
			public Range timeToIdle = new Range(10f, 20f);
			public bool alwaysSlowdown = false;
		}

		[CreateAssetMenu(menuName = "Behaviour/Wander Path")]
		public class WanderPath : StateComponent {

			[StateTransitionTrigger]
			private static string OnRest = "onRest";


			private WanderPathData m_data;


			private PathController m_path;
			private int m_pathDirection;
			private int m_currentNode;
			private Vector3 m_target;

			private float m_timer;



			public override StateComponentData CreateData() {
				return new WanderPathData();
			}

			public override System.Type GetDataType() {
				return typeof(WanderPathData);
			}

			protected override void OnInitialise() {
				m_data = m_pilot.GetComponentData<WanderPathData>();
				m_target = m_machine.position;
			}

			protected override void OnEnter(State _oldState, object[] _param) {
				m_target = m_machine.position;
				m_pilot.SlowDown(m_data.alwaysSlowdown); // this wander state doesn't have an idle check
				m_timer = m_data.timeToIdle.GetRandom();

				if (Random.Range(0, 100) < 50)
					m_pathDirection = -1;
				else
					m_pathDirection = 1;

				m_path = (PathController)m_pilot.guideFunction;
				m_path.SetDirection(m_pathDirection);
				m_currentNode = m_path.GetIndexNearestTo(m_machine.position);
				m_target = m_path.GetNextFrom(m_currentNode);
				m_currentNode = m_path.GetCurrentIndex();
				m_pathDirection = m_path.GetDirection();
			}

			protected override void OnUpdate() {
				m_pilot.SetMoveSpeed(m_data.speed);

				m_timer -= Time.deltaTime;

				if (m_data.timeToIdle.max > 0f && m_timer <= 0f) {
					Transition(OnRest);
				} else {
					float m = (m_machine.position - m_target).sqrMagnitude;
                    float d = m_path.radius;

                    if (m < d * d) {
						m_path.SetDirection(m_pathDirection);
						m_target = m_path.GetNextFrom(m_currentNode);
						m_currentNode = m_path.GetCurrentIndex();
						m_pathDirection = m_path.GetDirection();
					}

					m_pilot.GoTo(m_target);
				}
			}
		}
	}
}