using UnityEngine;
using System.Collections;

namespace AI {
	namespace Behaviour {


		[CreateAssetMenu(menuName = "Behaviour/Pet/Free Revive")]
		public class PetFreeRevive : StateComponent {


			PetFreeRevive(){
				Messenger.AddListener<DamageType, Transform>(GameEvents.PLAYER_KO, OnFreeRevive);
			}

			~PetFreeRevive(){
				Messenger.RemoveListener<DamageType, Transform>(GameEvents.PLAYER_KO, OnFreeRevive);
			}

			private bool m_revive = true;
			private bool m_executeFreeRevive = false;
			private float m_executingRevive = 0f;
			protected override void OnUpdate(){
				if ( m_executeFreeRevive ){
					m_executeFreeRevive = false;
					m_executingRevive = 0.25f;
					// InstanceManager.player.ResetStats(true, DragonPlayer.ReviveReason.FREE_REVIVE_PET);	// do it on next update?
					Messenger.Broadcast(GameEvents.PLAYER_PET_PRE_FREE_REVIVE);

					// Make pet lose aura!
					Transform t = m_machine.transform.FindTransformRecursive("PS_ReviveAura");
					if (t != null)
					{
						ParticleSystem[] particles = t.GetComponentsInChildren<ParticleSystem>();
						for( int i = 0; i<particles.Length; i++ )
							particles[i].Stop();
					}


				}
				else if ( m_executingRevive > 0 )
				{
					m_executingRevive -= Time.deltaTime;
					if ( m_executingRevive <= 0 )
					{
						InstanceManager.player.ResetStats(true, DragonPlayer.ReviveReason.FREE_REVIVE_PET);	// do it on next update?
					}
				}
			}

			void OnFreeRevive( DamageType _type, Transform _source ){
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