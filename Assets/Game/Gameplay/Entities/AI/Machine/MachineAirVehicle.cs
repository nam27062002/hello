﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AI {
	public class MachineAirVehicle : MachineAir, IArmored {
		[SeparatorAttribute("Armor")]
		[SerializeField] private HitsPerDragonTier m_armorDurabilityPerTier;

		[SeparatorAttribute("Ground")]
		[SerializeField] private Collider[] m_ground;

		private DevicePassengersSpawner m_passengersSpawner;	
		private Hit m_armorDurability = new Hit();
		private DragonTier m_minTierToBreak;


		protected override void Awake() {
			m_passengersSpawner = GetComponent<DevicePassengersSpawner>();
			if (m_passengersSpawner != null) {
				m_passengersSpawner.Initialize();
			}

			m_minTierToBreak = m_armorDurabilityPerTier.GetMinTier();

			base.Awake();
		}

		public override void Spawn(ISpawner _spawner) {
			DragonPlayer player = InstanceManager.player;
			DragonTier tier = player.GetTierWhenBreaking();

			Hit originalHits = m_armorDurabilityPerTier.Get(tier);
			m_armorDurability.count = originalHits.count;
			m_armorDurability.needBoost = originalHits.needBoost;

			if (m_passengersSpawner != null) {
				m_passengersSpawner.Respawn();
				m_passengersSpawner.PassengersEnterDevice();
			}

			for (int i = 0; i < m_ground.Length; ++i) {
				m_ground[i].isTrigger = false;
			}

			base.Spawn(_spawner);
		}

		public bool ReduceDurability(bool _boost, IEntity.Type _source) {
			if (!GetSignal(Signals.Type.Burning)) {
				if (m_armorDurability.count > 0) {
					if (!m_armorDurability.needBoost || _boost) {
						m_armorDurability.count--;
						if (m_armorDurability.count <= 0) {
							if (m_passengersSpawner != null) {
								m_passengersSpawner.PassengersLeaveDevice();
							}

							for (int i = 0; i < m_ground.Length; ++i) {
								m_ground[i].isTrigger = true;
							}
							SetSignal(Signals.Type.Destroyed, true);
						}
						return true;
					} else {
						// Message : You need boost!
						Messenger.Broadcast(MessengerEvents.BREAK_OBJECT_NEED_TURBO);
					}
				} else {
					Messenger.Broadcast<DragonTier, string>(MessengerEvents.BIGGER_DRAGON_NEEDED, m_minTierToBreak, m_entity.sku);
				}
			}

			return false;
		}

		public override bool CanBeBitten() {
			return false;
		}

		public override bool Burn(Transform _transform, IEntity.Type _source, bool instant = false) {			
			if (base.Burn(_transform, _source, instant)) {				
				if (m_passengersSpawner != null) {
					m_passengersSpawner.PassengersBurn(_source);
				}
				return true;
			}

			return false;
		}
	}
}