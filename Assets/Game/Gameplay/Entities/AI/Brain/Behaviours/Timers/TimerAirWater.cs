using UnityEngine;
using System.Collections;

namespace AI {
	namespace Behaviour {
		[System.Serializable]
		public class TimerAirWaterData : StateComponentData {
			public float airSeconds;
            public float waterSeconds;
        }

		[CreateAssetMenu(menuName = "Behaviour/Timers/Timer Air Water")]
		public class TimerAirWater : StateComponent {
            [StateTransitionTrigger]
            protected static string OnTimeFinished = "onTimeFinished";

            private TimerAirWaterData m_data;
            protected float m_timer;


            public override StateComponentData CreateData() {
				return new TimerAirWaterData();
			}

			public override System.Type GetDataType() {
				return typeof(TimerAirWaterData);
			}

			protected override void OnInitialise() {
				m_data = m_pilot.GetComponentData<TimerAirWaterData>();
			}

            protected override void OnEnter(State _oldState, object[] _param) {
                m_timer = 0;
            }

            protected override void OnUpdate() {
                m_timer += Time.deltaTime;

                if (m_machine.GetSignal(Signals.Type.InWater)) {
                    if (m_timer >= m_data.waterSeconds) {
                        Transition(OnTimeFinished);
                    }
                } else {
                    if (m_timer >= m_data.airSeconds) {
                        Transition(OnTimeFinished);
                    }
                }
            }
        }
	}
}