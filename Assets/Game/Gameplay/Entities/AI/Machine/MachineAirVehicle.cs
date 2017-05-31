using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AI {
	public class MachineAirVehicle : MachineAir, IArmored {
		[SeparatorAttribute("Armor")]
		[SerializeField] private HitsPerDragonTier m_armorDurabilityPerTier;

		private DevicePassengersSpawner m_passengersSpawner;	
		private Hit m_armorDurability = new Hit();
		private DragonTier m_minTierToBreak;


		protected override void Awake() {
			m_passengersSpawner = GetComponent<DevicePassengersSpawner>();
			m_passengersSpawner.Initialize();

			m_minTierToBreak = m_armorDurabilityPerTier.GetMinTier();

			base.Awake();
		}

		public override void Spawn(ISpawner _spawner) {
			DragonPlayer player = InstanceManager.player;
			DragonTier tier = player.GetTierWhenBreaking();

			Hit originalHits = m_armorDurabilityPerTier.Get(tier);
			m_armorDurability.count = originalHits.count;
			m_armorDurability.needBoost = originalHits.needBoost;

			m_passengersSpawner.Respawn();
			m_passengersSpawner.PassengersEnterDevice();

			base.Spawn(_spawner);
		}

		public bool ReduceDurability(bool _boost) {
			if (m_armorDurability.count > 0 && !GetSignal(Signals.Type.Burning)) {
				if (m_armorDurability.count > 0) {
					if (!m_armorDurability.needBoost || _boost) {
						m_armorDurability.count--;
						if (m_armorDurability.count <= 0) {
							m_passengersSpawner.PassengersLeaveDevice();
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

		public override bool Burn(Transform _transform) {			
			if (base.Burn(_transform)) {				
				m_passengersSpawner.PassengersBurn();
				return true;
			}

			return false;
		}
	}
}