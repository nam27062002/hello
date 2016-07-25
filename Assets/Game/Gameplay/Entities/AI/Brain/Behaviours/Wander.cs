using UnityEngine;
using System.Collections;

namespace AI {
	namespace Behaviour {
		[System.Serializable]
		public class WanderData : StateComponentData {
			public float speed;
			public float idleChance = 0f;
		}

		[CreateAssetMenu(menuName = "Behaviour/Wander")]
		public class Wander : StateComponent {

			[StateTransitionTrigger]
			private static string OnRest = "onRest";


			private WanderData m_data;

			private Vector3 m_target;

			private bool m_goToIdle;



			public override StateComponentData CreateData() {
				return new WanderData();
			}

			protected override void OnInitialise() {
				m_data = (WanderData)m_pilot.GetComponentData<Wander>();
				m_target = m_machine.position;
			}

			protected override void OnEnter(State _oldState, object[] _param) {
				m_target = m_machine.position;
				m_pilot.SlowDown(false); // this wander state doesn't have an idle check
				m_goToIdle = false;
			}

			protected override void OnUpdate() {
				m_pilot.SetMoveSpeed(m_data.speed); //TODO

				float m = (m_machine.position - m_target).sqrMagnitude;
				float d = Mathf.Min(2f, m_data.speed);// * Time.smoothDeltaTime;

				if (m < d * d) {
					if (m_goToIdle) {
						Transition(OnRest);
					} else {
						m_goToIdle = Random.Range(0f, 1f) < m_data.idleChance; // it will stop at next target
						m_pilot.SlowDown(m_goToIdle);

						if (m_pilot.guideFunction != null) {					
							m_target = m_pilot.guideFunction.NextPositionAtSpeed(m_data.speed);					
						} else {
							m_target = m_pilot.area.RandomInside();
							m_target.z = 0;
						} 
					}
				}

				m_pilot.GoTo(m_target);
			}
		}
	}
}