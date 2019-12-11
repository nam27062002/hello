using UnityEngine;
using System.Collections;

namespace AI {
	namespace Behaviour {


		[CreateAssetMenu(menuName = "Behaviour/Pet/Free Revive")]
		public class PetFreeRevive : StateComponent {

			[StateTransitionTrigger]
			public static readonly int onStartFreeRevive = UnityEngine.Animator.StringToHash("onStartFreeRevive");

			PetFreeRevive(){
				Messenger.AddListener(MessengerEvents.PLAYER_FREE_REVIVE, OnRevive);
			}

			~PetFreeRevive(){
				Messenger.RemoveListener(MessengerEvents.PLAYER_FREE_REVIVE, OnRevive);
			}
            			
			private bool m_executeRevive = false;
			private bool m_petCanRevive = true;

			protected override void OnUpdate(){
				if ( m_executeRevive ){
					m_executeRevive = false;
					Transition(onStartFreeRevive);
				}
			}

			void OnRevive(){
                DragonPlayer dragon = InstanceManager.player;
                if (dragon != null && !dragon.IsAlive()) {
                    if (dragon.CanUseFreeRevives() && m_petCanRevive) {
						m_petCanRevive = false;
                        m_executeRevive = true;
                    }
				}
			}
		}
	}
}