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

			protected override void OnInitialise() {
				m_data = m_pilot.GetComponentData<PetWaitDeactivatedData>();

			}

			protected override void OnEnter(State _oldState, object[] _param) {
				// Deactivate duration
				m_timer = m_data.m_waitingTime.GetRandom();

				// Deactivate pet
				m_machine.Deactivate(m_timer, Activate);
			}

			protected void Activate()
			{
				Transition( OnReactivated );
			}

		}
	}
}