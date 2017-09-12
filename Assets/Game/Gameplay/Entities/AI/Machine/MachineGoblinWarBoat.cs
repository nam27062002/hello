using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AI {
	public class MachineGoblinWarBoat : MachineAir, IArmored {
		[SeparatorAttribute("Targeting system")]
		[SerializeField] private Transform m_targetDummy;
		[SerializeField] private Transform m_cannonEye;

		[SeparatorAttribute("Armor")]
		[SerializeField] private HitsPerDragonTier m_armorDurabilityPerTier;

		[SeparatorAttribute("Ground")]
		[SerializeField] private Collider[] m_ground;


		public Transform targetDummy { get { return m_targetDummy; } }

		private Hit m_armorDurability = new Hit();
		private DragonTier m_minTierToBreak;


		protected override void Awake() {
			m_minTierToBreak = m_armorDurabilityPerTier.GetMinTier();

			base.Awake();
		}

		public override void Spawn(ISpawner _spawner) {
			DragonPlayer player = InstanceManager.player;
			DragonTier tier = player.GetTierWhenBreaking();

			Hit originalHits = m_armorDurabilityPerTier.Get(tier);
			m_armorDurability.count = originalHits.count;
			m_armorDurability.needBoost = originalHits.needBoost;

			for (int i = 0; i < m_ground.Length; ++i) {
				m_ground[i].isTrigger = false;
			}

			base.Spawn(_spawner);
		}

		public override void CustomUpdate() {
			base.CustomUpdate();

			if (!GetSignal(Signals.Type.Ranged)) {
				Vector3 lookAt = m_cannonEye.position;
				if (direction.x <= 0) { 
					lookAt += Vector3.left * 2f;
				} else {
					lookAt += Vector3.right * 2f;
				}

				m_targetDummy.position = Vector3.Lerp(m_targetDummy.position, lookAt, Time.smoothDeltaTime * 2f);
			}
		}

		public bool ReduceDurability(bool _boost) {
			if (m_armorDurability.count > 0 && !GetSignal(Signals.Type.Burning)) {
				if (m_armorDurability.count > 0) {
					if (!m_armorDurability.needBoost || _boost) {
						m_armorDurability.count--;
						if (m_armorDurability.count <= 0) {
							for (int i = 0; i < m_ground.Length; ++i) {
								m_ground[i].isTrigger = true;
							}
							SetSignal(Signals.Type.Destroyed, true);
						}
						return true;
					} else {
						// Message : You need boost!
						Messenger.Broadcast(GameEvents.BREAK_OBJECT_NEED_TURBO);
					}
				} else {
					Messenger.Broadcast<DragonTier, string>(GameEvents.BIGGER_DRAGON_NEEDED, m_minTierToBreak, m_entity.sku);
				}
			}

			return false;
		}

		public override bool CanBeBitten() {
			return false;
		}
	}
}