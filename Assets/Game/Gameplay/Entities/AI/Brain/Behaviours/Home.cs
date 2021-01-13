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
			
			[StateTransitionTrigger]
			protected static readonly int onBackAtHome = UnityEngine.Animator.StringToHash("onBackAtHome");


            private HomeData m_data;
			private bool m_alertRestoreValue;
			private Group m_group;


			public override StateComponentData CreateData() {
				return new HomeData();
			}

			public override System.Type GetDataType() {
				return typeof(HomeData);
			}

			protected override void OnInitialise() {
				m_data = m_pilot.GetComponentData<HomeData>();
			}

			protected override void OnEnter(State _oldState, object[] _param) {
				m_alertRestoreValue = m_machine.GetSignal(Signals.Type.Alert);
				m_machine.SetSignal(Signals.Type.Alert, false);
				m_pilot.SetMoveSpeed(m_data.speed);
				m_pilot.SlowDown(true);

				m_group = m_machine.GetGroup();
			}

			protected override void OnExit(State _newState) {
				m_machine.SetSignal(Signals.Type.Alert, m_alertRestoreValue);
			}

			protected override void OnUpdate() {
				m_pilot.GoTo(m_pilot.homePosition);

				float deltaSqr = 2f * 2f;
				if (m_group != null) {
					deltaSqr *= 2f;
				}

				float dSqr = (m_machine.position - m_pilot.homePosition).sqrMagnitude;
				if (dSqr < 2f * 2f) {
					Transition(onBackAtHome);
				}
			}
		}
	}
}