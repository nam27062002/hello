using UnityEngine;
using System.Collections;

namespace AI {
	namespace Behaviour {		
		[System.Serializable]
		public class BirdIdleData : StateComponentData {
			public float speed = 0f;
			public bool restUpsideDown = false;
			public bool teleport = false;
		}

		[CreateAssetMenu(menuName = "Behaviour/Bird Idle")]
		public class BirdIdle : StateComponent {

			[StateTransitionTrigger]
			protected static int onMove = UnityEngine.Animator.StringToHash("onMove");

			private enum IdleState {
				GoToRestPoint = 0,
				Rest
			}


			private bool m_avoidCollisionsRestoreValue;
			private bool m_avoidWaterRestoreValue;

			private BirdIdleData m_data;
			private IdleState m_idleState;

			private Vector3 m_restPoint;
			private Vector3 m_direction;

			public override StateComponentData CreateData() {
				return new BirdIdleData();
			}

			public override System.Type GetDataType() {
				return typeof(BirdIdleData);
			}

			protected override void OnInitialise() {				
				m_data = m_pilot.GetComponentData<BirdIdleData>();

				m_avoidCollisionsRestoreValue = m_pilot.avoidCollisions;
				m_avoidWaterRestoreValue = m_pilot.avoidWater;
			}

			protected override void OnEnter(State _oldState, object[] _param) {				
				//let's find a rest position
				RaycastHit hit;
				Vector3 start = (_oldState == null)? m_pilot.area.RandomInside() : m_machine.position;
				Vector3 end = start;

				if (m_data.restUpsideDown) {
					end.y += m_pilot.area.bounds.size.y;
				} else {
					end.y -= m_pilot.area.bounds.size.y;
				}
					
				m_pilot.avoidCollisions = false;
				m_pilot.avoidWater = false;

                if (Physics.Linecast(start, end, out hit, GameConstants.Layers.GROUND)) {
					m_restPoint = hit.point;
				
					m_direction = new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-0.5f, -1f));
					m_direction.Normalize();

					if (m_data.teleport && _oldState == null) {
						m_machine.position = m_restPoint;
						m_pilot.Stop();
						m_pilot.SetDirection(m_direction);

						m_idleState = IdleState.Rest;
					} else {
						m_pilot.SetMoveSpeed(m_data.speed);
						m_pilot.SlowDown(true);
						m_pilot.GoTo(m_restPoint);

						m_idleState = IdleState.GoToRestPoint;
					}
				} else {
					Transition(onMove);
				}
			}

			protected override void OnExit(State _newState) {
				m_pilot.avoidCollisions = m_avoidCollisionsRestoreValue;
				m_pilot.avoidWater = m_avoidWaterRestoreValue;
			}

			protected override void OnUpdate() {
				if (m_idleState == IdleState.GoToRestPoint) {
					float m = (m_machine.position - m_restPoint).sqrMagnitude;
					float d = 1f;

					if (m < d * d) {
						m_machine.position = m_restPoint;
						m_pilot.Stop();
						m_pilot.SetDirection(m_direction);
						m_idleState = IdleState.Rest;
					}
				}
			}
		}
	}
}