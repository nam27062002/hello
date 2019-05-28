using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AI {
    public class MachineArmored : MachineGround, IArmored {
        [SeparatorAttribute("Armor")]
        [SerializeField] private HitsPerDragonTier m_armorDurabilityPerTier;

        private Hit m_armorDurability = new Hit();
        private DragonTier m_minTierToBreak;

        public override void Spawn(ISpawner _spawner) {
            DragonPlayer player = InstanceManager.player;
            DragonTier tier = player.GetTierWhenBreaking();
            m_minTierToBreak = m_armorDurabilityPerTier.GetMinTier();

            Hit originalHits = m_armorDurabilityPerTier.Get(tier);
            m_armorDurability.count = originalHits.count;
            m_armorDurability.needBoost = originalHits.needBoost;

            base.Spawn(_spawner);
        }

        public bool ReduceDurability(bool _boost, IEntity.Type _source) {
            if (m_armorDurability.count > 0) {
                if (!m_armorDurability.needBoost || _boost) {
                    ReceiveHit();
                    m_armorDurability.count--;
                    if (m_armorDurability.count <= 0) {
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
                // player can't destroy the armor
                Messenger.Broadcast<DragonTier, string>(MessengerEvents.BIGGER_DRAGON_NEEDED, m_minTierToBreak, m_entity.sku);
            }

            return false;
        }

        public override bool CanBeBitten() {
            return false;
        }

        public override bool Smash(IEntity.Type _source) {
            return false;
        }
    }
}