using UnityEngine;
using System.Collections;

namespace AI {
	namespace Behaviour {
		
		[System.Serializable]
		public class WanderPlayerData : StateComponentData {
			public float speed;
		}

		[CreateAssetMenu(menuName = "Behaviour/Pet/Wander Player")]
		public class PetWanderPlayer : StateComponent {

			private WanderPlayerData m_data;
			private Transform m_target;


			public override StateComponentData CreateData() {
				return new WanderPlayerData();
			}

			public override System.Type GetDataType() {
				return typeof(WanderPlayerData);
			}

			protected override void OnInitialise() {
				m_data = m_pilot.GetComponentData<WanderPlayerData>();
				m_target = m_machine.transform;
			}

			protected override void OnEnter(State _oldState, object[] _param) {
				SelectTarget();
				m_pilot.SlowDown(true); // this wander state doesn't have an idle check
				m_pilot.SetMoveSpeed(m_data.speed); //TODO
			}

			protected override void OnUpdate() {
				if ( (m_target.position - m_pilot.transform.position).sqrMagnitude > 2 )
				{
					m_pilot.SetMoveSpeed(m_data.speed); //TODO
					m_pilot.GoTo(m_target.position);
				}
				else
				{
					// Move target?
					m_pilot.SetMoveSpeed(0);
					if ( m_pilot.speed <= 0.1f )
					{
						m_pilot.SetDirection( m_target.forward );
					}
				}
			}

			private void SelectTarget() {
				m_target = m_pilot.homeTransform;	//  Get Pet position??
			}
		}
	}
}