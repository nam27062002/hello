using UnityEngine;
using System.Collections;

namespace AI {
	namespace Behaviour {
		[System.Serializable]
		public class OnGroundData : StateComponentData {
			public float standUpTime;
		}

		[CreateAssetMenu(menuName = "Behaviour/On Ground")]
		public class OnGround : StateComponent {
			
			[StateTransitionTrigger]
			private static string OnRecover = "onRecover";


			private OnGroundData m_data;

			private float m_timer;


			public override StateComponentData CreateData() {
				return new OnGroundData();
			}

			public override System.Type GetDataType() {
				return typeof(OnGroundData);
			}

			protected override void OnInitialise() {
				m_data = m_pilot.GetComponentData<OnGroundData>();
			}

			protected override void OnEnter(State _oldState, object[] _param) {
				m_timer = m_data.standUpTime;
				m_pilot.SetMoveSpeed(0f, false);
				m_pilot.SetBoostSpeed(0f);
			}

			protected override void OnUpdate() {				
				//
				m_timer -= Time.deltaTime;
				if (m_timer <= 0) {
					Transition(OnRecover);
				}
			}
		}
	}
}