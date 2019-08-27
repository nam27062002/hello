using UnityEngine;
using System.Collections;

namespace AI {
	namespace Behaviour {
		[System.Serializable]
		public class PetDecideDogActionData : StateComponentData {
			public Range m_timeSecondAction;
		}

		[CreateAssetMenu(menuName = "Behaviour/Pet/Decide Dog Action")]
		public class PetDecideDogAction : StateComponent {

			[StateTransitionTrigger]
			private static int onTimedAction = UnityEngine.Animator.StringToHash("onTimedAction");


			protected PetDecideDogActionData m_data;
			protected float m_timer;
			protected PetDogSpawner m_spawner;

			public override StateComponentData CreateData() {
				return new PetDecideDogActionData();
			}

			public override System.Type GetDataType() {
				return typeof(PetDecideDogActionData);
			}

			protected override void OnInitialise() 
			{
				m_data = m_pilot.GetComponentData<PetDecideDogActionData>();
				m_timer =  Time.time + m_data.m_timeSecondAction.GetRandom();
				m_spawner = m_pilot.GetComponent<PetDogSpawner>();
			}


			protected override void OnUpdate()
			{
				if ( m_timer <= Time.time && m_spawner.CanRespawn())
				{
					// Seond Action
					Transition( onTimedAction );
					m_timer =  Time.time + m_data.m_timeSecondAction.GetRandom();
				}
			}
		}
	}
}