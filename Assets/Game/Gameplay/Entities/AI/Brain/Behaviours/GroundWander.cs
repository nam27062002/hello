using UnityEngine;
using System.Collections;

namespace AI {
	namespace Behaviour {		
		[System.Serializable]
		public class GroundWanderData : StateComponentData {
			public float speed = 1.5f;
			public float idleChance = 0.25f;
		}

		[CreateAssetMenu(menuName = "Behaviour/Ground Wander")]
		public class GroundWander : StateComponent {

			[StateTransitionTrigger]
			private static string OnRest = "onRest";

			private GroundWanderData m_data;

			private Vector3 m_target;

			private float m_xLimitMin;
			private float m_xLimitMax;

			private bool m_goToIdle;



			public override StateComponentData CreateData() {
				return new GroundWanderData();
			}

			protected override void OnInitialise() {
				m_data = (GroundWanderData)m_pilot.GetComponentData<GroundWander>();

				m_xLimitMin = m_pilot.area.min.x;
				m_xLimitMax = m_pilot.area.max.x;
			}

			protected override void OnEnter(State oldState, object[] param) {
				if (oldState.name != "flee") {
					m_target = Vector3.zero;
					m_target.x = Random.Range(m_xLimitMin, m_xLimitMax);
				}
				m_pilot.SetSpeed(m_data.speed);
				m_pilot.SlowDown(false);
				m_goToIdle = false;
			}

			protected override void OnUpdate() {
				float m = Mathf.Abs(m_machine.position.x - m_target.x);

				if (m < 1f) {
					if (m_goToIdle) {
						Transition(OnRest);
					} else {
						m_goToIdle = Random.Range(0f, 1f) < m_data.idleChance; // it will stop at next target
						m_pilot.SlowDown(m_goToIdle);
						m_target.x = Random.Range(m_xLimitMin, m_xLimitMax);
					}
				}

				m_pilot.GoTo(m_target);
			}
		}
	}
}