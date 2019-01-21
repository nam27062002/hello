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
			private Entity m_entity;
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
                m_dragon = InstanceManager.player.dragonHealthBehaviour;
				m_timer = 0;

				DragonTier dragonTier = InstanceManager.player.data.tier;
				m_entity = m_pilot.GetComponent<Entity>();
				m_enabled = !m_entity.IsEdible(dragonTier);
			}

			protected override void OnUpdate() {
				if (m_enabled) {
					if (m_timer > 0f) {
						m_timer -= Time.deltaTime;
						if (m_timer <= 0f) {
							m_timer = 0f;
						}
					}

					if (m_timer <= 0f) {
						if (m_machine.GetSignal(Signals.Type.Trigger)) {
							object[] param = m_machine.GetSignalParams(Signals.Type.Trigger);
							if (param != null && param.Length > 0) {
                                GameObject go = ((GameObject)param[0]);
                                if (go != null && go.CompareTag("Player")) {
                                    m_dragon.ReceiveDamageOverTime(m_data.damage, m_data.duration, DamageType.NORMAL, m_pilot.transform, true, m_entity.sku, m_entity);
                                }
                                m_timer = 1.0f;
                            }
						}
					}
				}
			}
		}
	}
}