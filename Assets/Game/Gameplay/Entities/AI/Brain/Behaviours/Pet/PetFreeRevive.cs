﻿using UnityEngine;
using System.Collections;

namespace AI {
	namespace Behaviour {


		[CreateAssetMenu(menuName = "Behaviour/Pet/Free Revive")]
		public class PetFreeRevive : StateComponent {

			[StateTransitionTrigger]
			public const string onStartFreeRevive = "onStartFreeRevive";

			PetFreeRevive(){
				Messenger.AddListener(MessengerEvents.PLAYER_FREE_REVIVE, OnRevive);
			}

			~PetFreeRevive(){
				Messenger.RemoveListener(MessengerEvents.PLAYER_FREE_REVIVE, OnRevive);
			}
            			
			private bool m_executeRevive = false;
			private float m_executingRevive = 0f;

			protected override void OnUpdate(){
				if ( m_executeRevive ){
					m_executeRevive = false;
					Transition(onStartFreeRevive);
				}
			}

			void OnRevive(){
                DragonPlayer dragon = InstanceManager.player;
                if (dragon != null && !dragon.IsAlive()) {
                    if (dragon.CanUseFreeRevives()) {
                        m_executeRevive = true;
                    }
				}
			}
		}
	}
}