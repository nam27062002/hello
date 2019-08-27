using UnityEngine;
using System.Collections;

namespace AI {
	namespace Behaviour {
		[System.Serializable]
		public class InAreaControlData : StateComponentData {
			public float timeBeforeBackHome = 2f;
		}

		[CreateAssetMenu(menuName = "Behaviour/Move In Area Control")]
		public class InAreaControl : StateComponent {
			[StateTransitionTrigger]
			protected static readonly int onOutsideArea = UnityEngine.Animator.StringToHash("onOutsideArea");


            protected InAreaControlData m_data;

			private bool m_isOutside;
			private float m_outsideTimer;


			public override StateComponentData CreateData() {
				return new InAreaControlData();
			}

			public override System.Type GetDataType() {
				return typeof(InAreaControlData);
			}

			protected override void OnInitialise() {
				m_data = m_pilot.GetComponentData<InAreaControlData>();
			}

			protected override void OnEnter(State oldState, object[] param) {
				m_isOutside = false;
			}

			protected override void OnUpdate() {
				if (m_isOutside) {
					m_outsideTimer -= Time.deltaTime;
					if (m_outsideTimer <= 0) {
						if (!m_pilot.area.Contains(m_machine.position)) {
							Transition(onOutsideArea);
						}
						m_isOutside = false;
					}
				} else {
					// if this machine is outside his area, go back to home position (if it has this behaviour)
					if (m_pilot.area != null && !m_pilot.area.Contains(m_machine.position)) {
						// we'll let the unit stay outside a few seconds before triggering the "back to home" state
						m_isOutside = true;
						m_outsideTimer = m_data.timeBeforeBackHome;
					}
				}
			}	
		}
	}
}
