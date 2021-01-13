using UnityEngine;
using System.Collections;

namespace AI {
	namespace Behaviour {
		[System.Serializable]
		public class WanderData : StateComponentData {
			public float speed;
			public float idleChance = 0f;
			public bool alwaysSlowdown = false;
		}

		[CreateAssetMenu(menuName = "Behaviour/Wander")]
		public class Wander : StateComponent {

			[StateTransitionTrigger]
			private static readonly int onRest = UnityEngine.Animator.StringToHash("onRest");


            private WanderData m_data;

			private Vector3 m_target;

			private float m_timer; // change target when time is over

			private bool m_goToIdle;



			public override StateComponentData CreateData() {
				return new WanderData();
			}

			public override System.Type GetDataType() {
				return typeof(WanderData);
			}

			protected override void OnInitialise() {
				m_data = m_pilot.GetComponentData<WanderData>();
				m_target = m_machine.position;
			}

			protected override void OnEnter(State _oldState, object[] _param) {
				SelectTarget();
				m_pilot.SlowDown(m_data.alwaysSlowdown); // this wander state doesn't have an idle check
				m_pilot.SetMoveSpeed(m_data.speed); //TODO
				m_goToIdle = false;
			}

			protected override void OnUpdate() {
				float dsqr = (m_pilot.target - m_machine.position).sqrMagnitude;
				float deltaDSqr = Mathf.Max(1f, m_pilot.speed * m_pilot.speed);

				m_timer -= Time.deltaTime;

				if (m_timer <= 0f || dsqr < deltaDSqr) {
					if (m_goToIdle) {
						Transition(onRest);
					} else {
						m_goToIdle = Random.Range(0f, 1f) < m_data.idleChance; // it will stop at next target
						m_pilot.SlowDown(m_data.alwaysSlowdown || m_goToIdle);

						SelectTarget();
					}
				}

				m_pilot.GoTo(m_target);
			}

			private void SelectTarget() {
				if (m_pilot.guideFunction != null) {
					m_target = m_pilot.guideFunction.NextPositionAtSpeed(m_pilot.speed);					
				} else {
					m_target = m_pilot.area.RandomInside();
				}

				if (m_pilot.IsActionPressed(Pilot.Action.Avoid)) {
					m_timer = 0.25f;
				} else if (m_data.speed > 0f) {
					m_timer = (m_machine.position - m_target).magnitude / Mathf.Max(m_pilot.speed, m_data.speed);
				} else {
					m_timer = 1f;
				}
			}
		}
	}
}