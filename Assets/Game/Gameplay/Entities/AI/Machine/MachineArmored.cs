using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AI {
	public class MachineArmored : Machine {
		[SeparatorAttribute("Armor")]
		[SerializeField] private int m_armorDurabilityMax;

		private int m_armorDurability;

		public override void Spawn(ISpawner _spawner) {
			m_armorDurability = m_armorDurabilityMax;
			base.Spawn(_spawner);
		}

		public void ReduceDurability() {
			m_armorDurability--;
			if (m_armorDurability <= 0) {
				SetSignal(Signals.Type.Destroyed, true);
			}
		}

		public override bool CanBeBitten() {
			return false;
		}
	}
}