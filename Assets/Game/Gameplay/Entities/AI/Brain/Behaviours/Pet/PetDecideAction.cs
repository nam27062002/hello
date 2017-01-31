using UnityEngine;
using System.Collections;

namespace AI {
	namespace Behaviour {
		[System.Serializable]
		public class PetDecideActionData : StateComponentData {
			public Range m_timeSecondAction;
		}

		[CreateAssetMenu(menuName = "Behaviour/Pet/Decide Action")]
		public class PetDecideAction : StateComponent {

			[StateTransitionTrigger]
			private static string OnDefaultAction = "onDefaultAction";

			[StateTransitionTrigger]
			private static string OnTimedAction = "onTimedAction";


			protected PetDecideActionData m_data;
			protected float m_timer;

			private object[] m_transitionParam;

			public override StateComponentData CreateData() {
				return new PetDecideActionData();
			}

			public override System.Type GetDataType() {
				return typeof(PetDecideActionData);
			}

			protected override void OnInitialise() 
			{
				m_data = m_pilot.GetComponentData<PetDecideActionData>();
				m_transitionParam = new object[1];

			}

			protected override void OnEnter(State oldState, object[] param) {

				if ( m_timer <= Time.time )
				{
					// Seond Action
					Transition( OnTimedAction, param);
				}
				else
				{
					// Default action
					Transition( OnDefaultAction, param);
				}
				m_timer =  Time.time + m_data.m_timeSecondAction.GetRandom();
			}


		}
	}
}