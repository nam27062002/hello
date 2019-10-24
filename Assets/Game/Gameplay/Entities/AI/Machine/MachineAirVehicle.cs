using System.Collections;
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
                        ReceiveHit();
                        m_armorDurability.count--;
						if (m_armorDurability.count <= 0) {
							if (m_passengersSpawner != null) {
								m_passengersSpawner.PassengersLeaveDevice();
							}

							for (int i = 0; i < m_ground.Length; ++i) {
								m_ground[i].isTrigger = true;
							}
							Smash(_source);
							/*
							SetSignal(Signals.Type.Destroyed, true);

							// Get the reward to be given from the entity
							Reward reward = m_entity.GetOnKillReward(false);
							InstanceManager.player.AddLife(InstanceManager.player.dragonHealthBehaviour.GetBoostedHp(reward.origin, reward.health), DamageType.NONE, m_transform);
							// Initialize some death info
							m_entity.onDieStatus.source = _source;
							// Dispatch global event
							Messenger.Broadcast<Transform, IEntity, Reward>(MessengerEvents.ENTITY_BURNED, m_transform, reward);
							*/
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

        public override void CheckInLove() {}
        public override void InLove(float _inLoveDuration) {}

        public override bool CanBeBitten() {
			return false;
		}

		public override bool Burn(Transform _transform, IEntity.Type _source, KillType _killType = KillType.BURNT, bool _instant = false, FireColorSetupManager.FireColorType _fireColorType = FireColorSetupManager.FireColorType.RED) {			
			if (base.Burn(_transform, _source, _killType, _instant, _fireColorType)) {				
				if (m_passengersSpawner != null) {
					m_passengersSpawner.PassengersBurn(_source);
				}
				return true;
			}

			return false;
		}
	}
}