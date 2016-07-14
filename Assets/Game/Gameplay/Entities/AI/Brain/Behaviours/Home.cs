using UnityEngine;
using System.Collections;

namespace AI {
	namespace Behaviour {		
		[System.Serializable]
		public class HomeData : StateComponentData {
			public float speed;
		}

		[CreateAssetMenu(menuName = "Behaviour/Home")]
		public class Home : StateComponent {		
			private HomeData m_data;
			private bool m_alertRestoreValue;


			public override StateComponentData CreateData() {
				return new HomeData();
			}
			protected override void OnInitialise() {
				m_data = (HomeData)m_pilot.GetComponentData<Home>();
			}

			protected override void OnEnter(State _oldState, object[] _param) {
				m_alertRestoreValue = m_machine.GetSignal(Signals.Alert.name);
				m_machine.SetSignal(Signals.Alert.name, false);
				m_pilot.SetMoveSpeed(m_data.speed);
				m_pilot.GoTo(m_pilot.homePosition);
			}

			protected override void OnExit(State _newState) {
				m_machine.SetSignal(Signals.Alert.name, m_alertRestoreValue);
				m_machine.SetSignal(Signals.BackToHome.name, false);
			}

			protected override void OnUpdate() {
				float dSqr = (m_machine.position - m_pilot.homePosition).sqrMagnitude;
				if (dSqr < 1f) {
					m_machine.SetSignal(Signals.BackToHome.name, false);
				}
			}
		}
	}
}