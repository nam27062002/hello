using UnityEngine;
using System.Collections;

namespace AI {
	namespace Behaviour {


		[CreateAssetMenu(menuName = "Behaviour/Pet/Free Revive")]
		public class PetFreeRevive : StateComponent {


			PetFreeRevive(){
				Messenger.AddListener<DamageType>(GameEvents.PLAYER_KO, OnFreeRevive);
			}

			~PetFreeRevive(){
				Messenger.RemoveListener<DamageType>(GameEvents.PLAYER_KO, OnFreeRevive);
			}

			private bool m_revive = true;
			private bool m_executeFreeRevive = false;
			protected override void OnUpdate(){
				if ( m_executeFreeRevive ){
					m_executeFreeRevive = false;
					InstanceManager.player.ResetStats(true, DragonPlayer.ReviveReason.FREE_REVIVE_PET);	// do it on next update?
					Messenger.Broadcast(GameEvents.PLAYER_FREE_REVIVE);
				}
			}

			void OnFreeRevive( DamageType _type ){
				if (  m_revive && InstanceManager.player != null && !InstanceManager.player.IsAlive() ){
					// Free Revive!
					// and tell view to lose aura
					m_revive = false;
					m_executeFreeRevive = true;
				}

			}
		}
	}
}