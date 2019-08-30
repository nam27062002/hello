using UnityEngine;
using System.Collections;

namespace AI {
	namespace Behaviour {


		[CreateAssetMenu(menuName = "Behaviour/Pet/Using Free Revive")]
		public class PetUsingFreeRevive : StateComponent {

			[StateTransitionTrigger]
            private static readonly int onFreeReviveDone = UnityEngine.Animator.StringToHash("onFreeReviveDone");

            float m_timer;
			protected override void OnEnter(State _oldState, object[] _param)
			{
				// InstanceManager.player.ResetStats(true, DragonPlayer.ReviveReason.FREE_REVIVE_PET);	// do it on next update?
				Messenger.Broadcast(MessengerEvents.PLAYER_PET_PRE_FREE_REVIVE);

				// Make pet lose aura!
				Transform t = m_machine.transform.FindTransformRecursive("PS_ReviveAura");
				if (t != null)
				{
					ParticleSystem[] particles = t.GetComponentsInChildren<ParticleSystem>();
					for( int i = 0; i<particles.Length; i++ )
						particles[i].Stop();
				}

				m_pilot.Stop();
				DragonPlayer player = InstanceManager.player;
                player.dragonMotion.OnPetPreFreeRevive();

                float distance = player.data.maxScale * 6;
				Vector3 dir = Vector3.back;
				m_pilot.transform.position = player.transform.position + dir * distance;
				m_pilot.SetDirection( Vector3.right , true);

				// Revive animation
				m_pilot.PressAction(Pilot.Action.Button_A);

				m_timer = 0.5f;

				// Teleport in front of player??
			}

			protected override void OnUpdate()
			{
				if ( m_timer > 0 )
				{
					m_timer -= Time.deltaTime;
					if ( m_timer <= 0 )
					{
						m_pilot.ReleaseAction(Pilot.Action.Button_A);
						// Hide Harp and Halo

						InstanceManager.player.ResetStats(true, DragonPlayer.ReviveReason.FREE_REVIVE_PET);	// do it on next update?

						Transition(onFreeReviveDone);
					}
				}
			}
		}
	}
}