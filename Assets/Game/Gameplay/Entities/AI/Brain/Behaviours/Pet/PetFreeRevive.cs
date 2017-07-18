using UnityEngine;
using System.Collections;

namespace AI {
	namespace Behaviour {


		[CreateAssetMenu(menuName = "Behaviour/Pet/Free Revive")]
		public class PetFreeRevive : StateComponent {

			[StateTransitionTrigger]
			public const string onStartFreeRevive = "onStartFreeRevive";

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
					Transition(onStartFreeRevive);
				}
			}
			/*
			protected override void OnUpdate(){
				if ( m_executeFreeRevive ){
					m_executeFreeRevive = false;
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

					// Revive animation
					m_pilot.PressAction(Pilot.Action.Button_A);

				}
				else if ( m_executingRevive > 0 )
				{
					m_executingRevive -= Time.deltaTime;
					if ( m_executingRevive <= 0 )
					{
						m_pilot.ReleaseAction(Pilot.Action.Button_A);
						// Hide Harp and Halo

						InstanceManager.player.ResetStats(true, DragonPlayer.ReviveReason.FREE_REVIVE_PET);	// do it on next update?
					}
				}
			}
			*/

			void OnFreeRevive( DamageType _type, Transform _source ){
				if (  m_revive && InstanceManager.player != null && !InstanceManager.player.IsAlive() ){
					m_revive = false;
					m_executeFreeRevive = true;
					// Transition(onStartFreeRevive);
				}
			}
		}
	}
}