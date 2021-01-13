using UnityEngine;
using System.Collections;

namespace AI {
	namespace Behaviour {		
		[System.Serializable]
		public class HomeSenseDragonData : StateComponentData {
			public float speed;
		}

		[CreateAssetMenu(menuName = "Behaviour/Home Sense Dragon")]
		public class HomeSenseDragon : StateComponent {		
			
			[StateTransitionTrigger]
			protected static readonly int onBackAtHome = UnityEngine.Animator.StringToHash("onBackAtHome");


            private HomeSenseDragonData m_data;


			public override StateComponentData CreateData() {
				return new HomeSenseDragonData();
			}

			public override System.Type GetDataType() {
				return typeof(HomeSenseDragonData);
			}

			protected override void OnInitialise() {
				m_data = m_pilot.GetComponentData<HomeSenseDragonData>();
			}

			protected override void OnEnter(State _oldState, object[] _param) {				
				m_pilot.SetMoveSpeed(m_data.speed);
			}

			protected override void OnUpdate() {
				m_pilot.GoTo(m_pilot.homePosition);

				float dSqr = (m_machine.position - m_pilot.homePosition).sqrMagnitude;
				if (dSqr < 3f) {
					Transition(onBackAtHome);
				}
			}
		}
	}
}