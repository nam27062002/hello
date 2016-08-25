using UnityEngine;
using System.Collections;

namespace AI {
	namespace Behaviour {
		[System.Serializable]
		public class PursuitData : StateComponentData {
			public float speed;
			public float arrivalRadius = 1f;
			public string attackPoint;
		}

		[CreateAssetMenu(menuName = "Behaviour/Attack/Pursuit")]
		public class Pursuit : StateComponent {

			[StateTransitionTrigger]
			private static string OnEnemyInRange = "onEnemyInRange";

			[StateTransitionTrigger]
			private static string OnEnemyOutOfSight = "onEnemyOutOfSight";

			protected PursuitData m_data;
			protected Transform m_target;


			public override StateComponentData CreateData() {
				return new PursuitData();
			}

			public override System.Type GetDataType() {
				return typeof(PursuitData);
			}

			protected override void OnInitialise() {
				m_data = m_pilot.GetComponentData<PursuitData>();

				m_machine.SetSignal(Signals.Type.Alert, true);

				m_target = null;
			}

			protected override void OnEnter(State oldState, object[] param) {
				m_pilot.SetMoveSpeed(m_data.speed);
				m_pilot.SlowDown(true);

				m_target = null;

				if (m_machine.enemy != null) {
					m_target = m_machine.enemy.FindTransformRecursive(m_data.attackPoint);
					if (m_target == null) {
						m_target = m_machine.enemy;
					}
				}
			}

			protected override void OnUpdate() {				
				if (m_target != null) {
					float m = (m_machine.position - m_target.position).sqrMagnitude;
					if (m < m_data.arrivalRadius * m_data.arrivalRadius) {
						Transition(OnEnemyInRange);
					} else {
						m_pilot.GoTo(m_target.position);
					}
				} else {
					Transition(OnEnemyOutOfSight);
				}
			}
		}
	}
}