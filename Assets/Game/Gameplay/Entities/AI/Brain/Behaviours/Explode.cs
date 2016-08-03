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

			public override System.Type GetDataType() {
				return typeof(ExplodeData);
			}

			protected override void OnInitialise() {
				m_data = m_pilot.GetComponentData<ExplodeData>();
			}

			protected override void OnEnter(State _oldState, object[] _param) {
				// explode
				DragonPlayer dragon = InstanceManager.player;
				if (dragon.HasMineShield()) {
					dragon.LoseMineShield();
				} else {
					dragon.GetComponent<DragonHealthBehaviour>().ReceiveDamage(m_data.damage, m_machine.transform);
				}

				DragonMotion dragonMotion = dragon.GetComponent<DragonMotion>();

				Vector3 knockBack = dragonMotion.transform.position - m_machine.position;
				knockBack.Normalize();

				knockBack *= Mathf.Log(Mathf.Max(dragonMotion.velocity.magnitude * m_data.damage, 2f));

				dragonMotion.AddForce(knockBack);

				m_machine.SetSignal(Signals.Type.Destroyed, true);
			}
		}
	}
}