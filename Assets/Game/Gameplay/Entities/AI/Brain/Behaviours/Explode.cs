using UnityEngine;
using System.Collections;

namespace AI {
	namespace Behaviour {		
		[System.Serializable]
		public class ExplodeData : StateComponentData {
			public float damage = 5f;
		}

		[CreateAssetMenu(menuName = "Behaviour/Explode")]
		public class Explode : StateComponent {

			private ExplodeData m_data;


			public override StateComponentData CreateData() {
				return new ExplodeData();
			}

			protected override void OnInitialise() {
				m_data = (ExplodeData)m_pilot.GetComponentData<Explode>();
			}

			protected override void OnEnter(State _oldState, object[] _param) {
				// explode
				DragonPlayer dragon = InstanceManager.player;
				if (dragon.HasMineShield()) {
					dragon.LoseMineShield();
				} else {
					dragon.GetComponent<DragonHealthBehaviour>().ReceiveDamage(m_data.damage, m_machine.transform);
				}

				m_machine.SetSignal(Signals.Type.Destroyed, true);
			}
		}
	}
}