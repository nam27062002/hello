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

			private HumanFleeData m_data;

			private Vector3 m_target;

			private float m_xLimitMin;
			private float m_xLimitMax;

			private float m_allowTargetChangeTimer;

			public override StateComponentData CreateData() {
				return new HumanFleeData();
			}

			public override System.Type GetDataType() {
				return typeof(HumanFleeData);
			}

			protected override void OnInitialise() {
				m_data = m_pilot.GetComponentData<HumanFleeData>();

				m_machine.SetSignal(Signals.Type.Alert, true);

				m_xLimitMin = m_pilot.area.bounds.min.x;
				m_xLimitMax = m_pilot.area.bounds.max.x;

				m_target = m_machine.position;
			}

			protected override void OnEnter(State oldState, object[] param) {
				m_pilot.SetMoveSpeed(m_data.speed);	

				m_allowTargetChangeTimer = 0f;
			}

			protected override void OnExit(State newState) {
				m_pilot.Scared(false);
				m_pilot.SlowDown(false);
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
					m_allowTargetChangeTimer = m_data.checkDragonPositionTime;
				} else {
					m_allowTargetChangeTimer -= Time.deltaTime;
				}

				float m = Mathf.Abs(m_machine.position.x - m_target.x);
				if (m < 1f) {
					m_pilot.SetMoveSpeed(0f);

					Vector3 dir = Vector3.zero;
					dir.x = m_machine.position.x - m_target.x;
					m_pilot.SetDirection(dir.normalized);
				} else {
					m_pilot.SetMoveSpeed(3f);
				}
			
				m_pilot.Scared(m_machine.GetSignal(Signals.Type.Danger));
				m_pilot.GoTo(m_target);
			}
		}
	}
}