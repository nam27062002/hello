using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AI {
	public class MachineArmored : MachineOld, IArmored {
		[SeparatorAttribute("Armor")]
		[SerializeField] private HitsPerDragonTier m_armorDurabilityPerTier;

		private Hit m_armorDurability = new Hit();


		public override void Spawn(ISpawner _spawner) {
			DragonPlayer player = InstanceManager.player;
			DragonTier tier = player.GetTierWhenBreaking();

			Hit originalHits = m_armorDurabilityPerTier.Get(tier);
			m_armorDurability.count = originalHits.count;
			m_armorDurability.needBoost = originalHits.needBoost;

			base.Spawn(_spawner);
		}

		public bool ReduceDurability(bool _boost) {
			if (m_armorDurability.count > 0) {
				if (!m_armorDurability.needBoost || _boost) {
					m_armorDurability.count--;
					if (m_armorDurability.count <= 0) {
						SetSignal(Signals.Type.Destroyed, true);
					}
					return true;
				}
			}

			return false;
		}

		public override bool CanBeBitten() {
			return false;
		}
	}
}