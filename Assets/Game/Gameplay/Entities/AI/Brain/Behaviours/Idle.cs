using UnityEngine;
using System.Collections;

namespace AI {
	namespace Behaviour {
		[System.Serializable]
		public class IdleData : StateComponentData {
			public Range speed = new Range(2f, 4f);
		}

		[CreateAssetMenu(menuName = "Behaviour/Idle")]
		public class Idle : StateComponent {
			
			[StateTransitionTrigger]
			private static string OnMove = "onMove";

			private float m_timer;

			public override StateComponentData CreateData() {
				return new IdleData();
			}

			protected override void OnEnter(State oldState, object[] param) {
				m_timer = Random.Range(2f, 4f);
				m_pilot.SetSpeed(0);
			}

			protected override void OnUpdate() {
				m_timer -= Time.deltaTime;
				if (m_timer <= 0f) {
					Transition(OnMove);
				}
			}
		}
	}
}