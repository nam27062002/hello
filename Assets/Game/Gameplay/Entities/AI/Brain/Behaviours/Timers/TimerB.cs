using UnityEngine;
using System.Collections;

namespace AI {
	namespace Behaviour {
		[System.Serializable]
		public class TimerBData : StateComponentData {
			public float seconds;
		}

		[CreateAssetMenu(menuName = "Behaviour/Timers/Timer B")]
		public class TimerB : Timing {
			
			private TimerBData m_data;


			public override StateComponentData CreateData() {
				return new TimerBData();
			}

			public override System.Type GetDataType() {
				return typeof(TimerBData);
			}

			protected override void OnInitialise() {
				m_data = m_pilot.GetComponentData<TimerBData>();
			}

			protected override void OnEnter(State oldState, object[] param) {
				m_timer = m_data.seconds;				
			}
		}
	}
}