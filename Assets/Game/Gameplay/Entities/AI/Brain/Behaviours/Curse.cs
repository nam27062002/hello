﻿using UnityEngine;
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

			public override StateComponentData CreateData() {
				return new CurseData();
			}

			protected override void OnInitialise() {
				m_data = (CurseData)m_pilot.GetComponentData<Curse>();
				m_dragon = InstanceManager.player.GetComponent<DragonHealthBehaviour>();
				m_timer = 0;
			}

			protected override void OnUpdate() {
				if (m_timer > 0f) {
					m_timer -= Time.deltaTime;
					if (m_timer <= 0f) {
						m_timer = 0f;
					}
				}

				if (m_machine.GetSignal(Signals.Type.CollisionTrigger)) {
					if (m_timer <= 0f) {
						m_dragon.ReceiveDamageOverTime(m_data.damage, m_data.duration, DamageType.CURSE);
						m_timer = 1.0f;
					}
				}
			}
		}
	}
}