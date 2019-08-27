using UnityEngine;
using System.Collections;

namespace AI {
	namespace Behaviour {
		[System.Serializable]
		public class IdleData : StateComponentData {
			public Range restTime = new Range(2f, 4f);
			public bool randomLookAt = true;
		}

		[CreateAssetMenu(menuName = "Behaviour/Idle")]
		public class Idle : StateComponent {
			
			[StateTransitionTrigger]
			protected static readonly int onMove = UnityEngine.Animator.StringToHash("onMove");


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
				m_pilot.Stop();

				if (m_data.randomLookAt) {
					Vector3 dir = Vector3.back;
					if (m_machine.direction.x >= 0f) 
						dir.x = Random.Range(0.1f, 1f);
					else 
						dir.x = Random.Range(-1f, -0.1f);
					dir.Normalize();

					m_pilot.SetDirection(dir);
				}
			}

			protected override void OnUpdate() {
				if (m_data.restTime.max > 0f) { 
					m_timer -= Time.deltaTime;
					if (m_timer <= 0f) {
						Transition(onMove);
					}
				}
			}
		}
	}
}