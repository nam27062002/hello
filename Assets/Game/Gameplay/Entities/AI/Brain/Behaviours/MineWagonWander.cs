using UnityEngine;
using System.Collections;

namespace AI {
	namespace Behaviour {
		public enum Direction {
			Right = 0,
			Left
		}

		[System.Serializable]
		public class MineWagonWanderData : StateComponentData {
			public float speed = 1.5f;
			public Direction direction = Direction.Right;
		}

		[CreateAssetMenu(menuName = "Behaviour/Mine Wagon Wander")]
		public class MineWagonWander: StateComponent {

			[StateTransitionTrigger]
			private static readonly int onRest = UnityEngine.Animator.StringToHash("onRest");

			private MineWagonWanderData m_data;
			private float m_side;



			public override StateComponentData CreateData() {
				return new MineWagonWanderData();
			}

			public override System.Type GetDataType() {
				return typeof(MineWagonWanderData);
			}

			protected override void OnInitialise() {
				m_data = m_pilot.GetComponentData<MineWagonWanderData>();
			}

			protected override void OnEnter(State oldState, object[] param) {				
				m_pilot.SetMoveSpeed(m_data.speed);
				m_pilot.SlowDown(false);

				if (m_data.direction == Direction.Right) {
					m_side = 1;
				} else {
					m_side = -1;
				}
			}

			protected override void OnUpdate() {
				m_pilot.SetMoveSpeed(m_data.speed);

				Vector3 direction = m_machine.groundDirection;
				direction.z = 0f;

				Vector3 target = m_machine.position + direction * m_side * 1.5f;
				m_pilot.GoTo(target);
			
			}
		}
	}
}