using UnityEngine;
using System.Collections;

namespace AI {
	namespace Behaviour {
		[System.Serializable]
		public class BigMineData : StateComponentData {
			public float distance;
		}

		[CreateAssetMenu(menuName = "Behaviour/Big Mine")]
		public class BigMine : StateComponent {

			private BigMineData m_data;
			private DragonMotion m_dragon;
			private float m_distanceSqr;


			public override StateComponentData CreateData() {
				return new BigMineData();
			}

			public override System.Type GetDataType() {
				return typeof(BigMineData);
			}

			protected override void OnInitialise() {
				m_data = m_pilot.GetComponentData<BigMineData>();
				m_dragon = InstanceManager.player.dragonMotion;
				m_distanceSqr = m_data.distance * m_data.distance;
			}

			protected override void OnUpdate() {				
				Vector3 d = m_machine.position - m_dragon.position;
				float dSqr = d.sqrMagnitude;

				if (dSqr < m_distanceSqr) {
					m_pilot.PressAction(Pilot.Action.Button_A);
				} else {
					m_pilot.ReleaseAction(Pilot.Action.Button_A);
				}
			}
		}
	}
}