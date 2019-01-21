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
			private Entity m_entity;
			private DragonHealthBehaviour m_dragon;
			private float m_timer;
			private bool m_enabled;


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

			protected override void OnEnter(State _oldState, object[] _param){
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
                                    m_dragon.ReceiveDamage(m_data.damage, m_data.type, m_machine.transform, true, m_entity.sku, m_entity);
                                }
								m_timer = m_data.delay;
							}
						}
					}
				}
			}
		}
	}
}