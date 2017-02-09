using UnityEngine;
using System.Collections;

namespace AI {
	namespace Behaviour {
		[System.Serializable]
		public class TimerAData : StateComponentData {
			public float seconds;
		}

		[CreateAssetMenu(menuName = "Behaviour/Timers/Timer A")]
		public class TimerA : Timing {
			
			private TimerAData m_data;


			public override StateComponentData CreateData() {
				return new TimerAData();
			}

			public override System.Type GetDataType() {
				return typeof(TimerAData);
			}

			protected override void OnInitialise() {
				m_data = m_pilot.GetComponentData<TimerAData>();
			}

			protected override void OnEnter(State oldState, object[] param) {
				m_timer = m_data.seconds;				
			}
		}
	}
}