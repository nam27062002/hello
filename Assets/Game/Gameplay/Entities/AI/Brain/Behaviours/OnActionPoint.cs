using UnityEngine;
using System.Collections;

namespace AI {
	namespace Behaviour {
		[System.Serializable]
		public class OnActionPointData : StateComponentData {
			
		}

		[CreateAssetMenu(menuName = "Behaviour/On Action Point")]
		public class OnActionPoint : StateComponent {
			
			//[StateTransitionTrigger]
			//protected static string OnGoBackHome = "onGoBackHome";


			protected OnActionPointData m_data;

			protected float m_timer;

			public override StateComponentData CreateData() {
				return new OnActionPointData();
			}

			public override System.Type GetDataType() {
				return typeof(OnActionPointData);
			}

			protected override void OnInitialise() {
				m_data = m_pilot.GetComponentData<OnActionPointData>();
			}

			protected override void OnEnter(State oldState, object[] param) {
				m_pilot.PressAction(Pilot.Action.Scared);
				m_pilot.Stop();
			}

			protected override void OnExit(State newState) {
				m_pilot.ReleaseAction(Pilot.Action.Scared);
			}

			protected override void OnUpdate() {
				
			}
		}
	}
}