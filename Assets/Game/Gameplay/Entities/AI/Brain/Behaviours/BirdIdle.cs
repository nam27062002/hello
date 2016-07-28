using UnityEngine;
using System.Collections;

namespace AI {
	namespace Behaviour {		
		[System.Serializable]
		public class BirdIdleData : StateComponentData {
			public float speed = 0f;
			public bool restUpsideDown = false;
		}

		[CreateAssetMenu(menuName = "Behaviour/Bird Idle")]
		public class BirdIdle : StateComponent {

			[StateTransitionTrigger]
			protected static string OnMove = "onMove";


			protected static int m_groundMask;

			private enum IdleState {
				GoToRestPoint = 0,
				Rest
			}


			private bool m_avoidCollisionsRestoreValue;

			private BirdIdleData m_data;
			private IdleState m_idleState;

			private Vector3 m_restPoint;
			private Vector3 m_direction;

			public override StateComponentData CreateData() {
				return new BirdIdleData();
			}

			protected override void OnInitialise() {
				m_groundMask = LayerMask.GetMask("Ground", "GroundVisible");
				m_data = (BirdIdleData)m_pilot.GetComponentData<BirdIdle>();

				m_avoidCollisionsRestoreValue = m_pilot.avoidCollisions;
			}

			protected override void OnEnter(State _oldState, object[] _param) {
				m_idleState = IdleState.GoToRestPoint;

				//let's find a rest position
				RaycastHit hit;
				Vector3 start = m_pilot.area.RandomInside();
				Vector3 end = start;

				if (m_data.restUpsideDown) {
					end.y += m_pilot.area.bounds.size.y;
				} else {
					end.y -= m_pilot.area.bounds.size.y;
				}
					
				m_pilot.avoidCollisions = false;

				if (Physics.Linecast(start, end, out hit, m_groundMask)) {
					m_restPoint = hit.point;
				
					m_direction = new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-0.5f, -1f));
					m_direction.Normalize();

					m_pilot.SetMoveSpeed(m_data.speed);
					m_pilot.SlowDown(true);
					m_pilot.GoTo(m_restPoint);
				} else {
					Transition(OnMove);
				}
			}

			protected override void OnExit(State _newState) {
				m_pilot.avoidCollisions = m_avoidCollisionsRestoreValue;
			}

			protected override void OnUpdate() {
				if (m_idleState == IdleState.GoToRestPoint) {
					float m = (m_machine.position - m_restPoint).sqrMagnitude;
					float d = 0.1f;

					if (m < d * d) {
						m_pilot.SetMoveSpeed(0);
						m_pilot.SetDirection(m_direction);
						m_idleState = IdleState.Rest;
					}
				}
			}
		}
	}
}