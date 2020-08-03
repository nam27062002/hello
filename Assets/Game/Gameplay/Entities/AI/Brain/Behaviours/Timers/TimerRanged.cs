using UnityEngine;
using System.Collections;

namespace AI {
	namespace Behaviour {
		[System.Serializable]
		public class TimerRangedData : StateComponentData {
			public Range seconds;
		}

		[CreateAssetMenu(menuName = "Behaviour/Timers/Timer Ranged")]
		public class TimerRanged : Timing {
			
			private TimerRangedData m_data;
			public TimerRangedData data { get{ return m_data; }}


			public override StateComponentData CreateData() {
				return new TimerRangedData();
			}

			public override System.Type GetDataType() {
				return typeof(TimerRangedData);
			}

			protected override void OnInitialise() {
				m_data = m_pilot.GetComponentData<TimerRangedData>();
			}

			protected override void OnEnter(State oldState, object[] param) {
				m_timer = m_data.seconds.GetRandom();
			}
		}
	}
}