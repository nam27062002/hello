using UnityEngine;
using System.Collections;

namespace AI {
	namespace Behaviour {
		[System.Serializable]
		public class AttackPassiveData : StateComponentData {
			public float damage;
			public DamageType type;
			public float delay;
		}

		[CreateAssetMenu(menuName = "Behaviour/Attack/Passive")]
		public class AttackPassive : StateComponent {

			private AttackPassiveData m_data;
			private DragonHealthBehaviour m_dragon;
			private float m_timer;


			public override StateComponentData CreateData() {
				return new AttackPassiveData();
			}

			public override System.Type GetDataType() {
				return typeof(AttackPassiveData);
			}

			protected override void OnInitialise() {
				m_data = m_pilot.GetComponentData<AttackPassiveData>();
				m_dragon = InstanceManager.player.dragonHealthBehaviour;
				m_timer = 0;
			}

			protected override void OnUpdate() {				
				if (m_timer > 0f) {
					m_timer -= Time.deltaTime;
					if (m_timer <= 0f) {
						m_timer = 0f;
					}
				}

				if (m_timer <= 0f) {
					if (m_machine.GetSignal(Signals.Type.Trigger)) {					
						object[] param = m_machine.GetSignalParams(Signals.Type.Trigger);
						if (param != null && param.Length > 0 && ((GameObject)param[0]).CompareTag("Player")) {
							m_dragon.ReceiveDamage(m_data.damage, m_data.type, m_machine.transform);
						}
						m_timer = m_data.delay;
					}
				}
			}
		}
	}
}