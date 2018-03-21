using UnityEngine;
using System.Collections;

namespace AI {
	namespace Behaviour {
		
		[System.Serializable]
		public class PetScreenGoldData : StateComponentData {
			public string m_audio;
		}


		[CreateAssetMenu(menuName = "Behaviour/Pet/Screen Gold")]
		public class PetScreenGold : StateComponent {

			[StateTransitionTrigger]
			private static string OnAnimFinished = "onAnimFinished";

			PetScreenGoldData m_data;
			float m_timer;

			public override StateComponentData CreateData() {
				return new PetPlayerInfiniteBoostData();
			}

			public override System.Type GetDataType() {
				return typeof(PetPlayerInfiniteBoostData);
			}

			protected override void OnInitialise() {
				m_data = m_pilot.GetComponentData<PetScreenGoldData>();
			}

			protected override void OnEnter(State oldState, object[] param) {
				m_pilot.PressAction(Pilot.Action.Button_A);
				m_timer = 1.0f;
				if (!string.IsNullOrEmpty(m_data.m_audio))
				{
					AudioController.Play(m_data.m_audio);
				}
			}

			protected override void OnUpdate(){
				m_timer -= Time.deltaTime;

				m_pilot.SlowDown(true);
				m_pilot.SetDirection( Vector3.forward, true );

				if ( m_timer <= 0 )	
				{
					EntityManager.instance.ForceOnScreenEntitiesGolden();
					Transition( OnAnimFinished );
				}
			}

			protected override void OnExit(State _newState){
				m_pilot.ReleaseAction(Pilot.Action.Button_A);
			}
		}
	}
}