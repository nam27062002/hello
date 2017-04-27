using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AI {
	public class MachineAirVehicle : MachineAir, IArmored {
		[SeparatorAttribute("Armor")]
		[SerializeField] private HitsPerDragonTier m_armorDurabilityPerTier;

		private DevicePassengersSpawner m_operatorSpawner;	
		private Hit m_armorDurability = new Hit();


		protected override void Awake() {
			m_operatorSpawner = GetComponent<DevicePassengersSpawner>();
			m_operatorSpawner.Initialize();
			base.Awake();
		}

		public override void Spawn(ISpawner _spawner) {
			DragonPlayer player = InstanceManager.player;
			DragonTier tier = player.GetTierWhenBreaking();

			Hit originalHits = m_armorDurabilityPerTier.Get(tier);
			m_armorDurability.count = originalHits.count;
			m_armorDurability.needBoost = originalHits.needBoost;

			m_operatorSpawner.Respawn();
			m_operatorSpawner.PassengersEnterDevice();

			base.Spawn(_spawner);
		}

		public bool ReduceDurability(bool _boost) {
			if (m_armorDurability.count > 0 && !GetSignal(Signals.Type.Burning)) {
				if (!m_armorDurability.needBoost || _boost) {
					m_armorDurability.count--;
					if (m_armorDurability.count <= 0) {
						m_operatorSpawner.PassengersLeaveDevice();
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

		public override bool Burn(Transform _transform) {			
			if (base.Burn(_transform)) {				
				m_operatorSpawner.PassengersBurn();
				return true;
			}

			return false;
		}
	}
}