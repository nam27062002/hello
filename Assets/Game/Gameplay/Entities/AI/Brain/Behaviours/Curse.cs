using UnityEngine;
using System.Collections;

namespace AI {
	namespace Behaviour {
		[System.Serializable]
		public class CurseData : StateComponentData {
			public float damage;
			public float duration;
		}

		[CreateAssetMenu(menuName = "Behaviour/Curse")]
		public class Curse : StateComponent {

			private CurseData m_data;
			private DragonHealthBehaviour m_dragon;
			private float m_timer;
			private bool m_enabled;

			public override StateComponentData CreateData() {
				return new CurseData();
			}

			public override System.Type GetDataType() {
				return typeof(CurseData);
			}

			protected override void OnInitialise() {
				m_data = m_pilot.GetComponentData<CurseData>();
				m_dragon = InstanceManager.player.GetComponent<DragonHealthBehaviour>();
				m_timer = 0;

				DragonTier dragonTier = InstanceManager.player.GetComponent<DragonPlayer>().data.tier;
				Entity entity = m_pilot.GetComponent<Entity>();
				m_enabled = !entity.IsEdible(dragonTier);
			}

			protected override void OnUpdate() {
				if (m_timer > 0f) {
					m_timer -= Time.deltaTime;
					if (m_timer <= 0f) {
						m_timer = 0f;
					}
				}

				if (m_timer <= 0f) {
					if (m_machine.GetSignal(Signals.Type.Trigger) && m_enabled) {					
						object[] param = m_machine.GetSignalParams(Signals.Type.Trigger);
						if (param != null && param.Length > 0 && ((GameObject)param[0]).CompareTag("Player")) {
							m_dragon.ReceiveDamageOverTime(m_data.damage, m_data.duration, DamageType.CURSE);
						}
						m_timer = 1.0f;
					}
				}
			}
		}
	}
}