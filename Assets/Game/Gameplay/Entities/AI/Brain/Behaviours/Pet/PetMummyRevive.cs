using UnityEngine;
using System.Collections;

namespace AI {
	namespace Behaviour {


		[CreateAssetMenu(menuName = "Behaviour/Pet/Mummy Revive")]
		public class PetMummyRevive : StateComponent {

			[StateTransitionTrigger]
			public static readonly int onStartRevive = UnityEngine.Animator.StringToHash("onStartMummyRevive");

            PetMummyRevive(){
                Messenger.AddListener(MessengerEvents.PLAYER_MUMMY_REVIVE, OnRevive);
			}

			~PetMummyRevive(){
				Messenger.RemoveListener(MessengerEvents.PLAYER_MUMMY_REVIVE, OnRevive);
			}

			private bool m_executeRevive = false;
			private float m_executingRevive = 0f;

			protected override void OnUpdate(){
				if ( m_executeRevive ) {
					m_executeRevive = false;
					Transition(onStartRevive);
				}
			}

			void OnRevive(){
                DragonPlayer dragon = InstanceManager.player;
                if (dragon != null && !dragon.IsAlive()) {
                    if (dragon.CanUseMummyPower()) {
                        m_executeRevive = true;
                    }
                }
            }
		}
	}
}