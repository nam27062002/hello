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
			private static readonly int onTimedAction = UnityEngine.Animator.StringToHash("onTimedAction");


			protected PetXmasElfGiftData m_data;
			protected float m_timer;
			protected PetXmasElfSpawner m_spawner;
			protected PreyAnimationEvents m_animEvents;

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
				m_animEvents = m_pilot.FindComponentRecursive<PreyAnimationEvents>();
			}

			protected override void OnEnter(State _oldState, object[] _param) {
				m_animEvents.onAttachProjectile += new PreyAnimationEvents.OnAttachprojectile(OnAttachProjectile);
			}

			protected override void OnExit(State _newState) {
				m_animEvents.onAttachProjectile -= new PreyAnimationEvents.OnAttachprojectile(OnAttachProjectile);
				m_pilot.ReleaseAction( Pilot.Action.Button_A );
			}

			private void OnAttachProjectile() {
				m_spawner.RamdomizeEntity();
				m_spawner.Respawn();
				m_pilot.ReleaseAction( Pilot.Action.Button_A );
			}

			protected override void OnUpdate()
			{
				if ( m_timer <= Time.time && m_spawner.HasAvailableEntities())
				{
					m_timer =  Time.time + m_data.m_timeSecondAction.GetRandom();
					m_pilot.PressAction(Pilot.Action.Button_A);
				}
			}
		}
	}
}