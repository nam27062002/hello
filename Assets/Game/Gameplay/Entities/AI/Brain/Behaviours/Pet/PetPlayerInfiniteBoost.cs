using UnityEngine;
using System.Collections;

namespace AI {
	namespace Behaviour {

		[System.Serializable]
		public class PetPlayerInfiniteBoostData : StateComponentData {
			public float m_duration;
		}

		[CreateAssetMenu(menuName = "Behaviour/Pet/Give Player Infinite Boost")]
		public class PetPlayerInfiniteBoost : StateComponent {

			[StateTransitionTrigger]
			private static string OnBoostFinished = "onBoostFinished";

			DragonBoostBehaviour m_playerBoost;
			PetPlayerInfiniteBoostData m_data;
			float m_timer;
			GameObject m_visualCue;

			public override StateComponentData CreateData() {
				return new PetPlayerInfiniteBoostData();
			}

			public override System.Type GetDataType() {
				return typeof(PetPlayerInfiniteBoostData);
			}

			protected override void OnInitialise() {
				m_playerBoost = InstanceManager.player.dragonBoostBehaviour;
				m_data = m_pilot.GetComponentData<PetPlayerInfiniteBoostData>();

				m_visualCue = m_pilot.FindObjectRecursive("visualCue");
			}

			protected override void OnEnter(State oldState, object[] param) {
				m_playerBoost.petInfiniteBoost = true;
				m_pilot.PressAction(Pilot.Action.Button_A);
				// Activate visual placeholder
				m_timer = m_data.m_duration;
				if (m_visualCue)
					m_visualCue.SetActive(true);


			}

			protected override void OnUpdate(){
				m_timer -= Time.deltaTime;
				if ( m_timer <= 0 )	
				{
					Transition( OnBoostFinished );
				}
			}

			protected override void OnExit(State _newState){
				m_playerBoost.petInfiniteBoost = false;
				m_pilot.ReleaseAction(Pilot.Action.Button_A);
				// Deactivate visual placeholder
				if (m_visualCue)
					m_visualCue.SetActive(false);
			}
		}
	}
}