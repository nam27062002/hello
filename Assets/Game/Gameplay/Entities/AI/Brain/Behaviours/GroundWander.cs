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

			private Vector3 m_target;

			private float m_xLimitMin;
			private float m_xLimitMax;

			private bool m_checkGoToRest;

			private float m_walkSpeed = 1.5f;


			public override StateComponentData CreateData() {
				return new GroundWanderData();
			}

			protected override void OnInitialise() {
				m_xLimitMin = m_pilot.area.min.x;
				m_xLimitMax = m_pilot.area.max.x;
			}

			protected override void OnEnter(State oldState, object[] param) {
				if (oldState.name != "flee") {
					m_target = Vector3.zero;
					m_target.x = Random.Range(m_xLimitMin, m_xLimitMax);
				}
				m_pilot.SetSpeed(m_walkSpeed);
				m_checkGoToRest = true;
			}

			protected override void OnUpdate() {
				float m = Mathf.Abs(m_machine.position.x - m_target.x);

				if (m < m_walkSpeed) {
					if (m_checkGoToRest) {
						bool goToRest = Random.Range(0f, 1f) < 0.25f;
						if (goToRest) {
							// don't check again for "idle" and get let the entity get closer to the current target
							m_checkGoToRest = false;
						} else {
							// this entity won't rest now, let's go to another location
							m_target.x = Random.Range(m_xLimitMin, m_xLimitMax);
						}
					} else {
						// we'll slown down and go to rest
						if (m < m_walkSpeed * 0.25f) {
							Transition(OnRest);
						}
					}
				}

				m_pilot.GoTo(m_target);
			}
		}
	}
}