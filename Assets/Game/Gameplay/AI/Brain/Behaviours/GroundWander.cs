using UnityEngine;
using System.Collections;
using AISM;

namespace AI {
	namespace Behaviour {		
		[CreateAssetMenu(menuName = "Behaviour/Ground Wander")]
		public class GroundWander : StateComponent {

			[StateTransitionTrigger]
			private static string OnRest = "onRest";

			private Vector3 m_target;

			private float m_xLimitMin;
			private float m_xLimitMax;

			private float m_timer;

			private Pilot m_pilot;
			private Machine m_machine;

			protected override void OnInitialise(GameObject _go) {
				m_pilot 	= _go.GetComponent<Pilot>();
				m_machine	= _go.GetComponent<Machine>();

				m_xLimitMin = m_machine.position.x - 20f;
				m_xLimitMax = m_machine.position.x + 20f;
			}

			protected override void OnEnter(State oldState, object[] param) {
				m_target = Vector3.zero;
				m_target.x = Random.Range(m_xLimitMin, m_xLimitMax);
				m_timer = Random.Range(10f, 35f);
				m_pilot.SetSpeed(1);
			}

			protected override void OnUpdate() {
				float m = Mathf.Abs(m_machine.position.x - m_target.x);

				if (m < 0.1f) {
					m_target.x = Random.Range(m_xLimitMin, m_xLimitMax);
				}

				m_pilot.GoTo(m_target);

				if (m_timer > 0f) {
					m_timer -= Time.deltaTime;
					if (m_timer <= 0f) {
						Transition(OnRest);
					}
				}
			}
		}
	}
}