using UnityEngine;
using System.Collections;

namespace AI {
	namespace Behaviour {
		
		[CreateAssetMenu(menuName = "Behaviour/Pet/Check Area Change")]
		public class PetCheckAreaChange : StateComponent {

			[StateTransitionTrigger]
			public static string onAreaChangeStart = "onAreaChangeStart";

			[StateTransitionTrigger]
			public static string onAreaChangeEnd = "onAreaChangeEnd";

			private MachineEatBehaviour m_eatBehaviour;
			protected override void OnInitialise()
			{
				m_eatBehaviour = m_pilot.GetComponent<MachineEatBehaviour>();
			}

			protected override void OnEnter(State _oldState, object[] _param) 
			{
				Messenger.AddListener(GameEvents.PLAYER_ENTERING_AREA, OnEnteringArea);
				Messenger.AddListener(GameEvents.PLAYER_LEAVING_AREA, OnLeavingArea);

				Messenger.AddListener<DragonMotion.PetsEatingTest>(GameEvents.PLAYER_ASK_PETS_EATING, OnEatingQuestion);
			}

			protected override void OnExit(State _newState)
			{
				Messenger.RemoveListener(GameEvents.PLAYER_ENTERING_AREA, OnEnteringArea);
				Messenger.RemoveListener(GameEvents.PLAYER_LEAVING_AREA, OnLeavingArea);

				Messenger.RemoveListener<DragonMotion.PetsEatingTest>(GameEvents.PLAYER_ASK_PETS_EATING, OnEatingQuestion);
			}

			void OnEatingQuestion( DragonMotion.PetsEatingTest eating )
			{
				if ( m_eatBehaviour != null )
				{
					if ( m_eatBehaviour.IsEating() || m_eatBehaviour.IsLatching() || m_eatBehaviour.IsGrabbing() )
					{
						eating.m_eating = true;
					}
				}
			}

			void OnLeavingArea()
			{
				if ( m_eatBehaviour != null )	
				{
					m_eatBehaviour.PauseEating();
				}
				Transition( onAreaChangeStart );
			}

			void OnEnteringArea()
			{
				if ( m_eatBehaviour != null )	
				{
					m_eatBehaviour.ResumeEating();
				}
				Transition( onAreaChangeEnd );
			}
		}
	}
}