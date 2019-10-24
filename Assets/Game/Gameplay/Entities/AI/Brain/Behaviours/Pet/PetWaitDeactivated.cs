using UnityEngine;
using System.Collections;

namespace AI {
	namespace Behaviour {
		[CreateAssetMenu(menuName = "Behaviour/Pet/Wait Deactivated")]
		public class PetWaitDeactivated : StateComponent {
			[System.Serializable]
			public class PetWaitDeactivatedData : StateComponentData {
				[Comment("Comma Separated list", 5)]
				public Range m_waitingTime;
			}

			public override StateComponentData CreateData() {
				return new PetWaitDeactivatedData();
			}

			public override System.Type GetDataType() {
				return typeof(PetWaitDeactivatedData);
			}

			[StateTransitionTrigger]
			private static readonly int OnReactivated = UnityEngine.Animator.StringToHash("OnReactivated");

			PetWaitDeactivatedData m_data;
			private float m_timer = 0;
			private bool m_needsToActivate = false;

			protected override void OnInitialise() {
				m_data = m_pilot.GetComponentData<PetWaitDeactivatedData>();

			}

			protected override void OnEnter(State _oldState, object[] _param) {
				// Deactivate duration
				m_timer = m_data.m_waitingTime.GetRandom();

				m_needsToActivate = false;

				// Deactivate pet
				m_machine.Deactivate(m_timer, Activate);
			}

			protected void Activate() {
				// The activation is delayed to the Update() to make sure that it's only done when this StateComponent is enable.
				// This method is called by Machine regardless the current pet's state, so if the reactivation was performed here
				// then an unwanted transition may be performed causing issues such as https://mdc-tomcat-jira100.ubisoft.org/jira/browse/HDK-6554
				m_needsToActivate = true;
			}

			protected override void OnUpdate() {
				if (m_needsToActivate) {
					m_needsToActivate = false;
					Transition( OnReactivated );
				}	
			}
		}
	}
}