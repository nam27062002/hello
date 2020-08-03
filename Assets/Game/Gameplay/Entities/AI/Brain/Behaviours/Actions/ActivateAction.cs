using UnityEngine;
using System.Collections;

namespace AI {
	namespace Behaviour {
		[System.Serializable]
		public class ActivateActionData : StateComponentData {
			public Pilot.Action action = Pilot.Action.Boost;
		}
		
		[CreateAssetMenu(menuName = "Behaviour/Actions/Activate Action")]
		public class ActivateAction : StateComponent {

			private ActivateActionData m_data;

			public override StateComponentData CreateData() {
				return new ActivateActionData();
			}

			public override System.Type GetDataType() {
				return typeof(ActivateActionData);
			}

			protected override void OnInitialise() {
				m_data = m_pilot.GetComponentData<ActivateActionData>();
			}

			protected override void OnEnter(State oldState, object[] param) 
			{
				m_pilot.PressAction( m_data.action );
			}

			protected override void OnExit(State newState)
			{
				m_pilot.ReleaseAction( m_data.action );
			}

		}
	}
}