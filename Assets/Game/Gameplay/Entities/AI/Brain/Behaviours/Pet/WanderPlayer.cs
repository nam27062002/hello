using UnityEngine;
using System.Collections;

namespace AI {
	namespace Behaviour {
		

		[CreateAssetMenu(menuName = "Behaviour/Wander Player")]
		public class WanderPlayer : StateComponent {

			private WanderData m_data;
			private Transform m_target;


			public override StateComponentData CreateData() {
				return new WanderData();
			}

			public override System.Type GetDataType() {
				return typeof(WanderData);
			}

			protected override void OnInitialise() {
				m_data = m_pilot.GetComponentData<WanderData>();
				m_target = m_machine.transform;
			}

			protected override void OnEnter(State _oldState, object[] _param) {
				SelectTarget();
				m_pilot.SlowDown(m_data.alwaysSlowdown); // this wander state doesn't have an idle check
				m_pilot.SetMoveSpeed(m_data.speed); //TODO
			}

			protected override void OnUpdate() {
				m_pilot.SetMoveSpeed(m_data.speed); //TODO
				m_pilot.GoTo(m_target.position);
			}

			private void SelectTarget() {

				if ( InstanceManager.player != null )
				{
					m_target = InstanceManager.player.transform;	//  Get Pet position??
				}
			}
		}
	}
}