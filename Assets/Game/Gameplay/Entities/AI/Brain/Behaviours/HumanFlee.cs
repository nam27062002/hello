using UnityEngine;
using System.Collections;

namespace AI {
	namespace Behaviour {		
		[System.Serializable]
		public class HumanFleeData : StateComponentData {
			public float speed = 1f;
			public float checkDragonPositionTime = 2f;
		}

		[CreateAssetMenu(menuName = "Behaviour/Human Flee")]
		public class HumanFlee : StateComponent {
			private Vector3 m_target;

			private float m_xLimitMin;
			private float m_xLimitMax;

			private float m_allowTargetChangeTimer;

			public override StateComponentData CreateData() {
				return new HumanFleeData();
			}

			protected override void OnInitialise() {
				m_machine.SetSignal(Signals.Alert.name, true);

				m_xLimitMin = m_machine.position.x - 20f;
				m_xLimitMax = m_machine.position.x + 20f;

				m_target = m_machine.position;
			}

			protected override void OnEnter(State oldState, object[] param) {
				m_pilot.SetSpeed(3f);	

				m_allowTargetChangeTimer = 0f;
			}

			protected override void OnExit(State newState) {
				m_pilot.Scared(false);
			}

			protected override void OnUpdate() {
				Transform enemy = m_machine.enemy;

				if (m_allowTargetChangeTimer <= 0f) {
					if (enemy) {
						m_target = Vector3.zero;
						if (enemy.position.x < m_machine.position.x) {
							m_target.x = m_xLimitMax;
						} else {
							m_target.x = m_xLimitMin;
						}
					}
					m_allowTargetChangeTimer = 2f;
				} else {
					m_allowTargetChangeTimer -= Time.deltaTime;
				}

				float m = Mathf.Abs(m_machine.position.x - m_target.x);
				if (m < 3f * 0.25f) {
					m_pilot.SetSpeed(0f);

					Vector3 dir = Vector3.zero;
					dir.x = m_machine.position.x - m_target.x;
					m_pilot.SetDirection(dir.normalized);
				} else {
					m_pilot.SetSpeed(3f);
				}
			
				m_pilot.Scared(m_machine.GetSignal(Signals.Danger.name));
				m_pilot.GoTo(m_target);
			}
		}
	}
}