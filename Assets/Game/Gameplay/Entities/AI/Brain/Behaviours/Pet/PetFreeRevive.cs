using UnityEngine;
using System.Collections;

namespace AI {
	namespace Behaviour {


		[CreateAssetMenu(menuName = "Behaviour/Pet/Free Revive")]
		public class PetFreeRevive : StateComponent {

			[StateTransitionTrigger]
			private static string onStartFreeRevive = "onStartFreeRevive";

			private object[] m_transitionParam;


			PetFreeRevive(){
				Messenger.AddListener<DamageType>(GameEvents.PLAYER_KO, OnFreeRevive);
			}

			~PetFreeRevive(){
				Messenger.RemoveListener<DamageType>(GameEvents.PLAYER_KO, OnFreeRevive);
			}

			private bool m_revive = true;
			private bool m_executeFreeRevive = false;

			protected override void OnInitialise() {
				base.OnInitialise();
				m_transitionParam = new object[1];
			}

			protected override void OnUpdate(){
				if ( m_executeFreeRevive ){
					m_executeFreeRevive = false;
					Messenger.Broadcast(GameEvents.PLAYER_PRE_FREE_REVIVE);

					m_transitionParam[0] = InstanceManager.player.transform;
					m_machine.enemy = InstanceManager.player.transform;
					m_machine.SetSignal(Signals.Type.Warning, true);
					Transition(onStartFreeRevive);
					// Turn off aura particle
				}
			}

			void OnFreeRevive( DamageType _type ){
				if (  m_revive && InstanceManager.player != null && !InstanceManager.player.IsAlive() ){
					// Free Revive!
					// and tell view to lose aura
					// m_revive = false;
					m_executeFreeRevive = true;
				}

			}
		}
	}
}