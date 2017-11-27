using UnityEngine;
using System.Collections;

namespace AI {
	namespace Behaviour {
		[System.Serializable]
		public class PetXmasElfGiftData : StateComponentData {
			public Range m_timeSecondAction;
		}

		[CreateAssetMenu(menuName = "Behaviour/Pet/Xmas Elf Gift")]
		public class PetXmasElfGift : StateComponent {

			[StateTransitionTrigger]
			private static string OnTimedAction = "onTimedAction";


			protected PetXmasElfGiftData m_data;
			protected float m_timer;
			protected PetXmasElfSpawner m_spawner;

			public override StateComponentData CreateData() {
				return new PetXmasElfGiftData();
			}

			public override System.Type GetDataType() {
				return typeof(PetXmasElfGiftData);
			}

			protected override void OnInitialise() 
			{
				m_data = m_pilot.GetComponentData<PetXmasElfGiftData>();
				m_timer =  Time.time + m_data.m_timeSecondAction.GetRandom();
				m_spawner = m_pilot.GetComponent<PetXmasElfSpawner>();

			}


			protected override void OnUpdate()
			{
				if ( m_timer <= Time.time && m_spawner.HasAvailableEntities())
				{
					m_timer =  Time.time + m_data.m_timeSecondAction.GetRandom();
					m_spawner.RamdomizeEntity();
					m_spawner.Respawn();
				}
			}

		}
	}
}