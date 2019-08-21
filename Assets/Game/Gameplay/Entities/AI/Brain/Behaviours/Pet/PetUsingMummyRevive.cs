using UnityEngine;
using System.Collections;

namespace AI {
	namespace Behaviour {

		[CreateAssetMenu(menuName = "Behaviour/Pet/Using Mummy Revive")]
		public class PetUsingMummyRevive : StateComponent {

			[StateTransitionTrigger]
			public static readonly int onMummyReviveDone = UnityEngine.Animator.StringToHash("onMummyReviveDone");

            float m_timer;
			protected override void OnEnter(State _oldState, object[] _param)
			{
				m_pilot.Stop();
				DragonPlayer player = InstanceManager.player;
                player.particleController.CastMummyPower();
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

                        InstanceManager.player.StartMummyPower();

						Transition(onMummyReviveDone);
					}
				}
			}
		}
	}
}