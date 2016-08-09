using UnityEngine;
using System.Collections;

namespace AI {
	namespace Behaviour {
		[System.Serializable]
		public class IdleData : StateComponentData {
			public Range restTime = new Range(2f, 4f);
		}

		[CreateAssetMenu(menuName = "Behaviour/Idle")]
		public class Idle : StateComponent {
			
			[StateTransitionTrigger]
			protected static string OnMove = "onMove";


			protected IdleData m_data;

			protected float m_timer;

			public override StateComponentData CreateData() {
				return new IdleData();
			}

			public override System.Type GetDataType() {
				return typeof(IdleData);
			}

			protected override void OnInitialise() {
				m_data = m_pilot.GetComponentData<IdleData>();
			}

			protected override void OnEnter(State oldState, object[] param) {
				m_timer = m_data.restTime.GetRandom();
				m_pilot.SetMoveSpeed(0);
			}

			protected override void OnUpdate() {
				if (m_data.restTime.max > 0f) { 
					m_timer -= Time.deltaTime;
					if (m_timer <= 0f) {
						Transition(OnMove);
					}
				}
			}
		}
	}
}